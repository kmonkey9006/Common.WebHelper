using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Common.WebHelper
{
    /// <summary>
    /// provide basic converstion for String
    /// </summary>
    public class StringConverter : IStringConverter
    {
        private static Type[] _types = new Type[]
		{
		    typeof (Char),
			typeof (Decimal),
			typeof (Boolean),
			typeof (Int16),
			typeof (Int32),
			typeof (Int64),
			typeof (UInt16),
			typeof (UInt32),
			typeof (UInt64),
			typeof (Byte),
			typeof (SByte),
			typeof (Single),
			typeof (Double),
			typeof (String)
		};

        public StringConverter()
        {
        }

        public bool CanConvert(Type t)
        {
            return CanConvertDirectly(t);
        }

        public object ConvertTo(Type obj_type, String value)
        {
            //if obj_type is String & string , return directly
            if (obj_type == typeof(String))
                return value;

            if (_types.Contains(obj_type))
                return System.Convert.ChangeType(value, obj_type, null);

            if (obj_type.IsEnum)
                return Convert2Enum(obj_type, value);

            if (obj_type == typeof(DateTime))
                return Convert2DateTime(value);

            if (obj_type == typeof(NameValueCollection))
                return Convert2NameValueCollection(value, CharGap);
            return null;
        }


        /// <summary>
        /// Check object type if can convert from String Directly
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool CanConvertDirectly(Type t)
        {
            if (t.Equals(typeof(String)) ||
                t.Equals(typeof(DateTime)) ||
                t.IsEnum ||
                _types.Contains(t))
                return true;
            return false;
        }

        /// <summary>
        /// String Convert to Enum 
        /// </summary>
        /// <param name="obj_type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object Convert2Enum(Type obj_type, String value)
        {
            try
            {
                return (Enum.Parse(obj_type, value));
            }
            catch (Exception)
            {
            }
            return null;
        }

        private static String[] DateFormate =
		{
			"yyyyMMddHHmmss",
			"yyyyMMddHHmm",
			"yyyyMMdd",
			"yyyy/MM/dd HH:mm:ss",
			"yyyy/MM/dd",
			"yyyy-MM-dd HH;mm:ss",
			"yyyy-MM-dd",
			"MM/dd/yyyy",
			"MM/dd/yyyy HH;mm:ss",
		};

        // Convert to DateTime
        public static DateTime Convert2DateTime(String value)
        {
            // try System.Convert first
            try
            {
                return (DateTime)System.Convert.ChangeType(value, typeof(DateTime), null);
            }
            catch (Exception) { }

            // use self defined date format
            for (int i = 0; i < DateFormate.Length; i++)
            {
                try
                {
                    return DateTime.ParseExact(value, DateFormate[i], null);
                }
                catch (Exception) { }
            }

            // can't conversion
            throw new Exception("Can't Convert to DateTime ");
        }


        private static Char[] CharGap = { '&', '|', ',', ';' };

        // Convert to NameValueCollection
        public static NameValueCollection Convert2NameValueCollection(String value, Char[] gap)
        {
            NameValueCollection nv = new NameValueCollection();

            try
            {
                String[] subString = value.Split(gap);
                foreach (String s in subString)
                {
                    int i = s.IndexOf('=');
                    if (i > 0 && i < s.Length)
                    {
                        String k = s.Substring(0, i);
                        String v = s.Substring(i + 1);
                        nv.Add(k, v);
                    }
                }
            }
            catch (Exception)
            { }
            return nv;
        }

        // Convert to Entity obejct
        public static object Convert2Entity(Type obj_type, String value, Char[] gap)
        {
            /*
            return CollectionHelper.CreateEntityObject(
                    obj_type,
                    Convert2NameValueCollection(inString, gap),
                    null);
             */
            return null;
        }
    }
}
