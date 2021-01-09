using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReadIEBookMarkOrder
{

    
    class IEBookMark
    {
        public static readonly string RegistryPah = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\MenuOrder";
        public static readonly string FavoritesKey = "Favorites";
        public static readonly string OrderValueName = "Order";

        static readonly int ItemCountOffset = 16; // offset of count value that ITEMIDLIST array number
        static readonly int ItemListBeginOffset = 20; // offset of begin that ITEMIDLIST array 

        static readonly int ItemSizeOffset = 0;
        static readonly int ItemSortIndexOffset = 4;
        static readonly int ItemIDListOffset = 8;

        [DllImport("shell32.dll", SetLastError = true, ExactSpelling = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SHGetPathFromIDListW(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszPath);
        public static (bool, List<BookMarkModel>,string message) GetBookMarkSort()
        {
            byte[] value = GetOrderFromRegistry($"{RegistryPah}\\{FavoritesKey}");
            if (value == null || value.Length == 0) return (false, null,"can not access registry value");
            int valueSize = value.Length;
            var pVaule = Marshal.AllocHGlobal(valueSize);
            Marshal.Copy(value, 0, pVaule, value.Length);

            if (!BinaryRead(ref pVaule, valueSize, ItemCountOffset, out int itemCount)) return (false, null,"can not get item count");

            List<BookMarkModel> result = new List<BookMarkModel>();
            int baseOffset = ItemListBeginOffset;
            for (int i = 0; i < itemCount; i++)
            {
                if (!BinaryRead(ref pVaule, valueSize,baseOffset+ItemSizeOffset, out int itemSize)
                    || baseOffset + itemSize > valueSize
                    || baseOffset + itemSize <= baseOffset) return (false, null,"can not get item size");
                if (!BinaryRead(ref pVaule, valueSize, baseOffset+ ItemSortIndexOffset, out int sortIndex)) return (false, null,"can not get sort index");

                if (!BinaryRead(ref pVaule, valueSize, baseOffset+ItemIDListOffset, out short idListSize)) return (false, null,"can not get IDList size");

                var pIdList = ReadIDList(ref pVaule, valueSize, baseOffset + ItemIDListOffset, idListSize);
                StringBuilder fullFileName = new StringBuilder(666);
                if (pIdList == IntPtr.Zero || !SHGetPathFromIDListW(pIdList, fullFileName)) return (false, null, "can not get IDList or SHGetPathFromIDListW failed");
                result.Add(new BookMarkModel() { FullName = fullFileName.ToString(), SortIndex = sortIndex });
                baseOffset += itemSize;
            }
            return (true,result,"fuck success");
        }
        static IntPtr ReadIDList(ref IntPtr source, int sourceSize, int offset, int idListSize)
        {
            int head = 0;
            while(true)
            {
                short cb = 0;
                if (head > idListSize || !BinaryRead(ref source, sourceSize, offset + head, out cb)) return IntPtr.Zero;
                if (cb == 0) break;
                head += cb;
            }
            return source + offset;
        }

        static bool BinaryRead<T>(ref IntPtr source,int sourceSize,int offset,out T value)
        {
            var type = typeof(T);
            if((type==typeof(ushort)|| type == typeof(short)) && offset+sizeof(ushort)<sourceSize)
            {
                value =(T)(object)Marshal.ReadInt16(source, offset);
                return true;
            }else if((type == typeof(uint) || type == typeof(int)) && offset + sizeof(uint) < sourceSize)
            {
                value = (T)(object)Marshal.ReadInt32(source, offset);
                return true;
            }else if ((type == typeof(ulong) || type == typeof(long)) && offset + sizeof(ulong) < sourceSize)
            {
                value = (T)(object)Marshal.ReadInt64(source, offset);
                return true;
            }
            else
            {
                value = (T)(object)0;
                return false;
            }
        }

        static byte[] GetOrderFromRegistry(string path)
        {
            using(var key=Registry.CurrentUser.OpenSubKey(path))
            {
                return (byte[])key?.GetValue(OrderValueName);
            }
        }
    }
}
