using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Hikkaba.Web.Utils;

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