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
    
    public class FavoritesController : Controller
    {
        private readonly IDbConnection dbConnection;

        public FavoritesController(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        public IActionResult Add(int id)
        {
            var items = dbConnection.Query<ItemViewModel>(@"
            SELECT Item_Id AS ItemId, 
            Name 
            FROM `Item`").ToList();

            var customers = dbConnection.Query<CustomerViewModel>(@"
            SELECT Customer_Id AS CustomerId,
            CONCAT(First_Name, ' ', Last_Name) AS FullName 
            FROM `Customer`").ToList();

            var model = new CustomerFavoriteViewModel() {
                AllCustomers = customers,
                AllItems = items
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Add(CustomerFavoriteViewModel model)
        {
            var customerFavoriteId = dbConnection.ExecuteScalar<int>(@"
                INSERT INTO `customer_favorite` (customer_id, item_id)
                VALUES (@CustomerId, @ItemId);

                SELECT LAST_INSERT_ID();
                ", new {
                    CustomerId = model.CustomerId,
                    ItemId = model.ItemId
                });
               
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var list = dbConnection.Query<CustomerFavoriteViewModel>(@"
            SELECT 
                F.Customer_Favorite_Id AS CustomerFavoriteId,
                F.Customer_Id AS CustomerId,
                F.Item_Id AS ItemId,
                I.Name AS ItemName,
                CONCAT(C.First_Name, ' ', C.Last_Name) AS CustomerName
            FROM 
                `Customer_Favorite` AS F 
                INNER JOIN `Item` AS I ON 
                    F.Item_Id = I.Item_Id
                INNER JOIN `Customer` C ON
                    C.Customer_Id = F.Customer_Id
            ").ToList();
            return View(list);
        }

        public IActionResult Search(string query)
        {
            var list = dbConnection.Query<CustomerFavoriteViewModel>(@"
            SELECT Customer_Favorite_Id AS CustomerFavoriteId,
            Customer_Id AS CustomerId,
            C.Item_Id AS ItemId,
            I.Name AS ItemName
            FROM `Customer_Favorite` AS C INNER JOIN `Item` AS I ON 
            c.Item_Id = I.Item_Id
            WHERE
                I.Name LIKE @Query
            ", new { 
                Query = $"%{query}%"
            }).ToList();

            return View("Index", list);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var rowsAffected = dbConnection.Execute(@"
                DELETE FROM Customer_Favorite
                WHERE
                    Customer_Favorite_Id = @CustomerFavoriteId
                ", new {
                    CustomerFavoriteId = id
                });

            return RedirectToAction("Index");
        }
    }
}

