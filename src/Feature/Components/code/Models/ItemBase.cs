using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Mvc.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Sitecore.HabitatHome.Feature.Components.Models
{
    public class ItemBase
    {
        private Item item;
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertyCache =
            new ConcurrentDictionary<Type, PropertyInfo[]>();



        public IEnumerable<T> GetChildren<T>() where T : ItemBase, new()
        {
            var children = new List<T>();
            foreach (Item child in item.Children)
            {
                var t = new T();
                t.Item = child;
                children.Add(t);
            }
            return children;
        }



        public Item Item
        {
            get
            {
                return item;
            }
            set
            {
                item = value;
                SetProperties();
            }
        }



        protected void SetProperties()
        {
            var type = GetType();
            var props = propertyCache.GetOrAdd(type, t => t.GetProperties());

            foreach (var prop in props)
            if (prop.CanWrite && prop.Name != "Item")
            {
                string value = null;
                if (item != null)
                    value = item[prop.Name];

                if (prop.PropertyType == typeof(string))
                    prop.SetValue(this, value);
                else if (prop.PropertyType == typeof(MediaItem))
                {
                    MediaItem mi = null;
                    if (!string.IsNullOrEmpty(value))
                        mi = ((ImageField)item.Fields[prop.Name]).MediaItem;
                    prop.SetValue(this, mi);
                }
                else if (typeof(ItemBase).IsAssignableFrom(prop.PropertyType))
                {
                    ItemBase ib = null;
                    if (!string.IsNullOrEmpty(value))
                    {
                        var relatedItem = Context.Database.GetItem(value);
                        ib = Activator.CreateInstance(prop.PropertyType) as ItemBase;
                        ib.Item = relatedItem;
                    }
                    prop.SetValue(this, ib);
                }
                //else if (prop.PropertyType == typeof(Item))
                //{
                //    Item relatedItem = null;
                //    if (!string.IsNullOrEmpty(value))
                //        relatedItem = Context.Database.GetItem(value);
                //    prop.SetValue(this, item);
                //}
            }
        }
    }



    public static class SitecoreHelperExtensions
    {

        public static HtmlString Edit<TBase, TProp>(this SitecoreHelper sitecoreHelper,
            TBase itemBase, Expression<Func<TBase, TProp>> property) where TBase : ItemBase
        {
            Type type = typeof(TBase);

            var member = property.Body as MemberExpression;
            if (member == null)
                return null;

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                var func = property.Compile();
                return new HtmlString(func(itemBase as TBase).ToString());
            }

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException($"Expression '{property.ToString()}' refers to a property that is not from type {type.FullName}.");

            return sitecoreHelper.Field(propInfo.Name, itemBase.Item);
        }

    }
}