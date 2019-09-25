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
    public class AccountController : Controller
    {
        private readonly IDbConnection dbConnection;

        public AccountController(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }
        public IActionResult Index()
        {
            var list = dbConnection.Query<CustomerViewModel>(@"SELECT Customer_Id AS CustomerId,
            Email_Address AS EmailAddress,
            First_Name AS FirstName, 
            Last_Name AS LastName, 
            Phone AS PhoneNumber,
            Reward_Balance AS RewardBalance
            FROM `Customer`").ToList();
            return View(list);
        }

        public IActionResult Search(string query)
        {
            var list = dbConnection.Query<CustomerViewModel>(@"SELECT Customer_Id AS CustomerId,
            Email_Address AS EmailAddress,
            First_Name AS FirstName, 
            Last_Name AS LastName, 
            Phone AS PhoneNumber,
            Reward_Balance AS RewardBalance
            FROM `Customer`
            WHERE
                Email_Address LIKE @Query
                OR First_Name LIKE @Query
                OR Last_Name LIKE @Query
                OR Phone LIKE @Query
            ", new { 
                Query = $"%{query}%"
            }).ToList();

            return View("Index", list);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CustomerViewModel model)
        {
            var rowsAffected = dbConnection.Execute(@"
                INSERT INTO customer (email_address, first_name, last_name, phone, reward_balance)
                VALUES (@EmailAddress, @FirstName, @LastName, @Phone, 0)
                ", new {
                    EmailAddress = model.EmailAddress,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.PhoneNumber,
                });

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var customer = dbConnection.Query<CustomerViewModel>(@"
            SELECT Customer_Id AS CustomerId,
            Email_Address AS EmailAddress,
            First_Name AS FirstName, 
            Last_Name AS LastName, 
            Phone AS PhoneNumber,
            Reward_Balance AS RewardBalance
            FROM `Customer`
            WHERE
                Customer_Id = @CustomerId
            ", new { CustomerId = id }).Single();

            return View(customer);
        }

        [HttpPost]
        public IActionResult Edit(int id, CustomerViewModel model)
        {
            var rowsAffected = dbConnection.Execute(@"
                UPDATE customer
                SET 
                    email_address = @EmailAddress, 
                    first_name = @FirstName, 
                    last_name = @LastName, 
                    phone = @Phone
                WHERE
                    Customer_ID = @CustomerId
                ", new {
                    CustomerId = id,
                    EmailAddress = model.EmailAddress,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.PhoneNumber,
                });

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var rowsAffected = dbConnection.Execute(@"
                DELETE FROM customer
                WHERE
                    Customer_ID = @CustomerId
                ", new {
                    CustomerId = id
                });

            return RedirectToAction("Index");
        }
    }
}

