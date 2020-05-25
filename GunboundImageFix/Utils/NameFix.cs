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

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace GunboundImageFix.Utils
{
    class NameFix
    {
        string[] imageFilePaths;

        [STAThread]
        public void ImportIMGThread()
        {
            Console.WriteLine("\n\nImporting Sprites...");

            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = true,
                InitialDirectory = Common.Directory + @"Input",
                Filter = "Image Files (PNG)|*.png"
            };

            dialog.ShowDialog();
            imageFilePaths = dialog.FileNames;
        }

        public void ImportSprites()
        {
            Console.WriteLine("Fixing Image Names");
            Thread t = new Thread(() => ImportIMGThread());
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            while (t.IsAlive) Thread.Sleep(1000);

            try
            {
                Console.WriteLine("Enter the starting number: ");
                int sNum = int.Parse(Console.ReadLine());

                string[] name = imageFilePaths[0].Split('\\');

                string text = string.Join("/", name.ToList().GetRange(0, name.Length - 1));

                foreach (string imgFilePath in imageFilePaths)
                {
                    try
                    {
                        File.Move(imgFilePath, text + "/" + sNum++ + ".png");
                    }
                    catch
                    {

                    }
                }

                Console.Write("\n\nProcess terminated.");
            }
            catch { }
        }
    }
}
