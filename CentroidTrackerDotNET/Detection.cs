using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp.Dnn;
using OpenCvSharp.Extensions;
using System.ComponentModel;
using OpenTK;
using NumSharp;

namespace CentroidTrackerDotNET
{
    class Detection
    {
        public void Detect()
        {
            Centroid ct = new Centroid();
            var objects = new Dictionary<int, (double x, double y)>();
            

            var prototxt = @".\model\deploy.prototxt";
            var model = @".\model\res10_300x300_ssd_iter_140000.caffemodel";
            var colors = Enumerable.Repeat(false, 21).Select(x => Scalar.RandomColor()).ToArray();

            double fps = 10;
            int sleepTime = (int)Math.Round(1000 / fps);
            
            VideoCapture capture = new VideoCapture(0);
            //capture.Open(0);

            //Console.WriteLine("Model is Loading");
            var net = CvDnn.ReadNetFromCaffe(prototxt, model);
            
            //using (Window window = new Window("capture"))
            using (Mat frame = new Mat())
            {
                while (true)
                {
                    capture.Read(frame);
                    if (frame.Empty())
                        break;
                    Cv2.Resize(frame, frame, new Size(400, 300));
                    double H = frame.Height;
                    double W = frame.Width;
                    
                    

                    var meanC = new Scalar(104.0, 177.0, 123.0);
                    var blob = CvDnn.BlobFromImage(frame, 1, new OpenCvSharp.Size(W, H), meanC, true, false);
                    net.SetInput(blob, "data");
                    var prob = net.Forward("detection_out");

                    List<List<double>> rects = new List<List<double>>();
                    List<double> box = new List<double>();

                    //var p = prob.Reshape(1, prob.Size(2));
                    for (int i = 0; i < prob.Size(2); i++)
                    {
                        var confidence = prob.At<float>(0, 0, i, 2);
                        if (confidence > 0.9)
                        {
                            //get value what we need
                            var idx = prob.At<float>(0, 0, i, 1);
                            var w1 = (W * prob.At<float>(0, 0, i, 3));
                            var h1 = (H * prob.At<float>(0, 0, i, 4));
                            var w2 = (W * prob.At<float>(0, 0, i, 5));
                            var h2 = (H * prob.At<float>(0, 0, i, 6));

                            rects.Add(new List<double> { w1,h1,w2,h2});
                            //System.Diagnostics.Debug.WriteLine(rectCount++.ToString());
                            Cv2.Rectangle(frame, new Rect((int)w1, (int)h1, (int)w2 - (int)w1, (int)h2 - (int)h1), Scalar.Green, 2);
                        }
                    }

                    objects = ct.Update(rects);
                    //System.Diagnostics.Debug.WriteLine($"Rect Count {rects.Count}");
                    foreach (var item in objects)
                    {


                        string text = "ID:" + item.Key.ToString();
                        var textSize = Cv2.GetTextSize(text, HersheyFonts.HersheyTriplex, 0.2, 1, out var baseline);
                        
                        Cv2.PutText(frame, text, new OpenCvSharp.Point(item.Value.x - 10, item.Value.y - 10), HersheyFonts.HersheyTriplex, 0.75, Scalar.Green, 2);

                        Cv2.Circle(frame, (int)item.Value.x, (int)item.Value.y, 2, Scalar.Green, 1);


                    }

                    using (new Window("capture", frame))
                        //window.ShowImage(frame);
                        Cv2.WaitKey(sleepTime);

                    //frame.Release();
                    //frame.Dispose();
                }
            }

        }

    }
}
