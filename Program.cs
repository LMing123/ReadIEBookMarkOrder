using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReadIEBookMarkOrder
{
    class Program
    {
        public struct SHITEMID
        {
            public ushort cb;
            public byte[] abID;
        }
        public struct ITEMIDLIST
        {
            public SHITEMID mkid;
        }

        [DllImport("shell32.dll", SetLastError = true,ExactSpelling =true, ThrowOnUnmappableChar = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SHGetPathFromIDListW(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszPath);
        [DllImport("Ole32.dll")]
        public extern static uint CoInitialize(IntPtr pvReserved);
      
        [STAThread]
        static void Main(string[] args)
        {

            var reuslt = IEBookMark.GetBookMarkSort();
            reuslt.Item2.ForEach(x => Console.WriteLine($"Name: {x.FullName} SortIndex:{x.SortIndex}"));
            Console.ReadLine();
        }
    }
}
