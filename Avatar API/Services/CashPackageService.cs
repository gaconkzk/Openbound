using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avatar_API.Data.Model;
using Microsoft.Extensions.Configuration;

namespace Avatar_API.Data.Services
{
    public class CashPackageService
    {
        private readonly IConfiguration _config;
        public CashPackageService(IConfiguration config)
        {
            _config = config;
        }

        public List<Product> GetPackages()
        {
            List<Product> products = new List<Product>();
            var cashPackages = _config.GetSection("CashPackages").GetChildren().ToList();

            foreach (var item in cashPackages)
            {
                products.Add(new Product() {    Amount = Convert.ToInt32(item.Value), 
                                                Price = Convert.ToInt32(item.Key) });
            }

            return products;
        }

        public string GetCurrency()
        {
            return _config.GetValue<string>("AppCurrency:Currency").ToUpper();
        }

        public int GetCashValue(int price)
        {
            var cashPackages = _config.GetSection("CashPackages").GetChildren().ToList();

            foreach (var item in cashPackages)
            {
                if (item.Key == price.ToString())
                {
                    return Convert.ToInt32(item.Value);
                }
            }

            return 0;
        }

        public bool IsPackageValid(int price)
        {
            var cashPackages = _config.GetSection("CashPackages").GetChildren().ToList();

            foreach (var item in cashPackages)
            {
                if (item.Key == price.ToString())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
