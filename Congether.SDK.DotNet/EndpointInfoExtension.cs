using System;
using System.Collections.Generic;
using System.Text;

namespace Congether.SDK.DotNet
{
    public partial class EndpointInfo
    {
        public void Pseudonymize()
        {
            DeviceId = null;
            if (Environment != null)
            {
                if (Environment.Hostname?.Length > 2)
                    Environment.Hostname = Environment.Hostname.Substring(0, 2);
            }
        }

        private void Anonymize()
        {
            InstallationId = null;
            DeviceId = null;

            if (Environment != null)
            {
                Environment.Hostname = null;
            }
        }

        internal void EnsurePrivacyPolicy()
        {
            if (Privacy_Accepted)
            {

            }
            else if (Privacy_Pseudonymize)
            {
                this.Pseudonymize();
            }
            else if (Privacy_Anonymize)
            {
                this.Anonymize();
            }
        }

        private bool Privacy_Accepted => String.Equals(this.PrivacyPolicy, CongetherPrivacyMode.ACCEPTED) || String.IsNullOrEmpty(this.PrivacyPolicy);
        private bool Privacy_Pseudonymize => String.Equals(this.PrivacyPolicy, CongetherPrivacyMode.PSEUDONYMISATION);
        private bool Privacy_Anonymize => String.Equals(this.PrivacyPolicy, CongetherPrivacyMode.ANONYMISATION);
    }
}
