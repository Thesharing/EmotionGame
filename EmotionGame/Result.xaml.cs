using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

enum Gender {
    Male, Female
}

namespace EmotionGame {
    public sealed partial class Result : Page {
        private double _score;
        private Gender _gender1;
        private Gender _gender2;
        private double _age1;
        private double _age2;
        private string _imagePath;
        public Result() {
            this.InitializeComponent();
        }

        public Result(double score, string gender1, string gender2, double age1, double age2, string imagePath) :this() {
            _score = score;
            _age1 = age1;
            _age2 = age2;
            _gender1 = ConvertStringToGender(gender1);
            _gender2 = ConvertStringToGender(gender2);
            _imagePath = imagePath;
            this.resultTextBlock.Text = _score.ToString();
            BitmapImage bmpImage = new BitmapImage(new Uri(imagePath));
            this.image.Source = bmpImage;
        }

        private void buttonRetry_Click(object sender, RoutedEventArgs e) {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack) {
                rootFrame.GoBack();
            }
        }

        private Gender ConvertStringToGender(String str) {
            if (str == "male") {
                return Gender.Male;
            }
            else {
                return Gender.Female;
            }
        }
    }
}
