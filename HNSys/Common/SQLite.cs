using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Data;

namespace HNSys
{
    public class SQLite
    {

        public static SQLiteConnection dbAlarmStopConn;
        public static SQLiteCommand dbAlarmStopCmd;

        public static SQLiteConnection dbAlarmTipConn;
        public static SQLiteCommand dbAlarmTipCmd;

        public static SQLiteConnection dbParameterConn;
        public static SQLiteCommand dbParameterCmd;

        public static SQLiteConnection dbFormulaConn;
        public static SQLiteCommand dbFormulaCmd;

        public static SQLiteConnection dbCurvecsConn;
        public static SQLiteCommand dbCurvecsCmd;

        public static SQLiteConnection dbCurvecsConn1;
        public static SQLiteCommand dbCurvecsCmd1;

        public static SQLiteConnection dbOverRollConn;
        public static SQLiteCommand dbOverRollCmd;


        #region 数据库初始化
        public static void SQLiteInit()
        {
            dbAlarmStopConn = new SQLiteConnection();
            dbAlarmStopConn.ConnectionString = "Data Source =" + Environment.CurrentDirectory + "/SQLite/TuBuAlarmStop";


            dbAlarmTipConn = new SQLiteConnection();
            dbAlarmTipConn.ConnectionString = "Data Source =" + Environment.CurrentDirectory + "/SQLite/TuBuAlarmTip";


            dbParameterConn = new SQLiteConnection();
            dbParameterConn.ConnectionString = "Data Source =" + Environment.CurrentDirectory + "/SQLite/TuBuParam1";

            dbFormulaConn = new SQLiteConnection();
            dbFormulaConn.ConnectionString = "Data Source =" + Environment.CurrentDirectory + "/SQLite/TuBuFrmula";

            dbCurvecsConn = new SQLiteConnection();
            dbCurvecsConn.ConnectionString = "Data Source =" + Environment.CurrentDirectory + "/SQLite/TuBuCurvecs";

            dbCurvecsConn1 = new SQLiteConnection();
            dbCurvecsConn1.ConnectionString = "Data Source =" + Environment.CurrentDirectory + "/SQLite/TuBuCurvecs1";




            dbOverRollConn = new SQLiteConnection();
            dbOverRollConn.ConnectionString = "Data Source =" + Environment.CurrentDirectory + "/SQLite/TuBuOverRoll";

            try
            {
              
                dbAlarmStopConn.Open();

                dbAlarmTipConn.Open();

                dbParameterConn.Open();
             
                dbFormulaConn.Open();
            
                dbCurvecsConn.Open();

                dbCurvecsConn1.Open();

                dbOverRollConn.Open();
            }
            catch(Exception EX)
            {
              //  MessageBox.Show(EX.Message);
            
            }
        }
        #endregion

        #region 停机报警插入读取

        public static void AlarmStopInsert(string insertTime,string alarmDescribe,bool alarmState, string alarmID)
        {
            if (dbAlarmStopConn.State == System.Data.ConnectionState.Closed)
            {
                dbAlarmStopConn.Open();
            }
            dbAlarmStopCmd = new SQLiteCommand();
            dbAlarmStopCmd.Connection = dbAlarmStopConn;
            dbAlarmStopCmd.CommandText = "Insert into Alarm(AlarmTime,AlarmInfo,AlarmState,AlarmID) ";
            dbAlarmStopCmd.CommandText = dbAlarmStopCmd.CommandText + "values('" + insertTime + "', '" + alarmDescribe + "', '" + alarmState.ToString() + "', '" + alarmID + "')";
            dbAlarmStopCmd.ExecuteNonQuery();
           // dbAlarmCmd.Dispose();
        }

        public static DataSet AlarmStopGetDataSet(string sql)
        {
            if (dbAlarmStopConn.State == System.Data.ConnectionState.Closed)
            {
                dbAlarmStopConn.Open();
            }

            dbAlarmStopCmd = new SQLiteCommand();
            dbAlarmStopCmd.Connection = dbAlarmStopConn;
            dbAlarmStopCmd.CommandText =sql;
            SQLiteDataAdapter da = new SQLiteDataAdapter(dbAlarmStopCmd);
            DataSet ds = new DataSet();
            try
            {
              
                da.Fill(ds);
                return ds;
            }
            catch (Exception EX)
            {
              //  MessageBox.Show(EX.Message);
                return null;
            }
            finally
            {
               // dbAlarmConn.Close();
            }
        }

