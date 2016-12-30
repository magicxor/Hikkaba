using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Internal;

namespace Hikkaba.Web.Utils
{
    public static class ModelStateDictionaryExtensions
    {
        public static string ModelErrorsToString(this ModelStateDictionary modelStateDictionary)
        {
            var modelErrorsCollection = modelStateDictionary
                .SelectMany(modelStateEntry => modelStateEntry
                                .Value
                                .Errors
                                .Select(modelError => 
                                    modelStateEntry.Key + ": " + modelError.ErrorMessage));
            var modelErrorsString = string.Join(Environment.NewLine, modelErrorsCollection).Trim();
            return modelErrorsString;
        }
    }
}
