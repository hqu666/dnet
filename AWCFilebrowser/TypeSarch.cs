using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using file_tree_clock_web1;


namespace AWSFileBroeser
{
	public partial class TypeSarch : Form
	{
		public string ResultText = "";      //追記先ファイル
		public bool AddPlayListTop = true;      //ユーザ入力値
		string sarchExtention="";
		List<String> ResultFileNames = new List<String>();

		Form1 rootForm = new Form1();

		public TypeSarch(string addFileName, string sarchExtention, string titolStr , string[] FileNames) {
			InitializeComponent();
			this.Text = titolStr;
			TypeSarchExtention.Text = sarchExtention;
			TypeSarchTargetURL.Text = addFileName;

			TypeSarchListBox.Items.AddRange(FileNames);
		}

		private void TypeSarchStartBt_Click(object sender, EventArgs e) {
			string TAG = "[TypeSarch_Load]" + sarchExtention;
			string dbMsg = TAG;
			try {
				//		sarchExtFils(sarchExtention);
				rootForm.MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				rootForm.MyLog(dbMsg);
			}


		}


		/// <summary>
		/// 指定した拡張子のファイルをリストアップ
		/// </summary>
		/// <param name="sarchExtention"></param>
	/*	private void sarchExtFils(string sarchExtention) {
			string TAG = "[sarchExtFils]" + sarchExtention;
			string dbMsg = TAG;
			try {
				int dCount = 0;
				int fCount = 0;
				foreach (DriveInfo drive in DriveInfo.GetDrives()) { /////http://www.atmarkit.co.jp/fdotnet/dotnettips/557driveinfo/driveinfo.html
					dCount++;
					string driveNames = drive.Name; // ドライブ名
					dbMsg += ",driveNames=" + driveNames;
					if (drive.IsReady) { // 使用可能なドライブのみ
						string[] rootFiles = Directory.GetFiles(driveNames, "*" + sarchExtention);
						dbMsg += ",rootFiles=" + rootFiles.Length.ToString() + "件";
						foreach (string rFile in rootFiles) {
							TypeSarchListBox.Items.Add(rFile);
						}
						string[] rootDirs = Directory.GetDirectories(driveNames, "*");
						dbMsg += ",rootDirs=" + rootDirs.Length.ToString() + "件";
						fCount += rootDirs.Length;
						foreach (string rDir in rootDirs) {
							dbMsg += "\nrDir="+ rDir;
							DirectoryInfo dirInfo = new DirectoryInfo(rDir);
									dbMsg += "；Dir;;Attributes=" + dirInfo.Attributes;
							//	if (dirInfo.Parent != null) {
							//	System.IO.FileAttributes attr = System.IO.Path.GetAttributes(passNameStr);
							if ((dirInfo.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden) {
								dbMsg += ">>Hidden";
							} else if ((dirInfo.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly) {
								dbMsg += ">>ReadOnly";
							} else if ((dirInfo.Attributes & System.IO.FileAttributes.System) == System.IO.FileAttributes.System) {
								dbMsg += ">>System";
							} else {
								if (rDir.Contains("Recycle") ||
									rDir.Contains("$") ||
									rDir.Contains("Program Files") ||
									rDir.Contains("Application Data") ||
									rDir.Contains("Windows") ||
									rDir.Contains("System ")) {
									dbMsg += ">>名前照合";
								} else {
									try {
										IEnumerable<string> fileList = Directory.EnumerateFiles(rDir, "*" + sarchExtention, SearchOption.AllDirectories); // 対象ファイルを検索する	 @"A_*年*月.xml"
										dbMsg += ", " + fileList.Count().ToString() + "件";
										foreach (string filePath in fileList) {
											dbMsg += "," + filePath;
											TypeSarchListBox.Items.Add(filePath);
											//	ResultFileNames.Add(filePath);
										}
										TypeSarchInfo.Text =  fCount.ToString() + " フォルダ/ " + dCount.ToString() + " ドライブ中";//TypeSarchListBox.Items.Count.ToString() + " / " +
										TypeSarchInfo.Update();
										TypeSarchInfo.Focus();
									} catch (UnauthorizedAccessException UAEx) {
										dbMsg += "<<以降でエラー発生>>" + UAEx.Message;
										rootForm.MyLog(dbMsg);
										throw;
									} catch (PathTooLongException PathEx) {
										dbMsg += "<<以降でエラー発生>>" + PathEx.Message;
										rootForm.MyLog(dbMsg);
										throw;
									} catch (Exception er) {
										dbMsg += "<<以降でエラー発生>>" + er.Message;
										rootForm.MyLog(dbMsg);
										throw;
									}
								}
							}

						}

					}
				}
				TypeSarchInfo.Text =  fCount.ToString() + " フォルダ/ " + dCount.ToString() + " ドライブ中";     //TypeSarchListBox.Items.Count.ToString() + " / " +
				TypeSarchInfo.Update();
				TypeSarchInfo.Focus();

				rootForm.MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				rootForm.MyLog(dbMsg);
			}
		}*/

		/// <summary>
		/// 追加先の再選択
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TypeSarchReSelectButton_Click(object sender, EventArgs e) {
			string TAG = "[TypeSarch.TypeSarchReSelectButton_Click]";// + fileName;
			string dbMsg = TAG;
			try {
				OpenFileDialog ofd = new OpenFileDialog();              //OpenFileDialogクラスのインスタンスを作成
				ofd.FileName = "default.m3u";                          //はじめのファイル名を指定する
																	   //はじめに「ファイル名」で表示される文字列を指定する
				ofd.InitialDirectory = @"C:\";              //はじめに表示されるフォルダを指定する
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
																		//	string fileName= ofd.FileName;
					dbMsg += ",選択されたファイル名=" + ofd.FileName;
					TypeSarchListBox.Items.Add(ofd.FileName);
				}
				rootForm.MyLog(dbMsg);
			} catch (Exception er) {
				dbMsg += "<<以降でエラー発生>>" + er.Message;
				rootForm.MyLog(dbMsg);
			}

		}
		
	}
}
