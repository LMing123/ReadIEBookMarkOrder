using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestReadMenuOrder;

namespace ReadIEBookMarkOrder
{
    public class BookMarkModel
    {
        public string FullName { get; set; }
        public int SortIndex { get; set; }
        public BookMarkType BookMarkType { get; set; }
        public List<BookMarkModel> SubBookMark { get; set; }
    }
}
