<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GenericFilter.ascx.cs"
    Inherits="ABCAPOS.GenericFilter" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<style type="text/css">
    .column1
    {
        width: 20px;
    }
    .column2
    {
        width: 120px;
    }
    .column3
    {
        width: 50px;
    }
    .column4
    {
        width: 300px;
    }
</style>
<script type="text/javascript">
    function check(chkSelectedClientID) {        
        $(chkSelectedClientID).attr("checked", true);
    }
</script>
<div style="border: 1px solid #000000; width: 800px; vertical-align:middle; margin-bottom:5px">
<div id="divCollapsible" runat="server" style="width: 100%; height: 25px; background: -webkit-gradient( linear, left top, left bottom, color-stop(0.05, #B0B0B0), color-stop(1, #7D7D7D) ); background: -moz-linear-gradient( center top, #B0B0B0 5%, #7D7D7D 100% );color: #FFFFFF; font-size: 16px; font-weight: bold; padding-bottom:4px;">
    <%--<div id="divCollapsible" runat="server" style="width: 100%; height: 25px; background-image: url('../../Styles/Images/filterBackground.png');
        color: #FFFFFF; font-size: 16px; font-weight: bold; padding-bottom:4px;">--%>
        <table style="width: 100%;">
            <tr>
                <td>
                    <%--<asp:LinkButton ID="lnkTitle" runat="server" Text="Filters" style="text-decoration:none; color:White" OnClientClick="return false;"></asp:LinkButton>--%>
                    <span style="color:White">Filters</span>
                </td>
                <td style="text-align:right">
                    <asp:Image ID="img1" runat="server" />
                </td>
            </tr>
        </table>
    </div>
    <asp:Panel ID="pnlFilter" runat="server" Style="padding-top: 0px; margin-top: 2px" DefaultButton="btnFilter">
        <asp:PlaceHolder ID="phFilterFields" runat="server"></asp:PlaceHolder>
        <table>
            <tr>
                <td class="column1">
                </td>
                <td class="column2">
                </td>
                <td>
                    <asp:Button ID="btnFilter" runat="server" Text="Cari" OnClick="btnFilter_Click" />
                    &nbsp;
                    <asp:Button ID="btnClear" runat="server" Text="Hapus Filter" OnClick="btnClear_Click" />
                    <asp:HiddenField ID="hdnOdsID" runat="server" />
                    <asp:Label ID="lblError" runat="server" CssClass="error" Visible="false"></asp:Label>
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:CollapsiblePanelExtender ID="CollapsiblePanelExtender1" runat="server" TargetControlID="pnlFilter"
        CollapseControlID="divCollapsible" ExpandControlID="divCollapsible" CollapsedImage="~/Styles/Images/expand_blue.jpg"
        ExpandedImage="~/Styles/Images/collapse_blue.jpg" ImageControlID="img1">
    </asp:CollapsiblePanelExtender>
</div>
