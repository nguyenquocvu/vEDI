using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Data.SqlClient;
using System.Data.Common;
using System.Threading;
using System.Xml;
using System.Transactions;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Net;

namespace EET
{
    public partial class EET : ServiceBase
    {
        private System.Timers.Timer m_mainTimer;
        private bool m_timerTaskSuccess;
        int m_TimerInMin = 5;
        private string m_APPGS = "G";
        static Database defaultDB = null;
        EDIParams m_ediparams = new EDIParams();
        private int m_LogLevel = 0;
        public EET()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            try
            {
                Log.Report("Service OnStart.");
                InitEDI();
                Log.Report("EdiJST was init OK.");
                InitTimer();
            }
            catch (Exception ex)
            {
                Log.Report(ex);
            }
        }
        private void InitTimer()
        {
            try
            {
                m_mainTimer = new System.Timers.Timer();
                m_mainTimer.Interval = 60000 * m_TimerInMin;   // every 5 min
                m_mainTimer.Elapsed += m_mainTimer_Elapsed;
                m_mainTimer.AutoReset = false;  // makes it fire only once
                m_mainTimer.Start(); // Start
                m_timerTaskSuccess = false;
            }
            catch (Exception ex)
            {
                Log.Report(ex);
            }
        }
        void m_mainTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Log.Report("Start Timer.");
                WorkWithOldFiles();
                m_timerTaskSuccess = true;
                Log.Report("End Timer.");
            }
            catch (Exception ex)
            {
                m_timerTaskSuccess = false;
                Log.Report(ex);
            }
            finally
            {
                if (m_timerTaskSuccess)
                {
                    m_mainTimer.Start();
                }
            }
        }
        public void InitEDI()
        {
            try
            {
                m_TimerInMin = Convert.ToInt32(ConfigurationManager.AppSettings["TimerInMin"]);
                m_APPGS = ConfigurationManager.AppSettings["APPGS"];

                if (ConfigurationManager.AppSettings["LOGLEVEL"] != null)
                {
                    m_LogLevel = Convert.ToInt32(ConfigurationManager.AppSettings["LOGLEVEL"]);
                }

                DatabaseProviderFactory factory = new DatabaseProviderFactory();
                defaultDB = factory.CreateDefault();

                using (IDataReader rdr = defaultDB.ExecuteReader(CommandType.Text, "select * from tblSetting"))
                {
                    while (rdr.Read())
                    {
                        FSWatcherTest.Path = rdr["EDIDir"].ToString();
                        m_ediparams.EDIFPT = rdr["EDIFPT"].ToString();
                        m_ediparams.EDIUser = rdr["EDIUser"].ToString();
                        m_ediparams.EDIPwd = rdr["EDIPwd"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                m_timerTaskSuccess = false;
                Log.Report(ex);
            }
        }
        protected override void OnStop()
        {
            Log.Report("Service OnStop.");
            try
            {
                // Service stopped. Also stop the timer.
                m_mainTimer.Stop();
                m_mainTimer.Dispose();
                m_mainTimer = null;
            }
            catch (Exception ex)
            {
                Log.Report(ex);
            }
        }
        private void FSWatcherTest_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                if (processWithFile(e.FullPath))
                {
                    File.Delete(e.FullPath);
                }
            }
            catch(Exception ex)
            {
                Log.Report(ex);
            } 
        }
        private bool processWithFile(string fullpath)
        {
            bool bret = false;
            try
            {
                string strFileExt = Path.GetExtension(fullpath);
                if (strFileExt.ToUpper() == ".EDI")
                {
                    string sID = Path.GetFileNameWithoutExtension(fullpath);
                    Log.Report("(S): " + sID + " ---");
                    bret = SendFTP(sID);
                    if (bret)
                    {
                        UpdateEDI(sID);
                        Log.Report("(E): " + sID + " --- OK.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Report(ex.Message);
            }
            return bret;
        }
        private string GetPartFromID(string sID, string part)
        {
            string sret = "";
            string stemp = (sID.EndsWith("I") || sID.EndsWith("U") || sID.EndsWith("D")) ? sID.Substring(0, sID.Length - 1) : sID;
            string[] arr = stemp.Split(new string[] { "___" }, StringSplitOptions.None);

            switch (part)
            {
                case "ORDERID":
                    if (arr.Length == 2)
                    {
                        sret = arr[0];
                    }
                    break;
                case "STATUS":
                    sret = (sID.EndsWith("I") || sID.EndsWith("U") || sID.EndsWith("D")) ? sID.Substring(sID.Length - 1, 1) : "I";
                    break;
                case "REFNO":
                    if (arr.Length == 2)
                    {
                        sret = arr[1];
                    }
                    break;
            }
            return sret;
        }
        public void UpdateEDI(string sID)
        {
            try
            {
                string orderid = GetPartFromID(sID, "ORDERID");
                string status = GetPartFromID(sID, "STATUS");

                if (status != "D")
                {
                    string storedProcName = "UpdateEDI";
                    using (DbCommand sprocCmd = defaultDB.GetStoredProcCommand(storedProcName))
                    {
                        defaultDB.AddInParameter(sprocCmd, "OrderID", DbType.String, orderid);
                        defaultDB.ExecuteReader(sprocCmd);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Report(ex);
            }
        }
        private void WorkWithOldFiles()
        {
            try
            {
                Log.Report("Start Working With Queue.");
                string[] filePaths = Directory.GetFiles(FSWatcherTest.Path, "*.edi");
                foreach (string sfileName in filePaths)
                {
                    if (processWithFile(sfileName))
                    {
                        File.Delete(sfileName);
                    }
                }
                Log.Report("End Working With Queue.");
            }
            catch (Exception ex)
            {
                Log.Report(ex.Message);
            }
        }
        private bool SendFTP(string sID)
        {
            bool bret = false;
            try
            {
                string xml = GetXML(sID);

                string orderid = GetPartFromID(sID, "ORDERID");
                string status = GetPartFromID(sID, "STATUS");
                string ediname = orderid.Trim() + status.Trim();


                string fptfile = m_ediparams.EDIFPT + (m_ediparams.EDIFPT.EndsWith("/") ? "" : "/") + ediname + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml";
                if (m_LogLevel == 888)
                {
                    bret = ByteArrayToFile(xml, ediname);
                }
                else
                {
                    bret = FtpUploadString(xml, fptfile, m_ediparams.EDIUser, m_ediparams.EDIPwd);
                }
            }
            catch (Exception ex)
            {
                Log.Report(ex.Message);
            }
            return bret;
        }
        //private string GetXML(string sID)
        //{
        //    string xml = "";
        //    try
        //    {
        //        string orderid = (sID.EndsWith("I") || sID.EndsWith("U") || sID.EndsWith("D")) ? sID.Substring(0, sID.Length - 1) : sID;
        //        string status = (sID.EndsWith("I") || sID.EndsWith("U") || sID.EndsWith("D")) ? sID.Substring(sID.Length - 1, 1) : "I";
        //        string storedProcName = "SelectOrderImportInfo";
        //        using (DbCommand sprocCmd = defaultDB.GetStoredProcCommand(storedProcName))
        //        {
        //            defaultDB.AddInParameter(sprocCmd, "OrderID", DbType.String, orderid);
        //            using (IDataReader rdr = defaultDB.ExecuteReader(sprocCmd))
        //            {
        //                while (rdr.Read())
        //                {
        //                    string booking = rdr["Booking"].ToString();
        //                    string refno = rdr["RefNo"].ToString();
        //                    string shipper = rdr["Shipper"].ToString();
        //                    string dealer = rdr["Dealer"].ToString();
        //                    string description = rdr["Description"].ToString();
        //                    DateTime etd = (rdr["ETDDate"] is DBNull) ? DateTime.Now.AddYears(-50) : Convert.ToDateTime(rdr["ETDDate"]);
        //                    string ctnno = rdr["CTNNo"].ToString();
        //                    string customerrefno = rdr["CustomerRefNo"].ToString();
        //                    string classctn = rdr["ClassCTN"].ToString();
        //                    string transportmode = rdr["TransportMode"].ToString();
        //                    string loadingport = rdr["LoadingPort"].ToString();
        //                    string destinationport = rdr["DestinationPort"].ToString();
        //                    DateTime eta = (rdr["ETADate"] is DBNull) ? DateTime.Now.AddYears(-50) : Convert.ToDateTime(rdr["ETADate"]);
        //                    DateTime deliverydate = (rdr["DeliveryDate"] is DBNull) ? DateTime.Now.AddYears(-50) : Convert.ToDateTime(rdr["DeliveryDate"]);
        //                    string fromlocation = rdr["FromLocation"].ToString();
        //                    string tolocation = rdr["ToLocation"].ToString();
        //                    string freightmode = rdr["FreightMode"].ToString();
        //                    bool cargoinsurace = Convert.ToBoolean(rdr["CargoInsurace"]);
        //                    bool ishasbl = Convert.ToBoolean(rdr["Released"]);

        //                    xml += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
        //                    xml += "<item>";
        //                    xml += "<bookingno>" + booking + "</bookingno>";
        //                    xml += "<orderno>" + refno + "</orderno>";
        //                    xml += "<shipper>" + shipper + "</shipper>";
        //                    xml += "<dealer>" + dealer + "</dealer>";
        //                    xml += "<notice>" + description + "</notice>";
        //                    xml += "<etd>" + ((etd < DateTime.Now.AddYears(-30)) ? "" : etd.ToString("dd.MM.yyyy")) + "</etd>";
        //                    xml += "<ctnno>" + ctnno + "</ctnno>";
        //                    xml += "<cusrefno>" + customerrefno + "</cusrefno>";
        //                    xml += "<classctn>" + (string.IsNullOrEmpty(classctn) ? "" : classctn.Replace("<", "")).Replace(">", "") + "</classctn>";
        //                    xml += "<transportmode>" + transportmode + "</transportmode>";
        //                    xml += "<blno>" + ((ishasbl) ? "YES" : "NO") + "</blno>";
        //                    xml += "<loadingport>" + loadingport + "</loadingport>";
        //                    xml += "<destinationport>" + destinationport + "</destinationport>";
        //                    xml += "<eta>" + ((eta < DateTime.Now.AddYears(-30)) ? "" : eta.ToString("dd.MM.yyyy")) + "</eta>";
        //                    xml += "<cargoinsurace>" + ((cargoinsurace) ? "YES" : "NO") + "</cargoinsurace>";
        //                    xml += "<fromlocation>" + fromlocation + "</fromlocation>";
        //                    xml += "<tolocation>" + tolocation + "</tolocation>";
        //                    xml += "<doordelivery>" + ((deliverydate < DateTime.Now.AddYears(-30)) ? "" : deliverydate.ToString("dd.MM.yyyy")) + "</doordelivery>";
        //                    xml += "<freightmode>" + freightmode + "</freightmode>";
        //                    xml += "<status>" + status + "</status>";
        //                    xml += "</item>";
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Report(ex.Message);
        //    }
        //    return xml;
        //}
        private string GetXML(string sID)
        {
            string xml = "";
            try
            {
                string orderid = GetPartFromID(sID, "ORDERID");
                string status = GetPartFromID(sID, "STATUS");
                string refnoorder = GetPartFromID(sID, "REFNO");

                if (status == "D")
                {
                    string sEmpty = "";
                    xml += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
                    xml += "<item>";
                    xml += "<bookingno>" + sEmpty + "</bookingno>";
                    xml += "<orderno>" + refnoorder + "</orderno>";
                    xml += "<shipper>" + sEmpty + "</shipper>";
                    xml += "<dealer>" + sEmpty + "</dealer>";
                    xml += "<notice>" + sEmpty + "</notice>";
                    xml += "<etd>" + sEmpty + "</etd>";
                    xml += "<ctnno>" + sEmpty + "</ctnno>";
                    xml += "<cusrefno>" + sEmpty + "</cusrefno>";
                    xml += "<classctn>" + sEmpty + "</classctn>";
                    xml += "<transportmode>" + sEmpty + "</transportmode>";
                    xml += "<blno>" + sEmpty + "</blno>";
                    xml += "<loadingport>" + sEmpty + "</loadingport>";
                    xml += "<destinationport>" + sEmpty + "</destinationport>";
                    xml += "<eta>" + sEmpty + "</eta>";
                    xml += "<cargoinsurace>" + sEmpty + "</cargoinsurace>";
                    xml += "<fromlocation>" + sEmpty + "</fromlocation>";
                    xml += "<tolocation>" + sEmpty + "</tolocation>";
                    xml += "<doordelivery>" + sEmpty + "</doordelivery>";
                    xml += "<freightmode>" + sEmpty + "</freightmode>";
                    xml += "<status>" + status + "</status>";
                    xml += "</item>";
                }
                else
                {

                    string storedProcName = "SelectOrderImportInfo";
                    using (DbCommand sprocCmd = defaultDB.GetStoredProcCommand(storedProcName))
                    {
                        defaultDB.AddInParameter(sprocCmd, "OrderID", DbType.String, orderid);
                        using (IDataReader rdr = defaultDB.ExecuteReader(sprocCmd))
                        {
                            while (rdr.Read())
                            {
                                string booking = rdr["Booking"].ToString();
                                string refno = rdr["RefNo"].ToString();
                                string shipper = rdr["Shipper"].ToString();
                                string dealer = rdr["Dealer"].ToString();
                                string description = rdr["Description"].ToString();
                                DateTime etd = (rdr["ETDDate"] is DBNull) ? DateTime.Now.AddYears(-50) : Convert.ToDateTime(rdr["ETDDate"]);
                                string ctnno = rdr["CTNNo"].ToString();
                                string customerrefno = rdr["CustomerRefNo"].ToString();
                                string classctn = rdr["ClassCTN"].ToString();
                                string transportmode = rdr["TransportMode"].ToString();
                                string loadingport = rdr["LoadingPort"].ToString();
                                string destinationport = rdr["DestinationPort"].ToString();
                                DateTime eta = (rdr["ETADate"] is DBNull) ? DateTime.Now.AddYears(-50) : Convert.ToDateTime(rdr["ETADate"]);
                                DateTime deliverydate = (rdr["DeliveryDate"] is DBNull) ? DateTime.Now.AddYears(-50) : Convert.ToDateTime(rdr["DeliveryDate"]);
                                string fromlocation = rdr["FromLocation"].ToString();
                                string tolocation = rdr["ToLocation"].ToString();
                                string freightmode = rdr["FreightMode"].ToString();
                                bool cargoinsurace = Convert.ToBoolean(rdr["CargoInsurace"]);
                                bool ishasbl = Convert.ToBoolean(rdr["Released"]);

                                xml += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
                                xml += "<item>";
                                xml += "<bookingno>" + booking + "</bookingno>";
                                xml += "<orderno>" + refno + "</orderno>";
                                xml += "<shipper>" + shipper + "</shipper>";
                                xml += "<dealer>" + dealer + "</dealer>";
                                xml += "<notice>" + description + "</notice>";
                                xml += "<etd>" + ((etd < DateTime.Now.AddYears(-30)) ? "" : etd.ToString("dd.MM.yyyy")) + "</etd>";
                                xml += "<ctnno>" + ctnno + "</ctnno>";
                                xml += "<cusrefno>" + customerrefno + "</cusrefno>";
                                xml += "<classctn>" + (string.IsNullOrEmpty(classctn) ? "" : classctn.Replace("<", "")).Replace(">", "") + "</classctn>";
                                xml += "<transportmode>" + transportmode + "</transportmode>";
                                xml += "<blno>" + ((ishasbl) ? "YES" : "NO") + "</blno>";
                                xml += "<loadingport>" + loadingport + "</loadingport>";
                                xml += "<destinationport>" + destinationport + "</destinationport>";
                                xml += "<eta>" + ((eta < DateTime.Now.AddYears(-30)) ? "" : eta.ToString("dd.MM.yyyy")) + "</eta>";
                                xml += "<cargoinsurace>" + ((cargoinsurace) ? "YES" : "NO") + "</cargoinsurace>";
                                xml += "<fromlocation>" + fromlocation + "</fromlocation>";
                                xml += "<tolocation>" + tolocation + "</tolocation>";
                                xml += "<doordelivery>" + ((deliverydate < DateTime.Now.AddYears(-30)) ? "" : deliverydate.ToString("dd.MM.yyyy")) + "</doordelivery>";
                                xml += "<freightmode>" + freightmode + "</freightmode>";
                                xml += "<status>" + status + "</status>";
                                xml += "</item>";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Report(ex);
            }
            return xml;
        }
        private bool FtpUploadString(string text, string to_uri, string user_name, string password)
        {
            FtpWebRequest request;
            bool bret = false;
            try
            {
                // Get the object used to communicate with the server.
                request = (FtpWebRequest)WebRequest.Create(to_uri);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // Get network credentials.
                request.Credentials = new NetworkCredential(user_name, password);

                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = true;

                // Write the text's bytes into the request stream.
                request.ContentLength = text.Length;
                using (Stream request_stream = request.GetRequestStream())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(text);
                    request_stream.Write(bytes, 0, text.Length);
                    request_stream.Flush();
                    request_stream.Close();
                    bret = true;
                }
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                if (response != null)
                {
                    Log.Report(string.Format("Upload File Complete, status {0}", response.StatusDescription));
                    response.Close();
                }
                request = null;
            }
            catch (Exception ex)
            {
                Log.Report(ex);
            }
            return bret;
        }
        public bool ByteArrayToFile(string text, string sID)
        {
            try
            {
                //string fileName = @"c:\edi\Log\" + sID + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml";
                string fileName = Path.Combine(Log.GetLog.LogFilePath, sID + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml");
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }
    }
}
