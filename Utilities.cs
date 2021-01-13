using ReadIEBookMarkOrder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestReadMenuOrder
{

    public class Utilities
    {
        public static BookMarkType GetBookMarkType(string path)
        {
            if (File.Exists(path)) return BookMarkType.URL;
            else if (Directory.Exists(path)) return BookMarkType.Folder;
            else return BookMarkType.Unknown;
        }
    }

    /// <summary>
    /// 提取IE书签工具类
    /// </summary>
    public class IEBookMarkUtilities
    {
        static readonly IDCounter iDCounter = new IDCounter();
        /// <summary>
        /// 将IE书签转换为Edge书签格式
        /// </summary>
        /// <returns></returns>
        public static BookMarkRoot ConvertIEBookMark()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
            var sortList = IEBookMark.GetMainFavoriteBookMarksWithOrderList().Item2;
            BookMarkRoot bookMarkRoot = new BookMarkRoot();
            var direc = new DirectoryInfo(path);
            foreach (var item in direc.EnumerateDirectories())
            {
                if (!item.EnumerateDirectories().Any() && !item.EnumerateFiles().Any()) continue;  //文件夹为空，跳过处理
                if (item.Name.ToLower() == "links") continue;
                else bookMarkRoot.BookmarkBar.Children.Add(ExploreAll(item, sortList));
            }
            bookMarkRoot.BookmarkBar.Children = bookMarkRoot.BookmarkBar.Children.OrderBy(x => x.OrderIndex).ToList();
            var linksFolder = new DirectoryInfo($"{path}\\links");
            if (linksFolder.Exists)//处理links文件夹内收藏  将links路径下的整合到Edge收藏夹中
            {
                foreach (var item2 in linksFolder.EnumerateDirectories())
                {
                    bookMarkRoot.BookmarkBar.Children.Add(ExploreAll(item2, sortList));
                }
                bookMarkRoot.BookmarkBar.Children.InsertRange(0, ExtractDirectoryURL(linksFolder, sortList));

            }

            bookMarkRoot.Other.Children.AddRange(ExtractDirectoryURL(direc, sortList));//添加所有书签到其他
            return bookMarkRoot;
        }
        static BookMarkDirectory ExploreAll(DirectoryInfo directory, Dictionary<string, int> sortList)
        {
            BookMarkDirectory bd = new BookMarkDirectory();
            bd.Name = directory.Name;
            bd.Guid = Guid.NewGuid();
            bd.Children = new List<BaseBookMarkInfo>();
            bd.Children.AddRange(ExtractDirectoryURL(directory, sortList));
            bd.ID = iDCounter.SelfIncrease();
            foreach (var item in directory.GetDirectories())
            {
                if (!item.EnumerateDirectories().Any() && !item.EnumerateFiles().Any()) continue;  //文件夹为空，跳过处理
                bd.Children.Add(ExploreAll(item, sortList));
            }
            if (sortList.ContainsKey(directory.FullName.ToLower()))
                bd.OrderIndex = sortList[directory.FullName.ToLower()];

            bd.Children = bd.Children.OrderBy(x => x.OrderIndex).ToList();

            return bd;
        }

        static List<BookMarkUrl> ExtractDirectoryURL(DirectoryInfo directory, Dictionary<string, int> sortList)
        {
            List<BookMarkUrl> result = new List<BookMarkUrl>();
            foreach (var item in directory.EnumerateFiles("*.url"))
            {
                if (item.Extension != ".url") continue;
                var nameIndex = item.Name.LastIndexOf(".url");
                var url = new BookMarkUrl()
                {
                    Guid = Guid.NewGuid(),
                    Name = item.Name.Substring(0, nameIndex == -1 ? 0 : nameIndex),
                    Url = ExtractIEBookMarkToURL(item),
                    ID = iDCounter.SelfIncrease(),

                };

                if (sortList.ContainsKey(item.FullName.ToLower()))
                    url.OrderIndex = sortList[item.FullName.ToLower()];
                result.Add(url);
            }
            return result.OrderBy(x => x.OrderIndex).ToList();
        }
        /// <summary>
        /// 读取IE 书签中的URL 链接
        /// </summary>
        /// <param name="filePath">url文件的路径</param>
        /// <returns></returns>
        static string ExtractIEBookMarkToURL(FileInfo filePath) //必须是.url结尾的文件
        {
            string result = string.Empty;
            try
            {
                var content = File.ReadAllText(filePath.FullName).Split(Environment.NewLine.ToCharArray());
                var line = content.FirstOrDefault(x => x.StartsWith("URL"));
                var anchor = line.IndexOf('=');
                result = line.Substring(anchor + 1);

            }
            catch (Exception e)
            {

                Console.WriteLine($"ExtractIEBookMarkToURL Error: \n{e}");
            }
            return result;

        }
    }

    public class IDCounter
    {

        volatile uint count = 0;
        static readonly object locker = new object();
        public IDCounter() : this(1) { }

        public IDCounter(uint initValue)
        {
            count = initValue;
        }

        public uint SelfIncrease()
        {
            //lock (locker)
            {
                count++;
            }
            return count;
        }
    }
}
