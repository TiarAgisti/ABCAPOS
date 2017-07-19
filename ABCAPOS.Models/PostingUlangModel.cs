using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABCAPOS.Models
{
    public class PostingUlangModel
    {
        public long ID { get; set; }
        public DateTime Date { get; set; }
        public int Doctype { get; set; }
        public string DoctypeDesc { get; set; }
        public string InOut { get; set; }
    }
}
