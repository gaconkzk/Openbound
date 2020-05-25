/* 
 * Copyright (C) 2020, Carlos H.M.S. <carlos_judo@hotmail.com>
 * This file is part of OpenBound.
 * OpenBound is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or(at your option) any later version.
 * 
 * OpenBound is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with OpenBound. If not, see http://www.gnu.org/licenses/.
 */

using GunboundImageProcessing.ImageUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Linq;

namespace GunboundImageFix.Utils
{
    class SpritesheetMaker5Blender
    {
        public class ImportedImage
        {
            public string filePath;
            public Bitmap image;
            public (int, int) pivot;
            public int id;
            //public int fileIndex;
        }

        List<ImportedImage> importedImageList2 = new List<ImportedImage>();
        List<ImportedImage> importedImageList = new List<ImportedImage>();

        void ReadAllImages()
        {
            Console.WriteLine("Importing Sprites...");
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.InitialDirectory = Common.Directory + "Output";
            dialog.ShowDialog();

            if (dialog.FileNames.Length == 0)
            {
                Console.WriteLine("Aborted!");
                return;
            }

            string[] filePath;
            Bitmap[] imageList;

            filePath = new string[dialog.FileNames.Length];
            imageList = new Bitmap[dialog.FileNames.Length];

            for (int i = 0; i < dialog.FileNames.Length; i++)
            {
                filePath[i] = dialog.FileNames[i];
                imageList[i] = new Bitmap(filePath[i]);
                Console.WriteLine("I: " + filePath[i]);
            }

            dialog.Multiselect = false;
            dialog.ShowDialog();

            if (dialog.FileName.Length == 0)
            {
                Console.WriteLine("Aborted!");
                return;
            }

            Console.WriteLine("\n\nImporting pivot reference file .txt...");
            string[] pivotContent = File.ReadAllLines(dialog.FileName);

            for (int i = 0; i < pivotContent.Length; i++)
            {
                if (pivotContent[i].Length < 1) continue;
                string[] str = pivotContent[i].Split(',');

                importedImageList.Add(new ImportedImage()
                {
                    filePath = filePath[i],
                    image = imageList[i],
                    pivot = (int.Parse(str[1]), int.Parse(str[2])),
                    id = int.Parse(str[0]),
                });
            }
        }

