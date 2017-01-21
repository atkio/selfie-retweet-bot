using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

namespace SelfieFacerecognizer
{
    class VisionClent
    {
        
        public static bool AnalyzeUrlAdult(string imageUrl,string SubscriptionKey)
        {
            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE STARTS HERE
            // -----------------------------------------------------------------------

            //
            // Create Project Oxford Vision API Service client
            //
            VisionServiceClient VisionServiceClient = new VisionServiceClient(SubscriptionKey);

            //
            // Analyze the url for all visual features
            //

            VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult/*, VisualFeature.Categories , VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags */};
            AnalysisResult analysisResult =VisionServiceClient.AnalyzeImageAsync(imageUrl, visualFeatures).Result;

            return analysisResult.Adult.IsAdultContent || analysisResult.Adult.IsRacyContent;

            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE ENDS HERE
            // -----------------------------------------------------------------------
        }

        internal static bool AnalyzeUrlAdult(string surl, object adultCheckKey)
        {
            throw new NotImplementedException();
        }
    }
}
