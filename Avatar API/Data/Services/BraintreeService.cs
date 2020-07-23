using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Braintree;
using Microsoft.Extensions.Configuration;

namespace Avatar_API.Data.Services
{
    public class BraintreeService : IBraintreeService
    {
        private readonly IConfiguration _config;
        public BraintreeService(IConfiguration config)
        {
            _config = config;
        }
        public IBraintreeGateway CreateGateway()
        {
            var newGateway = new BraintreeGateway()
            {
                Environment = Braintree.Environment.SANDBOX,
                MerchantId = _config.GetValue<string>("BraintreeGateway:MerchantId"),
                PublicKey = _config.GetValue<string>("BraintreeGateway:PublicKey"),
                PrivateKey = _config.GetValue<string>("BraintreeGateway:PrivateKey")
            };

            return newGateway;
        }

        public IBraintreeGateway GetGateway()
        {
            return CreateGateway();
        }

        public string GetCurrency()
        {
            //This currency is just for the view. The app's currency need to be defined on Braintree.
            return _config.GetValue<string>("BraintreeGateway:CurrencyPreview").ToUpper();
        }

    }
}
