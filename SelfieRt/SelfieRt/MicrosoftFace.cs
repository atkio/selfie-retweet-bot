using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ProjectOxford.Face;
using System.IO;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
namespace SelfieRt
{
    class MicrosoftFace
    {
        static SelfieBotConfig config = SelfieBotConfig.Instance;
        private static readonly IFaceServiceClient faceServiceClient = new FaceServiceClient(config.RecognizerKey);


       public  static bool MakeRequestLocalFile(String file)
        {
            var requiedFaceAttributes = new FaceAttributeType[] {
              //  FaceAttributeType.Age,
                FaceAttributeType.Gender,
                //FaceAttributeType.Smile,
                //FaceAttributeType.FacialHair,
                //FaceAttributeType.HeadPose
            };
            using (Stream s = File.OpenRead(file))
            {

                var faces = new List<Microsoft.ProjectOxford.Face.Contract.Face>(faceServiceClient.DetectAsync(s, true, false, requiedFaceAttributes).Result);
                return faces.Any(face => face.FaceAttributes.Gender == "female");
            }

        }

        public static bool MakeRequestUrl(string surl)
        {
            var requiedFaceAttributes = new FaceAttributeType[] {
              //  FaceAttributeType.Age,
                FaceAttributeType.Gender,
                //FaceAttributeType.Smile,
                //FaceAttributeType.FacialHair,
                //FaceAttributeType.HeadPose
            };

            var faces = new List<Microsoft.ProjectOxford.Face.Contract.Face>(faceServiceClient.DetectAsync(surl, true, false, requiedFaceAttributes).Result);
            return (faces.Any(face => face.FaceAttributes.Gender == "female"));
              


        }

        public static bool AnalyzeUrlAdult(string imageUrl)
        {
       
            VisionServiceClient VisionServiceClient = new VisionServiceClient(SelfieBotConfig.Instance.AdultCheckKey);

          
            VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult/*, VisualFeature.Categories , VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags */};
            AnalysisResult analysisResult = VisionServiceClient.AnalyzeImageAsync(imageUrl, visualFeatures).Result;

            return analysisResult.Adult.IsAdultContent || analysisResult.Adult.IsRacyContent;

        }
    }
}
