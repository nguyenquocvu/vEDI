namespace EET
{
    partial class frmEDI
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FSWatcherTest = new System.IO.FileSystemWatcher();
            this.txtFiles = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtToSQL = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmdClear = new System.Windows.Forms.Button();
            this.cmdGetAllFIK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.FSWatcherTest)).BeginInit();
            this.SuspendLayout();
            // 
            // FSWatcherTest
            // 
            this.FSWatcherTest.EnableRaisingEvents = true;
            this.FSWatcherTest.SynchronizingObject = this;
            this.FSWatcherTest.Created += new System.IO.FileSystemEventHandler(this.FSWatcherTest_Created);
            // 
            // txtFiles
            // 
            this.txtFiles.Location = new System.Drawing.Point(12, 24);
            this.txtFiles.Multiline = true;
            this.txtFiles.Name = "txtFiles";
            this.txtFiles.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtFiles.Size = new System.Drawing.Size(629, 216);
            this.txtFiles.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Files to process";
            // 
            // txtToSQL
            // 
            this.txtToSQL.Location = new System.Drawing.Point(12, 269);
            this.txtToSQL.Multiline = true;
            this.txtToSQL.Name = "txtToSQL";
            this.txtToSQL.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtToSQL.Size = new System.Drawing.Size(629, 231);
            this.txtToSQL.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 253);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Update FIK to SQL";
            // 
            // cmdClear
            // 
            this.cmdClear.Location = new System.Drawing.Point(492, 506);
            this.cmdClear.Name = "cmdClear";
            this.cmdClear.Size = new System.Drawing.Size(149, 35);
            this.cmdClear.TabIndex = 4;
            this.cmdClear.Text = "Clear";
            this.cmdClear.UseVisualStyleBackColor = true;
            
            // 
            // cmdGetAllFIK
            // 
            this.cmdGetAllFIK.Location = new System.Drawing.Point(12, 506);
            this.cmdGetAllFIK.Name = "cmdGetAllFIK";
            this.cmdGetAllFIK.Size = new System.Drawing.Size(149, 35);
            this.cmdGetAllFIK.TabIndex = 5;
            this.cmdGetAllFIK.Text = "Get All FIK";
            this.cmdGetAllFIK.UseVisualStyleBackColor = true;
            
            // 
            // frmEET
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(653, 553);
            this.Controls.Add(this.cmdGetAllFIK);
            this.Controls.Add(this.cmdClear);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtToSQL);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFiles);
            this.Name = "frmEET";
            this.Text = "frmEET";
            this.Load += new System.EventHandler(this.frmEET_Load);
            ((System.ComponentModel.ISupportInitialize)(this.FSWatcherTest)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.IO.FileSystemWatcher FSWatcherTest;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtToSQL;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFiles;
        private System.Windows.Forms.Button cmdClear;
        private System.Windows.Forms.Button cmdGetAllFIK;
    }
}