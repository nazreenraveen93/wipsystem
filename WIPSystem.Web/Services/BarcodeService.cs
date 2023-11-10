using ZXing;
using ZXing.Common;
using System.Drawing;
using System.IO;
using ZXing.QrCode;

namespace WIPSystem.Web.Services
{
    public class BarcodeService
    {
        public byte[] GenerateBarcode(string content, int width, int height)
        {
            var writer = new BarcodeWriter<Bitmap>();
            writer.Format = BarcodeFormat.CODE_128;
            writer.Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                PureBarcode = true
            };

            // Generate the barcode as a Bitmap
            using (Bitmap bitmap = writer.Write(content))
            {
                using (var stream = new MemoryStream())
                {
                    // Save the Bitmap as a PNG to a memory stream
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    // Return the byte array of the PNG image
                    return stream.ToArray();
                }
            }
        }
    }
}
