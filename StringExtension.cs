using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace System
{
    public static class StringExtension
    {
        public static T JsonToObject<T>(this string jsonStr)
        {
            T t = new JavaScriptSerializer().Deserialize<T>(jsonStr);
            return t;
        }
    }
}
