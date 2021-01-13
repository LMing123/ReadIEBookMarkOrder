using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestReadMenuOrder;
using static ReadIEBookMarkOrder.CustomJsonConverter;

namespace ReadIEBookMarkOrder
{
    public class EdgeFavorite
    {
        [JsonProperty(PropertyName = "checksum")]
        public string CheckSum { get; set; }
        [JsonProperty(PropertyName = "roots")]
        public BookMarkRoot Roots { get; set; } = new BookMarkRoot();
        [JsonProperty(PropertyName = "version")]
        public int Version { get; set; }
    }
    public class BookMarkRoot
    {
        [JsonProperty(PropertyName = "bookmark_bar")]
        public BookMarkDirectory BookmarkBar { get; set; } =
            new BookMarkDirectory() { Name = "收藏夹栏", Guid = new Guid("00000000-0000-4000-a000-000000000002"), Children = new List<BaseBookMarkInfo>(), ID = 1 };
        [JsonProperty(PropertyName = "other")]
        public BookMarkDirectory Other { get; set; } =
            new BookMarkDirectory() { Name = "其他收藏夹", Guid = new Guid("00000000-0000-4000-a000-000000000003"), Children = new List<BaseBookMarkInfo>(), ID = 2 };
        [JsonProperty(PropertyName = "synced")]
        public BookMarkDirectory Synced { get; set; } =
            new BookMarkDirectory() { Name = "移动收藏夹", Guid = new Guid("00000000-0000-4000-a000-000000000004"), Children = new List<BaseBookMarkInfo>(), ID = 3 };
    }

    public enum BookMarkType
    {
        Unknown,
        [JsonProperty(PropertyName = "url")]
        URL,
        [JsonProperty(PropertyName = "folder")]
        Folder
    }
    public class BaseBookMarkInfo
    {
        [JsonProperty(PropertyName = "date_added")]
        [JsonConverter(typeof(BookMarkDateTimeConverter))]
        public DateTime DateAdded { get; set; } = DateTime.Now.ToLocalTime();
        [JsonProperty(PropertyName = "guid")]
        public Guid Guid { get; set; } = Guid.NewGuid();
        [JsonProperty(PropertyName = "id")]
        public uint ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; } = "sync";
        [JsonProperty(PropertyName = "type")]
        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))] //Warning 这里用了小驼峰命名转换，当enum中枚举值内有两个单词时需要注意是否符合浏览器书签规范
        public BookMarkType Type { get; set; }

        [JsonIgnore]
        public int OrderIndex { get; set; }
    }

    public class BookMarkUrl : BaseBookMarkInfo
    {
        public BookMarkUrl()
        {
            this.Type = BookMarkType.URL;
        }
        [JsonProperty(PropertyName = "show_icon")]
        public bool ShowIcon { get; set; } = false;
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
    [JsonConverter(typeof(BookMarkDirectoryConverter))]
    public class BookMarkDirectory : BaseBookMarkInfo
    {
        public BookMarkDirectory()
        {
            this.Type = BookMarkType.Folder;
        }
        [JsonProperty(PropertyName = "date_modified")]
        [JsonConverter(typeof(BookMarkDateTimeConverter))]
        public DateTime DateModified { get; set; } = new DateTime(1601, 1, 1);
        [JsonProperty(PropertyName = "children")]

        public List<BaseBookMarkInfo> Children { get; set; } = new List<BaseBookMarkInfo>();

    }
    public class IEBookMarkModel
    {
        public string FullName { get; set; }
        public int SortIndex { get; set; }
        public BookMarkType BookMarkType { get; set; }
        public List<IEBookMarkModel> SubBookMark { get; set; }
    }
}
