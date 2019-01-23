using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Mvc.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace Sitecore.HabitatHome.Feature.Components.Models
{
    public class ItemBase
    {
        private Item _item;
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

        public IEnumerable<T> GetChildren<T>() where T : ItemBase, new()
        {
            var children = new List<T>();

            if (_item != null)
            {
                foreach (Item child in _item.Children)
                {
                    var t = new T();
                    t.Item = child;
                    children.Add(t);
                }
            }
            return children;
        }



        public Item Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                SetProperties();
            }
        }



        protected void SetProperties()
        {
            var type = GetType();
            var props = propertyCache.GetOrAdd(type, t => t.GetProperties());

            foreach (var prop in props)
            {
                if (prop.CanWrite && prop.Name != "Item")
                {
                    string value = null;
                    if (_item != null)
                    {
                        value = _item[prop.Name];
                    }

                    if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(this, value);
                    }
                    else if (prop.PropertyType == typeof(MediaItem))
                    {
                        MediaItem mi = null;
                        if (!string.IsNullOrEmpty(value))
                        {
                            mi = ((ImageField)_item.Fields[prop.Name]).MediaItem;
                        }
                        prop.SetValue(this, mi);
                    }
                    else if(prop.PropertyType == typeof(ImageField))
                    {
                        string fieldName = prop.Name.Contains("Field") ? prop.Name.Replace("Field", string.Empty) : prop.Name;
                        prop.SetValue(this, (ImageField)_item.Fields[fieldName]);
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
                }
            }
        }
    }


    //todo: move to Foundation.SitecoreExtensions
    public static class SitecoreHelperExtensions
    {

        public static HtmlString Edit<TBase, TProp>(this SitecoreHelper sitecoreHelper, 
            TBase itemBase, Expression<Func<TBase, TProp>> property, object parameters = null) where TBase : ItemBase
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

            return sitecoreHelper.Field(propInfo.Name, itemBase.Item, parameters);
        }

    }
}