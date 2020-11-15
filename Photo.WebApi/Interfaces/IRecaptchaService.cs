using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photo.WebApi.Interfaces
{
    public interface IRecaptchaService
    {
        bool IsValid(string recaptchaToken);
    }
}
