using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
//Importing visual basic to use input box
using Microsoft.VisualBasic;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;
using System.Data.SqlClient;
using System.Configuration;

namespace Arbeidskrav1
{
	public class Database
	{

        //Klasse for håndtering av kommunikasjon mellom databasen og C#
        //Funksjoner er samlet her for bedre oversikt


        public static void upload(string tagname, int Ivab, float Scaled_ivab, int errorcode, SqlConnection con)
		{

			/* Lagrer spørringen legger en ny "Datalog"-verdi i Datalog-tabellen */
			string sqlQuery = String.Concat(@"INSERT INTO Datalog (Tag, TimeStamp, Raw_SensorValue, Scaled_SensorValue, Error_Code, Units, WrittenAt)
                        VALUES('" + tagname + "','" + DateTime.Now + "','" + Ivab + "','" + Scaled_ivab + "','" + errorcode + "','" + "Kelvin" + "','" + DateTime.Now + "');");

			con.Open();
			SqlCommand command = new SqlCommand(sqlQuery, con);
			command.ExecuteNonQuery();
			con.Close();
		}
		public static void downloadconf(Form1 MainForm, string tagname, SqlConnection con)
        {
            List<string> Config = new List<string>();
            List<string> Config1 = new List<string>();
            List<string> Config2 = new List<string>();
            List<string> Config3 = new List<string>();
            List<string> Config4 = new List<string>();
            string sqlQuery = "SELECT [Tag], [LRV], [URV], [Alarm_Low], [Alarm_High] FROM [dbo].[Instrument] WHERE [Tag]='" + tagname + "'";
            SqlCommand sql = new SqlCommand(sqlQuery, con);
            con.Open();
            SqlDataReader dr = sql.ExecuteReader();

            while (dr.Read() == true)
            {
                sqlQuery = dr[0].ToString();
                Config.Add(sqlQuery);
                sqlQuery = dr[1].ToString();
                Config1.Add(sqlQuery);
                sqlQuery = dr[2].ToString();
                Config2.Add(sqlQuery);
                sqlQuery = dr[3].ToString();
                Config3.Add(sqlQuery);
                sqlQuery = dr[4].ToString();
                Config4.Add(sqlQuery);
            }
            con.Close();

            //Fjerner eventuelt tekstboksinnhold før start
            MainForm.namebox.Clear();
            MainForm.lrvbox.Clear();
            MainForm.urvbox.Clear();
            MainForm.alarmlbox.Clear();
            MainForm.alarmhbox.Clear();

            foreach (string a in Config)
            {
                MainForm.namebox.AppendText(a);
            }

            foreach (string b in Config1)
            {
                MainForm.lrvbox.AppendText(b);
            }

            foreach (string c in Config2)
            {
                MainForm.urvbox.AppendText(c);
            }

            foreach (string d in Config3)
            {
                MainForm.alarmlbox.AppendText(d);
            }

            foreach (string f in Config4)
            {
                MainForm.alarmhbox.AppendText(f);
            }

            Config.Clear();
            Config1.Clear();
            Config2.Clear();
            Config3.Clear();
            Config4.Clear();
        }

        public static void uploadconf(Form1 MainForm, string tagname, int lrv, int urv, int alarmlow, int alarmhigh, SqlConnection con)
        {
            string sqlQuery = "UPDATE [Instrument] SET [LRV] = '"+ lrv +"' ,[URV] = '"+ urv +"' ,[Alarm_Low] = '"+ alarmlow +"' ,[Alarm_High] ='" + alarmhigh +"' WHERE [Tag]='" + tagname + "'";
            con.Open();
            SqlCommand command = new SqlCommand(sqlQuery, con);
            command.ExecuteNonQuery();
            con.Close();
        }
        public static List<string> GetInstrumentNamesFromDB(SqlConnection con)
        {
            List<string> sqlResult = new List<string>();
            using (con)
            {
                SqlCommand cmd = new SqlCommand("Select [Tag] FROM [dbo].[Instrument]", con);
                con.Open();
                SqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    sqlResult.Add(r["Tag"].ToString());
                }
                con.Close();
                return sqlResult;
            }
        }

        public static List<string> Jobs(Form1 Mainform, string tagname, SqlConnection con)
        {
            List<string> DataLog_ID = new List<string>();
            List<string> RawS = new List<string>();;
            List<string> WrittenAt = new List<string>();
            List<string> DoJob = new List<string>();

           
            string sqlQuery = "SELECT [DataLog_ID], [Raw_SensorValue], [WrittenAt] FROM [dbo].[DataLog] WHERE [Tag]='" + tagname + "'";
            SqlCommand sql = new SqlCommand(sqlQuery, con);
            con.Open();
            SqlDataReader dr = sql.ExecuteReader();

            while (dr.Read() == true)
            {
                sqlQuery = dr[0].ToString();
                DataLog_ID.Add(sqlQuery);
                sqlQuery = dr[1].ToString();
                RawS.Add(sqlQuery);
                sqlQuery = dr[2].ToString();
                WrittenAt.Add(sqlQuery);
            }
            int i = 0;
            foreach (string j in WrittenAt)
            {
                i = i + 1;
                if (j.Contains("e") || j.Contains("f"))
                {
                    DoJob.Add(DataLog_ID[i-1].ToString());
                    DoJob.Add(RawS[i-1].ToString());
                    DoJob.Add(WrittenAt[i-1].ToString());
                    WrittenAt.RemoveAt(i-1);
                    con.Close();
                    return DoJob;
                }

            }
            con.Close();
            return DoJob;


        }

        public static void updatejobs(string jobid, DateTime time, SqlConnection con)
        {
            string sqlQuery = "UPDATE [DataLog] SET [WrittenAt] = '" + time + "'  WHERE [DataLog_ID]='" + jobid + "'";
            con.Open();
            SqlCommand command = new SqlCommand(sqlQuery, con);
            command.ExecuteNonQuery();
            con.Close();
        }


    }
}
