<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CS.aspx.cs" Inherits="CS" %>
     

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        body
        {
            font-family: Arial;
            font-size: 10pt;
        }
        input
        {
            width: 200px;
        }
        table
        {
            border: 1px solid #ccc;
        }
        table th
        {
            background-color: #F7F7F7;
            color: #333;
            font-weight: bold;
        }
        table th, table td
        {
            padding: 5px;
            border-color: #ccc;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
<table border="0" cellpadding="0" cellspacing="0">
    <tr>
        <th colspan="3">
            Email til Kundan
        </th>
    </tr>
    <tr>
        <td>
            Kundanummar
        </td>
        <td>
            <asp:TextBox ID="txtCustomer" runat="server" />
        </td>
        <td>
            <asp:RequiredFieldValidator ErrorMessage="Required" ForeColor="Red" ControlToValidate="txtCustomer"
                runat="server" />
        </td>
    </tr>
    <tr>
        <td>
            Starvsfólk
        </td>
        <td>
            <asp:DropDownList ID="ddlEmployee" runat="server" AppendDataBoundItems="true">
                <asp:ListItem Text="<Select Subject>" Value="0" />
            </asp:DropDownList>              
        </td>
        <td>
            <asp:RequiredFieldValidator ErrorMessage="Required" ForeColor="Red" ControlToValidate="ddlEmployee"
                runat="server" />
        </td>
    </tr>
    <tr>
        <td>
            Email
        </td>
        <td>
            <asp:TextBox ID="txtEmail" runat="server" />
        </td>
        <td>
            <asp:RequiredFieldValidator ErrorMessage="Required" Display="Dynamic" ForeColor="Red"
                ControlToValidate="txtEmail" runat="server" />
            <asp:RegularExpressionValidator runat="server" Display="Dynamic" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                ControlToValidate="txtEmail" ForeColor="Red" ErrorMessage="Invalid email address." />
        </td>
    </tr>
     <tr>
        <td>
            Prísur
        </td>
        <td>
            <asp:TextBox ID="txtPrice" runat="server" />
        </td>
        <td>
            <asp:RequiredFieldValidator ErrorMessage="Required" ForeColor="Red" ControlToValidate="txtPrice"
                runat="server" />
        </td>
    </tr>
     <tr>
        <td>
            Variansur
        </td>
        <td>
            <asp:TextBox ID="txtVariance" runat="server" Text="375" />
        </td>
    </tr>
    <tr>
        <td>
        </td>
        <td>
            <asp:Button Text="Send Email" runat="server" OnClick="RegisterOrder" />
        </td>
        <td>
        </td>
    </tr>
</table>
<div>
<asp:PlaceHolder id="tbl_Orders" runat="server"></asp:PlaceHolder>
</div>
</form>    
</body>
</html>
