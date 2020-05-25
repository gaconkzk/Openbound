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

using GunboundImageFix.Utils;
using GunboundImageProcessing.ImageUtils;
using System;
using System.Drawing;

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
                Console.WriteLine("1 - Create Spritesheet");
                Console.WriteLine("2 - Create Spritesheet [Repositioning]");
                Console.WriteLine("3 - Create Spritesheet [Custom Image Size]");
                Console.WriteLine("b - Create Spritesheet [SFX][Custom Image Size]");
                Console.WriteLine("c - Create Spritesheet [SFX][+Blend]");
                Console.WriteLine("e - Create Minimap Tumbnails");

                Console.WriteLine("\nImage Processing");
                Console.WriteLine("4 - Aggressive Alpha Fix for stages.");
                Console.WriteLine("5 - Massive .IMG import.");
                Console.WriteLine("7 - Image Sync Comparer");
                Console.WriteLine("9 - Image Comparer");

                Console.WriteLine("\nFile manipulation");
                Console.WriteLine("6 - Name Fixer.");

                Console.WriteLine("\nDecrypt/Cypher");
                Console.WriteLine("8 - XTF Crack");

                Console.WriteLine("\nCreate Assets");
                Console.WriteLine("a - Crosshair Drawer");
                Console.WriteLine("d - Mobile Buttons");


                /*try
                {*/
                    DateTime sDate = DateTime.Now;

                    switch (Console.ReadLine()[0])
                    {
                        case '1':
                            new SpritesheetMaker().CreateSpritesheet();
                            break;
                        case '2':
                            new SpritesheetMaker2().CreateSpritesheet();
                            break;
                        case '3':
                            new SpritesheetMaker3().CreateSpritesheet();
                            break;
                        case '4':
                            new TerrainFix().AggressiveAlphaFix();
                            break;
                        case '5':
                            new SpriteImportManager().ImportSprites();
                            break;
                        case '6':
                            new NameFix().ImportSprites();
                            break;
                        case '7':
                            ImageSyncComparer.ImageSyncCompare();
                            break;
                        case '8':
                            XTFCracker.Crack();
                            break;
                        case '9':
                            ImageComparer.CompareImages();
                            break;
                        case 'a':
                            CrosshairDrawer.DrawCrosshairs();
                            break;
                        case 'b':
                            new SpritesheetMaker4().CreateSpritesheet();
                            break;
                        case 'c':
                            new SpritesheetMaker5Blender().CreateSpritesheet();
                            break;
                        case 'd':
                            new AssetMaker().CreateButton();
                            break;
                        case 'e':
                            MinimapThumbGenerator.GenerateButtonThumbnails();
                            break;
                        case 'f':
                            FixImage();
                            break;
                        default:
                            throw new Exception();
                    }

                    Console.WriteLine("Process complete. Running time: " + (DateTime.Now - sDate).TotalSeconds);
                    Console.ReadKey();
                /*}
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadKey();
                    Console.Clear();
                }*/
            } while (true);
        }

        public static void FixImage()
        {
            Bitmap bmp = (Bitmap)Image.FromFile(@"C:\Users\Carlos\source\repos\OpenBound\OpenBound\Content\Graphics\Maps\FourHeads\ForegroundA.png");
            int[][][] imageChannels = new int[4][][];

            imageChannels = ImageProcessing.SplitImageChannels(bmp);

            imageChannels[0] = ImageProcessing.Erode(imageChannels[0]);
            imageChannels[1] = ImageProcessing.Erode(imageChannels[1]);
            imageChannels[2] = ImageProcessing.Erode(imageChannels[2]);
            imageChannels[3] = ImageProcessing.Erode(imageChannels[3]);

            //imageChannels[0] = ImageProcessing.Dilatate(imageChannels[0]);
            //imageChannels[1] = ImageProcessing.Dilatate(imageChannels[1]);
            //imageChannels[2] = ImageProcessing.Dilatate(imageChannels[2]);
            //imageChannels[3] = ImageProcessing.Dilatate(imageChannels[3]);

            ImageProcessing.CreateImage(ImageProcessing.JoinImageChannels(imageChannels)).Save(@"C:\Users\Carlos\source\repos\OpenBound\GunboundImageFix\Output\FixedImage\output.png");
        }
    }
}
