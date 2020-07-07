using GunboundImageFix.Common;
using GunboundImageFix.Entity;
using GunboundImageFix.Extension;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GunboundImageFix.Utils
{
    public class IMGCracker
    {
        /// <summary>
        /// Exports the IMG data saving it on RawOutputDirectory folder.
        /// </summary>
        public static void ExportIMGData()
        {
            List<string> paths = FileImportManager.ReadMultipleIMGFiles();

            for(int i = 0; i < paths.Count; i++)
            {
                List<ImportedImage> importedImageList = ExtractIMGData(paths[i]);
                string filePath = paths[i];
                string fileName = filePath.Split('\\').Last().Split('.').First();

                string pivotList = "[\n";
                string pivotFileStr = $"{Parameters.PivotFileHeader}\n";
                Directory.CreateDirectory($@"{Parameters.RawOutputDirectory}\{fileName}");
                for(int j = 0; j < importedImageList.Count; j++)
                {
                    ImportedImage ii = importedImageList[j];
                    
                    pivotFileStr += $"{j},{ii.Pivot.Item1},{ii.Pivot.Item2},{ii.RiderOffset.Item1},{ii.RealRiderOffset.Item2},{ii.RealRiderOffset.Item1},{ii.RealRiderOffset.Item2}\n";
                    pivotList += $"[ {ii.RealRiderOffset.Item1}, {ii.RealRiderOffset.Item2} ],\n";
                    ii.BitmapImage.Save($@"{Parameters.RawOutputDirectory}\{fileName}\{fileName}_{j}.png");
                }

                pivotList += "]";

                Console.WriteLine($"Imported: {paths[i]}");
                File.WriteAllText($@"{Parameters.RawOutputDirectory}\{fileName}\{fileName}.txt", pivotFileStr);
                File.WriteAllText($@"{Parameters.RawOutputDirectory}\{fileName}\RiderPivot.txt", pivotList);
            }
        }

        /// <summary>
        /// Extracts the IMG file data and returns a list of ImportedImage.
        /// </summary>
        public static List<ImportedImage> ExtractIMGData(string fileName)
        {
            byte[] imgByteArray = File.ReadAllBytes(fileName);

            // I have decided to parse the whole byte array into queue to make things easier to understand
            Queue<byte> valueQueue = new Queue<byte>();

            for (int i = 0; i < imgByteArray.Length; i++)
                valueQueue.Enqueue(imgByteArray[i]);

            // File data:
            //
            // bits  |   i      | value
            //  2x16 |   0      | zero - ? 
            //  2x16 |   1      | number of images
            //  2x16 |   2  (0) | ? - v0
            // ------+----------+ Loop starts here
            //    32 |   3  (1) | width
            //    32 |   4  (2) | height
            //    32 |   5  (3) | pivot X
            //    32 |   6  (4) | pivot Y
            //   4x8 |   7  (5) | ? - v5
            //   4x8 |   8  (6) | ? - v6
            //    32 |   9  (7) | pilot pivot X
            //    32 |  10  (8) | pilot pivot Y
            //    32 |  11  (9) | Data stream length
            // (9)x8 |  12 (10) | Image Begining...
            // (9)x8 |  12+(09) | Image Ending
            // ------+----------+ Loop ends here
            //
            // After iterating dataStreamLength times, it will start another header, begining from the loop indicator.
            // until the eof is reached
            //
            // Each pixel of the image is built like this:
            //
            // byte1 = 0x 0011 0100; // (queue.Dequeue();)
            // byte2 = 0x 0110 1100; // (queue.Dequeue();)
            //
            // Where:
            //
            // alpha = 0011 XXXX (1st 4 bits of byte1 + 4 don't care); 
            // red   = 0100 XXXX (2nd 4 bits of byte1 + 4 don't care);
            // green = 0110 XXXX (1st 4 bits of byte2 + 4 don't care);
            // blue  = 1100 XXXX (2st 4 bits of byte2 + 4 don't care);
            //
            // I have decided to replace the 4 don't care for proportional values scaled, where
            // 240 (0xF0 - 0b1111000) is 255 (0xFF - 0b11111111). Using the following formula:
            //
            // trueColor = 255 * current / 240;
            //
            // Where v0 is the first information on the header, numberOfImages is v1, and v2 is the third
            // Every v# variable from now on are unknown values.

            int v0 = valueQueue.DequeueInt32();
            int numberOfImages = valueQueue.DequeueInt32();

            List<ImportedImage> importedImage = new List<ImportedImage>();

            for (int i = 0; i < numberOfImages; i++)
            {
                int v2 = valueQueue.DequeueInt32();
                int width = valueQueue.DequeueInt32();
                int height = valueQueue.DequeueInt32();
                int pivotX = valueQueue.DequeueInt32();
                int pivotY = valueQueue.DequeueInt32();
                int v5 = valueQueue.DequeueInt32();
                int v6 = valueQueue.DequeueInt32();
                int pilotPivotX = valueQueue.DequeueInt32();
                int pilotPivotY = valueQueue.DequeueInt32();
                int dataStreamLength = valueQueue.DequeueInt32();

                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);

                for (int h = 0; h < dataStreamLength; h += 2)
                {
                    byte b1 = valueQueue.Dequeue();
                    byte b2 = valueQueue.Dequeue();

                    int a = (b2 & 0xF0) << 0;
                    int r = (b2 & 0x0F) << 4;
                    int g = (b1 & 0xF0) << 0;
                    int b = (b1 & 0x0F) << 4;

                    a = (int)Math.Ceiling(255f / 240f * a);
                    r = (int)Math.Ceiling(255f / 240f * r);
                    g = (int)Math.Ceiling(255f / 240f * g);
                    b = (int)Math.Ceiling(255f / 240f * b);

                    bitmap.SetPixel((h / 2) % width, (h / 2) / width, Color.FromArgb(a, r, g, b));
                }

                importedImage.Add(new ImportedImage(bitmap, (pivotX, pivotY), (pilotPivotX, pilotPivotY)));
            }

            return importedImage;
        }
    }
}
