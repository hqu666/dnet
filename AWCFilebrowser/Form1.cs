﻿using Microsoft.Win32;          ///WebBrowserコントロールを配置すると、IEのバージョン 7をIE11の Edgeモードに変更///
using System;
using System.Text.RegularExpressions;         ///WebBrowserコントロールを配置すると、IEのバージョン 7をIE11の Edgeモードに変更///
using System.IO;
using System.Collections.Generic;       //playList
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;      //playList
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;    // 参照設定に追加を忘れずに
							//using System.Object;
							//using System.MarshalByRefObject;
							//using System.ComponentModel.Component;
							//using System.Management.ManagementBaseObject;
							//using System.Management.ManagementObject;
using Microsoft.VisualBasic.FileIO; //DelFiles,MoveFolderのFileSystem
using System.Diagnostics;
using AWSFileBroeser;
using System.Runtime.InteropServices;

///FileOpenDialogのカスタマイズ//////////////////////////////////////////////////////////////////////
using Microsoft.WindowsAPICodePack;                                     //WindowsAPICodePack-Core 1.1.2で追加
using Microsoft.WindowsAPICodePack.Dialogs;                             //'Windows7APICodePack-Core.1.1.0で追加
using Microsoft.WindowsAPICodePack.Dialogs.Controls;                    //	☆WindowsAPICodePack-Shell 1.1.1では呼べない関数が発生
																		///FileOpenDialogのカスタマイズ//////////////////////////////////////////////////////////////////////

namespace file_tree_clock_web1
{
	public partial class Form1 : Form
	{
		Microsoft.Win32.RegistryKey regkey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(FEATURE_BROWSER_EMULATION);
		const string FEATURE_BROWSER_EMULATION = @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
		string process_name = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";
		string process_dbg_name = System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".vshost.exe";

		///システムメニューのカスタマイズ /////////////////////////////////////////////////////////////////////////////////////////////
		[StructLayout(LayoutKind.Sequential)]
		struct MENUITEMINFO
		{
			public uint cbSize;
			public uint fMask;
			public uint fType;
			public uint fState;
			public uint wID;
			public IntPtr hSubMenu;
			public IntPtr hbmpChecked;
			public IntPtr hbmpUnchecked;
			public IntPtr dwItemData;
			public string dwTypeData;
			public uint cch;
			public IntPtr hbmpItem;

			// return the size of the structure
			public static uint SizeOf
			{
				get { return (uint)Marshal.SizeOf(typeof(MENUITEMINFO)); }
			}
		}

		[DllImport("user32.dll")]
		static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);      //ウィンドウのシステムメニューを取得

		[DllImport("user32.dll")]
		static extern bool InsertMenuItem(IntPtr hMenu, uint uItem, bool fByPosition,
		  [In] ref MENUITEMINFO lpmii);

		private const uint MENU_ID_20 = 0x0001;                         //ファイルエリア開閉
		private const uint MENU_ID_60 = 0x0002;                     //プレイリストエリア開閉
		private const uint MENU_ID_99 = 0x0003;

		private const uint MFT_BITMAP = 0x00000004;
		private const uint MFT_MENUBARBREAK = 0x00000020;
		private const uint MFT_MENUBREAK = 0x00000040;
		private const uint MFT_OWNERDRAW = 0x00000100;
		private const uint MFT_RADIOCHECK = 0x00000200;
		private const uint MFT_RIGHTJUSTIFY = 0x00004000;
		private const uint MFT_RIGHTORDER = 0x000002000;

		private const uint MFT_SEPARATOR = 0x00000800;
		private const uint MFT_STRING = 0x00000000;

		private const uint MIIM_FTYPE = 0x00000100;
		private const uint MIIM_STRING = 0x00000040;
		private const uint MIIM_ID = 0x00000002;

		private const uint WM_SYSCOMMAND = 0x0112;
		/////////////////////////////////////////////////////////////////////////////////////////////システムメニューのカスタマイズ///
		Settings appSettings = new Settings();

		string[] systemFiles = new string[] { "RECYCLE", ".bak", ".bdmv", ".blf", ".BIN", ".cab",  ".cfg",  ".cmd",".css",  ".dat",".dll",
												".inf",  ".inf", ".ini", ".lsi", ".iso",  ".lst", ".jar",  ".log", ".lock",".mis",
												".mni",".MARKER",  ".mbr", ".manifest","swapfile",
											  ".properties",".pnf" ,  ".prx", ".scr", ".settings",  ".so",  ".sys",  ".xml", ".exe"};
		string[] videoFiles = new string[] { ".mov", ".qt", ".mpg",".mpeg",  ".mp4",  ".m1v", ".mp2", ".mpa",".mpe",".webm",  ".ogv",
												".3gp",  ".3g2",  ".asf",  ".asx",
												".m2ts",".ts",".dvr-ms",".ivf",".wax",".wmv", ".wvx",  ".wm",  ".wmx",  ".wmz",
												".swf", ".flv", ".f4v",".rm" };
		string[] imageFiles = new string[] { ".jpg", ".jpeg", ".gif", ".png", ".tif", ".ico", ".bmp" };
		string[] audioFiles = new string[] { ".adt",  ".adts", ".aif",  ".aifc", ".aiff", ".au", ".snd", ".cda",
												".mp3", ".m4a", ".aac", ".ogg", ".mid", ".midi", ".rmi", ".ra",".ram", ".flac", ".wax", ".wma", ".wav" };
		string[] textFiles = new string[] { ".txt", ".html", ".htm", ".xhtml", ".xml", ".rss", ".xml", ".css", ".js", ".vbs", ".cgi", ".php" };
		string[] applicationFiles = new string[] { ".zip", ".pdf", ".doc", ".xls", ".wpl", ".wmd", ".wms", ".wmz", ".wmd" };
		string[] playListFiles = new string[] { ".m3u" };
		string copySouce = "";      //コピーするアイテムのurl
		string cutSouce = "";       //カットするアイテムのurl
		string assemblyPath = "";       //実行デレクトリ
		string configFileName;      //設定ファイル名 
		string assemblyName = "";       //実行ファイル名
		string playerUrl = "";
		string lsFullPathName = ""; //リストで選択されたアイテムのフルパス
		string plaingItem = "";             //再生中アイテムのフルパス;連続再生スタート時、自動送り、プレイリストからのアイテムクリックで更新
		string listUpDir = "";             //プレイリストにリストアップするデレクトリ
		string wiPlayerID = "wiPlayer";         //webに埋め込むプレイヤーのID
		List<PlayListItems> PlayListBoxItem = new List<PlayListItems>();
		List<int> treeSelectList = new List<int>();
		string nowLPlayList = "";               //現在使っているプレイリスト
		int plIndex;             //プレイリスト上のアイテムのインデックスを取得
		int PlaylistDragDropNo;
		int PlaylistDragOverNo;
		int PlaylistDragEnterNo;
		int PlaylistMouseUp;
		string plRightClickItemUrl = "";
		ListBox draglist;
		Point mouceDownPoint;
		int PlayListMouseDownNo;
		string PlayListMouseDownValue = "";
		int dragSouceIDl = -1;
		int dragSouceIDP = -1;                          //ドラッグ開始時のマウスの位置から取得
		string dragSouceUrl = "";
		private Point PlaylistMouseDownPoint = Point.Empty;     //マウスの押された位置
																//アイコン
																/*	private Cursor noneCursor = new Cursor("none.cur");
																	private Cursor moveCursor = new Cursor("move.cur");
																	private Cursor copyCursor = new Cursor("copy.cur");
																	private Cursor linkCursor = new Cursor("link.cur");*/

		//	int playListWidth = 234;            //プレイリストの幅
		//	ProgressDialog pDialog;
		List<String> PlayListFileNames = new List<String>();

		public Form1() {
			string TAG = "[Form1]";
			string dbMsg = TAG;
			typeof(Form).GetField("defaultIcon",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(
				null, new System.Drawing.Icon("awcfb_icon.ico"));

			InitializeComponent();
			assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;  //実行デレクトリ		+Path.AltDirectorySeparatorChar + "brows.htm";
			dbMsg += ",assemblyPath=" + assemblyPath;
			//	configFileName =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			//							Application.CompanyName + "\\" + Application.ProductName +"\\" + Application.ProductName + ".config");
			configFileName = assemblyPath.Replace(".exe", ".config");
			//				string[] CItems = System.Text.RegularExpressions.Regex.Split(ClickedItem, "ToolStripMenuItem");
			//	configFileName = configFileName + Path.DirectorySeparatorChar + Application.ProductName + ".config";      //設定ファイル名 //H:\develop\dnet\AWCFilebrowser\bin\Debug	@"C:\test\settings.config"
			dbMsg += ",configFileName=" + configFileName;
			ReadSetting();

			///WebBrowserコントロールを配置すると、IEのバージョン 7をIE11の Edgeモードに変更//http://blog.livedoor.jp/tkarasuma/archives/1036522520.html
			regkey.SetValue(process_name, 11001, Microsoft.Win32.RegistryValueKind.DWord);
			regkey.SetValue(process_dbg_name, 11001, Microsoft.Win32.RegistryValueKind.DWord);

			fileTree.LabelEdit = true;         //ツリーノードをユーザーが編集できるようにする

			ReWriteSysMenu();   //システムメニューカスタマイズ
								//イベントハンドラの追加
								/*		fileTree.BeforeLabelEdit += new NodeLabelEditEventHandler( FileTree_BeforeLabelEdit );
										fileTree.AfterLabelEdit += new NodeLabelEditEventHandler( FileTree1_AfterLabelEdit );
										fileTree.KeyUp += new KeyEventHandler( FileTree_KeyUp );*/

			元に戻す.Visible = false;
			ペーストToolStripMenuItem.Visible = false;
			playListRedoroe.Visible = false;                //プレイリストへボタン非表示

			MyLog(dbMsg);
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
			regkey.DeleteValue(process_name);
			regkey.DeleteValue(process_dbg_name);
			regkey.Close();
		}

		private void Form1_Load(object sender, EventArgs e) {
			string TAG = "[Form1_Load]";
			string dbMsg = TAG;

			fileTree.ImageList = this.imageList1;   //☆treeView1では設定できなかった
			MakeDriveList();

			AWSFileBroeser.Properties.Settings.Default.SettingChanging += new System.Configuration.SettingChangingEventHandler(Default_SettingChanging);//プリファレンスの変更イベント
																																						//		playListWidth = splitContainer2.Width;
																																						//dbMsg += "playListWidth" + playListWidth;
																																						/*	//イベントハンドラを追加する
																																							fileTree.ItemDrag += new ItemDragEventHandler( TreeView1_ItemDrag );
																																							fileTree.DragOver += new DragEventHandler( TreeView1_DragOver );
																																							fileTree.DragDrop += new DragEventHandler( TreeView1_DragDrop );*/
			continuousPlayCheckBox.Checked = false;//連続再生ボタン
			viewSplitContainer.Panel1Collapsed = true;
			playListBox.AllowDrop = true;
			playListBox.DragEnter += new DragEventHandler(PlayListBox_DragEnter);
			playListBox.DragDrop += new DragEventHandler(PlayListBox_DragDrop);
			MyLog(dbMsg);
		}

		private void Application_ApplicationExit(object sender, EventArgs e) {
			WriteSetting();
			Application.ApplicationExit -= new EventHandler(Application_ApplicationExit);         //ApplicationExitイベントハンドラを削除
		}       //ApplicationExitイベントハンドラ

		/*		private void Form1_ResizeEnd(object sender, EventArgs e)
				{
					string TAG = "[Form1_ResizeEnd]";
					string dbMsg = TAG;

					ReSizeViews();
					MyLog( dbMsg );
				}*/
		/// <summary>
		/// /////////////////////////////////////////////////////////////////////////////////////////////////////Formイベント/////
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e) {
			string TAG = "[TreeView1_BeforeExpand]";
			string dbMsg = TAG;
			//	dbMsg += "sender=" + sender;
			//	dbMsg += "e=" + e;
			try {
				TreeNode tn = e.Node;//, tn2;
				string sarchDir = tn.Text;//展開するノードのフルパスを取得		FullPath だとM:\\DL
				dbMsg += ",sarchDir=" + sarchDir;
				/*		string motoPass = passNameLabel.Text + "";
						dbMsg += ",motoPass=" + motoPass;
						if (motoPass != "") {
							sarchDir = motoPass + sarchDir;// + Path.DirectorySeparatorChar
						} else if (0 < motoPass.IndexOf( ":", StringComparison.OrdinalIgnoreCase )) {
							sarchDir = tn.Text;
						}
						dbMsg += ">sarchDir>" + sarchDir;
						passNameLabel.Text = sarchDir;
						*/
				//20170825		tn.Nodes.Clear();
				//	FolderItemListUp( sarchDir, tn );

				/*
								tn.Nodes.Clear();
								di = new DirectoryInfo( sarchDir );//ディレクトリ一覧を取得
								//string sarchDir = di.Name;
								MyLog( dbMsg );
								foreach (FileInfo fi in di.GetFiles(  )) {
									tn2 = new TreeNode( fi.Name, 3, 3 );
									string rfileName = fi.Name;
									rfileName = rfileName.Replace( sarchDir,"" );
									dbMsg += ",rfileName=" + rfileName;
									tn.Nodes.Add( rfileName );
								}
								MyLog( dbMsg );
								foreach (DirectoryInfo d2 in di.GetDirectories(  )) {
									tn2 = new TreeNode( d2.Name, 1, 2 );
									string rfolereName = d2.Name;
									 rfolereName = rfolereName.Replace( sarchDir + Path.DirectorySeparatorChar, "" );
									dbMsg += ",rfolereName=" + rfolereName;
									tn.Nodes.Add( rfolereName );
									FolderItemListUp( d2.Name, tn2 );
									//	tn2.Nodes.Add( "..." );
								}
								*/
				MyLog(dbMsg);
			} catch (Exception er) {
				Console.WriteLine(TAG + "でエラー発生" + er.Message + ";" + dbMsg);
			}
		}       //ノードを展開しようとしているときに発生するイベント

