
using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.IO;
using Emgu.CV.Face;


namespace SelfieRt
{
    class SelfieFacerecognizer
    {

        static SelfieBotConfig config = SelfieBotConfig.Instance;

        public static void Run()
        {

            var db = new SelfieBotDB();

            while (db.getAllWaitRecognizer().Count > 0)
            {
                var nr = db.getAllWaitRecognizer().First();

                if (IsFileLocked(nr.PhotoPath))
                    return;

                if (!File.Exists(nr.PhotoPath))
                    db.removeWaitRecognizer(nr);

                try
                {
                    if (Detect(nr.PhotoPath, nr.PhotoUrl))
                    {
                        db.addToRetweet(nr.TID);
                        var noneed = db.getAllWaitRecognizerWithTID(nr.TID);

                        foreach(var todel in noneed)
                        {
                            db.removeWaitRecognizer(todel);
                            File.Delete(todel.PhotoPath);
                        }
                    }
                }
                finally
                {
                    ////
                    //db.removeWaitRecognizer(nr);
                    //File.Delete(nr.PhotoPath);
                }
            }

        }

        protected static bool IsFileLocked(string file)
        {
            FileStream stream = null;

            try
            {
                stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        const string anifaceFileName = "lbpcascade_animeface.xml";
        const string faceFileName2 = "haarcascade_frontalface_default.xml";
        const string eyeFileName = "haarcascade_eye.xml";
        const string faceFileName = "visionary_FACES_01_LBP_5k_7k_50x50.xml";


        public static bool Detect(string file, string url)
        {

            try
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

                    if (aniface.DetectMultiScale(
                        ugray,
                        1.1,
                        10,
                        new Size(20, 20)).Count() > 0)
                        return false;

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

                    return (faces.Count > 0);

                    //if (faces.Count < 1)
                    //    return false;


                    //if (config.RecognizerService == "true")
                    //    return MicrosoftFace.MakeRequestUrl(url);
                    //else
                    //    return true;


                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }



        }


        static int Predict(string file)
        {
            FisherFaceRecognizer facerco = new FisherFaceRecognizer();
            facerco.Load(@"trained.data");
            using (CascadeClassifier face2 = new CascadeClassifier(faceFileName2))
            using (UMat ugray = new UMat())
            {



                Mat image = new Image<Bgr, byte>(file).Mat;

                CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                CvInvoke.EqualizeHist(ugray, ugray);
                var faces = face2.DetectMultiScale(
              ugray,
              1.1,
              10,
              new Size(300, 300));
                if (faces.Count() < 1)
                    return 0;
                CvInvoke.Resize(ugray, image, new Size(300, 300));
                var res = facerco.Predict(image);
                return res.Label;
            }

        }

    }


}
