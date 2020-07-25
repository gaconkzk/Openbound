using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Avatar_API.Services
{
    public class MercadoPagoService
    {
        private readonly IConfiguration _config;
        public MercadoPagoService(IConfiguration config)
        {
            _config = config;
        }

        public string GetPublicKey()
        {
            return _config.GetValue<string>("MercadoPago:PublicKey");
        }

        public string GetAccessToken()
        {
            return _config.GetValue<string>("MercadoPago:AccessToken");
        }
    }
}
