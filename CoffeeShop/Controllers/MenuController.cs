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
    public class MenuController : Controller
    {
        private readonly IDbConnection dbConnection;

        public MenuController(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }
        public IActionResult Index()
        {
            var list = dbConnection.Query<ItemViewModel>(@"SELECT Item_Id AS 
            ItemId, 
            Name, 
            Type, 
            Price 
            FROM `Item`").ToList();

            return View(list);
        }

        public IActionResult Search(string query)
        {
            var list = dbConnection.Query<ItemViewModel>(@"SELECT Item_Id AS ItemId,
            Name, 
            Type,
            Price 
            FROM `Item`
            WHERE
                Name LIKE @Query
                OR Type LIKE @Query
            ", new { 
                Query = $"%{query}%"
            }).ToList();

            return View("Index", list);
        }
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(ItemViewModel model)
        {
            var rowsAffected = dbConnection.Execute(@"
                INSERT INTO item (name, type, price)
                VALUES (@Name, @Type, @Price)
                ", new {
                    Name = model.Name,
                    Type = model.Type,
                    Price = model.Price,
                });

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var item = dbConnection.Query<ItemViewModel>(@"
            SELECT Item_Id AS ItemId,
            Name,
            Type,
            Price
            FROM `Item`
            WHERE
                Item_Id = @ItemId
            ", new { ItemId = id }).Single();

            return View(item);
        }

        [HttpPost]
        public IActionResult Edit(int id, ItemViewModel model)
        {
            var rowsAffected = dbConnection.Execute(@"
                UPDATE Item
                SET 
                    name = @Name, 
                    type = @Type, 
                    price = @Price 
                WHERE
                    Item_ID = @ItemId
                ", new {
                    ItemId = id,
                    Name = model.Name,
                    Type = model.Type,
                    Price = model.Price,
                });

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var rowsAffected = dbConnection.Execute(@"
                DELETE FROM item
                WHERE
                    Item_ID = @ItemId
                ", new {
                    ItemId = id
                });

            return RedirectToAction("Index");
        }
    }
}

