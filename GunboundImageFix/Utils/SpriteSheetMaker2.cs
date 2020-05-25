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

namespace GunboundImageFix.Utils
{
    class SpritesheetMaker2
    {
        public class ImportedImage
        {
            public string filePath;
            public Bitmap image;
            public (int, int) pivot;
            public int id;
            //public int fileIndex;
        }

        List<ImportedImage> importedImageList = new List<ImportedImage>();
        List<List<ImportedImage>> subsets = new List<List<ImportedImage>>();
        int threadNumber = 10;
        int startedThread = 0;

        void PivotThread(int nmb)
        {
            Console.WriteLine("--Thread: " + nmb + " / Sub: " + subsets[nmb].Count);

            startedThread++;

            while (startedThread < threadNumber) Thread.Sleep(100);

            Thread.Sleep(2000);

            subsets[nmb].ForEach((img) =>
            {
                (int, int) pivot = img.pivot;
                int shiftX = pivot.Item1;
                int shiftY = pivot.Item2;

                if (shiftX != 0 || shiftY != 0)
                {
                    Color[][] c = ImageProcessing.CreateBlankColorMatrix(img.image.Width, img.image.Height);
                    ImageProcessing.AddImageIntoMatrix(c, img.image, -shiftX, -shiftY);
                    img.image = ImageProcessing.CreateImage(c);
                    Console.Write(img.id + ",");
                }
                else
                {
                    Console.Write(img.id + ",");
                }
            });
        }

        void TrimThread(int nmb, int newDesiredX, int newDesiredY, (int, int, int, int) validCoordinates)
        {
            Console.WriteLine("--Thread: " + nmb + " / Sub: " + subsets[nmb].Count);

            startedThread++;

            while (startedThread < threadNumber) Thread.Sleep(100);

            Thread.Sleep(2000);

            subsets[nmb].ForEach((img) =>
            {
                Color[][] c = ImageProcessing.CreateBlankColorMatrix(newDesiredX + 1, newDesiredY + 1);
                ImageProcessing.AddImageIntoMatrix(c, img.image, -validCoordinates.Item1, -validCoordinates.Item2);
                img.image = ImageProcessing.CreateImage(c);

                Console.Write(img.id + ",");
            });
        }

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

            for (int i = 0; i < threadNumber; i++) subsets.Add(new List<ImportedImage>());

            for (int i = 0; i < threadNumber; i++)
                for (int x = 0; x < importedImageList.Count; x++)
                    if (x % threadNumber == i) subsets[i].Add(importedImageList[x]);
        }

        void CreateNewSpritesheet()
        {
            List<Thread> tList = new List<Thread>();
            /*
            //First Step (Fixing Alpha Channel)
            Console.WriteLine("\n\nFix Alpha Channel? [y/n]: ");

            if (Console.ReadLine().Contains("y"))
            {
                Console.WriteLine("\n\nFixing Alpha Channel...");
                for (int i = 0; i < importedImageList.Count; i++)
                    importedImageList[i].image = ImageProcessing.FixAlphaChannel(importedImageList[i].image);
            }*/

            //Second Step (Big Shift)
            
            Console.WriteLine("\n\nShifting sprites into newly oversized created sprites...");

            int biggestWidth = 0;
            int biggestHeight = 0;

            Console.WriteLine("--Calculating new sprite sizes");

            importedImageList.ForEach((x) =>
            {
                biggestWidth = Math.Max(biggestWidth, x.image.Width);
                biggestHeight = Math.Max(biggestHeight, x.image.Height);
            });

            Console.WriteLine("--Creating Sprites");

            Console.WriteLine("--Insert the new shifting value (x):");
            int extraShiftX = int.Parse(Console.ReadLine());

            Console.WriteLine("--Insert the new shifting value (y):");
            int extraShiftY = int.Parse(Console.ReadLine());

            //if number is odd
            if ((extraShiftX & 0x1) > 0) { extraShiftX++; }
            if ((extraShiftY & 0x1) > 0) { extraShiftY++; }

            Console.WriteLine("--Shifted:");

            importedImageList.ForEach((img) =>
            {
                Color[][] c = ImageProcessing.CreateBlankColorMatrix(biggestWidth + extraShiftX, biggestHeight + extraShiftY);
                ImageProcessing.AddImageIntoMatrix(c, img.image,
                    extraShiftX / 2,
                    extraShiftY / 2);
                img.image = ImageProcessing.CreateImage(c);
                Console.Write(img.id + ",");
            });

            //Fifth Step - Create the spritesheet
            Console.WriteLine("\n\nCreating Spritesheet");

            int maxXSize = 0;
            int maxYSize = importedImageList[0].image.Height;
            double spritesPerLine = 20;

            //Calculating new image size
            importedImageList.ForEach((x) => {
                if (maxXSize < x.image.Width)
                    maxXSize = x.image.Width;

                if (maxYSize < x.image.Height)
                    maxYSize = x.image.Height;
            });

            Color[][] newImageMatrix = ImageProcessing.CreateBlankColorMatrix(maxXSize * 20, maxYSize * (int)Math.Ceiling(importedImageList.Count / spritesPerLine));

            Console.WriteLine("--Appending:");

            int accX = 0;
            int accY = 0;
            int index = 0;
            importedImageList.ForEach((x) =>
            {
                ImageProcessing.AddImageIntoMatrix(newImageMatrix, x.image, accX, accY, -x.pivot.Item1, -x.pivot.Item2);

                accX += maxXSize;
                Console.Write(x.id + ",");

                if (++index % 20 == 0)
                {
                    accY += maxYSize;
                    accX = 0;
                }
            });

            ImageProcessing.CreateImage(newImageMatrix)
                .Save(Common.Directory + @"Output\Spritesheet\output.png");
        }

        public void CreateSpritesheet()
        {
            Thread t = new Thread(() => ReadAllImages());
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            while (t.IsAlive)
            {
                Thread.Sleep(1000);
            }
            if (importedImageList.Count == 0) return;
            CreateNewSpritesheet();
        }
    }
}
