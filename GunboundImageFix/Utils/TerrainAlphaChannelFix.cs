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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GunboundImageFix.Utils
{
    class TerrainFix
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

        void ReadAllImages()
        {
            Console.WriteLine("Importing Sprites...");
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.InitialDirectory = Common.Directory + @"Output";
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
        }

        public TerrainFix()
        {
            Thread t = new Thread(ReadAllImages);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            while (t.IsAlive)
                Thread.Sleep(1000);

            if (importedImageList.Count == 0)
                return;
        }

        public void AggressiveAlphaFix()
        {
            Console.WriteLine("Enter the number of dilatations:");
            int dAmmount = int.Parse(Console.ReadLine());

            int[][][] aRGB = ImageProcessing.SplitImageChannels(importedImageList[0].image);

            for (int i = 0; i < dAmmount; i++)
                aRGB[0] = ImageProcessing.IncreaseMatrixSize(aRGB[0]);

            for (int i = 0; i < dAmmount; i++)
                aRGB[0] = ImageProcessing.Dilatate(aRGB[0]);

            for (int i = 0; i < dAmmount; i++)
                aRGB[0] = ImageProcessing.Erode(aRGB[0]);

            for (int i = 0; i < dAmmount; i++)
                aRGB[0] = ImageProcessing.DecreaseMatrixSize(aRGB[0]);

            ImageProcessing.CreateImage(aRGB[0], aRGB[1], aRGB[2], aRGB[3]).Save(Common.Directory + @"\Output\Stage\output.png");
        }
    }
}
