using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.WebHelper
{
    /// <summary>
    ///  interface of String conversion
    ///  Convert String to any other type
    /// </summary>
    public interface IStringConverter
    {
        bool CanConvert(Type t);
        object ConvertTo(Type t, String obj);
    }
}
