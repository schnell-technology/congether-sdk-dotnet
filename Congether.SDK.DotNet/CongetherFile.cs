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

    internal class CongetherPrivacyMode
    {
        internal const string ACCEPTED = null;
        internal const string PSEUDONYMISATION = "congether.privacy.pseudonymisation";
        internal const string ANONYMISATION = "congether.privacy.anonymisation";

        internal static bool IsValid(string policy)
        {
            return new List<string>(new string[] { ACCEPTED, PSEUDONYMISATION, ANONYMISATION }).Contains(policy);
        }
    }
}
