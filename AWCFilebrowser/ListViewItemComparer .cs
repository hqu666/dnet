using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

/// <summary>
/// ListViewの項目の並び替えに使用するクラス
/// https://dobon.net/vb/dotnet/control/lvitemsort.html
/// </summary>
public class ListViewItemComparer : IComparer
{
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

	private int _column;
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

				//現在と同じ列の時は、昇順降順を切り替える
				dbMsg += "_column=" + _column;
				if (_column == value) {
					dbMsg += "_order=" + _order;
					if (_order == SortOrder.Ascending) {
						_order = SortOrder.Descending;
					} else if (_order == SortOrder.Descending) {
						_order = SortOrder.Ascending;
					}
				}
				dbMsg += ">_order>" + _order;
				_column = value;
				dbMsg += ">_column>" + _column;
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}
		get {
			string TAG = "[ListViewItemComparer・Column・get]";
			string dbMsg = TAG;
			try {
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
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
	/// ListViewItemComparerクラスのコンストラクタ
	/// </summary>
	/// <param name="col">並び替える列の番号</param>
	/// <param name="ord">昇順か降順か</param>
	/// <param name="cmod">並び替えの方法</param>
	public ListViewItemComparer(
		int col, SortOrder ord, ComparerMode cmod) {
		_column = col;
		_order = ord;
		_mode = cmod;
	}
	public ListViewItemComparer() {
		_column = 0;
		_order = SortOrder.Ascending;
		_mode = ComparerMode.String;
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

			ListViewItem itemx = (ListViewItem)x;           //ListViewItemの取得
			string xName = itemx.Name;
			dbMsg += ",itemx=" + xName;
			System.IO.FileInfo fiX = new System.IO.FileInfo(xName);
			string xAttributes = fiX.Attributes.ToString();
						dbMsg += "=" + xAttributes;
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
			string yName = itemy.Name;
			string yAttributes = "";
			string yStr = "";
			int yInt=0;
			DateTime yDate = DateTime.MinValue;
			System.IO.FileInfo fiY;
			dbMsg += ",itemy=" + yName;
			if (yName != null && yName != "") {
				fiY = new System.IO.FileInfo(yName);
				yAttributes = fiY.Attributes.ToString();
				dbMsg += "=" + yAttributes;
				yStr = fiY.Name;
				yDate = fiY.LastWriteTime;
				if (yAttributes.Contains("Directory")) {
					DirectoryInfo diY = new DirectoryInfo(yName);
					yInt = diY.GetFiles().Length;
				} else {
					yInt = (int)fiY.Length;
				}
			}

			dbMsg += ",_columnModes=" + _columnModes;
			if (_columnModes != null && _columnModes.Length > _column) {            //並べ替えの方法を決定
				_mode = _columnModes[_column];
			}
			dbMsg += ",_mode=" + _mode;
			switch (_mode) {            //並び替えの方法別に、xとyを比較する
				case ComparerMode.String:                   //文字列をとして比較
					dbMsg += ";" + xStr + "と" + yStr;
					result = string.Compare(xStr, yStr);      //itemx.SubItems[_column].Text, itemy.SubItems[_column].Text
					break;
				case ComparerMode.Integer:                  //Int32に変換して比較//.NET Framework 2.0からは、TryParseメソッドを使うこともできる
					dbMsg += ";" + xInt + "と" + yInt;
					result = int.Parse(xInt.ToString()).CompareTo(int.Parse(yInt.ToString()));
					//		result = int.Parse(itemx.SubItems[_column].Text).CompareTo(int.Parse(itemy.SubItems[_column].Text));
					break;
				case ComparerMode.DateTime:                 //DateTimeに変換して比較					//.NET Framework 2.0からは、TryParseメソッドを使うこともできる
					dbMsg += ";" + xDate + "と" + yDate;
					result = DateTime.Compare(xDate, yDate);
					/*					result = DateTime.Compare(
											DateTime.Parse(itemx.SubItems[_column].Text),
											DateTime.Parse(itemy.SubItems[_column].Text));*/
					break;
			}
			dbMsg += ",result=" + result;
			dbMsg += "," + xAttributes + "と" + yAttributes;

			if (!xAttributes.Contains("Directory") && yAttributes.Contains("Directory")) {              //Directoryが上に有れば
				dbMsg += ">Directory:" + yStr + "を下に>";
				result = -1;
			} else {
				dbMsg += ",result=" + result;
				if (_order == SortOrder.Descending) {           //降順の時は結果を+-逆にする
					result = -1;
					dbMsg += ">result:" + result + ">" + yStr + "を下に>";
				} else if (_order == SortOrder.Ascending) {
					result = 1;
					dbMsg += ">result:" + result + ">" + yStr + "を下に>";
				}
			}
			dbMsg += ">result>" + result;
			MyLog(dbMsg);
		} catch (Exception er) {
			dbMsg += "<<以降でエラー発生>>" + er.Message;
			MyLog(dbMsg);
		}
		return result;      //結果を返す
	}


	//デバッグツール///////////////////////////////////////////////////////////その他//
	Boolean debug_now = true;
	public void MyLog(string msg) {
		if (debug_now) {
			Console.WriteLine(msg);
		}
	}


}