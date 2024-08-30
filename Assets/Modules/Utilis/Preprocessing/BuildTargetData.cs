using System;
using UnityEditor;

namespace com.playbux.utilis.preprocessing
{
    [Serializable]
    public class BuildTargetData
    {
        public BuildTarget target;
        public iOSTargetDevice iOSSubtarget;
        public AndroidTargetDevices androidSubtarget;
        public bool enabled;
    }
}