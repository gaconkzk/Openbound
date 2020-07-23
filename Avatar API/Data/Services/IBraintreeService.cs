using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Braintree;

namespace Avatar_API.Data.Services
{
    public interface IBraintreeService
    {
        IBraintreeGateway CreateGateway();

        IBraintreeGateway GetGateway();
        string GetCurrency();
    }
}
