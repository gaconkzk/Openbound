using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Database.Controller;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.ValidationModel;

namespace Openbound_Login_Server.Service
{
    public class RegistrationService
    {
        public static string RegistrationAttempt(string param)
        {
            try
            {
                Account deserializedAccount = ObjectWrapper.DeserializeRequest<Account>(param);
                string registrationResult = new PlayerController().RegisterPlayer(deserializedAccount);

                return registrationResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                throw ex;
            }
        }
    }
}
