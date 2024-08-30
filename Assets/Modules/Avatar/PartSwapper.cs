using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using com.playbux.bux;
using com.playbux.utilis.extension;
using Animator = UnityEngine.Animator;

namespace com.playbux.avatar
{
    public class PartDirectionWorker
    {
        private readonly IAnimator animator;
        private readonly Animator[] directions;

        private int currentDirection;

        public PartDirectionWorker(IAnimator animator, Animator[] directions)
        {
            this.animator = animator;
            this.directions = directions;
        }

        public virtual void ChangeDirection(int newDirection)
        {
            for (int i = 0; i < directions.Length; i++)
                directions[i].gameObject.SetActive(false);

            directions[newDirection % 4].gameObject.SetActive(true);

            if (currentDirection % 4 != newDirection % 4)
                animator.ClearCurrentAnimationName();

            currentDirection = newDirection;
        }
    }

    public class PartSwapper
    {
        private const string DELETED_NAME = "Deleted";
        private const string S_FIND_PART = "Prefab_Avatar_Part";
        private const string BASE_FOLDER = "Assets/Modules/Avatar/Bux Parts/";
        private readonly PartLayerWorker layerWorker;
        private readonly SpriteRendererFactory factory;

        private Dictionary<string, SpriteRenderer> spriteMap;
        private Dictionary<string, PartPathMaping> allCollection;

        public PartSwapper(
            PartLayerWorker layerWorker,
            SpriteRendererFactory factory,
            SpriteRenderer[] spriteRenderers,
            AvatarPartsCollection[] collectons)
        {
            this.factory = factory;
            this.layerWorker = layerWorker;
            allCollection = new Dictionary<string, PartPathMaping>();
            allCollection = collectons.SelectMany(avatarPartCollection => avatarPartCollection.Map)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            spriteMap = new Dictionary<string, SpriteRenderer>();
            foreach (var renderer in spriteRenderers)
            {
                spriteMap[renderer.name.ToLower()] = renderer;
            }
        }

        public void ChangeParts(IAvatarSet changedSet)
        {
            // Retrieve all avatar parts (e.g., ["hat", "shoes", "shirt"]).
            var allParts = changedSet.GetParts();
            // Loop through all the parts.
            for (int i = 0; i < allParts.Length; i++)
            {
                var part = allParts[i]; // e.g., "hat"
                //clear all direction part
                for (int d = 0; d < 4; d++)
                {
                    string keyBase = part + "_" + d; // e.g., "hat_0"
                    if (!spriteMap.ContainsKey(keyBase))
                    {
                        ClearSplitPart(part, d);
                        continue;
                    }

                    ClearPart(spriteMap[keyBase]);
                }
                // Check the part for all 4 directions (0, 1, 2, 3).
                for (int d = 0; d < 4; d++)
                {
                    string keyBase = part + "_" + d; // e.g., "hat_0"
                    if (!spriteMap.ContainsKey(keyBase))
                    {
                        UpdateSplitPart(part, d, changedSet);
                        continue;
                    }

                    TrySwap(GetNFTCollection(changedSet.Normalized[part]), spriteMap[keyBase], changedSet.Normalized[part] + "_" + d);
                }
            }
        }

        public void TrySwap(string collectionIndex, Object swapTarget, string idToPath)
        {
            switch (collectionIndex)
            {
                case "1":
                    SwapImage((SpriteRenderer)swapTarget, idToPath);
                    break;
                case "2":
                    SwapPrefab((GameObject)swapTarget, idToPath);
                    break;
            }
        }

        public string GetNFTCollection(string normalizedPart)
        {
            return normalizedPart.Split('/')[0].ToLower();
        }

        private void UpdateSplitPart(string part, int direction, IAvatarSet changedSet)
        {
            var subpart = part.Substring(0, part.Length - 1); // e.g., "shoes" becomes "shoe".
            var directions = new[] { "_r", "_l" };            // Right and left identifiers.
            foreach (var dir in directions)
            {
                var partname = subpart + "_" + direction + dir; // e.g., "shoe_0_r"

                if (spriteMap.ContainsKey(partname))
                {
                    TrySwap(GetNFTCollection(changedSet.Normalized[part]), spriteMap[partname], changedSet.Normalized[part] + "_" + direction + dir);
                    continue;
                }

#if DEVELOPMENT
                    Debug.LogWarning("not found BUX part:" + partname);
#endif
            }
        }

        private void ClearPart(SpriteRenderer swapTarget)
        {
            if (swapTarget == null)
                return;

            var pref = swapTarget.transform.Find(S_FIND_PART);
            if (pref != null)
            {
                pref.name = DELETED_NAME;
                Object.Destroy(pref.gameObject);
            }

            swapTarget.sprite = null;
        }

        private void ClearSplitPart(string part, int direction)
        {
            var subpart = part.Substring(0, part.Length - 1); // e.g., "shoes" becomes "shoe".
            var directions = new[] { "_r", "_l" };            // Right and left identifiers.
            foreach (var dir in directions)
            {
                var partname = subpart + "_" + direction + dir; // e.g., "shoe_0_r"
                if (spriteMap.ContainsKey(partname))
                {
                    ClearPart(spriteMap[partname]);
                }
                else
                {
                    Debug.LogWarning("not found BUX part:" + partname);
                }
            }
        }

        private void SwapImage(SpriteRenderer swapTarget, string idToPath)
        {
            if (swapTarget == null)
                return;

            try
            {
                Texture2D t2d = (Texture2D)allCollection[(BASE_FOLDER + idToPath).ToLower()].Data;
                Sprite loadedSprite = t2d.ToSprite();
                swapTarget.sprite = loadedSprite;
            }
            catch
            {
                swapTarget.sprite = null;
            }
        }

        private void SwapPrefab(GameObject swapTarget, string idToPath)
        {
            if (swapTarget == null)
                return;

            try
            {
                var prefab = (GameObject)allCollection[(BASE_FOLDER + idToPath).ToLower()].Data;
                var avatarPart = factory.Create(prefab, swapTarget.transform);
                avatarPart.transform.localPosition = Vector3.zero;
                avatarPart.transform.localRotation = Quaternion.Euler(Vector3.zero);
                avatarPart.name = S_FIND_PART;
                layerWorker.SetLayerRecursively(avatarPart.gameObject, avatarPart.transform.parent.gameObject.layer);
            }
            catch
            {

            }
        }
    }
}