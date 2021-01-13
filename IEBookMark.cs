using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TestReadMenuOrder;

namespace ReadIEBookMarkOrder
{

    
    public class IEBookMark
    {
        public static readonly string RegistryPah = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\MenuOrder";
        public static readonly string FavoritesKey = "Favorites";
        public static readonly string OrderValueName = "Order";

        public static readonly string IEFavoritesPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Favorites";

        static readonly int ItemCountOffset = 16; // offset of count value that ITEMIDLIST array number
        static readonly int ItemListBeginOffset = 20; // offset of begin that ITEMIDLIST array 

        static readonly int ItemSizeOffset = 0;
        static readonly int ItemSortIndexOffset = 4;
        static readonly int ItemIDListOffset = 8;

        static Dictionary<string, int> OrderList = new Dictionary<string, int>();

        [DllImport("shell32.dll", SetLastError = true, ExactSpelling = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SHGetPathFromIDListW(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszPath);

        //public static BookMarkModel GetMainFavoriteBookMarks()
        //{
        //    return GetBookMarks($"{RegistryPah}\\{FavoritesKey}",$"{IEFavoritesPath}", "Main", -1);            
        //}
        public static (IEBookMarkModel, Dictionary<string, int>) GetMainFavoriteBookMarksWithOrderList()
        {
            OrderList = new Dictionary<string, int>();
            return (GetBookMarks($"{RegistryPah}\\{FavoritesKey}", $"{IEFavoritesPath}", "Main", -1), OrderList);
        }

        static IEBookMarkModel GetBookMarks(string registryPath,string path,string name,int sortIndex)
        {
            var result = GetBookMarkSort(registryPath, path);
            IEBookMarkModel bm = new IEBookMarkModel()
            {
                FullName = name,
                SortIndex = sortIndex,
                BookMarkType = BookMarkType.Folder,

            };
            bm.SubBookMark = new List<IEBookMarkModel>();
            if (result.Item1)
            {
                foreach (var item in result.Item2)
                {
                    if (item.BookMarkType == BookMarkType.Folder)
                    {
                        var temName = GetName(item.FullName);
                        var temResult = GetBookMarks($"{registryPath}\\{temName}", $"{path}\\{temName}", item.FullName, item.SortIndex);
                        if (temResult != null) bm.SubBookMark.Add(temResult);
                    }
                    else
                    {
                        bm.SubBookMark.Add(item);
                    }
                }
            }
            else bm.SubBookMark = null;
            return bm;

        }
        public static (bool, List<IEBookMarkModel>,string message) GetBookMarkSort(string registryPath,string path)
        {
            byte[] value = GetOrderFromRegistry(registryPath);
            if (value == null || value.Length == 0) return (false, null,"can not access registry value");
            int valueSize = value.Length;
            var pVaule = Marshal.AllocHGlobal(valueSize);
            Marshal.Copy(value, 0, pVaule, value.Length);

            if (!BinaryRead(ref pVaule, valueSize, ItemCountOffset, out int itemCount)) return (false, null,"can not get item count");

            List<IEBookMarkModel> result = new List<IEBookMarkModel>();
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

                var bookMarkPath = ($"{path}\\{GetName(fullFileName.ToString())}").ToLower();
                OrderList.Add(bookMarkPath, sortIndex);
                result.Add(new IEBookMarkModel() { FullName = bookMarkPath, SortIndex = sortIndex,BookMarkType=Utilities.GetBookMarkType(bookMarkPath)});
                baseOffset += itemSize;
            }
            Marshal.FreeHGlobal(pVaule);
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

        static string GetName(string path)
        {
            return  Path.GetFileName(path);
        }
    }
}
