﻿using System;
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
using Windows.Storage;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace EmotionGame
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ImageType : Page
    {
        string filePath = "";
        BitmapImage img;
        int index = 0;
        public ImageType()
        {
            this.InitializeComponent();
        }

        private void back_button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void next_button_Click(object sender, RoutedEventArgs e)
        {
            Random rm = new Random(index);
            int ranNum = rm.Next(11);
            img = new BitmapImage(new Uri("ms-appx:///Image/" + ranNum.ToString() + ".png"));
            template_image.Source = img;
            index++;
        }
    }
}