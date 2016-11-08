using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using System.Reflection;

namespace StrokeLocal.WebHelper
{
    public class CollectionConversionManager
    {
        /// <summary>
        /// Convert NameValueCollection to a String like this
        /// key=value&key1=value1....
        /// </summary>
        /// <param name="nv"></param>
        /// <param name="exclude"></param>
        /// <returns></returns>
        public static String NameValueCollection2String(NameValueCollection nv, String[] exclude)
        {
            if (nv.Count == 0)
                return null;

            StringBuilder sb = new StringBuilder();
            bool skip;

            for (int i = 0; i < nv.Count; i++)
            {
                skip = false;
                if (i > 0)
                    sb.Append("&");

                if (exclude != null && exclude.Contains(nv.GetKey(i).Trim()))
                    skip = true;

                if (!skip)
                    sb.Append(nv.GetKey(i)).Append("=").Append(nv.GetValues(i)[0]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// similar with Collection2String ,but key is sorted by first alphabet
        /// </summary>
        /// <param name="nv"></param>
        /// <returns>String</returns>
        public static String NameValueCollection2StringSorted(NameValueCollection nv, String[] exclude)
        {
            if (nv == null)
                return null;

            SortedList s = new SortedList();
            int i;

            for (i = 0; i < nv.Count; i++)
            {
                if (exclude != null && exclude.Contains(nv.GetKey(i).Trim()))
                    continue;
                s.Add(nv.GetKey(i).Trim(), nv.GetValues(i)[0].Trim());
            }

            StringBuilder sb = new StringBuilder();
            i = 0;

            foreach (DictionaryEntry d in s)
            {
                //here, I will remove blank from head and tail of string
                if (i > 0)
                    sb.Append("&");
                sb.Append(d.Key.ToString()).Append("=").Append(d.Value.ToString());
                i++;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Convert NameValueCollection to xml String
        /// 
        /// </summary>
        /// <param name="nv">NameValueCollection</param>
        /// <param name="tag_name">tag name</param>
        /// <returns></returns>
        public static String NameValueCollection2Xml(NameValueCollection nv, String tag_name)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(XmlHelper.TagBegin(tag_name));

            //get all keys and insert into List
            List<String> keys = new List<string>(nv.AllKeys);

            while (keys.Count > 0)
            {
                // get first key from keys , if this key don't contain '.' then 
                // output it as a simple element, and removed it after handle
                String key = (String)keys[0];

                if (!key.Contains('.'))
                {
                    // simple property, output directly
                    sb.Append(XmlHelper.AddElement(key, nv[key]));
                    keys.RemoveAt(0);
                }
                else
                {
                    // key like this : xxxx.yyy.zzz
                    // and xxxx is a current property's name, yyy is perproty of xxx, etc...

                    // So, i choose all keys whick contain prefix of this key, certainly, these
                    // keys mush be property of some class or entity.

                    // in order to simple handle this process, i create another collection
                    // to save these keys, so I can handle by recursion invoke

                    NameValueCollection nv2 = new NameValueCollection();

                    int index = key.IndexOf('.');
                    String prefix = key.Substring(0, index);

                    // define a local delegate to filter these keys
                    Predicate<String> FilterKeys = delegate(String in_key)
                    {
                        // index == 0 
                        return in_key.IndexOf(prefix) == 0 ? true : false;
                    };

                    List<String> removed = keys.FindAll(FilterKeys);

                    /* after .Net framework3.5, the format of above can write like below also:
                    List<String> removed2 = keys.FindAll( 
                        (String in_key )=>
                        {
                            return in_key.IndexOf(prefix) == 0 ? true : false;
                        } );
                    */

                    // insert all keys and value into new collection and remove from keys
                    foreach (String r in removed)
                    {
                        String newkey = r.Substring(index + 1);
                        nv2.Add(newkey, nv[r]);
                    }

                    //remove keys from original collection
                    keys.RemoveAll(FilterKeys);

                    // recursion invoke !
                    sb.Append(NameValueCollection2Xml(nv2, prefix));
                }
            }

            sb.Append(XmlHelper.TagEnd(tag_name));
            return sb.ToString();
        }

        /// <summary>
        /// Convert NameValueCollection to a Entity object
        /// </summary>
        /// <param name="nv"></param>
        /// <param name="obj_type"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static object NameValueCollection2Entity(NameValueCollection nv, Type obj_type, String prefix)
        {
            if (nv == null || nv.Count == 0)
                return null;

            // create a intance for this object and get all property
            object instance = Activator.CreateInstance(obj_type);
            PropertyInfo[] pi = instance.GetType().GetProperties();

            // in first invoke, prefix is null
            String name = (prefix == null) ? "" : (prefix + ".");

            foreach (PropertyInfo p in pi)
            {
                try
                {
                    if (StringConverter.CanConvertDirectly(p.PropertyType))
                    {
                        //must check exist this key , otherwise exception will be thrown
                        if (nv.AllKeys.Contains(name + p.Name))
                        {
                            p.SetValue(instance,
                                        StringConverterManager.ConvertTo(nv[name + p.Name], p.PropertyType),
                                        null);
                        }
                    }
                    else if (p.PropertyType.IsArray)
                    {
                        //arrary
                    }
                    else if (p.PropertyType.IsClass)
                    {
                        //class
                        p.SetValue(instance,
                                    NameValueCollection2Entity(nv, p.PropertyType, name + p.Name),
                                    null);
                    }
                }
                catch (Exception) { }
            }
            return instance;
        }
    }
}
