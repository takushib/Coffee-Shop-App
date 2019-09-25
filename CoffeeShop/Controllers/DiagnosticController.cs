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
    public class DiagnosticController : Controller
    {
        private readonly IDbConnection dbConnection;

        public DiagnosticController(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            var list = dbConnection.Query<DiagnosticViewModel>("SELECT * FROM diagnostic").ToList();

            return View(list);
        }
    }
}
