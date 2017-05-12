/*-------------------------------------------------------------------------
 * ��Ȩ���У�Dorado
 * ʱ�䣺 2011/10/25 14:32:40
 * ���ߣ�
 * �汾            ʱ��                  ����                 ����
 * v 1.0    2011/10/25 14:32:40               ����
 * ������Ҫ��;������
 *  -------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Dorado.VWS.Model;

//using Dorado.VWS.Admin.PermissionWCF;
using Dorado.VWS.Model.Enum;
using Dorado.VWS.Services;

namespace Dorado.VWS.Admin
{
    public partial class RoleMember : WebBasePage
    {
        private readonly DomainProvider _domianProvider = new DomainProvider();
        private readonly RoleProvider _roleProvider = new RoleProvider();
        private readonly UserRoleProvider _userroleProvider = new UserRoleProvider();

        private readonly LogProvider _logProvider = new LogProvider();

        //private readonly PermissionWCFClient permissionClient = new PermissionWCFClient();
        private readonly ServerProvider _serverProvider = new ServerProvider();

        private IList<string> allUsers;
        private IList<string> roleUsers;
        private Model.RoleEntity role;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int roleId = 0;
                //if(string.IsNullOrWhiteSpace(Request.QueryString["roleId"]))
                //{
                int.TryParse(Request.QueryString["roleId"], out roleId);
                //}

                //ddlDomains.DataSource = _domianProvider.GetAllDomains();
                //ddlDomains.DataTextField = "DomainName";
                //ddlDomains.DataValueField = "DomainId";
                //ddlDomains.DataBind();
                InitEnvironmentData();

                if (roleId > 0)
                {
                    role = _roleProvider.GetRoleByID(roleId);
                    if (role != null)
                    {
                        var dataSource = GetUserDomians(EnumManageType.DailyManage);
                        var domain = dataSource.First(item => item.DomainId == role.DomainId);
                        if (DdlEnvironment.Items.FindByValue(domain.Environment.ToString()) != null)
                        {
                            DdlEnvironment.SelectedIndex = -1;
                            DdlEnvironment.Items.FindByValue(domain.Environment.ToString()).Selected = true;
                        }

                        ddlDomains.DataSource = (from item in dataSource where (item.Environment == domain.Environment) select item);

                        ddlDomains.DataValueField = "DomainId";
                        ddlDomains.DataTextField = "DomainName";
                        ddlDomains.DataBind();
                        ddlDomains.Items.Insert(0, new ListItem("��ѡ��", "0"));

                        if (ddlDomains.Items.FindByValue(role.DomainId.ToString()) != null)
                        {
                            ddlDomains.SelectedIndex = -1;
                            ddlDomains.Items.FindByValue(role.DomainId.ToString()).Selected = true;
                        }
                        bindRole();
                        if (ddlRoles.Items.FindByValue(role.Id.ToString()) != null)
                        {
                            ddlRoles.SelectedIndex = -1;
                            ddlRoles.Items.FindByValue(role.Id.ToString()).Selected = true;
                            bindRoleUser();
                        }
                    }
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (lbxAllUsers.Items.Count <= 0)
            {
                return;
            }
            List<ListItem> items = new List<ListItem>();
            for (int i = 0; i < lbxAllUsers.Items.Count; i++)
            {
                items.Add(lbxAllUsers.Items[i]);
                lbxRoleUsers.Items.Add(lbxAllUsers.Items[i]);
            }

            lbxAllUsers.Items.Clear();

            userRoleAdd(items, int.Parse(ddlRoles.SelectedValue));
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            int[] arrayIndex = lbxAllUsers.GetSelectedIndices();

            if (arrayIndex.Length <= 0)
            {
                return;
            }

            List<ListItem> items = new List<ListItem>();
            for (int i = arrayIndex.Length - 1; i >= 0; i--)
            {
                items.Add(lbxAllUsers.Items[arrayIndex[i]]);

                lbxRoleUsers.Items.Add(lbxAllUsers.Items[arrayIndex[i]]);
                lbxAllUsers.Items.Remove(lbxAllUsers.Items[arrayIndex[i]]);
            }

            userRoleAdd(items, int.Parse(ddlRoles.SelectedValue));
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            int[] arrayIndex = lbxRoleUsers.GetSelectedIndices();

            if (arrayIndex.Length <= 0)
            {
                return;
            }

            List<ListItem> items = new List<ListItem>();

            for (int i = arrayIndex.Length - 1; i >= 0; i--)
            {
                items.Add(lbxRoleUsers.Items[arrayIndex[i]]);

                lbxAllUsers.Items.Add(lbxRoleUsers.Items[arrayIndex[i]]);
                lbxRoleUsers.Items.Remove(lbxRoleUsers.Items[arrayIndex[i]]);
            }

            userRoleDelete(items, int.Parse(ddlRoles.SelectedValue));
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            if (lbxRoleUsers.Items.Count <= 0)
            {
                return;
            }

            List<ListItem> items = new List<ListItem>();

            for (int i = 0; i < lbxRoleUsers.Items.Count; i++)
            {
                items.Add(lbxRoleUsers.Items[i]);
                lbxAllUsers.Items.Add(lbxRoleUsers.Items[i]);
            }

            lbxRoleUsers.Items.Clear();

            userRoleDelete(items, int.Parse(ddlRoles.SelectedValue));
        }

        protected void ddlDomains_SelectedIndexChanged(object sender, EventArgs e)
        {
            bindRole();
            bindRoleUser();
        }

        protected void ddlRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            bindRoleUser();
        }

        protected void bindRole()
        {
            ddlRoles.DataSource = _roleProvider.GetRoleByDomainId(int.Parse(ddlDomains.SelectedValue));
            ddlRoles.DataTextField = "RoleName";
            ddlRoles.DataValueField = "ID";
            ddlRoles.DataBind();
        }

        /// <summary>
        /// �󶨽�ɫ��Ϣ
        /// </summary>
        /// <param name="roleId"></param>
        protected void BindRoleInfo(int roleId)
        {
            if (roleId > 0)
            {
                ltGotoRoleDetail.Text = string.Format("<a href='EditRole.aspx?id={0}'>�༭</a>", roleId);
                PermissionProvider pmsProvider = new PermissionProvider();
                IList<PermissionEntity> permissionList = pmsProvider.GetPermissionByRoleId(roleId);
                StringBuilder sb = new StringBuilder();
                sb.Append("�ý�ɫӵ�б����������ļ��е�ͬ��Ȩ��:<br/>");
                foreach (PermissionEntity permissionEntity in permissionList)
                {
                    if (permissionEntity.Path.Trim() == string.Empty)
                    {
                        ltRoleInfo.Text = "�ý�ɫӵ�б������µ������ļ�ͬ��Ȩ��<br/>";
                        return;
                    }
                    sb.Append(permissionEntity.Path + "<br/>");
                }

                ltRoleInfo.Text = sb.ToString();
            }
        }

        /// <summary>
        /// ���û���Ϣ
        /// </summary>
        protected void bindRoleUser()
        {
            lbxAllUsers.Items.Clear();
            lbxRoleUsers.Items.Clear();
            ltGotoRoleDetail.Text = string.Empty;
            ltRoleInfo.Text = string.Empty;

            if (!string.IsNullOrWhiteSpace(ddlRoles.SelectedValue))
            {
                int roleId = 0;
                int.TryParse(ddlRoles.SelectedValue, out roleId);

                //�󶨽�ɫ��Ϣ
                BindRoleInfo(roleId);

                ActivateUserProvider userProvider = new ActivateUserProvider();

                allUsers = userProvider.GetAllUsers().Select(x => x.UserName).ToList();

                roleUsers = _userroleProvider.GetUsersByRoleId(roleId);

                foreach (string user in allUsers)
                {
                    lbxAllUsers.Items.Add(user);
                }

                foreach (string user in roleUsers)
                {
                    lbxRoleUsers.Items.Add(user);
                    lbxAllUsers.Items.Remove(user);
                }
            }
        }

        /// <summary>
        /// ��ӽ�ɫ��Ա
        /// </summary>
        /// <param name="items"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        protected bool userRoleAdd(IList<ListItem> items, int roleId)
        {
            var listUserRole = new List<Model.UserRoleEntity>();

            foreach (ListItem item in items)
            {
                Model.UserRoleEntity userRole = new Model.UserRoleEntity();
                userRole.roleId = roleId;
                userRole.UserName = item.Text;
                listUserRole.Add(userRole);
            }
            int result = _userroleProvider.Add(listUserRole);

            if (items.Count == result)
            {
                writeOperateLog(0, items);
                return true;
            }
            else
            {
                writeOperateLog(0, items, false);
                return false;
            }
        }

        /// <summary>
        /// ɾ����ɫ��Ա
        /// </summary>
        /// <param name="items"></param>
        /// <param name="roleId"></param>
        protected bool userRoleDelete(IList<ListItem> items, int roleId)
        {
            int result = 0;
            foreach (ListItem item in items)
            {
                result += _userroleProvider.Delete(roleId, item.Text);
            }
            if (items.Count == result)
            {
                writeOperateLog(1, items);
                return true;
            }
            else
            {
                writeOperateLog(1, items, false);
                return false;
            }
        }

        /// <summary>
        /// д�������־
        /// </summary>
        /// <param name="opType">0��ӣ�1ɾ��</param>
        /// <param name="userList">�û��б�</param>
        protected void writeOperateLog(int opType, IList<ListItem> userList)
        {
            writeOperateLog(opType, userList, true);
        }

        /// <summary>
        /// д�������־
        /// </summary>
        /// <param name="opType">0��ӣ�1ɾ��</param>
        /// <param name="userList">�û��б�</param>
        /// <param name="state">boolִ�н��״̬</param>
        protected void writeOperateLog(int opType, IList<ListItem> userList, bool state)
        {
            //���������־
            var operateLogEntity = new Model.OperationLogEntity
            {
                UserName = CurUserName,
                DomainName = ddlDomains.SelectedItem.Text,
                OperateType = EnumOperateType.PermissionManage,
                Log = "���½�ɫ(" + ddlRoles.SelectedItem.Text + ")��Ա ",
                Result = state
            };

            string operation = "";
            switch (opType)
            {
                case 0:
                    operation = "���";
                    break;

                case 1:
                    operation = "ɾ��";
                    break;

                default:
                    operation = "δ֪����";
                    break;
            }
            operateLogEntity.Log += operation + ":";

            foreach (ListItem item in userList)
            {
                operateLogEntity.Log += item.Text + ",";
            }
            _logProvider.AddOperateLog(operateLogEntity);
        }

        private void InitEnvironmentData()
        {
            DdlEnvironment.Items.Insert(0, new ListItem("��ѡ��", EnvironmentType.UnSelected));
            //DdlEnvironment.Items.Insert(1, new ListItem("����", EnvironmentType.Development));
            //DdlEnvironment.Items.Insert(2, new ListItem("����", EnvironmentType.Testing));
            DdlEnvironment.Items.Insert(1, new ListItem("����", EnvironmentType.Acceptance));
            //DdlEnvironment.Items.Insert(4, new ListItem("Ԥ����", EnvironmentType.Advanced));
            DdlEnvironment.Items.Insert(2, new ListItem("����", EnvironmentType.Production));
        }

        protected void DdlEnvironment_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlDomains.DataSource = GetUserDomians(EnumManageType.DailyManage, DdlEnvironment.SelectedValue);

            ddlDomains.DataValueField = "DomainId";
            ddlDomains.DataTextField = "DomainName";
            ddlDomains.DataBind();
            ddlDomains.Items.Insert(0, new ListItem("��ѡ��", "0"));
        }
    }
}