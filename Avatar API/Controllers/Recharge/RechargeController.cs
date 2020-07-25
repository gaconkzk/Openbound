using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avatar_API.Data.Model;
using Avatar_API.Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RechargeController : Controller
    {
        private readonly CashPackageService _cashPackageService;

        public RechargeController(CashPackageService cashPackageService)
        {
            _cashPackageService = cashPackageService;
        }

        public IActionResult Index()
        {
            ViewBag.Currency = _cashPackageService.GetCurrency();
            var model = new ProductsModel
            {
                ProductList = _cashPackageService.GetPackages()
            };

            ViewData.Model = model;
            return View();
        }

        public IActionResult Gateway(int price)
        {
            ViewBag.Price = price;
            return View();
        }
    }
}
