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
    class SpritesheetMaker
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

        void PivotThread(int nmb, (int, int) reference)
        {
            Console.WriteLine("--Thread: " + nmb + " / Sub: " + subsets[nmb].Count);

            startedThread++;

            while (startedThread < threadNumber) Thread.Sleep(100);

            Thread.Sleep(2000);

            subsets[nmb].ForEach((img) =>
            {
                (int, int) pivot = img.pivot;
                int shiftX = pivot.Item1 - reference.Item1;
                int shiftY = pivot.Item2 - reference.Item2;

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

                //int fileIndex = int.Parse((@filePath[i]).Split('/').Last().Split('-').Last().Replace(".png", ""));

                importedImageList.Add(new ImportedImage()
                {
                    filePath = filePath[i],
                    image = imageList[i],
                    pivot = (int.Parse(str[1]), int.Parse(str[2])),
                    id = int.Parse(str[0]),
                    //  fileIndex = fileIndex
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

            //First Step (Fixing Alpha Channel)
            Console.WriteLine("\n\nFix Alpha Channel? [y/n]: ");

            if (Console.ReadLine().Contains("y"))
            {
                Console.WriteLine("\n\nFixing Alpha Channel...");
                for (int i = 0; i < importedImageList.Count; i++)
                    importedImageList[i].image = ImageProcessing.FixAlphaChannel(importedImageList[i].image);
            }
            
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
                ImageProcessing.AddImageIntoMatrix(c, img.image, extraShiftX / 2, extraShiftY / 2);
                img.image = ImageProcessing.CreateImage(c);
                Console.Write(img.id + ",");
            });

            //Third Step (Fix Sprite Pivot) - Multithread

            Console.WriteLine("\n\nFixing Sprite Pivots");

            //Ref
            (int, int) reference = importedImageList[0].pivot;
            
            Console.WriteLine("--Fixed:");

            for (int i = 0; i < threadNumber; i++)
            {
                Thread t = new Thread(() => PivotThread(i, reference));
                tList.Add(t);
                t.Start();
                Thread.Sleep(500);
            }

            while (true)
            {
                Thread.Sleep(1000);
                bool isAlive = false;
                tList.ForEach((t) => isAlive = t.IsAlive || isAlive);
                if (!isAlive) break;
            }

            startedThread = 0;

            //Forth step (Remove Sprite Extra Spaces (if possible))
            Console.WriteLine("\n\nRemoving Extra Spaces");

            //Selecting the coordinates for the cut
            (int, int, int, int) validCoordinates = (int.MaxValue, int.MaxValue, 0, 0);
            importedImageList.ForEach((img) =>
            {
                //(x, y, maxX, maxY)
                (int, int, int, int) tmp = ImageProcessing.GetImageStartingCoordinates(img.image);
                validCoordinates.Item1 = Math.Min(tmp.Item1, validCoordinates.Item1);
                validCoordinates.Item2 = Math.Min(tmp.Item2, validCoordinates.Item2);
                validCoordinates.Item3 = Math.Max(tmp.Item3, validCoordinates.Item3);
                validCoordinates.Item4 = Math.Max(tmp.Item4, validCoordinates.Item4);
            });

            int newDesiredX = validCoordinates.Item3 - validCoordinates.Item1;
            int newDesiredY = validCoordinates.Item4 - validCoordinates.Item2;

            tList.Clear();

            //First Step (Fixing Alpha Channel)
            Console.WriteLine("\n\nTrim Images? [y/n]: ");

            if (Console.ReadLine().Contains("y"))
            {
                Console.WriteLine("--Trimmed:");

                for (int i = 0; i < threadNumber; i++)
                {
                    Thread t = new Thread(() => TrimThread(i, newDesiredX, newDesiredY, validCoordinates));
                    tList.Add(t);
                    t.Start();
                    Thread.Sleep(500);
                }

                while (true)
                {
                    Thread.Sleep(1000);
                    bool isAlive = false;
                    tList.ForEach((t) => isAlive = t.IsAlive || isAlive);
                    if (!isAlive) break;
                }
            }

            //Fifth Step - Create the spritesheet
            Console.WriteLine("\n\nCreating Spritesheet");

            int maxXSize = 0;
            int maxYSize = importedImageList[0].image.Height;
            int spritesPerLine = 20;
            //Calculating new image size
            for (int i = 0; i < importedImageList.Count;)
            {

                if (i < spritesPerLine)
                {
                    maxXSize += importedImageList[0].image.Width;
                    i++;
                }
                else
                {
                    maxYSize += importedImageList[0].image.Height;
                    i += 20;
                }
            }

            Color[][] newImageMatrix = ImageProcessing.CreateBlankColorMatrix(maxXSize, maxYSize);

            Console.WriteLine("--Appending:");

            int accX = 0;
            int accY = 0;
            int index = 0;
            importedImageList.ForEach((x) =>
            {
                ImageProcessing.AddImageIntoMatrix(newImageMatrix, x.image, accX, accY);
                accX += x.image.Width;
                Console.Write(x.id + ",");

                if (++index % 20 == 0)
                {
                    accY += x.image.Height;
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
