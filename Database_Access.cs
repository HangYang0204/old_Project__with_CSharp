using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace IADMarketingOffers
{
    class sxmDB
    {
        #region sxmDB.Variables

        private SqlConnection _con;
        private SqlCommand _com;
        private String _constr;
        private SqlTransaction _tran;

        public string dbmsg;
        public StringBuilder sbXML;

        public SqlDataReader sqlDR;
        public DataSet sxmDS;
        public IDataReader iDR;
        //private SqlBulkCopy sqlBULK;
        private SqlDataAdapter sda;

        public DataTable DT; 

        #endregion

        public sxmDB(string connectionstring)
        {
            _con = new SqlConnection();
            _com = new SqlCommand();

            _constr = connectionstring;

            _con.ConnectionString = _constr;
            //_com.Connection = _con;
        }
        public void Connect()
        {
            if (_con.State != ConnectionState.Open)
                try
                {
                    _con.Open();
                }
                catch (SqlException sqlexc)
                {
                    dbmsg += sqlexc.Message;
                }
                catch (Exception exc)
                {
                    dbmsg += exc.Message;
                }
        }
        public void DisConnect()
        {
            try
            {
                _con.Close();
            }
            catch (SqlException sqlexc)
            {
                dbmsg += sqlexc.Message;
            }
            catch (Exception exc)
            {
                dbmsg += exc.Message;
            }
        }

        public DataTable[] GetSqlDataTables(string SPName)
        {
            DataTable[] dbtables = null;

            this.Connect();
            _com.CommandType = CommandType.StoredProcedure;
            _com.CommandTimeout = 0;
            _com.CommandText = SPName;
            _com.Connection = _con;

            sda = new SqlDataAdapter(_com);
            sxmDS = new DataSet();
            try
            {
                sda.Fill(sxmDS);
                if (sxmDS != null)
                {
                    DataTable[] tbls = new DataTable[sxmDS.Tables.Count];
                    dbtables = tbls;
                    tbls = null;
                    for (int i = 0; i < sxmDS.Tables.Count; i++)
                    {

                        dbtables[i] = new DataTable();
                        sxmDS.Tables[i].TableName = "Table" + i.ToString();
                        dbtables[i] = sxmDS.Tables[i];

                    }

                }
            }
            catch (Exception ex)
            {
                this.dbmsg += ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                DisConnect();
            }

            return dbtables;

        }
        public DataTable GetSqlDataTable(string SPName)
        {
            DataTable DT = null;

            this.Connect();
            _com.CommandType = CommandType.StoredProcedure;
            _com.CommandTimeout = 0;
            _com.CommandText = SPName;
            _com.Connection = _con;

            sda = new SqlDataAdapter(_com);
            sxmDS = new DataSet();
            try
            {
                sda.Fill(sxmDS);
                if (sxmDS != null)
                {
                    DT = sxmDS.Tables[0];
                }
            }
            catch (Exception ex)
            {
                this.dbmsg += ex.Message + Environment.NewLine + ex.StackTrace;
            }
            finally
            {
                DisConnect();
            }

            return DT;

        }
        public IDataReader GetSqlData(string querystring)
        {
            this.Connect();

            _com.Parameters.Clear();
            _com.CommandText = querystring;
            _com.Connection = _con;
            try
            {
                iDR = _com.ExecuteReader();
            }
            catch (SqlException sE)
            {
                dbmsg += sE.Message;
            }
            return iDR;
        }
        public String GetXmlRequests(string SPName)
        {
            String dbXml = string.Empty;
            DataTable DT = new DataTable();
            this.Connect();
            _com.CommandType = CommandType.StoredProcedure;
            _com.CommandTimeout = 0;
            _com.CommandText = SPName;
            _com.Connection = _con;

            _com.Parameters.Clear();
            _com.Parameters.AddWithValue("@RunDate", DateTime.Now.Date);


            SqlDataAdapter DA = new SqlDataAdapter(_com);
            DataSet DS = new DataSet();

            try
            {
                DA.Fill(DS);
                DT = DS.Tables[0];
                //dbXml = DT.Rows[0][0].ToString(); 
                int CheckRows = DT.Rows.Count;

                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    dbXml = dbXml + DT.Rows[i][0].ToString();
                }

            }
            catch (SqlException sE)
            {
                dbmsg += sE.Message;
            }
            finally
            {
                this.DisConnect();
            }


            return dbXml;
        }
        public DataTable GetTable(string query)
        {
            this.Connect();
            _com.Connection = _con;
            _com.CommandType = CommandType.Text;
            _com.CommandText = query;

            DataTable DT = this.GetDataTableGeneric(_com);
            return DT;

        }
        private DataTable GetDataTableGeneric(SqlCommand command)
        {
            DataTable DT = new DataTable();
            this.Connect();

            SqlDataAdapter DA = new SqlDataAdapter(command);
            DataSet DS = new DataSet();

            try
            {
                DA.Fill(DS);
                DT = DS.Tables[0];
            }
            catch (SqlException sE)
            {
                dbmsg += sE.Message;
            }
            finally
            {
                this.DisConnect();
            }

            return DT;
        }
        
        // DML functions
        public bool ModifyData(string DeleteQueryString)
        {
            bool bResult = false;
            int iResult = 0;
            SqlTransaction _tn;
            _com.CommandText = DeleteQueryString;
            this.Connect();
            _tn = _con.BeginTransaction();
            _com.Transaction = _tn;

            try
            {
                iResult = _com.ExecuteNonQuery();
                if (iResult > 0)
                {
                    bResult = true;
                    _tn.Commit();
                }
                else
                {
                    bResult = false;
                    _tn.Rollback();
                }
            }
            catch (SqlException sqlEx)
            {
                _tn.Rollback();
                dbmsg += sqlEx.Message;
            }
            finally
            {
                this.DisConnect();
            }
            return bResult;

        }
        public bool ModifyDataWithParams(SqlCommand sqlcomm)
        {
            bool bResult = false;
            int iResult = 0;
            SqlTransaction _tn;
            _com.Parameters.Clear();
            _com = sqlcomm;
            this.Connect();
            _tn = _con.BeginTransaction();
            _com.Transaction = _tn;

            try
            {
                iResult = _com.ExecuteNonQuery();
                if (iResult > 0)
                {
                    bResult = true;
                    _tn.Commit();
                }
                else
                {
                    bResult = false;
                    _tn.Rollback();
                }
            }
            catch (SqlException sqlEx)
            {
                _tn.Rollback();
                dbmsg += sqlEx.Message;
            }
            finally
            {
                this.DisConnect();
            }
            return bResult;
        }

        // Troubleshooting methods
        public void CheckConnectToSQLServer()
        {
            try
            {
                this.Connect();
                dbmsg += "Connection is " + this._con.State.ToString() + ". ";
            }
            catch (SqlException sqlexc)
            {
                dbmsg += sqlexc.Message;
            }
            catch (Exception exc)
            {
                dbmsg += exc.Message;

            }
            finally
            {
                this.DisConnect();
            }
        }
        public SqlConnection ConnectToSqlTarget()
        {
            return _con;
        }

        // Helpers
        public bool SaveFileLinesIntoDataTable(List<string> fLines)
        {
            bool done = true;

            // Initialize DT
            DT = new DataTable();

            // Declare DataColumn and DataRow variables.
            DataColumn column;
            DataRow row;
            DataView view;

            // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int64");
            column.ColumnName = "account";
            DT.Columns.Add(column);

            // Create second column.
            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "promo";
            DT.Columns.Add(column);

            // i = 1, skips header line at position 0.
            for (int i = 1; i < fLines.Count; i++)
            {
                string oneLine = fLines[i];

                // Ignoring blank lines 
                if (fLines[i].Trim() != "")
                {
                    Int64 iAccount = 0;
                    string[] values;
                    string sAccount = string.Empty;
                    string promo = string.Empty;

                    try
                    {
                        values = fLines[i].Split('|');
                        sAccount = values[0];
                        promo = values[1];
                        iAccount = Int64.Parse(sAccount);

                        row = DT.NewRow();

                        row["account"] = iAccount;
                        row["promo"] = promo;

                        DT.Rows.Add(row);

                    }
                    catch (Exception e)
                    {
                        done = false;
                        dbmsg = "ERROR at file line - " + i.ToString() + ". \n";
                        dbmsg += e.Message;
                    } //End.Of.Try

                } // End.Of.If

            } //End.Of.For

            if (done == true)
            {
                dbmsg += DT.Rows.Count.ToString("N0") + " records prepared to be saved into database, List to DT.\n";
            }
            return done;
        }

        public void SaveFilesRecordsIntoDB(String SPName)
        {
            this.Connect();
            _com.Connection = _con;
            _com.CommandType = CommandType.StoredProcedure;
            _com.CommandText = SPName;

            //Infinite, the Command will wait infinitely until query will be completed.
            _com.CommandTimeout = 0;

            _com.Parameters.Clear();
            SqlParameter paramTable = _com.Parameters.AddWithValue("@RecordsTable", this.DT);
            paramTable.SqlDbType = SqlDbType.Structured;

            this.dbmsg = string.Empty; 
            try
            {
                using (SqlDataReader reader = _com.ExecuteReader())
                {
                    // Gets the logging messages of stored procedure
                    while (reader.Read())
                    {
                        dbmsg += "\t\t" + reader[0].ToString() + "\n";
                    }
                }
            }
            catch (Exception ex)
            {
                dbmsg += "*** Exception in SaveFileRecordsIntoDB: " + ex.Message;
                dbmsg += ex.StackTrace;
            }
            finally
            {
                DisConnect();
            }
           
        }




    } // End.Of.Class  

} // End.Of.Namespace 
