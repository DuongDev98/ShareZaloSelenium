
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
            this.btnDung = new System.Windows.Forms.Button();
            this.btnFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnMoZalo
            // 
            this.btnMoZalo.Location = new System.Drawing.Point(12, 12);
            this.btnMoZalo.Name = "btnMoZalo";
            this.btnMoZalo.Size = new System.Drawing.Size(104, 30);
            this.btnMoZalo.TabIndex = 0;
            this.btnMoZalo.Text = "Mở Zalo";
            this.btnMoZalo.UseVisualStyleBackColor = true;
            // 
            // btnChiaSe
            // 
            this.btnChiaSe.Location = new System.Drawing.Point(122, 12);
            this.btnChiaSe.Name = "btnChiaSe";
            this.btnChiaSe.Size = new System.Drawing.Size(104, 30);
            this.btnChiaSe.TabIndex = 1;
            this.btnChiaSe.Text = "Chia sẻ";
            this.btnChiaSe.UseVisualStyleBackColor = true;
            // 
            // btnDung
            // 
            this.btnDung.Location = new System.Drawing.Point(232, 12);
            this.btnDung.Name = "btnDung";
            this.btnDung.Size = new System.Drawing.Size(104, 30);
            this.btnDung.TabIndex = 2;
            this.btnDung.Text = "Dừng";
            this.btnDung.UseVisualStyleBackColor = true;
            // 
            // btnFile
            // 
            this.btnFile.Location = new System.Drawing.Point(342, 12);
            this.btnFile.Name = "btnFile";
            this.btnFile.Size = new System.Drawing.Size(191, 30);
            this.btnFile.TabIndex = 3;
            this.btnFile.Text = "Xuất danh sách người dùng";
            this.btnFile.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 49);
            this.Controls.Add(this.btnFile);
            this.Controls.Add(this.btnDung);
            this.Controls.Add(this.btnChiaSe);
            this.Controls.Add(this.btnMoZalo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Main";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnMoZalo;
        private System.Windows.Forms.Button btnChiaSe;
        private System.Windows.Forms.Button btnDung;
        private System.Windows.Forms.Button btnFile;
    }
}