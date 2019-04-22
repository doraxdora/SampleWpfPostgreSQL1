using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WpfApp1
{
    [Table("tblcat")]
    class Cat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("no")]
        public int No { get; set; }
        [Column("name")]
        public String Name { get; set; }
        [Column("sex")]
        public String Sex { get; set; }
        [Column("age")]
        public int Age { get; set; }
        [Column("kind_cd")]
        public String Kind { get; set; }
        [Column("favorite")]
        public String Favorite { get; set; }
    }
}