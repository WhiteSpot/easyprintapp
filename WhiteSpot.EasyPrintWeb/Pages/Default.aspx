<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WhiteSpot.EasyPrintWeb.Pages.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Easy Print</title>
	<script src="../Scripts/jquery-1.8.2.min.js" type="text/javascript"></script>
	<script src="../Scripts/jquery-bbq.min.js" type="text/javascript"></script>
	<script type="text/javascript">
		"use strict";

		var hostweburl;

		// Load the SharePoint resources.
		$(document).ready(function () {
			var params = $.deparam.querystring();

			hostweburl = decodeURIComponent(params.SPHostUrl);

			// The SharePoint js files URL are in the form: web_url/_layouts/15/resource.js
			var scriptbase = hostweburl + "/_layouts/15/";

			// Load the js file and continue to the success handler.
			$.getScript(scriptbase + "SP.UI.Controls.js")
		});
	</script>
</head>
<body>
	<!-- Chrome control placeholder; options are declared inline.  -->
    <div
        id="chrome_ctrl_container"
        data-ms-control="SP.UI.Controls.Navigation"
        data-ms-options='{  
                "appIconUrl" : "../styles/img/AppIcon.png",
                "appTitle" : "Easy Print"
             }'>
    </div>

    <!-- The chrome control also makes the SharePoint style sheet available to your page. -->
    <h2 class="ms-accentText">White Spot - Easy Print</h2>
    <div id="MainContent">
        This is where we can put a basic description about the app, contact details, etc.
    </div>
</body>
</html>
