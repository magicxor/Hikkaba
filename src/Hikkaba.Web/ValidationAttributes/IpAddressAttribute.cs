using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Hikkaba.Web.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IpAddressAttribute : ValidationAttribute, IClientModelValidator
    {
        public IpAddressAttribute()
            : base("Please upload a supported file.")
        {
            ErrorMessage = "Please enter a valid IP address";
        }

        public override bool IsValid(object value)
        {
            string str = value as string;
            if (str != null)
            {
                bool result = true;

                IPAddress ip;
                if (!IPAddress.TryParse(str, out ip))
                {
                    result = false;
                }

                return result;
            }

            return true;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            throw new NotImplementedException(); // todo: implement
        }
    }
}
