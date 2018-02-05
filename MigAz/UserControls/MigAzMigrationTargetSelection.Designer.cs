﻿namespace MigAz.UserControls
{
    partial class MigAzMigrationTargetSelection
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblMigrationSourceStatus = new System.Windows.Forms.Label();
            this.btnAzureStack = new System.Windows.Forms.Button();
            this.btnAzure = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblMigrationSourceStatus);
            this.groupBox1.Controls.Add(this.btnAzureStack);
            this.groupBox1.Controls.Add(this.btnAzure);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(239, 83);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Migration Target";
            // 
            // lblMigrationSourceStatus
            // 
            this.lblMigrationSourceStatus.AutoSize = true;
            this.lblMigrationSourceStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMigrationSourceStatus.Location = new System.Drawing.Point(17, 24);
            this.lblMigrationSourceStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMigrationSourceStatus.Name = "lblMigrationSourceStatus";
            this.lblMigrationSourceStatus.Size = new System.Drawing.Size(330, 13);
            this.lblMigrationSourceStatus.TabIndex = 2;
            this.lblMigrationSourceStatus.Text = "Select Migration Source prior to setting Migration Target.";
            // 
            // btnAzureStack
            // 
            this.btnAzureStack.Enabled = false;
            this.btnAzureStack.Location = new System.Drawing.Point(133, 47);
            this.btnAzureStack.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnAzureStack.Name = "btnAzureStack";
            this.btnAzureStack.Size = new System.Drawing.Size(83, 25);
            this.btnAzureStack.TabIndex = 1;
            this.btnAzureStack.Text = "Azure Stack";
            this.btnAzureStack.UseVisualStyleBackColor = true;
            this.btnAzureStack.Click += new System.EventHandler(this.btnAzureStack_Click);
            // 
            // btnAzure
            // 
            this.btnAzure.Enabled = false;
            this.btnAzure.Location = new System.Drawing.Point(41, 47);
            this.btnAzure.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnAzure.Name = "btnAzure";
            this.btnAzure.Size = new System.Drawing.Size(78, 25);
            this.btnAzure.TabIndex = 0;
            this.btnAzure.Text = "Azure";
            this.btnAzure.UseVisualStyleBackColor = true;
            this.btnAzure.Click += new System.EventHandler(this.btnAzure_Click);
            // 
            // MigAzMigrationTargetSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MigAzMigrationTargetSelection";
            this.Size = new System.Drawing.Size(243, 86);
            this.Resize += new System.EventHandler(this.MigAzMigrationTargetSelection_Resize);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnAzureStack;
        private System.Windows.Forms.Button btnAzure;
        private System.Windows.Forms.Label lblMigrationSourceStatus;
    }
}