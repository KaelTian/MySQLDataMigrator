namespace DataMigrator.App
{
    partial class FrmSimple
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
            panel1 = new Panel();
            btnStartMigration = new Button();
            panel2 = new Panel();
            txtLog = new RichTextBox();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(btnStartMigration);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1054, 67);
            panel1.TabIndex = 0;
            // 
            // btnStartMigration
            // 
            btnStartMigration.Location = new Point(390, 12);
            btnStartMigration.Name = "btnStartMigration";
            btnStartMigration.Size = new Size(123, 43);
            btnStartMigration.TabIndex = 0;
            btnStartMigration.Text = "开始迁移";
            btnStartMigration.UseVisualStyleBackColor = true;
            btnStartMigration.Click += btnStartMigration_Click;
            // 
            // panel2
            // 
            panel2.Controls.Add(txtLog);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 67);
            panel2.Name = "panel2";
            panel2.Size = new Size(1054, 552);
            panel2.TabIndex = 1;
            // 
            // txtLog
            // 
            txtLog.Dock = DockStyle.Fill;
            txtLog.Location = new Point(0, 0);
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(1054, 552);
            txtLog.TabIndex = 0;
            txtLog.Text = "";
            // 
            // FrmSimple
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1054, 619);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "FrmSimple";
            Text = "FrmSimple";
            FormClosing += MainForm_FormClosing;
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnStartMigration;
        private Panel panel2;
        private RichTextBox txtLog;
    }
}