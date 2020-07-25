using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Avatar_API.Services
{
    public class PagarmeService
    {
        private readonly IConfiguration _config;
        public PagarmeService(IConfiguration config)
        {
            _config = config;
        }

        public string GetApiKey()
        {
            return _config.GetValue<string>("Pagarme:ApiKey");
        }

        public string GetEncryptionKey()
        {
            return _config.GetValue<string>("Pagarme:EncryptionKey");
        }

    }
}
