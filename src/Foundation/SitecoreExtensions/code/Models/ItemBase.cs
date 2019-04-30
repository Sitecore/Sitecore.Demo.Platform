using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Sitecore.HabitatHome.Foundation.SitecoreExtensions.Models
{
    public class ItemBase
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertyCache =
            new ConcurrentDictionary<Type, PropertyInfo[]>();

        private Item _item;

        public Item Item
        {
            get => _item;
            set
            {
                _item = value;
                SetProperties();
            }
        }

        public IEnumerable<T> GetChildren<T>() where T : ItemBase, new()
        {
            var children = new List<T>();

            if (_item != null)
                foreach (Item child in _item.Children)
                {
                    var t = new T();
                    t.Item = child;
                    children.Add(t);
                }

            return children;
        }

        protected void SetProperties()
        {
            var type = GetType();
            var props = propertyCache.GetOrAdd(type, t => t.GetProperties());

            foreach (var prop in props)
                if (prop.CanWrite && prop.Name != "Item")
                {
                    string value = null;
                    if (_item != null) value = _item[prop.Name];

                    if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(this, value);
                    }
                    else if (prop.PropertyType == typeof(MediaItem))
                    {
                        MediaItem mi = null;
                        if (!string.IsNullOrEmpty(value)) mi = ((ImageField) _item.Fields[prop.Name]).MediaItem;
                        prop.SetValue(this, mi);
                    }
                    else if (prop.PropertyType == typeof(DateField))
                    {
                        var fieldName = prop.Name.Contains("Field")
                            ? prop.Name.Replace("Field", string.Empty)
                            : prop.Name;
                        prop.SetValue(this, (DateField) _item.Fields[fieldName]);
                    }
                    else if (prop.PropertyType == typeof(ImageField))
                    {
                        var fieldName = prop.Name.Contains("Field")
                            ? prop.Name.Replace("Field", string.Empty)
                            : prop.Name;
                        prop.SetValue(this, (ImageField) _item.Fields[fieldName]);
                    }
                    else if (prop.PropertyType == typeof(LinkField))
                    {
                        var fieldName = prop.Name.Contains("Field")
                            ? prop.Name.Replace("Field", string.Empty)
                            : prop.Name;
                        prop.SetValue(this, (LinkField) _item.Fields[fieldName]);
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