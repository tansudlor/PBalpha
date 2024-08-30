using System;
using com.playbux.utilis;
using UnityEngine;

namespace com.playbux.LOD
{
    [Serializable]
    [CreateAssetMenu(menuName = "Playbux/LOD/Create LODTexture2D", fileName = "LODTexture2D", order = 0)]
    public class LODTexture2D : LODAsset<Texture2D>
    {
        public override bool IsEmpty
        {
            get
            {
                bool hasHighQuality = !HighQuality.Equals(null);
                bool hasMediumQuality = !MediumQuality.Equals(null);
                bool hasLowQuality = !LowQuality.Equals(null);
                bool hasDefaultQuality = !DefaultQuality.Equals(null);

                if (hasHighQuality)
                    return HighQuality.IsEmpty();

                if (hasMediumQuality)
                    return MediumQuality.IsEmpty();

                if (hasLowQuality)
                    return LowQuality.IsEmpty();

                return hasDefaultQuality && DefaultQuality.IsEmpty();

            }
        }
    }
}