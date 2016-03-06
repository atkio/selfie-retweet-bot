using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;

namespace SelfieBot
{
    class SelfieFacerecognizer
    {
        static Mutex mutex = new Mutex(false, "japaninfoz.com SelfieFacerecognizer");
        static SelfieBotConfig config = SelfieBotConfig.Instance;
        private static readonly IFaceServiceClient faceServiceClient = new FaceServiceClient(config.RecognizerKey);

        static void Main(string[] args)
        {

            if (!mutex.WaitOne(TimeSpan.FromSeconds(5), false))
            {
                Console.WriteLine("Another instance of the app is running. Bye!");
                return;
            }

            var db = new SelfieBotDB();
            try
            {
                db.getAllWaitRecognizer()
                    .ForEach(nr =>{
                        if ( Detect(nr.PhotoPath,nr.PhotoUrl))
                            db.addToRetweet(nr.TID);
                        //
                        db.removeWaitRecognizer(nr);
                        File.Delete(nr.PhotoPath);
                });

            }
            finally
            {
                mutex.ReleaseMutex();
            }

        }

        const string anifaceFileName = @".\lbpcascade_animeface.xml";
        const string faceFileName2 = @".\haarcascade_frontalface_default.xml";
        const string eyeFileName = @".\haarcascade_eye.xml";
        const string faceFileName = @".\visionary_FACES_01_LBP_5k_7k_50x50.xml";

        public static bool Detect(string file,string url)
        {


            using (CascadeClassifier aniface = new CascadeClassifier(anifaceFileName))
            using (CascadeClassifier face = new CascadeClassifier(faceFileName))
            using (CascadeClassifier face2 = new CascadeClassifier(faceFileName2))
            using (CascadeClassifier eye = new CascadeClassifier(eyeFileName))
            using (UMat ugray = new UMat())
            {

                Mat image = new Image<Bgr, byte>(file).Mat;

                CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                CvInvoke.EqualizeHist(ugray, ugray);

                //if (aniface.DetectMultiScale(
                //    ugray,
                //    1.1,
                //    10,
                //    new Size(20, 20)).Count() > 0)
                //    return false;

                Rectangle[] facesDetected = face.DetectMultiScale(
                    ugray,
                    1.1,
                    10,
                    new Size(20, 20));

                Rectangle[] facesDetected2 = face2.DetectMultiScale(
                    ugray,
                    1.1,
                    10,
                    new Size(20, 20));

                var faces = new List<Rectangle>(facesDetected);
                faces.AddRange(facesDetected2);

                if (faces.Count < 1)
                    return false;


                if (faces.Any(fa =>
                     eye.DetectMultiScale(
                                   new UMat(ugray, fa),
                                   1.1,
                                   10,
                                   new Size(20, 20)).Count() > 0
                ))
                return MakeRequestUrl(url);

                return false;

            }

        }

        static bool MakeRequestLocalFile(String file)
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

                var faces = new List<Face>(faceServiceClient.DetectAsync(s,true,false, requiedFaceAttributes).Result);
                return faces.Any(face => face.FaceAttributes.Gender == "female");
            }

        }

        static  bool MakeRequestUrl(string surl)
        {
            var requiedFaceAttributes = new FaceAttributeType[] {
              //  FaceAttributeType.Age,
                FaceAttributeType.Gender,
                //FaceAttributeType.Smile,
                //FaceAttributeType.FacialHair,
                //FaceAttributeType.HeadPose
            };

            var faces = new List<Face>( faceServiceClient.DetectAsync(surl, true, false, requiedFaceAttributes).Result );
            return faces.Any(face => face.FaceAttributes.Gender == "female");

        }
    }
}
