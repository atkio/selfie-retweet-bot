using Newtonsoft.Json;
using System;
using System.IO;

namespace SelfieRT
{


    public class SelfieBotConfig
    {

        /// <summary>
        /// 微软的面部识别服务定义
        /// https://www.microsoft.com/cognitive-services/
        /// </summary>
        public struct CognitiveServicesType
        {

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

        public TwitterDefine Twitter { get; set; }


        /// <summary>
        /// 本地临时保存图片的位置
        /// </summary>
        public string PhotoTempPath =Path.Combine(Environment.CurrentDirectory, "TEMP");
                   

        public CognitiveServicesType MicrosoftCognitiveServices { get; set; }

        public const string Define = "Define.conf";

        public static SelfieBotConfig Instance
        {
            get
            {
                if (!File.Exists(Define))
                {
                    File.WriteAllText(Define,
                     JsonConvert.SerializeObject(new SelfieBotConfig(),
                     Formatting.Indented, new BoolConverter()));
                }

                string PhotoTempPath = Path.Combine(Environment.CurrentDirectory, "TEMP");
                if (!Directory.Exists(PhotoTempPath))
                {
                    Directory.CreateDirectory(PhotoTempPath);
                }

                if (_Instance == null)
                {
                    try
                    {
                        _Instance = JsonConvert.DeserializeObject<SelfieBotConfig>(
                            File.ReadAllText(Define), new BoolConverter());
                    }
                    catch
                    {
                        throw new Exception("Define file failed.");
                    }
                }
                return _Instance;
            }
        }

        private static SelfieBotConfig _Instance = null;

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
