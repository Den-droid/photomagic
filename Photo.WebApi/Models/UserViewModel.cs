using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photo.WebApi.Models
{
    public class UserViewModel
    {
        public class UserLoginViewModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string RecaptchaToken { get; set; }
        }

        public class UserRegisterViewModel
        {
            public string Email { get; set; }
            public string ImageBase64 { get; set; }
            public string Password { get; set; }
        }
    }
}
