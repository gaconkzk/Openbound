using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avatar_API.Data.Model;
using Avatar_API.Data.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Avatar_API.Controllers
{
    public class StripeController : Controller
    {
        private readonly StripeService _stripeService;
        private readonly CashPackageService _cashPackageService;
        public StripeController(StripeService stripeService, CashPackageService cashPackageService)
        {
            _stripeService = stripeService;
            _cashPackageService = cashPackageService;
        }

        public IActionResult Index()
        {
            ViewBag.Controller = "Stripe";
            ViewBag.Currency = _stripeService.GetCurrency();
            var model = new ProductsModel
            {
                ProductList = _cashPackageService.GetPackages("Stripe")
            };


            ViewData.Model = model;
            return View("SelectProduct");
        }

        public IActionResult Purchase(int price)
        {
            if (!_cashPackageService.IsPackageValid(price)) return View("Invalid");
            ViewBag.PublishableKey = _stripeService.GetPublishableKey();
            ViewBag.Currency = _stripeService.GetCurrency();
            CashRecharge cashRecharge = new CashRecharge
            {
                Price = price,
                Description = $"Adding {_cashPackageService.GetCashValue(price)} Cash"!,
                Nonce = ""
            };

            return View(cashRecharge);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Checkout(string stripeToken, CashRecharge cashRecharge)
        {
            if (!_cashPackageService.IsPackageValid(cashRecharge.Price)) return View("Invalid");
            var chargeOptions = new ChargeCreateOptions()
            {
                Amount = (long)(Convert.ToDouble(cashRecharge.Price) * 100),
                Currency = _stripeService.GetCurrency(),
                Source = stripeToken,
                Metadata = new Dictionary<string, string>()
                {
                    {"CashAmount", _cashPackageService.GetCashValue(cashRecharge.Price).ToString() }
                }
            };

            ChargeService service = new ChargeService();
            Charge charge = service.Create(chargeOptions);

            if(charge.Status == "succeeded")
            {
                cashRecharge.DefineCashAmount(_cashPackageService);
                return View("Success");
            }

            return View("Failure");
        }

        
    }
}
