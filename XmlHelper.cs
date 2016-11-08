using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Serialization;
using System.IO;

namespace Common.WebHelper
{
    /// <summary>
    /// provide Xml helper class
    /// </summary>
    public class XmlHelper
    {
        /// <summary>
        /// add xml begin tag
        /// </summary>
        /// <param name="tag_name"></param>
        /// <returns></returns>
        public static String TagBegin(String tag_name)
        {
            return "<" + tag_name + ">";
        }

        /// <summary>
        /// add xml closed tag
        /// </summary>
        /// <param name="tag_name"></param>
        /// <returns></returns>
        public static String TagEnd(String tag_name)
        {
            return "</" + tag_name + ">";
        }

        /// <summary>
        /// add xml element
        /// </summary>
        /// <param name="ele_name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String AddElement(String ele_name, object value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(TagBegin(ele_name)).Append((value == null) ? "" : value.ToString()).Append(TagEnd(ele_name));
            return sb.ToString();
        }

        /// <summary>
        /// Get Sub node in current XmlNode
        /// </summary>
        /// <param name="node"></param>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public static XmlNode GetSubNode(XmlNode node, String xpath)
        {
            return string.IsNullOrEmpty(xpath) ? node : node.SelectSingleNode(xpath);
        }

        /// <summary>
        /// Deserialize xml String( Convert to a entity class instance)
        /// </summary>
        /// <param name="obj_type"></param>
        /// <param name="xml"></param>
        /// <param name="node_name"></param>
        /// <returns></returns>
        public static object Deserialize(Type obj_type, String xml, String xpath)
        {
            if (xml == null)
                return null;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                return Deserialize(obj_type, GetSubNode(doc.DocumentElement, xpath));
            }
            catch (Exception) { return null; }
        }

        public static object Deserialize(Type obj_type, XmlNode node)
        {
            if (!obj_type.IsArray)
                return DeserializeToSingle(obj_type, node);
            else
                return DeserializeToArray(obj_type, node);
        }

        /// <summary>
        /// Create Array object
        /// </summary>
        /// <param name="obj_type"></param>
        /// <param name="root_node"></param>
        /// <returns></returns>
        internal static object DeserializeToArray(Type obj_type, XmlNode node)
        {
            if (!obj_type.IsArray || node == null)
                return null;

            //get element type of Array
            Type elementType = obj_type.GetElementType();

            // I build a new ArrayList to save each array element
            ArrayList list = new ArrayList();

            foreach (XmlNode n in node)
            {
                // skip text, blank,etc...
                // handle each element
                if (n.NodeType == XmlNodeType.Element && n.ChildNodes.Count > 0)
                {
                    PropertyInfo[] pinfos = elementType.GetProperties();
                    object instance = DeserializeToSingle(elementType, n);

                    // insert into list
                    list.Add(instance);
                }
            }

            object[] obj;
            obj = (object[])Activator.CreateInstance(obj_type, new object[] { list.Count });

            // construct array object
            list.CopyTo(obj, 0);
            return obj;
        }

        /// <summary>
        /// Create single object
        /// </summary>
        /// <param name="obj_type"></param>
        /// <param name="root_node"></param>
        /// <returns></returns>
        internal static object DeserializeToSingle(Type obj_type, XmlNode node)
        {
            if (node == null)
                return null;

            // create a object instance
            object instance = Activator.CreateInstance(obj_type);

            PropertyInfo[] pinfos = obj_type.GetProperties();

            //only root node has tagName(nodeName),others should be null,
            //for i have set node with current node.

            foreach (PropertyInfo p in pinfos)
            {
                try
                {
                    XmlNode n = node.SelectSingleNode(p.Name);

                    if (p.PropertyType.IsArray)	// array object
                        p.SetValue(instance, DeserializeToArray(p.PropertyType, n), null);
                    else if (StringConverter.CanConvertDirectly(p.PropertyType))
                        p.SetValue(instance, StringConverterManager.ConvertTo(n.InnerText, p.PropertyType), null);
                    else
                        p.SetValue(instance, DeserializeToSingle(p.PropertyType, n), null);
                }
                catch (Exception) { }
            }
            return instance;
        }

        /// <summary>
        /// Convert a xml String to NameValueCollection
        /// Example <key><key1>value1</key1><key2>value2</key2></key>
        /// 	will be converted to { key1/value1;key2/value2 }
        /// </summary>
        /// <param name="xml">xml String</param>
        /// <param name="node_name"></param>
        /// <returns></returns>
        public static NameValueCollection ToNameValueCollection(String xml, String xpath)
        {
            if (xml == null)
                return null;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNode node = GetSubNode(doc.DocumentElement, xpath);
                return ToNameValueCollection(node, null);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static NameValueCollection ToNameValueCollection(XmlNode node, String node_name)
        {
            try
            {
                NameValueCollection nv = new NameValueCollection();

                foreach (XmlNode n in node)
                {
                    // is leaf node
                    if (n.ChildNodes.Count == 1 &&
                         n.ChildNodes[0].NodeType == XmlNodeType.Text)
                        nv.Add(node_name == null ? n.Name : node_name + "." + n.Name, n.InnerText);
                    else
                    {
                        // recursive 
                        nv.Add(ToNameValueCollection(
                                n,
                                node_name == null ? n.Name : node_name + "." + n.Name));
                    }
                }
                return nv;
            }
            catch (Exception) { }
            return null;
        }

        /// <summary>
        /// Deserialize xml by attribute
        /// </summary>
        /// <param name="n"></param>
        /// <param name="obj_type"></param>
        /// <returns></returns>
        public static object DeserializeByAttributes(Type obj_type, XmlNode node)
        {
            try
            {
                object obj = Activator.CreateInstance(obj_type);
                SetObjectPropertyByAttribute(obj, node);
                return obj;
            }
            catch (Exception)
            { }
            return null;
        }

        /// <summary>
        /// Set value of object's property by XmlNode's attributes
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="?"></param>
        public static void SetObjectPropertyByAttribute(object obj, XmlNode node)
        {
            if (obj == null)
                return;
            try
            {
                PropertyInfo[] pinfo = obj.GetType().GetProperties();

                foreach (PropertyInfo p in pinfo)
                {
                    if (node.Attributes[p.Name] != null)
                    {
                        try
                        {
                            p.SetValue(obj, StringConverterManager.ConvertTo(node.Attributes[p.Name].Value.Trim(), p.PropertyType), null);
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception)
            { }
        }

        public static void SetObjectPropertyByElement(object obj, XmlNode node)
        {
            if (obj == null)
                return;

            // create a object instance
            PropertyInfo[] pinfos = obj.GetType().GetProperties();

            foreach (PropertyInfo p in pinfos)
            {
                try
                {
                    XmlNode n = node.SelectSingleNode(p.Name);
                    if (n != null && StringConverter.CanConvertDirectly(p.PropertyType))
                        p.SetValue(obj, StringConverterManager.ConvertTo(n.InnerText, p.PropertyType), null);
                }
                catch (Exception) { }
            }
        }


        public static T ToOjbect<T>(string xml, Encoding encoding)
        {
            byte[] buff = encoding.GetBytes(xml);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            MemoryStream stream = new MemoryStream(buff, 0, buff.Length);
            object obj = xmlSerializer.Deserialize(stream);
            stream.Close();
            return (T)obj;
        }
    }
}
