using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Net.Security;

namespace StrokeLocal.WebHelper
{
    public class HttpHelper
    {
        static HttpHelper()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                            new System.Net.Security.RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);
        }
        public static string Get(string url, string data, HttpCookieCollection cookies, string charsSet = "utf-8")
        {
            return Response(url, data, cookies, "GET", charsSet);
        }
        public static string Get(string url, string data, string charsSet = "utf-8")
        {
            return Get(url, data, null, charsSet);
        }
        public static string Post(string url, string data, string charsSet = "utf-8")
        {
            return Post(url, data, null, charsSet);
        }
        public static string Post(string url, string data, HttpCookieCollection cookies, string charsSet = "utf-8")
        {
            return Response(url, data, cookies, "POST", charsSet);
        }
        /// <summary>
        /// 向指定地址发送POST请求
        /// </summary>
        /// <param name="getUrl">指定的网页地址</param>
        /// <param name="postData">POST的数据（格式为：p1=v1&p1=v2）</param>
        /// <param name="chars_set">可采用如UTF-8,GB2312,GBK等</param>
        /// <returns>页面返回内容</returns>
        public static string Response(string url, string postData, HttpCookieCollection cookies, string method = "POST", string charsSet = "utf-8")
        {

            Encoding encoding = Encoding.GetEncoding(charsSet);
            HttpWebRequest Request;
            if (url.StartsWith("https", StringComparison.CurrentCultureIgnoreCase))
            {
                //是https请求的时候
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);
                Request = WebRequest.Create(url) as HttpWebRequest;
                Request.ProtocolVersion = HttpVersion.Version10;
            }
            else
                Request = (HttpWebRequest)WebRequest.Create(url);
            //设置CookieContainer，商城页面使用了cookie，此处不设置CookieContainer会请求失败
            Request.CookieContainer = new CookieContainer();
            Request.Method = method;
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.AllowAutoRedirect = true;
            byte[] postdata = encoding.GetBytes(postData);
            if (!method.Equals("get", StringComparison.CurrentCultureIgnoreCase) || !string.IsNullOrEmpty(postData))
            {
                using (Stream newStream = Request.GetRequestStream())
                {
                    newStream.Write(postdata, 0, postdata.Length);
                }
            }
            using (HttpWebResponse response = (HttpWebResponse)Request.GetResponse())
            {

                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, encoding, true))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public static bool RemoteCertificateValidationCallback(
            Object sender,
            X509Certificate certificate,
            X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static byte[] Post4ReturnByteArray(string url, string para)
        {
            using (WebClient wc = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                string[] paraArray = para.Split('&');
                foreach (string paraTemp in paraArray)
                {
                    string[] paraTempArray = paraTemp.Split('=');
                    if (paraTempArray.Length >= 2)
                    {
                        string name = paraTempArray[0];
                        string value = paraTemp.Substring(name.Length + 1);
                        data[name] = value;
                    }
                }
                byte[] bs = wc.UploadValues(url, "POST", data);
                return bs;
            }
        }


        /// <summary>
        ///获取返回到浏览器的URL地址
        /// </summary>
        /// <param name="Url">地址</param>
        /// <param name="postDataStr">参数</param>
        /// <returns></returns>
        public static string GetBrowserUrl(string Url, string postDataStr)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
                Stream myRequestStream = request.GetRequestStream();
                StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
                myStreamWriter.Write(postDataStr);
                myStreamWriter.Close();
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream myResponseStream = response.GetResponseStream())
                    {
                        using (StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8")))
                        {
                            string retString = myStreamReader.ReadToEnd();
                            string ret = response.ResponseUri.ToString();
                            myResponseStream.Close();
                            myStreamReader.Close();
                            return ret + "|" + retString;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }

        }
    }
}
