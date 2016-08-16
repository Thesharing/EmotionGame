using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace EmotionGame {
    class CompressImage {
        async public void compressImage() {
            using (IRandomAccessStream fileStream = await result.OpenAsync(FileAccessMode.Read)) {
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
                }
            }
        }
    }
}
