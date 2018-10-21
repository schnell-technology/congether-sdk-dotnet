using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Congether.SDK.DotNet
{
    public class AppInfoHandler
    {
        CongetherClient client = null;
        internal AppInfoHandler(CongetherClient client)
        {
            this.client = client;
        }

        public async Task<AppInfo> GetAppInfo()
        {
            var manifest = await client.GetManifest();
            if (manifest != null && manifest.AppInfo != null)
            {
                return manifest.AppInfo;
            }

            return new AppInfo() { HasUpdate=false };
        }
    }
}
