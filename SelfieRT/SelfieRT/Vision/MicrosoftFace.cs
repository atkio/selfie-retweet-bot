using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SelfieRT
{
    /// <summary>
    /// FREE:
    /// 30,000 transactions per month, 20 per minute.
    /// </summary>
    class MicrosoftFace
    {
        private static volatile MicrosoftFace instance;
        private static object syncRoot = new Object();

        public static MicrosoftFace Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MicrosoftFace();
                    }
                }

                return instance;
            }
        }

        private MicrosoftFace()
        {
            faceServiceClient = new FaceServiceClient(SelfieBotConfig.Instance.MicrosoftCognitiveServices.FaceKey);
        }
        private IFaceServiceClient faceServiceClient;

        public bool MakeRequestLocalFile(String file)
        {
            var requiedFaceAttributes = new FaceAttributeType[] {
                FaceAttributeType.Age,
                FaceAttributeType.Gender,
                //FaceAttributeType.Smile,
                //FaceAttributeType.FacialHair,
                //FaceAttributeType.HeadPose
            };
            using (Stream s = File.OpenRead(file))
            {

                var faces = new List<Face>(faceServiceClient.DetectAsync(s, true, false, requiedFaceAttributes).Result);
                return faces.Any(face => face.FaceAttributes.Gender == "female" &&
                                         face.FaceAttributes.Age < 30);
            }

        }

        public bool MakeRequestUrl(string surl)
        {
            var requiedFaceAttributes = new FaceAttributeType[] {
                FaceAttributeType.Age,
                FaceAttributeType.Gender,
                //FaceAttributeType.Smile,
                //FaceAttributeType.FacialHair,
                //FaceAttributeType.HeadPose
            };

            var faces = new List<Microsoft.ProjectOxford.Face.Contract.Face>(faceServiceClient.DetectAsync(surl, true, false, requiedFaceAttributes).Result);
            return faces.Any(face => face.FaceAttributes.Gender == "female" &&
                                         face.FaceAttributes.Age < 30);

        }

    
    }
}
