namespace EET
{
    partial class EET
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //this.eventLog1 = new System.Diagnostics.EventLog();
            this.FSWatcherTest = new System.IO.FileSystemWatcher();
            //((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FSWatcherTest)).BeginInit();
            // 
            // FSWatcherTest
            // 
            this.FSWatcherTest.EnableRaisingEvents = true;
            this.FSWatcherTest.Created += new System.IO.FileSystemEventHandler(this.FSWatcherTest_Created);
            // 
            // EM
            // 
            this.ServiceName = "EM";
            //((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FSWatcherTest)).EndInit();

        }

        #endregion

        //private System.Diagnostics.EventLog eventLog1;
        private System.IO.FileSystemWatcher FSWatcherTest;
    }
}
