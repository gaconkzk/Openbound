using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avatar_API.Data.Model;
using Avatar_API.Data.Services;
using Braintree;
using Microsoft.AspNetCore.Mvc;

namespace Avatar_API.Controllers
{
    [Route("recharge/[controller]/[action]"), ApiExplorerSettings(IgnoreApi = true)]
    public class BraintreeController : Controller
    {
        private readonly IBraintreeService _braintreeService;
        private readonly CashPackageService _cashPackageService;

        public BraintreeController(IBraintreeService braintreeService, CashPackageService cashPackageService)
        {
            _braintreeService = braintreeService;
            _cashPackageService = cashPackageService;
        }

        public IActionResult Purchase(int price)
        {
            if (!_cashPackageService.IsPackageValid(price)) return View("Invalid");
            var gateway = _braintreeService.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;
            ViewBag.Currency = _cashPackageService.GetCurrency();

            CashRecharge cashRecharge = new CashRecharge
            {
                Price = price,
                Description = $"Adding {_cashPackageService.GetCashValue(price)} Cash"!,
                Nonce = ""
            };

            return View(cashRecharge);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Checkout(CashRecharge cashRecharge)
        {
            if (!_cashPackageService.IsPackageValid(cashRecharge.Price)) return View("Invalid");
            IBraintreeGateway gateway = _braintreeService.GetGateway();

            TransactionRequest request = new TransactionRequest
            {
                Amount = Convert.ToDecimal(cashRecharge.Price),
                PaymentMethodNonce = cashRecharge.Nonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            Result<Transaction> result = gateway.Transaction.Sale(request);

            if (result.IsSuccess())
            {
                cashRecharge.DefineCashAmount(_cashPackageService);
                //POST Endpoint
                //Add cash to player ~ somehow
                return View("Success");
            } else
            {
                return View("Failure");
            }
        }

    }
}
