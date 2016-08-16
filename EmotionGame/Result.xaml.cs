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

enum Gender
{
    Male, Female
}

namespace EmotionGame
{
    public sealed partial class Result : Page
    {
        private double _score;
        private Gender _gender1;
        private Gender _gender2;
        private double _age1;
        private double _age2;
        private string _imagePath;
        public Result()
        {
            this.InitializeComponent();
        }

        public Result(double score, string gender1, string gender2, double age1, double age2, string imagePath) : this()
        {
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

        public string evaluate(double score, double age1, double age2, string gender1, string gender2)
        {

            if (gender1 != gender2)
            {
                if (score >= 95)
                {
                    return "你们默契度很高，简直就是天生的一对";
                }
                else if (score > 75)
                {
                    return "你们的默契度还不错";
                }
                else if (score > 50)
                {
                    return "可能你们有人心里没有对方";
                }
                else if (Math.Abs(age2 - age1) >= 15)
                {
                    return "看来你们的代沟有点大";
                }
                else return "从默契度来看，你们不合适";
            }
            else
            {
                if (score >= 95)
                {
                    return "你们默契度这么高，在一起吧 ";
                }
                else if (score > 75)
                {
                    return "你们的默契度还不错";
                }
                else if (score > 50)
                {
                    return "你们的默契度这么低，还是不要相爱了吧";
                }
                else if (Math.Abs(age2 - age1) >= 15)
                {
                    return "看来你们的代沟有点大";
                }
                else return "默契度这么低，你们再试试吧";
            }
        }


        private void buttonRetry_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
            }
        }

        private Gender ConvertStringToGender(String str)
        {
            if (str == "male")
            {
                return Gender.Male;
            }
            else
            {
                return Gender.Female;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter == null)
            {
                this.resultTextBlock.Text = "";
                this.commentTextBlock.Text = "一个人不可以乱来哦！";
            }
            else
            {
                Info info = (Info)e.Parameter;
                this.resultTextBlock.Text = info.Score.ToString();
                BitmapImage bmpImage = new BitmapImage(new Uri(info.ImgPath));
                this.image.Source = bmpImage;
                this.commentTextBlock.Text = evaluate(info.Score, info.Age1, info.Age2, info.Gender1, info.Gender2);
            }
        }
    }
}
