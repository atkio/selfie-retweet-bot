using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RankTrainer
{
    class Program
    {
        static void Main(string[] args)
        {
            prdict();
        }

        const string faceFileName2 = @".\haarcascade_frontalface_default.xml";
        const string cos = @"\\JAPANINFOZ\Share\cos\_maochun";
        static void prdict()
        {
            FisherFaceRecognizer facerco = new FisherFaceRecognizer();
            facerco.Load(@"trained.data");
            foreach (var file in Directory.GetFiles(cos, "*.jpg"))
            {
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
                        continue;
                    CvInvoke.Resize(ugray,image, new Size(300,300));
                   var res= facerco.Predict(image);
                    Console.WriteLine("label:"+res.Label);
                }

            }
            
        }

        static void train()
        {
            FisherFaceRecognizer facerco = new FisherFaceRecognizer();
            var rates = File.ReadAllLines(@"d:\rate.csv")
                .ToDictionary(str => "SCUT-FBP-" + str.Split(',')[0], str => int.Parse(str.Split(',')[1]));

            int[] labels = new int[300];
            Image<Gray, Byte>[] images = new Image<Gray, byte>[300];

            int i = 0;
            foreach (var kv in rates.Take(300))
            {
                using (UMat ugray = new UMat())
                {
                    try
                    {
                        Mat image1 = new Mat(@"D:\Data_Collection\" + kv.Key + ".jpg", LoadImageType.Color);
                        Mat image = new Mat(300, 300, image1.Depth, image1.NumberOfChannels);


                        CvInvoke.Resize(image1, image, image.Size, 0.5, 0.5, Inter.Cubic);

                        CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                        //normalizes brightness and increases contrast of the image
                        CvInvoke.EqualizeHist(ugray, ugray);


                        images[i] = ugray.ToImage<Gray, Byte>();
                        labels[i] = kv.Value;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                i++;
            }

            facerco.Train<Gray, Byte>(images, labels);
            facerco.Save(@"trained.data");
        }
    }
}
