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
    public class StoreController : Controller
    {
        private readonly IDbConnection dbConnection;

        public StoreController(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }
        public IActionResult Index()
        {
            var list = dbConnection.Query<StoreViewModel>(@"SELECT Store_Id AS 
            StoreId, 
            Name, 
            Street, 
            City, 
            State, 
            Zip, 
            Phone AS PhoneNumber 
            FROM `Store`").ToList();

            return View(list);
        }

        public IActionResult Search(string query)
        {
            var list = dbConnection.Query<StoreViewModel>(@"SELECT Store_Id AS StoreId,
            Name, 
            Street, 
            City, 
            State, 
            Zip, 
            Phone AS PhoneNumber 
            FROM `Store`
            WHERE
                Name LIKE @Query
                OR Street LIKE @Query
                OR City LIKE @Query
                OR State LIKE @Query
                OR Zip = @Query
                OR Phone LIKE @Query
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
        public IActionResult Add(StoreViewModel model)
        {
            var rowsAffected = dbConnection.Execute(@"
            INSERT INTO store(Name, Street, City, State, Zip, Phone)
                VALUES (@Name, @Street, @City, @State, @Zip, @Phone)
                ", new {
                    Name = model.Name,
                    Street = model.Street,
                    City = model.City,
                    State = model.State,
                    Zip = model.Zip,
                    Phone = model.PhoneNumber,
                });

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var store = dbConnection.Query<StoreViewModel>(@"SELECT
            Store_Id AS StoreId, 
            Name, 
            Street, 
            City, 
            State, 
            Zip, 
            Phone AS PhoneNumber 
            FROM `Store`
            WHERE
                Store_Id = @StoreId
            ", new { StoreId = id }).Single();

            return View(store);
        }

        [HttpPost]
        public IActionResult Edit(int id, StoreViewModel model)
        {
            var rowsAffected = dbConnection.Execute(@"
                UPDATE store
                SET 
                    name = @Name, 
                    Street = @Street, 
                    City = @City, 
                    State = @State, 
                    Zip = @Zip,
                    phone = @Phone
                WHERE
                    Store_ID = @StoreId
                ", new {
                    StoreId = id,
                    Name = model.Name,
                    Street = model.Street,
                    City = model.City,
                    State = model.State,
                    Zip = model.Zip,
                    Phone = model.PhoneNumber,
                });

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var rowsAffected = dbConnection.Execute(@"
                DELETE FROM store
                WHERE
                    Store_ID = @StoreId
                ", new {
                    StoreId = id
                });

            return RedirectToAction("Index");
        }
    }
}

