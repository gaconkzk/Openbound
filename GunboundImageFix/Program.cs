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

using GunboundImageFix.Extension;
using GunboundImageFix.Utils;
using GunboundImageProcessing.ImageUtils;
using InsideGB.Storage;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace GunboundImageFix
{
    class Program
    {
        public static void Main(string[] args)
        {
            do
            {
                Console.WriteLine("Gunbound Raw Image Fix Tools");

                Console.WriteLine("SpriteSheet tools");
                Console.WriteLine("1 - Create Spritesheet [SFX][Custom Image Size][1-Layer]");
                Console.WriteLine("2 - Create Spritesheet [SFX][Custom Image Size][2-Layer - AlphaBlend]");
                Console.WriteLine("3 - Create Spritesheet [Equal Images]");

                Console.WriteLine("\nImage Utils");
                Console.WriteLine("4 - Image Border Fixer");

                Console.WriteLine("\nImage Processing");
                Console.WriteLine("5 - Massive .IMG import.");
                Console.WriteLine("6 - Image Sync Comparer [DEPRECATED]");
                Console.WriteLine("7 - Image Comparer");

                Console.WriteLine("\nFile manipulation");
                Console.WriteLine("8 - Name Fixer.");
                Console.WriteLine("9 - Pivot Offset Fixer");

                Console.WriteLine("\nDecrypt/Cypher");
                Console.WriteLine("0 - XTF Crack");

                Console.WriteLine("\nCreate Assets");
                Console.WriteLine("a - Crosshair Drawer");
                Console.WriteLine("b - Mobile Buttons");
                Console.WriteLine("c - Create Minimap Tumbnails");
                Console.WriteLine("d - Spritefont Range Builder");

                ExtractIMGData();

                try
                {
                    DateTime sDate = DateTime.Now;

                    switch (Console.ReadLine()[0])
                    {
                        case '1':
                            new SingleLayerSpritesheetMaker().CreateSpritesheet();
                            break;
                        case '2':
                            new MultiLayerSpritesheetMaker().CreateSpritesheet();
                            break;
                        case '3':
                            new SimpleSpritesheetMaker().CreateSpritesheet();
                            break;

                        case '4':
                            ImageBorderFix.FixBorder();
                            break;

                        case '5':
                            new SpriteImportManager().ImportSprites();
                            break;
                        case '6':
                            ImageSyncComparer.ImageSyncCompare();
                            break;
                        case '7':
                            ImageComparer.CompareImages();
                            break;

                        case '8':
                            new FileNameFixer().ImportSprites();
                            break;
                        case '9':
                            PivotFileManager.FixPivotFile();
                            break;

                        case '0':
                            XTFCracker.Crack();
                            break;

                        case 'a':
                            CrosshairDrawer.DrawCrosshairs();
                            break;
                        case 'b':
                            new AssetMaker().CreateButton();
                            break;
                        case 'c':
                            MinimapThumbGenerator.GenerateButtonThumbnails();
                            break;
                        case 'd':
                            SpritefontRangeBuilder.BuildSpritefontRange();
                            break;
                        default:
                            throw new Exception();
                    }

                    Console.WriteLine("Process complete. Running time: " + (DateTime.Now - sDate).TotalSeconds);
                    Console.ReadKey();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadKey();
                    Console.Clear();
                }
            } while (true);
        }

        public static void ExtractIMGData()
        {
            byte[] imgByteArray = File.ReadAllBytes(@"C:\Users\Carlos\source\repos\Openbound\OpenBound\Tanks Test\tank7.img");

            //Parsing data stream into integer
            Queue<byte> valueQueue = new Queue<byte>();

            for (int i = 0; i < imgByteArray.Length; i++)
                valueQueue.Enqueue(imgByteArray[i]);

            //File data:
            //
            // bits  |   i      | value
            //  2x16 |   0      | zero - ? 
            //  2x16 |   1      | number of images
            //  2x16 |   2  (0) | ? - v0
            // ------+----------+ Loop starts here
            //    32 |   3  (1) | width
            //    32 |   4  (2) | height
            //    32 |   5  (3) | coord X
            //    32 |   6  (4) | coord Y
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
            // I have decided to replace the 4 don't care for 1 if the current value of alpha, red, green or blue isn't 0
            //
            // Where v0 is the first information on the header, numberOfImages is v1, and v2 is the third
            // Every v# variable from now on are unknown values.

            int v0 = valueQueue.DequeueInt32();
            int numberOfImages = valueQueue.DequeueInt32();

            for (int i = 0; i < numberOfImages; i++)
            {
                int v2 = valueQueue.DequeueInt32();
                int width = valueQueue.DequeueInt32();
                int height = valueQueue.DequeueInt32();
                int coordX = valueQueue.DequeueInt32();
                int coordY = valueQueue.DequeueInt32();
                int v5 = valueQueue.DequeueInt32();
                int v6 = valueQueue.DequeueInt32();
                int pilotPivotX = valueQueue.DequeueInt32();
                int pilotPivotY = valueQueue.DequeueInt32();
                int dataStreamLength = valueQueue.DequeueInt32();

                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);

                for (int h = 0; h < dataStreamLength; h+=2)
                {
                    byte b1 = valueQueue.Dequeue();
                    byte b2 = valueQueue.Dequeue();

                    int a = (b2 & 0xF0) << 0;
                    int r = (b2 & 0x0F) << 4;
                    int g = (b1 & 0xF0) << 0;
                    int b = (b1 & 0x0F) << 4;


                    if (b > 0) b |= 0xF;
                    if (g > 0) g |= 0xF;
                    if (r > 0) r |= 0xF;
                    if (a > 0) a |= 0xF;

                    bitmap.SetPixel((h / 2) % width, (h / 2) / width, Color.FromArgb(a, r, g, b));
                }


            }
        }
    }
}
