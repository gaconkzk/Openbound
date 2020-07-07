using Microsoft.Xna.Framework;
using Openbound_Network_Object_Library.Common;
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

        public void Initialize()
        {
            metadataBasePath = $@"{Directory.GetCurrentDirectory()}\ContentMetadata\";
        }

        public void LoadAssetMetadata()
        {
            LoadMetadata<List<int[]>>("Mobile");
        }

        private void LoadMetadata<T>(string metadataPath)
        {
            List<string> filePaths = new List<string>();

            string basePivotPath = $"{metadataBasePath}{metadataPath}";
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
