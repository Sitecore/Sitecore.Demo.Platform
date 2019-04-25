using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Sitecore.HabitatHome.Foundation.SitecoreExtensions.Models;
using Sitecore.Mvc.Helpers;

namespace Sitecore.HabitatHome.Foundation.SitecoreExtensions.Extensions
{
    public static class SitecoreHelperExtensions
    {
        public static HtmlString Edit<TBase, TProp>(this SitecoreHelper sitecoreHelper,
            TBase itemBase, Expression<Func<TBase, TProp>> property, object parameters = null) where TBase : ItemBase
        {
            var type = typeof(TBase);

            var member = property.Body as MemberExpression;
            if (member == null)
                return null;

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                var func = property.Compile();
                return new HtmlString(func(itemBase).ToString());
            }

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(
                    $"Expression '{property}' refers to a property that is not from type {type.FullName}.");

            return sitecoreHelper.Field(propInfo.Name, itemBase.Item, parameters);
        }
    }
}