using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace file_tree_clock_web1
{
	public partial class ProgressDialog : Form
	{
		OrgFunc orgFunc = new OrgFunc();

		public ProgressDialog(string title, string maxvaluestr, string valuestr)      //,		　DoWorkEventHandler doWork,　object argument
		{
			string TAG = "[ProgressDialog]";
			string dbMsg = TAG;
			try
			{
				dbMsg += ",title=" + title;
				InitializeComponent();

		//		this.workerArgument = argument;
				this.Text = title;
				dbMsg += valuestr + "/" + maxvaluestr + ",title=" + title;
				if (maxvaluestr == "")
				{
					maxvaluestr = "100";
				}
				PLTotallabel.Text = maxvaluestr;
				progressBar1.Maximum = Int32.Parse(maxvaluestr);
				progCountLabel.Text = valuestr;
				progressBar1.Value = Int32.Parse(valuestr);
				targetCountlabel.Text = "0";
				messageLabel.Text = "リストアップ開始";

				//イベント
		/*		this.Shown += new EventHandler(ProgressDialog_Shown);
				this.cancelAsyncButton.Click += new EventHandler(cancelAsyncButton_Click);
				this.backgroundWorker1.DoWork += doWork;
				this.backgroundWorker1.ProgressChanged +=
					new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
				this.backgroundWorker1.RunWorkerCompleted +=
					new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
*/

				orgFunc.MyLog(dbMsg);
			}
			catch (Exception er)
			{
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				orgFunc.MyLog(dbMsg);
			}
		}

		//// https://dobon.net/vb/dotnet/programing/progressdialogbw.html ///////////////////////////////////////////////////////////////
		/// <summary>
		/// ProgressDialogクラスのコンストラクタ
		/// </summary>
/*		public ProgressDialog(string formTitle,DoWorkEventHandler doWorkHandler)
			: this(formTitle, doWorkHandler, null)
		{
		}

		private object workerArgument = null;

		private object _result = null;
		/// <summary>
		/// DoWorkイベントハンドラで設定された結果
		/// </summary>
		public object Result
		{
			get {
				return this._result;
			}
		}

		private Exception _error = null;
		/// <summary>
		/// バックグラウンド処理中に発生したエラー
		/// </summary>
		public Exception Error
		{
			get {
				return this._error;
			}
		}

		/// <summary>
		/// 進行状況ダイアログで使用しているBackgroundWorkerクラス
		/// </summary>
		public BackgroundWorker BackgroundWorker
		{
			get {
				return this.backgroundWorker1;
			}
		}

		//フォームが表示されたときにバックグラウンド処理を開始
		private void ProgressDialog_Shown(object sender, EventArgs e)
		{
			this.backgroundWorker1.RunWorkerAsync(this.workerArgument);
		}

		//キャンセルボタンが押されたとき
		private void cancelAsyncButton_Click(object sender, EventArgs e)
		{
			cancelAsyncButton.Enabled = false;
			backgroundWorker1.CancelAsync();
		}

		//ReportProgressメソッドが呼び出されたとき
		private void backgroundWorker1_ProgressChanged(
			object sender, ProgressChangedEventArgs e)
		{
			//プログレスバーの値を変更する
			if (e.ProgressPercentage < this.progressBar1.Minimum)
			{
				this.progressBar1.Value = this.progressBar1.Minimum;
			}
			else if (this.progressBar1.Maximum < e.ProgressPercentage)
			{
				this.progressBar1.Value = this.progressBar1.Maximum;
			}
			else
			{
				this.progressBar1.Value = e.ProgressPercentage;
			}
			//メッセージのテキストを変更する
			this.messageLabel.Text = (string)e.UserState;
		}

		//バックグラウンド処理が終了したとき
		private void backgroundWorker1_RunWorkerCompleted(
			object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				MessageBox.Show(this,
					"エラー",
					"エラーが発生しました。\n\n" + e.Error.Message,
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				this._error = e.Error;
				this.DialogResult = DialogResult.Abort;
			}
			else if (e.Cancelled)
			{
				this.DialogResult = DialogResult.Cancel;
			}
			else
			{
				this._result = e.Result;
				this.DialogResult = DialogResult.OK;
			}

			this.Close();
		}
		/////////////////////////////////////////////////////////////// https://dobon.net/vb/dotnet/programing/progressdialogbw.html //
*/
		/// <summary>
		/// プログレスバーに設定している最大値を返す
		/// </summary>
		/// <returns></returns>
		public int GetMaXValue()
		{
			string TAG = "[GetMaXValue]";
			string dbMsg = TAG;
			int retInt = 0;
			try
			{
				retInt = progressBar1.Maximum;
				dbMsg += ",retInt=" + retInt;
				//			orgFunc.MyLog(dbMsg);
			}
			catch (Exception er)
			{
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				orgFunc.MyLog(dbMsg);
			}
			return retInt;
		}

		/// <summary>
		/// プログレスバーの設定値を返す
		/// </summary>
		/// <returns></returns>
		public int GetProgValue()
		{
			string TAG = "[GetProgValue]";
			string dbMsg = TAG;
			int retInt = 0;
			try
			{
				retInt = progressBar1.Value;
				dbMsg += ",retInt=" + retInt;
				//		orgFunc.MyLog(dbMsg);
			}
			catch (Exception er)
			{
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				orgFunc.MyLog(dbMsg);
			}
			return retInt;
		}

		public void RedrowPDialog(string valuestr = "", string maxvaluestr = "", string targetCount = "", string comennt = "")
		{
			string TAG = "[RedrowPDialog]";
			string dbMsg = TAG;
			try
			{
				dbMsg += valuestr + "/" + maxvaluestr + ",targetCount=" + targetCount + ",comennt=" + comennt;
				if (maxvaluestr != "")
				{
					PLTotallabel.Text = maxvaluestr;
					PLTotallabel.Update();
					progressBar1.Maximum = Int32.Parse(maxvaluestr);
				}

				if (valuestr=="")
				{
					valuestr = progCountLabel.Text;                                 //現在のvalueを読取り
				}
				int checkCount = Int32.Parse(progCountLabel.Text) + 1;          //整数化して
				dbMsg += ">vCount>" + checkCount;
				progCountLabel.Text = checkCount.ToString();
				progCountLabel.Update();
				if (progressBar1.Maximum < checkCount)
				{
					progressBar1.Maximum = checkCount * 2;
					dbMsg += ">Maximum>" + progressBar1.Maximum;
				}
				progressBar1.Value = checkCount;
				//		progressBar1.Value = Int32.Parse(valuestr);
				targetCountlabel.Text = targetCount;
				targetCountlabel.Update();
				messageLabel.Text = comennt;
				messageLabel.Update();
				orgFunc.MyLog(dbMsg);
			}
			catch (Exception er)
			{
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				orgFunc.MyLog(dbMsg);
			}
		}
	}
}
