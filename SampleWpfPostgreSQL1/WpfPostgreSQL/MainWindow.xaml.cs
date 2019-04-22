using log4net;
using System;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Windows;

using System.IO;
using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Generic;

using Microsoft.WindowsAPICodePack.Dialogs;

using MahApps.Metro.Controls;

using System.ComponentModel;

using Npgsql;
using System.ComponentModel;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();
            // SampleDb.sqlite を作成（存在しなければ）
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            //using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
            using (var conn = new NpgsqlConnection("Server=localhost; Port=5432; Database=DB01;User Id=USER01;Password=USER01;"))
            {
                // 接続
                conn.Open();

                // コマンドの実行
                using (var command = conn.CreateCommand())
                {
                    // テーブルが存在しなければ作成する
                    // 種別マスタ
                    StringBuilder sb = new StringBuilder();
                    sb.Append("CREATE TABLE IF NOT EXISTS mstkind (");
                    sb.Append(" kind_cd CHAR(2) NOT NULL");
                    sb.Append(" , kind_name VARCHAR(20)");
                    sb.Append(" , PRIMARY KEY (kind_cd)");
                    sb.Append(")");
                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();

                    // 猫テーブル
                    sb.Clear();
                    sb.Append("CREATE TABLE IF NOT EXISTS tblcat (");
                    sb.Append(" no INTEGER NOT NULL");
                    sb.Append(" , name VARCHAR(20) NOT NULL");
                    sb.Append(" , sex CHAR(3) NOT NULL");
                    sb.Append(" , age INTEGER DEFAULT 0 NOT NULL");
                    sb.Append(" , kind_cd CHAR(2) DEFAULT '00' NOT NULL");
                    sb.Append(" , favorite VARCHAR(40)");
                    sb.Append(" , PRIMARY KEY (no)");
                    sb.Append(")");

                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();
                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();
                }
                // 切断
                conn.Close();
            }

            using (var context = new PgDbContext())
            {

                // データを取得
                var mstKind = context.Kinds;
                IQueryable<Kind> result = from x in mstKind orderby x.KindCd select x;

                // 最初の要素は「指定なし」とする
                Kind empty = new Kind();
                empty.KindCd = "";
                empty.KindName = "指定なし";
                var list = result.ToList();
                list.Insert(0, empty);

                // コンボボックスに設定
                this.search_kind.ItemsSource = list;
                this.search_kind.DisplayMemberPath = "KindName";
                this.search_kind.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 検索ボタンクリックイベント.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void search_button_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("検索ボタンクリック");

            Object[] param = new Object[2];
            param[0] = this.search_name.Text;
            param[1] = (this.search_kind.SelectedValue as Kind).KindCd;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += SearchProcess;
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SearchProcessCompleted);
            worker.RunWorkerAsync(param);

            ToggleProgressRing();

        }

        /// <summary>
        /// データ検索処理.
        /// </summary>
        private void searchData()
        {
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            //using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
            // PgDbContext に変更
            using (var context = new PgDbContext())
            {

                // 猫データマスタを取得してコンボボックスに設定する
                String searchName = this.search_name.Text;
                String searchKind = (this.search_kind.SelectedValue as Kind).KindCd;

                // データを取得
                // context から DbSet を取得
                //Table<Cat> tblCat = con.GetTable<Cat>();
                var tblCat = context.Cats;

                // サンプルなので適当に組み立てる
                IQueryable<Cat> result;
                if (searchKind == "")
                {
                    // 名前は前方一致のため常に条件していしても問題なし
                    result = from x in tblCat
                             where x.Name.StartsWith(searchName)
                             orderby x.No
                             select x;
                }
                else
                {
                    result = from x in tblCat
                             where x.Name.StartsWith(searchName) & x.Kind == searchKind
                             orderby x.No
                             select x;

                }
                this.dataGrid.ItemsSource = result.ToList();
            }
        }

        /// <summary>
        /// 検索処理（非同期）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchProcess(object sender, DoWorkEventArgs e)
        {
            // 猫データマスタを取得してコンボボックスに設定する
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            // PgDbContext に変更
            using (var context = new PgDbContext())
            {
                String searchName = (e.Argument as Object[])[0] as String;
                String searchKind = (e.Argument as Object[])[1] as String;

                // データを取得
                // context から DbSet を取得
                //Table<Cat> tblCat = con.GetTable<Cat>();
                var tblCat = context.Cats;

                // サンプルなので適当に組み立てる
                IQueryable<Cat> result;
                if (searchKind == "")
                {
                    // 名前は前方一致のため常に条件していしても問題なし
                    result = from x in tblCat
                             where x.Name.StartsWith(searchName)
                             orderby x.No
                             select x;
                }
                else
                {
                    result = from x in tblCat
                             where x.Name.StartsWith(searchName) & x.Kind == searchKind
                             orderby x.No
                             select x;

                }
                e.Result = result.ToList();
            }
        }

        /// <summary>
        /// 検索完了処理（非同期）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchProcessCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.dataGrid.ItemsSource = e.Result as List<Cat>;
            ToggleProgressRing();
        }

        /// <summary>
        /// 処理中イメージの表示／非表示切り替え
        /// 
        /// </summary>
        private void ToggleProgressRing()
        {
            if (this.loading_image.IsActive)
            {
                this.loading_image.IsActive = false;
                this.rec_overlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.loading_image.IsActive = true;
                this.rec_overlay.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// 追加ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void add_button_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("追加ボタンクリック");

            // データを追加する
            // PgDbContext に変更
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            using (var context = new PgDbContext())
            {
                // データ作成
                // テーブルはコンテキスト経由でアクセスする
                //var table = context.GetTable<Cat>();
                Cat cat = new Cat();
                cat.No = 5;
                cat.No = 5;
                cat.Name = "こなつ";
                cat.Sex = "♀";
                cat.Age = 7;
                cat.Kind = "01";
                cat.Favorite = "布団";
                // データ追加
                // コンテキスト経由でエンティティを追加
                //table.InsertOnSubmit(cat);
                context.Cats.Add(cat);
                // DBの変更を確定
                // メソッド変更
                //context.SubmitChanges();
                context.SaveChanges();
            }

            // データ再検索
            searchData();
            MessageBox.Show("データを追加しました。");
        }

        /// <summary>
        /// 更新ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void upd_button_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("更新ボタンクリック");

            // 選択チェック
            if (this.dataGrid.SelectedItem == null)
            {
                MessageBox.Show("更新対象を選択してください。");
                return;
            }

            // データを更新する
            // PgDbContext に変更
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            using (var context = new PgDbContext())
            {
                // 対象のテーブルオブジェクトを取得
                // テーブルはコンテキスト経由でアクセスする
                //var table = context.GetTable<Cat>();
                var table = context.Cats;
                // 選択されているデータを取得
                Cat cat = this.dataGrid.SelectedItem as Cat;
                // テーブルから対象のデータを取得
                var target = table.Single(x => x.No == cat.No);
                // データ変更
                target.Favorite = "高いところ";
                // DBの変更を確定
                // メソッド変更
                //context.SubmitChanges();                
                context.SaveChanges();
            }

            // データ再検索
            searchData();

            MessageBox.Show("データを更新しました。");
        }

        /// <summary>
        /// 削除ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void del_button_Click(object sender, RoutedEventArgs e)
        {
            logger.Info("追加ボタンクリック");

            // 選択チェック
            if (this.dataGrid.SelectedItem == null)
            {
                MessageBox.Show("削除対象を選択してください。");
                return;
            }

            // データを削除する
            // PgDbContext に変更
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            using (var context = new PgDbContext())
            {
                // 対象のテーブルオブジェクトを取得
                // テーブルはコンテキスト経由でアクセスする
                //var table = context.GetTable<Cat>();
                var table = context.Cats;
                // 選択されているデータを取得
                Cat cat = this.dataGrid.SelectedItem as Cat;
                // テーブルから対象のデータを取得
                var target = table.Single(x => x.No == cat.No);
                // データ削除
                // メソッド変更
                //table.DeleteOnSubmit(target);
                table.Remove(target);
                // DBの変更を確定
                context.SaveChanges();
            }

            // データ再検索
            searchData();
            MessageBox.Show("データを削除しました。");
        }

        /// <summary>
        /// CSV読込ボタンクリックイベント.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imp_button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = "";
            ofd.DefaultExt = "*.csv";
            if (ofd.ShowDialog() == false)
            {
                return;
            }

            List<Cat> list = readFile(ofd.FileName);

            // 接続
            int count = 0;
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            //using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
            using (var context = new PgDbContext())
            {
                // 対象のテーブルオブジェクトを取得
                var table = context.Cats;

                // データを追加する
                foreach (Cat cat in list)
                {
                    // テーブルから対象のデータを取得
                    // データが存在するかどうか判定
                    if (table.SingleOrDefault(x => x.No == cat.No) == null)
                    {
                        // データ追加
                        // コンテキスト経由でエンティティを追加
                        //table.InsertOnSubmit(cat);
                        context.Cats.Add(cat);
                        // DBの変更を確定
                        // メソッド変更
                        //context.SubmitChanges();
                        context.SaveChanges();
                        count++;
                    }
                }
            }

            MessageBox.Show(count + " / " + list.Count + " 件 のデータを取り込みました。");

            // データ再検索
            searchData();
        }

        /// <summary>
        /// CSVファイル読み込み処理
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static List<Cat> readFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string ret = string.Empty;
            List<Cat> list = new List<Cat>();
            using (TextFieldParser tfp = new TextFieldParser(fileInfo.FullName, Encoding.GetEncoding("Shift_JIS")))
            {
                tfp.TextFieldType = FieldType.Delimited;
                tfp.Delimiters = new string[] { "," };
                tfp.HasFieldsEnclosedInQuotes = true;
                tfp.TrimWhiteSpace = true;
                while (!tfp.EndOfData)
                {
                    string[] fields = tfp.ReadFields();
                    Cat cat = new Cat();
                    cat.No = int.Parse(fields[0]);
                    cat.Name = fields[1];
                    cat.Sex = fields[2];
                    cat.Age = int.Parse(fields[3]);
                    cat.Kind = fields[4];
                    cat.Favorite = fields[5];
                    list.Add(cat);
                }
            }
            return list;
        }

        /// <summary>
        /// CSV出力ボタンクリックイベント.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exp_button_Click(object sender, RoutedEventArgs e)
        {
            // ファイル保存ダイアログ
            SaveFileDialog dlg = new SaveFileDialog();

            // デフォルトファイル名
            dlg.FileName = "cat.csv";

            // デフォルトディレクトリ
            dlg.InitialDirectory = @"c:\";

            // ファイルのフィルタ
            dlg.Filter = "CSVファイル|*.csv|すべてのファイル|*.*";

            // ファイルの種類
            dlg.FilterIndex = 0;

            // 指定されたファイル名を取得

            if (dlg.ShowDialog() == true)
            {
                List<Cat> list = this.dataGrid.ItemsSource as List<Cat>;
                String delmiter = ",";
                StringBuilder sb = new StringBuilder();
                Cat lastData = list.Last();
                foreach (Cat cat in list)
                {
                    sb.Append(cat.No).Append(delmiter);
                    sb.Append(cat.Name).Append(delmiter);
                    sb.Append(cat.Sex).Append(delmiter);
                    sb.Append(cat.Age).Append(delmiter);
                    sb.Append(cat.Kind).Append(delmiter);
                    sb.Append(cat.Favorite);
                    if (!cat.Equals(lastData))
                    {
                        sb.Append(Environment.NewLine);
                    }
                }

                Stream st = dlg.OpenFile();
                StreamWriter sw = new StreamWriter(st, Encoding.GetEncoding("UTF-8"));

                sw.Write(sb.ToString());
                sw.Close();
                st.Close();
                MessageBox.Show("CSVファイルを出力しました。");
            }
            else
            {
                MessageBox.Show("キャンセルされました。");
            }

        }

        /// <summary>
        /// ディレクトリ内のCSVファイルを全て読み込む
        /// </summary>
        /// <param name="sourceDir"></param>
        private List<Cat> readFiles(String sourceDir)
        {
            string[] files = Directory.GetFiles(sourceDir, "*.csv");
            List<Cat> list = new List<Cat>();
            // リストを走査してコピー
            for (int fileCount = 0; fileCount < files.Length; fileCount++)
            {
                List<Cat> catList = readFile(files[fileCount]) as List<Cat>;
                list.AddRange(catList);
            }

            return list;
        }

        /// <summary>
        /// フォルダ参照ボタンクリックイベント.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fld_button_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログ生成
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();

            // パラメタ設定

            // タイトル
            dlg.Title = "フォルダ選択";
            // フォルダ選択かどうか
            dlg.IsFolderPicker = true;
            // 初期ディレクトリ
            dlg.InitialDirectory = @"c:\";
            // ファイルが存在するか確認する
            //dlg.EnsureFileExists = false;
            // パスが存在するか確認する
            //dlg.EnsurePathExists = false;
            // 読み取り専用フォルダは指定させない
            //dlg.EnsureReadOnly = false;
            // コンパネは指定させない
            //dlg.AllowNonFileSystemItems = false;

            //ダイアログ表示
            var Path = dlg.ShowDialog();
            if (Path == CommonFileDialogResult.Ok)
            {
                // 選択されたフォルダ名を取得、格納されているCSVを読み込む
                List<Cat> list = readFiles(dlg.FileName);

                // 接続
                int count = 0;
                //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
                //using (var conn = new MySqlConnection("Database=DB01;Data Source=localhost;User Id=USER01;Password=USER01; sqlservermode=True;"))
                using (var context = new PgDbContext())
                {
                    // 対象のテーブルオブジェクトを取得
                    var table = context.Cats;

                    // データを追加する
                    foreach (Cat cat in list)
                    {
                        // テーブルから対象のデータを取得
                        // データが存在するかどうか判定
                        if (table.SingleOrDefault(x => x.No == cat.No) == null)
                        {
                            // データ追加
                            // コンテキスト経由でエンティティを追加
                            //table.InsertOnSubmit(cat);
                            context.Cats.Add(cat);
                            // DBの変更を確定
                            // メソッド変更
                            //context.SubmitChanges();
                            context.SaveChanges();
                            count++;
                        }
                    }
                }

                MessageBox.Show(count + " / " + list.Count + " 件 のデータを取り込みました。");

                // データ再検索
                searchData();
            }
        }
    }
}