        void CreateNewSpritesheet()
        {
            Console.WriteLine("Wiping unecessary assets... ");

            WipeBlackImages();

            (int, int) maxImageSize = (importedImageList.Max((x) => x.image.Width), importedImageList.Max((x) => x.image.Height));
            (int, int) maxImagePivot = (importedImageList.Max((x) => x.pivot.Item1), importedImageList.Max((x) => x.pivot.Item2));
            (int, int) minImagePivot = (importedImageList.Min((x) => x.pivot.Item1), importedImageList.Min((x) => x.pivot.Item2));

            (int, int) newSize = (
                maxImageSize.Item1 + Math.Max(Math.Abs(minImagePivot.Item1), Math.Abs(minImagePivot.Item1)),
                maxImageSize.Item2 + Math.Max(Math.Abs(maxImagePivot.Item2), Math.Abs(minImagePivot.Item2)));

            Console.WriteLine("Sprites per line: ");

            int imgPerLine = int.Parse(Console.ReadLine());


            Console.WriteLine("Squish X factor:");
            int squishXFactor = int.Parse(Console.ReadLine());

            Console.WriteLine("Squish Y factor:");
            int squishYFactor = int.Parse(Console.ReadLine());

            Console.WriteLine("Inicial X Shift factor:");
            int initialXFactor = int.Parse(Console.ReadLine());

            (int, int) newBigImageSize = ((newSize.Item1 - squishXFactor + initialXFactor) * imgPerLine, (newSize.Item2 - squishYFactor) * (int)Math.Ceiling((double)change / imgPerLine));

            Color[][] nCM1 = ImageProcessing.CreateBlankColorMatrix(newBigImageSize.Item1, newBigImageSize.Item2);
            Color[][] nCM2 = ImageProcessing.CreateBlankColorMatrix(newBigImageSize.Item1, newBigImageSize.Item2);

            int index = 0;
            int w = 0;
            int h = 0;

            bool reaply = false;

            foreach(ImportedImage img in importedImageList)
            {
                w = initialXFactor * (1 + (index % imgPerLine)) + (newSize.Item1 - squishXFactor) * (index % imgPerLine) + newSize.Item1 / 2 + img.pivot.Item1;
                h = (newSize.Item2 - squishYFactor) * (index / imgPerLine) + newSize.Item2 / 2 + img.pivot.Item2;

                if (!reaply)
                {
                    ImageProcessing.AddImageIntoMatrix(nCM1, img.image, w, h);
                    ImageProcessing.AddImageIntoMatrix(nCM2, img.image, w, h);
                }
                else
                {
                    ImageProcessing.BlendImageIntoMatrix(nCM1, img.image, w, h, (y, x) =>
                    {
                        return Color.FromArgb(Math.Max(x.A, y.A),
                            (byte)CalculatePixel(x.R, x.A, y.R, y.A),
                            (byte)CalculatePixel(x.G, x.A, y.G, y.A),
                            (byte)CalculatePixel(x.B, x.A, y.B, y.A));


                        //return Color.FromArgb(Math.Max(x.A, y.A), (byte)(x.R * xA + y.R * yA), (byte)(x.G *xA + y.G * yA), (byte)(x.B * xA + y.B * yA));
                        //return Color.FromArgb(Math.Max(x.A, y.A), Math.Max(x.R, y.R), Math.Max(x.G, y.G), Math.Max(x.B, y.B));
                        //return Color.FromArgb(Math.Max(x.A, y.A), Math.Max(x.R * x.A, y.R * y.R), Math.Max(x.G * x.A, y.G * y.A), Math.Max(x.B * x.A, y.B * y.A));
                        //return Color.FromArgb(Math.Max(x.A, y.A), (byte)((x.R + y.R)/2), (byte)((x.G + y.G)/2), (byte)((x.B + y.B)/2));
                    });

                    ImageProcessing.BlendImageIntoMatrix(nCM2, img.image, w, h, (x, y) =>
                    {
                        return Color.FromArgb(Math.Max(x.A, y.A),
                            (byte)CalculatePixel(x.R, x.A, y.R, y.A),
                            (byte)CalculatePixel(x.G, x.A, y.G, y.A),
                            (byte)CalculatePixel(x.B, x.A, y.B, y.A));


                        //return Color.FromArgb(Math.Max(x.A, y.A), (byte)(x.R * xA + y.R * yA), (byte)(x.G *xA + y.G * yA), (byte)(x.B * xA + y.B * yA));
                        //return Color.FromArgb(Math.Max(x.A, y.A), Math.Max(x.R, y.R), Math.Max(x.G, y.G), Math.Max(x.B, y.B));
                        //return Color.FromArgb(Math.Max(x.A, y.A), Math.Max(x.R * x.A, y.R * y.R), Math.Max(x.G * x.A, y.G * y.A), Math.Max(x.B * x.A, y.B * y.A));
                        //return Color.FromArgb(Math.Max(x.A, y.A), (byte)((x.R + y.R)/2), (byte)((x.G + y.G)/2), (byte)((x.B + y.B)/2));
                    });
                }

                if (++index == change && !reaply)
                {
                    reaply = true;
                    index = 0;
                }
            }

            ImageProcessing.BlendImageIntoMatrix(nCM1, ImageProcessing.CreateImage(nCM2), 0, 0, (x, y) =>
            {
                return Color.FromArgb(Math.Max(x.A, y.A), Math.Max(x.R, y.R), Math.Max(x.G, y.G), Math.Max(x.B, y.B));
            });

            ImageProcessing.CreateImage(nCM1).Save(@"C:\Users\Carlos\source\repos\OpenBound\GunboundImageFix\Output\Spritesheet\output.png");
        }

        public float CalculatePixel(float ca, float aa, float cb, float ab)
        {
            aa /= 255;
            ab /= 255;
            return (ca * aa + cb * ab * (1 - aa)) / (aa + ab * (1 - aa));
        }

        public void WipeBlackImages()
        {
            Bitmap dummy = new Bitmap(@"C:\Users\Carlos\source\repos\OpenBound\GunboundImageFix\Files\IgnoredDummy.png");

            foreach (ImportedImage img in importedImageList)
            {
                if (!ImageProcessing.IsEqual(dummy, img.image)) continue;

                img.image = ImageProcessing.CreateImage(ImageProcessing.CreateBlankColorMatrix(img.image.Width, img.image.Height));
                Console.WriteLine($"Wiped image: {img.filePath}");
            }
        }

        int change;

        public void CreateSpritesheet()
        {
            Thread t = new Thread(() => ReadAllImages());
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            while (t.IsAlive)
            {
                Thread.Sleep(1000);
            }

            importedImageList2 = importedImageList;
            importedImageList = new List<ImportedImage>();

            change = importedImageList2.Count();

            Console.WriteLine("Step 2");

            t = new Thread(() => ReadAllImages());
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            while (t.IsAlive)
            {
                Thread.Sleep(1000);
            }

            importedImageList2.AddRange(importedImageList);
            importedImageList = importedImageList2;

            if (importedImageList.Count == 0) return;
            CreateNewSpritesheet();
        }
    }
}
