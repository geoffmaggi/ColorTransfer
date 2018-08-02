<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="Default.aspx.cs" Inherits="ColorTransfer.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <p><b>Source: </b><asp:FileUpload ID="sourceFU" runat="server" /></p>
      <p><b>Target: </b><asp:FileUpload ID="targetFU" runat="server" /></p>
      <p><b>Color Space:
      <asp:DropDownList ID="spaceDDL" runat="server">
        <asp:ListItem Text="lαβ" Value="1" />
        <asp:ListItem Text="CIE CAM97" Value="2" />
        <asp:ListItem Text="LMS" Value="3" />
        <asp:ListItem Text="RGB" Value="4" />
      </asp:DropDownList></b></p>
      <asp:Button ID="runBtn" text="Run!" OnClick="run" runat="server" />
      <asp:Button ID="clearBtn" text="Clear!" OnClick="clear" runat="server" />
      <hr />
      <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" ShowHeader="false">
        <Columns>
            <asp:BoundField DataField="Text" />
            <asp:ImageField DataImageUrlField="Value">
                <ItemStyle Width="250px" />
            </asp:ImageField>
        </Columns>
    </asp:GridView>
    </div>
    </form>
</body>
</html>
