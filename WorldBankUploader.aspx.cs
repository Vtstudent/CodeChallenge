using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Text;
using System.Linq.Expressions;
using MySql.Data.MySqlClient;
using System.Windows;


namespace WebApplication2
{
    public partial class WorldBankUploader : System.Web.UI.Page
    {
        private string sqlIndicatorScript = "SELECT IndiacatorId,Name,Code FROM Indicator";
        // private string sqlIndicatorIfScript = "if( (select count(IndiacatorId) from Indicator where Code ='{0}') =0) begin";
        private string sqlIndicatorInsertScript = "call InsertIndicator('{0}','{1}'); ";
        private string sqlIndicatorYearScript = " INSERT INTO IndicatorYearValues(IndicatorYearId,IndicatedYear,Value) values('{0}','{1}','{2}');  ";

        string sqlscriptExecuteYEarValues = " select Distinct Indicator.IndiacatorId ,Indicator.Name,Indicator.Code from IndicatorYearValues inner join Indicator on IndicatorYearValues.IndicatorYearId=Indicator.IndiacatorId  ";

        // private string sqlIndicatorScript = "SELECT IndiacatorId [Id],[Indicator Name],[Indicator Code] FROM Indicator";
        #region privatevariables

        DataTable dtMasterData = new DataTable();
        #endregion
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        #region buttonEvents

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            DataTable dtInput = GetDataFromCSVFile("", true);


            // Step1 : to Insert Master Data
            //  dtMasterData = dtInput.DefaultView.ToTable(true, "Indicator Name", "Indicator Code"); // Get all distinct Records
            StringBuilder buildScript = new StringBuilder();
            int counter = 0;
            foreach (DataRow rowItem in dtInput.Rows)
            {
                if (rowItem[1] != null && rowItem[1].ToString() != string.Empty)
                {
                    // buildScript.AppendLine(string.Format(sqlIndicatorIfScript, rowItem[1].ToString().Replace("'", "''")));

                    buildScript.AppendLine(string.Format(sqlIndicatorInsertScript, rowItem[0].ToString().Replace("'", "''"), rowItem[1].ToString().Replace("'", "''")));
                }
                counter++;
                if (counter == 300)
                {
                    if (buildScript.ToString().Length > 0)
                    {
                        InsertData(buildScript.ToString(), true);
                        buildScript.Clear();
                        counter = 0;
                    }

                }
            }

            // buildScript.Clear existing db script becuase script has been exected
            buildScript.Clear();
            // Step2 : Insert Transactional Data
            int indicatorId = 0;
            var tranCounter = 0;
            foreach (DataRow item in dtInput.DefaultView.ToTable(true).Rows)
            {
                if (!string.IsNullOrEmpty(item[1].ToString()))
                {
                    foreach (DataColumn column in dtInput.Columns)
                    {
                        if (!string.IsNullOrEmpty(column.ColumnName) && column.ColumnName.ToLower() != "indicator name" && column.ColumnName.ToLower() != "indicator code")
                        {

                            DataRow[] drMaster = dtMasterData.Select(" Code = '" + item[1].ToString() + "'");
                            if (drMaster.Length > 0)
                                indicatorId = Convert.ToInt32(drMaster[0]["IndiacatorId"]);
                            buildScript.AppendLine(string.Format(sqlIndicatorYearScript, indicatorId, column.ColumnName.Replace("'", "''"), item[column.ColumnName].ToString().Replace("'", "''")));
                        }
                        tranCounter++;
                        if (tranCounter == 300 && buildScript.ToString().Length > 0)
                        {
                            InsertData(buildScript.ToString(), false);
                            buildScript.Clear();
                            tranCounter = 0;
                        }
                    }
                    string myStringVariable = "File has been uploaded suscessfully";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + myStringVariable + "');", true);

                }


            }


        }
        
        protected void btnDisplay_Click(object sender, EventArgs e)
        {


            DataTable dtIndiactor = GetTransData(sqlscriptExecuteYEarValues);
            Session["Data"] = dtIndiactor;
            GridIndicator.DataSource = dtIndiactor;
            GridIndicator.DataBind();
        }
        #endregion

        #region GridEvents

        protected void OnPaging(object sender, GridViewPageEventArgs e)
        {
            GridIndicator.PageIndex = e.NewPageIndex;
            GridIndicator.DataSource = Session["Data"];
            GridIndicator.DataBind();
        }

        DataTable dtTotalData;
        protected void GridIndicator_OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string indicatorId = GridIndicator.DataKeys[e.Row.RowIndex].Value.ToString();
                string sqlscript = " select IndiacatorId ,Name,Code,IndicatedYear,Value from IndicatorYearValues inner join Indicator ";
                sqlscript = sqlscript + " on IndicatorYearValues.IndicatorYearId=Indicator.IndiacatorId ";
                if (dtTotalData == null)
                {
                    dtTotalData = GetTransData(sqlscript);
                    GridView innerGrid = (GridView)e.Row.FindControl("GridView2");
                    innerGrid.DataSource = dtTotalData.Select(" IndiacatorId= " + indicatorId).CopyToDataTable();
                    innerGrid.DataBind();
                }
                else
                {
                    GridView innerGrid = (GridView)e.Row.FindControl("GridView2");
                    innerGrid.DataSource = dtTotalData.Select(" IndiacatorId= " + indicatorId).CopyToDataTable();
                    innerGrid.DataBind();
                }

            }
        }
        #endregion

        #region privateMethods
        DataTable GetDataFromCSVFile(string path, bool isFirstRowHeader)
        {
            // used to check whether the file has uploaded
            if (flUpload.HasFile)
            {
                
                flUpload.SaveAs(Server.MapPath("~/") + flUpload.FileName);
                string header = isFirstRowHeader ? "Yes" : "No";

                string pathOnly = Path.GetDirectoryName(Server.MapPath("~/") + flUpload.FileName);
                string fileName = Path.GetFileName(flUpload.FileName);

                string sql = @"SELECT * FROM [" + fileName + "]";

                // using class required to dispose() the object once after its operation
                using (OleDbConnection connection = new OleDbConnection(
                          @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly +
                          ";Extended Properties=\"Text;HDR=" + header + "\""))
                using (OleDbCommand command = new OleDbCommand(sql, connection))
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }

            return null;
        }

        public string conString
        {
            get
            {
                return System.Configuration.ConfigurationManager.ConnectionStrings["worldbank"].ToString();
            }
        }

        /// <summary>
        ///  insert Master Data
        /// </summary>
        /// <param name="sqlScript"> db execution script like insert statement with select query to check the Indicator code exists or not </param>
        private void InsertData(String sqlScript, Boolean isGetData)
        {
            using (MySqlConnection con = new MySqlConnection(conString))
            {
                MySqlCommand cmd = new MySqlCommand(sqlScript, con);
                cmd.CommandType = CommandType.Text;
                con.Open();

                cmd.ExecuteNonQuery();
                if (isGetData)
                {
                    MySqlCommand cmd1 = new MySqlCommand(sqlIndicatorScript, con);

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd1);

                    da.Fill(dtMasterData);
                }
            }
        }
        private DataTable GetTransData(string sqlscript)
        {
            DataTable getData = new DataTable();
            using (MySqlConnection con = new MySqlConnection(conString))
            {
                MySqlCommand cmd = new MySqlCommand(sqlscript, con);
                con.Open();

                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                da.Fill(getData);

            }
            return getData;
        }

        #endregion


        
    }
}