using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Congether.SDK.DotNet
{
    /// <summary>
    /// Client for the Congether service
    /// </summary>
    public class CongetherClient : IDisposable
    {
        private Client _client = null;
        private EndpointManifest _latestCachedManifest = null;
        private DateTime? _latestEndpointRequest = null;

        private CongetherFile _commonCongetherFile;
        private CongetherFile _appCongetherFile;

        private bool RequiresManifestFromService
        {
            get
            {
                return _latestEndpointRequest == null || (_latestEndpointRequest.Value.AddMinutes(30) < DateTime.Now);
            }
        }

        internal CongetherFileHandler FileHandler { get; private set; }
        internal string _appIdentifier { get; private set; }
        internal string _version { get; private set; }
        internal string _baseUrl { get; private set; }
        internal string _endpoint { get; private set; }
        internal string _secret { get; private set; }
        internal string _deviceKey { get; private set; }

        
        /// <summary>
        /// Handler for the Conductor-Module
        /// </summary>
        public ConductorHandler Conductor { get; private set; }

        /// <summary>
        /// Common Application informations
        /// </summary>
        public AppInfoHandler AppInfo { get; private set; }

        /// <summary>
        /// Handler for the Tracer-Module
        /// </summary>
        public TraceHandler Tracer { get; private set; }

        /// <summary>
        /// Currently used Device-Key
        /// </summary>
        public string DeviceKey { get { return _deviceKey; } }
        

        /// <summary>
        /// Constructs a new Congether-Client, for using the Congether-APIs
        /// </summary>
        /// <param name="appIdentifier">Application Identifier</param>
        /// <param name="baseUrl">URL of the Congether Service</param>
        /// <param name="endpoint">Your Endpoint-Identifier</param>
        /// <param name="secret">Shared Endpoint-Secret</param>
        /// <param name="deviceKey">Optional Device-Key (e.g. hostname, etc.)</param>
        /// <param name="version">Optional Version (if null, the version is the Assembly-Version)</param>
        public CongetherClient(string appIdentifier, string baseUrl, string endpoint, string secret, string deviceKey = null, string version = null)
        {
            this._baseUrl = baseUrl;
            this._endpoint = endpoint;
            this._secret = secret;
            this._appIdentifier = appIdentifier;
            this._version = version;
            this._deviceKey = deviceKey;

            FileHandler = new CongetherFileHandler(this);


            Conductor = new ConductorHandler(this);
            Tracer = new TraceHandler(this);
            AppInfo = new AppInfoHandler(this);        
        }

        public void Dispose()
        {
            this._baseUrl = null;
            this._endpoint = null;
            this._secret = null;
            this._client = null;
        }

        /// <summary>
        /// Initialize the Service and request the Manifest
        /// </summary>
        /// <returns>Async Task</returns>
        public async Task Initialize()
        {
            this._client = new Client(_baseUrl, _secret);
            this._commonCongetherFile = await FileHandler.ReadCommonCongetherFile();
            this._appCongetherFile = await FileHandler.ReadAppCongetherFile();
            await GetManifest();
        }
        
        /// <summary>
        /// Set the privacy-policy for this endpoint.
        /// </summary>
        /// <param name="policy">Policy-Identifier</param>
        /// <returns>Async Task</returns>
        public async Task SetPrivacyPolicy(string policy)
        {
            if(CongetherPrivacyMode.IsValid(policy))
            {
                var congetherFile = await this.FileHandler.GetCongetherFile(this._appIdentifier);
                congetherFile.Privacy_Mode = policy;
                congetherFile.Privacy_PolicyAccepted = DateTime.Now;
                await this.FileHandler.SetAppCongetherFile(congetherFile);
            }
        }
        
        /// <summary>
        /// Returns 
        /// </summary>
        /// <returns></returns>
        internal async Task<EndpointManifest> GetManifest()
        {
            EndpointManifest manifest = null;
            if(RequiresManifestFromService)
            {
                manifest = await GetManifestFromService();
                if (manifest != null)
                {
                    await this.FileHandler.SetCachedEndpointManifest(manifest);
                    _latestCachedManifest = manifest;

                    if(manifest.Endpoint.DeviceId != null && manifest.Endpoint.DeviceId != this._commonCongetherFile.InstanceId)
                    {
                        this._commonCongetherFile.InstanceId = manifest.Endpoint.DeviceId;
                        this.FileHandler.SetCommonCongetherFile(this._commonCongetherFile);
                    }
                    if (manifest.Endpoint.InstallationId != null && manifest.Endpoint.InstallationId != this._appCongetherFile.InstanceId)
                    {
                        this._appCongetherFile.InstanceId = manifest.Endpoint.InstallationId;
                        this.FileHandler.SetAppCongetherFile(this._appCongetherFile);
                    }
                }
            }
            if(manifest == null && _latestCachedManifest == null)
            {
                manifest = await this.FileHandler.GetCachedEndpointManifest();
                _latestCachedManifest = manifest;
            }
            if (manifest == null && _latestCachedManifest != null)
                manifest = _latestCachedManifest;
            
            return manifest;
        }

        internal async Task SendQueue(EndpointMessageQueue queue)
        {
            await this._client.ApiByVersionEndpointByEndpointEventPostAsync(this._endpoint, null, queue);
        }

        private async Task<EndpointManifest> GetManifestFromService()
        {
            var commonFile = await this.FileHandler.ReadCommonCongetherFile();
            var appFile = await this.FileHandler.ReadAppCongetherFile();

            EndpointManifest manifest = null;
            _latestEndpointRequest = DateTime.Now;
            try
            {
                manifest = await this._client.ApiByVersionEndpointByEndpointManifestGetAsync(_endpoint, null, await GetEndpointInfo());            
            }catch(Exception ex)
            {
                manifest = null;
            }

            return manifest;
        }

        internal async Task<EndpointInfo> GetEndpointInfo()
        {
            var endpoint = new EndpointInfo
            {
                DeviceKey = _deviceKey,
                PrivacyPolicy = _commonCongetherFile?.Privacy_Mode,
                AppIdentifier = _appIdentifier,
                AppVersion = _version ?? Assembly.GetEntryAssembly().GetName().Version.ToString(),
                InstallationId = _appCongetherFile?.InstanceId,
                DeviceId = _commonCongetherFile?.InstanceId,
                Environment = new Environment()
            };
            endpoint.Environment.OSVersion = System.Environment.OSVersion.VersionString;
            endpoint.Environment.Platform = System.Environment.OSVersion.Platform.ToString();            
            endpoint.Environment.RuntimeIdentifier = ".NET";
            endpoint.Environment.Hostname = System.Environment.MachineName;

            endpoint.EnsurePrivacyPolicy();

            return endpoint;
        }


    }
}
