﻿// <auto-generated />
namespace PowerPointLabs
{
    partial class PositionsPane
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
            this.WPF = new System.Windows.Forms.Integration.ElementHost();
            this.positionsPaneWPF = new PowerPointLabs.PositionsLab.PositionsPaneWpf();
            this.SuspendLayout();
            // 
            // WPF
            // 
            this.WPF.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WPF.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.WPF.Location = new System.Drawing.Point(0, 0);
            this.WPF.Name = "WPF";
            this.WPF.Size = new System.Drawing.Size(304, 883);
            this.WPF.TabIndex = 5;
            this.WPF.Text = "WPF";
            this.WPF.Child = this.positionsPaneWPF;
            // 
            // PositionssPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.WPF);
            this.Name = "PositionsPane";
            this.Size = new System.Drawing.Size(304, 883);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost WPF;
        public PositionsLab.PositionsPaneWpf positionsPaneWPF { get; private set; }
    }
}
