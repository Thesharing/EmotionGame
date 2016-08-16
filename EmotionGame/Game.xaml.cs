using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

// namesapce for EmotionServiceClient
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace EmotionGame
{

    //public string Emotion1

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Game : Page
    {

        //Emotion API订阅密钥
        string SubscriptionKey = "af19714575d745d99a3fbf5f5ccf54bf";
        string FilePath = "";
        public Game()
        {
            this.InitializeComponent();
            InitCamera();
        }

        Windows.Media.Capture.MediaCapture captureManager;

        async private void InitCamera()
        {
            captureManager = new MediaCapture();
            await captureManager.InitializeAsync();
            capturePreview.Source = captureManager;
            await captureManager.StartPreviewAsync();
        }

        async private void StopCapturePreview()
        {
            await captureManager.StopPreviewAsync();
        }

        async private void CapturePhoto_Click(object sender, RoutedEventArgs e)
        {
            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();

            // create storage file in local app storage
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                "TestPhoto.jpg",
                CreationCollisionOption.GenerateUniqueName);

            // take photo
            await captureManager.CapturePhotoToStorageFileAsync(imgFormat, file);

            // Get photo as a BitmapImage
            BitmapImage bmpImage = new BitmapImage(new Uri(file.Path));

            // imagePreivew is a <Image> object defined in XAML
            imagePreivew.Source = bmpImage;
            imagePreivew.Opacity = 1;

            Log("Capture finished!");
            FilePath = file.Path;

            StopCapturePreview();
        }

        public void Log(string logMessage)
        {
            if (String.IsNullOrEmpty(logMessage) || logMessage == "\n")
            {
                logBox.Text += "\n";
            }
            else
            {
                string timeStr = DateTime.Now.ToString("HH:mm:ss.ffffff");
                string messaage = "[" + timeStr + "]: " + logMessage + "\n";
                logBox.Text += messaage;
            }
        }


        /// <summary>
        /// Uploads the image to Project Oxford and detect emotions.
        /// </summary>
        /// <param name="imageFilePath">The image file path.</param>
        /// <returns></returns>
        private async Task<Emotion[]> UploadAndDetectEmotions(string imageFilePath)
        {
            //MainWindow window = (MainWindow)Application.Current.MainWindow;
            //string subscriptionKey = window.ScenarioControl.SubscriptionKey;
            string subscriptionKey = SubscriptionKey;

            Log("EmotionServiceClient is created");

            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE STARTS HERE
            // -----------------------------------------------------------------------

            //
            // Create Project Oxford Emotion API Service client
            //
            EmotionServiceClient emotionServiceClient = new EmotionServiceClient(subscriptionKey);

            Log("Calling EmotionServiceClient.RecognizeAsync()...");
            try
            {
                Emotion[] emotionResult;
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    //
                    // Detect the emotions in the URL
                    //
                    emotionResult = await emotionServiceClient.RecognizeAsync(imageFileStream);
                    return emotionResult;
                }
            }
            catch (Exception exception)
            {
                Log(exception.ToString());
                return null;
            }
            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE ENDS HERE
            // -----------------------------------------------------------------------

        }

        public void LogEmotionResult(Emotion[] emotionResult)
        {
            int emotionResultCount = 0;
            if (emotionResult != null && emotionResult.Length > 0)
            {
                foreach (Emotion emotion in emotionResult)
                {
                    Log("Emotion[" + emotionResultCount + "]");
                    Log("  .FaceRectangle = left: " + emotion.FaceRectangle.Left
                             + ", top: " + emotion.FaceRectangle.Top
                             + ", width: " + emotion.FaceRectangle.Width
                             + ", height: " + emotion.FaceRectangle.Height);

                    Log("  Anger    : " + emotion.Scores.Anger.ToString());
                    Log("  Contempt : " + emotion.Scores.Contempt.ToString());
                    Log("  Disgust  : " + emotion.Scores.Disgust.ToString());
                    Log("  Fear     : " + emotion.Scores.Fear.ToString());
                    Log("  Happiness: " + emotion.Scores.Happiness.ToString());
                    Log("  Neutral  : " + emotion.Scores.Neutral.ToString());
                    Log("  Sadness  : " + emotion.Scores.Sadness.ToString());
                    Log("  Surprise  : " + emotion.Scores.Surprise.ToString());
                    Log("");
                    emotionResultCount++;
                }
            }
            else
            {
                Log("No emotion is detected. This might be due to:\n" +
                    "    image is too small to detect faces\n" +
                    "    no faces are in the images\n" +
                    "    faces poses make it difficult to detect emotions\n" +
                    "    or other factors");
            }
        }

        private async void Detect_Click(object sender, RoutedEventArgs e)
        {
            Log("Detecting...");

            Emotion[] emotionResult = await UploadAndDetectEmotions(FilePath);

            Log("Detection done!");
            LogEmotionResult(emotionResult);
            imagePreivew.Opacity = 0;
            InitCamera();
        }
    }
}
