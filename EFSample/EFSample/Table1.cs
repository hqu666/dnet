using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;        // 参照を追加。
using System.ComponentModel.DataAnnotations.Schema; // 参照を追加。

namespace EFSample
{
    /// <summary>
    /// https://creatorhyp.com/tips/program/entity-framework-install/
    /// </summary>
    [Table("table_info")] // テーブルの名前を入力。
                          // [Table("table_info", schema = "name")] // スキーマをここで設定する事も可能。
    public class Table1
    {
        [Key] // 主キーを設定。
        [Column("id")] // データベース上のカラム名を入力。
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("last_sign")]
        public DateTime LastSign { get; set; }
    }
}
