using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Helper;
using Openbound_Network_Object_Library.Models;

namespace Openbound_Network_Object_Library.ValidationModel
{
    public class Account : ValidationModelBase
    {
        [Required, MinLength(4), MaxLength(30), RegularExpression(@"[0-9]*[A-z ]+[0-9]*")]
        public string Nickname { get; set; }
        [Required, MinLength(3), MaxLength(30), EmailAddress]
        public string Email { get; set; }
        public Gender CharacterGender { get; set; }
        [Required, MinLength(6), MaxLength(30),
         NotOrCharacter(ErrorMessage = "The password contains invalid characters.")]
        public string Password { get; set; }
        [Required, Compare("Password"), Display(Name = "Password confirmation")]
        public string PasswordConfirmation { get; set; }

    }
}
