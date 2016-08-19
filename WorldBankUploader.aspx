<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WorldBankUploader.aspx.cs" Inherits="WebApplication2.WorldBankUploader" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="WorldBank.css" rel="stylesheet" />
    <script src="Scripts/jquery-1.8.2.min.js"></script>
      <script type="text/javascript">
          $("[src*=plus]").live("click", function () {
              $(this).closest("tr").after("<tr><td></td><td colspan = '999'>" + $(this).next().html() + "</td></tr>")
              $(this).attr("src", "images/minus.png");
          });
          $("[src*=minus]").live("click", function () {
              $(this).attr("src", "images/plus.png");
              $(this).closest("tr").next().remove();
          });
          function FileExtentionsCheck() {
             
              console.log($("#flUpload"));
              return true;
             // return false;
              
          }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div>
                <h1>World Bank Data</h1>
                                <div class="top"></div>

                <div id="InpuControl" style="display: inline">
                    <div>
                        <h3>
                            Upload and Display Records
                        </h3>
                    <div style="float: left; padding: 10px">
                        <asp:FileUpload runat="server" ID="flUpload" Width="300px" />
                    </div>
                    <div style="float: left; padding: 10px">


                        <asp:Button runat="server" Text="Upload" ID="btnUpload" OnClick="btnUpload_Click" OnClientClick="return FileExtentionsCheck();" />
                    </div>
                    <div style="float: left; padding: 10px">


                        <asp:Button runat="server" Text="Display all Records" ID="btnDisplay"  OnClick="btnDisplay_Click" />
                    </div>
                    </div>



                    <br />
                    <br />
                    <br />
                </div>

                <div>
                    <asp:GridView ID="GridIndicator" cssClass="Grid" runat="server" AutoGenerateColumns="false" DataKeyNames="IndiacatorId" AllowPaging="true" PageSize="20"
                        OnRowDataBound="GridIndicator_OnRowDataBound" HeaderStyle-BackColor="#A52A2A" HeaderStyle-ForeColor="White" OnPageIndexChanging="OnPaging">
                        <Columns>
                            <asp:TemplateField ItemStyle-Width="20px">
                                <ItemTemplate>
                                   <img alt = "" style="cursor: pointer" src="images/plus.png" />
                                    <asp:Panel ID="pnlOrders" runat="server" Style="display: none">
                                        <asp:GridView ID="GridView2" runat="server" AutoGenerateColumns="false" 
                                            HeaderStyle-BackColor="#FFA500" HeaderStyle-ForeColor="White">
                                            <Columns>
                                                <asp:BoundField ItemStyle-Width="150px" DataField="IndicatedYear" HeaderText="Indicated Year" />
                                                <asp:BoundField ItemStyle-Width="100px" DataField="Value" HeaderText="Value" />
                                               
                                            </Columns>
                                        </asp:GridView>
                                    </asp:Panel>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField ItemStyle-Width="300px" DataField="Name" HeaderText="Indiacator Name" />
                            <asp:BoundField ItemStyle-Width="200px" DataField="Code" HeaderText="Indiacator Code" />
                        </Columns>
                    </asp:GridView>

                </div>

            </div>

        </div>
    </form>
</body>
</html>
