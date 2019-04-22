using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp1
{
    [Table("mstkind")]
    class Kind
    {
        [Key]
        [Column("kind_cd")]
        public String KindCd { get; set; }
        [Column("kind_name")]
        public String KindName { get; set; }
    }
}