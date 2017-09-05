namespace file_tree_clock_web1
{
	partial class ProgressDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressDialog));
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.targetCountlabel = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.PLTotallabel = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.progCountLabel = new System.Windows.Forms.Label();
			this.messageLabel = new System.Windows.Forms.Label();
			this.cancelAsyncButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(12, 10);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(473, 12);
			this.progressBar1.TabIndex = 2;
			// 
			// targetCountlabel
			// 
			this.targetCountlabel.AutoSize = true;
			this.targetCountlabel.Location = new System.Drawing.Point(126, 34);
			this.targetCountlabel.Name = "targetCountlabel";
			this.targetCountlabel.Size = new System.Drawing.Size(41, 12);
			this.targetCountlabel.TabIndex = 12;
			this.targetCountlabel.Text = "000000";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(70, 34);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(41, 12);
			this.label9.TabIndex = 11;
			this.label9.Text = ">対象>";
			// 
			// PLTotallabel
			// 
			this.PLTotallabel.AutoSize = true;
			this.PLTotallabel.Location = new System.Drawing.Point(444, 34);
			this.PLTotallabel.Name = "PLTotallabel";
			this.PLTotallabel.Size = new System.Drawing.Size(41, 12);
			this.PLTotallabel.TabIndex = 10;
			this.PLTotallabel.Text = "000000";
			this.PLTotallabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(412, 34);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(11, 12);
			this.label1.TabIndex = 9;
			this.label1.Text = "/";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// progCountLabel
			// 
			this.progCountLabel.AutoSize = true;
			this.progCountLabel.Location = new System.Drawing.Point(12, 34);
			this.progCountLabel.Name = "progCountLabel";
			this.progCountLabel.Size = new System.Drawing.Size(41, 12);
			this.progCountLabel.TabIndex = 8;
			this.progCountLabel.Text = "000000";
			this.progCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// messageLabel
			// 
			this.messageLabel.AutoSize = true;
			this.messageLabel.Location = new System.Drawing.Point(12, 58);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = new System.Drawing.Size(66, 12);
			this.messageLabel.TabIndex = 13;
			this.messageLabel.Text = "リストアップ中";
			// 
			// cancelAsyncButton
			// 
			this.cancelAsyncButton.Location = new System.Drawing.Point(411, 53);
			this.cancelAsyncButton.Name = "cancelAsyncButton";
			this.cancelAsyncButton.Size = new System.Drawing.Size(75, 23);
			this.cancelAsyncButton.TabIndex = 14;
			this.cancelAsyncButton.Text = "キャンセル";
			this.cancelAsyncButton.UseVisualStyleBackColor = true;
			// 
			// ProgressDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(498, 80);
			this.Controls.Add(this.cancelAsyncButton);
			this.Controls.Add(this.messageLabel);
			this.Controls.Add(this.targetCountlabel);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.PLTotallabel);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.progCountLabel);
			this.Controls.Add(this.progressBar1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ProgressDialog";
			this.Text = "Form3";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label targetCountlabel;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label PLTotallabel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label progCountLabel;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Button cancelAsyncButton;
	}
}