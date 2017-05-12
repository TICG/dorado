/*-------------------------------------------------------------------------
 * ��Ȩ���У�Dorado
 * ʱ�䣺 2011/11/28 17:19:27
 * ���ߣ�
 * �汾            ʱ��                  ����                 ����
 * v 1.0    2011/11/28 17:19:27               ����
 * ������Ҫ��;������
 *  -------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using Dorado.VWS.Model;
using Dorado.VWS.Services;
using Dorado.VWS.Utils;

namespace Dorado.VWS.Admin
{
    public partial class getfilelist : WebBasePage
    {
        private readonly ServerProvider _serverProvider = new ServerProvider();
        private readonly FileListProvider _flProvider = new FileListProvider();
        private readonly Regex _regexFileName = new Regex(@"[\u4e00-\u9fa5]+");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ddlDomain.DataSource = _serverProvider.GetDomainsByIdcId(0); ;
                ddlDomain.DataValueField = "DomainId";
                ddlDomain.DataTextField = "DomainName";
                ddlDomain.DataBind();
                ddlDomain.Items.Insert(0, new ListItem("��ѡ��", "0"));
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            tbResult.Text = string.Empty;
            int domainID = 0;
            int.TryParse(ddlDomain.SelectedValue, out domainID);
            if (domainID < 1)
            {
                Label1.Text = "��ѡ������";
                return;
            }

            StringBuilder sb = new StringBuilder();
            int fileCount = 0;
            //GetFileList(int.Parse(ddlDomain.SelectedValue), "", ref sb, ref fileCount);

            //tbResult.Text = sb.ToString();

            ServerEntity serverEntity = _serverProvider.GetSourceServerByDomainId(domainID);
            List<string> list = _flProvider.GetAllFileName(serverEntity.ServerId);
            if (list != null)
            {
                fileCount = list.Count;
                foreach (string f in list)
                {
                    if (_regexFileName.Match(f).Success)
                    {
                        fileCount--;
                        continue;
                    }
                    sb.AppendLine(f.Replace(serverEntity.Root, ""));
                }
            }
            tbResult.Text = sb.ToString();
            Label1.Text = "��ȡ��� " + DateTime.Now + " �� " + fileCount + " ���ļ�";
        }

        /// <summary>
        /// �����ļ��б� �ݹ����
        /// </summary>
        /// <param name="domainid"></param>
        /// <param name="dir"></param>
        /// <param name="sb"></param>
        protected void GetFileList(int domainid, string dir, ref StringBuilder sb, ref int fileCount)
        {
            ServerEntity serverEntity = _serverProvider.GetSourceServerByDomainId(domainid);

            if (serverEntity != null)
            {
                if (string.IsNullOrEmpty(dir))
                {
                    dir = serverEntity.Root;
                }
                //��ȡ�ļ��б�
                List<VwsDirectoryInfo> filelist = _flProvider.GetFileListNoMd5(serverEntity.ServerId, dir);
                foreach (VwsDirectoryInfo file in filelist)
                {
                    if (_regexFileName.Match(file.FullName).Success)
                    {
                        continue;
                    }

                    if (file.IsFolder)
                    {
                        GetFileList(domainid, file.FullName, ref sb, ref fileCount);
                    }
                    else
                    {
                        sb.AppendLine(file.FullName.Replace(serverEntity.Root, ""));
                        fileCount++;
                    }
                }
            }
        }
    }
}