        public static void AlarmStopDelate(string time)
        {
            if (dbAlarmStopConn.State == System.Data.ConnectionState.Closed)
            {
                dbAlarmStopConn.Open();
            }

            dbAlarmStopCmd = new SQLiteCommand();
            dbAlarmStopCmd.Connection = dbAlarmStopConn;
            dbAlarmStopCmd.CommandText = "delete  from Alarm where AlarmTime < " + "'" + time + "'";
            dbAlarmStopCmd.ExecuteScalar();

            // dbCurvecsCmd.Dispose();
        }

        #endregion

        #region 提示报警插入读取

        public static void AlarmTipInsert(string insertTime, string alarmDescribe, bool alarmState, string alarmID)
        {
            if (dbAlarmTipConn.State == System.Data.ConnectionState.Closed)
            {
                dbAlarmTipConn.Open();
            }
            dbAlarmTipCmd = new SQLiteCommand();
            dbAlarmTipCmd.Connection = dbAlarmTipConn;
            dbAlarmTipCmd.CommandText = "Insert into Alarm(AlarmTime,AlarmInfo,AlarmState,AlarmID) ";
            dbAlarmTipCmd.CommandText = dbAlarmTipCmd.CommandText + "values('" + insertTime + "', '" + alarmDescribe + "', '" + alarmState.ToString() + "', '" + alarmID + "')";
            dbAlarmTipCmd.ExecuteNonQuery();
            // dbAlarmCmd.Dispose();
        }

        public static DataSet AlarmTipGetDataSet(string sql)
        {
            if (dbAlarmTipConn.State == System.Data.ConnectionState.Closed)
            {
                dbAlarmTipConn.Open();
            }

            dbAlarmTipCmd = new SQLiteCommand();
            dbAlarmTipCmd.Connection = dbAlarmTipConn;
            dbAlarmTipCmd.CommandText = sql;
            SQLiteDataAdapter da = new SQLiteDataAdapter(dbAlarmTipCmd);
            DataSet ds = new DataSet();
            try
            {

                da.Fill(ds);
                return ds;
            }
            catch (Exception EX)
            {
               /// MessageBox.Show(EX.Message);
                return null;
            }
            finally
            {
                // dbAlarmConn.Close();
            }
        }

        public static void AlarmTipDelate(string time)
        {
            if (dbAlarmTipConn.State == System.Data.ConnectionState.Closed)
            {
                dbAlarmTipConn.Open();
            }

            dbAlarmTipCmd = new SQLiteCommand();
            dbAlarmTipCmd.Connection = dbAlarmTipConn;
            dbAlarmTipCmd.CommandText = "delete  from Alarm where AlarmTime < " + "'" + time + "'";
            dbAlarmTipCmd.ExecuteScalar();

            // dbCurvecsCmd.Dispose();
        }

        #endregion

        #region 参数插入读取

        public static void ParameterInsert(string sql)
        {
            if (dbParameterConn.State == System.Data.ConnectionState.Closed)
            {
                dbParameterConn.Open();
            }

           dbParameterCmd = new SQLiteCommand();
           dbParameterCmd.Connection = dbParameterConn;
           dbParameterCmd.CommandText = sql;
           
           dbParameterCmd.ExecuteNonQuery();
           dbParameterCmd.Dispose();
        }

        public static DataSet ParameterGetDataSet(string sql)
        {
            if (dbParameterConn.State == System.Data.ConnectionState.Closed)
            {
                dbParameterConn.Open();
            }

            dbParameterCmd = new SQLiteCommand();
            dbParameterCmd.Connection = dbParameterConn;
            dbParameterCmd.CommandText = sql;
            SQLiteDataAdapter da = new SQLiteDataAdapter(dbParameterCmd);
            DataSet ds = new DataSet();
            try
            {

                da.Fill(ds);
                return ds;
            }
            catch (Exception EX)
            {
               // MessageBox.Show(EX.Message);
                return null;
            }
            finally
            {
               // dbParameterConn.Close();
            }
        }

