using ZXing;
using ZXing.QrCode;
using System.Drawing;
using System.IO;
using ZXing.QrCode;
using System.Drawing.Imaging;

namespace WIPSystem.Web.Services
{
    public class BarcodeService
    {
        public byte[] GenerateQRCode(string content, int width, int height)
        {
            var writer = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 0 // QR code's quiet zone
                }
            };

            var pixelData = writer.Write(content);
            // Create a bitmap and save it to a memory stream as a PNG
            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                try
                {
                    // We assume that the pixel data is of format BGRA32
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                // Save the Bitmap as a PNG to a memory stream
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                // Return the byte array of the PNG image
                return ms.ToArray();
            }
        }
    }

}
