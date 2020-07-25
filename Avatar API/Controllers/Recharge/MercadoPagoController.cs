using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MercadoPago;
using MercadoPago.Resources;
using MercadoPago.DataStructures.Payment;
using MercadoPago.Common;
using Avatar_API.Data.Services;
using Avatar_API.Services;

namespace Avatar_API.Controllers.Recharge
{
    [Route("recharge/[controller]/[action]")]
    [ApiController]
    public class MercadoPagoController : Controller
    {
        private readonly CashPackageService _cashPackageService;
        private readonly MercadoPagoService _mercadoPagoService;
        public MercadoPagoController(CashPackageService cashPackageService, MercadoPagoService mercadoPagoService)
        {
            _cashPackageService = cashPackageService;
            _mercadoPagoService = mercadoPagoService;
        }
        public IActionResult Purchase(int price)
        {
            ViewBag.PublicKey = _mercadoPagoService.GetPublicKey();
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(string x)
        {
            MercadoPago.SDK sdk = new MercadoPago.SDK();
            sdk.AccessToken = _mercadoPagoService.GetAccessToken();

            Payment payment = new Payment(sdk)
            {
                TransactionAmount = (float)100.0,
                Token = "YOUR_CARD_TOKEN",
                Description = "Ergonomic Silk Shirt",
                PaymentMethodId = "visa",
                Installments = 1,
                Payer = new Payer
                {
                    Email = "larue.nienow@hotmail.com"
                }
            };

            payment.Save();

            Console.Out.WriteLine(payment.Status);

            return View();
        }
    }
}
