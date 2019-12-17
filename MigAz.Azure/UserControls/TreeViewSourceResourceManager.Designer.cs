// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MigAz.Azure.UserControls
{
    partial class TreeViewSourceResourceManager
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
            this.treeAzureARM = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // treeAzureARM
            // 
            this.treeAzureARM.CheckBoxes = true;
            this.treeAzureARM.Location = new System.Drawing.Point(0, 0);
            this.treeAzureARM.Margin = new System.Windows.Forms.Padding(2);
            this.treeAzureARM.Name = "treeAzureARM";
            this.treeAzureARM.Size = new System.Drawing.Size(217, 284);
            this.treeAzureARM.TabIndex = 72;
            this.treeAzureARM.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeAzureResourcesSource_AfterCheck);
            this.treeAzureARM.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeAzureARM_BeforeExpand);
            // 
            // TreeViewSourceResourceManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeAzureARM);
            this.Name = "TreeViewSourceResourceManager";
            this.Size = new System.Drawing.Size(384, 349);
            this.Resize += new System.EventHandler(this.TreeViewSourceResourceManager_Resize);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TreeView treeAzureARM;
    }
}

