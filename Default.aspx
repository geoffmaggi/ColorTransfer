<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="Default.aspx.cs" Inherits="ColorTransfer.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
		<title></title>
</head>
<body>
		<form id="form1" runat="server">
		<div>
				<p>
						<b>Source: </b>
						<asp:FileUpload ID="sourceFU" runat="server" /></p>
				<p>
						<b>Target: </b>
						<asp:FileUpload ID="targetFU" runat="server" /></p>
				<p>
						<b>Color Space:
								<asp:DropDownList ID="spaceDDL" runat="server">
										<asp:ListItem Text="lαβ" Value="1" />
										<asp:ListItem Text="CIE CAM97" Value="2" />
										<asp:ListItem Text="LMS" Value="3" />
										<asp:ListItem Text="RGB" Value="4" />
								</asp:DropDownList>
						</b>
				</p>
				<asp:ScriptManager ID="ScriptManager1" runat="server" />
				<asp:UpdatePanel runat="server" ID="imageUP">
						<Triggers>
								<asp:PostBackTrigger ControlID="runBtn" />
								<asp:PostBackTrigger ControlID="clearBtn" />
						</Triggers>
						<ContentTemplate>
								<asp:Button ID="runBtn" Text="Run!" OnClick="run" runat="server" />
								<asp:Button ID="clearBtn" Text="Clear!" OnClick="clear" runat="server" />
								<hr />
								<asp:Label ID="debug" runat="server" />
						</ContentTemplate>
				</asp:UpdatePanel>
		</div>
		</form>
</body>
</html>
