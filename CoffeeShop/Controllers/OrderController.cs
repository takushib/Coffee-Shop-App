using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;

namespace CoffeeShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly IDbConnection dbConnection;

        public OrderController(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }
        public IActionResult Index()
        {
            var list = dbConnection.Query<OrderViewModel>(@"
            SELECT 
                O.Order_Id AS OrderId, 
                O.Customer_Id AS CustomerId,
                O.Store_Id AS StoreId,
                Date, 
                Completed, 
                Picked_Up AS PickedUp,
                Reward_Used AS RewardUsed,
                Reward_Earned AS RewardEarned,
                Sub_Total AS SubTotal,
                Tax_Total AS TaxTotal,
                Grand_Total AS GrandTotal,
                CONCAT(C.First_Name, ' ', C.Last_Name) AS CustomerName,
                S.Name AS StoreName
            FROM `Order` AS O
            INNER JOIN `Store` AS S ON
            O.Store_Id = S.Store_Id
            INNER JOIN `Customer` AS C ON
            O.Customer_Id = C.Customer_Id
            ORDER BY 
                O.Date DESC
            ").ToList();

            return View(list);
        }

        public IActionResult Search(string query)
        {
            var list = dbConnection.Query<OrderViewModel>(@"
            SELECT O.Order_Id AS OrderId, 
                O.Customer_Id AS CustomerId,
                O.Store_Id AS StoreId,
                Date, 
                Completed, 
                Picked_Up AS PickedUp,
                Reward_Used AS RewardUsed,
                Reward_Earned AS RewardEarned,
                Sub_Total AS SubTotal,
                Tax_Total AS TaxTotal,
                Grand_Total AS GrandTotal,
                CONCAT(C.First_Name, ' ', C.Last_Name) AS CustomerName,
                S.Name AS StoreName
            FROM `Order` AS O
            INNER JOIN `Store` AS S ON
            O.Store_Id = S.Store_Id
            INNER JOIN `Customer` AS C ON
            O.Customer_Id = C.Customer_Id
            WHERE
                CONCAT(C.First_Name, ' ', C.Last_Name) LIKE @Query
                OR S.Name LIKE @Query
            ORDER BY 
                O.Date DESC
            ", new
            {
                Query = $"%{query}%"
            }).ToList();

            return View("Index", list);
        }

        public IActionResult Create()
        {
            var stores = dbConnection.Query<StoreViewModel>(@"
            SELECT Store_Id AS StoreId, 
            Name
            FROM `Store`").ToList();

            var customers = dbConnection.Query<CustomerViewModel>(@"
            SELECT Customer_Id AS CustomerId,
            CONCAT(First_Name, ' ', Last_Name) AS FullName 
            FROM `Customer`").ToList();

            var model = new OrderViewModel()
            {
                AllCustomers = customers,
                AllStores = stores
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(OrderViewModel model)
        {
            var orderId = dbConnection.ExecuteScalar<int>(@"
                INSERT INTO `order` (customer_id, store_id, date)
                VALUES (@CustomerId, @StoreId, @Date);

                SELECT LAST_INSERT_ID();
                ", new
            {
                CustomerId = model.CustomerId,
                StoreId = model.StoreId,
                Date = DateTime.Now
            });

            return RedirectToAction("Create", "OrderItem", new { id = orderId });
        }


        public IActionResult Confirm(int id)
        {
            var model = GetOrderViewModel(id);

            return View(model);
        }

        [HttpPost]
        public IActionResult Confirm(OrderViewModel model)
        {
            var newModel = GetOrderViewModel(model.OrderId);

            // can't earn points on orders paid with points
            if (model.PayWithPoints)
            {
                // update order rewards used
                dbConnection.Execute(@"
                    UPDATE `order` 
                    SET
                        reward_used = @PointsUsed
                    WHERE
                        order_id = @OrderId
                ", new
                {
                    OrderId = newModel.OrderId,
                    PointsUsed = model.GrandTotalAsPoints
                });

                // update customer reward balance
                dbConnection.Execute(@"
                    UPDATE `customer`
                    SET reward_balance = reward_balance - @PointsUsed
                    WHERE customer_id = @CustomerId
                ", new
                {
                    CustomerId = newModel.CustomerId,
                    PointsUsed = model.GrandTotalAsPoints
                });
            }
            else
            {
                var pointsEarned = CalculatePointsEarned(newModel.GrandTotal);

                // update order rewards earned
                dbConnection.Execute(@"
                    UPDATE `order` 
                    SET
                        reward_earned = @PointsEarned
                    WHERE
                        order_id = @OrderId
                ", new
                {
                    OrderId = newModel.OrderId,
                    PointsEarned = pointsEarned
                });

                // update customer reward balance
                dbConnection.Execute(@"
                    UPDATE `customer`
                    SET reward_balance = reward_balance + @PointsEarned
                    WHERE customer_id = @CustomerId
                ", new
                {
                    CustomerId = newModel.CustomerId,
                    PointsEarned = pointsEarned
                });
            }

            return RedirectToAction("Complete", routeValues: new { id = model.OrderId });
        }

        public IActionResult Complete(int id)
        {
            var model = GetOrderViewModel(id);

            return View(model);
        }

        public IActionResult Details(int id)
        {
            var model = GetOrderViewModel(id);

            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var order = GetOrderViewModel(id);

            return View(order);
        }

        [HttpPost]
        public IActionResult MarkComplete(int id)
        {
            dbConnection.Execute(@"UPDATE `order`
            SET completed = 1
            WHERE order_id = @OrderId",
            new { OrderId = id });

            return RedirectToAction("Details", routeValues: new { id = id });
        }

        [HttpPost]
        public IActionResult MarkPickUp(int id)
        {
            dbConnection.Execute(@"UPDATE `order`
            SET picked_up = 1
            WHERE order_id = @OrderId",
            new { OrderId = id });

            return RedirectToAction("Details", routeValues: new { id = id });
        }

        private int CalculatePointsEarned(decimal orderTotal)
        {
            // 10 points per dollar (at least one point per order)
            return (int)Math.Ceiling(orderTotal / 10);
        }

        private OrderViewModel GetOrderViewModel(int id)
        {
            var model = dbConnection.Query<OrderViewModel>(@"
             SELECT 
                O.Order_Id AS OrderId, 
                O.Customer_Id AS CustomerId,
                O.Store_Id AS StoreId,
                Date, 
                Completed, 
                Picked_Up AS PickedUp,
                Reward_Used AS RewardUsed,
                Reward_Earned AS RewardEarned,
                Sub_Total AS SubTotal,
                Tax_Total AS TaxTotal,
                Grand_Total AS GrandTotal,
                CONCAT(C.First_Name, ' ', C.Last_Name) AS CustomerName,
                S.Name AS StoreName,
                C.Reward_Balance as CustomerRewardBalance
            FROM `Order` AS O
            INNER JOIN `Store` AS S ON
            O.Store_Id = S.Store_Id
            INNER JOIN `Customer` AS C ON
            O.Customer_Id = C.Customer_Id
            WHERE
                O.Order_Id = @OrderId
            ", new { OrderId = id }).Single();

            model.OrderItems = GetOrderItemsByOrderId(id);

            return model;
        }

        private ICollection<OrderItemViewModel> GetOrderItemsByOrderId(int id)
        {
            var model = dbConnection.Query<OrderItemViewModel>(@"
             SELECT 
                O.Order_Id AS OrderId, 
                O.Customer_Id AS CustomerId,
                O.Store_Id AS StoreId,
                Date, 
                Completed, 
                Picked_Up AS PickedUp,
                Reward_Used AS RewardUsed,
                Reward_Earned AS RewardEarned,
                Sub_Total AS SubTotal,
                Tax_Total AS TaxTotal,
                Grand_Total AS GrandTotal,
                CONCAT(C.First_Name, ' ', C.Last_Name) AS CustomerName,
                S.Name AS StoreName,
                OI.Quantity AS Quantity,
                I.Name AS ItemName,
                I.Type AS ItemType,
                I.Price as ItemPrice
            FROM `Order` AS O
            INNER JOIN `Store` AS S ON
            O.Store_Id = S.Store_Id
            INNER JOIN `Customer` AS C ON
            O.Customer_Id = C.Customer_Id
            INNER JOIN `Order_Item` AS OI ON
            O.Order_Id = OI.Order_Id
            INNER JOIN `Item` AS I ON
            OI.Item_Id = I.Item_Id
            WHERE
                O.Order_Id = @OrderId
            ", new { OrderId = id });

            return model.ToList();
        }

    }
}