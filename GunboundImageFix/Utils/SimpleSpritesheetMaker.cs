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
using GunboundImageFix.Entity;
using GunboundImageFix.Common;
using System.Linq;

namespace GunboundImageFix.Utils
{
    class SimpleSpritesheetMaker
    {
        public void CreateSpritesheet()
        {
            List<ImportedImage> imgList = FileImportManager.ReadMultipleImages();

            int width = imgList[0].Image.Width;
            int height = imgList[0].Image.Height;

            Console.WriteLine("--Sprites per Line: ");

            int spritesPerLine = int.Parse(Console.ReadLine());

            Color[][] newImageMatrix = ImageProcessing.CreateBlankColorMatrix((width + 2) * spritesPerLine, (height + 2) * (int)Math.Ceiling(imgList.Count / (double)spritesPerLine));

            int accX = 1;
            int accY = 1;
            int index = 0;
            foreach (ImportedImage img in imgList)
            {
                ImageProcessing.AddImageIntoMatrix(newImageMatrix, img.Image, accX, accY);

                accX += width + 2;

                if (++index % spritesPerLine == 0)
                {
                    accY += height + 2;
                    accX = 1;
                }
            }

            ImageProcessing.CreateImage(newImageMatrix).Save(Parameters.SpritesheetOutputDirectory + @"output.png");
        }
    }
}
