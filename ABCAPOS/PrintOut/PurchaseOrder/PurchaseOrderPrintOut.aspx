<%@ Page Title="" Language="C#" MasterPageFile="~/PrintOut/Site.Master" AutoEventWireup="true"
    CodeBehind="PurchaseOrderPrintOut.aspx.cs" Inherits="ABCAPOS.PrintOut.PurchaseOrder.PurchaseOrderPrintOut" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <rsweb:ReportViewer ID="ReportViewer1" runat="server" Width="100%" Height="900px"
        Font-Names="Verdana" Font-Size="8pt" InteractiveDeviceInfos="(Collection)" WaitMessageFont-Names="Verdana"
        WaitMessageFont-Size="14pt">
        <LocalReport ReportPath="PrintOut\PurchaseOrder\UserControls\PurchaseOrder.rdlc">
        </LocalReport>
    </rsweb:ReportViewer>
</asp:Content>
