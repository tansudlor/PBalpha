using System;
using UnityEngine;

namespace com.playbux.LOD
{
    [Serializable]
    public abstract class LODAsset<T> : ScriptableObject
    {
        public abstract bool IsEmpty { get; }
        public virtual T HighQuality => highQuality.Equals(null) ? defaultQuality : highQuality;
        public virtual T MediumQuality => mediumQuality.Equals(null) ? defaultQuality : mediumQuality;
        public virtual T LowQuality => lowQuality.Equals(null) ? defaultQuality : lowQuality;
        public virtual T DefaultQuality => defaultQuality;

        [SerializeField]
        private T highQuality;
        [SerializeField]
        private T mediumQuality;
        [SerializeField]
        private T lowQuality;
        [SerializeField]
        private T defaultQuality;

        public void SetHighQualityTexture(T highQuality)
        {
            this.highQuality = highQuality;
        }

        public void SetMediumQualityTexture(T mediumQuality)
        {
            this.mediumQuality = mediumQuality;
        }

        public void SetLowQualityTexture(T lowQuality)
        {
            this.lowQuality = lowQuality;
        }

        public void SetDefaultTexture(T defaultQuality)
        {
            this.defaultQuality = defaultQuality;
        }
    }

}