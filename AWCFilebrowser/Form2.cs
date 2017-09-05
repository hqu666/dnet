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
	public partial class InputDialog : Form
	{
		public string ResultText = "";      //ユーザ入力値

		/*
				public inputDialog()
				{
					InitializeComponent();
				}

				*/


		public InputDialog(string text = "", string title = "", string defaultvalue = "")      //コンストラクタ
		{
			string TAG = "[inputDialog]";
			string dbMsg = TAG;
			try {
				dbMsg += " , text=" + text + ",title=" + text + ",defaultvalue=" + defaultvalue;

				InitializeComponent();

				this.StartPosition = FormStartPosition.CenterParent;            //親フォームの中央に表示する//FormStartPosition.CenterScreenとお好みで
				inputDaialogLabel.Text = text;           //説明文
														 //行の高さを取得
				int lineH = inputDaialogLabel.GetPositionFromCharIndex( inputDaialogLabel.GetFirstCharIndexFromLine( 1 ) ).Y - inputDaialogLabel.GetPositionFromCharIndex( inputDaialogLabel.GetFirstCharIndexFromLine( 0 ) ).Y;
				if (lineH > 0) {
					inputDaialogLabel.Height = lineH * ( inputDaialogLabel.GetLineFromCharIndex( inputDaialogLabel.TextLength ) + 1 );                  //TextBoxの高さ＝行の高さ×行数
				}
				this.Text = title;          //タイトル
				inputDialogInput.Text = defaultvalue;      //デフォルト値
				inputDialogInput.Top = Math.Max( inputDaialogLabel.Bottom, InputDialogCancelButton.Bottom ) + 10;               //位置調整
				this.Height = inputDialogInput.Bottom + 50;
				string[] extStrs = defaultvalue.Split( '.' );
				if (1 < extStrs.Length) {
					string motoName = extStrs[extStrs.Length - 2];
					int reSelectEnd = motoName.Length;
					dbMsg += " , reSelectEnd=" + reSelectEnd + "まで";
					inputDialogInput.Select( 0, reSelectEnd );
				}
				MyLog( dbMsg );
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog( dbMsg );
			}
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			string TAG = "[buttonOk_Click]";
			string dbMsg = TAG;
			try {
				this.DialogResult = System.Windows.Forms.DialogResult.OK;
				dbMsg += " , DialogResult=" + this.DialogResult;
				ResultText = inputDialogInput.Text;             //ユーザ入力値を格納
				dbMsg += " , ResultText=" + ResultText;
				MyLog( dbMsg );
				this.Close();
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog( dbMsg );
			}
		}

		private void buttonCancel_Click(object sender, EventArgs e)
		{
			string TAG = "[buttonCancel_Click]";
			string dbMsg = TAG;
			try {
				this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				this.Close();
				MyLog( dbMsg );
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog( dbMsg );
			}
		}

		private void textBoxText_Enter(object sender, EventArgs e)
		{
			string TAG = "[textBoxText_Enter]";
			string dbMsg = TAG;
			try {
				//説明文にはフォーカスを当てられないようにする
				inputDialogInput.Focus();
				MyLog( dbMsg );
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog( dbMsg );
			}
		}

		private void InputSlect(object sender, EventArgs e)
		{
			string TAG = "[inputSlect]";
			string dbMsg = TAG;
			try {
				string defaultvalue = inputDialogInput.Text;      //デフォルト値
				string[] extStrs = defaultvalue.Split( '.' );
				if (1 < extStrs.Length) {
					string motoName = extStrs[extStrs.Length - 2];
					int reSelectEnd = motoName.Length;
					dbMsg += " , reSelectEnd=" + reSelectEnd + "まで";
					inputDialogInput.Select( 0, reSelectEnd );
				}
				MyLog( dbMsg );
			} catch (Exception er) {
				Console.WriteLine( TAG + "でエラー発生" + er.Message + ";" + dbMsg );
				throw new NotImplementedException();//要求されたメソッドまたは操作が実装されない場合にスローされる例外。
			}

		}

		//デバッグツール///////////////////////////////////////////////////////////その他//
		Boolean debug_now = true;
		private void MyLog(string msg)
		{
			if (debug_now) {
				Console.WriteLine( msg );
			}
		}
	}
}
/*
 参考	http://harapeko-kid.hungry.jp/csharp-inputbox
	 */
