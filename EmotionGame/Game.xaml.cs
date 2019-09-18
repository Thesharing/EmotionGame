﻿using System;
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
using Windows.Graphics;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

// namesapce for EmotionServiceClient
using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;

// namesapce for Face
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;

using LLM;
using Windows.UI.Core;
using Windows.Devices.Enumeration;

namespace EmotionGame
{
    public class Info
    {
        public double Score { get; set; }
        public double Age1 { get; set; }
        public double Age2 { get; set; }
        public string Gender1 { get; set; }
        public string Gender2 { get; set; }
        public string ImgPath { get; set; }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Game : Page
    {
        // Emotion API订阅密钥
        private string SubscriptionKey = "af19714575d745d99a3fbf5f5ccf54bf";
        // 图片路径
        private string FilePath = "";
        private int countdown = 3;
        // 定时器部分
        private DispatcherTimer dispatcherTimer;
        private MediaCapture captureManager;

        // 出题部分
        private BitmapImage qimg;

        private double age1 = 0;
        private double age2 = 0;
        private string gender1 = "";
        private string gender2 = "";

        public Game() {
            this.InitializeComponent();
            InitCamera();
            DispatcherTimerSetup();
        }

        public void DispatcherTimerSetup()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1); //设置间隔为1s
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            Log(countdown.ToString());
            countdown--;
            if (countdown <= 3) {
                Animator.Use(AnimationType.FadeIn).SetDuration(TimeSpan.FromMilliseconds(800)).PlayOn(countdownText);
                countdownText.Visibility = Visibility.Visible;
                countdownText.Text = countdown.ToString();
                if (countdown == 0) {
                    dispatcherTimer.Stop();
                    countdown = 3;
                    CapturePhoto();
                }
            }
        }

