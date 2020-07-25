using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avatar_API.Data.Services;
using Avatar_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using PagarMe;

namespace Avatar_API.Controllers.Recharge
{
    [Route("recharge/[controller]/[action]"), ApiExplorerSettings(IgnoreApi = true)]
    public class PagarmeController : Controller
    {
        private readonly CashPackageService _cashPackageService;
        private readonly PagarmeService _pagarmeService;
        public PagarmeController(CashPackageService cashPackageService, PagarmeService pagarmeService)
        {
            _cashPackageService = cashPackageService;
            _pagarmeService = pagarmeService;
        }

        public IActionResult Purchase(int price)
        {
            ViewBag.EncryptionKey = _pagarmeService.GetEncryptionKey();
            ViewBag.Price = price * 100;
            ViewBag.Cash = _cashPackageService.GetCashValue(price);
            return View();
        }

        public IActionResult Checkout()
        {
            return View();
        }
    }
}
