using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Threading;

namespace SelfieRT
{
    /// <summary>
    /// Free:
    /// 5,000 transactions per month, 20 per minute.
    /// </summary>
    class MicrosoftVision
    {

        private static volatile MicrosoftVision instance;
        private static object syncRoot = new Object();

        public static MicrosoftVision Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MicrosoftVision();
                    }
                }

                return instance;
            }
        }

        private MicrosoftVision()
        {
            VisionServiceClient = new VisionServiceClient(SelfieBotConfig.Instance.MicrosoftCognitiveServices.ComputerVisionKey);
        }

        private VisionServiceClient VisionServiceClient;

        public bool AnalyzeUrlAdult(string imageUrl)
        {
            try
            {
                Thread.Sleep(3 * 1000);
                VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult
                /*, VisualFeature.Categories , VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags */};
                AnalysisResult analysisResult = VisionServiceClient.AnalyzeImageAsync(imageUrl, visualFeatures).Result;

                return analysisResult.Adult.IsAdultContent;
            }
            catch (Exception e)
            {
                DebugLogger.Instance.W("AnalyzeUrlAdult >" + e.Message);
                return false;
            }
        }
    }
}
