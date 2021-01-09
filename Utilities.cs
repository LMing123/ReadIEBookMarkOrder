using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestReadMenuOrder
{
    public enum BookMarkType
    {
        Unknow,
        Directory,
        File
    }
    public class Utilities
    {
        public static BookMarkType GetBookMarkType(string path)
        {
            if (File.Exists(path)) return BookMarkType.File;
            else if (Directory.Exists(path)) return BookMarkType.Directory;
            else return BookMarkType.Unknow;
        }
    }
}
