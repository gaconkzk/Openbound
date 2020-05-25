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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using OpenBound.Common;
using OpenBound.Extension;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenBound.GameComponents.Renderer
{
    public class AssetHandler
    {
        private AssetHandler() { }
        private static AssetHandler instance;
        public static AssetHandler Instance
        {
            get
            {
                if (instance == null) instance = new AssetHandler();
                return instance;
            }
        }

        private AssetManager manager;
        private ContentManager contentManager;
        private GraphicsDevice graphicsDevice;

        private string contentAssetPath;

        public void Initialize(ContentManager ContentManager, GraphicsDevice GraphicsDevice)
        {
            contentManager = ContentManager;
            graphicsDevice = GraphicsDevice;
            manager = new AssetManager(ContentManager);
            contentAssetPath = $@"{Directory.GetCurrentDirectory()}\{contentManager.RootDirectory}\";
        }

        private void AppendAllSubdirectoriesIntoList(ref List<String> directoryList, string directoryRoot)
        {
            foreach (string subDirectory in Directory.GetDirectories(directoryRoot))
            {
                directoryList.Add(subDirectory);
                AppendAllSubdirectoriesIntoList(ref directoryList, subDirectory);
            }
        }

        private void AppendFilePaths(ref List<string> filePathList, string directoryRoot)
        {
            directoryRoot = directoryRoot.Replace("/", "\\");
            List<string> fileList = new List<string>();
            List<string> directoryList = new List<string>();

            string baseDirectory = $@"{contentAssetPath}\{directoryRoot}";

            directoryList.Add(baseDirectory);

            AppendAllSubdirectoriesIntoList(ref directoryList, baseDirectory);

            foreach (string subDirectory in directoryList)
            {
                fileList.AddRange(Directory.GetFiles(subDirectory));
            }

            foreach (string path in fileList)
            {
#if DEBUG
                Console.WriteLine($"ImportedFile: {path}");
#endif

                if (path.Contains(".xnb"))
                {
                    filePathList.Add(
                        path.Replace(@contentAssetPath + "\\", "")
                            .Replace(".xnb", ""));
                }
            }
        }

        public SpriteFont RequestFont(string path)
        {
            SpriteFont rFont = (SpriteFont)manager.RequestAsset(path);

            if (rFont == null)
                throw new Exception();

            return rFont;
        }

        public Texture2D RequestTexture(string path)
        {
            Texture2D rAsset = (Texture2D)manager.RequestAsset(path);

            if (rAsset == null)
            {
#if DEBUG
                Console.WriteLine($"Resource not found: {path}");
#endif
                throw new Exception();
            }

            return rAsset;
        }

        public Texture2D RequestTextureCopy(string path)
        {
            Texture2D rAsset = (Texture2D)manager.RequestAsset(path);

            if (rAsset == null)
            {
#if DEBUG
                Console.WriteLine($"Resource not found: {path}");
#endif
                throw new Exception();
            }

            //Performing the copy
            Color[] colorArray = new Color[rAsset.Width * rAsset.Height];
            rAsset.GetData(colorArray);

            Texture2D newTexture = new Texture2D(graphicsDevice, rAsset.Width, rAsset.Height);
            newTexture.SetData(colorArray);

            return newTexture;
        }

        public Texture2D RequestTextureCopy(Texture2D texture2D, int newWidth, int newHeight, int startingIndexX, int startingIndexY, int offsetX, int offsetY)
        {
            Color[][] originalData = texture2D.GetData();
            Color[][] newOriginalData = originalData.ExtractMatrixSlice(startingIndexX, texture2D.Width - startingIndexX, startingIndexY, texture2D.Height - startingIndexY);
            Color[][] newColorMatrix = Helper.CreateMatrix(newWidth, newHeight, Color.Transparent);

            newColorMatrix.ApplyMatrixSlice(newOriginalData, offsetX, offsetY);
            Texture2D newTexture = new Texture2D(graphicsDevice, newWidth, newHeight);
            newTexture.SetData(newColorMatrix.ConvertTo1D());

            return newTexture;
        }

        public Texture2D CreateAsset(int Width, int Height)
        {
            return new Texture2D(graphicsDevice, Width, Height);
        }

        public Song RequestSong(string path)
        {
            Song rAsset = (Song)manager.RequestAsset(path);

            if (rAsset == null)
            {
#if DEBUG
                Console.WriteLine($"Resource not found: {path}");
#endif
                throw new Exception();
            }

            return rAsset;
        }

        public SoundEffect RequestSoundEffect(string path)
        {
            SoundEffect rAsset = (SoundEffect)manager.RequestAsset(path);

            if (rAsset == null)
            {
#if DEBUG
                Console.WriteLine($"Resource not found: {path}");
#endif
                throw new Exception();
            }

            return rAsset;
        }

        public void LoadAllAssets()
        {
            LoadSpriteFonts();

            // 2D Textures
            List<string> sList = new List<string>();
            AppendFilePaths(ref sList, @"Debug");
            AppendFilePaths(ref sList, @"Graphics");
            AppendFilePaths(ref sList, @"Interface");
            AppendFilePaths(ref sList, @"Misc");
            manager.LoadAsset<Texture2D>(sList);

            //Audio
            sList.Clear();
            AppendFilePaths(ref sList, @"Audio/Music");
            manager.LoadAsset<Song>(sList);

            sList.Clear();
            AppendFilePaths(ref sList, @"Audio/SFX");
            manager.LoadAsset<SoundEffect>(sList);

            /*
            LoadMenuAssets();
            LoadMapAssets();
            LoadTankAssets();
            LoadCursorAssets();
            LoadHUDAssets();
            LoadDebugAssets();
            LoadHealthAsset();
            */
        }

        public void LoadSpriteFonts()
        {
            List<string> sList = new List<string>();
            AppendFilePaths(ref sList, @"Fonts");
            manager.LoadAsset<SpriteFont>(sList);
        }
    }
}
