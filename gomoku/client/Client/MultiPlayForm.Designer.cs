namespace Client
{
    partial class MultiPlayForm
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
            this.boardPicture = new System.Windows.Forms.PictureBox();
            this.enterButton = new System.Windows.Forms.Button();
            this.status = new System.Windows.Forms.Label();
            this.roomTextBox = new System.Windows.Forms.TextBox();
            this.askButton = new System.Windows.Forms.Button();
            this.readyButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.boardPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // boardPicture
            // 
            this.boardPicture.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(216)))));
            this.boardPicture.Location = new System.Drawing.Point(17, 18);
            this.boardPicture.Margin = new System.Windows.Forms.Padding(4);
            this.boardPicture.Name = "boardPicture";
            this.boardPicture.Size = new System.Drawing.Size(400, 400);
            this.boardPicture.TabIndex = 0;
            this.boardPicture.TabStop = false;
            this.boardPicture.Click += new System.EventHandler(this.boardPicture_Click);
            this.boardPicture.Paint += new System.Windows.Forms.PaintEventHandler(this.boardPicture_Paint);
            this.boardPicture.MouseDown += new System.Windows.Forms.MouseEventHandler(this.boardPicture_MouseDown);
            // 
            // enterButton
            // 
            this.enterButton.Location = new System.Drawing.Point(464, 18);
            this.enterButton.Margin = new System.Windows.Forms.Padding(4);
            this.enterButton.Name = "enterButton";
            this.enterButton.Size = new System.Drawing.Size(104, 41);
            this.enterButton.TabIndex = 2;
            this.enterButton.Text = "접속하기";
            this.enterButton.UseVisualStyleBackColor = true;
            this.enterButton.Click += new System.EventHandler(this.enterButton_Click);
            // 
            // status
            // 
            this.status.Location = new System.Drawing.Point(444, 275);
            this.status.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(178, 57);
            this.status.TabIndex = 4;
            this.status.Text = "방입력 or 열린 방 묻기";
            this.status.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // roomTextBox
            // 
            this.roomTextBox.Location = new System.Drawing.Point(435, 78);
            this.roomTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.roomTextBox.Name = "roomTextBox";
            this.roomTextBox.Size = new System.Drawing.Size(162, 28);
            this.roomTextBox.TabIndex = 1;
            // 
            // askButton
            // 
            this.askButton.Location = new System.Drawing.Point(464, 130);
            this.askButton.Name = "askButton";
            this.askButton.Size = new System.Drawing.Size(104, 44);
            this.askButton.TabIndex = 5;
            this.askButton.Text = "Ask";
            this.askButton.UseVisualStyleBackColor = true;
            this.askButton.Click += new System.EventHandler(this.askButton_Click);
            // 
            // readyButton
            // 
            this.readyButton.Location = new System.Drawing.Point(464, 197);
            this.readyButton.Name = "readyButton";
            this.readyButton.Size = new System.Drawing.Size(104, 44);
            this.readyButton.TabIndex = 6;
            this.readyButton.Text = "ready";
            this.readyButton.UseVisualStyleBackColor = true;
            this.readyButton.Click += new System.EventHandler(this.readyButton_Click);
            // 
            // MultiPlayForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(628, 444);
            this.Controls.Add(this.readyButton);
            this.Controls.Add(this.askButton);
            this.Controls.Add(this.status);
            this.Controls.Add(this.enterButton);
            this.Controls.Add(this.roomTextBox);
            this.Controls.Add(this.boardPicture);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MultiPlayForm";
            this.Text = "MultiPlayForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MultiPlayForm_FormClosed);
            this.Load += new System.EventHandler(this.MultiPlayForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.boardPicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox boardPicture;
        private System.Windows.Forms.Button enterButton;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.TextBox roomTextBox;
        private System.Windows.Forms.Button askButton;
        private System.Windows.Forms.Button readyButton;
    }
}