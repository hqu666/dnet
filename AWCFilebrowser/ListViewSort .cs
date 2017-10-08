using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;
using System.IO;

namespace AWSFileBroeser
{
	//★ ListView カラムソートクラス
	public class ListViewSort : IComparer
	{
		// 比較モード
		public enum ComparerMode
		{
			String,
			Integer,
			DateTime
		};

		private ComparerMode[] _columnModes;
		private ComparerMode _mode;
		private int _column;
		private SortOrder _order;

		//☆ Set,Get アクセッサ
		// 各列ごとの比較モードを設定
		public ComparerMode[] ColumnModes
		{
			set { _columnModes = value; }
		}

		// 列がクリックされたときに設定
		public int Column
		{
			set {
				if (_column == value)       //昇順・降順の切替
				{
					if (_order == SortOrder.Ascending)
						_order = SortOrder.Descending;
					else if (_order == SortOrder.Descending)
						_order = SortOrder.Ascending;
				}
				_column = value;
			}
			get { return _column; }
		}

		// ListViewSortクラスのコンストラクタ
		// 並び替える列番号
		// 昇順か降順か
		// 並び替えの方法
		public ListViewSort(int col, SortOrder ord, ComparerMode cmod) {
			_column = col;
			_order = ord;
			_mode = cmod;
		}
		public ListViewSort() {
			_column = 0;
			_order = SortOrder.Ascending;
			_mode = ComparerMode.String;
		}

		// 比較メソッド
		public int Compare(object x, object y) {
			int result = 0;
			string TAG = "[ListViewSort・Compare]";
			string dbMsg = TAG;
			try {

				//ListViewItemの取得
				ListViewItem itemx = (ListViewItem)x;
				ListViewItem itemy = (ListViewItem)y;

				//並べ替えの方法を決定
				if (_columnModes != null && _columnModes.Length > _column)
					_mode = _columnModes[_column];

				//比較モード毎に x と y を比較
				switch (_mode) {
					case ComparerMode.String:
						result = string.Compare(itemx.SubItems[_column].Text,
							itemy.SubItems[_column].Text);
						break;
					case ComparerMode.Integer:
						result = int.Parse(itemx.SubItems[_column].Text) -
							int.Parse(itemy.SubItems[_column].Text);
						break;
					case ComparerMode.DateTime:
						result = DateTime.Compare(
							DateTime.Parse(itemx.SubItems[_column].Text),
							DateTime.Parse(itemy.SubItems[_column].Text));
						break;
				}

				//降順の時は符号を反転
				if (_order == SortOrder.Descending)
					result = -result;
				else if (_order == SortOrder.None)
					result = 0;
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
			return result;
		}

		//デバッグツール///////////////////////////////////////////////////////////その他//
		Boolean debug_now = true;
		public void MyLog(string msg) {
			if (debug_now) {
				Console.WriteLine(msg);
			}
		}
	}

}

//http://www.eonet.ne.jp/~maeda/cs/listsort.htm
