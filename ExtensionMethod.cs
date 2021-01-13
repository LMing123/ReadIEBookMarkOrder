using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadIEBookMarkOrder
{
    public static class ExtensionMethod
    {
        /// <summary>
        /// 将Edge数据中的时间转换为.Net DateTime
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static DateTime EdgeTimeToDateTime(this string content)
        {
            var oriTime = new DateTime(1601, 1, 1);
            
            long value = long.Parse(content);
            oriTime = oriTime.AddTicks(value * 10);
            return oriTime;
        }
        /// <summary>
        /// 将书签类型从string 转换到Enum
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static BookMarkType ConvertToBookMarkType(this string content)
        {
            switch(content)
            {
                case "url":return BookMarkType.URL;
                case "folder": return BookMarkType.Folder;
                default:return BookMarkType.Unknown;
            }
        }
    }
}
