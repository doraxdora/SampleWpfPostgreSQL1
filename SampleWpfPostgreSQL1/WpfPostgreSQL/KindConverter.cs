using System;
using System.Globalization;
using System.Linq;
// Converter 用
// IValueConverter、CultureInfo
using System.Windows.Data;


namespace WpfApp1
{
    /// <summary>
    /// 種別コンバータークラス.
    /// </summary>
    public class KindConverter : IValueConverter
    {
        /// <summary>
        /// データ変換処理
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //using (var conn = new SQLiteConnection("Data Source=SampleDb.sqlite"))
            // PgDbContext に変更
            using (var context = new PgDbContext())
            {
                // データを取得
                //Table<Kind> tblCat = con.GetTable<Kind>();
                //Kind k = tblCat.Single(c => c.KindCd == value as String);
                var kind = context.Kinds.SingleOrDefault(c => c.KindCd == (String)value);
                if (kind != null)
                {
                    return kind.KindName;
                }
                return "";
            }
        }

        /// <summary>
        /// データ復元処理
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            using (var context = new PgDbContext())
            {
                // データを取得
                //Table<Kind> tblCat = con.GetTable<Kind>();
                //Kind k = tblCat.Single(c => c.KindCd == value as String);
                var kind = context.Kinds.SingleOrDefault(c => c.KindName == value as String);
                if (kind != null)
                {
                    return kind.KindCd;
                }

            }
            return "";
        }
    }
}