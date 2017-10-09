using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;


namespace file_tree_clock_web1
{
	/// <summary>
	/// ListViewの項目の並び替えに使用するクラス
	/// https://dobon.net/vb/dotnet/control/lvitemsort.html
	/// </summary>
	public class ListViewItemComparer : IComparer
	{
		Form1 rootForm = new Form1();

		/// <summary>
		/// 比較する方法
		/// </summary>
		public enum ComparerMode
		{
			/// <summary>
			/// 文字列として比較
			/// </summary>
			String,
			/// <summary>
			/// 数値（Int32型）として比較
			/// </summary>
			Integer,
			/// <summary>
			/// 日時（DataTime型）として比較
			/// </summary>
			DateTime
		};

		private int _column = -1;
		private SortOrder _order;
		private ComparerMode _mode;
		private ComparerMode[] _columnModes;

		/// <summary>
		/// 並び替えるListView列の番号
		/// </summary>
		public int Column
		{
			set {
				string TAG = "[ListViewItemComparer・Column・set]";
				string dbMsg = TAG;
				try {
					dbMsg += "value=" + value + "(前;" + _column + ")";
					dbMsg += ",_order=" + _order;// + "(前;" + b_order + ")";
					if (_column == value) {                 //現在と同じ列の時は、昇順降順を切り替える
						if (_order == SortOrder.Ascending) {
							_order = SortOrder.Descending;
						} else if (_order == SortOrder.Descending || _order == SortOrder.None) {
							_order = SortOrder.Ascending;
						}
						dbMsg += ">_order>" + _order;
					}
					switch (value) {
						case 0:
							_mode = ListViewItemComparer.ComparerMode.String;
							break;
						case 1:
							_mode = ListViewItemComparer.ComparerMode.Integer;
							break;
						case 2:
							_mode = ListViewItemComparer.ComparerMode.DateTime;
							break;
					}
					_column = value;
					rootForm.MyLog(dbMsg);
				} catch (Exception er) {
					dbMsg += "<<以降でエラー発生>>" + er.Message;
					rootForm.MyLog(dbMsg);
				}
			}
			get {
				string TAG = "[ListViewItemComparer・Column・get]";
				string dbMsg = TAG;
				try {
					dbMsg += "_column=" + _column;
					rootForm.MyLog(dbMsg);
				} catch (Exception er) {
					dbMsg += "<<以降でエラー発生>>" + er.Message;
					rootForm.MyLog(dbMsg);
				}
				return _column;
			}
		}
		/// <summary>
		/// 昇順か降順か
		/// </summary>
		public SortOrder Order
		{
			set {
				_order = value;
			}
			get {
				return _order;
			}
		}
		/// <summary>
		/// 並び替えの方法
		/// </summary>
		public ComparerMode Mode
		{
			set {
				_mode = value;
			}
			get {
				return _mode;
			}
		}
		/// <summary>
		/// 列ごとの並び替えの方法
		/// </summary>
		public ComparerMode[] ColumnModes
		{
			set {
				_columnModes = value;
			}
		}

		/// <summary>
		/// ListViewItemComparerクラスのコンストラクタ	//呼ばれない
		/// </summary>
		/// <param name="col">並び替える列の番号</param>
		/// <param name="ord">昇順か降順か</param>
		/// <param name="cmod">並び替えの方法</param>
		public ListViewItemComparer(int cold) {                     //, SortOrder ord, ComparerMode cmo
			string TAG = "[ListViewItemComparer・ListViewItemComparer;指定]";
			string dbMsg = TAG;
			try {
				dbMsg += ",col=" + cold;// + ",ord=" + ord + ",cmod=" + cmod;
				_column = cold;
		//		_order = ord;
		//		_mode = cmod;
				rootForm.MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				rootForm.MyLog(dbMsg);
			}
		}

		/// <summary>
		/// 起動時に一度だけ呼ばれる
		/// </summary>
		public ListViewItemComparer() {
			string TAG = "[ListViewItemComparer・ListViewItemComparer]";
			string dbMsg = TAG;
			try {
				_column = 0;
				_order = SortOrder.Ascending;
				_mode = ComparerMode.String;
				rootForm.MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				rootForm.MyLog(dbMsg);
			}
		}

