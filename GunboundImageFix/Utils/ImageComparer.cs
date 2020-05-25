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
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace GunboundImageFix.Utils
{
    class ImageComparer
    {
        static string[] imageFilePaths;
        static string[] imageFilePaths2;

        [STAThread]
        public static void ImportIMGThread()
        {
            Console.WriteLine("\n\nImporting Sprites...");
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.InitialDirectory = Common.Directory + @"Input";
            dialog.Filter = "Image Files (png)|*.png;";
            dialog.ShowDialog();
            imageFilePaths = dialog.FileNames;
        }

        public static void CompareImages()
        {
            Console.WriteLine("Importing IMG Files");
            Thread t = new Thread(() => ImportIMGThread());
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            while (t.IsAlive) Thread.Sleep(100);

            imageFilePaths2 = imageFilePaths;

            Console.WriteLine("Importing IMG Files");
            t = new Thread(() => ImportIMGThread());
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            while (t.IsAlive) Thread.Sleep(100);

            Bitmap img1 = new Bitmap(imageFilePaths[0]);
            Bitmap img2 = new Bitmap(imageFilePaths2[0]);

            ImageProcessing.CompareImages(img1, img2, 0);
        }
    }
}
