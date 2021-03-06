﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="SyncSelectFiles.aspx.cs" Inherits="Dorado.VWS.Admin.SyncSelectFiles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>同步-选择文件</title>
    <link rel="stylesheet" href="Styles/zTreeStyle.css" type="text/css" />
    <style>
        .historyditails
        {
            display: none;
        }
    </style>
    <script type="text/javascript" src="Scripts/jquery.ztree-2.6.js"> </script>
    <script type="text/javascript" src="Scripts/jquery.ffpager.js"> </script>
    <script type="text/javascript">
        var SyncModel = false;
        var zTree1;
        var setting;
        var zNodes = [];
        var did;
        var domainid = 0;
        setting = {
            checkable: true,
            addDiyDom: addDiyDom,
            async: true,
            fontCss: setFont,
            asyncUrl: "Handler/FileListHandler.ashx",
            //获取节点数据的URL地址
            asyncParam: ["id"],
            callback: {
                asyncSuccess: refreshSuc
            }
        };

        $(
            function () {
                $("#tdFolderList").hide();
                $("#loading").fadeOut(500);
                $("#<%=BtnAdd.ClientID %>").hide();
                $("#<%=BtnRefresh.ClientID %>").hide();
                $("#<%=DdlDomains.ClientID%>").change(
                    function () {
                        // 当域名ID不为0时，刷新树
                        domainid = $("#<%=DdlDomains.ClientID%>").val();
                        if (domainid != '0') {
                            did = domainid;
                            refreshTree(domainid);
                            ShowRecord();
                        } else {
                            HideRecord();
                        }
                    }
                );
                // 当域名ID不为0时，刷新树
                domainid = $("#<%=DdlDomains.ClientID%>").val();
                if (domainid != '0') {
                    did = domainid;
                    refreshTree(domainid);
                    ShowRecord();
                } else {
                    HideRecord();
                }
                $("#divFileInput").hide();
                $("#divFileSelect").show();

                $("#aFileInput").click(
                    function () {
                        $("#divFileInput").show();
                        $("#divFileSelect").hide();
                    }
                );

                $("#aFileSelect").click(
                    function () {
                        $("#divFileInput").hide();
                        $("#divFileSelect").show();
                    }
                );

                $("#chkAll").click(
                    function () {
                        if ($(this).attr("checked")) {
                            zTree1.checkAllNodes(true);
                        } else {
                            zTree1.checkAllNodes(false);
                        }
                    }
                );

                $("#cbxModified").click(
                    function () {
                        var checked = false;
                        if ($("#cbxModified").attr("checked")) {
                            checked = true;
                        }

                        zTree1.checkModifiedNodes(checked);
                    }
                );

                $("#cbxShowModifiedFiles").click(
                    function () {
                        if ($("#cbxShowModifiedFiles").attr("checked")) {
                            SyncModel = true;
                        } else {
                            SyncModel = false;
                        }

                        Refresh();
                    }
                );

            }
	);

        function setFont(treeId, treeNode) {
            if (!treeNode.isParent && treeNode.isProce) {
                var css = { color: "#f0742e" };

                return css;
            } else {
                return {};
            }
        }

        function refreshTree(domainid) {
            $.getJSON("/Handler/GetInfoHandler.ashx", { "action": "testConnByDomainId", "domainid": domainid }, function (json) {
                if (json.ret) {
                    if (!SyncModel) {
                        $("#loading").fadeIn(500);
                        setting.asyncParamOther = {
                            "permission": "1",
                            "domainid": domainid
                        };

                        zTree1 = $("#tree").zTree(setting, zNodes);
                        if (domainid != '0') {
                            getversion(1, 10);
                        }
                        $("#loading").fadeOut(500);
                    } else {
                        var zNodes;
                        $("#loading").fadeIn(500);
                        $.ajax({
                            url: 'Handler/ChangedFileListHandler.ashx?permission=1&domainid='+domainid, //url  action是方法的名称
                            //data: { id: "", permission: "1", domainid: domainid },
                            type: 'POST',
                            dataType: "text", //可以是text，如果用text，返回的结果为字符串；如果需要json格式的，可是设置为json
                            ContentType: "application/json; charset=utf-8",
                            success: function (data) {
                                zNodes = data;
                                //$.fn.zTree.init($("#tree"), settingSync, eval('(' + zNodes + ')'));
                                zTree1 = $("#tree").zTree(settingSync, eval('(' + zNodes + ')'));
                                zTree1.expandAll(true);
                                if (domainid != '0') {
                                    getversion(1, 10);
                                }
                                refreshSuc();
                                $("#loading").fadeOut(500);
                            },
                            error: function (msg) {
                                alert("加载失败");
                                $("#loading").fadeOut(500);
                            }
                        });
                    }
                } else {
                    alert("同步源连接失败！");
                    window.location.href = 'SyncSelectFiles.aspx';
                }
            });
        }

        function refreshSuc() {
            $("#<%=BtnAdd.ClientID %>").show();
            $("#<%=BtnRefresh.ClientID %>").show();
            $("#loading").fadeOut(500);
        }

        function addDiyDom(treeId, treeNode) {
            //alert(treeNode.toString());
            var filepath = treeNode.id.replace(/\\/g, '\\\\');
            if ($("#diyChk_" + treeNode.id).length > 0 || treeNode.isParent) return;
            var aObj = $("#" + treeNode.tId + "_a");
            var downLink = "&nbsp;&nbsp;<a href='javascript:;' onclick='DownloadFile(\"" + filepath + "\")'>下载</a>";
            aObj.after(downLink);
            if (treeNode.hasDelPermission) {
                var editStr = "<span id='diyChk_space_" + treeNode.id + "'>&nbsp;</span>" + "<input type='checkbox' class='delcheckbox' id='diyChk_" + treeNode.id + "' value='" + treeNode.id + "'>删除</input>";
                aObj.after(editStr);
            }

            //            if(treeNode.hasChanged) {
            //                treeNode.icon = "edit.png";
            //            }
        }

        ;

        function check() {
            if ($("#<%=TxtFileList.ClientID%>").length > 0 && $("#<%=TxtFileList.ClientID%>").val() != '') {
                return true;
            }
            var tmp = zTree1.getCheckedNodes();

            var chks = "";
            $.each(tmp, function (i, n) {
                if ($(n).attr("isParent") == false) {
                    chks = chks + $(n).attr("id") + "###";
                }
            });
            $("#<%=HfAddFiles.ClientID%>").val(chks);

            chks = "";
            var deltmp = $(".delcheckbox:checked");
            $.each(deltmp, function (i, n) {
                chks = chks + $(n).val() + "###";
            });
            $("#<%=HfDelFiles.ClientID%>").val(chks);

            if ($("#<%=HfAddFiles.ClientID%>").val() == '' && $("#<%=HfDelFiles.ClientID%>").val() == '') {
                alert("请选择文件");
                return false;
            }

            if ($("#<%=HfDelFiles.ClientID%>").val() != '') {
                return confirm("删除很危险，请确认！");
            }

            return true;
        }
        //载入历史版本
        function getversion(pageIndex, pageSize) {
            var begin = (pageIndex - 1) * pageSize + 1;
            var status = "";
            $.getJSON("/Handler/GetInfoHandler.ashx", { "action": "sucsynctasks", 'view': 'all', "domainid": domainid, "begin": begin, "end": begin + pageSize - 1 }, function (data) {
                if (data.Version.length > 0) {
                    var str = "<table cellpadding='5' cellspacing='0' class='listTable'><tr><th align='center'>任务编号</th><th align='center'>状态</th><th align='center'>同步任务信息</th><th align='center'>创建者</th><th align='center'>创建时间</th><th align='center' class=\"historyditails\">详情</th></tr>";
                    for (var i = 0; i < data.Version.length; i++) {
                        //状态信息（1：同步中 2：同步挂起 3：同步成功 4：同步失败 5：同步被回滚 6：同步回滚失败）
                        switch (data.Version[i].SyncStatus) {
                            case 1:
                                status = "同步中";
                                break
                            case 2:
                                status = "同步挂起";
                                break
                            case 3:
                                status = "同步成功";
                                break
                            case 4:
                                status = "同步失败";
                                break
                            case 5:
                                status = "同步被回滚";
                                break
                            case 6:
                                status = "同步回滚失败";
                                break
                        }
                        var description = data.Version[i].Description.length > 30 ? data.Version[i].Description.substring(0, 30) : data.Version[i].Description;
                        str += "<tr><td align='center'>" + data.Version[i].TaskId + "</td><td align='center'>" + status + "</td><td>" + description + "</td><td>" + data.Version[i].UserName + "</td><td>" + ConvertDatetime(data.Version[i].CreateTime) + "</td><td align='left' class=\"historyditails\"> 增加文件：<br />" + data.Version[i].AddFiles.replace(/,/g, '<br />') + "<br />删除文件：<br />" + data.Version[i].DelFiles.replace(',', '<br />') + "</td><tr>";
                    }
                    str += "</table>";
                    $("#divversion").html(str);
                    $("#divpager").ffpager({ "pageIndex": pageIndex, "pageSize": pageSize, "totalRecord": data.Count, "pageIndexChange": getversion });
                }
                else {
                    $("#divversion").html('没有同步记录！');
                }
            }
           );
        }

        // 转化日期函数

        function ConvertDatetime(obj) {
            var s;
            if (obj.indexOf("Date") > -1) {
                obj = obj.substring(obj.indexOf("(") + 1, obj.lastIndexOf(")"));
                var d = new Date(parseInt(obj));
                s = d.toLocaleString();
            } else {
                s = obj;
            }
            return s;
        }

        //下载文件

        function DownloadFile(filePath) {
            $.getJSON("/Handler/DownloadFile.ashx", { "action": "remote", "filePath": filePath, "domainID": did, "isHistory": false }, function (json) {
                if (json.ret) {
                    //                    var fileName = filePath.substr(filePath.lastIndexOf("\\") + 1);
                    location.href = "/Handler/DownloadFile.ashx?action=local&fileName=" + json.filepath;
                } else {
                    $("#tip").text("下载失败！");
                }
            });
        }
        function Refresh() {
            // 当域名ID不为0时，刷新树
            sync = false;
            domainid = $("#<%=DdlDomains.ClientID%>").val();
            if (domainid != '0') {
                did = domainid;
                refreshTree(domainid);
                ShowRecord();
            } else {
                HideRecord();
            }
                    //var zTree1 = $("#tree").zTree(setting, zNodes);

        }

        var settingSync = {
            data: {
                simpleData: {
                    enable: true
                }
            },
            checkable: true,
            addDiyDom: addDiyDom,
            fontCss: setFont,
        };
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="loading" style="display: none; width: 100%; height: 100%; position: fixed;
        top: 0%; background: #fff; z-index: 3; left: 0%; filter: alpha(Opacity=80); -moz-opacity: 0.8;
        opacity: 0.8;">
        <div style="text-align: center; padding-top: 300px;">
            <img alt="loading" src="styles/img/loading2.gif" style="width: 25px; height: 25px" /><span
                style="font-size: x-large;">刷新中……</span></div>
    </div>
    <div style="text-align: center">
        <p class="tip">
            <lable style="font-weight: bolder">选择文件：</lable>
            1.只有打勾文件才会被同步；2.<lable style="color: #f0742e">橙色</lable>文件表示正在被其它同事同步；3.全选只选中展开的部分；4.<img
                src="Styles/img/edit.png" alt="变更图标" />表示文件有变化。
            <lable style="font-weight: bolder">输入列表：</lable>
            1.需要向管理员申请；2.使用相对路径名；3.以”#”开头的行是注释。
        </p>
        <table border="0" align="center" cellpadding="0" cellspacing="5">
            <tr>
                <td align="center" colspan="2">
                    <b>同步-选择文件</b>
                </td>
            </tr>
            <tr id="trdomain">
                <td align="center" valign="top" colspan="2">
                    环境：
                    <asp:DropDownList ID="DdlEnvironment" runat="server" AutoPostBack="True" OnSelectedIndexChanged="DdlEnvironment_SelectedIndexChanged">
                    </asp:DropDownList>
                    域名：
                    <asp:DropDownList ID="DdlDomains" runat="server" AutoPostBack="True" OnSelectedIndexChanged="DdlDomains_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:Button ID="BtnRefresh" TabIndex="0" OnClientClick="return Refresh();" runat="server"
                        Text="刷新" />
                    <asp:Button ID="BtnAdd" TabIndex="0" OnClientClick="return check();" runat="server"
                        Text="下一步" OnClick="BtnAdd_Click" />
                </td>
            </tr>
            <tr id="trFileInput" runat="server">
                <td align="right" valign="top">
                    <a href="#" id="aFileInput">输入列表</a>
                </td>
                <td align="left" valign="top">
                    <div id="divFileInput">
                        <asp:TextBox ID="TxtFileList" runat="server" TextMode="MultiLine" Width="500px" Height="150px"></asp:TextBox>
                    </div>
                </td>
            </tr>
            <tr id="trFileSelect" runat="server">
                <td align="right" valign="top">
                    <a href="#" id="aFileSelect">选择文件</a>
                </td>
                <td align="left" valign="top">
                    <div id="divFileSelect">
                        <div>
                            <asp:HiddenField ID="HfAddFiles" runat="server" />
                            <asp:HiddenField ID="HfDelFiles" runat="server" />
                            <input type="checkbox" id="chkAll" title="子路径中的文件需要展开后才能选择">全选 </input>
                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                            <input type="checkbox" id="cbxShowModifiedFiles" title="同步加载该项目所有被修改的文件" />仅显示修改的
                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                            <input type="checkbox" id="cbxModified" title="子路径中的文件需要展开后才能选择" />选中修改的
                            <ul id="tree" class="tree" style="overflow: auto;">
                            </ul>
                        </div>
                    </div>
                </td>
            </tr>
        </table>
        <div id="tip" class="tipMsg">
        </div>
    </div>
    <script type="text/javascript">
        function ShowHideDetail() {
            if ($('.historyditails').css("display") == "none") {
                $('.historyditails').css("display", "block");
            } else {
                $('.historyditails').css("display", "none");
            }
        }

        function ShowRecord() {
            var domain = $("#<%=DdlDomains.ClientID%>").find("option:selected").text();
            $('#lbDomainName').text(domain);
            $('#divSyncRecord').show();
            $('.historyditails').css("display", "none");
        }

        function HideRecord() {
            $('#divSyncRecord').hide();
        }
    </script>
    <div id="divSyncRecord" style="padding: 10px; display: block;">
        域名【<label id="lbDomainName"></label>】同步记录&nbsp;&nbsp;&nbsp;&nbsp;
        <input id="btnShow" type="button" value="显示/隐藏—任务详情" onclick="ShowHideDetail(); " /><br />
        <!--<table cellpadding='5' cellspacing='0' class='listTable'>
            <tr>
                <th align='center'>
                    任务编号
                </th>
                <th align='center'>
                    同步原因
                </th>
                <th align='center'>
                    同步结果
                </th>
                <th align='center'>
                    创建者
                </th>
                <th align='center'>
                    创建时间
                </th>
                <th align='center'  class='resourceShowHidden'>
                    任务详情
                </th>
            </tr>
            <tr><td>598</td><td>test2</td><td>同步成功</td><td>heyongdong</td><td>Wed Nov 09 2011 16:14:33 GMT+0800 (中国标准时间)</td><td class='resourceShowHidden'>添加文件:<br />a.txt<br />b.txt<br />c.dll<br />删除文件:<br /></td></tr>
            <tr><td>599</td><td>a1</td><td>同步成功</td><td>leibin</td><td>Wed Nov 09 2011 16:14:34 GMT+0800 (中国标准时间)</td><td class='resourceShowHidden'>添加文件:<br />b.txt<br />c.dll<br />删除文件:<br />e.txt</td></tr>
            <tr><td>658</td><td>55</td><td>同步失败</td><td>leibin</td><td>Fri Nov 11 2011 11:41:34 GMT+0800 (中国标准时间)</td><td class='resourceShowHidden'>添加文件:<br />a.txt<br />删除文件:<br /></td></tr>
            <tr><td>722</td><td>ll</td><td>已撤销</td><td>leibin</td><td>Wed Nov 16 2011 17:15:09 GMT+0800 (中国标准时间)</td><td class='resourceShowHidden'>添加文件:<br />x.dll<br />删除文件:<br />a.txt</td></tr>
            <tr><td>757</td><td>111</td><td>同步成功</td><td>leibin</td><td>Thu Nov 17 2011 09:06:28 GMT+0800 (中国标准时间)</td><td class='resourceShowHidden'>添加文件:<br />a.txt<br />c.dll<br />删除文件:<br /></td></tr>
            <tr><td>767</td><td>all</td><td>同步成功</td><td>leibin</td><td>Thu Nov 17 2011 09:44:55 GMT+0800 (中国标准时间)</td><td class='resourceShowHidden'>添加文件:<br />a.txt<br />b.txt<br />c.dll<br />删除文件:<br />f.txt</td></tr>
        </table>-->
        <div id="divversion">
        </div>
        <div id="divpager">
        </div>
    </div>
</asp:Content>