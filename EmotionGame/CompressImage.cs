using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace EmotionGame {
    class CompressImage {
        public static async Task compressImage() {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("TestPhoto.jpg");
            using (IRandomAccessStream fileStream = await file.OpenReadAsync()) {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);

                var memStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
                BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(memStream, decoder);

                encoder.BitmapTransform.ScaledWidth = 640;
                encoder.BitmapTransform.ScaledHeight = 480;

                await encoder.FlushAsync();

                memStream.Seek(0);
                fileStream.Seek(0);
                fileStream.Size = 0;
                await RandomAccessStream.CopyAsync(memStream, fileStream);

                memStream.Dispose();
            }
        }
    }
}
