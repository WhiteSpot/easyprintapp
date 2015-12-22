<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EasyPrintWebPart.aspx.cs" Inherits="WhiteSpot.EasyPrintWeb.Pages.EasyPrintWebPart" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Easy Print</title>
    <script type="text/javascript" src="../Scripts/jquery-1.8.2.min.js"></script>
    <script type="text/javascript" src="../Scripts/jquery-bbq.min.js"></script>
    <script type="text/javascript" src="../Scripts/App.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="app-container">
            <div id="ResultMessage">
                <asp:Literal ID="MessageLiteral" Visible="false" runat="server" />
            </div>
            <asp:Panel ID="CheckBoxes" CssClass="hcf-form" runat="server" />
            <div class="hcf-form-submit">
                <asp:Button runat="server" ID="PrintAllButton" Text="Print All" Enabled="false" OnClick="PrintAllButton_Click" />
                <span id="ProgressMessage">
                    Processing the selected documents ...
                </span>
            </div>
        </div>
    </form>
</body>
</html>
