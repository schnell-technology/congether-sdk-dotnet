using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Congether.SDK.DotNet
{
    /// <summary>
    /// SDK for Conductor-Module
    /// </summary>
    public class ConductorHandler
    {
        CongetherClient client = null;
        internal ConductorHandler(CongetherClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Get a configuration-value from Conductor. If no value can be found, the defaultValue will be returned.
        /// </summary>
        /// <typeparam name="T">Type of the value (primitive)</typeparam>
        /// <param name="key">Requested configuration-key</param>
        /// <param name="defaultValue">Default value, if no key can be found</param>
        /// <returns></returns>
        public async Task<T> GetConfigurationValue<T>(string key, T defaultValue = default(T))
        {
            var manifest = await client.GetManifest();
            if(manifest != null && manifest.Configuration as IDictionary<string, object> != null)
            {
                var conf = manifest.Configuration as IDictionary<string, object>;
                try
                {
                    return (T)conf[key];
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"The requested configuration-value {key} could not be found. The default-value will be returned.");
                }
            }

            return defaultValue;
        }
    }
}
