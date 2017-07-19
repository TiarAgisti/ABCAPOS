<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GenericFilterField.ascx.cs"
    Inherits="ABCAPOS.GenericFilterField" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<style type="text/css">
    .column1
    {
        width:20px;
    }
    .column2
    {    
        width:120px;
    }
    .column3
    {
        width:50px;
        display:none;
    }
    .column4
    {
        width:300px;
    }
    
</style>

<table>
    <tr>
        <td class="column1">
            <asp:CheckBox ID="chkSelected" runat="server" />
        </td>
        <td class="column2">
            <asp:HiddenField ID="hdnField" runat="server" />
            <asp:HiddenField ID="hdnFieldType" runat="server" />
            <asp:HiddenField ID="hdnFieldDataType" runat="server" />
            <asp:Label ID="lblFieldText" runat="server"></asp:Label>
        </td>
        <td class="column3">
            <asp:DropDownList ID="ddlOperator" runat="server">
                <asp:ListItem Text="=" Value="Equal">
                </asp:ListItem>
                <asp:ListItem Text="<" Value="LessThan">
                </asp:ListItem>
                <asp:ListItem Text="<=" Value="LessThanEqual">
                </asp:ListItem>
                <asp:ListItem Text=">" Value="MoreThan">
                </asp:ListItem>
                <asp:ListItem Text=">=" Value="MoreThanEqual">
                </asp:ListItem>
                <asp:ListItem Text="<>" Value="NotEqual">
                </asp:ListItem>
                <asp:ListItem Text="Like" Value="Like">
                </asp:ListItem>
            </asp:DropDownList>
        </td>
        <td class="column4">
            <asp:TextBox ID="txtText" runat="server" Width="250px"></asp:TextBox>
            <div id="divDate" runat="server" visible="false">
                <asp:TextBox ID="txtDate" runat="server" Width="100px"></asp:TextBox>
                <asp:ImageButton ID="btnDate" runat="server" ImageUrl="~/Styles/Images/calendar.png" CausesValidation="false" />
                <asp:CalendarExtender ID="calDate" runat="server" PopupButtonID="btnDate" TargetControlID="txtDate">
                </asp:CalendarExtender>               
            </div>
             <div id="divDateRange" runat="server" visible="false">                
                <asp:TextBox ID="txtDateFrom" runat="server" Width="100px"></asp:TextBox>
                <asp:ImageButton ID="btnDateFrom" runat="server" ImageUrl="~/Styles/Images/calendar.png" CausesValidation="false" />
                <asp:CalendarExtender ID="calDateFrom" runat="server" PopupButtonID="btnDateFrom" TargetControlID="txtDateFrom">
                </asp:CalendarExtender>        
                &nbsp;s/d&nbsp;
                <asp:TextBox ID="txtDateTo" runat="server" Width="100px"></asp:TextBox>
                <asp:ImageButton ID="btnDateTo" runat="server" ImageUrl="~/Styles/Images/calendar.png" CausesValidation="false" />
                <asp:CalendarExtender ID="calDateTo" runat="server" PopupButtonID="btnDateTo" TargetControlID="txtDateTo">
                </asp:CalendarExtender>                 
            </div>
            <div id="divIntegerRange" runat="server" visible="false">
                <asp:TextBox ID="txtIntegerFrom" runat="server" class="integerNumeric" Width="50px"></asp:TextBox>
                &nbsp;s/d&nbsp;
                <asp:TextBox ID="txtIntegerTo" runat="server" class="integerNumeric" Width="50px"></asp:TextBox>
            </div>
             <asp:DropDownList ID="ddlList" runat="server" Visible="false">
             </asp:DropDownList>
        </td>
    </tr>
</table>
