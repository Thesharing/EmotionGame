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
        async public BitmapBuffer compressImage() {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("TestPhoto.jpg");
            using (IRandomAccessStream fileStream = await file.OpenReadAsync()) {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                using (var encoderStream = new InMemoryRandomAccessStream()) {
                    BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(encoderStream, decoder);
                    var newHeight = decoder.PixelHeight / 2;
                    var newWidth = decoder.PixelWidth / 2;
                    encoder.BitmapTransform.ScaledHeight = newHeight;
                    encoder.BitmapTransform.ScaledWidth = newWidth;

                    await encoder.FlushAsync();

                    byte[] pixels = new byte[newWidth * newHeight * 4];

                    await encoderStream.ReadAsync(pixels.AsBuffer(), (uint)pixels.Length, InputStreamOptions.None);

                    BitmapImage image = new BitmapImage();
                    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream()) {
                        await stream.WriteAsync(pixels.AsBuffer());
                        stream.Seek(0);
                        await image.SetSourceAsync(stream);
                    }
                    return image;
                }
            }
        }
    }
}
