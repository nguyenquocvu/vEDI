using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EET
{
    sealed internal class Log
    {

        static Log()
        {
            Log.lockObject = new object();
        }
        private static Log log;
        private static object lockObject;
        public static Log GetLog
        {
            get
            {
                if (Log.log == null)
                {
                    lock (Log.lockObject)
                    {
                        if (Log.log == null)
                        {
                            Log.log = new Log();
                        }
                    }
                }
                return Log.log;
            }
        }
        #region Fields
        private const long MAX_LENGTH = 200;
        private string logFileName;
        public string LogFileName
        {
            get
            {
                if (string.IsNullOrEmpty(this.logFileName))
                {
                    if (!Directory.Exists(this.LogFilePath))
                    {
                        Directory.CreateDirectory(this.LogFilePath);
                    }
                    this.logFileName = Path.Combine(this.LogFilePath, "EdiJST.log");
                }
                return this.logFileName;
            }
        }

        private string logFilePath;
        public string LogFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(this.logFilePath))
                {
                    this.logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

                }
                return logFilePath;
            }
            set { logFilePath = value; }
        }
        private long logSizeLimit;

        public long LogSizeLimit
        {
            get { return logSizeLimit; }
            set { logSizeLimit = value; }
        }


        #endregion

        #region Ctor&Dispose
        private Log()
        {
            this.logSizeLimit = 50 * 1024;
        }
        #endregion
        private void CheckLogSize()
        {
            FileInfo fInfo = new FileInfo(this.LogFileName);
            if (fInfo.Exists)
            {
                long len = fInfo.Length;
                if (len > this.LogSizeLimit)
                {
                    fInfo.MoveTo(Path.Combine(this.LogFilePath, DateTime.Now.ToString("yyMMddHHmmss") + "_EET(G)(S).log"));
                }
            }
        }
        public static void Enter()
        {
            Log.Report("*************Begin****************");
        }
        public static void Exit()
        {
            Log.Report("*************End****************");
        }
        public static void Report(string message)
        {
            Log.GetLog.ReportInfo(message);
        }
        public static void Report(Exception e)
        {
            Log.GetLog.ReportException(e);
        }

        #region Public Methods
        /// <summary>
        /// Report textový zprávy
        /// </summary>
        /// <param name="strLogText">zpráva</param>
        public void ReportInfo(string strLogText)
        {
            this.CheckLogSize();
            uint threadID = (uint)System.Threading.Thread.CurrentThread.ManagedThreadId;
            using (StreamWriter sr = new StreamWriter(this.LogFileName, true))
            {
                sr.WriteLine("{0}[{1}]\t{2}", DateTime.Now.ToString("dd.MM.yy HH:mm:ss"), threadID, strLogText);
                sr.Flush();
            }
        }
        /// <summary>
        /// Report výjimek
        /// </summary>
        /// <param name="exp">výjimka</param>
        public void ReportException(Exception exp)
        {
            Report("EXCEPTION: " + exp.ToString());
        }
        #endregion

    }
}
