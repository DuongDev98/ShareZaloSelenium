﻿
namespace ZaloMessage
{
    partial class Main
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
            this.btnMoZalo = new System.Windows.Forms.Button();
            this.btnChiaSe = new System.Windows.Forms.Button();
            this.btnFile = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblThongBao = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnMoZalo
            // 
            this.btnMoZalo.Location = new System.Drawing.Point(13, 45);
            this.btnMoZalo.Name = "btnMoZalo";
            this.btnMoZalo.Size = new System.Drawing.Size(104, 30);
            this.btnMoZalo.TabIndex = 0;
            this.btnMoZalo.Text = "Mở Zalo";
            this.btnMoZalo.UseVisualStyleBackColor = true;
            // 
            // btnChiaSe
            // 
            this.btnChiaSe.Location = new System.Drawing.Point(123, 45);
            this.btnChiaSe.Name = "btnChiaSe";
            this.btnChiaSe.Size = new System.Drawing.Size(104, 30);
            this.btnChiaSe.TabIndex = 1;
            this.btnChiaSe.Text = "Chia sẻ";
            this.btnChiaSe.UseVisualStyleBackColor = true;
            // 
            // btnFile
            // 
            this.btnFile.Location = new System.Drawing.Point(233, 45);
            this.btnFile.Name = "btnFile";
            this.btnFile.Size = new System.Drawing.Size(191, 30);
            this.btnFile.TabIndex = 3;
            this.btnFile.Text = "Xuất danh sách người dùng";
            this.btnFile.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(13, 12);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(411, 23);
            this.progressBar.TabIndex = 4;
            // 
            // lblThongBao
            // 
            this.lblThongBao.BackColor = System.Drawing.Color.PapayaWhip;
            this.lblThongBao.ForeColor = System.Drawing.Color.Red;
            this.lblThongBao.Location = new System.Drawing.Point(12, 78);
            this.lblThongBao.Name = "lblThongBao";
            this.lblThongBao.Size = new System.Drawing.Size(412, 23);
            this.lblThongBao.TabIndex = 5;
            this.lblThongBao.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 108);
            this.Controls.Add(this.lblThongBao);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnFile);
            this.Controls.Add(this.btnChiaSe);
            this.Controls.Add(this.btnMoZalo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Chia sẻ tin nhắn zalo";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnMoZalo;
        private System.Windows.Forms.Button btnChiaSe;
        private System.Windows.Forms.Button btnFile;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblThongBao;
    }
}