		/// <summary>
		/// FileTreeのアイテムクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>	
		private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)//NodeMouseClickが利かなかった
		{
			string TAG = "[TreeView1_AfterSelect]";
			string dbMsg = TAG;
			try {
				//		dbMsg += "sender=" + sender;                    //sender=	常にSystem.Windows.Forms.TreeView, Nodes.Count: 5, Nodes[0]: TreeNode: C:\,
				//		dbMsg += ",e=" + e;                             //e=		常にSystem.Windows.Forms.TreeViewEventArgs,
				typeName.Text = "";
				mineType.Text = "";
				TreeNode selectNode = e.Node;
				string selectItem = selectNode.Text;
				dbMsg += ",selectItem=" + selectItem;
				lsFullPathName = selectNode.FullPath;
				dbMsg += ",fullPathName=" + lsFullPathName;
				FileInfo fi = new FileInfo(lsFullPathName);
				String infoStr = ",Exists;";
				infoStr += fi.Exists;
				string fullName = fi.FullName;
				infoStr += ",絶対パス;" + fullName;
				infoStr += ",親ディレクトリ;" + fi.Directory;// 
				string passNameStr = fi.DirectoryName + "";    //親ディレクトリ名
				if (passNameStr == "") {
					passNameStr = fullName;
				}
				infoStr += ">>" + passNameStr;
				passNameLabel.Text = passNameStr;    //親ディレクトリ名
				string fileNameStr = fi.Name + "";//ファイル名= selectItem;
				if (fileNameStr == "") {
					fileNameStr = fullName;
				}
				fileNameLabel.Text = fileNameStr;//ファイル名= selectItem;
				lastWriteTime.Text = fi.LastWriteTime.ToString();//更新
				creationTime.Text = fi.CreationTime.ToString();//作成
				lastAccessTime.Text = fi.LastAccessTime.ToString();//アクセス
				rExtension.Text = fi.Extension.ToString();//拡張子
														  //		int32 fileLength = fi.Length*1;
				dbMsg += ",infoStr=" + infoStr;                             //infoStr=,Exists;False,拡張子;作成;2012/11/04 3:56:33,アクセス;2012/11/04 3:56:33,絶対パス;I:\Dtop,親ディレクトリ;I:\

				string fileAttributes = fi.Attributes.ToString();
				dbMsg += ",Attributes=" + fileAttributes;
				//	dbMsg += ",Directory.Exists=" + Directory.Exists( fullName );                             //infoStr=,Exists;False,拡張子;作成;2012/11/04 3:56:33,アクセス;2012/11/04 3:56:33,絶対パス;I:\Dtop,親ディレクトリ;I:\
				名称変更ToolStripMenuItem.Visible = true;
				if (copySouce != "" || cutSouce != "") {
					ペーストToolStripMenuItem.Visible = true;
					コピーToolStripMenuItem.Visible = false;
					if (cutSouce != "") {
						カットToolStripMenuItem.Visible = false;
					}
				} else {
					ペーストToolStripMenuItem.Visible = false;
					コピーToolStripMenuItem.Visible = true;
					カットToolStripMenuItem.Visible = true;
				}
				削除ToolStripMenuItem.Visible = true;
				mineType.Text = "";
				if (fileAttributes.Contains("Directory")) {
					dbMsg += ",Directoryを選択";
					FolderItemListUp(fullName, e.Node);
					フォルダ作成ToolStripMenuItem.Visible = true;
					他のアプリケーションで開くToolStripMenuItem.Visible = false;
					if (fileNameLabel.Text == passNameLabel.Text) {
						dbMsg += ",ドライブを選択";
						名称変更ToolStripMenuItem.Visible = false;
						コピーToolStripMenuItem.Visible = false;
						カットToolStripMenuItem.Visible = false;
						削除ToolStripMenuItem.Visible = false;
						元に戻す.Visible = false;
					}
					typeName.Text = "フォルダ";
					mineType.Text = fileAttributes.Replace("Directory", "");        //systemなどの他属性が有れば記載
					if (mineType.Text == "") {                                   //何の記載も無いままなら
						dbMsg += "；内容確認";
						/*	System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(lsFullPathName);       //'C:\Users\博臣\AppData\Local\Application Data' へのアクセスが拒否されました。
							System.IO.FileInfo[] files =di.GetFiles("*", System.IO.SearchOption.AllDirectories);
							if (0 < files.Length) {
								fileLength.Text += files.Length.ToString() + "アイテム";
							}*/
					}
					if (passNameLabel.Text != @"C:\") {
						PlaylistComboBox.Items[0] = passNameLabel.Text;
						dbMsg += ">PlaylistComboBox>" + PlaylistComboBox.Items[0].ToString();

					}
					//	playListRedoroe.Visible = false;                //プレイリストへボタン非表示
				} else {        //ファイルの時はArchive
					dbMsg += ",ファイルを選択";
					if (rExtension.Text != "") {
						fileLength.Text = fi.Length.ToString();//ファイルサイズ
						typeName.Text = GetFileTypeStr(lsFullPathName);
						他のアプリケーションで開くToolStripMenuItem.Visible = true;
						dbMsg += "Checked=" + continuousPlayCheckBox.Checked;
						if (continuousPlayCheckBox.Checked) {                   //連続再生中
							playListRedoroe.Visible = true;                     //プレイリストへボタン表示
						} else {                                                //でなければ
							PlayFromFileBrousert(fullName);                       //再生動作へ
						}
					}
					appSettings.CurrentFile = lsFullPathName;               //ファイルが選択される度に書換
					WriteSetting();
				}
				if (typeName.Text == "video" || typeName.Text == "audio") {
					continuousPlayCheckBox.Visible = true;                 //連続再生中チェックボックス表示
																		   //splitContainer2.Panel1Collapsed = false;                 //playlistPanelを開く
				} else {
					continuousPlayCheckBox.Visible = false;                 //連続再生中チェックボックス非表示
					continuousPlayCheckBox.Checked = false;
					//	splitContainer2.Panel1Collapsed = true;             //playlistPanelを閉じる
				}
				appSettings.CurrentFile = lsFullPathName;
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// 指定したフォルダ内のアイテムをfileTreeにリストアップする
		/// </summary>
		/// <param name="sarchDir"></param>
		/// <param name="tNode"></param>
		private void FolderItemListUp(string sarchDir, TreeNode tNode)//, string sarchTyp
		{
			string TAG = "[FolderItemListUp]";
			string dbMsg = TAG;
			try {
				dbMsg += "sarchDir=" + sarchDir;                    //sender=System.Windows.Forms.TreeView, Nodes.Count: 5, Nodes[0]: TreeNode: C:\,
				dbMsg += ",tNode=" + tNode;                             //e=System.Windows.Forms.TreeViewEventArgs,
				dbMsg += ",Nodes=" + tNode.Nodes.ToString();
				tNode.Nodes.Clear();

				string[] files = Directory.GetFiles(sarchDir);        //		sarchDir	"C:\\\\マイナンバー.pdf"	string	☆sarchDir = "\\2013.m3u"でフルパスになっていない
				if (files != null) {
					foreach (string fileName in files) {
						string[] extStrs = fileName.Split('.');
						string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
						dbMsg += "\n拡張子=" + extentionStr;
						if (-1 < Array.IndexOf(systemFiles, extentionStr) ||
							0 < fileName.IndexOf("BOOTNXT", StringComparison.OrdinalIgnoreCase) ||
							0 < fileName.IndexOf("-ms", StringComparison.OrdinalIgnoreCase) ||
							0 < fileName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase)
							) {
						} else {
							int iconType = 2;
							if (-1 < Array.IndexOf(videoFiles, extentionStr)) {
								iconType = 3;
							} else if (-1 < Array.IndexOf(imageFiles, extentionStr)) {
								iconType = 4;
							} else if (-1 < Array.IndexOf(audioFiles, extentionStr)) {
								iconType = 5;
							} else if (-1 < Array.IndexOf(textFiles, extentionStr)) {
								iconType = 2;
							}
							dbMsg += ",iconType=" + iconType;
							string rfileName = fileName.Replace(sarchDir, "");
							rfileName = rfileName.Replace(Path.DirectorySeparatorChar + "", "");
							dbMsg += ",file=" + rfileName;
							tNode.Nodes.Add(fileName, rfileName, iconType, iconType);
						}
					}
				}
				string[] foleres = Directory.GetDirectories(sarchDir);//
				if (foleres != null) {
					foreach (string folereName in foleres) {
						if (-1 < folereName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
							-1 < folereName.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase)) {
						} else {
							string rfolereName = folereName.Replace(sarchDir, "");// + 
							rfolereName = rfolereName.Replace(Path.DirectorySeparatorChar + "", "");
							dbMsg += ",foler=" + rfolereName;
							tNode.Nodes.Add(folereName, rfolereName, 1, 1);
						}
					}           //ListBox1に結果を表示する
				}
				//		MyLog( dbMsg );
			} catch (UnauthorizedAccessException UAEx) {
				Console.WriteLine(TAG + "で" + UAEx.Message + "発生;" + dbMsg);
			} catch (PathTooLongException PathEx) {
				Console.WriteLine(TAG + "で" + PathEx.Message + "発生;" + dbMsg);
			} catch (Exception er) {
				Console.WriteLine(TAG + "でエラー発生" + er.Message + ";" + dbMsg);
			}
		}       //フォルダの中身をリストアップ

		/// <summary>
		/// 連続再生時、再生対象をファイルリストで選択しているファイルに切り替える
		/// プレイリストファイルを選択した場合の読込み
		/// </summary>
		/// <param name="fullName"></param>
		public void PlayFromFileBrousert(string fullName) {
			string TAG = "[PlayFromFileList]";
			string dbMsg = TAG;
			try {
				dbMsg += fullName;
				string[] extStrs = fullName.Split('.');
				string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
				dbMsg += ",extentionStr=" + extentionStr;
				MyLog(dbMsg);
				if (extentionStr == ".m3u") {
					ComboBoxAddItems(PlaylistComboBox, fullName);
					string[] PLArray = ComboBoxItems2StrArray(PlaylistComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };
					dbMsg += ",PLArray=" + PLArray.Length + "件";
					int plSelIndex = Array.IndexOf(PLArray, fullName) + 1;
					dbMsg += "," + plSelIndex + "番目";
					PlaylistComboBox.SelectedIndex = plSelIndex;
					//		ReadPlayList(fullName);
				} else {
					playerUrl = fullName; //リストで選択されたアイテムのフルパス
					plaingItem = fullName;             //再生中アイテムのフルパス
					lsFullPathName = fullName;
					MakeWebSouce(fullName);
					/*		dbMsg += ";;playList準備；既存;" + PlayListBoxItem.Count + "件";
							PlayListBoxItem = new List<PlayListItems>();
							progCountLabel.Text = "0";
							int nowToTal = CurrentItemCount( passNameLabel.Text );
							dbMsg += ";nowToTal;" + nowToTal + "件";
							if (0 == nowToTal) {
								nowToTal = 100;
								dbMsg += ">>" + nowToTal + "件";
							}
							progressBar1.Maximum = nowToTal;
							progressBar1.Value = PlayListBoxItem.Count;*/
					SetPlayListItems(passNameLabel.Text, typeName.Text);
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// 再生状態取得	未使用
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayStateChangeEvent(object sender, EventArgs e)           //AxWMPLib._WMPOCXEvents_PlayStateChangeEvent
		{
			string TAG = "[PlayStateChangeEvent]";
			string dbMsg = TAG;
			try {
				/*	switch (e.newState) {
						case 0:    // Undefined
						dbMsg+= "Undefined";
						break;

						case 1:    // Stopped
						dbMsg +=  "Stopped";
						break;

						case 2:    // Paused
						currentStateLabel.Text = "Paused";
						break;

						case 3:    // Playing
						currentStateLabel.Text = "Playing";
						break;

						case 4:    // ScanForward
						currentStateLabel.Text = "ScanForward";
						break;

						case 5:    // ScanReverse
						currentStateLabel.Text = "ScanReverse";
						break;

						case 6:    // Buffering
						currentStateLabel.Text = "Buffering";
						break;

						case 7:    // Waiting
						currentStateLabel.Text = "Waiting";
						break;

						case 8:    // MediaEnded
						currentStateLabel.Text = "MediaEnded";
						break;

						case 9:    // Transitioning
						currentStateLabel.Text = "Transitioning";
						break;

						case 10:   // Ready
						currentStateLabel.Text = "Ready";
						break;

						case 11:   // Reconnecting
						currentStateLabel.Text = "Reconnecting";
						break;

						case 12:   // Last
						currentStateLabel.Text = "Last";
						break;

						default:
						currentStateLabel.Text = ( "Unknown State: " + e.newState.ToString() );
						break;
					}*/
				MyLog(dbMsg);
			} catch (NotImplementedException er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
				throw new NotImplementedException();
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}

		}

		/*		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
				{
					string selectDrive = comboBox1.SelectedItem.ToString();
			//		listBox1.Items.Clear();
					MakeFolderList( selectDrive );
				}           //ドライブセレクト*/

		private void MakeFolderList(string sarchDir)//, string sarchTyp
		{
			try {
				string[] files = Directory.GetFiles(sarchDir);
				if (files != null) {
					foreach (string fileName in files) {
						if (-1 < fileName.IndexOf("RECYCLE.BIN", StringComparison.OrdinalIgnoreCase)) {
						} else {

							string rfileName = fileName.Replace(sarchDir, "");
							//					listBox1.Items.Add( rfileName );      //ListBox1に結果を表示する
						}
					}
				}
				string[] foleres = Directory.GetDirectories(sarchDir);//
				if (foleres != null) {
					foreach (string folereName in foleres) {
						if (-1 < folereName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
							-1 < folereName.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase)
							) { } else {
							//	listBox1.Items.Add( folereName );
							//        MakeFolderList(folereName);
						}
					}           //ListBox1に結果を表示する

				}
			} catch (UnauthorizedAccessException UAEx) {
				Console.WriteLine(UAEx.Message);
			} catch (PathTooLongException PathEx) {
				Console.WriteLine(PathEx.Message);
			}

		}       //ファイルリストアップ

		private void MakeFileList(string sarchDir, string sarchType) {
			string[] files = Directory.GetFiles("c:\\");
			foreach (string fileName in files) {
				//			listBox1.Items.Add( fileName );
			}           //ListBox1に結果を表示する

			//     System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(sarchDir);
			//     System.IO.FileInfo[] files =di.GetFiles(sarchType, System.IO.SearchOption.AllDirectories);
			//        foreach (System.IO.FileInfo f in files)
			//       {
			//           listBox1.Items.Add(f.FullName);
			//       }           //ListBox1に結果を表示する

			//以下2行でも同様      https://dobon.net/vb/dotnet/file/getfiles.html
			//            string[] files = System.IO.Directory.GetFiles( sarchDir, sarchType, System.IO.SearchOption.AllDirectories);           //"C:\test"以下のファイルをすべて取得する
			//         listBox1.Items.AddRange(files);           //ListBox1に結果を表示する
		}       //ファイルリストアップ

		private void MakeDriveList() {
			TreeNode tn;
			foreach (DriveInfo drive in DriveInfo.GetDrives())//http://www.atmarkit.co.jp/fdotnet/dotnettips/557driveinfo/driveinfo.html
			{
				string driveNames = drive.Name; // ドライブ名
				if (drive.IsReady) { // ドライブの準備はOK？
					tn = new TreeNode(driveNames, 0, 0);
					fileTree.Nodes.Add(tn);//親ノードにドライブを設定
					FolderItemListUp(driveNames, tn);
					tn.ImageIndex = 0;          //hd_icon.png
				}
			}
		}//使用可能なドライブリスト取得

		////ファイル操作////////////////////////////////////////////////////////////////////
		/// <summary>
		/// 拡張子からファイルタイプを返し、MIMEをセットする
		/// </summary>
		/// <param name="checkFileName"></param>
		/// <returns></returns>
		public string GetFileTypeStr(string checkFileName) {
			string TAG = "[GetFileTypeStr]";
			string dbMsg = TAG;
			//	try {
			string retType = "";
			string retMIME = "";
			string[] extStrs = checkFileName.Split('.');
			string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
			dbMsg += "\n拡張子=" + extentionStr;
			if (-1 < extentionStr.IndexOf(".mov", StringComparison.OrdinalIgnoreCase) ||
				-1 < extentionStr.IndexOf(".qt", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "video/quicktime";
			} else if (-1 < extentionStr.IndexOf(".mpg", StringComparison.OrdinalIgnoreCase) ||
				-1 < extentionStr.IndexOf(".mpeg", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "video/mpeg";
			} else if (-1 < extentionStr.IndexOf(".mp4", StringComparison.OrdinalIgnoreCase)) {          //動画コーデック：H.264/音声コーデック：MP3、AAC
				retType = "video";
				retMIME = "video/mp4";        //ver12:MP4 ビデオ ファイル <source src="movie.mp4" type='video/mp4; codecs="avc1.42E01E, mp4a.40.2"' />
			} else if (-1 < extentionStr.IndexOf(".webm", StringComparison.OrdinalIgnoreCase)) {          //動画コーデック：VP8 / Vorbis
				retType = "video";
				retMIME = "video/webm";//  <source src="movie.webm" type='video/webm; codecs="vp8, vorbis"' />
			} else if (-1 < extentionStr.IndexOf(".ogv", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "video/ogv";
			} else if (-1 < extentionStr.IndexOf(".avi", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "video/x-msvideo";
			} else if (-1 < extentionStr.IndexOf(".3gp", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "video/3gpp";     //audio/3gpp
			} else if (-1 < extentionStr.IndexOf(".3g2", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "video/3gpp2";            //audio/3gpp2
			} else if (-1 < extentionStr.IndexOf(".asf", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "video/x-ms-asf";
			} else if (-1 < extentionStr.IndexOf(".asx", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "video/x-ms-asf";   //ver9:Windows Media メタファイル 
			} else if (-1 < extentionStr.IndexOf(".wax", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";   //ver9:Windows Media メタファイル 
			} else if (-1 < extentionStr.IndexOf(".wmv", StringComparison.OrdinalIgnoreCase)) {
				retMIME = "video/x-ms-wmv";      //ver9:Windows Media 形式
				retType = "video";
			} else if (-1 < extentionStr.IndexOf(".wvx", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "video/x-ms-wvx";       //ver9:Windows Media メタファイル 
			} else if (-1 < extentionStr.IndexOf(".wmx", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "video/x-ms-wmx";       //ver9:Windows Media メタファイル 
			} else if (-1 < extentionStr.IndexOf(".wmz", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "application/x-ms-wmz";
			} else if (-1 < extentionStr.IndexOf(".wmd", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "application/x-ms-wmd";
			} else if (-1 < extentionStr.IndexOf(".swf", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "application/x-shockwave-flash";
			} else if (-1 < extentionStr.IndexOf(".flv", StringComparison.OrdinalIgnoreCase)) {          //動画コーデック：Sorenson Spark / On2VP6/音声コーデック：MP3
				retType = "video";
				retMIME = "application/x-shockwave-flash";
				//	retMIME = "video/x-flv";
			} else if (-1 < extentionStr.IndexOf(".f4v", StringComparison.OrdinalIgnoreCase)) {          //動画コーデック：H.264/音声コーデック：MP3、AAC、HE - AAC
				retType = "video";
				retMIME = "video/mp4";
			} else if (-1 < extentionStr.IndexOf(".rm", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				retMIME = "application/vnd.rn-realmedia";
			} else if (-1 < extentionStr.IndexOf(".ivf", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";     //ver10:Indeo Video Technology
			} else if (-1 < extentionStr.IndexOf(".dvr-ms", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";            //ver12:Microsoft デジタル ビデオ録画
			} else if (-1 < extentionStr.IndexOf(".m2ts", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";           //m2tsと同じ
											 /*
											  .htaccess や Apache のMIME Type設定
											  AddType application/x-mpegURL .m3u8
AddType video/MP2T .ts
										  */
			} else if (-1 < extentionStr.IndexOf(".ts", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";           //ver12:MPEG-2 TS ビデオ ファイル 
			} else if (-1 < extentionStr.IndexOf(".m1v", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
			} else if (-1 < extentionStr.IndexOf(".mp2", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
			} else if (-1 < extentionStr.IndexOf(".mpa", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
			} else if (-1 < extentionStr.IndexOf(".mpe", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
			} else if (-1 < extentionStr.IndexOf(".m4v", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
			} else if (-1 < extentionStr.IndexOf(".mp4v", StringComparison.OrdinalIgnoreCase)) {
				retType = "video";
				//image/////////////////////////////////////////////////////////////////////////
			} else if (-1 < extentionStr.IndexOf(".jpg", StringComparison.OrdinalIgnoreCase) ||
					 -1 < extentionStr.IndexOf(".jpeg", StringComparison.OrdinalIgnoreCase)) {
				retType = "image";
				retMIME = "image/jpeg";
			} else if (-1 < extentionStr.IndexOf(".gif", StringComparison.OrdinalIgnoreCase)) {
				retType = "image";
				retMIME = "image/gif";
			} else if (-1 < extentionStr.IndexOf(".png", StringComparison.OrdinalIgnoreCase)) {
				retType = "image";
				retMIME = "image/png";
			} else if (-1 < extentionStr.IndexOf(".ico", StringComparison.OrdinalIgnoreCase)) {
				retType = "image";
				retMIME = "image/vnd.microsoft.icon";
			} else if (-1 < extentionStr.IndexOf(".bmp", StringComparison.OrdinalIgnoreCase)) {
				retType = "image";
				retMIME = "image/x-ms-bmp";
				//audio/////////////////////////////////////////////////////////////////////////
			} else if (-1 < extentionStr.IndexOf(".mp3", StringComparison.OrdinalIgnoreCase)) {
				retType = "audio";
				retMIME = "audio/mpeg";
			} else if (-1 < extentionStr.IndexOf(".m4a", StringComparison.OrdinalIgnoreCase) ||
				-1 < extentionStr.IndexOf(".aac", StringComparison.OrdinalIgnoreCase)
				) {
				retType = "audio";
				retMIME = "audio/aac";         //var12;MP4 オーディオ ファイル
			} else if (-1 < extentionStr.IndexOf(".ogg", StringComparison.OrdinalIgnoreCase)) {
				retType = "audio";
				retMIME = "audio/ogg";
			} else if (-1 < extentionStr.IndexOf(".midi", StringComparison.OrdinalIgnoreCase) ||
				-1 < extentionStr.IndexOf(".mid", StringComparison.OrdinalIgnoreCase) ||
				-1 < extentionStr.IndexOf(".rmi", StringComparison.OrdinalIgnoreCase)
				) {
				retType = "audio";
				retMIME = "audio/midi";          //var9;MIDI 
			} else if (-1 < extentionStr.IndexOf(".ra", StringComparison.OrdinalIgnoreCase) ||
				-1 < extentionStr.IndexOf(".ram", StringComparison.OrdinalIgnoreCase)
				) {
				retType = "audio";
				retMIME = "audio/vnd.rn-realaudio";
			} else if (-1 < extentionStr.IndexOf(".flac", StringComparison.OrdinalIgnoreCase)) {
				retType = "audio";
				retMIME = "audio/flac";
			} else if (-1 < extentionStr.IndexOf(".wma", StringComparison.OrdinalIgnoreCase)) {
				retType = "audio";
				retMIME = "audio/x-ms-wma";
			} else if (-1 < extentionStr.IndexOf(".wav", StringComparison.OrdinalIgnoreCase)) {
				retType = "audio";
				retMIME = "audio/wav";           //var9;Windows 用オーディオ   
			} else if (-1 < extentionStr.IndexOf(".aif", StringComparison.OrdinalIgnoreCase) ||
				-1 < extentionStr.IndexOf(".aifc", StringComparison.OrdinalIgnoreCase) ||
				-1 < extentionStr.IndexOf(".aiff", StringComparison.OrdinalIgnoreCase)
				) {
				retType = "audio";           //var9;Audio Interchange File FormatI 
			} else if (-1 < extentionStr.IndexOf(".au", StringComparison.OrdinalIgnoreCase)) {
				retType = "audio";          //var9;Sun Microsystems  
			} else if (-1 < extentionStr.IndexOf(".snd", StringComparison.OrdinalIgnoreCase)) {
				retType = "audio";          //var9; NeXT  
			} else if (-1 < extentionStr.IndexOf(".cda", StringComparison.OrdinalIgnoreCase)) {
				retType = "audio";          //var9;CD オーディオ トラック 
			} else if (-1 < extentionStr.IndexOf(".adt", StringComparison.OrdinalIgnoreCase)) {
				retType = "audio";          //var12;Windows オーディオ ファイル 
			} else if (-1 < extentionStr.IndexOf(".adts", StringComparison.OrdinalIgnoreCase)) {
				retType = "audio";           //var12;Windows オーディオ ファイル 
			} else if (-1 < extentionStr.IndexOf(".asx", StringComparison.OrdinalIgnoreCase)) {
				retType = "audio";
				//text/////////////////////////////////////////////////////////////////////////
			} else if (-1 < extentionStr.IndexOf(".txt", StringComparison.OrdinalIgnoreCase)) {
				retType = "text";
				retMIME = "text/plain";
			} else if (-1 < extentionStr.IndexOf(".html", StringComparison.OrdinalIgnoreCase) ||
				-1 < extentionStr.IndexOf(".htm", StringComparison.OrdinalIgnoreCase)
				) {
				retType = "text";
				retMIME = "text/html";
			} else if (-1 < extentionStr.IndexOf(".xhtml", StringComparison.OrdinalIgnoreCase)) {
				retMIME = "application/xhtml+xml";
			} else if (-1 < extentionStr.IndexOf(".xml", StringComparison.OrdinalIgnoreCase)) {
				retType = "text";
				retMIME = "text/xml";
			} else if (-1 < extentionStr.IndexOf(".rss", StringComparison.OrdinalIgnoreCase)) {
				retType = "text";
				retMIME = "application/rss+xml";
			} else if (-1 < extentionStr.IndexOf(".xml", StringComparison.OrdinalIgnoreCase)) {
				retType = "text";
				retMIME = "application/xml";            //、text/xml
			} else if (-1 < extentionStr.IndexOf(".css", StringComparison.OrdinalIgnoreCase)) {
				retType = "text";
				retMIME = "text/css";
			} else if (-1 < extentionStr.IndexOf(".js", StringComparison.OrdinalIgnoreCase)) {
				retType = "text";
				retMIME = "text/javascript";
			} else if (-1 < extentionStr.IndexOf(".vbs", StringComparison.OrdinalIgnoreCase)) {
				retType = "text";
				retMIME = "text/vbscript";
			} else if (-1 < extentionStr.IndexOf(".cgi", StringComparison.OrdinalIgnoreCase)) {
				retType = "text";
				retMIME = "application/x-httpd-cgi";
			} else if (-1 < extentionStr.IndexOf(".php", StringComparison.OrdinalIgnoreCase)) {
				retType = "text";
				retMIME = "application/x-httpd-php";
				//application/////////////////////////////////////////////////////////////////////////
			} else if (-1 < extentionStr.IndexOf(".zip", StringComparison.OrdinalIgnoreCase)) {
				retType = "application";
				retMIME = "application/zip";
			} else if (-1 < extentionStr.IndexOf(".pdf", StringComparison.OrdinalIgnoreCase)) {
				retType = "application";
				retMIME = "application/pdf";
			} else if (-1 < extentionStr.IndexOf(".doc", StringComparison.OrdinalIgnoreCase)) {
				retType = "application";
				retMIME = "application/msword";
			} else if (-1 < extentionStr.IndexOf(".xls", StringComparison.OrdinalIgnoreCase)) {
				retType = "application";
				retMIME = "application/msexcel";
			} else if (-1 < extentionStr.IndexOf(".wmx", StringComparison.OrdinalIgnoreCase)) {
				retType = "application";        //ver9:Windows Media Player スキン 
			} else if (-1 < extentionStr.IndexOf(".wms", StringComparison.OrdinalIgnoreCase)) {
				retType = "application";       //ver9:Windows Media Player スキン  
			} else if (-1 < extentionStr.IndexOf(".wmz", StringComparison.OrdinalIgnoreCase)) {
				retType = "application";       //ver9:Windows Media Player スキン  
			} else if (-1 < extentionStr.IndexOf(".wpl", StringComparison.OrdinalIgnoreCase)) {
				retType = "application";       //ver9:Windows Media Player スキン  
			} else if (-1 < extentionStr.IndexOf(".wmd", StringComparison.OrdinalIgnoreCase)) {
				retType = "application";       //ver9:Windows Media Download パッケージ   
											   /*		} else if (-1 < extentionStr.IndexOf(".m3u", StringComparison.OrdinalIgnoreCase)) {
														   retType = "video";*/

			} else if (-1 < extentionStr.IndexOf(".wm", StringComparison.OrdinalIgnoreCase)) {        //以降wmで始まる拡張子が誤動作
				retType = "video";
				retMIME = "video/x-ms-wm";
			}
			//	}
			typeName.Text = retType;
			mineType.Text = retMIME;
			return retType;
			//		MyLog( dbMsg );
			//		} catch (Exception er) {
			//		Console.WriteLine( TAG + "でエラー発生" + er.Message + ";" + dbMsg );
			//	}
		}       //拡張子からタイプとMIMEを返す

		/// <summary>
		/// テキストファイルをStreamReaderで読み込む
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="emCord"></param>
		/// <returns></returns>
		private string ReadTextFile(string fileName, string emCord) {
			string TAG = "[ReadTextFile]";
			string dbMsg = TAG;
			string retStr = "";
			try {
				dbMsg += ",fileName=" + fileName + ",emCord=" + emCord;
				StreamReader sr = new StreamReader(fileName, Encoding.GetEncoding(emCord));
				retStr = sr.ReadToEnd();
				sr.Close();
			} catch (Exception e) {
				Console.WriteLine(TAG + "でエラー発生" + e.Message + ";" + dbMsg);
			}
			MyLog(dbMsg);
			return retStr;
		}           //テキスト系ファイルの読込み	http://www.atmarkit.co.jp/ait/articles/0306/13/news003.html

		/// <summary>
		/// 指定された階層にあるアイテム数を返す
		/// ☆ボリューム直下対策
		/// </summary>
		/// <param name="passNameStr"></param>
		/// <returns></returns>
		private int CurrentItemCount(string passNameStr) {
			string TAG = "[CurrentItemCount]";
			string dbMsg = TAG;
			int retIntr = 0;
			try {
				dbMsg += ",対象階層=" + passNameStr;
				System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(passNameStr);
				//		dbMsg += "；Dir;;Attributes=" + dirInfo.Attributes;
				if (dirInfo.Parent != null) {
					//	System.IO.FileAttributes attr = System.IO.Path.GetAttributes(passNameStr);
					if ((dirInfo.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden) {
						dbMsg += ">>Hidden";
					} else if ((dirInfo.Attributes & System.IO.FileAttributes.System) == System.IO.FileAttributes.System) {
						dbMsg += ">>System";
					} else {

						dbMsg += ",Parent=" + dirInfo.Parent;//☆ドライブルートはこれで落ちる
						dbMsg += "フォルダ内";
						System.IO.FileInfo[] files = dirInfo.GetFiles("*", System.IO.SearchOption.AllDirectories);
						retIntr = files.Length;      // サブディレクトリ内のファイルもカウントする場合	, SearchOption.AllDirectories
													 //			System.IO.DirectoryInfo[] dirs = dirInfo.GetDirectories( "*", System.IO.SearchOption.AllDirectories );
													 //			retIntr += dirs.Length;
					}
				} else {
					dbMsg += "ドライブ確認";
					System.IO.DriveInfo driveInfo = new System.IO.DriveInfo(passNameStr);     //.Substring( 0, 1 )
					if (driveInfo.IsReady) {
						dbMsg += ">>ドライブ直下" + driveInfo.RootDirectory.ToString();
						var rootItems = Directory.EnumerateFiles(passNameStr);//.Where( x => !x.Contains( ( passNameStr + "System Volume Infomation") ) );
																			  //	dbMsg += ",rootItems=" + rootItems.All.ToString();
						foreach (var rootItem in rootItems) {
							dbMsg += "(" + retIntr + ")" + rootItem;
							//System Volume Infomation(復元ポイントが保存されている隠しフォルダ)にアクセスが発生して落ちる
							dirInfo = new System.IO.DirectoryInfo(rootItem);
							if ((dirInfo.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden) {
								dbMsg += ">>Hidden";
							} else if ((dirInfo.Attributes & System.IO.FileAttributes.System) == System.IO.FileAttributes.System) {
								dbMsg += ">>System";
							} else {

								if (-1 < rootItem.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
								-1 < rootItem.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase)) {
								} else {
									try {
										/*	string[] foleres = Directory.GetDirectories( rootItem );
											if (foleres != null) {
												dbMsg += "\nfoleres=" + foleres.Length + "件";
												foreach (string folereName in foleres) {
													//		if (-1 < folereName.IndexOf( "RECYCLE", StringComparison.OrdinalIgnoreCase ) ||
													//			-1 < folereName.IndexOf( "System Vol", StringComparison.OrdinalIgnoreCase )) {
													//		} else {
													dirInfo = new System.IO.DirectoryInfo( folereName );
													dbMsg += ";dirInfo=" + dirInfo.Attributes;
													if (dirInfo.Attributes.ToString() == "Directory") {
														System.IO.DirectoryInfo[] rootDirs = dirInfo.GetDirectories( "*", System.IO.SearchOption.AllDirectories );
														dbMsg += ",rootDirs=" + rootDirs.Length;
														retIntr += rootDirs.Length;
														System.IO.FileInfo[] rootFiles = dirInfo.GetFiles( "*", System.IO.SearchOption.AllDirectories );
														dbMsg += ",rootFiles=" + rootFiles.Length;
														retIntr += rootFiles.Length;      // サブディレクトリ内のファイルもカウントする場合	, SearchOption.AllDirectories
													} else {
														retIntr++;
													}
													//	}
												}           //ListBox1に結果を表示する
											}
											*/


										//	if (rootItem.ToString() != ( passNameStr + "System" + "*" )) {
										//		dirInfo = new System.IO.DirectoryInfo(rootItem);
										string dirAttributes = dirInfo.Attributes.ToString();
										dbMsg += ";dirInfo=" + dirAttributes;
										if (dirAttributes == "Directory") {
											System.IO.DirectoryInfo[] rootDirs = dirInfo.GetDirectories("*", System.IO.SearchOption.AllDirectories);
											dbMsg += ",rootDirs=" + rootDirs.Length;
											retIntr += rootDirs.Length;
											System.IO.FileInfo[] rootFiles = dirInfo.GetFiles("*", System.IO.SearchOption.AllDirectories);
											dbMsg += ",rootFiles=" + rootFiles.Length;
											retIntr += rootFiles.Length;      // サブディレクトリ内のファイルもカウントする場合	, SearchOption.AllDirectories
										} else {
											retIntr++;
										}
										/*=I:\an\workspace2015\参考資料\Android SDK逆引きハンドブック\sample\Chap-15\244\assets；Dir;;Attributes=Directory,Parent=244フォルダ内,
										 * このデレクトリには0件 マネージ デバッグ アシスタント 'ContextSwitchDeadlock' 
	CLR は、COM コンテキスト 0x6aa35230 から COM コンテキスト 0x6aa35108 へ 60 秒で移行できませんでした。ターゲット コンテキストおよびアパートメントを所有するスレッドが、ポンプしない待機を行っているか、Windows のメッセージを表示しないで非常に長い実行操作を処理しているかのどちらかです。この状態は通常、パフォーマンスを低下させたり、アプリケーションが応答していない状態および増え続けるメモリ使用を導く可能性があります。この問題を回避するには、すべての Single Thread Apartment (STA) のスレッドが、CoWaitForMultipleHandles のようなポンプする待機プリミティブを使用するか、長い実行操作中に定期的にメッセージをポンプしなければなりません。*/
										//									}
									} catch (Exception e) {
										dbMsg += "<<以降でエラー発生>>" + e.Message;
										MyLog(dbMsg);
										return retIntr;
										throw;
									}
								}
							}
						}//for
					}
				}
				dbMsg += ",このデレクトリには" + retIntr + "件";
				MyLog(dbMsg);
			} catch (Exception e) {
				dbMsg += "<<以降でエラー発生>>" + e.Message;
				MyLog(dbMsg);
			}
			return retIntr;
		}

		/// <summary>
		/// フォルダの中身をリストアップ
		///		フリーズ発生
		/// </summary>
		/// <param name="sarchDir"></param>
		/// <returns></returns>
		private List<string> GetFolderFiles(string sarchDir) {
			string TAG = "[GetFolderFiles]";
			string dbMsg = TAG;
			List<string> retItems = new List<string>();
			try {
				dbMsg += "sarchDir=" + sarchDir;                    //sender=System.Windows.Forms.TreeView, Nodes.Count: 5, Nodes[0]: TreeNode: C:\,
				IEnumerable<string> files = Directory.EnumerateFiles(sarchDir, "*"); // サブ・ディレクトも含める	, System.IO.SearchOption.AllDirectories
				foreach (string fileName in files) {
					dbMsg += "(" + retItems.Count + ")" + fileName;
					string[] extStrs = fileName.Split('.');
					string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
					if (-1 < Array.IndexOf(systemFiles, extentionStr) ||
						0 < fileName.IndexOf("BOOTNXT", StringComparison.OrdinalIgnoreCase) ||
						0 < fileName.IndexOf("-ms", StringComparison.OrdinalIgnoreCase) ||
						0 < fileName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase)
						) {
					} else {
						retItems.Add(fileName);
					}
				}
				/*		try {
							List<string> folders = new List<string>( Directory.EnumerateDirectories( sarchDir ) );

							//	IEnumerable<string> folders = Microsoft.VisualBasic.FileIO.FileSystem.GetDirectories( sarchDir );
							//	IEnumerable<string> folders = Directory.EnumerateDirectories( sarchDir, "*" ).Where( x => !x.Contains( ( sarchDir + "System Volume Infomation" ) ) );

							// サブ・ディレクトも含める	, System.IO.SearchOption.AllDirectories
							dbMsg += "," + folders.Count() + "件";
							foreach (string folereName in folders) {
								dbMsg += "," + folereName;
								if (-1 < folereName.IndexOf( "RECYCLE", StringComparison.OrdinalIgnoreCase ) ||
										-1 < folereName.IndexOf( "System Vol", StringComparison.OrdinalIgnoreCase )) {      // 'M:\System Volume Information' へのアクセスが拒否されました。
								} else {
									dbMsg += "sarchDir=" + sarchDir;                    //sender=System.Windows.Forms.TreeView, Nodes.Count: 5, Nodes[0]: TreeNode: C:\,
									files = Directory.EnumerateFiles( sarchDir, "*", System.IO.SearchOption.AllDirectories ); // サブ・ディレクトも含める	
									foreach (string file in files) {
										dbMsg += "(" + retItems.Count + ")" + file;
										retItems.Add( file );
									}
								}
							}
						} catch (UnauthorizedAccessException UAEx) {
							dbMsg += "<<以降でエラー発生>>" + UAEx.Message;
							MyLog( dbMsg );
							throw;
						} catch (PathTooLongException PathEx) {
							dbMsg += "<<以降でエラー発生>>" + PathEx.Message;
							MyLog( dbMsg );
							throw;
						} catch (Exception er) {
							dbMsg += "<<以降でエラー発生>>" + er.Message;
							MyLog( dbMsg );
							throw;
						}*/
				dbMsg += ",結果" + retItems.Count + "件";
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
				throw;
			}
			return retItems;
		}

		/// <summary>
		/// 指定した階層以下のファイルのフルパス名をListにして返す
		/// </summary>
		/// <param name="sarchDir"></param>
		/// <returns></returns>
		private List<string> GetFolderItems(string sarchDir, List<string> retItems) {
			string TAG = "[GetFolderItems]";
			string dbMsg = TAG;
			try {
				dbMsg += "sarchDir=" + sarchDir;
				string[] files = Directory.GetFiles(sarchDir);
				dbMsg += "," + files.Length + "件";
				if (files != null) {
					foreach (string fileName in files) {
						string[] extStrs = fileName.Split('.');
						string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
						if (-1 < Array.IndexOf(systemFiles, extentionStr) ||
							0 < fileName.IndexOf("BOOTNXT", StringComparison.OrdinalIgnoreCase) ||
							0 < fileName.IndexOf("-ms", StringComparison.OrdinalIgnoreCase) ||
							0 < fileName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase)
							) {
						} else {
							dbMsg += "(" + retItems.Count + ")" + fileName;
							retItems.Add(fileName);
						}
					}
				}
				string[] foleres = Directory.GetDirectories(sarchDir);
				if (foleres != null) {
					dbMsg += "\nfoleres=" + foleres.Length + "件";
					foreach (string folereName in foleres) {
						if (-1 < folereName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
							-1 < folereName.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase)) {
						} else {
							/*	List<string> retItems2 = GetFolderFiles( folereName, retItems );
								dbMsg += ",retItems2=" + retItems2.Count + "件";
								for (int i = 0; i < retItems2.Count; ++i) {
									retItems.Add( retItems2[i] );
								}*/
						}
					}           //ListBox1に結果を表示する
				}
				MyLog(dbMsg);
			} catch (UnauthorizedAccessException UAEx) {
				dbMsg += "<<以降でエラー発生>>" + UAEx.Message;
				//	MyLog( dbMsg );
				throw;
			} catch (PathTooLongException PathEx) {
				dbMsg += "<<以降でエラー発生>>" + PathEx.Message;
				//	MyLog( dbMsg );
				throw;
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				//	MyLog( dbMsg );
				throw;
			}
			return retItems;
		}

		////各プレイヤーの生成/////////////////////////////////////////////////////////////////ファイル操作///
		////web/////////////////////////////////////////////////////////////////ファイル操作///

		/*		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
				{
					string selectItem = listBox1.SelectedItem.ToString();
					FileInfo fi = new FileInfo( selectItem );
					String infoStr = ",Exists;";
					infoStr += fi.Exists;
					infoStr += ",拡張子;";
					infoStr += fi.Extension;
					infoStr += "作成;";
					infoStr += fi.CreationTime;
					infoStr += ",アクセス;";
					infoStr += fi.LastAccessTime;
					infoStr += ",更新;";
					infoStr += fi.LastWriteTime;
					if (fi.Exists) {
						infoStr += ",ファイルサイズ;";
						infoStr += fi.Length;
					} else {
						MakeFolderList( selectItem );
					}
					infoStr += ",絶対パス;";
					infoStr += fi.FullName;//       
					infoStr += ",ファイル名;";
					infoStr += fi.Name;
					infoStr += ",親ディレクトリ;";
					infoStr += fi.Directory;//     
					infoStr += ",親ディレクトリ名;";
					infoStr += fi.DirectoryName;
					fileinfo.Text = infoStr;
				}           //リストアイテムのクリック
				*/
		private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
			string TAG = "[WebBrowser1_DocumentCompleted]";
			string dbMsg = TAG;
			try {
				HtmlDocument wDoc = playerWebBrowser.Document;
				string wText = playerWebBrowser.DocumentText;
				/*
								if (( wText.Contains( "object" ) ) || ( wText.Contains( "embed" ) )) {
									HtmlElement playerElem = playerWebBrowser.Document.GetElementById( wiPlayerID );
									playerElem.AttachEventHandler( "PlayStateChangeEvent", new EventHandler( PlayStateChangeEvent ) );     //PlayState.MediaEnded		CurrentState 
									dbMsg += ",Controls=" + playerWebBrowser.Controls;
									dbMsg += ",ReadyState=" + playerWebBrowser.ReadyState;
								}
				*/
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		private string MakeVideoSouce(string fileName, int webWidth, int webHeight) {
			string TAG = "[MakeVideoSouce]";
			string dbMsg = TAG;
			dbMsg += ",lsFullPathName=" + lsFullPathName;
			dbMsg += ",fileName=" + fileName;
			string contlolPart = "";
			string comentStr = "このタイプの表示は検討中です。";
			string[] extStrs = fileName.Split('.');
			string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
			string[] souceNames = fileName.Split(Path.DirectorySeparatorChar);
			string souceName = souceNames[souceNames.Length - 1];
			string mineTypeStr = mineType.Text;//	"video/x-ms-asf";     //.asf
			string clsId = "";
			string codeBase = "";
			string dbWorning = "";

			if (lsFullPathName != "" && fileName != "未選択" && lsFullPathName != fileName) {       //8/31;仮対応；書き換わり対策
				dbMsg += ",***書き換わり発生*<<" + lsFullPathName + " ; " + fileName + ">>";
				fileName = lsFullPathName;
			}

			if (extentionStr == ".webm" ||
				extentionStr == ".ogv"
				) {
				contlolPart += "\t\t\t<meta http - equiv = " + '"' + "X-UA-Compatible" + '"' + " content=" + '"' + "chrome=1" + '"' + " >\n";
				//<meta http-equiv="X-UA-Compatible" content="chrome=1">			http://mrs.suzu841.com/mini_memo/numero_23.html
				contlolPart += "\t\t</head>\n";
				contlolPart += "\t\t<body style = " + '"' + "background-color: #000000;color:#ffffff;" + '"' + " >\n";
				//	contlolPart += "\t\t<div class=" + '"' + "video-container" + '"' + ">\n";
				contlolPart += "\t\t\t<video id=" + '"' + wiPlayerID + '"' + " controls autoplay style = " + '"' + "width:100%;height: auto;" + '"' + ">\n";
				contlolPart += "\t\t\t\t<source src=" + '"' + "file://" + fileName + '"' + " type=" + "'" + mineTypeStr;
				if (extentionStr == ".webm") {
					contlolPart += "; codecs=" + '"' + "vp8, vorbis" + '"' + "'" + ">\n";
				} else if (extentionStr == ".ogv") {
					contlolPart += "; codecs=" + '"' + "theora, vorbis" + '"' + "'" + ">\n";
				}
				// "file://" +		//  <source src="movie.webm" type='video/webm; codecs="vp8, vorbis"' />
				/*		contlolPart += "\t\t\t<video id=" + '"' + wiPlayerID + '"' + " src=" + '"' + "file://" + fileName + '"' +
													" controls autoplay style = " + '"' + "width:100%;height: auto;" + '"' +
														"></video>\n\t\t</div>";          */
				contlolPart += "\t\t\t</video>\n";
				//		contlolPart += "\t\t</div>"; 
				comentStr = "読み込めないファイルは対策検討中です。。";

			} else if (extentionStr == ".flv" ||
				extentionStr == ".f4v" ||
				extentionStr == ".swf"
				) {
				Uri urlObj = new Uri(fileName);
				if (urlObj.IsFile) {             //Uriオブジェクトがファイルを表していることを確認する
					fileName = urlObj.AbsoluteUri;                 //Windows形式のパス表現に変換する
					dbMsg += "Path=" + fileName;
				}
				dbMsg += ",assemblyPath=" + assemblyPath + ",assemblyName=" + assemblyName;
				dbMsg += ",playerUrl=" + playerUrl;//,playerUrl=C:\Users\博臣\source\repos\file_tree_clock_web1\file_tree_clock_web1\bin\Debug\fladance.swf 
				clsId = "clsid:d27cdb6e-ae6d-11cf-96b8-444553540000";       //ブラウザーの ActiveX コントロール
				codeBase = "http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=10,0,0,0";
				string pluginspage = "http://www.macromedia.com/go/getflashplayer";
				dbMsg += "[" + webWidth + "×" + webHeight + "]";        //4/3=1.3		1478/957=1.53  801/392=2.04
																		/*		if (4 / 3 < webWidth / webHeight) {
																					webWidth = webHeight/3*4;		.
																				} else {
																					webWidth = webHeight / 3 * 4;
																				}
																				dbMsg += ">>[" + webWidth + "×" + webHeight + "]";*/
				playerUrl = assemblyPath.Replace(assemblyName, "fladance.swf");       //☆デバッグ用を\bin\Debugにコピーしておく
																					  //		string nextMove = assemblyPath.Replace( assemblyName, "tonext.htm" );
				string flashVvars = "fms_app=&video_file=" + fileName + "&" +       // & amp;
																					//								"link_url ="+ nextMove + "&" +
										 "image_file=&link_url=&autoplay=true&mute=false&controllbar=true&buffertime=10" + '"';
				contlolPart += "\t</head>\n";
				contlolPart += "\t<body style = " + '"' + "background-color: #000000;color:#ffffff;" + '"' + " >\n\t\t";
				contlolPart += "<object id=" + '"' + wiPlayerID + '"' +
									" classid=" + '"' + clsId + '"' +
								" codebase=" + '"' + codeBase + '"' +
								" width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
								 ">\n";
				contlolPart += "\t\t\t<param name=" + '"' + "FlashVars" + '"' + " value=" + '"' + flashVvars + '"' + "/>\n";                        //常にバーを表示する
				contlolPart += "\t\t\t<param name= " + '"' + "allowFullScreen" + '"' + " value=" + '"' + "true" + '"' + "/>\n";
				contlolPart += "\t\t\t<param name =" + '"' + "movie" + '"' + " value=" + '"' + playerUrl + '"' + "/>\n";
				contlolPart += "\t\t\t\t<embed name=" + '"' + wiPlayerID + '"' +
												" src=" + '"' + playerUrl + '"' +            // "file://" + fileName
												" width=" + '"' + webWidth + '"' + " height= " + '"' + webHeight + '"' +
												" type=" + '"' + mineTypeStr + '"' +
												" allowfullscreen=" + '"' + " true= " + '"' +
												" flashvars=" + '"' + flashVvars + '"' +
												" type=" + '"' + "application/x-shockwave-flash" + '"' +
												" pluginspage=" + '"' + pluginspage + '"' +
									   "/>\n";

				comentStr = souceName + " ; プレイヤーには「ふらだんす」http://www.streaming.jp/fladance/　を使っています。" + dbWorning;


				//		fileName = fileName.Replace((":" + Path.DirectorySeparatorChar), ":" + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar);
				//		fileName = fileName.Replace((":" + Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, ":" + Path.DirectorySeparatorChar);
				//		dbMsg += "Path=" + fileName;
				/*				playerUrl = assemblyPath.Replace(assemblyName, "flvplayer-305.swf");       //☆デバッグ用を\bin\Debugにコピーしておく
																										   //	string flashVvars = "fms_app=&video_file=" + fileName + "&" +       // & amp;
								contlolPart += "<object id=" + '"' + wiPlayerID + '"' +
																					" width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
																					" classid=" + '"' + clsId + '"' +
																				" codebase=" + '"' + codeBase + '"' +
																					 //	" type=" + '"' + "application/x-shockwave-flash" + '"' +
																					 //						" data=" + '"' + playerUrl + '"' +
																					 ">\n";
								contlolPart += "\t\t\t<param name =" + '"' + "movie" + '"' + " value=" + '"' + playerUrl + '"' + "/>\n";
								contlolPart += "\t\t\t<param name=" + '"' + "allowFullScreen" + '"' + " value=" + '"' + "true" + '"' + "/>\n";
								contlolPart += "\t\t\t<param name=" + '"' + "FlashVars" + '"' + " value=" + '"' + fileName + '"' + "/>\n";
								contlolPart += "\t\t\t\t<embed name=" + '"' + wiPlayerID + '"' +
																" width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
																" src=" + '"' + playerUrl + '"' +
																" flashvars=" + '"' + fileName + '"' +           //" flashvars=" + '"' + @"flv=" + fileName + +'"' +
																" allowFullScreen=" + '"' + "true" + '"' +
													   ">\n";
								contlolPart += "\t\t\t\t</ embed>\n";
								comentStr = souceName + " ; プレイヤーには「Adobe Flash Player」https://www.mi-j.com/service/FLASH/player/index.html　を使っています。";
				*/

				/*
				playerUrl = assemblyPath.Replace( assemblyName, "player_flv_maxi.swf" );       //☆デバッグ用を\bin\Debugにコピーしておく
								contlolPart += "<object type=" + '"' + "application/x-shockwave-flash" + '"' +
																			" data=" + '"' + playerUrl + '"' +
																" width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
																 ">\n";
								contlolPart += "\t\t\t<param name =" + '"' + "movie" + '"' + " value=" + '"' + playerUrl + '"' + "/>\n";
								contlolPart += "\t\t\t<param name=" + '"' + "allowFullScreen" + '"' + " value=" + '"' + "true" + '"' + "/>\n";
								contlolPart += "\t\t\t<param name=" + '"' + "FlashVars" + '"' + " value=" + '"' + fileName + "&" +
																				 "width=" + webWidth + "&" +
																				 "height=" + webHeight + "&" +
																				 "showstop=" + 1 + "&" +          //ストップボタンを表示
																				 "showvolume=" + 1 + "&" +
																				 "showtime=" + 1 + "&" +
																				 "showfullscreen=" + 1 + "&" +
																									 "showplayer = always" +
									'"' + "/>\n";
								contlolPart += "\t\t\t\t<embed name=" + '"' + "monFlash" + '"' +
																" src=" + '"' + playerUrl + '"' +            // "file://" + fileName
																" flashvars=" + '"' + @"flv=" + fileName + +'"' +
																" pluginspage=" + '"' + pluginspage + '"' +
																" type=" + '"' + "application/x-shockwave-flash" + '"' +
													   "/>\n";
								comentStr = souceName + " ; プレイヤーには「FLV Player」http://flv-player.net/　を使っています。";


												playerUrl = assemblyPath.Replace( assemblyName, "flaver.swf" );       //☆デバッグ用を\bin\Debugにコピーしておく
						contlolPart += "<object id=" + '"' + "flvp" + '"' +
															" data=" + '"' + playerUrl + '"' +
														" type=" + '"' + "application/x-shockwave-flash" + '"' +
														" width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
														" ALIGN=" + '"' + "right" + '"' +
														 ">\n";
										contlolPart += "\t\t\t<param name =" + '"' + "movie" + '"' + " value=" + '"' + fileName + '"' + "/>\n";
										contlolPart += "\t\t\t<param name=" + '"' + "FlashVars" + '"' + " value=" + '"' + fileName + '"' + "/>\n";
										contlolPart += "\t\t\t<param name= " + '"' + "allowFullScreen" + '"' + " value=" + '"' + "true" + '"' + "/>\n";
										contlolPart += "\t\t\t<param name= " + '"' + "allowScriptAccess" + '"' + " value=" + '"' + "always" + '"' + "/>\n";
										comentStr = souceName + " ; プレイヤーには「FLAVER 3.0」http://rexef.com/webtool/flaver3/installation.html　を使っています。";

	
										*/
				/*-		//		fileName = "media.flv";
							//	fileName = "file:///"+fileName.Replace( Path.DirectorySeparatorChar ,'/');
								codeBase = "http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=9,0,115,0";      //26,0,0,151は？
								contlolPart += "<object id=" + '"' + "flvp" + '"' +
													//			" type=" + '"' + mineTypeStr + '"' +
														//		 " data=" + '"' + fileName + //"&fullscreen=true" + '"' +
														//		 " data=" + '"' + "player_flv_mini.swf" + '"' +
													" classid=" + '"' + clsId + '"' +
												" codebase=" + '"' + codeBase + '"' +
												" width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
										//		" menu=" + true  +
												 ">\n";
							//	contlolPart += "\t\t\t<param name =" + '"' + "movie" + '"' + " value=" + '"' + "flvplayer-305.swf" + '"' + "/>\n";
								contlolPart += "\t\t\t<param name =" + '"' + "bgcolor" + '"' + " value=" + '"' + "#FFFFFF" + '"' + "/>\n";
								//		contlolPart += "\t\t\t<param name= " + '"' + "bgcolor" + '"' + " value=" + '"' + "#fff" + '"' + "/>\n";
								contlolPart += "\t\t\t<param name =" + '"' + "loop" + '"' + " value=" + '"' + "false" + '"' + "/>\n";
								contlolPart += "\t\t\t<param name =" + '"' + "quality" + '"' + " value=" + '"' + "high" + '"' + "/>\n";
								contlolPart += "\t\t\t<param name =" + '"' + "menu" + '"' + " value=" + '"' + "true" + '"' + "/>\n";
								//		contlolPart += "\t\t\t<param name =" + '"' + "allowScriptAccess" + '"' + " value = " + '"' + "sameDomain" + '"' + "/>\n";
							//	contlolPart += "\t\t\t<param name= " + '"' + "allowScriptAccess" + '"' + " value=" + '"' + "always" + '"' + "/>\n";
									contlolPart += "\t\t\t<param name=" + '"' + "FlashVars" + '"' + " value=" + '"' +
																		"src=" + fileName + "&" +       // & amp;
															 //			"flvmov=" + fileName + "&" +       // & amp;
															 //		"flv=" + fileName +"&" +       // & amp;
															 "width=" + webWidth + "&" + "height=" + webHeight + "&" +
															 "showstop=" + 1 + "&" +                              //ストップボタンを表示
															 "showvolume=" + 1 + "&" +                            //showvolume
															 "showtime=" + 1 + "&" +                              //時間を表示
															 "showfullscreen=" + 1 + "&" +                        //全画面表示ボタンを表示
															 "showplayer=always" + '"' + "/>\n";                        //常にバーを表示する


								contlolPart += "\t\t\t\t<embed name=" + '"' + "flvp" + '"' +
																" type=" + '"' + mineTypeStr + '"' +
																" src=" + '"' + fileName + '"' +            // "file://" + fileName
																											//		" allowScriptAccess=" + '"' + " sameDomain= " + '"' +
																" width=" + '"' + webWidth + '"' + " height= " + '"' + webHeight + '"' + " bgcolor=" + '"' + "#FFFFFF" + '"' +
																" pluginspage=" + '"' + pluginspage + '"' + 
																" loop=" + '"' + "false" + '"' + " quality=" + '"' + "high" + '"' +
													   "/>\n";
				*/
				//グローバルセキュリティ設定パネルで)「これらの場所にあるファイルを常に信頼する」で、[追加]-[フォルダを参照]にローカルディスクを登録する？
				//	http://www.macromedia.com/support/documentation/jp/flashplayer/help/settings_manager04.html
				//属性指定は	https://helpx.adobe.com/jp/flash/kb/231465.html
				//C#でFlashファイルを読み込み表示する	http://sivaexstrage.orz.hm/blog/softwaredevelopment/800
				//		contlolPart += "\n\t\t< param name = " + '"' + "FlashVars" + '"' + "value = " + '"' + "flv= + '"' +fileName + '"' +"&autoplay=1&margin=0" + '"' + "/>\n\t\t\t";
				contlolPart += "\t\t</object>\n";
			} else if (extentionStr == ".rm") {
				contlolPart += "\t</head>\n";
				contlolPart += "\t<body style = " + '"' + "background-color: #000000;color:#ffffff;" + '"' + " >\n";
				clsId = "clsid:CFCDAA03-8BE4-11CF-B84B-0020AFBBCCFA";       //ブラウザーの ActiveX コントロール
				contlolPart += "\t\t<object  id=" + '"' + wiPlayerID + '"' +
									"  classid=" + '"' + clsId + '"' +
									" width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
								 ">\n";
				contlolPart += "\t\t\t<param name =" + '"' + "src" + '"' + " value=" + '"' + fileName + '"' + "/>\n";
				contlolPart += "\t\t\t<param name =" + '"' + "AUTOSTART" + '"' + " value=" + '"' + "TRUEF" + '"' + "/>\n";
				contlolPart += "\t\t\t<param name =" + '"' + "CONTROLS" + '"' + " value=" + '"' + "All" + '"' + "/>\n"; //http://www.tohoho-web.com/wwwmmd3.htm
				contlolPart += "\t\t</object>\n";
			} else if (extentionStr == ".wmv" ||        //ver9:Windows Media 形式
				extentionStr == ".asf" ||
				extentionStr == ".wm" ||
				extentionStr == ".asx" ||        //ver9:Windows Media メタファイル 
				extentionStr == ".wax" ||        //ver9:Windows Media メタファイル 
				extentionStr == ".wvx" ||        //ver9:Windows Media メタファイル 
				extentionStr == ".wmx" ||        //ver9:Windows Media メタファイル 
				extentionStr == ".ivf" ||        //ver10:Indeo Video Technology
				extentionStr == ".dvr-ms" ||        //ver12:Microsoft デジタル ビデオ録画
				extentionStr == ".m2ts" ||        //ver12:MPEG-2 TS ビデオ ファイル 
				extentionStr == ".ts" ||
				extentionStr == ".mpg" ||
				extentionStr == ".m1v" ||
				extentionStr == ".mp2" ||
				extentionStr == ".mpa" ||
				extentionStr == ".mpe" ||
				extentionStr == ".mp4" ||        //ver12:MP4 ビデオ ファイル 
				extentionStr == ".m4v" ||
				extentionStr == ".mp4" ||
				extentionStr == ".mp4v" ||
				extentionStr == ".mpeg" ||
				extentionStr == ".mpeg" ||
				extentionStr == ".mpeg" ||
				extentionStr == ".3gp" ||
				extentionStr == ".3gpp" ||
				extentionStr == ".qt" ||
				extentionStr == ".mov"       //ver12:QuickTime ムービー ファイル 
				) {
				/*	contlolPart += "\t\t\t<script type=" + '"' + "text/javascript" + '"' + " > \n";

					contlolPart += "\t\t\t</script>\n\n";

				contlolPart += "\t\t\t<script for=" + '"' + wiPlayerID + '"' +            //"MediaPlayer"		document.getElementById( )
												" event=" + '"' + "PlayStateChange(lOldState, lNewState)" + '"' +
												" type=" + '"' + "text/javascript" + '"' + ">\n";

				contlolPart += "\t\t\t\tdocument.getElementById(" + '"' + wiPlayerID + '"' + " ).PlayStateChanged = function( old_state, new_state ){\n" +
								"\t\t\t\t\t var comentStr =" + "new_state" + ";\n" +
								"\t\t\t\t\t switch (new_state) {\n" +
								"\t\t\t\t\t\tcase 0:\n" +
								"\t\t\t\t\t\t comentStr =" + '"' + "Windows Media Player の状態が定義されません。" + '"' + "\n" +
								"\t\t\t\t\t\tbreak;\n" +
								"\t\t\t\t\t\tcase 8:\n" +
								"\t\t\t\t\t\t comentStr =" + '"' + "メディアの再生が完了し、最後の位置にあります。" + '"' + "\n" +
								"\t\t\t\t\t\tbreak;\n" +
								"\t\t\t\t\t}\n" +
								"\t\t\t\t\t alert( " +  "comentStr" + " );\n" +         // it_dispRate.value = mplayer.Rate;	 '"' + "+new_state" + 
								"\t\t\t\t\t document.getElementById(" + '"' + "statediv" + '"' + ").innerHTML = "  + "comentStr"  + ";\n" +
								"\t\t\t\t" + "}\n" +
								"\t\t\t</script>\n\n";          //https://msdn.microsoft.com/ja-jp/library/cc364798.aspx

					contlolPart += "\t\t\t<script for=" + '"' + wiPlayerID + '"' + " event=" + '"' + "EndOfStream(lResult)" + '"' + 
										"\t\t\t\t type=" + '"' + "text/javascript" + '"' + ">\n" +
														//		"\t\t\t\t\t alert( " + '"' + "EndOfStream" + '"' + " );\n" +
										"\t\t\t\t\t document.getElementById(" + '"' + "statediv" + '"' + ").innerHTML = " +'"' + "次へ" + '"' + ";\n" +
										"\t\t\t</script>\n\n";          //http://www.tohoho-web.com/wwwmmd2.htm
					*/
				contlolPart += "\t\t</head>\n";
				contlolPart += "\t\t<body style = " + '"' + "background-color: #000000;color:#ffffff;" + '"' + " >\n";
				clsId = "CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6";   //Windows Media Player9
				contlolPart += "\t\t\t<object classid =" + '"' + clsId + '"' + " id=" + '"' + wiPlayerID + '"' + "  width = " + '"' + webWidth + '"' + " height = " + '"' + webHeight + '"' + ">\n";
				contlolPart += "\t\t\t\t<param name =" + '"' + "url" + '"' + "value = " + '"' + "file://" + fileName + '"' + "/>\n";
				contlolPart += "\t\t\t\t<param name =" + '"' + "stretchToFit" + '"' + " value = true />\n";//右クリックして縮小/拡大で200％
				contlolPart += "\t\t\t\t<param name =" + '"' + "autoStart" + '"' + " value = " + true + "/>\n";
				contlolPart += "\t\t\t</object>\n";
				comentStr = souceName + " ; " + "Windows Media Player読み込めないファイルは対策検討中です。";
				///参照 http://so-zou.jp/web-app/tech/html/sample/embed-video.htm/////
				/////https://support.microsoft.com/ja-jp/help/316992/file-types-supported-by-windows-media-player
			} else {
				comentStr = "この形式は対応確認中です。";
			}
			/*		contlolPart += "\t\t\t<input type=" + '"' + "button" + '"' + " value=" + '"' + "開始" + '"' + " onclick=" + '"' + wiPlayerID + ".play()" + '"' + ">\n" +
									"\t\t\t<input type=" + '"' + "button" + '"' + " value=" + '"' + "停止" + '"' + " onclick=" + '"' + wiPlayerID + ".stop();" + wiPlayerID + " .CurrentPosition=0;" + '"' + ">\n" +
									"\t\t\t<input type=" + '"' + "button" + '"' + " value=" + '"' + "一時停止" + '"' + " onclick=" + '"' + wiPlayerID + ".pause()" + '"' + ">\n";
		*/
			contlolPart += "\t\t\t<span id =" + '"' + "statediv" + '"' + ">" + '"' + souceName + '"' + "</span>\n";

			MyLog(dbMsg);
			return contlolPart;
		}           //Video用のタグを作成

		private string MakeImageSouce(string fileName, int webWidth, int webHeight) {
			string TAG = "[MakeImageSouce]";
			string dbMsg = TAG;
			string contlolPart = "";
			string comentStr = "";
			string[] extStrs = fileName.Split('.');
			string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
			contlolPart += "\t</head>\n";
			contlolPart += "\t<body style = " + '"' + "background-color: #000000;color:#ffffff;" + '"' + " >\n\t\t";
			if (extentionStr == ".jpg" ||
				extentionStr == ".jpeg" ||
				extentionStr == ".png" ||
				extentionStr == ".gif"
				) {
			} else {
				/*	 ".tif", ".ico", ".bmp" };*/
				comentStr = "静止画はimgタグで読めるもののみ対応しています。";
			}
			contlolPart += "\n\t\t<img src = " + '"' + fileName + '"' + " style=" + '"' + "width:100%" + '"' + "/>\n";
			// + '"' + webWidth + '"' + " height = " + '"' + webHeight + '"' +
			contlolPart += "\t\t<div>\n\t\t\t" + comentStr + "\n\t\t</div>\n";
			MyLog(dbMsg);
			return contlolPart;
		}  //静止画用のタグを作成

		private string MakeAudioSouce(string fileName, int webWidth, int webHeight) {
			string TAG = "[MakeAudioSouce]";
			string dbMsg = TAG;
			string contlolPart = "";
			contlolPart += "\t</head>\n";
			contlolPart += "\t<body style = " + '"' + "background-color: #000000;color:#ffffff;" + '"' + " >\n\t\t";
			string comentStr = "";
			string[] extStrs = fileName.Split('.');
			string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
			string[] souceNames = fileName.Split(Path.DirectorySeparatorChar);
			string souceName = souceNames[souceNames.Length - 1];

			if (
				extentionStr == ".ogg"
				) {
				//		contlolPart += "<div class=" + '"' + "video-container" + '"' + ">\n";
				contlolPart += "\t\t\t<audio src=" + '"' + "file://" + fileName + '"' + " controls autoplay style = " + '"' + "width:100%" + '"' + " />\n";
				comentStr = "audioタグで読み込めないファイルは対策検討中です。。";
			} else if (extentionStr == ".ra") {
				string clsId = "clsid:CFCDAA03-8BE4-11CF-B84B-0020AFBBCCFA";       //ブラウザーの ActiveX コントロール
				contlolPart += "<objec id=" + '"' + wiPlayerID + '"' +
									"  classid=" + '"' + clsId + '"' +
									" width=" + '"' + webWidth + '"' + " height=" + '"' + webHeight + '"' +
								 ">\n";
				contlolPart += "\t\t\t<param name =" + '"' + "src" + '"' + " value=" + '"' + fileName + '"' + "/>\n";
				contlolPart += "\t\t\t<param name =" + '"' + "AUTOSTART" + '"' + " value=" + '"' + "TRUEF" + '"' + "/>\n";
				//	contlolPart += "\t\t\t<param name =" + '"' + "CONTROLS" + '"' + " value=" + '"' + "All" + '"' + "/>\n"; //http://www.tohoho-web.com/wwwmmd3.htm
			} else if (extentionStr == ".wma" ||
				extentionStr == ".wvx" ||
				extentionStr == ".wax" ||
				extentionStr == ".wav" ||
				extentionStr == ".m4a" ||           //var12;MP4 オーディオ ファイル
				extentionStr == ".mp3" ||
				extentionStr == ".aac" ||
				extentionStr == ".m4a" ||           //iTurne				extentionStr == ".midi" ||           //var9;MIDI 
				extentionStr == ".mid" ||           //var9;MIDI 
				extentionStr == ".rmi" ||           //var9;MIDI 
				extentionStr == ".aif" ||           //var9;Audio Interchange File FormatI 
				extentionStr == ".aifc" ||           //var9;Audio Interchange File FormatI 
				extentionStr == ".aiff" ||           //var9;Audio Interchange File FormatI 
				extentionStr == ".au" ||           //var9;Sun Microsystems および NeXT  
				extentionStr == ".snd" ||           //var9;Sun Microsystems および NeXT  
				extentionStr == ".wav" ||           //var9;Windows 用オーディオ   
				extentionStr == ".cda" ||           //var9;CD オーディオ トラック 
				extentionStr == ".adt" ||           //var12;Windows オーディオ ファイル 
				extentionStr == ".adts" ||           //var12;Windows オーディオ ファイル 
				extentionStr == ".asx"
				) {
				string clsId = "CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6";   //Windows Media Player9
				contlolPart += "\n\t\t<div><object id=" + '"' + wiPlayerID + '"' +
									"  classid =" + '"' + clsId + '"' + " style = " + '"' + "width:100%;higth :90%" + '"' + " >\n";
				contlolPart += "\t\t\t<param name =" + '"' + "url" + '"' + "value = " + '"' + "file://" + fileName + '"' + "/>\n";
				contlolPart += "\t\t\t<param name =" + '"' + "stretchToFit" + '"' + " value = true />\n";//右クリックして縮小/拡大で200％
				contlolPart += "\t\t\t<param name =" + '"' + "autoStart" + '"' + " value = " + true + "/></div>\n<br><div style=" + '"' + "top:96%" + '"' + ">";
				comentStr = "\t\t\t<pre>" + souceName + " " + " ; Windows Media Player読み込めないファイルは対策検討中です。</pre></div>\n";
				//この行が表示されない
				/*		contlolPart += "<ASX VERSION =" + '"' + "3.0"  + '"' + " >\n";
						contlolPart += "\t\t<ENTRY >\n";
						contlolPart += "\t\t\t<REF HREF =" + '"' +  fileName + '"' + " >\n";//"file://" +
						contlolPart += "\t\t\t</ENTRY >\n";
						contlolPart += "\t\t\t</ASX >\n";
						  comentStr = "ASXタグで確認中です。(Windows Media Player　がサポートしている形式)";*/
			} else {
				/* ".ra", ".flac",  }; */
				comentStr = "このファイルの再生方法は確認中です。";
			}
			contlolPart += "\t\t<div>\n\t\t\t" + comentStr + "\n\t\t</div>\n";
			MyLog(dbMsg);
			return contlolPart;
		}  //静止画用のタグを作成


		private string MakeTextSouce(string fileName, int webWidth, int webHeight) {
			string TAG = "[MakeTextSouce]";
			string dbMsg = TAG;
			string contlolPart = "";
			string comentStr = "";
			dbMsg += ",fileName=" + fileName;

			string rText = ReadTextFile(fileName, "UTF-8"); //"Shift_JIS"では文字化け発生
			dbMsg += ",rText=" + rText;

			string[] extStrs = fileName.Split('.');
			string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
			contlolPart += "\t\t<pre>\n";
			if (extentionStr == ".htm" ||
				extentionStr == ".html" ||
				extentionStr == ".xhtml" ||
				extentionStr == ".xml" ||
				extentionStr == ".rss" ||
				extentionStr == ".xml" ||
				extentionStr == ".css" ||
				extentionStr == ".js" ||
				extentionStr == ".vbs" ||
				extentionStr == ".cgi" ||
				extentionStr == ".php"
				) {
				rText = rText.Replace("<", "&lt;");
				rText = rText.Replace(">", "&gt;");
				contlolPart += rText;
			} else if (extentionStr == ".txt") {
				contlolPart += "\t\t\t" + rText + "\n";
			} else {
				comentStr = "このファイルの表示方法は確認中です。";
			}
			contlolPart += "\t\t</pre>\n";
			contlolPart += "\t\t<div>\n\t\t\t" + comentStr + "\n\t\t</div>\n";
			MyLog(dbMsg);
			return contlolPart;
		}  //Text用のタグを作成		


		private string MakeApplicationeSouce(string fileName, int webWidth, int webHeight) {
			string TAG = "[MakeApplicationeSouce]";
			string dbMsg = TAG;
			string contlolPart = "";
			string comentStr = "";
			string[] extStrs = fileName.Split('.');
			string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
			if (extentionStr == ".wmx" ||        //ver9:Windows Media Player スキン 
				extentionStr == ".wms" ||        //ver9:Windows Media Player スキン  
				extentionStr == ".wmz" ||     //ver9:Windows Media Player スキン  
				extentionStr == ".wms" ||     //ver9:Windows Media Player スキン  
				extentionStr == ".m3u" ||//MPEGだがrealPlayyerのプレイリスト
				extentionStr == ".wmd"     //ver9:Windows Media Download パッケージ   
				) {
				string clsId = "CLSID:6BF52A52-394A-11d3-B153-00C04F79FAA6";   //Windows Media Player9
				contlolPart += "\n\t\t<object classid =" + '"' + clsId + '"' + " style = " + '"' + "width:100%" + '"' + " >\n";
				contlolPart += "\t\t\t<param name =" + '"' + "url" + '"' + "value = " + '"' + "file://" + fileName + '"' + "/>\n";
				contlolPart += "\t\t\t<param name =" + '"' + "stretchToFit" + '"' + " value = true />\n";//右クリックして縮小/拡大で200％
				contlolPart += "\t\t\t<param name =" + '"' + "autoStart" + '"' + " value = " + true + "/>\n";
				comentStr = "Windows Media Player9読み込めないファイルは対策検討中です。";
			} else {
				comentStr = "このファイルの再生方法は確認中です。";
			}
			contlolPart += "\t\t<div>\n\t\t\t" + comentStr + "\n\t\t</div>\n";
			MyLog(dbMsg);
			return contlolPart;
		}  //アプリケーション用のタグを作成

		private void MakeWebSouceBody(string fileName, string urlStr) {
			string TAG = "[MakeWebSouceBody]";
			string dbMsg = TAG;
			try {
				dbMsg += ",fileName=" + fileName;
				dbMsg += ",url=" + urlStr;
				int webWidth = playerWebBrowser.Width - 28;
				int webHeight = playerWebBrowser.Height - 60;
				dbMsg += ",web[" + webWidth + "×" + webHeight + "]";
				string[] extStrs = fileName.Split('.');
				string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();

				string contlolPart = @"<!DOCTYPE html>
<html>
	<head>
		<meta charset = " + '"' + "UTF-8" + '"' + " >\n";
				contlolPart += "\t\t<meta http-equiv = " + '"' + "Pragma" + '"' + " content =  " + '"' + "no-cache" + '"' + " />\n";          //キャッシュを残さない；HTTP1.0プロトコル
				contlolPart += "\t\t<meta http-equiv = " + '"' + "Cache-Control" + '"' + " content =  " + '"' + "no-cache" + '"' + " />\n"; //キャッシュを残さない；HTTP1.1プロトコル
				contlolPart += "\t\t<meta http-equiv = " + '"' + "X-UA-Compatible" + '"' + " content =  " + '"' + "requiresActiveX =true" + '"' + " />\n";
				//	contlolPart += "\n\t\t\t<link rel = " + '"' + "stylesheet" + '"' + " type = " + '"' + "text/css" + '"' + " href = " + '"' + "brows.css" + '"' + "/>\n";
				string retType = GetFileTypeStr(fileName);
				dbMsg += ",retType=" + retType;
				if (retType == "video" ||
					 retType == "image" ||
					retType == "audio"
					) {
				} else {
					contlolPart += "\t</head>\n";
					contlolPart += "\t<body>\n\t\t";
				}
				dbMsg += ",fileName=" + fileName;
				if (lsFullPathName != fileName) {       //8/31;仮対応；書き換わり対策
					dbMsg += ",***書き換わり発生***" + fileName;
					fileName = lsFullPathName;
				}

				if (retType == "video") {
					contlolPart += MakeVideoSouce(fileName, webWidth, webHeight);
				} else if (retType == "image") {
					contlolPart += MakeImageSouce(fileName, webWidth, webHeight);
				} else if (retType == "audio") {
					contlolPart += MakeAudioSouce(fileName, webWidth, webHeight);
				} else if (retType == "text") {
					contlolPart += MakeTextSouce(fileName, webWidth, webHeight);
				} else if (retType == "application") {
					contlolPart += MakeApplicationeSouce(fileName, webWidth, webHeight);
				}
				if (debug_now) {
					contlolPart += "\t\t<div>,urlStr=" + urlStr;
					contlolPart += "<br>\n\t\t" + ",playerUrl=" + playerUrl + "</div>\n";
				}
				contlolPart += "\t</body>\n</html>\n\n";
				dbMsg += ",contlolPart=" + contlolPart;
				if (File.Exists(urlStr)) {
					dbMsg += "既存ファイル有り";
					System.IO.File.Delete(urlStr);                //20170818;ここで停止？
					dbMsg += ">Exists=" + File.Exists(urlStr);
				}
				////UTF-8でテキストファイルを作成する
				System.IO.StreamWriter sw = new System.IO.StreamWriter(urlStr, false, System.Text.Encoding.UTF8);
				sw.Write(contlolPart);
				sw.Close();
				dbMsg += ">Exists=" + File.Exists(urlStr);
				Uri nextUri = new Uri("file://" + urlStr);
				dbMsg += ",nextUri=" + nextUri;
				try {
					playerWebBrowser.Navigate(nextUri);
				} catch (System.UriFormatException er) {
					dbMsg += "<<playerWebBrowser.Navigateでエラー発生>>" + er.Message;
				}
				//		MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}//形式に合わせたhtml作成

		private void MakeWebSouce(string fileName) {
			string TAG = "[MakeWebSouce]";
			string dbMsg = TAG;
			try {
				dbMsg += ",fileName=" + fileName;
				FileInfo fi = new FileInfo(fileName);
				if (fi.Exists) {                     //変換するURIがファイルを表していることを確認する☆読み込み時にリロードのループになる
					string[] urlStrs = assemblyPath.Split(Path.DirectorySeparatorChar);
					assemblyName = urlStrs[urlStrs.Length - 1];
					string urlStr = assemblyPath.Replace(assemblyName, "brows.htm");//	urlStr = urlStr.Substring( 0, urlStr.IndexOf( "bin" ) ) + "brows.htm";
					dbMsg += ",url=" + urlStr;
					/*		int webWidth = webBrowser1.Width - 20;
							int webHeight = webBrowser1.Height - 40;
							dbMsg += ",web[" + webWidth + "×" + webHeight + "]";*/
					string[] extStrs = fileName.Split('.');
					string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
					if (extentionStr == ".htm" ||
						extentionStr == ".html") {
						string titolStr = "webでHTMLを読み込みますか？";
						string msgStr = "組み込んであるScriptなどで異常終了する場合があります\n" +
							"「はい」　web表示\n" +
							"     　　　※異常終了する場合は読み込みを中断します。" +
							"「いいえ」ソースをテキストで表示\n" +
							"「キャンセル」読込み中止";
						DialogResult result = MessageBox.Show(msgStr, titolStr,
							MessageBoxButtons.YesNoCancel,
							MessageBoxIcon.Asterisk,
							MessageBoxDefaultButton.Button1);                  //メッセージボックスを表示する
						if (result == DialogResult.Yes) {
							//「はい」が選択された時
							urlStr = fileName;
							Uri nextUri = new Uri("file://" + urlStr);
							dbMsg += ",nextUri=" + nextUri;
							try {
								playerWebBrowser.ScriptErrorsSuppressed = true;      //
								playerWebBrowser.Navigate(nextUri);
							} catch (Exception e) {
								Console.WriteLine(TAG + "でエラー発生" + e.Message + ";" + dbMsg);
							}
						} else if (result == DialogResult.No) {
							//「いいえ」が選択された時
							MakeWebSouceBody(fileName, urlStr);
						} else if (result == DialogResult.Cancel) {
							//「キャンセル」が選択された時
						}
					} else {
						if (lsFullPathName != "" && fileName != "未選択" && lsFullPathName != fileName) {       //8/31;仮対応；書き換わり対策
							dbMsg += ",***書き換わり発生*<<" + lsFullPathName + " ; " + fileName + ">>";
							fileName = lsFullPathName;
						}
						MakeWebSouceBody(fileName, urlStr);
					}
				} else {
					dbMsg += ",***指定されたファイルが無い？？*";
				}
				//			MyLog(dbMsg);
			} catch (Exception e) {
				dbMsg += "<<以降でエラー発生>>" + e.Message;
				MyLog(dbMsg);
			}
		}//形式に合わせたhtml作成
		 /*		http://html5-css3.jp/tips/youtube-html5video-window.html
		  *		http://dobon.net/vb/dotnet/string/getencodingobject.html
		  */

		protected override void OnPaint(PaintEventArgs e) {
			string TAG = "[OnPaint]";
			string dbMsg = TAG;
			base.OnPaint(e);
			if (typeName.Text != "") {      //rExtension.Text !="" &&
				MakeWebSouce(passNameLabel.Text + Path.DirectorySeparatorChar + fileNameLabel.Text);
			}
			MyLog(dbMsg);
		}           //リサイズ時の再描画

		private void ReSizeViews(object sender, EventArgs e) {
			string TAG = "[ReSizeViews]";
			string dbMsg = TAG;
			try {
				//		Size size = Form1.ScrollRectangle.Size; //webBrowser1.Document.Bodyだとerror! Body is null;
				//	var leftPWidth = 405;
				dbMsg += "[" + this.Width + "×" + this.Height + "]";
				dbMsg += ",leftTop=" + FileBrowserSplitContainer.Height + ",Center=" + FileBrowserCenterSplitContainer.Height;
				//		splitContainer1.Panel1.Width = leftPWidth;
				//	splitContainerLeftTop.Height = 60;
				//	splitContainerCenter.Panel1.Height = this.Height-(60+80);            //_Panel2.
				//	splitContainerCenter.Panel2.Height = 80;            //_Panel2.
				//		splitContainerCenter.Width = leftPWidth;
				dbMsg += ">>2=" + FileBrowserSplitContainer.Height + ">>Center=" + FileBrowserCenterSplitContainer.Height;
				/*		dbMsg += ",continuousPlayCheck=" + continuousPlayCheckBox.Checked;
						if (continuousPlayCheckBox.Checked) {
							viewSplitContainer.Width = playListWidth;
							PlayListsplitContainer.Height = fileTree.Bottom;
							dbMsg += ",playLis[" + playListWidth + "×" + PlayListsplitContainer.Height;
						}*/
				MakeWebSouce(fileNameLabel.Text);
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}//表示サイズ変更

		/// <summary>
		/// FileTreeを検索して該当Nodeを選択
		/// </summary>
		public TreeNode FindSelectFileViews(TreeNodeCollection tree, int level, int index, string fullName) {
			string TAG = "[FindSelectFileViews]";
			string dbMsg = TAG;
			try {
				dbMsg += "level=" + level + ")fullName=" + fullName;

				string[] findNames = fullName.Split(Path.DirectorySeparatorChar);
				if (level < findNames.Length) {
					string findStr = findNames[level];
					dbMsg += ",findStr;" + findStr;
					if (2 == findStr.Length) {              //ドライブルート
						findStr = findStr + Path.DirectorySeparatorChar;// + Path.DirectorySeparatorChar;
						dbMsg += ">>" + findStr;
					} else if (findStr == "") {             //2階層目
						level++;
						dbMsg += ">level>" + level;
						findStr = findNames[level];
					}
					foreach (TreeNode node in tree) {
						string nodeFullPath = node.FullPath;
						if (nodeFullPath.Contains(findStr)) {       //nodeText.Contains(findStr)
							string nodeText = node.Text;
							int nIndex = node.Index;
							dbMsg += "(" + nIndex + ")" + nodeFullPath;
							treeSelectList.Add(nIndex);
							System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(nodeFullPath);
							if (di.Exists) {                                                    //まだフォルダなら
								FolderItemListUp(nodeFullPath, node);                           //Nodeを追加
								node.Expand();
								if (node.Nodes != null) {                          //子ノードがある時
									level++;
									TreeNode node2 = FindSelectFileViews(node.Nodes, level, index, fullName);
								}
							} else {
								dbMsg += "::選択開始;SelectedNode=" + fileTree.SelectedNode.FullPath;
								TreeNode sNode = fileTree.Nodes[treeSelectList[0]];
								dbMsg += ",sNode=" + sNode.FullPath;
								for (int i = 1; i <= treeSelectList.Count - 1; i++) {
									int sIndex = treeSelectList[i];
									dbMsg += "(" + sIndex + ")";
									fileTree.SelectedNode = sNode.Nodes[sIndex];
									sNode = fileTree.SelectedNode;
									dbMsg += sNode.FullPath;

								}
								fileTree.Focus();
							}
							MyLog(dbMsg);
						}
					}
				}

				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
			return null;
		}//表示サイズ変更

		//ファイルTree操作/////////////////////////////////////////////////////////////////////////////////
		//		System.IO.FileInfo fCpoy = null;
		//		System.IO.FileInfo fMove = null;

		/// <summary>
		/// プレイリストへボタンのクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayListRedoroe_Click(object sender, EventArgs e) {
			string TAG = "[PlayListRedoroe_Click]";
			string dbMsg = TAG;
			try {
				string selectFullName = passNameLabel.Text + Path.DirectorySeparatorChar + fileNameLabel.Text;
				dbMsg += selectFullName;
				PlayFromFileBrousert(selectFullName);
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		///Nodeを書き直して再び開く ///////////////////////////////////
		/// </summary>
		public void ReExpandNode() {
			string TAG = "[ReExpandNode]";
			string dbMsg = TAG;
			try {
				string selectItem = fileTree.SelectedNode.FullPath;
				dbMsg += ",selectItem=" + selectItem;//
				string passNameStr = fileTree.SelectedNode.Parent.FullPath;
				dbMsg += ",passNameStr=" + passNameStr;
				//	if (File.Exists( selectItem )) {
				//		dbMsg += ",ファイルを選択";
				fileTree.SelectedNode.Parent.Collapse();                            //閉じて
				FolderItemListUp(passNameStr, fileTree.SelectedNode);      //TreeNodeを再構築して
				fileTree.SelectedNode.Expand();                             //開く
																			/*			} else if (Directory.Exists( selectItem )) {
																							dbMsg += ",フォルダを選択";
																						}
																						dbMsg += ",reDrowNode=" + reDrowNode.Name + ",passNameStr=" + passNameStr;
																						*/
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// 指定されたものを削除
		/// </summary>
		/// <param name="sourceName">ファイルもしくはフォルダ名</param>
		/// <param name="isTrash">trueでゴミ箱　/　falseで完全削除</param>
		public void DelFiles(string sourceName, bool isTrash) {
			string TAG = "[DelFiles]";
			string dbMsg = TAG;
			try {
				dbMsg += ",元=" + sourceName + "を削除;isTrash=" + isTrash;
				Microsoft.VisualBasic.FileIO.RecycleOption recycleOption = RecycleOption.DeletePermanently;         //ファイルまたはディレクトリを完全に削除します。 既定モード。
				元に戻す.Visible = false;
				if (isTrash) {
					recycleOption = RecycleOption.SendToRecycleBin;                                                //ファイルまたはディレクトリの送信、 ごみ箱します。																												   //			元に戻す.Visible = true;
				}
				if (File.Exists(sourceName)) {
					dbMsg += ",ファイルを選択";
					FileSystem.DeleteFile(sourceName, UIOption.AllDialogs, recycleOption, UICancelOption.DoNothing);
					//もしくは	System.IO.File.Delete( sourceName );             //フォルダ"C:\TEST"を削除する
				} else if (Directory.Exists(sourceName)) {
					dbMsg += ",フォルダを選択";
					FileSystem.DeleteDirectory(sourceName, UIOption.AllDialogs, recycleOption, UICancelOption.DoNothing);
					//もしくは	System.IO.Directory.Delete( sourceName, true );   //true;エラーを無視して削除？
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
			//https://dobon.net/vb/dotnet/file/directorycreate.html
		}

		public void MoveMyFile(string sourceName, string destName) {
			string TAG = "[MoveMyFile]";
			string dbMsg = TAG;
			try {
				dbMsg += ",元=" + sourceName + ",先=" + destName;
				System.IO.FileInfo fi = new System.IO.FileInfo(sourceName);   //変更元のFileInfoのオブジェクトを作成します。 @"C:\files1\sample1.txt" 
				fi.MoveTo(destName);                                           //MoveToメソッドで移動先を指定してファイルを移動します。@"D:\files2\sample2.txt"
																			   // http://www.openreference.org/articles/view/329
																			   //	fi = null;
				MyLog(dbMsg);
			} catch (Exception er) {
				Console.WriteLine(TAG + "でエラー発生" + er.Message + ";" + dbMsg);
				throw new NotImplementedException();//要求されたメソッドまたは操作が実装されない場合にスローされる例外。
			}
		}

		public void MoveFolder(string sourceName, string destName) {
			string TAG = "[MoveFolder]";
			string dbMsg = TAG;
			try {
				dbMsg += ",元=" + sourceName + ",先=" + destName;
				//https://dobon.net/vb/dotnet/file/directorycreate.html
				/*			string[] dirs = System.IO.Directory.GetFiles( sourceName, "*", System.IO.SearchOption.AllDirectories );
								dbMsg += ",中身は" + dirs.Length + "フォルダ" ;
								foreach (string dir in dirs) {
									string newSouce = dir;
									dbMsg += "," + newSouce;
								}*/

				//Directoryクラスを使用する方法;中身移動せず
				/*			System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory( destName );   //フォルダ"C:\TEST\SUB"を作成する
							System.IO.Directory.Move( sourceName, destName );               //フォルダ"C:\1"を"C:\2\SUB"に移動（名前を変更）する
							string[] files = System.IO.Directory.GetFiles( di.FullName, "*", System.IO.SearchOption.AllDirectories );
							dbMsg += ",di（" + di.FullName + "）に" + files.Length + "件";//
							System.IO.Directory.Delete( sourceName, true );             //フォルダ"C:\TEST"を削除する
							*/
				//DirectoryInfoクラスを使用する方法;中身移動せず
				/*		System.IO.DirectoryInfo di = new System.IO.DirectoryInfo( sourceName ); //@"C:\TEST\SUB"；DirectoryInfoオブジェクトを作成する
						string[] files = System.IO.Directory.GetFiles( di.FullName, "*", System.IO.SearchOption.AllDirectories );
						dbMsg += ",di（" + di.FullName + "）に" + files.Length + "件";//
						di.Create();                                                           //フォルダ"C:\TEST\SUB"を作成する
						System.IO.DirectoryInfo subDir = di.CreateSubdirectory( "1" );     //サブフォルダを作成する☆subDirには、フォルダ"C:\TEST\SUB\1"のDirectoryInfoオブジェクトが入る
						files = System.IO.Directory.GetFiles( subDir.FullName, "*", System.IO.SearchOption.AllDirectories );
						dbMsg += ",di（" + subDir.FullName + "）に" + files.Length + "件";
						subDir.MoveTo( destName );                                           //フォルダ"C:\TEST\SUB\1"を"C:\TEST\SUB\2"に移動する☆subDirの内容は、"C:\TEST\SUB\2"のものに変わる
						di.Delete( true );                                                  //フォルダ"C:\TEST\SUB"を根こそぎ削除する☆trueにしないと中身が有った場合にエラー発生
						*/

				//FileSystemを使用:参照設定に"Microsoft.VisualBasic.dll"が追加されている必要がある
				FileSystem.CreateDirectory(destName);                     //フォルダdestを作成する
				string[] dirs = System.IO.Directory.GetFiles(destName, "*", System.IO.SearchOption.AllDirectories);
				dbMsg += ",中身は" + dirs.Length + "フォルダ";
				FileSystem.MoveDirectory(sourceName, destName, true);    //sourceをdestに移動する☆第3項にTrueを指定すると、destが存在する時、上書きする
																		 //		FileSystem.MoveDirectory( sourceName, destName, UIOption.AllDialogs, UICancelOption.DoNothing );//sourceをdestに移動する
																		 //進行状況ダイアログとエラーダイアログを表示する☆ユーザーがキャンセルしても例外OperationCanceledExceptionをスローしない
				dirs = System.IO.Directory.GetFiles(destName, "*", System.IO.SearchOption.AllDirectories);
				dbMsg += ">>" + dirs.Length + "件";
				DelFiles(sourceName, false);
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
			//https://dobon.net/vb/dotnet/file/directorycreate.html
		}


		public void TargetReName(string destName) {
			string TAG = "[TargetReName]";
			string dbMsg = TAG;
			try {
				dbMsg += " , destName=" + destName;
				//		TreeNode selectNode = fileTree.SelectedNode;
				string selectItem = fileNameLabel.Text + "";
				string passNameStr = passNameLabel.Text + "";
				dbMsg += " , passNameStr=" + passNameStr;
				if (passNameStr != selectItem) {                        //ドライブ選択でなければ
					dbMsg += " , selectItem=" + selectItem;
					if (selectItem != passNameLabel.Text) {
						selectItem = passNameLabel.Text + Path.DirectorySeparatorChar + selectItem;
						dbMsg += ">>" + selectItem;  // selectItem=media2.flv>>M:\sample/media2.flv,選択；ペースト,
					}
					string titolStr = selectItem + "の名称変更";
					string msgStr = "元の名称\n" + selectItem;
					dbMsg += ",titolStr=" + titolStr + ",msgStr=" + msgStr;

					InputDialog f = new InputDialog(msgStr, titolStr, destName);
					if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
						destName = f.ResultText;
						dbMsg += ",元=" + selectItem + ",先=" + destName;
						string renewName = passNameLabel.Text + Path.DirectorySeparatorChar + destName;
						if (File.Exists(selectItem)) {
							dbMsg += ">>ファイル名変更>" + renewName;
							MoveMyFile(selectItem, renewName);
						} else if (Directory.Exists(selectItem)) {
							dbMsg += ">>フォルダ名変更>" + renewName;
							MoveFolder(selectItem, renewName);
						}
						//  https://dobon.net/vb/dotnet/control/tvlabeledit.html
					} else {
						dbMsg += ">>Cancel";
					}
				} else {
					string titolStr = selectItem + "の名称は変更できません";
					string msgStr = "ドライブ名称は変更できません";
					DialogResult result = MessageBox.Show(msgStr, titolStr,
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation,
						MessageBoxDefaultButton.Button1);                  //メッセージボックスを表示する
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// ファイルもしくはフォルダをコピーする
		/// </summary>
		/// <param name="sourceName">コピー元</param>
		/// <param name="destName">コピー先</param>
		public void FilesCopy(string sourceName, string destName) {
			string TAG = "[FilesCopy]";
			string dbMsg = TAG;
			try {
				dbMsg += ",元=" + sourceName + ",先=" + destName;
				string[] souceNames = sourceName.Split(Path.DirectorySeparatorChar);
				string souceEnd = souceNames[souceNames.Length - 1];
				destName += Path.DirectorySeparatorChar + souceEnd;
				dbMsg += ">>" + destName;
				if (File.Exists(sourceName)) {
					dbMsg += ">>ファイル";
					if (File.Exists(destName)) {
						string[] extStrs = destName.Split('.');
						souceEnd = extStrs[extStrs.Length - 2];
						destName = destName + "のコピー." + extStrs[extStrs.Length - 1];
						dbMsg += ">>" + destName;
					}
					//		FileSystem.CopyFile( sourceName, destName );                  //"C:\test\1.txt"を"C:\test\2.txt"にコピーする
					//		FileSystem.CopyFile( sourceName, destName, true );	//"C:\test\2.txt"がすでに存在している場合は、これを上書きする
					//		FileSystem.CopyFile( sourceName, destName, UIOption.OnlyErrorDialogs );                    //エラーの時、ダイアログを表示する
					//		FileSystem.CopyFile( sourceName, destName,UIOption.AllDialogs );                    //進行状況ダイアログと、エラーダイアログを表示する
					FileSystem.CopyFile(sourceName, destName, UIOption.AllDialogs, UICancelOption.DoNothing);
					//進行状況ダイアログやエラーダイアログでキャンセルされても例外をスローしない
					//UICancelOption.DoNothingを指定しないと、例外OperationCanceledExceptionが発生
				} else if (Directory.Exists(sourceName)) {
					dbMsg += ">>フォルダ";
					if (Directory.Exists(destName)) {
						destName = destName + "のコピー";
						dbMsg += ">>" + destName;
					}
					FileSystem.CopyDirectory(sourceName, destName, UIOption.AllDialogs, UICancelOption.DoNothing);
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// ファイルもしくはフォルダをコピーする
		/// </summary>
		/// <param name="sourceName">コピー元</param>
		/// <param name="destName">コピー先</param>
		public void FilesMove(string sourceName, string destName) {
			string TAG = "[FilesMove]";
			string dbMsg = TAG;
			try {
				dbMsg += ",元=" + sourceName + ",先=" + destName;
				string[] souceNames = sourceName.Split(Path.DirectorySeparatorChar);
				string souceEnd = souceNames[souceNames.Length - 1];
				destName += Path.DirectorySeparatorChar + souceEnd;
				dbMsg += ">>" + destName;
				if (File.Exists(sourceName)) {
					dbMsg += ">>ファイル";
					MoveMyFile(sourceName, destName);
				} else if (Directory.Exists(sourceName)) {
					dbMsg += ">>フォルダ";
					MoveFolder(sourceName, destName);
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
		}

		public static void CopyDirectory(string sourceDirName, string destDirName) {
			string TAG = "[CopyDirectory]";
			string dbMsg = TAG;
			try {
				dbMsg += ",元=" + sourceDirName + ",先=" + destDirName;
				if (!System.IO.Directory.Exists(destDirName)) {                                                           //コピー先のディレクトリがないときは
					System.IO.Directory.CreateDirectory(destDirName);                                                      //作る
					System.IO.File.SetAttributes(destDirName, System.IO.File.GetAttributes(sourceDirName));              //属性もコピー
				}

				if (destDirName[destDirName.Length - 1] != System.IO.Path.DirectorySeparatorChar) {
					destDirName = destDirName + System.IO.Path.DirectorySeparatorChar;                                      //コピー先のディレクトリ名の末尾に"\"をつける
				}

				string[] files = System.IO.Directory.GetFiles(sourceDirName);
				foreach (string file in files) {
					System.IO.File.Copy(file, destDirName + System.IO.Path.GetFileName(file), true);                     //コピー元のディレクトリにあるファイルをコピー
				}

				string[] dirs = System.IO.Directory.GetDirectories(sourceDirName);
				foreach (string dir in dirs) {
					CopyDirectory(dir, destDirName + System.IO.Path.GetFileName(dir));          //コピー元のディレクトリにあるディレクトリについて、再帰的に呼び出す
				}

				//		MyLog( dbMsg );
			} catch (Exception er) {
				Console.WriteLine(TAG + "でエラー発生" + er.Message + ";" + dbMsg);
			}
		}

		/// <summary>
		/// コピーかカットかを判定してペースト動作へ
		/// </summary>
		/// <param name="copySouce"></param>
		/// <param name="cutSouce"></param>
		/// <param name="peastFor"></param>
		public void PeastSelecter(string copySouce, string cutSouce, string peastFor) {
			string TAG = "[PeastSelecter]";
			string dbMsg = TAG;
			try {
				dbMsg += ",copy=" + copySouce + ",cut=" + cutSouce + ",先=" + peastFor;
				if (copySouce != "") {
					FilesCopy(copySouce, peastFor);
					//		copySouce = "";
				} else if (cutSouce != "") {
					FilesMove(cutSouce, peastFor);
					cutSouce = "";
				}
				コピーToolStripMenuItem.Visible = true;
				カットToolStripMenuItem.Visible = true;
				ペーストToolStripMenuItem.Visible = false;
				ReExpandNode();
				dbMsg += ",copy=" + copySouce + ",cut=" + cutSouce + ",先=" + peastFor;
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// 割り付けられたアプリケーションを起動する
		/// </summary>
		/// <param name="sourceName"></param>
		public void SartApication(string sourceName) {
			string TAG = "[SartApication]";
			string dbMsg = TAG;
			try {
				dbMsg += ",元=" + sourceName;
				System.Diagnostics.Process p = System.Diagnostics.Process.Start(sourceName);
				dbMsg += ",MainWindowTitle=" + p.MainWindowTitle;
				dbMsg += ",ModuleName=" + p.MainModule.ModuleName;
				dbMsg += ",ProcessName=" + p.ProcessName;
				MyLog(dbMsg);                                             //何故かここのLogが出ない
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		string flRightClickItemUrl = "";
		/// <summary>
		/// fileTreeで右クリックされたアイテムからフルパスをグローバル変数に設定
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void FilelistBoxMouseUp(object sender, MouseEventArgs e) {
			string TAG = "[PlaylistBox1_MouseUp]";
			string dbMsg = TAG;
			try {
				if (e.Button == System.Windows.Forms.MouseButtons.Right) {          // 右クリックされた？
																					/*	int index = fileTree.IndexFromPoint(e.Location);             // マウス座標から選択すべきアイテムのインデックスを取得
																						dbMsg += ",index=" + index;
																						if (index >= 0) {               // インデックスが取得できたら
																							*/
					TreeNode flRightItem = fileTree.GetNodeAt(e.X, e.Y);
					flRightClickItemUrl = flRightItem.FullPath;
					dbMsg += ",flRightClickItemUrl=" + flRightClickItemUrl;
					//	playListBox.ClearSelected();                    // すべての選択状態を解除してから
					//playListBox.SelectedIndex = index;                  // アイテムを選択
					Point pos = fileTree.PointToScreen(e.Location);
					dbMsg += ",pos=" + pos;
					fileTreeContextMenuStrip.Show(pos);                     // コンテキストメニューを表示
																			//	}
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// ファイルリストの右クリックで開くコンテキストメニューの処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void ContextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
			string TAG = "[ContextMenuStrip1_ItemClicked]";
			string dbMsg = TAG;
			try {
				//			dbMsg += "sender=" + sender;                    //sender=	常にSystem.Windows.Forms.TreeView, Nodes.Count: 5, Nodes[0]: TreeNode: C:\,
				//			dbMsg += ",e=" + e;                             //e=		常にSystem.Windows.Forms.TreeViewEventArgs,
				dbMsg += ",ClickedItem=" + e.ClickedItem.Name;                             //e=		常にSystem.Windows.Forms.TreeViewEventArgs,
				string clickedMenuItem = e.ClickedItem.Name.Replace("ToolStripMenuItem", "");
				dbMsg += ">>" + clickedMenuItem;               // Name: contextMenuStrip1, Items: 7,e=System.Windows.Forms.ToolStripItemClickedEventArgs,ClickedItem=ペーストToolStripMenuItem>>ペーストToolStripMenuItem ,
				TreeNode selectNode = fileTree.SelectedNode;
				string selectItem = selectNode.Text;
				dbMsg += " , selectItem=" + selectItem;
				if (selectItem != passNameLabel.Text) {
					selectItem = passNameLabel.Text + Path.DirectorySeparatorChar + selectItem;
					dbMsg += ">>" + selectItem;  // selectItem=media2.flv>>M:\sample/media2.flv,選択；ペースト,
				}
				string destDirName = selectItem + Path.DirectorySeparatorChar + "新しいフォルダ";
				string selectFullName = flRightClickItemUrl;// selectNode.FullPath;
				fileTreeContextMenuStrip.Close();                                           //☆ダイアログが出ている間、メニューが表示されっぱなしになるので強制的に閉じる

				switch (clickedMenuItem) {                                           // クリックされた項目の Name を判定します。 
					case "フォルダ作成":
						dbMsg += ",選択；フォルダ作成=" + destDirName;
						System.IO.Directory.CreateDirectory(destDirName);
						break;

					case "名称変更":
						dbMsg += ",選択；名称変更=" + destDirName;
						TargetReName(selectNode.Text);
						ReExpandNode();
						break;

					case "カット":
						cutSouce = selectItem;
						dbMsg += ",選択；カット" + cutSouce;
						break;

					case "コピー":
						copySouce = selectItem;
						dbMsg += ",選択；コピー" + copySouce;
						break;

					case "ペースト":
						dbMsg += ",選択；ペースト";
						PeastSelecter(copySouce, cutSouce, selectFullName);
						break;

					case "削除":
						dbMsg += ",選択；削除;" + selectFullName;
						DelFiles(selectFullName, true);
						ReExpandNode();
						break;

					case "元に戻す":
						dbMsg += ",選択；元に戻す";
						元に戻す.Visible = false;
						break;

					case "他のアプリケーションで開く":
						dbMsg += ",選択；他のアプリケーションで開く";
						SartApication(selectItem);
						break;

					case "プレイリストに追加":
						dbMsg += ",選択；プレイリストに追加；" + selectFullName;
						string[] PLArray = ComboBoxItems2StrArray(PlaylistComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };
						dbMsg += ",PLArray=" + PLArray.Length + "件";
						if (PLArray.Length < 1) {
							AddPlayListFromFile(selectFullName);
						}
						break;

					case "プレイリストを作成":
						dbMsg += ",選択；プレイリストを作成；" + selectFullName;
						break;
					case "再生ToolStripMenuItem":
						dbMsg += ",選択；再生；" + selectFullName;
						break;

					default:
						break;
				}

				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
		}


		private void PlaylistAddMenuStrip_Opening(object sender, CancelEventArgs e) {
			string TAG = "[PlaylistAddMenuStrip_Opening]";
			string dbMsg = TAG;
			try {
				//		ToolStripMenuItem mi = (ToolStripMenuItem)sender;               // sender にはクリックされたメニューの ToolStripMenuItem が入ってきますので、 必要に応じて処理を行います
				//		string ClickedItem = mi.ToolTipText;
				string[] PLArray = ComboBoxItems2StrArray(PlaylistComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };
				dbMsg += ",PLArray=" + PLArray.Length + "件";
				if (PLArray.Length < 1) {
				} else {
					string addItem = "";
					プレイリストに追加ToolStripMenuItem.DropDownItems.Clear();
					for (int i = 0; i < PLArray.Length; i++) {
						dbMsg += "(" + i + ")";
						addItem = PLArray[i].ToString();
						dbMsg += addItem;
						ToolStripMenuItem tsi2 = new ToolStripMenuItem();
						tsi2.Text = addItem;
						string tsi2ToolTipText = addItem + "ToolStripMenuItem";
						tsi2.ToolTipText = tsi2ToolTipText;
						tsi2.Click += ContextMenuStrip_SubMenuClick;                                // クリックイベントを追加する
																									// フォームで設定した ItemClicked イベントは第1階層の項目のみ発生する
						プレイリストに追加ToolStripMenuItem.DropDownItems.Add(tsi2); // 第1階層のメニューの最後尾に追加

						ToolStripMenuItem tsi31 = new ToolStripMenuItem();
						tsi31.Text = "先頭に挿入";
						tsi31.ToolTipText = tsi2ToolTipText + "先頭に挿入ToolStripMenuItem";
						tsi31.Click += ContextMenuStrip_SubMenuClick;                                // クリックイベントを追加する
																									 // フォームで設定した ItemClicked イベントは第1階層の項目のみ発生する
						tsi2.DropDownItems.Add(tsi31); // 第2階層のメニューの最後尾に追加

						ToolStripMenuItem tsi32 = new ToolStripMenuItem();
						tsi32.Text = "末尾に追加";
						tsi32.ToolTipText = tsi2ToolTipText + "末尾に追加ToolStripMenuItem";
						tsi32.Click += ContextMenuStrip_SubMenuClick;                                // クリックイベントを追加する
																									 // フォームで設定した ItemClicked イベントは第1階層の項目のみ発生する
						tsi2.DropDownItems.Add(tsi32); // 第2階層のメニューの最後尾に追加

					}
					ToolStripMenuItem tsi2e = new ToolStripMenuItem();
					tsi2e.Text = "その他のリスト";
					string tsi2eToolTipText = "その他のリストToolStripMenuItem";
					tsi2e.ToolTipText = tsi2eToolTipText;
					tsi2e.Click += ContextMenuStrip_SubMenuClick;                                // クリックイベントを追加する
					プレイリストに追加ToolStripMenuItem.DropDownItems.Add(tsi2e); // 第1階層のメニューの最後尾に追加

					ToolStripMenuItem tsi3e1 = new ToolStripMenuItem();
					tsi3e1.Text = "先頭に挿入";
					tsi3e1.ToolTipText = tsi2eToolTipText + "先頭に挿入ToolStripMenuItem";
					tsi3e1.Click += ContextMenuStrip_SubMenuClick;                                // クリックイベントを追加する
					tsi2e.DropDownItems.Add(tsi3e1); // 第2階層のメニューの最後尾に追加

					ToolStripMenuItem tsi3e2 = new ToolStripMenuItem();
					tsi3e2.Text = "末尾に追加";
					tsi3e2.ToolTipText = tsi2eToolTipText + "末尾に追加ToolStripMenuItem";
					tsi3e2.Click += ContextMenuStrip_SubMenuClick;                                // クリックイベントを追加する
					tsi2e.DropDownItems.Add(tsi3e2); // 第2階層のメニューの最後尾に追加
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
		}

		// プレイリストに追加・第2階層のメニュー項目のクリックイベント
		private void ContextMenuStrip_SubMenuClick(object sender, EventArgs e) {
			string TAG = "[ContextMenuStrip1_ItemClicked]";
			string dbMsg = TAG;
			try {
				ToolStripMenuItem mi = (ToolStripMenuItem)sender;               // sender にはクリックされたメニューの ToolStripMenuItem が入ってきますので、 必要に応じて処理を行います
				string ClickedItem = mi.ToolTipText;
				dbMsg += ",ClickedItem=" + ClickedItem;             //[ContextMenuStrip1_ItemClicked],ClickedItem=M:\DL\2017.m3uToolStripMenuItem先頭に挿入ToolStripMenuItem
																	//		ToolTipTextに	"M:\\DL\\2017.m3u先頭に挿入ToolStripMenuItem"
				string[] CItems = System.Text.RegularExpressions.Regex.Split(ClickedItem, "ToolStripMenuItem");
				string ListUrl = CItems[0];
				dbMsg += ",ListUrl=" + ListUrl;                     //ListUrl=M:\DL\2017.m3u
				string clickedMenuItem = CItems[CItems.Length - 3]; //mi.Text.Replace("ToolStripMenuItem", "");
				dbMsg += ",clickedMenuItem=" + clickedMenuItem;               //clickedMenuItem=先頭に挿入
				if (clickedMenuItem == "") {
					clickedMenuItem = ListUrl;
					dbMsg += ">>" + clickedMenuItem;               //clickedMenuItem=先頭に挿入
				}
				string TopBottom = CItems[CItems.Length - 2];
				dbMsg += ",TopBottom=" + TopBottom;               // Name: contextMenuStrip1, Items: 7,e=System.Windows.Forms.ToolStripItemClickedEventArgs,ClickedItem=ペーストToolStripMenuItem>>ペーストToolStripMenuItem ,
				bool toTop = false;
				if (TopBottom == "先頭に挿入") {
					toTop = true;
				}
				string destDirName = flRightClickItemUrl;
				dbMsg += ",destDirName=" + destDirName;  //,,,TopBottom=>>先頭に挿入
				switch (clickedMenuItem) {                                           // クリックされた項目の Name を判定します。 
					case "その他のリスト":
						dbMsg += ",選択；その他のリスト";
						AddPlayListFromFile(flRightClickItemUrl);
						break;
					default:
						AddOne2PlayList(ListUrl, flRightClickItemUrl, toTop);
						break;
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}


		/// <summary>
		/// failreeが選択されている時に同時に押されているキーの有無を判定する
		/// F2；ラベル編集
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FileTree_KeyUp(object sender, KeyEventArgs e) {
			string TAG = "[FileTree_KeyUp]";
			string dbMsg = TAG;
			try {
				TreeView tv = (TreeView)sender;
				if (e.KeyCode == Keys.F2 && tv.SelectedNode != null && tv.LabelEdit) {              //F2キーが離されたときは、フォーカスのあるアイテムの編集を開始
					tv.SelectedNode.BeginEdit();
				}
			} catch (Exception er) {
				Console.WriteLine(TAG + "でエラー発生" + er.Message + ";" + dbMsg);
				throw new NotImplementedException();//要求されたメソッドまたは操作が実装されない場合にスローされる例外。
			}
		}

		/// <summary>
		/// fileTreeのノードがドラッグされた時
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TreeView1_ItemDrag(object sender, ItemDragEventArgs e) {
			string TAG = "[TreeView1_ItemDrag]";
			string dbMsg = TAG;
			try {
				cutSouce = "";       //カットするアイテムのurl
				copySouce = "";      //コピーするアイテムのurl
				TreeView tv = (TreeView)sender;
				tv.SelectedNode = (TreeNode)e.Item;
				dbMsg += " ,SelectedNode=" + tv.SelectedNode.FullPath;
				tv.Focus();
				DragDropEffects dde = tv.DoDragDrop(e.Item, DragDropEffects.All);
				dbMsg += "のドラッグを開始";
				if ((dde & DragDropEffects.Move) == DragDropEffects.Move) {
					cutSouce = tv.SelectedNode.FullPath;       //カットするアイテムのurl
					dbMsg += " , 移動した時は、ドラッグしたノードを削除";
					tv.Nodes.Remove((TreeNode)e.Item);
				}
				dbMsg += ",copy=" + copySouce + ",cut=" + cutSouce;
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// ドラッグしている時
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TreeView1_DragOver(object sender, DragEventArgs e) {
			string TAG = "[TreeView1_DragOver]";
			string dbMsg = TAG;
			try {
				TreeView tv = (TreeView)sender;
				dbMsg += " ,SelectedNode=" + tv.SelectedNode.FullPath;
				if (e.Data.GetDataPresent(typeof(TreeNode))) {              //ドラッグされているデータがTreeNodeか調べる
					if ((e.KeyState & 8) == 8 && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) {
						dbMsg += " , Ctrlキーが押されている>>Copy";//Ctrlキーが押されていればCopy//"8"はCtrlキーを表す
						copySouce = tv.SelectedNode.FullPath;      //コピーするアイテムのurl
						e.Effect = DragDropEffects.Copy;
					} else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move) {
						dbMsg += " , 何も押されていない>>Move";
						cutSouce = tv.SelectedNode.FullPath;     //カットするアイテムのurl
						e.Effect = DragDropEffects.Move;
					} else {
						cutSouce = tv.SelectedNode.FullPath;     //カットするアイテムのurl
						e.Effect = DragDropEffects.None;
					}
				} else {
					dbMsg += " ,TreeNodeでなければ受け入れない";
					e.Effect = DragDropEffects.None;
					if (e.Effect != DragDropEffects.None) {                 //マウス下のNodeを選択する
						TreeNode target = tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y)));             //マウスのあるNodeを取得する
						TreeNode source = (TreeNode)e.Data.GetData(typeof(TreeNode));                     //ドラッグされているNodeを取得する
						if (target != null && target != source && !IsChildNode(source, target)) {             //マウス下のNodeがドロップ先として適切か調べる
							if (target.IsSelected == false) {                                                   //Nodeを選択する
								tv.SelectedNode = target;
							}
						} else {
							e.Effect = DragDropEffects.None;
						}
					}
				}
				dbMsg += ",copy=" + copySouce + ",cut=" + cutSouce;
				//		MyLog( dbMsg );
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// ドロップされたとき
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TreeView1_DragDrop(object sender, DragEventArgs e) {
			string TAG = "[TreeView1_DragDrop]";
			string dbMsg = TAG;
			try {
				if (e.Data.GetDataPresent(typeof(TreeNode))) {                              //ドロップされたデータがTreeNodeか調べる
					TreeView tv = (TreeView)sender;
					dbMsg += " Drop先は" + tv.SelectedNode.FullPath;
					TreeNode source = (TreeNode)e.Data.GetData(typeof(TreeNode));         //ドロップされたデータ(TreeNode)を取得
					TreeNode target = tv.GetNodeAt(tv.PointToClient(new Point(e.X, e.Y))); //ドロップ先のTreeNodeを取得する
					string dropSouce = target.FullPath;
					if (target != null && target != source && !IsChildNode(source, target)) { //マウス下のNodeがドロップ先として適切か調べる
						dbMsg += ",copy=" + copySouce + ",cut=" + cutSouce + ",peast先=" + dropSouce;
						PeastSelecter(copySouce, cutSouce, dropSouce);
						/*表示だけの書き換えなら
							TreeNode cln = ( TreeNode ) source.Clone();                             //ドロップされたNodeのコピーを作成
							target.Nodes.Add( cln );												//Nodeを追加
							target.Expand();														//ドロップ先のNodeを展開
							tv.SelectedNode = cln;                                                  //追加されたNodeを選択
						*/
					} else {
						e.Effect = DragDropEffects.None;
					}
				} else {
					e.Effect = DragDropEffects.None;
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "でエラー発生" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// あるTreeNodeが別のTreeNodeの子ノードか調べる
		/// </summary>
		/// <param name="parentNode">親ノードか調べるTreeNode</param>
		/// <param name="childNode">子ノードか調べるTreeNode</param>
		/// <returns>子ノードの時はTrue</returns>
		private static bool IsChildNode(TreeNode parentNode, TreeNode childNode) {
			string TAG = "[IsChildNode]";
			string dbMsg = TAG;
			//			try {

			if (childNode.Parent == parentNode) {
				return true;
			} else if (childNode.Parent != null) {
				return IsChildNode(parentNode, childNode.Parent);
			} else {
				return false;
			}
			/*	//		MyLog( dbMsg );
					} catch (Exception er) {
						dbMsg += "でエラー発生" + er.Message;
				//		MyLog( dbMsg );
					}*/
		}
		//連続再生///////////////////////////////////////////////////////////その他//
		private List<PlayListItems> ListUpFiles(string carrentDir, string type) {
			string TAG = "[ListUpFiles]";
			string dbMsg = TAG;
			try {
				dbMsg += "carrentDir=" + carrentDir + ",type=" + type;
				string sarchDir = carrentDir;
				string[] files = Directory.GetFiles(sarchDir);
				int listCount = -1;
				int tCount = 0;
				int nowCount = PlayListBoxItem.Count;
				dbMsg += "/PlayListBoxItem; " + PlayListBoxItem.Count + "件";
				string wrTitol = "";
				int nowToTal = CurrentItemCount(sarchDir);      // サブディレクトリ内のファイルもカウントする場合	, SearchOption.AllDirectories
				dbMsg += ",このデレクトリには" + nowToTal + "件";
				int barMax = progressBar1.Maximum;
				dbMsg += ",progressMax=" + barMax + "件";
				if (barMax < nowToTal) {
					progressBar1.Maximum = nowToTal;
					ProgressMaxLabel.Text = progressBar1.Maximum.ToString();        //Max
					ProgressMaxLabel.Update();
				}
				if (files != null) {
					dbMsg += "ファイル=" + files.Length + "件";
					foreach (string plFileName in files) {
						listCount++;
						dbMsg += "\n(" + listCount + ")" + plFileName;
						string[] pathStrs = plFileName.Split(Path.DirectorySeparatorChar);
						System.IO.FileAttributes attr = System.IO.File.GetAttributes(plFileName);
						if ((attr & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden) {
							dbMsg += ">>Hidden";
						} else if ((attr & System.IO.FileAttributes.System) == System.IO.FileAttributes.System) {
							dbMsg += ">>System";
						} else {
							string[] extStrs = plFileName.Split('.');
							string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
							dbMsg += "拡張子=" + extentionStr;
							if (type == "video" && 0 < Array.IndexOf(videoFiles, extentionStr) ||
								type == "audio" && 0 < Array.IndexOf(audioFiles, extentionStr)
								) {
								string wrPathStr = plFileName.Replace((":" + Path.DirectorySeparatorChar), ":" + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar);
								dbMsg += "Path=" + wrPathStr;
								wrTitol = Path2titol(wrPathStr);
								dbMsg += ",Titol=" + wrTitol;
								PlayListItems pli = new PlayListItems(wrTitol, wrPathStr);
								PlayListBoxItem.Add(pli);      //	ListBoxItem.Add( mi );//	tNode.Nodes.Add( fileName, rfileName, iconType, iconType );
								tCount = Int32.Parse(targetCountLabel.Text) + 1;
								targetCountLabel.Text = tCount.ToString();                    //確認
								targetCountLabel.Update();
								prgMessageLabel.Text = pathStrs[pathStrs.Length - 1];
								prgMessageLabel.Update();
							}
						}

						int checkCount = Int32.Parse(progCountLabel.Text) + 1;                          //pDialog.GetProgValue() + 1;
						progCountLabel.Text = checkCount.ToString();                   //確認
						progCountLabel.Update();
						dbMsg += ",vCount=" + checkCount;
						if (progressBar1.Maximum < checkCount) {
							progressBar1.Maximum = checkCount + 10;
							ProgressMaxLabel.Text = progressBar1.Maximum.ToString();        //Max
							ProgressMaxLabel.Update();
						}
						progressBar1.Value = checkCount;
						progresPanel.Update();
						PlayListLabelWrigt(tCount.ToString(), plFileName);
						//	pDialog.RedrowPDialog(checkCount.ToString(),  maxvaluestr, nowCount.ToString(), wrTitol);   保留；プログレスダイアログ更新
					}
				}
				string[] foleres = Directory.GetDirectories(sarchDir);//
				if (foleres != null) {
					foreach (string folereName in foleres) {
						if (-1 < folereName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
							-1 < folereName.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase)) {
						} else {
							string rfolereName = folereName.Replace(sarchDir, "");// + 
							rfolereName = rfolereName.Replace(Path.DirectorySeparatorChar + "", "");
							dbMsg += ",foler=" + rfolereName;
							ListUpFiles(folereName, type);        //再帰
						}
					}           //ListBox1に結果を表示する
				}
				MyLog(dbMsg);
				/*	} catch (System.UnauthorizedAccessExceptionr) {
						dbMsg += "<<以降でエラー発生>>" + er.Message;
						MyLog(dbMsg);*/
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
			return PlayListBoxItem;
		}

		/// <summary>
		/// プレイリストの表示とwebのリサイズ
		/// </summary>
		private void SetPlayListItems(string carrentDir, string type) {
			string TAG = "[SetPlayListItems]";
			string dbMsg = TAG;
			try {
				dbMsg += "Checked=" + continuousPlayCheckBox.Checked;
				if (continuousPlayCheckBox.Checked) {
					viewSplitContainer.Panel1Collapsed = false;//リストエリアを開く
															   //		viewSplitContainer.Width = playListWidth;
					progresPanel.Visible = true;
					dbMsg += ";;playList準備；既存;" + PlayListBoxItem.Count + "件";
					PlayListBoxItem = new List<PlayListItems>();
					string valuestr = PlayListBoxItem.Count.ToString();
					string titolStr = carrentDir + "から" + type + "をリストアップ";
					int nowToTal = CurrentItemCount(passNameLabel.Text);
					dbMsg += ";nowToTal;" + nowToTal + "件";
					if (0 == nowToTal) {
						nowToTal = 100;
						dbMsg += ">>" + nowToTal + "件";
					}
					progressBar1.Maximum = nowToTal;
					ProgressTitolLabel.Text = titolStr;
					progCountLabel.Text = valuestr;                     //確認
					targetCountLabel.Text = "0";                        //リストアップ
					prgMessageLabel.Text = "リストアップ開始";
					ProgressMaxLabel.Text = nowToTal.ToString();        //Max
																		//		pDialog = new ProgressDialog(titolStr, maxvaluestr, valuestr);
																		//		pDialog.ShowDialog(this);										//プログレスダイアログ表示
					listUpDir = carrentDir;             //プレイリストにリストアップするデレクトリ
					System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(listUpDir);
					//		string parentDir = dirInfo.Parent.FullName.ToString();
					//		dbMsg += ",Parent=" + parentDir;
					string rootStr = dirInfo.Root.ToString();
					dbMsg += ",Root=" + rootStr;
					dbMsg += "；Dir;;Attributes=" + dirInfo.Attributes;
					if (listUpDir == rootStr) {                 //ドライブルートの場合
						upDirButton.Visible = false;                //上の階層ボタン非表示
					} else {
						upDirButton.Visible = true;
					}
					plPosisionLabel.Text = "リストアップ中";
					plPosisionLabel.Update();
					//再帰しながらプレイリストを作成/////////////////////
					PlayListBoxItem = ListUpFiles(listUpDir, type);
					/////////////////////再帰しながらプレイリストを作成//
					progresPanel.Visible = false;
					playListBox.DisplayMember = "NotationName";
					playListBox.ValueMember = "FullPathStr";
					playListBox.DataSource = PlayListBoxItem;
					dbMsg += ";PlayListBoxItem= " + PlayListBoxItem.Count + "件";
					dbMsg += ",plaingItem= " + plaingItem;
					string selStr = Path2titol(plaingItem);           //タイトル(ファイル名)だけを抜き出し
					dbMsg += "、再生中=" + selStr;
					int plaingID = playListBox.FindString(selStr);    //リスト内のインデックスを引き当て
					dbMsg += "; " + plaingID + "件目";
					dbMsg += ",plaingID=" + plaingID;
					if (-1 < plaingID) {
						//	playListBox.SetSelected( plaingID, true );
						playListBox.SelectedIndex = plaingID;           //リスト上で選択
					}
					PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), playListBox.SelectedValue.ToString());
					//			PlaylistComboBox.Items[0] = carrentDir;
				} else {
					viewSplitContainer.Panel1Collapsed = true;
					playListBox.Items.Clear();
				}
				//		MakeWebSouce( fileNameLabel.Text );
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		private void PlayFromPlayList(string plaingItem) {
			string TAG = "[PlayFromPlayList]";
			string dbMsg = TAG;
			try {
				dbMsg += "(" + playListBox.SelectedIndex + ")" + playListBox.Text;
				//	plaingItem = playListBox.SelectedValue.ToString();
				dbMsg += ";plaingItem=" + plaingItem;
				lsFullPathName = plaingItem;
				PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), plaingItem);
				MakeWebSouce(plaingItem);
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}


		/// <summary>
		/// プレイリストからの再生動作
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayListBox_Select(object sender, System.EventArgs e) {
			string TAG = "[PlayListBox_Select]";
			string dbMsg = TAG;
			try {
				dbMsg += "(" + playListBox.SelectedIndex + ")" + playListBox.Text;
				plaingItem = playListBox.SelectedValue.ToString();
				dbMsg += ";plaingItem=" + plaingItem;
				lsFullPathName = plaingItem;
				PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), plaingItem);
				MakeWebSouce(plaingItem);
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// プレイリストのラベルを更新する
		/// </summary>
		/// <param name="posisionStr">最上部のカウンタ数字</param>
		/// <param name="wrUrl">二つ、一つ上のフォルダ名</param>
		private void PlayListLabelWrigt(string posisionStr, string wrUrl) {
			string TAG = "[PlayListLabelWrigt]";
			string dbMsg = TAG;
			try {
				int plaingID = playListBox.SelectedIndex;
				dbMsg += "(" + plaingID + ")" + playListBox.Text;
				plPosisionLabel.Text = "";
				plPosisionLabel.Text = posisionStr;
				string[] souceNames = wrUrl.Split(Path.DirectorySeparatorChar);
				grarnPathLabel.Text = "";
				if (3 < souceNames.Length) {
					grarnPathLabel.Text = souceNames[souceNames.Length - 3];
				}
				parentPathLabel.Text = "";
				if (2 < souceNames.Length) {
					parentPathLabel.Text = souceNames[souceNames.Length - 2];
				}
				PlayListsplitContainer.Panel2.Update();
				//		MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// プレイリストの次へボタンをクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlNextBbutton_Click(object sender, EventArgs e) {
			string TAG = "[PlNextBbutton_Click]";
			string dbMsg = TAG;
			try {
				int plaingID = playListBox.SelectedIndex;
				dbMsg += "(" + plaingID + ")" + playListBox.Text;
				plaingID++;
				if ((PlayListBoxItem.Count - 1) < plaingID) {
					plaingID = 0;
				}
				playListBox.SelectedIndex = plaingID;           //リスト上で選択
				plaingItem = playListBox.SelectedValue.ToString();
				dbMsg += ">>(" + plaingID + ")" + playListBox.Text;
				dbMsg += ";plaingItem=" + plaingItem;
				lsFullPathName = plaingItem;
				PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), playListBox.SelectedValue.ToString());
				MakeWebSouce(plaingItem);
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// プレイリストの前へボタンをクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlRewButton_Click(object sender, EventArgs e) {
			string TAG = "[PlRewButton_Click]";
			string dbMsg = TAG;
			try {
				int plaingID = playListBox.SelectedIndex;
				dbMsg += "(" + plaingID + ")" + playListBox.Text;
				plaingID--;
				if (plaingID < 0) {
					plaingID = (PlayListBoxItem.Count - 1);
				}
				playListBox.SelectedIndex = plaingID;           //リスト上で選択
				plaingItem = playListBox.SelectedValue.ToString();
				dbMsg += ">>(" + plaingID + ")" + playListBox.Text;
				dbMsg += ";plaingItem=" + plaingItem;
				lsFullPathName = plaingItem;
				PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), playListBox.SelectedValue.ToString());
				MakeWebSouce(plaingItem);
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}

		}


		/// <summary>
		/// 連続再生チェックボックスのクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ContinuousPlayCheckBox_CheckedChanged(object sender, EventArgs e) {
			string TAG = "[ContinuousPlayCheckBox_CheckedChanged]";
			string dbMsg = TAG;
			try {
				dbMsg += "Checked=" + continuousPlayCheckBox.Checked;
				if (continuousPlayCheckBox.Checked) {
					plaingItem = passNameLabel.Text + Path.DirectorySeparatorChar + fileNameLabel.Text;
					dbMsg += ";plaingItem=" + plaingItem;
					lsFullPathName = plaingItem;
					string carrentDir = passNameLabel.Text;
					dbMsg += ",carrentDir=" + carrentDir;
					string type = typeName.Text;
					dbMsg += ",type=" + type;
					SetPlayListItems(carrentDir, type);
					playListRedoroe.Visible = true;
				} else {
					playListRedoroe.Visible = true;
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// 上の階層からボタンで再リストアップ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UpDirButton_Click(object sender, EventArgs e) {
			string TAG = "[upDirButton_Click]";
			string dbMsg = TAG;
			try {
				string carrentDir = PlaylistComboBox.Items[0].ToString();           //listUpDir;             //プレイリストにリストアップするデレクトリ
				dbMsg += ",現在Dir=" + carrentDir;
				System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(carrentDir);
				string parentDir = dirInfo.Parent.FullName.ToString();
				dbMsg += ",Parent=" + parentDir;
				string rootStr = dirInfo.Root.ToString();
				dbMsg += ",Root=" + rootStr;
				dbMsg += "；Dir;;Attributes=" + dirInfo.Attributes;
				//		if (carrentDir != rootStr) {
				string type = typeName.Text;
				dbMsg += ",type=" + type;
				SetPlayListItems(parentDir, type);
				//	}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// プレイリストのコンテキストメニュ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayListContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {
			string TAG = "[PlayListContext]";
			string dbMsg = TAG;
			try {
				dbMsg += ",ClickedItem=" + e.ClickedItem.Name;                             //e=		常にSystem.Windows.Forms.TreeViewEventArgs,
				string clickedMenuItem = e.ClickedItem.Name.Replace("plToolStripMenuItem", "");         //他のコンテキストメニューと同じNameは使えないのでプレイリストはplを付ける
				dbMsg += ">>" + clickedMenuItem;
				PlayListContextMenuStrip.Close();                                           //☆ダイアログが出ている間、メニューが表示されっぱなしになるので強制的に閉じる
				switch (clickedMenuItem) {                                           // クリックされた項目の Name を判定します。 
					case "ファイルブラウザで選択":
						dbMsg += ",選択；ファイルブラウザで選択=" + plRightClickItemUrl;
						treeSelectList = new List<int>();
						FindSelectFileViews(fileTree.Nodes, 0, 0, plRightClickItemUrl);
						break;

					case "削除":
						dbMsg += ",選択；削除；" + plRightClickItemUrl;


						DelFromPlayList(PlaylistComboBox.Text, plIndex);
						break;

					case "他のアプリケーションで開く":
						dbMsg += ",選択；他のアプリケーションで開く；" + plRightClickItemUrl;
						SartApication(plRightClickItemUrl);
						break;

					case "プレイリストに追加":
						dbMsg += ",選択；プレイリストに追加；" + plRightClickItemUrl;
						AddPlayListFromFile(plRightClickItemUrl);
						break;

					case "プレイリストを作成":
						dbMsg += ",選択；プレイリストを作成；" + plRightClickItemUrl;
						//ここから他のメソッドを呼べない？？
						break;

					default:
						break;
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}


		private void PlaylistComboBox_TextChanged(object sender, EventArgs e) {
			string TAG = "[PlaylistComboBox_TextChanged]";
			string dbMsg = TAG;
			try {
				string selePLName = PlaylistComboBox.SelectedItem.ToString();//    "M:\\DL\\2013.m3u"  object { string}
				dbMsg += ",selePLName=" + selePLName;
				if (selePLName.Contains(".m3u")) {
					ReadPlayList(selePLName);
				} else if (0 == PlaylistComboBox.SelectedIndex) {
					string setType = "video";
					if (typeName.Text == "audio") {
						setType = typeName.Text;
					}
					dbMsg += ",setType=" + setType;
					continuousPlayCheckBox.Checked = true;
					SetPlayListItems(selePLName, setType);
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// プレイリストDataSource
		/// 参照　　　https://www.ipentec.com/document/document.aspx?page=csharp-listbox-use-original-datasource
		/// </summary>
		class PlayListItems
		{
			string notationName;            //表記名
			string fullPathStr;             //フルパス名

			public PlayListItems() {
			}
			public PlayListItems(string notation, string pathStr) {
				notationName = notation;
				fullPathStr = pathStr;
			}

			public string NotationName
			{
				set {
					notationName = value;
				}
				get {
					return notationName;
				}
			}

			public string FullPathStr
			{
				set {
					fullPathStr = value;
				}
				get {
					return fullPathStr;
				}
			}
		}
		//playList///////////////////////////////////////////////////////////連続再生//
		/// <summary>
		/// 汎用プレイリストの読み込みとリスト作成
		/// </summary>
		/// <param name="fileName"></param>
		private void ReadPlayList(string fileName) {
			string TAG = "[ReadPlayList]" + fileName;
			string dbMsg = TAG;
			try {
				string rText = ReadTextFile(fileName, "UTF-8"); //"Shift_JIS"では文字化け発生
																//	dbMsg += ",rText=" + rText;
																//	rText = rText.Replace('/', Path.DirectorySeparatorChar);
				string[] files = System.Text.RegularExpressions.Regex.Split(rText, "\r\n");
				plaingItem = files[0];
				dbMsg += ",一行目=" + plaingItem;
				Uri urlObj = new Uri(plaingItem);                    //  http://dobon.net/vb/dotnet/file/uritofilepath.html
				if (urlObj.IsFile) {                     //変換するURIがファイルを表していることを確認する
					plaingItem = urlObj.LocalPath + Uri.UnescapeDataString(urlObj.Fragment);                          //Windows形式のパス表現に変換する
					dbMsg += "  >> " + plaingItem;
				}


				string type = GetFileTypeStr(fileName);
				string titolStr = fileName + "から" + type + "をリストアップ";
				viewSplitContainer.Panel1Collapsed = false;//リストエリアを開く
														   //		viewSplitContainer.Width = playListWidth;
				progresPanel.Visible = true;
				dbMsg += ";;playList準備；既存;" + PlayListBoxItem.Count + "件";
				PlayListBoxItem = new List<PlayListItems>();
				string valuestr = PlayListBoxItem.Count.ToString();
				int listCount = 0;
				//		int tCount = 0;
				targetCountLabel.Text = listCount.ToString();                    //確認
				int nowToTal = files.Length;
				dbMsg += ";nowToTal;" + nowToTal + "件";
				/*		if (0 == nowToTal) {
							nowToTal = 100;
							dbMsg += ">>" + nowToTal + "件";
						}*/
				progressBar1.Maximum = nowToTal;
				ProgressTitolLabel.Text = titolStr;
				progCountLabel.Text = valuestr;                     //確認
				targetCountLabel.Text = "0";                        //リストアップ
				prgMessageLabel.Text = "リストアップ開始";
				ProgressMaxLabel.Text = nowToTal.ToString();        //Max
																	//		pDialog = new ProgressDialog(titolStr, maxvaluestr, valuestr);保留；プログレスダイアログ
																	//		pDialog.ShowDialog(this);										//プログレスダイアログ表示

				if (files != null) {
					dbMsg += "ファイル=" + files.Length + "件";
					string rFileName = "";
					foreach (string plFileName in files) {
						listCount++;
						dbMsg += "\n(" + listCount + ")" + plFileName;
						if (plFileName != "") {
							FileInfo fi = new FileInfo(fileName);
							if (fi.Exists) {                     //変換するURIがファイルを表していることを確認する☆読み込み時にリロードのループになる
								string winPath = plFileName;
								urlObj = new Uri(plFileName);                    //  http://dobon.net/vb/dotnet/file/uritofilepath.html
								if (urlObj.IsFile) {                     //変換するURIがファイルを表していることを確認する
									winPath = urlObj.LocalPath + Uri.UnescapeDataString(urlObj.Fragment);                          //Windows形式のパス表現に変換する
									dbMsg += "  ,winPath= " + winPath;
									string[] pathStrs = winPath.Split(Path.DirectorySeparatorChar);
									winPath = winPath.Replace((":" + Path.DirectorySeparatorChar), ":" + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar);
									dbMsg += "Path=" + winPath;
									string wrTitol = Path2titol(winPath);//Path2titol2(plFileName, "/");
									dbMsg += ",Titol=" + wrTitol;
									PlayListItems pli = new PlayListItems(wrTitol, winPath);
									PlayListBoxItem.Add(pli);      //	ListBoxItem.Add( mi );//	tNode.Nodes.Add( fileName, rfileName, iconType, iconType );
																   //	tCount = Int32.Parse(targetCountLabel.Text) + 1;
									prgMessageLabel.Text = pathStrs[pathStrs.Length - 1];
									prgMessageLabel.Update();
								} else {
									dbMsg += "はファイルURIではありません。";
								}
							} else {
								dbMsg += "は正常に読み込めません。";
							}
							targetCountLabel.Text = listCount.ToString();                    //確認
							targetCountLabel.Update();
							int checkCount = Int32.Parse(progCountLabel.Text) + 1;                          //pDialog.GetProgValue() + 1;
							dbMsg += ",vCount=" + checkCount;
							progCountLabel.Text = checkCount.ToString();                   //確認
							progCountLabel.Update();
							if (progressBar1.Maximum < checkCount) {
								progressBar1.Maximum = checkCount + 10;
								ProgressMaxLabel.Text = progressBar1.Maximum.ToString();        //Max
								ProgressMaxLabel.Update();
							}
							progressBar1.Value = checkCount;
							progresPanel.Update();
							PlayListLabelWrigt(listCount.ToString(), plFileName);
							//	pDialog.RedrowPDialog(checkCount.ToString(),  maxvaluestr, nowCount.ToString(), wrTitol);   保留；プログレスダイアログ更新

						} else {
							dbMsg += "処理スキップ";
						}
						rFileName = plFileName;
					}
					typeName.Text = GetFileTypeStr(rFileName);
					typeName.Update();
				}
				progresPanel.Visible = false;
				playListBox.DisplayMember = "NotationName";
				playListBox.ValueMember = "FullPathStr";
				playListBox.DataSource = PlayListBoxItem;
				dbMsg += ";PlayListBoxItem= " + PlayListBoxItem.Count + "件";
				dbMsg += " , plaingItem= " + plaingItem;
				string selStr = Path2titol(plaingItem);//Path2titol2(plFileName, "/");          //タイトル(ファイル名)だけを抜き出し
				dbMsg += "、再生中=" + selStr;
				int plaingID = playListBox.FindString(selStr);    //リスト内のインデックスを引き当て
				dbMsg += "; " + plaingID + "件目";
				dbMsg += ",plaingID=" + plaingID;
				if (-1 < plaingID) {
					playListBox.SelectedIndex = plaingID;           //リスト上で選択
					PlayListLabelWrigt((playListBox.SelectedIndex + 1).ToString() + "/" + PlayListBoxItem.Count.ToString(), playListBox.SelectedValue.ToString());
				}
				//		MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		private void AddPlayListFromFile(string addFileName) {
			string TAG = "[AddPlayListFromFile]" + addFileName;
			string dbMsg = TAG;
			try {
				//Windows API Code Packの CommonOpenFileDialogを使用		//☆標準はOpenFileDialog
				CommonOpenFileDialog ofd = new CommonOpenFileDialog();              //OpenFileDialogクラスのインスタンスを作成☆
				ofd.Title = "プレイリストを選択してください";              //タイトルを設定する
															//	ofd.FileName = "default.m3u";                          //はじめのファイル名を指定する
															//はじめに「ファイル名」で表示される文字列を指定する


				string initialDirectory = @"C:\";
				if (passNameLabel.Text != "") {
					initialDirectory = passNameLabel.Text;
				}
				ofd.InitialDirectory = initialDirectory;              //はじめに表示されるフォルダを指定する
																	  //指定しない（空の文字列）の時は、現在のディレクトリが表示される
																	  /*	string[] filters = new string[]{
																		  "プレイリスト(*.m3u)|*.m3u",
																		  "All files(*.*)|*.*"
																   };
																		  ofd.Filters = String.Join("|", filters);*/
																	  //	ofd.Filter = "プレイリスト(*.m3u)|*.m3u|すべてのファイル(*.*)|*.*";               //[ファイルの種類]に表示される選択肢を指定する		"HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";  
																	  //指定しないとすべてのファイルが表示される
																	  //	ofd.FilterIndex = 2;                //[ファイルの種類]ではじめに選択されるものを指定する
																	  //2番目の「すべてのファイル」が選択されているようにする
				ofd.RestoreDirectory = true;                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
															//	ofd.CheckFileExists = true;             //存在しないファイルの名前が指定されたとき警告を表示する
															//デフォルトでTrueなので指定する必要はない
															//	ofd.CheckPathExists = true;             //存在しないパスが指定されたとき警告を表示する
															//デフォルトでTrueなので指定する必要はない

				CommonFileDialogComboBox OFDcomboBox = new CommonFileDialogComboBox();
				OFDcomboBox.Items.Add(new CommonFileDialogComboBoxItem("先頭に挿入"));
				OFDcomboBox.Items.Add(new CommonFileDialogComboBoxItem("末尾に追加"));
				OFDcomboBox.SelectedIndex = 0;
				ofd.Controls.Add(OFDcomboBox);

				if (ofd.ShowDialog() == CommonFileDialogResult.Ok) {        //OpenFileDialogでは == DialogResult.OK)
					nowLPlayList = ofd.FileName;
					dbMsg += ",選択されたファイル名=" + nowLPlayList;
					ComboBoxAddItems(PlaylistComboBox, nowLPlayList);
					/*		string[] PLArray = ComboBoxItems2StrArray(PlaylistComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };
							dbMsg += ",PLArray=" + PLArray.Length + "件";
							if (Array.IndexOf(PLArray, nowLPlayList) < 0) {         //既に登録されているリストでなければ
								PlaylistComboBox.Items.Add(nowLPlayList);
								appSettings.PlayLists = nowLPlayList;
								WriteSetting();
							}*/

					//			label1.Text = ofd.FileName;
					//			label2.Text = OFDcomboBox.Items[OFDcomboBox.SelectedIndex].Text;
				}

				dbMsg += ",PlayListFileNames=" + PlaylistComboBox.Items.Count + "件";
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// プレイリストに一行追加
		/// </summary>
		/// <param name="playList"></param>
		/// <param name="addRecord"></param>
		/// <param name="insarPosition"></param>
		private void Item2PlayListIndex(string playList, string addRecord, int insarPosition) {
			string TAG = "[Item2PlayListIndex]";
			string dbMsg = TAG;
			try {
				string uriPath = addRecord;
				Uri urlObj = new Uri(addRecord);                    //  http://dobon.net/vb/dotnet/file/uritofilepath.html
				if (urlObj.IsFile) {                     //変換するURIがファイルを表していることを確認する
					uriPath = urlObj.AbsoluteUri;
					uriPath = uriPath.Replace("://", ":/");
				}
				dbMsg += ",uriPath=" + uriPath;

				string rText = ReadTextFile(playList, "UTF-8"); //"Shift_JIS"では文字化け発生
				string[] items = System.Text.RegularExpressions.Regex.Split(rText, "\r\n");
				dbMsg += ",rText=" + items.Length + "件";
				List<string> stringList = new List<string>();
				stringList.AddRange(items);//配列→List
				dbMsg += ",stringList=" + stringList.Count + "件";
				stringList.Insert(insarPosition, uriPath);
				dbMsg += ">>" + stringList.Count + "件";
				rText = "";
				foreach (string lItem in stringList) {
					rText += lItem + "\r\n";
				}
				dbMsg += ">>" + rText.Length + "文字";
				System.IO.StreamWriter sw = new System.IO.StreamWriter(playList, false, new UTF8Encoding(true));     // BOM付き
				sw.Write(rText);
				sw.Close();
				dbMsg += ">Exists=" + File.Exists(playList);
				if (PlaylistComboBox.Text == playList) {
					ReadPlayList(playList);             //	再読込み
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}



		/// <summary>
		/// アイテムを一つプレイリストに追記
		/// </summary>
		/// <param name="addList"></param>
		/// <param name="addRecord"></param>
		/// <param name="toTop"></param>
		/// <returns></returns>
		private string Item2PlayListBody(string addList, string addRecord, bool toTop) {
			string TAG = "[Item2PlayListBody]";
			string dbMsg = TAG;
			try {
				string uriPath = addRecord;
				Uri urlObj = new Uri(addRecord);                    //  http://dobon.net/vb/dotnet/file/uritofilepath.html
				if (urlObj.IsFile) {                     //変換するURIがファイルを表していることを確認する
					uriPath = urlObj.AbsoluteUri;
					uriPath = uriPath.Replace("://", ":/");
				}
				/*
								string[] files = System.Text.RegularExpressions.Regex.Split(rText, "\r\n");
								plaingItem = files[0];
								dbMsg += ",一行目=" + plaingItem;
								*/
				dbMsg += ",uriPath=" + uriPath;

				if (toTop) {
					addList = uriPath + "\r\n" + addList;
				} else {
					addList = addList + "\r\n" + uriPath;
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
			return addList;
		}

		private string AddOne2PlayListBody(string addList, string addRecord, bool toTop) {
			string TAG = "[AddOne2PlayListBody]";
			string dbMsg = TAG;
			try {
				dbMsg += addList + "へ" + addRecord + "をtoTop=" + toTop;
				FileInfo fi = new FileInfo(addRecord);
				string fileAttributes = fi.Attributes.ToString();
				dbMsg += ",fileAttributes=" + fileAttributes;
				if (fileAttributes.Contains("Directory")) {
					System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(lsFullPathName);       //'C:\Users\博臣\AppData\Local\Application Data' へのアクセスが拒否されました。
					string[] files = Directory.GetFiles(addRecord);        //		sarchDir	"C:\\\\マイナンバー.pdf"	string	☆sarchDir = "\\2013.m3u"でフルパスになっていない
					foreach (string fileName in files) {
						dbMsg += ",fileName=" + fileName;
						addList = AddOne2PlayListBody(addList, fileName, toTop);
					}

					string[] foleres = Directory.GetDirectories(addRecord);//
					if (foleres != null) {
						foreach (string folereName in foleres) {
							if (-1 < folereName.IndexOf("RECYCLE", StringComparison.OrdinalIgnoreCase) ||
								-1 < folereName.IndexOf("System Vol", StringComparison.OrdinalIgnoreCase)) {
							} else {
								string rfolereName = folereName.Replace(addRecord, "");// + 
								rfolereName = rfolereName.Replace(Path.DirectorySeparatorChar + "", "");
								dbMsg += ",foler=" + rfolereName;
								addList = AddOne2PlayListBody(addList, addRecord, toTop);

							}
						}           //ListBox1に結果を表示する
					}
				} else {
					addList = Item2PlayListBody(addList, addRecord, toTop);
				}


				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
			return addList;
		}

		/// <summary>
		/// 指定したプレイリストも先頭か末尾にアイテムを追加
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="addRecord"></param>
		/// <param name="topBottom"></param>
		private void AddOne2PlayList(string fileName, string addRecord, bool toTop) {
			string TAG = "[AddOne2PlayList]" + fileName;
			string dbMsg = TAG;
			try {
				dbMsg += fileName + "へ" + addRecord + "をtoTop=" + toTop;
				string rText = ReadTextFile(fileName, "UTF-8"); //"Shift_JIS"では文字化け発生
																//	dbMsg += ",rText=" + rText;
																//	rText = rText.Replace('/', Path.DirectorySeparatorChar);
				rText = AddOne2PlayListBody(rText, addRecord, toTop);

				MyLog(dbMsg);
				System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName, false, new UTF8Encoding(true));     // BOM付き
				sw.Write(rText);
				sw.Close();
				dbMsg += ">Exists=" + File.Exists(fileName);
				if (PlaylistComboBox.Text == fileName) {
					ReadPlayList(fileName);             //	再読込み
				}


				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// プレイリストから指定した位置のアイテムを削除する
		/// ☆文字照合では同じアイテムを全て消してしまうので位置指定
		/// </summary>
		/// <param name="playList"></param>
		/// <param name="delPosition"></param>
		private void DelFromPlayList(string playList, int delPosition) {       //, string deldRecordp
			string TAG = "[DelFromPlayList]";
			string dbMsg = TAG;
			try {
				dbMsg += ",playList=" + playList + " から" + delPosition + " を削除";
				string rText = ReadTextFile(playList, "UTF-8"); //"Shift_JIS"では文字化け発生
																//	dbMsg += ",rText=" + rText;
																//	rText = rText.Replace('/', Path.DirectorySeparatorChar);
				dbMsg += ",rText=" + rText.Length + "文字";
				/*		string uriPath = deldRecordp;
							Uri urlObj = new Uri(deldRecordp);                    //  http://dobon.net/vb/dotnet/file/uritofilepath.html
							if (urlObj.IsFile) {                     //変換するURIがファイルを表していることを確認する
								uriPath = urlObj.AbsoluteUri;
								uriPath = uriPath.Replace("://", ":/");
							}
							uriPath = uriPath + "\r\n";
							dbMsg +=  " uriPath" + uriPath;
						//	rText = rText.Replace(uriPath, "");*/
				string[] items = System.Text.RegularExpressions.Regex.Split(rText, "\r\n");
				dbMsg += ",rText=" + items.Length + "件";
				List<string> stringList = new List<string>();
				stringList.AddRange(items);//配列→List
				dbMsg += ",stringList=" + stringList.Count + "件";
				string deldRecord = stringList[delPosition];
				dbMsg += ",deldRecordp=" + deldRecord;
				stringList.RemoveAt(delPosition);
				dbMsg += ",stringList=" + stringList.Count + "件";
				rText = "";
				foreach (string lItem in stringList) {
					rText += lItem + "\r\n";
				}
				dbMsg += ">>" + rText.Length + "文字";
				System.IO.StreamWriter sw = new System.IO.StreamWriter(playList, false, new UTF8Encoding(true));     // BOM付き
				sw.Write(rText);
				sw.Close();
				dbMsg += ">Exists=" + File.Exists(playList);
				if (PlaylistComboBox.Text == playList) {
					ReadPlayList(playList);             //	再読込み
				}

				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		private void MakePlayList(string fileName, string urlStr) {
			string TAG = "[MakePlayList]";
			string dbMsg = TAG;
			try {
				dbMsg += ",fileName=" + fileName;
				dbMsg += ",url=" + urlStr;
				int webWidth = playerWebBrowser.Width - 28;
				int webHeight = playerWebBrowser.Height - 60;
				dbMsg += ",web[" + webWidth + "×" + webHeight + "]";
				string[] extStrs = fileName.Split('.');
				string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();

				string contlolPart = @"<!DOCTYPE html>
<html>
	<head>
		<meta charset = " + '"' + "UTF-8" + '"' + " >\n";
				contlolPart += "\t\t<meta http-equiv = " + '"' + "Pragma" + '"' + " content =  " + '"' + "no-cache" + '"' + " />\n";          //キャッシュを残さない；HTTP1.0プロトコル
				contlolPart += "\t\t<meta http-equiv = " + '"' + "Cache-Control" + '"' + " content =  " + '"' + "no-cache" + '"' + " />\n"; //キャッシュを残さない；HTTP1.1プロトコル
				contlolPart += "\t\t<meta http-equiv = " + '"' + "X-UA-Compatible" + '"' + " content =  " + '"' + "requiresActiveX =true" + '"' + " />\n";
				//	contlolPart += "\n\t\t\t<link rel = " + '"' + "stylesheet" + '"' + " type = " + '"' + "text/css" + '"' + " href = " + '"' + "brows.css" + '"' + "/>\n";
				string retType = GetFileTypeStr(fileName);
				dbMsg += ",retType=" + retType;
				if (retType == "video" ||
					 retType == "image" ||
					retType == "audio"
					) {
				} else {
					contlolPart += "\t</head>\n";
					contlolPart += "\t<body>\n\t\t";
				}
				dbMsg += ",fileName=" + fileName;
				if (lsFullPathName != fileName) {       //8/31;仮対応；書き換わり対策
					dbMsg += ",***書き換わり発生***" + fileName;
					fileName = lsFullPathName;
				}

				if (retType == "video") {
					contlolPart += MakeVideoSouce(fileName, webWidth, webHeight);
				} else if (retType == "image") {
					contlolPart += MakeImageSouce(fileName, webWidth, webHeight);
				} else if (retType == "audio") {
					contlolPart += MakeAudioSouce(fileName, webWidth, webHeight);
				} else if (retType == "text") {
					contlolPart += MakeTextSouce(fileName, webWidth, webHeight);
				} else if (retType == "application") {
					contlolPart += MakeApplicationeSouce(fileName, webWidth, webHeight);
				}
				if (debug_now) {
					contlolPart += "\t\t<div>,urlStr=" + urlStr;
					contlolPart += "<br>\n\t\t" + ",playerUrl=" + playerUrl + "</div>\n";
				}
				contlolPart += "\t</body>\n</html>\n\n";
				dbMsg += ",contlolPart=" + contlolPart;
				if (File.Exists(urlStr)) {
					dbMsg += "既存ファイル有り";
					System.IO.File.Delete(urlStr);                //20170818;ここで停止？
					dbMsg += ">Exists=" + File.Exists(urlStr);
				}
				////UTF-8でテキストファイルを作成する
				System.IO.StreamWriter sw = new System.IO.StreamWriter(urlStr, false, System.Text.Encoding.UTF8);
				sw.Write(contlolPart);
				sw.Close();
				dbMsg += ">Exists=" + File.Exists(urlStr);
				Uri nextUri = new Uri("file://" + urlStr);
				dbMsg += ",nextUri=" + nextUri;
				try {
					playerWebBrowser.Navigate(nextUri);
				} catch (System.UriFormatException er) {
					dbMsg += "<<playerWebBrowser.Navigateでエラー発生>>" + er.Message;
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}//形式に合わせたhtml作成


		/// <summary>
		/// ファイルセレクトからプレイリストを選択する
		/// </summary>
		public void SelectPlayList() {
			string TAG = "[SelecPlayList]";// + fileName;
			string dbMsg = TAG;
			try {
				string initialDirectory = appSettings.CurrentFile;
				if (passNameLabel.Text != "") {
					initialDirectory = passNameLabel.Text;
				}
				string initialFile = "*.m3u";

				string[] PLArray = ComboBoxItems2StrArray(PlaylistComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };		playerUrl
				dbMsg += ",PLArray=" + PLArray.Length + "件";
				if (0 < PLArray.Length) {
					nowLPlayList = PLArray[0].ToString();// PlayListFileNames[PlayListFileNames.Count()-1].ToString();
					dbMsg += ",nowLPlayList=" + nowLPlayList;
					string[] iDirectorys = nowLPlayList.Split(Path.DirectorySeparatorChar);
					initialFile = iDirectorys[iDirectorys.Length - 1];
					dbMsg += ",initialFile=" + initialFile;
					initialDirectory = nowLPlayList.Replace(initialFile, "");
					dbMsg += ",initialDirectory=" + initialDirectory;
				}

				OpenFileDialog ofd = new OpenFileDialog();              //OpenFileDialogクラスのインスタンスを作成
				ofd.FileName = initialFile;                          //はじめのファイル名を指定する
																	 //はじめに「ファイル名」で表示される文字列を指定する
				ofd.InitialDirectory = initialDirectory;              //はじめに表示されるフォルダを指定する
																	  //指定しない（空の文字列）の時は、現在のディレクトリが表示される
				ofd.Filter = "プレイリスト(*.m3u)|*.m3u|すべてのファイル(*.*)|*.*";               //[ファイルの種類]に表示される選択肢を指定する		"HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";  
																					//指定しないとすべてのファイルが表示される
																					//	ofd.FilterIndex = 2;                //[ファイルの種類]ではじめに選択されるものを指定する
																					//2番目の「すべてのファイル」が選択されているようにする
				ofd.Title = "プレイリストを選択してください";              //タイトルを設定する
				ofd.RestoreDirectory = true;                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
				ofd.CheckFileExists = true;             //存在しないファイルの名前が指定されたとき警告を表示する
														//デフォルトでTrueなので指定する必要はない
				ofd.CheckPathExists = true;             //存在しないパスが指定されたとき警告を表示する
														//デフォルトでTrueなので指定する必要はない

				if (ofd.ShowDialog() == DialogResult.OK) {              //ダイアログを表示する
					nowLPlayList = ofd.FileName;                                               //	string fileName= ofd.FileName;
					dbMsg += ",選択されたファイル名=" + nowLPlayList;
					ComboBoxAddItems(PlaylistComboBox, nowLPlayList);
					if (passNameLabel.Text == "") {
						FileInfo fi = new FileInfo(nowLPlayList);
						dbMsg += ",Directory=" + fi.Directory.ToString();
						passNameLabel.Text = fi.Directory.ToString();
					}

				}
				PLArray = ComboBoxItems2StrArray(PlaylistComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };		playerUrl
				dbMsg += ">>PLArray=" + PLArray.Length + "件";
				viewSplitContainer.Panel1Collapsed = false;//リストエリアを開く

				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}

		}


		private List<String> SarchExtFilsBody(string carrentDir, string sarchExtention, List<String> PlayListFileNames) {
			string TAG = "[SarchExtFilsBody]" + sarchExtention;
			string dbMsg = TAG;
			try {
				dbMsg += "carrentDir=" + carrentDir + ",sarchExtention=" + sarchExtention;
				string sarchDir = carrentDir;
				string[] files = Directory.GetFiles(sarchDir);
				int listCount = -1;
				int tCount = PlayListFileNames.Count();
				int nowCount = PlayListFileNames.Count;
				int checkCount = progressBar1.Value;
				dbMsg += "/PlayListBoxItem; " + PlayListBoxItem.Count + "件";
				string wrTitol = "";
				//		int nowToTal = CurrentItemCount(sarchDir);      // サブディレクトリ内のファイルもカウントする場合	, SearchOption.AllDirectories
				//		dbMsg += ",このデレクトリには" + nowToTal + "件";
				int barMax = progressBar1.Maximum;
				dbMsg += ",progressMax=" + barMax + "件";
				/*	if (barMax < nowToTal) {
						progressBar1.Maximum = nowToTal;
						ProgressMaxLabel.Text = progressBar1.Maximum.ToString();        //Max
						ProgressMaxLabel.Update();
					}*/
				if (files != null) {
					dbMsg += "ファイル=" + files.Length + "件";
					foreach (string plFileName in files) {
						listCount++;
						dbMsg += "\n(" + listCount + ")" + plFileName;
						string[] pathStrs = plFileName.Split(Path.DirectorySeparatorChar);
						System.IO.FileAttributes attr = System.IO.File.GetAttributes(plFileName);
						if ((attr & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden) {
							dbMsg += ">>Hidden";
						} else if ((attr & System.IO.FileAttributes.System) == System.IO.FileAttributes.System) {
							dbMsg += ">>System";
						} else if (-1 == Array.IndexOf(systemFiles, plFileName)) {
							string[] extStrs = plFileName.Split('.');
							string extentionStr = "." + extStrs[extStrs.Length - 1].ToLower();
							dbMsg += "拡張子=" + extentionStr;
							if (extentionStr == sarchExtention) {
								//				string wrPathStr = plFileName.Replace((":" + Path.DirectorySeparatorChar), ":" + Path.DirectorySeparatorChar + Path.DirectorySeparatorChar);
								//			dbMsg += "Path=" + wrPathStr;
								dbMsg += ",Titol=" + wrTitol;
								PlayListFileNames.Add(plFileName);      //	ListBoxItem.Add( mi );//	tNode.Nodes.Add( fileName, rfileName, iconType, iconType );
								tCount = PlayListFileNames.Count();//Int32.Parse(targetCountLabel.Text) + 1;
								targetCountLabel.Text = tCount.ToString();                    //確認
								targetCountLabel.Update();
								//				prgMessageLabel.Text = plFileName;// pathStrs[pathStrs.Length - 1];
							}
							//		prgMessageLabel.Text = plFileName;       //StackOverflowException
							//		prgMessageLabel.Update();
						}
						checkCount = progressBar1.Value + 1;// Int32.Parse(progCountLabel.Text) + 1;                          //pDialog.GetProgValue() + 1;
						dbMsg += ",checkCount=" + checkCount.ToString();

						progCountLabel.Update();
						if (progressBar1.Maximum < checkCount) {
							progressBar1.Maximum = checkCount + 100;
							ProgressMaxLabel.Text = (progressBar1.Maximum - 100).ToString();        //Max
																									//		ProgressMaxLabel.Update();
						}
						progressBar1.Value = checkCount;
						//	progresPanel.Update();
						//	PlayListLabelWrigt(tCount.ToString(), plFileName);
						//	pDialog.RedrowPDialog(checkCount.ToString(),  maxvaluestr, nowCount.ToString(), wrTitol);   保留；プログレスダイアログ更新
					}
				}
				prgMessageLabel.Text = carrentDir;       //StackOverflowException
				progCountLabel.Text = checkCount.ToString();                   //$exception	{"種類 'System.StackOverflowException' の例外がスローされました。"}	System.StackOverflowException
				progresPanel.Update();

				string[] foleres = Directory.GetDirectories(sarchDir);//
				if (foleres != null) {
					foreach (string folereName in foleres) {
						System.IO.FileAttributes attr = System.IO.File.GetAttributes(folereName);
						if ((attr & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden) {
							dbMsg += ">>Hidden";
						} else if ((attr & System.IO.FileAttributes.System) == System.IO.FileAttributes.System) {
							dbMsg += ">>System";
						} else if (0 < Array.IndexOf(systemFiles, folereName)) {
						} else {
							SarchExtFilsBody(folereName, sarchExtention, PlayListFileNames);        //再帰
						}
					}
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
			return PlayListFileNames;
		}

		/// <summary>
		/// 指定した拡張子のファイルをリストアップ
		/// </summary>
		/// <param name="sarchExtention"></param>
		private void SarchExtFils(string sarchExtention) {
			string TAG = "[sarchExtFils]" + sarchExtention;
			string dbMsg = TAG;
			try {
				int dCount = 0;
				//	int fCount = 0;
				dbMsg += ",driveNames=" + PlayListFileNames.Count + "件";

				if (0 < PlayListFileNames.Count) {
					PlayListFileNames = new List<String>();
				}
				progresPanel.Visible = true;
				prgMessageLabel.Text = "リストアップ開始";
				progressBar1.Maximum = 100;
				progressBar1.Value = 0;

				ProgressTitolLabel.Text = "プレイリスト " + sarchExtention + "を抽出";
				foreach (DriveInfo drive in DriveInfo.GetDrives()) { /////http://www.atmarkit.co.jp/fdotnet/dotnettips/557driveinfo/driveinfo.html
					dCount++;
					string driveNames = drive.Name; // ドライブ名
					dbMsg += ",driveNames=" + driveNames;
					if (drive.IsReady) { // 使用可能なドライブのみ
						PlayListFileNames = SarchExtFilsBody(driveNames, sarchExtention, PlayListFileNames);
						progresPanel.Update();
						progresPanel.Focus();
					}
				}
				progresPanel.Visible = false;
				dbMsg += ",=" + PlayListFileNames.Count() + "件";
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// リストファイル選択ボタンのクリック
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayListFileAddBottun_Click(object sender, EventArgs e) {
			SelectPlayList();
		}

		/// <summary>
		/// 指定したComboBoxのアイテムをstring[]で返す
		/// </summary>
		/// <param name="readComboBox"></param>
		/// <param name="startCount"></param>
		/// <returns></returns>
		public string[] ComboBoxItems2StrArray(ComboBox readComboBox, int startCount) {
			string TAG = "[ComboBoxItems2StrArray]";// + fileName;
			string dbMsg = TAG;
			string[] PLArray = { };       //new string[1]ではnullが一つ入る
			try {
				int ArraySize = readComboBox.Items.Count;
				dbMsg += ",Items=" + ArraySize + "件";
				if (0 < ArraySize) {
					PLArray = new string[ArraySize - 1];// { PlaylistComboBox.Items.Contains() };
					for (int i = startCount; i <= ArraySize - startCount; i++) {
						dbMsg += "(" + i + ")";
						string addItem = readComboBox.Items[i].ToString();
						dbMsg += addItem;
						PLArray[i - startCount] = addItem;
					}
					dbMsg += ",PLArray=" + PLArray.Length + "件";
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
			return PLArray;
		}

		/// <summary>
		/// 指定したComboBoxにアイテムを追加する
		/// </summary>
		/// <param name="wrComboBox"></param>
		/// <param name="addItemeName"></param>
		public void ComboBoxAddItems(ComboBox wrComboBox, string addItemeName) {
			string TAG = "[ComboBoxAddItems]";// + fileName;
			string dbMsg = TAG;
			try {
				dbMsg += wrComboBox.Name + "　に　" + addItemeName + "を追加";
				string[] PLArray = ComboBoxItems2StrArray(wrComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };
				dbMsg += ",PLArray=" + PLArray.Length + "件";
				bool wr = true;
				if (0 < PLArray.Length) {
					if (-1 < Array.IndexOf(PLArray, addItemeName)) {         //既に登録されているリストでなければ
						wr = false;
					}
				}
				dbMsg += ",追記=" + wr;
				if (wr) {
					wrComboBox.Items.Add(addItemeName);
					PLArray = ComboBoxItems2StrArray(wrComboBox, 1);//new string[] { PlaylistComboBox.Items.ToString() };
					dbMsg += ",PLArray=" + PLArray.Length + "件";
					appSettings.PlayLists = PLArray;
					WriteSetting();

				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}


		private void PlayListBox_Click(object sender, EventArgs e) {
			string TAG = "[PlayListBox_Click]";
			string dbMsg = TAG;
			try {
				dbMsg += "(" + playListBox.SelectedIndex + ")" + playListBox.Text;
				plaingItem = playListBox.SelectedValue.ToString();
				dbMsg += ";plaingItem=" + plaingItem;
				PlayFromPlayList(plaingItem);
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}

		}

		private void PlayListBox_MouseClick(object sender, MouseEventArgs e) {
			string TAG = "[PlayListBox_MouseClick]";
			string dbMsg = TAG;
			try {
				dbMsg += "(" + playListBox.SelectedIndex + ")" + playListBox.Text;
				plaingItem = playListBox.SelectedValue.ToString();
				dbMsg += ";plaingItem=" + plaingItem;
				PlayFromPlayList(plaingItem);
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}

		}


		/// <summary>
		/// 始めのマウスクリック
		/// https://dobon.net/vb/dotnet/control/draganddrop.html
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PlayListBox_MouseDown(object sender, MouseEventArgs e) {
			string TAG = "[PlayListBox_MouseDown]";// + fileName;
			string dbMsg = TAG;
			try {
				draglist = (ListBox)sender;
				PlayListMouseDownNo = draglist.SelectedIndex;
				dbMsg += "(Down;" + PlayListMouseDownNo + ")";
				 PlayListMouseDownValue = draglist.SelectedValue.ToString();
				dbMsg += PlayListMouseDownValue;

				dragSouceIDl = draglist.SelectedIndex;
				dbMsg += "(dragSouc;" + dragSouceIDl + ")";
				dragSouceUrl = draglist.SelectedValue.ToString();
				dbMsg += "Url;" + dragSouceUrl;
				mouceDownPoint = Control.MousePosition;
				mouceDownPoint = draglist.PointToClient(mouceDownPoint);//ドラッグ開始時のマウスの位置をクライアント座標に変換
				dbMsg += "(mouceDownPoint;" + mouceDownPoint.X + "," + mouceDownPoint.Y + ")";
				dragSouceIDP = draglist.IndexFromPoint(mouceDownPoint);//マウス下のListBoxのインデックスを得る
				dbMsg += "(Pointから;" + dragSouceIDP + ")";
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}


		private void PlayListBox_MouseMove(object sender, MouseEventArgs e) {
			string TAG = "[PlayListBox_MouseMove]";
			string dbMsg = TAG;
			try {
				dbMsg += "(MovePoint;" + e.X + "," + e.Y + ")";

				/*	dbMsg += "Button=" + e.Button;
					if (e.Button == System.Windows.Forms.MouseButtons.Left) {        //左ボタン*/
				if (mouceDownPoint != Point.Empty) {
			//		Point mouceMovePoint = Control.MousePosition;
			//		mouceMovePoint = draglist.PointToClient(mouceMovePoint);//ドラッグ開始時のマウスの位置をクライアント座標に変換
					dbMsg += "(DownPoint;" + mouceDownPoint.X + "," + mouceDownPoint.Y + ")";
					if (mouceDownPoint.X != e.X || mouceDownPoint.Y != e.Y) {
						if (-1 < dragSouceIDP) {
							draglist.DoDragDrop(draglist.Items[dragSouceIDP].ToString(), DragDropEffects.Move);//ドラッグスタート
						}           //http://www.kisoplus.com/sample2/sub/listbox.html
									//		https://dobon.net/vb/dotnet/control/draganddrop.html

					}
				}

				/*		https://dobon.net/vb/dotnet/control/draganddrop.html
		if (PlaylistMouseDownPoint != Point.Empty) {
				dbMsg += "(" + PlaylistMouseDownPoint.X + " , " + PlaylistMouseDownPoint.Y + " ) ";
				Rectangle moveRect = new Rectangle(
					PlaylistMouseDownPoint.X - SystemInformation.DragSize.Width / 2,
					PlaylistMouseDownPoint.Y - SystemInformation.DragSize.Height / 2,
					SystemInformation.DragSize.Width,
					SystemInformation.DragSize.Height);                 //ドラッグとしないマウスの移動範囲を取得する
				if (!moveRect.Contains(e.X, e.Y)) {																//ドラッグとする移動範囲を超えたか調べる
					ListBox lbx = (ListBox)sender;																//ドラッグの準備
					int itemIndex = lbx.IndexFromPoint(PlaylistMouseDownPoint);                                 //ドラッグするアイテムのインデックスを取得する
					dbMsg += "(itemIndex;" + itemIndex + ")";
					if (itemIndex < 0) { return; }
					string itemText = (string)lbx.Items[itemIndex];                                             //ドラッグするアイテムの内容を取得する
					dbMsg += ";" + itemText;
					DragDropEffects dde =lbx.DoDragDrop(itemText,DragDropEffects.All | DragDropEffects.Link);   //ドラッグ&ドロップ処理を開始する
					if (dde == DragDropEffects.Move) {															//ドロップ効果がMoveの時はもとのアイテムを削除する
						lbx.Items.RemoveAt(itemIndex);
						PlaylistMouseDownPoint = Point.Empty;
					}
					//??		PlaylistMouseDownPoint = Point.Empty;
				}
			}
			*/
							MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// マウスボタンを離す
		/// 右クリックされたアイテムからフルパスをグローバル変数に設定
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void PlaylistBoxMouseUp(object sender, MouseEventArgs e) {
			string TAG = "[PlaylistBoxMouseUp]";
			string dbMsg = TAG;
			try {
				ListBox droplist = (ListBox)sender;
				PlaylistMouseUp = droplist.SelectedIndex;
				dbMsg += "(MouseUp:" + PlaylistMouseUp + ")";
				string listSelectValue = "";
				if (-1< PlaylistMouseUp) {
					listSelectValue = droplist.SelectedValue.ToString();
					dbMsg += listSelectValue;
				}

				if (e.Button == System.Windows.Forms.MouseButtons.Right) {
					dbMsg += "右ボタンを離した";
					plIndex = playListBox.IndexFromPoint(e.Location);             //プレイリスト上のマウス座標から選択すべきアイテムのインデックスを取得
					dbMsg += ",index=" + plIndex;
					if (plIndex >= 0) {               // インデックスが取得できたら
						plRightClickItemUrl = PlayListBoxItem[plIndex].FullPathStr;
						dbMsg += ",plRightClickItemUrl=" + plRightClickItemUrl;
						//	playListBox.ClearSelected();                    // すべての選択状態を解除してから
						//playListBox.SelectedIndex = index;                  // アイテムを選択
						// コンテキストメニューを表示
						Point pos = playListBox.PointToScreen(e.Location);
						dbMsg += ",pos=" + pos;
						PlayListContextMenuStrip.Show(pos);
					}
				} else if (e.Button == System.Windows.Forms.MouseButtons.Left) {        //左ボタン
					dbMsg += "左ボタンを離した(" + PlayListMouseDownNo + ")" + dragSouceUrl;
					int listChangeNo = droplist.SelectedIndex;      //移動先のインデックスを取得
					dbMsg += ">>(" + listChangeNo;
					if (PlayListMouseDownNo != listChangeNo) {
						dbMsg += "に移動";
						/*	ここでdropを受け取る場合	http://note.phyllo.net/?eid=563651	☆カーソルエフェクトは無い
										if (-1 < dragSouceIDl && "" != dragSouceUrl) {
											string playList = PlaylistComboBox.Text;
											if (draglist == droplist) {         //プレイリスト内の移動なら
												DelFromPlayList(playList, dragSouceIDl);
												if (dragSouceIDl < listChangeNo) {
													listChangeNo--;
												}
											}
											dragSouceIDl = -1;
											Item2PlayListIndex(playList, dragSouceUrl, listChangeNo);
											dragSouceUrl = "";
											dbMsg += ",最終選択=" + listChangeNo;
											droplist.SelectedIndex = listChangeNo;          //選択先のインデックスを指定
											plaingItem = playListBox.SelectedValue.ToString();
											dbMsg += ";plaingItem=" + plaingItem;
										}*/
					} else {
						dbMsg += ")" + listSelectValue;
						if (listSelectValue == PlayListMouseDownValue) {
							PlayFromPlayList(listSelectValue);
						}
					}
				} else {
					dbMsg += "dragではない";
				}
				mouceDownPoint = Point.Empty;
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/*
				private void PlayListBox_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
					string TAG = "[PlayListBox_GiveFeedback]";
					string dbMsg = TAG;
					try {
						/*		https://dobon.net/vb/dotnet/control/draganddrop.html

		dbMsg += "Effect=" + e.Effect.ToString();
		e.UseDefaultCursors = false;                //既定のカーソルを使用しない
		//ドロップ効果にあわせてカーソルを指定する
		if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move) {
		//			Cursor.Current = moveCursor;
		} else if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy) {
		//			Cursor.Current = copyCursor;
		} else if ((e.Effect & DragDropEffects.Link) == DragDropEffects.Link) {
		//			Cursor.Current = linkCursor;
		} else {
		//			Cursor.Current = noneCursor;
		}

						MyLog(dbMsg);
					} catch (Exception er) {
						dbMsg += "<<以降でエラー発生>>" + er.Message;
						MyLog(dbMsg);
					}
				}
		*/
		/*
				private void PlayListBox_QueryContinueDrag(object sender, QueryContinueDragEventArgs e) {
					string TAG = "[PlayListBox_QueryContinueDrag]";
					string dbMsg = TAG;
					try {


						/*		https://dobon.net/vb/dotnet/control/draganddrop.html
		//マウスの右ボタンが押されていればドラッグをキャンセル
		//"2"はマウスの右ボタンを表す
		dbMsg += "KeyState=" + e.KeyState;
		if ((e.KeyState & 2) == 2) {
			e.Action = DragAction.Cancel;
		}

						MyLog(dbMsg);
					} catch (Exception er) {
						dbMsg += "<<以降でエラー発生>>" + er.Message;

						MyLog(dbMsg);
					}
				}
		*/
		private void PlayListBox_DragEnter(object sender, DragEventArgs e) {
			string TAG = "[PlayListBox_DragEnter]";
			string dbMsg = TAG;
			try {
				e.Effect = DragDropEffects.Move;//ドラッグ＆ドロップの効果を、Moveに設定

				/*		ListBox list = (ListBox)sender;
				PlaylistDragEnterNo = list.SelectedIndex;
							dbMsg += "(DragEnter;" + PlaylistDragOverNo + ")";
							string listSelectValue = list.SelectedValue.ToString();
							dbMsg += listSelectValue;*/
				//		e.Effect = DragDropEffects.All;     //http://www.itlab51.com/?p=2904
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/*		/// <summary>
				/// ドラッグ中
				/// </summary>
				/// <param name="sender"></param>
				/// <param name="e"></param>
				private void PlayListBox_DragOver(object sender, DragEventArgs e) {
					string TAG = "[PlayListBox_DragOver]";// + fileName;
					string dbMsg = TAG;
					try {
						/*		https://dobon.net/vb/dotnet/control/draganddrop.html

		if (e.Data.GetDataPresent(typeof(string))) {                //ドラッグされているデータがstring型か調べる
			if ((e.KeyState & 8) == 8 && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy) {                //Ctrlキーが押されていればCopy//"8"はCtrlキーを表す
				e.Effect = DragDropEffects.Copy;
			} else if ((e.KeyState & 32) == 32 && (e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link) {   //Altキーが押されていればLink//"32"はAltキーを表す
				e.Effect = DragDropEffects.Link;
			} else if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move) {                              //何も押されていなければMove
				e.Effect = DragDropEffects.Move;
			} else {
				e.Effect = DragDropEffects.None;
			}
		} else {
			e.Effect = DragDropEffects.None;                    //string型でなければ受け入れない
		}

						MyLog(dbMsg);
					} catch (Exception er) {
						dbMsg += "<<以降でエラー発生>>" + er.Message;
						MyLog(dbMsg);
					}
				}
		*/
		private void PlayListBox_DragDrop(object sender, DragEventArgs e) {
			string TAG = "[PlayListBox_DragDrop]";
			string dbMsg = TAG;
			try {

				ListBox droplist = (ListBox)sender;
				string str = e.Data.GetData(DataFormats.Text).ToString();               //ドラッグしてきたアイテムの文字列をstrに格納する
				dbMsg += "Drop;" + str;

				Point dropPoint = Control.MousePosition;
				dropPoint = playListBox.PointToClient(dropPoint);//ドロップ時のマウスの位置をクライアント座標に変換
				dbMsg += "(dropPoint;" + dropPoint.X + "," + dropPoint.Y + ")";
				int listChangeNo = playListBox.IndexFromPoint(dropPoint);//マウス下のＬＢのインデックスを得る
				dbMsg += "(ind;" + listChangeNo + ")" + str;
				if (listChangeNo > -1 && listChangeNo < playListBox.Items.Count) {
					dbMsg += "を;" + listChangeNo + "に移動";
					if (dragSouceIDl != listChangeNo) {
						if (-1 < dragSouceIDl && "" != dragSouceUrl) {
							string playList = PlaylistComboBox.Text;
							if (draglist == droplist) {         //プレイリスト内の移動なら
								DelFromPlayList(playList, dragSouceIDl);
								if (dragSouceIDl < listChangeNo) {
									listChangeNo--;
								}
							}
							dragSouceIDl = -1;
							Item2PlayListIndex(playList, dragSouceUrl, listChangeNo);
							dragSouceUrl = "";
							dbMsg += ",最終選択=" + listChangeNo;
							droplist.SelectedIndex = listChangeNo;          //選択先のインデックスを指定
							plaingItem = playListBox.SelectedValue.ToString();
							dbMsg += ";plaingItem=" + plaingItem;
						}
					}

					//					playListBox.Items[dragSouceIDP] = playListBox.Items[ind];
					//					playListBox.Items[ind] = str;
				}



				/*	http://www.itlab51.com/?p=2904
				foreach (string item in (string[])e.Data.GetData(DataFormats.FileDrop)) {
					playListBox.Items.Add(item);
				}
				*/
				/*		https://dobon.net/vb/dotnet/control/draganddrop.html
if (e.Data.GetDataPresent(typeof(string))) {                    //ドロップされたデータがstring型か調べる
	ListBox target = (ListBox)sender;
	string itemText = (string)e.Data.GetData(typeof(string));   //ドロップされたデータ(string型)を取得
	if ((e.Effect & DragDropEffects.Link) == DragDropEffects.Link) {
		itemText = "[Link]" + itemText;
	}
	target.Items.Add(itemText);                 //ドロップされたデータをリストボックスに追加する
}
*/
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/*
		<M3U／WPL共通＞
		¡最大ディレクトリ階層 ：8階層
		¡最大フォルダ名／最大ファイル名文字数 ：半角28文字
		¡フォルダ名／ファイル名使用可能文字 ：A〜Z（全角／半角）、0〜9（全角／半角）、
		_（アンダースコア）、全角漢字（JIS 第2水準まで）、
		ひらがな、カタカナ（全角／半角）
		¡最大プレイリストファイル数 ：30
		¡1プレイリストファイル中の最大ファイル数 ：100

		*/
		//システムメニュー///////////////////////////////////////////////////////////playList//
		private void ReWriteSysMenu() {
			string TAG = "[ReWriteSysMenu]";
			string dbMsg = TAG;
			try {
				IntPtr hSysMenu = GetSystemMenu(this.Handle, false);

				MENUITEMINFO item1 = new MENUITEMINFO();                //メニュー要素はMENUITEMINFO構造体に値を設定
				item1.cbSize = (uint)Marshal.SizeOf(item1);             //構造体のサイズ
				item1.fMask = MIIM_FTYPE;                               //この構造体で設定するメンバを指定
				item1.fType = MFT_SEPARATOR;                            //
				InsertMenuItem(hSysMenu, 5, true, ref item1);           //①メニューのハンドル②識別子または位置③uItem パラメータの意味④メニュー項目の情報

				MENUITEMINFO item20 = new MENUITEMINFO();
				item20.cbSize = (uint)Marshal.SizeOf(item20);
				item20.fMask = MIIM_STRING | MIIM_ID;
				item20.wID = MENU_ID_20;                                 //メニュー要素を識別するためのID
				item20.dwTypeData = "ファイルブラウザ";
				InsertMenuItem(hSysMenu, 6, true, ref item20);

				MENUITEMINFO item60 = new MENUITEMINFO();
				item60.cbSize = (uint)Marshal.SizeOf(item60);
				item60.fMask = MIIM_STRING | MIIM_ID;
				item60.wID = MENU_ID_60;
				item60.dwTypeData = "プレイリスト";
				InsertMenuItem(hSysMenu, 7, true, ref item60);

				MENUITEMINFO item99 = new MENUITEMINFO();
				item99.cbSize = (uint)Marshal.SizeOf(item60);
				item99.fMask = MIIM_STRING | MIIM_ID;
				item99.wID = MENU_ID_99;
				item99.dwTypeData = "バージョン情報";
				InsertMenuItem(hSysMenu, 8, true, ref item99);

				/*			IntPtr hMenu = GetSystemMenu(this.Handle, 0);           // タイトルバーのコンテキストメニューを取得
							AppendMenu(hMenu, MF_SEPARATOR, 0, string.Empty);           // セパレータとメニューを追加
							AppendMenu(hMenu, MF_STRING, MF_BYCOMMAND, "プレイリスト");
							AppendMenu(hMenu, MF_SEPARATOR, 0, string.Empty);           // セパレータとメニューを追加
							AppendMenu(hMenu, MF_STRING, MF_BYCOMMAND, "バージョン情報");
							*/

				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// システムメニューの動作
		/// </summary>
		/// <param name="m"></param>
		protected override void WndProc(ref Message m) {
			string TAG = "[WndProc]";
			string dbMsg = TAG;
			try {
				dbMsg += "Message=" + m.ToString();
				base.WndProc(ref m);
				dbMsg += ",m.Msg=" + m.Msg.ToString();
				if (m.Msg == WM_SYSCOMMAND) {
					uint menuid = (uint)(m.WParam.ToInt32() & 0xffff);
					dbMsg += ",menuid=" + menuid.ToString();

					switch (menuid) {
						case MENU_ID_20:
							dbMsg += ",FileBrowserSplitContainer=" + baseSplitContainer.Panel1Collapsed;
							if (baseSplitContainer.Panel1Collapsed) {
								baseSplitContainer.Panel1Collapsed = false;//ファイルブラウザを開く
							} else {
								baseSplitContainer.Panel1Collapsed = true;//ファイルブラウザを閉じる

							}
							break;
						case MENU_ID_60:
							dbMsg += ",viewSplitContainer=" + viewSplitContainer.Panel1Collapsed;
							if (viewSplitContainer.Panel1Collapsed) {
								viewSplitContainer.Panel1Collapsed = false;//リストエリアを開く
							} else {
								viewSplitContainer.Panel1Collapsed = true;//リストエリアを閉じる

							}
							break;
						case MENU_ID_99:
							System.Diagnostics.FileVersionInfo ver = System.Diagnostics.FileVersionInfo.GetVersionInfo(
								System.Reflection.Assembly.GetExecutingAssembly().Location);
							dbMsg += ",ver=" + ver.ToString();
							MessageBox.Show(ver.ProductVersion.ToString(),                 //H:\develop\dnet\AWCFilebrowser\Properties\AssemblyInfo.cs の[assembly: AssemblyFileVersion( "1.2.0.4" )]
							"バージョン情報", MessageBoxButtons.OK,
							MessageBoxIcon.Information);
							break;
					}
				}
				//		MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}
		//設定///////////////////////////////////////////////////////////システムメニュー//
		//		https://dobon.net/vb/dotnet/programing/storeappsettings.html
		/// <summary>
		/// プリファレンスの変更イベント
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Default_SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e) {
			string TAG = "[Default_SettingChanging]";
			string dbMsg = TAG;
			try {
				dbMsg += "変更=" + e.SettingName;
				dbMsg += " を" + e.NewValue.ToString() + "に";
				//変更しようとしている設定が"Text"のとき
				if (e.SettingName == "CurrentFile") {
					//設定しようとしている値を取得
					string str = e.NewValue.ToString();
					if (str.Length > 10) {
						//変更をキャンセルする
						e.Cancel = true;
					}
				}
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// config書込み
		/// </summary>
		private void WriteSetting() {
			string TAG = "[WriteSetting]";
			string dbMsg = TAG;
			try {
				dbMsg += "configFileName=" + configFileName;
				dbMsg += " , CurrentFile=" + appSettings.CurrentFile;
				dbMsg += " , PlayLists=" + appSettings.PlayLists.Length + "件";

				//＜XMLファイルに書き込む＞
				System.Xml.Serialization.XmlSerializer serializer1 = new System.Xml.Serialization.XmlSerializer(typeof(Settings));       //XmlSerializerオブジェクトを作成
																																		 //書き込むオブジェクトの型を指定する
				System.IO.StreamWriter sw = new System.IO.StreamWriter(configFileName, false, new System.Text.UTF8Encoding(false));     //ファイルを開く（UTF-8 BOM無し）
				serializer1.Serialize(sw, appSettings);                                                                                 //シリアル化し、XMLファイルに保存する
				sw.Close();                                                                                                             //閉じる

				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}

		/// <summary>
		/// config読込み
		/// </summary>
		private void ReadSetting() {
			string TAG = "[ReadSetting]";
			string dbMsg = TAG;
			try {
				dbMsg += "configFileName=" + configFileName;
				if (File.Exists(configFileName)) {
					//＜XMLファイルから読み込む＞
					System.Xml.Serialization.XmlSerializer serializer2 = new System.Xml.Serialization.XmlSerializer(typeof(Settings));   //XmlSerializerオブジェクトの作成
					System.IO.StreamReader sr = new System.IO.StreamReader(configFileName, new System.Text.UTF8Encoding(false));        //ファイルを開く
					appSettings = (Settings)serializer2.Deserialize(sr);                                                                    //XMLファイルから読み込み、逆シリアル化する
					sr.Close();                                                                                                         //閉じる

					dbMsg += " , CurrentFile=" + appSettings.CurrentFile;
					if (appSettings.CurrentFile != "") {
						playerUrl = appSettings.CurrentFile.ToString();
						System.IO.FileInfo fi = new System.IO.FileInfo(playerUrl);   //変更元のFileInfoのオブジェクトを作成します。 @"C:\files1\sample1.txt" 
						fileNameLabel.Text = playerUrl;
						//if (fi.Directory.Name != "") {
						passNameLabel.Text = fileNameLabel.Text.Replace(fi.Name, "");
						PlaylistComboBox.Items.Add(passNameLabel.Text);
						/*	if (passNameLabel.Text != @"C:\") {
								PlaylistComboBox.Items[0] = passNameLabel.Text;

							}*/
						//	}
						dbMsg += ">PlaylistComboBox0=>" + PlaylistComboBox.Items[0].ToString();
					}
					dbMsg += " , PlayLists=" + appSettings.PlayLists.Length + "件";
					if (0 < appSettings.PlayLists.Length) {
						foreach (string fileName in appSettings.PlayLists) {
							PlaylistComboBox.Items.Add(fileName);
						}
					}
				} else {
					appSettings = new Settings();
				}


				/*			AWSFileBroeser.Properties.Settings.Default.Upgrade();              //前のバージョンの設定を読み込み、新しいバージョンの設定とする
							string rStr = AWSFileBroeser.Properties.Settings.Default.GetPreviousVersion("CurrentFile");                //前のバージョンの設定"Text"の値を取得
								dbMsg += "変更=" + e.SettingName;
							dbMsg += " を" + e.NewValue.ToString() + "に";
							//変更しようとしている設定が"Text"のとき
							if (e.SettingName == "CurrentFile") {
								//設定しようとしている値を取得
								string str = e.NewValue.ToString();
								if (str.Length > 10) {
									//変更をキャンセルする
									e.Cancel = true;
								}
							}*/
				MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}


		/// <summary>
		/// 設定の管理クラス
		/// </summary>
		public class Settings
		{
			private string currentFile;
			private string[] playLists;

			public string CurrentFile
			{
				get { return currentFile; }
				set { currentFile = value; }
			}
			public string[] PlayLists
			{
				get { return playLists; }
				set { playLists = value; }
			}

			public Settings() {
				currentFile = @"C:\Users";
				playLists = new string[1] { "*.m3u" }; ;
			}
		}

		//その他///////////////////////////////////////////////////////////設定//
		/// <summary>
		/// フルパスを示す文字列からコンテンツのタイトルになる文字列を抜き出す
		/// </summary>
		/// <param name="pathStr">パスを示す文字列</param>
		/// <returns>タイトル</returns>
		private string Path2titol(string pathStr) {
			string TAG = "[Path2titol]";
			string dbMsg = TAG;
			string retStr = "";
			try {
				dbMsg += "pathStr=" + pathStr;
				string[] names = pathStr.Split(Path.DirectorySeparatorChar);           //
				retStr = names[names.Length - 1];
				dbMsg += ",retStr=" + retStr;
				string[] names2 = retStr.Split('.');
				retStr = names2[0];
				dbMsg += " >>" + retStr;
				//		MyLog( dbMsg );
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
			return retStr;
		}

		private void GetFileListByType(string type) {
			string TAG = "[GetFileListByType]" + type;
			string dbMsg = TAG;
			try {
				//		MyLog( dbMsg );
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				MyLog(dbMsg);
			}
		}


		//デバッグツール///////////////////////////////////////////////////////////その他//
		Boolean debug_now = true;
		public void MyLog(string msg) {
			if (debug_now) {
				Console.WriteLine(msg);
			}
		}

		//http://www.usefullcode.net/2016/03/index.html
	}
}
