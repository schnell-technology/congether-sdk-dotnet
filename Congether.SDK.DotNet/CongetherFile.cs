using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Congether.SDK.DotNet
{
    [DataContract]
    public class CongetherFile
    {   
        [DataMember]
        public Guid? InstanceId { get; set; }
        [DataMember]
        public string Privacy_Mode { get; set; }
        [DataMember]
        public DateTime? Privacy_PolicyAccepted { get; set; }
        
        public CongetherFile()
        {
            Privacy_Mode = CongetherPrivacyMode.PSEUDONYMISATION;
        }
    }

    public class CongetherPrivacyMode
    {
        /// <summary>
        /// Accepts all installation- and device-informations including Hostname and IP-Addresses
        /// </summary>
        public const string ACCEPTED = null;

        /// <summary>
        /// Accepts installation-informations and a subset of Hostname and IP-Adresses. It disallows a device-reference.
        /// </summary>
        public const string PSEUDONYMISATION = "congether.privacy.pseudonymisation";

        /// <summary>
        /// This will disallow all installation- and device-informations.
        /// </summary>
        public const string ANONYMISATION = "congether.privacy.anonymisation";

        internal static bool IsValid(string policy)
        {
            return new List<string>(new string[] { ACCEPTED, PSEUDONYMISATION, ANONYMISATION }).Contains(policy);
        }
    }
}
