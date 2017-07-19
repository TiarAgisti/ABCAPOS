﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Report/Site.Master" AutoEventWireup="true"
    CodeBehind="DeliveryOrderDetailReport.aspx.cs" Inherits="ABCAPOS.Report.DeliveryOrder.DeliveryOrderDetailReport" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<%@ Register Src="~/GenericFilter.ascx" TagName="GenericFilter" TagPrefix="uc1" %>
<%@ Register Src="~/GenericFilterField.ascx" TagName="GenericFilterField" TagPrefix="uc2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <uc1:GenericFilter ID="GenericFilter1" runat="server" OnFilter="GenericFilter1_Filter">
        <FilterFields>
            <uc2:GenericFilterField ID="gffDate" runat="server" FieldName="Date" FieldText="Tanggal Transaksi"
                Type="DateRange" Selected="true" />
            <uc2:GenericFilterField ID="gffCode" runat="server" FieldName="Code" FieldText="Kode Delivery Order"
                DataType="String" Operator="Like" />
            <uc2:GenericFilterField ID="gffCustomerName" runat="server" FieldName="CustomerName"
                FieldText="Nama Pelanggan" DataType="String" Operator="Like" />
        </FilterFields>
    </uc1:GenericFilter>
    <rsweb:ReportViewer ID="ReportViewer1" runat="server" Width="100%" 
        Height="800px" Font-Names="Verdana" Font-Size="8pt" 
        InteractiveDeviceInfos="(Collection)" WaitMessageFont-Names="Verdana" 
        WaitMessageFont-Size="14pt">
        <LocalReport ReportPath="Report\DeliveryOrder\UserControls\DeliveryOrderDetail.rdlc">
        </LocalReport>
    </rsweb:ReportViewer>
</asp:Content>