        async private void InitCamera()
        {
            captureManager = new MediaCapture();
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            // Get the desired camera by panel
            DeviceInformation cameraDevice =
                allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null &&
                x.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);
            await captureManager.InitializeAsync(new MediaCaptureInitializationSettings {
                MediaCategory = MediaCategory.Communications,
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo,
                VideoDeviceId = cameraDevice.Id
            });
            string currentorientation = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().CurrentOrientation.ToString();
            switch (currentorientation) {
                case "Landscape":
                    captureManager.SetPreviewRotation(VideoRotation.None);
                    break;
                case "Portrait":
                    captureManager.SetPreviewRotation(VideoRotation.Clockwise270Degrees);
                    break;
                case "LandscapeFlipped":
                    captureManager.SetPreviewRotation(VideoRotation.Clockwise180Degrees);
                    break;
                case "PortraitFlipped":
                    captureManager.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
                    break;
                default:
                    captureManager.SetPreviewRotation(VideoRotation.None);
                    break;
            }
            //await captureManager.InitializeAsync();
            capturePreview.Source = captureManager;
            await captureManager.StartPreviewAsync();
        }

        private async void CapturePhoto() {
            progressRing.IsActive = true;
            progressRing.Visibility = Visibility.Visible;
            ImageEncodingProperties imgFormat = ImageEncodingProperties.CreateJpeg();
            InMemoryRandomAccessStream imageStream = new InMemoryRandomAccessStream();
            await captureManager.CapturePhotoToStreamAsync(imgFormat, imageStream);
            BitmapDecoder dec = await BitmapDecoder.CreateAsync(imageStream);
            BitmapEncoder enc = await BitmapEncoder.CreateForTranscodingAsync(imageStream, dec);

            string currentorientation = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().CurrentOrientation.ToString();
            switch (currentorientation) {
                case "Landscape":
                    enc.BitmapTransform.Rotation = BitmapRotation.None;
                    break;
                case "Portrait":
                    enc.BitmapTransform.Rotation = BitmapRotation.Clockwise270Degrees;
                    break;
                case "LandscapeFlipped":
                    enc.BitmapTransform.Rotation = BitmapRotation.Clockwise180Degrees;
                    break;
                case "PortraitFlipped":
                    enc.BitmapTransform.Rotation = BitmapRotation.Clockwise90Degrees;
                    break;
                default:
                    enc.BitmapTransform.Rotation = BitmapRotation.None;
                    break;
            }
            await enc.FlushAsync();

            // create storage file in local app storage
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                "TestPhoto.jpg",
                CreationCollisionOption.GenerateUniqueName);
            var filestream = await file.OpenAsync(FileAccessMode.ReadWrite);
            await RandomAccessStream.CopyAsync(imageStream, filestream);
            // take photo
            //await captureManager.CapturePhotoToStorageFileAsync(imgFormat, file);
            if (captureManager != null) {
                await captureManager.StopPreviewAsync();
                captureManager.Dispose();
            }
            imageStream.Dispose();
            filestream.Dispose();
            //await compressImage(file);

            // Get photo as a BitmapImage
            BitmapImage bmpImage = new BitmapImage(new Uri(file.Path));
            // imagePreivew is a <Image> object defined in XAML
            imagePreivew.Source = bmpImage;
            imagePreivew.Opacity = 1;

            Log("Capture finished!");
            FilePath = file.Path;
            Detect();
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
            string subscriptionKey = SubscriptionKey;

            Log("EmotionServiceClient is created");

            EmotionServiceClient emotionServiceClient = new EmotionServiceClient(subscriptionKey);

            Log("Calling EmotionServiceClient.RecognizeAsync()...");
            try
            {
                Emotion[] emotionResult;
                using (Stream imageFileStream = File.OpenRead(imageFilePath))
                {
                    emotionResult = await emotionServiceClient.RecognizeAsync(imageFileStream);
                    return emotionResult;
                }
            }
            catch (Exception exception)
            {
                Log(exception.ToString());
                return null;
            }
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

        public void LogFaceResult(Face[] faceResult)
        {
            int faceResultCount = 0;
            if (faceResult != null && faceResult.Length > 0)
            {
                foreach (Face face in faceResult)
                {
                    Log("Face[" + faceResultCount + "]");
                    Log("  Gender  : " + face.FaceAttributes.Gender);
                    Log("  Age     : " + face.FaceAttributes.Age.ToString());
                }
            }
            if (faceResult.Length == 2)
            {
                age1 = faceResult[0].FaceAttributes.Age;
                age2 = faceResult[1].FaceAttributes.Age;
                gender1 = faceResult[0].FaceAttributes.Gender;
                gender2 = faceResult[1].FaceAttributes.Gender;
                
            }
        }

        public double calculate(double[] x, double[] y)
        {
            double count = 0;
            for (int i = 0; i < 7; i++)
            {
                count += Math.Pow(x[i] - y[i], 2);
            }
            count = Math.Sqrt(count / 8) * 200;
            count = (2 - Math.Log10(count)) * 100;
            return count;
        }

        public double Scores(Emotion emotion1, Emotion emotion2)
        {
            double[] x = new double[8];
            double[] y = new double[8];
            x[0] = emotion1.Scores.Anger;
            x[1] = emotion1.Scores.Contempt;
            x[2] = emotion1.Scores.Disgust;
            x[3] = emotion1.Scores.Fear;
            x[4] = emotion1.Scores.Happiness;
            x[5] = emotion1.Scores.Neutral;
            x[6] = emotion1.Scores.Sadness;
            x[7] = emotion1.Scores.Surprise;
            y[0] = emotion2.Scores.Anger;
            y[1] = emotion2.Scores.Contempt;
            y[2] = emotion2.Scores.Disgust;
            y[3] = emotion2.Scores.Fear;
            y[4] = emotion2.Scores.Happiness;
            y[5] = emotion2.Scores.Neutral;
            y[6] = emotion2.Scores.Sadness;
            y[7] = emotion2.Scores.Surprise;

            return calculate(x, y);
        }

        private async void DetectFace()
        {
            using (var fileStream = File.OpenRead(FilePath))
            {
                try
                {
                    string subscriptionKey = "21d760920ac646fca6ee25b6b6f44f0b";

                    var faceServiceClient = new FaceServiceClient(subscriptionKey);
                    Face[] faces = await faceServiceClient.DetectAsync(fileStream, false, true, new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Glasses });
                    LogFaceResult(faces);
                }
                catch (FaceAPIException ex)
                {
                    Log(ex.ErrorMessage);
                }
            }
        }

        private async void Detect() {
            Log("Detecting...");

            Emotion[] emotionResult = await UploadAndDetectEmotions(FilePath);

            Log("Detection done!");
            LogEmotionResult(emotionResult);

            DetectFace();

            double score = 0;
            try {
                score = Scores(emotionResult[0], emotionResult[1]);
                Log(" Scores : " + score.ToString());
                Info info = new Info();
                info.Score = score;
                info.Age1 = age1;
                info.Age2 = age2;
                info.Gender1 = gender1;
                info.Gender2 = gender2;
                info.ImgPath = FilePath;
                Frame.Navigate(typeof(Result), info);
            }
            catch {
                Log("ERROR");
                Frame.Navigate(typeof(Result));
            }
            imagePreivew.Opacity = 0;
            //InitCamera();

            
        }

        public async Task compressImage(StorageFile imageFile) {
            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.ReadWrite)) {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);

                var memStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(memStream, decoder);

                encoder.BitmapTransform.ScaledWidth = 320;
                encoder.BitmapTransform.ScaledHeight = 240;

                await encoder.FlushAsync();

                memStream.Seek(0);
                fileStream.Seek(0);
                fileStream.Size = 0;
                await RandomAccessStream.CopyAsync(memStream, fileStream);

                memStream.Dispose();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);
            this.dispatcherTimer.Stop();
            if (captureManager != null) {
                captureManager.Dispose();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack) {
                // Show UI in title bar if opted-in and in-app backstack is not empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
            }
            else {
                // Remove the UI from the title bar if in-app back stack is empty.
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }

            Random rm = new Random();
            int ranNum = rm.Next(1, 20);
            qimg = new BitmapImage(new Uri("ms-appx:///Image/" + ranNum.ToString() + ".png"));
            q_image.Source = qimg;
        }
    }
}
