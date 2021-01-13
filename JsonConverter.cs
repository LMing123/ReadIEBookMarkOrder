using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadIEBookMarkOrder
{
    public class CustomJsonConverter
    {
        /// <summary>
        /// 用来转换BookMarkDirectory类
        /// </summary>
        public class BookMarkDirectoryConverter : JsonConverter
        {
            public override bool CanWrite => false;
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(List<BaseBookMarkInfo>));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var obj = serializer.Deserialize<JObject>(reader);
                BookMarkDirectory bookMarkDirectory = new BookMarkDirectory()
                {
                    DateAdded = obj.Value<string>("date_added").EdgeTimeToDateTime(),
                    Guid = new Guid(obj.Value<string>("guid")),
                    ID = obj.Value<uint>("id"),
                    Name = obj.Value<string>("name"),
                    Source = obj.Value<string>("source"),
                    Type = obj.Value<string>("type").ConvertToBookMarkType(),
                    DateModified = obj.Value<string>("date_modified").EdgeTimeToDateTime(),
                    Children = GetInfo(obj.Value<JArray>("children"))

                };

                return bookMarkDirectory;

            }
            /// <summary>
            /// 递归调用，用来转换Children内的Directory和Url
            /// </summary>
            /// <param name="array"></param>
            /// <returns></returns>
            List<BaseBookMarkInfo> GetInfo(JArray array)  
            {
                List<BaseBookMarkInfo> lists = new List<BaseBookMarkInfo>();

                foreach (var item in array)
                {

                    if (item.Value<string>("type") == "url")
                    {
                        lists.Add(item.ToObject<BookMarkUrl>());
                    }
                    else
                    {
                        BookMarkDirectory bookMarkDirectory = new BookMarkDirectory()
                        {
                            DateAdded = item.Value<string>("date_added").EdgeTimeToDateTime(),
                            Guid = new Guid(item.Value<string>("guid")),
                            ID = item.Value<uint>("id"),
                            Name = item.Value<string>("name"),
                            Source = item.Value<string>("source"),
                            Type = item.Value<string>("type").ConvertToBookMarkType(),
                            DateModified = item.Value<string>("date_modified").EdgeTimeToDateTime(),
                            Children = GetInfo(item.Value<JArray>("children"))  //这里进行递归

                        };
                        lists.Add(bookMarkDirectory);
                    }
                }
                return lists;
            }
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 用来转换书签内的date_added
        /// </summary>
        public class BookMarkDateTimeConverter : JsonConverter
        {

            public override bool CanConvert(Type objectType)
            {
                return (objectType.IsValueType && objectType == typeof(DateTime));
            }
            ///从JSON->DateTime
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var oriTime = new DateTime(1601, 1, 1);
                if (reader.Path == "date_added")
                {
                    var content = (string)reader.Value;
                    long value = long.Parse(content);
                    oriTime = oriTime.AddTicks(value * 10);
                }
                return oriTime;

            }
            //DateTime->JSON
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is DateTime dateTime)
                {
                    var oriTime = new DateTime(1601, 1, 1);
                    var convertTime = (dateTime.Ticks - oriTime.Ticks) / 10; //if thousand years later this app still use, then this code must be cared😉
                  
                    writer.WriteValue(convertTime.ToString());
                }
                else
                {
                    throw new TypeAccessException("Wrong Type");
                }
            }
        }
    }
}
