using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.IO;
using System.Configuration;
using System.Web;
using System.Xml;
using System.Net;

namespace Synjones.Common.Helper
{
    /// <summary>    
    /// 邮件发送工具   
    /// </summary>  
    public class MailHelper
    {
        private MailHelper() { }
        /// <summary>      
        /// 发送邮件     
        /// </summary>             
        /// <param name="receiver">邮件接受者</param>       
        /// <param name="subject">邮件标题</param>       
        /// <param name="body">邮件正文</param>       
        /// <param name="attachments">附件</param>   
        public static void Send(string receiver, string subject, string body, IList<Attachment> attachments)
        {
            Send(SmtpConfig.Provider.SmtpSetting.Sender, receiver, subject, body, attachments);
        }

        /// <summary>        
        /// 发送邮件        
        /// </summary>             
        /// <param name="sender">邮件发送者</param>       
        ///  <param name="receiver">邮件接受者</param>        
        /// <param name="subject">邮件标题</param>     
        ///  <param name="body">邮件正文</param>        
        /// <param name="attachments">附件</param>       
        public static void Send(string sender, string receiver, string subject, string body, IList<Attachment> attachments)
        {
            Send(SmtpConfig.Provider.SmtpSetting.Host, SmtpConfig.Provider.SmtpSetting.Port, SmtpConfig.Provider.SmtpSetting.UserName, SmtpConfig.Provider.SmtpSetting.Password, sender, receiver, subject, body, attachments);
        }

        /// <summary>        
        /// 发送邮件       
        /// </summary>      
        /// <param name="host">邮件服务器</param>    
        /// <param name="port">端口</param>      
        /// <param name="userName">邮件服务器登录用户</param>        
        /// <param name="passwrod">邮件服务器登录密码</param>        
        /// <param name="sender">邮件发送者</param>        
        /// <param name="receiver">邮件接受者</param>        
        /// <param name="subject">邮件标题</param>        
        /// <param name="body">邮件正文</param>        
        /// <param name="attachments">附件</param>        
        public static void Send(string host, int port, string userName, string passwrod, string sender, string receiver, string subject, string body, IList<Attachment> attachments)
        {
            SmtpClient smtpClient = new SmtpClient(host, port);
            MailMessage msg = new MailMessage(sender, receiver);
            msg.IsBodyHtml = true;
            msg.SubjectEncoding = Encoding.GetEncoding("utf-8");
            msg.BodyEncoding = Encoding.GetEncoding("utf-8");
            msg.Subject = subject;
            msg.Body = body;
            msg.Attachments.Clear();
            if (attachments != null)
            {
                foreach (Attachment attach in attachments)
                {
                    // 增加附件                   
                    msg.Attachments.Add(attach);
                }
            }
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(userName, passwrod);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Send(msg);
        }
    }

    /// <summary>    
    /// 配置邮件服务器    
    /// </summary>    
    internal class SmtpConfig
    {
        private static SmtpConfig _smtpConfig;
        private SmtpSetting _smtpSetting;
        private SmtpConfig() { }
        /// <summary>        
        /// 获取邮件服务器配置        
        /// </summary>       
        public static SmtpConfig Provider
        {
            get
            {
                if (_smtpConfig == null)
                {
                    _smtpConfig = new SmtpConfig();
                }
                return _smtpConfig;
            }
        }
        /// <summary>        
        /// 配置信息        
        /// </summary>       
        public SmtpSetting SmtpSetting
        {
            get
            {
                if (_smtpSetting == null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(this.ConfigFile);
                    _smtpSetting = new SmtpSetting();
                    _smtpSetting.Host = doc.DocumentElement.SelectSingleNode("Host").InnerText;
                    _smtpSetting.Port = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("Port").InnerText);
                    _smtpSetting.UserName = doc.DocumentElement.SelectSingleNode("UserName").InnerText;
                    _smtpSetting.Password = doc.DocumentElement.SelectSingleNode("Password").InnerText;
                    _smtpSetting.Sender = doc.DocumentElement.SelectSingleNode("Sender").InnerText;
                }
                return _smtpSetting;
            }
        }
        /// <summary>        
        /// 读取配置文件        
        /// </summary>        
        private string ConfigFile
        {
            get
            {
                /*                 
                 * *  增加如下配置(Web.config/App.config)                 
                 * *                   
                 * * ------------------------------------------                 
                 * *  <appSettings>                 
                 * *      <add key="SmtpConfig" value="Config/SmtpSetting.config"/>                
                 * *  </appSettings>                 
                 * * ------------------------------------------                 
                 * *   配置文件内容(SmtpSetting.config)                 
                 * *                    
                 * * ------------------------------------------                 
                 * *  <SmtpSetting>                 
                 * *      <Host>mail.welan.cn</Host>                 
                 * *      <Port>25</Port>                 
                 * *      <UserName>yourcompany@welan.com</UserName>                 
                 * *      <Password>yourpassword</Password>                 
                 * *  <   Sender>yourcompany@welan.com</Sender>                 
                 * *  </SmtpSetting>                 
                 * * ------------------------------------------                 
                 */
                string configPath = ConfigurationManager.AppSettings["SmtpConfig"];
                if (HttpContext.Current != null)
                {
                    configPath = HttpContext.Current.Server.MapPath(configPath);
                }
                else
                {
                    configPath = configPath.Replace("/", "\\");
                    configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configPath);
                } return configPath;
            }
        }
    }
    internal class SmtpSetting
    {
        private string _host;
        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }
        private int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        private string _sender;
        public string Sender
        {
            get { return _sender; }
            set { _sender = value; }
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            IList<Attachment> attachs = new List<Attachment>();
            Attachment attach = new Attachment(@"E:\job\doc\TFS.txt");
            attachs.Add(attach); for (int i = 0; i < 10; i++)
            {
                // 测试                
                MailHelper.Send("ideal35500@163.com", "测试邮件", "测试邮件正文", attachs);
            } System.Console.ReadKey();
        }
    }
}

