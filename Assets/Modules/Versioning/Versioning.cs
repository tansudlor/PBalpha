using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.versioning
{
    [CreateAssetMenu(menuName = "Playbux/Versioning/Version", fileName = "VersioningFile")]
    public class Versioning : ScriptableObject
    {
        //Versioning Format
        //[major].[minor].[patch].[revision]
        //Version 0.1.0.0
        [SerializeField]
        private string serverVersion;

        [SerializeField]
        private string clientVersion;

        [SerializeField]
        private List<string> supportServerVersion;

        public string ServerVersion { get => serverVersion; set => serverVersion = value; }
        public string ClientVersion { get => clientVersion; set => clientVersion = value; }
        public List<string> SupportServerVersion { get => supportServerVersion; set => supportServerVersion = value; }
    }
}
