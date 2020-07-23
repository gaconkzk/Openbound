using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avatar_API.Data.Services;

namespace Avatar_API.Data.Model
{
    public class CashRecharge
    {
        public int Price { get; set; }
        public string Description { get; set; }

        public int Cash { get; private set; }
        public string Nonce { get; set; }

        public void DefineCashAmount(CashPackageService cashPackageService)
        {
            Cash = cashPackageService.GetCashValue(Price);
        }

    }
}
