namespace file_tree_clock_web1
{
	partial class InputDialog
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
			if (disposing && ( components != null )) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.InputDialgAcceptButton = new System.Windows.Forms.Button();
			this.InputDialogCancelButton = new System.Windows.Forms.Button();
			this.inputDialogInput = new System.Windows.Forms.TextBox();
			this.inputDaialogLabel = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// InputDialgAcceptButton
			// 
			this.InputDialgAcceptButton.Location = new System.Drawing.Point( 459, 4 );
			this.InputDialgAcceptButton.Name = "InputDialgAcceptButton";
			this.InputDialgAcceptButton.Size = new System.Drawing.Size( 75, 23 );
			this.InputDialgAcceptButton.TabIndex = 1;
			this.InputDialgAcceptButton.Text = "OK";
			this.InputDialgAcceptButton.UseVisualStyleBackColor = true;
			this.InputDialgAcceptButton.Click += new System.EventHandler( this.buttonOk_Click );
			// 
			// InputDialogCancelButton
			// 
			this.InputDialogCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.InputDialogCancelButton.Location = new System.Drawing.Point( 459, 34 );
			this.InputDialogCancelButton.Name = "InputDialogCancelButton";
			this.InputDialogCancelButton.Size = new System.Drawing.Size( 75, 23 );
			this.InputDialogCancelButton.TabIndex = 2;
			this.InputDialogCancelButton.Text = "Cancel";
			this.InputDialogCancelButton.UseVisualStyleBackColor = true;
			this.InputDialogCancelButton.Click += new System.EventHandler( this.buttonCancel_Click );
			// 
			// inputDialogInput
			// 
			this.inputDialogInput.Location = new System.Drawing.Point( 12, 66 );
			this.inputDialogInput.Name = "inputDialogInput";
			this.inputDialogInput.Size = new System.Drawing.Size( 522, 19 );
			this.inputDialogInput.TabIndex = 3;
			this.inputDialogInput.DoubleClick += new System.EventHandler( this.InputSlect );
			// 
			// inputDaialogLabel
			// 
			this.inputDaialogLabel.BackColor = System.Drawing.SystemColors.Control;
			this.inputDaialogLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.inputDaialogLabel.Location = new System.Drawing.Point( 12, 9 );
			this.inputDaialogLabel.Name = "inputDaialogLabel";
			this.inputDaialogLabel.Size = new System.Drawing.Size( 441, 12 );
			this.inputDaialogLabel.TabIndex = 4;
			// 
			// InputDialog
			// 
			this.AcceptButton = this.InputDialgAcceptButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF( 6F, 12F );
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.InputDialogCancelButton;
			this.ClientSize = new System.Drawing.Size( 546, 99 );
			this.Controls.Add( this.inputDaialogLabel );
			this.Controls.Add( this.inputDialogInput );
			this.Controls.Add( this.InputDialogCancelButton );
			this.Controls.Add( this.InputDialgAcceptButton );
			this.Name = "InputDialog";
			this.Text = "Form2";
			this.ResumeLayout( false );
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button InputDialgAcceptButton;
		private System.Windows.Forms.Button InputDialogCancelButton;
		private System.Windows.Forms.TextBox inputDialogInput;
		private System.Windows.Forms.TextBox inputDaialogLabel;
	}
}