using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Congether.SDK.DotNet
{
    internal class CongetherFileHandler
    {
        private DirectoryInfo CongetherCommonDir
        {
            get
            {
                var appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
                var congetherAppDataDir = Path.Combine(appData, "congether");
                if (!Directory.Exists(congetherAppDataDir))
                    Directory.CreateDirectory(congetherAppDataDir);
                return new DirectoryInfo(congetherAppDataDir);
            }
        }

        private DirectoryInfo CongetherAppDir
        {
            get
            {
                var congetherAppDataDir = Path.Combine(CongetherCommonDir.FullName, client._appIdentifier);
                if (!Directory.Exists(congetherAppDataDir))
                    Directory.CreateDirectory(congetherAppDataDir);
                return new DirectoryInfo(congetherAppDataDir);
            }
        }

        private CongetherClient client;
        internal CongetherFileHandler(CongetherClient client)
        {
            this.client = client;
        }

        internal async Task<CongetherFile> ReadAppCongetherFile()
        {
            return await GetCongetherFile(this.client._appIdentifier);
        }

        internal async Task<CongetherFile> ReadCommonCongetherFile()
        {
            return await GetCongetherFile();
        }

        internal async Task SetAppCongetherFile(CongetherFile file)
        {
            await SetCongetherFile(file, this.client._appIdentifier);
        }

        internal async Task SetCommonCongetherFile(CongetherFile file)
        {
            await SetCongetherFile(file);
        }

        internal async Task<EndpointManifest> GetCachedEndpointManifest()
        {
            var filePath = Path.Combine(CongetherAppDir.FullName, ".manifest");
            var ep = await GetOrCreateFile<EndpointManifest>(filePath);
            return ep;
        }

        internal async Task SetCachedEndpointManifest(EndpointManifest manifest)
        {
            var filePath = Path.Combine(CongetherAppDir.FullName, ".manifest");
            WriteToFile(manifest, filePath);
        }

        internal async Task SetCongetherFile(CongetherFile congetherFile, string appId = null)
        {
            var dir = CongetherCommonDir.FullName;
            if (appId != null)
                dir = CongetherAppDir.FullName;
            var filePath = Path.Combine(dir, ".congether");


            await WriteToFile<CongetherFile>(congetherFile, filePath);
        }

        internal async Task<CongetherFile> GetCongetherFile(string appId = null)
        {
            var dir = CongetherCommonDir.FullName;
            if (appId != null)
                dir = CongetherAppDir.FullName;
            var filePath = Path.Combine(dir, ".congether");
            return await GetOrCreateFile<CongetherFile>(filePath);
        }

        private async Task<T> GetOrCreateFile<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath))
            {
                var serializeObject = new T();
                await WriteToFile<T>(serializeObject, filePath);
                return serializeObject;
            }
            else
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
            }
        }

        private async Task WriteToFile<T>(T obj, string filePath) where T : new()
        {
            File.WriteAllText(filePath, Newtonsoft.Json.JsonConvert.SerializeObject(obj));
        
        }
    }
}