		//xがyより小さいときはマイナスの数、大きいときはプラスの数、
		//同じときは0を返す
		public int Compare(object x, object y) {
			string TAG = "[ListViewItemComparer・Compare]";
			string dbMsg = TAG;
			int result = 0;
			try {
				dbMsg += _column + "列目" + _order;
				if (_order == SortOrder.None) {             //並び替えない時
					return 0;
				}
				//	dbMsg += ",_columnModes=" + _columnModes;
				if (_columnModes != null && _columnModes.Length > _column) {            //並べ替えの方法を決定
					_mode = _columnModes[_column];
				}
				dbMsg += ",_mode=" + _mode;                                             //Integer

				ListViewItem itemx = (ListViewItem)x;           //ListViewItemの取得
				string xName = itemx.Name.Replace(@":\\", @":\");
				//		dbMsg += "\nitemx=" + xName;
				System.IO.FileInfo fiX = new System.IO.FileInfo(xName);
				string xAttributes = fiX.Attributes.ToString();
				//		dbMsg += "=" + xAttributes;
				string xStr = fiX.Name;
				DateTime xDate = fiX.LastWriteTime;
				int xInt = 0;
				if (xAttributes.Contains("Directory")) {
					DirectoryInfo diX = new DirectoryInfo(xName);
					xInt = (diX.GetDirectories().Length + diX.GetFiles().Length);
				} else {
					xInt = (int)fiX.Length;
				}

				ListViewItem itemy = (ListViewItem)y;
				string yName = itemy.Name.Replace(@":\\", @":\");
				string yAttributes = "";
				string yStr = "";
				int yInt = 0;
				DateTime yDate = DateTime.MinValue;
				System.IO.FileInfo fiY;
				//			dbMsg += "\nitemy=" + yName;
				if (yName != null && yName != "") {
					fiY = new System.IO.FileInfo(yName);
					yAttributes = fiY.Attributes.ToString();
					//				dbMsg += "=" + yAttributes;
					yStr = fiY.Name;
					yDate = fiY.LastWriteTime;
					if (yAttributes.Contains("Directory")) {
						DirectoryInfo diY = new DirectoryInfo(yName);
						yInt = diY.GetFiles().Length;
					} else {
						yInt = (int)fiY.Length;
					}
				}

				switch (_mode) {            //並び替えの方法別に、xとyを比較する
					case ComparerMode.String:                   //文字列をとして比較
						dbMsg += "\nX:" + xStr + "とY;" + yStr;
						result = string.Compare(xStr, yStr);      //itemx.SubItems[_column].Text, itemy.SubItems[_column].Text
						break;
					case ComparerMode.Integer:                  //Int32に変換して比較//.NET Framework 2.0からは、TryParseメソッドを使うこともできる
						dbMsg += "\nX:" + xInt + "とY;" + yInt;
						if (xInt < yInt) {
							result = -1;
						} else {
							result = 1;
						}
						break;
					case ComparerMode.DateTime:                 //DateTimeに変換して比較					//.NET Framework 2.0からは、TryParseメソッドを使うこともできる
						dbMsg += "\nX:" + xDate + "とY;" + yDate;
						result = DateTime.Compare(xDate, yDate);
						break;
				}
				dbMsg += " ,result=" + result;
				dbMsg += "\n," + xAttributes + "と" + yAttributes;
				if (!xAttributes.Contains("Directory") && yAttributes.Contains("Directory")) {              //Directoryが上に有れば
					dbMsg += ">YはDirectory>";
					result = -1;
				} else if (xAttributes.Contains("Directory") && !yAttributes.Contains("Directory")) {              //Directoryが上に有れば
					dbMsg += ">XはDirectory>";
					result = 1;
				} else { 
					if (_order == SortOrder.Descending) {           //降順の時は結果を+-逆にする
						dbMsg += ",反転";
						result = -result;
					}
				}
				dbMsg += ">result>" + result;
				if ( result<0) {
					dbMsg += "でYを下に>";
				} else {
					dbMsg += "でYを上に>";

				}
				rootForm.MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				rootForm.MyLog(dbMsg);
			}
			return result;      //結果を返す
		}
	}
}