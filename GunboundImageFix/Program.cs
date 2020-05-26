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

                Console.WriteLine("\nDecrypt/Cypher");
                Console.WriteLine("9 - XTF Crack");

                Console.WriteLine("\nCreate Assets");
                Console.WriteLine("0 - Crosshair Drawer");
                Console.WriteLine("a - Mobile Buttons");
                Console.WriteLine("b - Create Minimap Tumbnails");


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
                            XTFCracker.Crack();
                            break;

                        case '0':
                            CrosshairDrawer.DrawCrosshairs();
                            break;
                        case 'a':
                            new AssetMaker().CreateButton();
                            break;
                        case 'b':
                            MinimapThumbGenerator.GenerateButtonThumbnails();
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
    }
}
