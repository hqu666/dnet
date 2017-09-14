namespace AWSFileBroeser
{
	partial class TypeSarch
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.TypeSarchExtention = new System.Windows.Forms.Label();
			this.TypeSarchListBox = new System.Windows.Forms.ListBox();
			this.TypeSarchTargetURL = new System.Windows.Forms.Label();
			this.TypeSarchTBGroupBox = new System.Windows.Forms.GroupBox();
			this.TypeSarchEndRadioButton = new System.Windows.Forms.RadioButton();
			this.TypeSarchTopRadioButton = new System.Windows.Forms.RadioButton();
			this.TypeSarchAcceptButton = new System.Windows.Forms.Button();
			this.TypeSarchReSelectButton = new System.Windows.Forms.Button();
			this.TypeSarchInfo = new System.Windows.Forms.Label();
			this.TypeSarchStartBt = new System.Windows.Forms.Button();
			this.TypeSarchTBGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// TypeSarchExtention
			// 
			this.TypeSarchExtention.AutoSize = true;
			this.TypeSarchExtention.Location = new System.Drawing.Point(12, 30);
			this.TypeSarchExtention.Name = "TypeSarchExtention";
			this.TypeSarchExtention.Size = new System.Drawing.Size(84, 12);
			this.TypeSarchExtention.TabIndex = 1;
			this.TypeSarchExtention.Text = "検索する拡張子";
			// 
			// TypeSarchListBox
			// 
			this.TypeSarchListBox.FormattingEnabled = true;
			this.TypeSarchListBox.ItemHeight = 12;
			this.TypeSarchListBox.Location = new System.Drawing.Point(12, 45);
			this.TypeSarchListBox.Name = "TypeSarchListBox";
			this.TypeSarchListBox.Size = new System.Drawing.Size(357, 196);
			this.TypeSarchListBox.TabIndex = 2;
			// 
			// TypeSarchTargetURL
			// 
			this.TypeSarchTargetURL.AutoSize = true;
			this.TypeSarchTargetURL.Location = new System.Drawing.Point(12, 244);
			this.TypeSarchTargetURL.Name = "TypeSarchTargetURL";
			this.TypeSarchTargetURL.Size = new System.Drawing.Size(114, 12);
			this.TypeSarchTargetURL.TabIndex = 3;
			this.TypeSarchTargetURL.Text = "追加するファイルのURL";
			// 
			// TypeSarchTBGroupBox
			// 
			this.TypeSarchTBGroupBox.Controls.Add(this.TypeSarchEndRadioButton);
			this.TypeSarchTBGroupBox.Controls.Add(this.TypeSarchTopRadioButton);
			this.TypeSarchTBGroupBox.Location = new System.Drawing.Point(14, 259);
			this.TypeSarchTBGroupBox.Name = "TypeSarchTBGroupBox";
			this.TypeSarchTBGroupBox.Size = new System.Drawing.Size(135, 44);
			this.TypeSarchTBGroupBox.TabIndex = 5;
			this.TypeSarchTBGroupBox.TabStop = false;
			this.TypeSarchTBGroupBox.Text = "の追加先";
			// 
			// TypeSarchEndRadioButton
			// 
			this.TypeSarchEndRadioButton.AutoSize = true;
			this.TypeSarchEndRadioButton.Location = new System.Drawing.Point(74, 19);
			this.TypeSarchEndRadioButton.Name = "TypeSarchEndRadioButton";
			this.TypeSarchEndRadioButton.Size = new System.Drawing.Size(47, 16);
			this.TypeSarchEndRadioButton.TabIndex = 1;
			this.TypeSarchEndRadioButton.Text = "末尾";
			this.TypeSarchEndRadioButton.UseVisualStyleBackColor = true;
			// 
			// TypeSarchTopRadioButton
			// 
			this.TypeSarchTopRadioButton.AutoSize = true;
			this.TypeSarchTopRadioButton.Checked = true;
			this.TypeSarchTopRadioButton.Location = new System.Drawing.Point(7, 19);
			this.TypeSarchTopRadioButton.Name = "TypeSarchTopRadioButton";
			this.TypeSarchTopRadioButton.Size = new System.Drawing.Size(47, 16);
			this.TypeSarchTopRadioButton.TabIndex = 0;
			this.TypeSarchTopRadioButton.TabStop = true;
			this.TypeSarchTopRadioButton.Text = "先頭";
			this.TypeSarchTopRadioButton.UseVisualStyleBackColor = true;
			// 
			// TypeSarchAcceptButton
			// 
			this.TypeSarchAcceptButton.Location = new System.Drawing.Point(280, 249);
			this.TypeSarchAcceptButton.Name = "TypeSarchAcceptButton";
			this.TypeSarchAcceptButton.Size = new System.Drawing.Size(89, 54);
			this.TypeSarchAcceptButton.TabIndex = 6;
			this.TypeSarchAcceptButton.Text = "確定";
			this.TypeSarchAcceptButton.UseVisualStyleBackColor = true;
			// 
			// TypeSarchReSelectButton
			// 
			this.TypeSarchReSelectButton.Location = new System.Drawing.Point(137, 4);
			this.TypeSarchReSelectButton.Name = "TypeSarchReSelectButton";
			this.TypeSarchReSelectButton.Size = new System.Drawing.Size(119, 23);
			this.TypeSarchReSelectButton.TabIndex = 7;
			this.TypeSarchReSelectButton.Text = "追加先の手動選択";
			this.TypeSarchReSelectButton.UseVisualStyleBackColor = true;
			// 
			// TypeSarchInfo
			// 
			this.TypeSarchInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.TypeSarchInfo.AutoSize = true;
			this.TypeSarchInfo.Location = new System.Drawing.Point(197, 30);
			this.TypeSarchInfo.Name = "TypeSarchInfo";
			this.TypeSarchInfo.Size = new System.Drawing.Size(170, 12);
			this.TypeSarchInfo.TabIndex = 8;
			this.TypeSarchInfo.Text = "TypeSarchInfo/file/dirctry/drive";
			this.TypeSarchInfo.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// TypeSarchStartBt
			// 
			this.TypeSarchStartBt.Location = new System.Drawing.Point(12, 4);
			this.TypeSarchStartBt.Name = "TypeSarchStartBt";
			this.TypeSarchStartBt.Size = new System.Drawing.Size(119, 23);
			this.TypeSarchStartBt.TabIndex = 9;
			this.TypeSarchStartBt.Text = "拡張子で検索";
			this.TypeSarchStartBt.UseVisualStyleBackColor = true;
			this.TypeSarchStartBt.Click += new System.EventHandler(this.TypeSarchStartBt_Click);
			// 
			// TypeSarch
			// 
			this.AcceptButton = this.TypeSarchAcceptButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(379, 310);
			this.Controls.Add(this.TypeSarchStartBt);
			this.Controls.Add(this.TypeSarchInfo);
			this.Controls.Add(this.TypeSarchReSelectButton);
			this.Controls.Add(this.TypeSarchAcceptButton);
			this.Controls.Add(this.TypeSarchTBGroupBox);
			this.Controls.Add(this.TypeSarchTargetURL);
			this.Controls.Add(this.TypeSarchListBox);
			this.Controls.Add(this.TypeSarchExtention);
			this.Name = "TypeSarch";
			this.Text = "Form3";
			this.TypeSarchTBGroupBox.ResumeLayout(false);
			this.TypeSarchTBGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label TypeSarchExtention;
		private System.Windows.Forms.ListBox TypeSarchListBox;
		private System.Windows.Forms.Label TypeSarchTargetURL;
		private System.Windows.Forms.GroupBox TypeSarchTBGroupBox;
		private System.Windows.Forms.RadioButton TypeSarchTopRadioButton;
		private System.Windows.Forms.RadioButton TypeSarchEndRadioButton;
		private System.Windows.Forms.Button TypeSarchAcceptButton;
		private System.Windows.Forms.Button TypeSarchReSelectButton;
		private System.Windows.Forms.Label TypeSarchInfo;
		private System.Windows.Forms.Button TypeSarchStartBt;
	}
}