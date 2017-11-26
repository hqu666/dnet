using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;   // 参照を追加。
using Npgsql;               // 参照を追加。
 
namespace EFSample
{
    public class TestConnection : DbContext // DbContextを継承。
    {
        #region 接続プロパティ

        /// <summary>
        /// テーブル接続情報。
        /// </summary>
        public DbSet<Table1> Table { get; set; }

        /// <summary>
        /// 参照するスキーマ。
        /// </summary>
        public string DefaultSchema { get; private set; }

        #endregion

        #region イベント

        /// <summary>
        /// スキーマを変更したい場合はここで変更。
        /// 指定が無いとスキーマ名は『dbo』に設定される。
        /// </summary>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
            => modelBuilder.HasDefaultSchema(DefaultSchema);

        #endregion

        #region メソッド

        /// <summary>
        /// コネクションの取得。
        /// </summary>
        /// <param name="userid">ユーザーID</param>
        /// <param name="password">パスワード</param>
        static NpgsqlConnection GetConnecting(string userid, string password)
        {
            var sb = new StringBuilder();
            sb.Append($"Server=接続先ホスト;")
            .Append($"Port=接続ポート番号;")
            .Append($"Database=データベース名;")
            .Append($"User Id={userid};")
            .Append($"Password={password};");
            return new NpgsqlConnection(sb.ToString());
        }

        #endregion

        #region コンストラクタ。

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="userid">接続ID。</param>
        /// <param name="password">接続パスワード。</param>
        /// <param name="defaultSchema">接続先スキーマ。</param>
        public TestConnection(string userid, string password, string defaultSchema)
            : base(GetConnecting(userid, password), true)
        {
            DefaultSchema = defaultSchema;
        }

        #endregion

    }
}