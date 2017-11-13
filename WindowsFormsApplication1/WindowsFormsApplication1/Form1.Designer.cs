namespace WindowsFormsApplication1
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.MediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
			this.FlashPlater = new AxShockwaveFlashObjects.AxShockwaveFlash();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.MediaPlayer)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FlashPlater)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MediaPlayer
			// 
			this.MediaPlayer.Enabled = true;
			this.MediaPlayer.Location = new System.Drawing.Point(0, 0);
			this.MediaPlayer.Name = "MediaPlayer";
			this.MediaPlayer.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("MediaPlayer.OcxState")));
			this.MediaPlayer.Size = new System.Drawing.Size(981, 381);
			this.MediaPlayer.TabIndex = 0;
			// 
			// FlashPlater
			// 
			this.FlashPlater.Enabled = true;
			this.FlashPlater.Location = new System.Drawing.Point(0, 3);
			this.FlashPlater.Name = "FlashPlater";
			this.FlashPlater.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("FlashPlater.OcxState")));
			this.FlashPlater.Size = new System.Drawing.Size(984, 378);
			this.FlashPlater.TabIndex = 1;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.FlashPlater);
			this.splitContainer1.Size = new System.Drawing.Size(984, 769);
			this.splitContainer1.SplitterDistance = 384;
			this.splitContainer1.TabIndex = 2;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(984, 769);
			this.Controls.Add(this.MediaPlayer);
			this.Controls.Add(this.splitContainer1);
			this.Name = "Form1";
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.MediaPlayer)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FlashPlater)).EndInit();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private AxWMPLib.AxWindowsMediaPlayer MediaPlayer;
        private AxShockwaveFlashObjects.AxShockwaveFlash FlashPlater;
		private System.Windows.Forms.SplitContainer splitContainer1;
	}
}