        public static void ParameterDelate(string time)
        {
            if (dbParameterConn.State == System.Data.ConnectionState.Closed)
            {
                dbParameterConn.Open();
            }

            dbParameterCmd = new SQLiteCommand();
            dbParameterCmd.Connection = dbParameterConn;
            dbParameterCmd.CommandText = "delete  from table1 where CellectTime < " + "'" + time + "'";
            dbParameterCmd.ExecuteScalar();

            // dbCurvecsCmd.Dispose();

        }




        #endregion

        #region 配方插入读取

        public static void FormulaInsert(string sql)
        {
            if (dbFormulaConn.State == System.Data.ConnectionState.Closed)
            {
                dbFormulaConn.Open();
            }

            dbFormulaCmd = new SQLiteCommand();
            dbFormulaCmd.Connection = dbFormulaConn;
            dbFormulaCmd.CommandText = sql;

            dbFormulaCmd.ExecuteNonQuery();
            //dbFormulaCmd.Dispose();
        }

        public static DataSet FormulaGetDataSet(string sql)
        {
            if (dbFormulaConn.State == System.Data.ConnectionState.Closed)
            {
                dbFormulaConn.Open();
            }

            dbFormulaCmd = new SQLiteCommand();
            dbFormulaCmd.Connection = dbFormulaConn;
            dbFormulaCmd.CommandText = sql;
            SQLiteDataAdapter da = new SQLiteDataAdapter(dbFormulaCmd);
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds);
                return ds;
            }
            catch (Exception EX)
            {
               // MessageBox.Show(EX.Message);
                return null;
            }
            finally
            {
               dbFormulaConn.Close();
            }
        }


        public static int FormulaCount()
        {
            if (dbFormulaConn.State == System.Data.ConnectionState.Closed)
            {
                dbFormulaConn.Open();
            }

            dbFormulaCmd = new SQLiteCommand();
            dbFormulaCmd.Connection = dbFormulaConn;
            dbFormulaCmd.CommandText = "SELECT Count(*) FROM  table1 ";
            object aa=  dbFormulaCmd.ExecuteScalar();
            dbFormulaCmd.Dispose();
            return Convert.ToInt32(aa);
        }

        public static bool FormulaExistJudge(string formula)
        {
            if (dbFormulaConn.State == System.Data.ConnectionState.Closed)
            {
                dbFormulaConn.Open();
            }

            dbFormulaCmd = new SQLiteCommand();
            dbFormulaCmd.Connection = dbFormulaConn;
            dbFormulaCmd.CommandText = "SELECT Count(*) from table1 where Parameter1 like "+ "'" + formula + "'" ;
            object aa = dbFormulaCmd.ExecuteScalar();
            int bb = Convert.ToInt32(aa);
            dbFormulaCmd.Dispose();

            if (bb > 0)
            {
                return true;
            } 
            else
            {
                return false;
            }      
        }

        public static void FormularDelate(string formula)
        {
            if (dbFormulaConn.State == System.Data.ConnectionState.Closed)
            {
                dbFormulaConn.Open();
            }

            dbFormulaCmd = new SQLiteCommand();
            dbFormulaCmd.Connection = dbFormulaConn;
            dbFormulaCmd.CommandText = "delete  from table1 where Parameter1 like "+ "'" + formula + "'";
            dbFormulaCmd.ExecuteScalar();
        
            // dbFormulaCmd.Dispose();

        }
        #endregion

        #region 曲线插入读取

        public static void CurvecsInsert(string sql)
        {
            if (dbCurvecsConn.State == System.Data.ConnectionState.Closed)
            {
                dbCurvecsConn.Open();
            }

            dbCurvecsCmd = new SQLiteCommand();
            dbCurvecsCmd.Connection = dbCurvecsConn;
            dbCurvecsCmd.CommandText = sql;

            dbCurvecsCmd.ExecuteNonQuery();
            // dbCurvecsCmd.Dispose();
        }

        public static DataSet CurvecsGetDataSet(string sql)
        {
            if (dbCurvecsConn.State == System.Data.ConnectionState.Closed)
            {
                dbCurvecsConn.Open();
            }

            dbCurvecsCmd = new SQLiteCommand();
            dbCurvecsCmd.Connection = dbCurvecsConn;
            dbCurvecsCmd.CommandText = sql;
            SQLiteDataAdapter da = new SQLiteDataAdapter(dbCurvecsCmd);
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds);
                return ds;
            }
            catch (Exception EX)
            {
               // MessageBox.Show(EX.Message);
                return null;
            }
            finally
            {
                //  dbFormulaConn.Close();
            }
        }
        public static void CurvecsDelate(string time)
        {
            if (dbCurvecsConn.State == System.Data.ConnectionState.Closed)
            {
                dbCurvecsConn.Open();
            }

            dbCurvecsCmd = new SQLiteCommand();
            dbCurvecsCmd.Connection = dbCurvecsConn;
            dbCurvecsCmd.CommandText = "delete  from table1 where CollectTime < " + "'" + time + "'";
            dbCurvecsCmd.ExecuteScalar();

            // dbCurvecsCmd.Dispose();

        }


        #endregion

        #region 曲线插入读取1

        public static void CurvecsInsert1(string sql)
        {
            if (dbCurvecsConn1.State == System.Data.ConnectionState.Closed)
            {
                dbCurvecsConn1.Open();
            }

            dbCurvecsCmd1 = new SQLiteCommand();
            dbCurvecsCmd1.Connection = dbCurvecsConn1;
            dbCurvecsCmd1.CommandText = sql;

            dbCurvecsCmd1.ExecuteNonQuery();
            // dbCurvecsCmd.Dispose();
        }

        public static DataSet CurvecsGetDataSet1(string sql)
        {
            if (dbCurvecsConn1.State == System.Data.ConnectionState.Closed)
            {
                dbCurvecsConn1.Open();
            }

            dbCurvecsCmd1 = new SQLiteCommand();
            dbCurvecsCmd1.Connection = dbCurvecsConn1;
            dbCurvecsCmd1.CommandText = sql;
            SQLiteDataAdapter da = new SQLiteDataAdapter(dbCurvecsCmd1);
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds);
                return ds;
            }
            catch (Exception EX)
            {
                // MessageBox.Show(EX.Message);
                return null;
            }
            finally
            {
                //  dbFormulaConn.Close();
            }
        }
        public static void CurvecsDelate1(string time)
        {
            if (dbCurvecsConn1.State == System.Data.ConnectionState.Closed)
            {
                dbCurvecsConn1.Open();
            }

            dbCurvecsCmd1 = new SQLiteCommand();
            dbCurvecsCmd1.Connection = dbCurvecsConn1;
            dbCurvecsCmd1.CommandText = "delete  from table1 where CollectTime < " + "'" + time + "'";
            dbCurvecsCmd1.ExecuteScalar();

            // dbCurvecsCmd.Dispose();

        }


        #endregion

        #region 过辊速度
        public static void OverRollInsert(string sql)
        {
            if (dbOverRollConn.State == System.Data.ConnectionState.Closed)
            {
                dbOverRollConn.Open();
            }

           dbOverRollCmd = new SQLiteCommand();
           dbOverRollCmd.Connection = dbOverRollConn;
           dbOverRollCmd.CommandText = sql;

           dbOverRollCmd.ExecuteNonQuery();
           // dbOverRollCmd.Dispose();
        }

        public static DataSet OverRollGetDataSet(string sql)
        {
            if (dbOverRollConn.State == System.Data.ConnectionState.Closed)
            {
                dbOverRollConn.Open();
            }

           dbOverRollCmd = new SQLiteCommand();
           dbOverRollCmd.Connection = dbOverRollConn;
            dbOverRollCmd.CommandText = sql;
            SQLiteDataAdapter da = new SQLiteDataAdapter(dbOverRollCmd);
            DataSet ds = new DataSet();
            try
            {

                da.Fill(ds);
                return ds;
            }
            catch (Exception EX)
            {
                MessageBox.Show(EX.Message);
                return null;
            }
            finally
            {
             //   dbOverRollConn.Close();
            }
        }

        public static void OverRollDelate(string time)
        {
            if (dbOverRollConn.State == System.Data.ConnectionState.Closed)
            {
                dbOverRollConn.Open();
            }

           dbOverRollCmd = new SQLiteCommand();
           dbOverRollCmd.Connection = dbOverRollConn;
           dbOverRollCmd.CommandText = "delete  from table1 where CollectTime < " + "'" + time + "'";
           dbOverRollCmd.ExecuteScalar();

            

        }
        #endregion

    }
}
