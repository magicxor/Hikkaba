using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Hikkaba.Web.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FormFileCollectionAttribute : ValidationAttribute, IClientModelValidator
    {
        public long? MaxSize { get; set; }
        public long? MaxTotalSize { get; set; }
        public long? MaxCount { get; set; }

        public FormFileCollectionAttribute(long? maxSize, long? maxTotalSize, long? maxCount)
            : base("Please upload a supported file.")
        {
            MaxSize = maxSize;
            MaxTotalSize = maxTotalSize;
            MaxCount = maxCount;
            if (MaxSize.HasValue)
            {
                ErrorMessage = "Each file size must be <= " + MaxSize.Value + " bytes. ";
            }
            if (MaxTotalSize.HasValue)
            {
                ErrorMessage = "Total size of all files must be <= " + MaxTotalSize.Value + " bytes. ";
            }
            if (MaxCount.HasValue)
            {
                ErrorMessage = "Total file count must be <= " + MaxCount.Value + ". ";
            }
        }

        public override bool IsValid(object value)
        {
            IFormFileCollection formFileCollection = value as IFormFileCollection;
            if (formFileCollection != null)
            {
                bool result = true;

                if (MaxSize.HasValue)
                {
                    result &= (formFileCollection.All(fl => fl.Length <= MaxSize.Value));
                }
                if (MaxTotalSize.HasValue)
                {
                    result &= (formFileCollection.Sum(fl => fl.Length) <= MaxTotalSize.Value);
                }
                if (MaxCount.HasValue)
                {
                    result &= (formFileCollection.Count <= MaxCount.Value);
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
