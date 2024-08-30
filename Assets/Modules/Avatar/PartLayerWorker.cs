using UnityEngine;
using System.Collections.Generic;

namespace com.playbux.avatar
{
    public class PartLayerWorker
    {
        private List<string> ignoreLayerList = new List<string>();

        public virtual void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null)
                return;

            foreach (string name in ignoreLayerList) {
                if (obj.name == name)
                {
                    return;
                }
            }

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (child == null)
                    continue;

                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}