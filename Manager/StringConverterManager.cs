using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.WebHelper
{
    /// <summary>
    /// String conversion container
    /// Provide String convert to other data type
    /// </summary>
    public class StringConverterManager
    {
        static private List<IStringConverter> _converter;

        static StringConverterManager()
        {
            _converter = new List<IStringConverter>();
            _converter.Add(new StringConverter());
        }

        /// <summary>
        /// convert String to type
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static object ConvertTo(String s, Type t)
        {
            foreach (IStringConverter c in _converter)
            {
                if (c.CanConvert(t))
                    return c.ConvertTo(t, s);
            }
            return null;
        }

        /// <summary>
        /// add new converter
        /// </summary>
        /// <param name="converter"></param>
        public static void AddConverter(IStringConverter converter)
        {
            _converter.Add(converter);
        }
    }
}
