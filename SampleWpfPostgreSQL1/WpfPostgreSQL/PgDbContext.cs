using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Entity;
using Npgsql;

namespace WpfApp1
{
    class PgDbContext : DbContext
    {
        private const string ConnectionString = "Server=localhost;User ID=USER01;Password=USER01;Database=DB01;port=5432";

        // コンストラクタにて接続文字列を設定
        public PgDbContext() : base(new NpgsqlConnection(ConnectionString), true) { }

        public DbSet<Kind> Kinds { get; set; }
        public DbSet<Cat> Cats { get; set; }

        // スキーマを変更する場合にはここに設定
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Configure default schema
            modelBuilder.HasDefaultSchema("dora");
        }
    }
}