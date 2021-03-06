﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="EditServer.aspx.cs" Inherits="Dorado.VWS.Admin.EditServer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>修改服务器</title>
    <script type="text/javascript">
        $(document).ready(function () {
            sourceshow();
        });

        function sourceshow() {
            var id = $("#<%=ddlServerType.ClientID%>").val();

            //如果是同步宿
            if (id == "1") {
                $("#trdomain").show();
                $("#trroot").show();
            } else {
                //如果是同步中继
                if (id == "2") {
                    $("#trdomain").show();
                    $("#trroot").hide();
                }
                //如果是同步源
                else {
                    $("#trdomain").show();
                    $("#trroot").show();
                }
            }
        }

        function check() {
            var serverTypeId = $("#<%=ddlServerType.ClientID%>").val();
            var domainId = $("#<%=ddlDomain.ClientID%>").val();
            var ipRegExp = /^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$/;
            var fileRegExp = /^[a-zA-Z]\:[\\](([^\\])?[\w\.\-]+\\)*$/;

            if (domainId == "0" || domainId == null) {
                alert("域名不能为空！");
                return false;
            }
            if ($("#<%=txtIP.ClientID%>").val() == "") {
                alert("IP不能为空！");
                $("#<%=txtIP.ClientID%>").focus();
                return false;
            }
            if (!ipRegExp.test($("#<%=txtIP.ClientID%>").val())) {
                alert("IP输入有误，请重新输入！");
                $("#<%=txtIP.ClientID%>").focus();
                return false;
            }

            if (serverTypeId == "1") {

                if ($("#<%=txtRoot.ClientID%>").val() == "") {
                    alert("根目录不能为空！");
                    $("#<%=txtRoot.ClientID%>").focus();
                    return false;
                }
                //if (!fileRegExp.test($("#<%=txtRoot.ClientID%>").val())) {
                //    alert("根目录格式输入有误，请重新输入！");
                //    $("#<%=txtRoot.ClientID%>").focus();
                //    return false;
                //}
            } else if (serverTypeId == "3") {
                if ($("#<%=txtRoot.ClientID%>").val() == "") {
                    alert("根目录不能为空！");
                    $("#<%=txtRoot.ClientID%>").focus();
                    return false;
                }
                //if (!fileRegExp.test($("#<%=txtRoot.ClientID%>").val())) {
                //    alert("根目录格式输入有误，请重新输入！");
                //    $("#<%=txtRoot.ClientID%>").focus();
                //    return false;
                //}
            }
            return true;
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div style="text-align: center">
        <table border="0" align="center" cellpadding="0" cellspacing="5">
            <tr>
                <td align="center" colspan="2">
                    <b>修改服务器</b>
                </td>
            </tr>
            <tr>
                <td align="right">
                    IDC名称：
                </td>
                <td align="left">
                    <asp:DropDownList ID="ddlIDC" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlIDC_SelectedIndexChanged">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td align="right">
                    服务器类型：
                </td>
                <td align="left">
                    <asp:DropDownList ID="ddlServerType" onchange="sourceshow()" runat="server" Enabled="False">
                        <asp:ListItem Value="1">同步宿</asp:ListItem>
                        <asp:ListItem Value="2">同步中继</asp:ListItem>
                        <asp:ListItem Value="3">同步源</asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td align="right">
                    环境：
                </td>
                <td align="left">
                    <asp:DropDownList ID="ddlEnvironment" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlEnvironment_SelectedIndexChanged">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr id="trdomain">
                <td align="right">
                    域名：
                </td>
                <td align="left">
                    <asp:DropDownList ID="ddlDomain" runat="server" AutoPostBack="True">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td align="right">
                    IP：
                </td>
                <td align="left">
                    <asp:TextBox ID="txtIP" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr id="trroot">
                <td align="right">
                    根目录：
                </td>
                <td align="left">
                    <asp:TextBox ID="txtRoot" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr id="tr1">
                <td align="right">
                    预上线：
                </td>
                <td align="left">
                    <asp:CheckBox ID="chkIsAdvanced" runat="server"></asp:CheckBox>
                </td>
            </tr>
            <tr>
                <td align="right" colspan="2">
                    <asp:Button ID="btnEdit" TabIndex="0" OnClientClick="return check();" runat="server"
                        Text="修 改" OnClick="btnEdit_Click" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>