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

    public class OrderItemController : Controller
    {
        private readonly IDbConnection dbConnection;

        private const decimal _taxRate = 0.10m;

        public OrderItemController(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        public IActionResult Create(int id)
        {
            var items = dbConnection.Query<ItemViewModel>(@"
            SELECT Item_Id AS ItemId, 
            Name, 
            Type, 
            Price 
            FROM `Item`").ToList();

            var model = new OrderItemViewModel
            {
                OrderId = id,
                AllItems = items
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(OrderItemViewModel model, string nextStep)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // create order item
            var rowsAffected = dbConnection.Execute(@"
                INSERT INTO `order_item` (order_id, item_id, quantity)
                VALUES (@OrderId, @ItemId, @Quantity)
            ", new
            {
                OrderId = model.OrderId,
                ItemId = model.ItemId,
                Quantity = model.Quantity
            });

            // calculate totals
            var subtotal = dbConnection.ExecuteScalar<decimal>(@"
            SELECT 
                SUM(I.price * OI.quantity)
            FROM 
                `order_item` OI
            INNER JOIN item I ON    
                OI.item_id = I.item_id
            WHERE
                OI.order_id = @OrderId
            ", new { OrderId = model.OrderId });

            var tax = subtotal * _taxRate;

            var total = subtotal + tax;

            // update totals
            dbConnection.Execute(@"UPDATE `order`
            SET
                sub_total = @SubTotal, 
                tax_total = @TaxTotal, 
                grand_total = @GrandTotal
            WHERE
                order_id = @OrderId", new
            {
                OrderId = model.OrderId,
                SubTotal = subtotal,
                TaxTotal = tax,
                GrandTotal = total
            });

            if (nextStep == "AddAnother")
            {
                return RedirectToAction("Create", "OrderItem", new { id = model.OrderId });
            }
            else
            {
                return RedirectToAction("Confirm", "Order", new { id = model.OrderId });
            }
        }  
    }
}