using UnityEngine;

namespace com.playbux.utilis
{
    public static class ImageHelper
    {
        public static bool IsEmpty(this Texture2D tex)
        {
            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                    if (tex.GetPixel(x, y).a != 0)
                        return false;
            }
            
            return true;
        }
    }
}