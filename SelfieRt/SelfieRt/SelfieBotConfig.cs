using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace SelfieRt
{

     public enum SelfieBotDBType
    {
        Sqlite=0
    }

    public class SelfieBotConfig
    {

        /// <summary>
        /// 微软的面部识别服务定义
        /// https://www.microsoft.com/cognitive-services/
        /// </summary>
        public struct CognitiveServicesType
        {
            /// <summary>
            /// 是否使用服务
            /// </summary>
            public Boolean IsValid { get; set; }

            /// <summary>
            /// 是否使用ComputerVision服务,用于成人内容检查
            /// </summary>
            public Boolean UseComputerVision { get; set; }

            /// <summary>
            /// ComputerVision订阅key
            /// </summary>
            public String ComputerVisionKey { get; set; }

            /// <summary>
            /// 是否使用Face服务,用于性别检查
            /// </summary>
            public Boolean UseFace { get; set; }

            /// <summary>
            /// Face订阅key
            /// </summary>
            public String FaceKey { get; set; }
        }

        /// <summary>
        /// 推特API定义
        /// </summary>
        public struct TwitterDefine
        {
            public string AccessToken;
            public string AccessTokenSecret;
            public string ConsumerKey;
            public string ConsumerSecret;
        }

        /// <summary>
        /// 数据库定义
        /// </summary>
        public struct DBDefine
        {
            public SelfieBotDBType Type;
            public string ConnectString;
        }

        public TwitterDefine Twitter { get; set; }
        public DBDefine DB { get; set; }

        /// <summary>
        /// 本地临时保存图片的位置
        /// </summary>
        public string PhotoTempPath { get; set; }
        
        public CognitiveServicesType MicrosoftCognitiveServices { get; set; }

        public static SelfieBotConfig Instance
        {
            get
            {
                if (_Instance == null) _Instance = JsonConvert.DeserializeObject<SelfieBotConfig>(File.ReadAllText("default.conf"), new BoolConverter());
                return _Instance;
            }
        }

        private static SelfieBotConfig _Instance = null;

        public void init()
        {
            File.WriteAllText("init.conf", JsonConvert.SerializeObject(this, Formatting.Indented, new BoolConverter()));
        }
    }

    public class BoolConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((bool)value) ? 1 : 0);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value.ToString() == "1";
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }
    }
}
