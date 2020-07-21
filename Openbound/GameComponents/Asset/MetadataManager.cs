﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenBound.GameComponents.Pawn;
using Openbound_Network_Object_Library.Common;
using Openbound_Network_Object_Library.Entity;
using Openbound_Network_Object_Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace OpenBound.GameComponents.Asset
{
    public class MetadataManager
    {
        public static Dictionary<string, object> ElementMetadata = new Dictionary<string, object>();
        
        public static Dictionary<Gender, 
            Dictionary<AvatarCategory, 
                Dictionary<int, AvatarMetadata>>> AvatarMetadataDictionary =
            new Dictionary<Gender, 
                Dictionary<AvatarCategory, 
                    Dictionary<int, AvatarMetadata>>>();

        private static MetadataManager instance;
        public static MetadataManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new MetadataManager();

                return instance;
            }
        }

        public MetadataManager() { }

        string metadataBasePath;
        //ideally the game should fetch the information from a browser
        string downloadedMetadataBasePath;

        public void Initialize()
        {
            metadataBasePath = $@"{Directory.GetCurrentDirectory()}\ContentMetadata\";
            downloadedMetadataBasePath = $@"{Directory.GetCurrentDirectory()}\";
        }

        public void LoadAssetMetadata()
        {
            //Mobile metadata
            LoadMetadata<List<int[]>>(metadataBasePath, "Mobile");

            //Avatar metadata
            LoadMetadata<List<AvatarMetadata>>(downloadedMetadataBasePath, "DatabaseSeed");
            LoadAvatarMetadata();
        }
        

        private void LoadAvatarMetadata()
        {
            AvatarMetadataDictionary.Add(Gender.Male,   new Dictionary<AvatarCategory, Dictionary<int, AvatarMetadata>>());
            AvatarMetadataDictionary.Add(Gender.Female, new Dictionary<AvatarCategory, Dictionary<int, AvatarMetadata>>());

            foreach(AvatarCategory category in (AvatarCategory[])Enum.GetValues(typeof(AvatarCategory)))
            {
                AvatarMetadataDictionary[Gender.Male].Add(category, new Dictionary<int, AvatarMetadata>());
                AvatarMetadataDictionary[Gender.Female].Add(category, new Dictionary<int, AvatarMetadata>());
            }

            foreach (AvatarMetadata am in (List<AvatarMetadata>)ElementMetadata["DatabaseSeed/AvatarMetadata"])
            {
                if (am.Gender != Gender.Unissex)
                    AvatarMetadataDictionary[am.Gender][am.AvatarCategory].Add(am.ID, am);
                else
                {
                    AvatarMetadataDictionary[Gender.Male][am.AvatarCategory].Add(am.ID, am);
                    AvatarMetadataDictionary[Gender.Female][am.AvatarCategory].Add(am.ID, am);
                }
            }
        }

        private void LoadMetadata<T>(string basePath, string metadataPath)
        {
            List<string> filePaths = new List<string>();

            string basePivotPath = $"{basePath}{metadataPath}";
            IOManager.GetAllSubdirectories(basePivotPath).ForEach((x) => filePaths.AddRange(Directory.GetFiles(x)));

            foreach (string str in filePaths)
            {
#if DEBUG
                Console.WriteLine(str);
#endif

                string fileContent = File.ReadAllText(str);

                string dicPath = str.Replace($"{basePivotPath}\\", "").Replace("\\","/").Split('.')[0];
                ElementMetadata.Add($@"{metadataPath}/{dicPath}", ObjectWrapper.DeserializeRequest<T>(fileContent));
            }
        }
    }
}
