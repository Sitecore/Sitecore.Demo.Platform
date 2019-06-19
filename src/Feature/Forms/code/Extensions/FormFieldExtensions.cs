using Sitecore.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Sitecore.ExperienceForms.Models;
using System;

namespace Sitecore.HabitatHome.Feature.Forms.SubmitActions
{
    internal static class FormFieldExtensions
    {
        internal static string GetValue(this IViewModel fieldModel)
        {
            if (fieldModel == null)
            {
                throw new ArgumentNullException(nameof(fieldModel));
            }

            var property = fieldModel.GetType().GetProperty("Value");
            var postedValue = property.GetValue(fieldModel);
            return postedValue != null ? ParseFieldValue(postedValue) : string.Empty;
        }

        private static string ParseFieldValue(object postedValue)
        {
            Assert.ArgumentNotNull(postedValue, nameof(postedValue));
            List<string> stringList = new List<string>();
            IList list = postedValue as IList;
            if (list != null)
            {
                foreach (object obj in list)
                {
                    stringList.Add(obj.ToString());
                }
            }
            else
            {
                stringList.Add(postedValue.ToString());
            }

            return string.Join(",", stringList);
        }

    }
}