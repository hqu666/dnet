using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace EFSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // 接続インスタンスを作成。
            var dbc = new TestConnection("ユーザーID", "パスワード", "接続スキーマ");

            //
            // データベースの内容を取得(SELECT文)。
            //
            {
                // 全ての情報を取得。
                var allData = dbc.Table.ToList();

                // 絞り込んで取得。
                var filterData = dbc.Table.Where(w => w.Name == "絞り込むID").ToList();
            }
            //
            // 内容を変更(UPDATE文)。
            //
            {
                // 変更したいレコードを取得。
                var data = dbc.Table.First(f => f.Id == 0);

                // 取得データの内容を変更。
                data.Name = "変更する名称";
                data.LastSign = DateTime.Now;

                // 変更を反映。
                dbc.SaveChanges();
            }
            //
            // 新しいデータを登録(INSERT文)。
            //
            {
                // 登録する新規データの入れ物を作成。
                var data = dbc.Table.Create();

                // 内容を設定。
                data.Id = 1; // serial型の場合は無くても勝手に登録される(ハズ)。
                data.Name = "林 航嗣";
                data.Address = "kouji@creatorhyp.com";
                data.LastSign = DateTime.Now;

                // レコードををテーブルに登録。
                dbc.Table.Add(data);

                // 設定を反映。
                dbc.SaveChanges();
            }
            //
            // データを削除(DELETE文)。
            //
            {
                // 削除したいレコードを取得。
                var data = dbc.Table.First(f => f.Id == 1);

                // レコードを削除。
                dbc.Table.Remove(data);

                // 変更を反映。
                dbc.SaveChanges();
            }
        }
    }
}
