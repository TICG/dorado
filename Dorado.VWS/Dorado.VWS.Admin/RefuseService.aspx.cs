/*-------------------------------------------------------------------------
 * ��Ȩ���У�Dorado
 * ʱ�䣺 2012/1/4 10:49:46
 * ���ߣ�
 * �汾            ʱ��                  ����                 ����
 * v 1.0    2012/1/4 10:49:46               ����
 * ������Ҫ��;������
 *  -------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Xml;
using Dorado.Configuration;
using Dorado.Core;
using Dorado.Core.Logger;
using Dorado.VWS.Services;

namespace Dorado.VWS.Admin
{
    public partial class RefuseService : System.Web.UI.Page
    {
        //private static readonly ILog Logger = LogManager.GetLogger(typeof(RefuseService));
        private string file = AppDomain.CurrentDomain.BaseDirectory + AppSettingProvider.Get("RefuseService");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                XmlDocument xml = new XmlDocument();
                try
                {
                    xml.Load(file);
                    var list = xml.GetElementsByTagName("service");
                    foreach (XmlNode node in list)
                    {
                        TextBox1.Text += node.InnerText + '\n';
                    }
                }
                catch (Exception ex)
                {
                    LoggerWrapper.Logger.Error("VWS.Admin", ex.ToString());
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            //����Xml�������֣���<?xml version="1.0" encoding="utf-8" ?>
            //xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"); //�������ڵ�
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes")); //�������ڵ�
            XmlNode rootNode = xmlDoc.CreateElement("services"); //����services�ӽڵ�
            string[] services = TextBox1.Text.Split('\n');
            var list = new List<string>();
            foreach (string service in services)
            {
                if (!string.IsNullOrWhiteSpace(service) && !list.Contains(service))
                {
                    list.Add(service);
                    XmlNode serviceNode = xmlDoc.CreateElement("service");
                    serviceNode.InnerText = service.Trim();
                    rootNode.AppendChild(serviceNode);
                }
            }
            xmlDoc.AppendChild(rootNode);

            try
            {
                xmlDoc.Save(file);//����Xml�ĵ�
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("VWS.Admin", ex.ToString());
            }
            Label1.Text = "����ɹ�";
            WebCache.Remove("refuseservicellist");
        }
    }
}