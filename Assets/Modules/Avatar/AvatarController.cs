/*
 * ตัว `AvatarController` ใน Unity จัดการภาพ, ทิศทาง, และอนิเมชันของตัวละครหรือ "Avatar" ภายในเกม โดยพิจารณาการเปลี่ยนแปลงที่เกิดขึ้นจาก `IAvatarBoardObserver`. ด้านล่างนี้คือคำอธิบายและประเด็นหลักๆ:

### โครงสร้างหลักของ `AvatarController`:
- **การตั้งค่า (SetUp)**
    - เมื่อ Avatar ถูกสร้าง, `AvatarController` ได้รับ `avatarId`, `board`, `animator`, `directions`, และ `collections` โดยใช้ Dependency Injection ผ่านเมทอด `SetUp`.
    - `collectionSwapper` เป็น Dictionary ที่ใช้จัดการวิธีการเปลี่ยนส่วนประกอบของ Avatar ตามประเภทของส่วนประกอบนั้นๆ (เช่น ภาพหรือ Prefab).
    - `avatarID`, `board`, `animator`, และ `directions` ถูกกำหนดค่าเบื้องต้น.
    - มันจะสร้าง `spriteMap` จาก SpriteRenderers ที่เชื่อมโยงกับ GameObject.

- **การตรวจจับการเปลี่ยนแปลง**
    - เมทอด `OnAvatarChanged` ตรวจสอบการเปลี่ยนแปลงในส่วนประกอบของ Avatar และอัพเดต SpriteRenderers ตามที่จำเป็น.
    - `OnDirectionChanged` ตรวจสอบการเปลี่ยนแปลงทิศทางของ Avatar และเปลี่ยน Animator ที่ใช้ตามทิศทางใหม่.
    - `OnAnimationChanged` ตรวจสอบการเปลี่ยนแปลงในอนิเมชันของ Avatar และปรับปรุง Animator ตามการกระทำที่ได้รับ.

- **การเปลี่ยนส่วนประกอบของ Avatar**
    - `SwapImage` และ `SwapPrefab` คือเมทอดที่ใช้เปลี่ยนหรืออัพเดต Sprite หรือ Prefab ของส่วนประกอบต่างๆ ของ Avatar.

### การใช้งาน `AvatarController`:
1. **การตั้งค่า (Initialization):**
   - ใช้ `SetUp` เพื่อตั้งค่าเบื้องต้น, อาจต้องมีการทำ Dependency Injection เพื่อให้ทราบถึง avatarId, board, และค่าอื่นๆ ที่จำเป็น.

2. **การตรวจจับและการตอบสนองต่อการเปลี่ยนแปลง:**
   - ผ่าน `IAvatarBoardObserver`, `AvatarController` จะถูกแจ้งเตือนเมื่อมีการเปลี่ยนแปลงใดๆ ใน Avatar และดำเนินการตามที่จำเป็น.

3. **การจัดการทิศทางและอนิเมชัน:**
   - เมทอดเหล่านี้จะจัดการการ

เปลี่ยนอนิเมชันและทิศทางของ Avatar ตามที่ได้รับจาก IAvatarBoard.

### สรุป
`AvatarController` เป็น Class ที่ใช้จัดการละเอียดทั้งหมดที่เกี่ยวข้องกับการแสดง Avatar ใน Unity, โดยรับการตั้งค่าเบื้องต้นจาก `SetUp` และตรวจจับ/ตอบสนองต่อการเปลี่ยนแปลงใน Avatar ผ่าน `IAvatarBoardObserver`.

 */



#if UNITY_EDITOR
using System.IO;
#endif
using Zenject;
using UnityEngine;
using System.Linq;
using com.playbux.bux;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using Animator = UnityEngine.Animator;
using com.playbux.utilis.extension;

namespace com.playbux.avatar
{
    public abstract class AvatarController<T, R> : MonoBehaviour, IAvatarBoardObserver<T, R>, IReportable
    {
        private delegate void PerformSwap(string part, Object swapTarget, string idToPath);
        public R AvatarId => avatarId;

        public List<string> IgnoreLayerList { get => ignoreLayerList; set => ignoreLayerList = value; }

        [SerializeField]
        private Transform target;
        private R avatarId;
        private IAvatarBoard<T, R> board;
        private Dictionary<string, SpriteRenderer> spriteMap;
        private Animator[] directions;
        private IAnimator animator;
        private Dictionary<string, PartPathMaping> allCollection;
        private Dictionary<string, PerformSwap> collectionSwapper;
        private int currentDirection;
        private AvatarPartsCollection[] collection;
        private string BaseFolder;
        private List<string> ignoreLayerList = new List<string>();
#if UNITY_EDITOR && !SERVER
        public List<DisplayName> debugAllCollectionMap;
#endif
        protected abstract T GetId(R refereance);

        [Inject]
        public void SetUp(R avatarId, IAvatarBoard<T, R> board, IAnimator animator, Transform target, Animator[] directions, AvatarPartsCollection[] collectons)
        {
#if !SERVER
            this.target = target;
            collectionSwapper = new Dictionary<string, PerformSwap>
            {
                {"1",SwapImage},
                {"2",SwapPrefab}
            };
            this.BaseFolder = "Assets/Modules/Avatar/Bux Parts/";
            this.avatarId = avatarId;
            this.board = board;
            this.board.RegisterObserver(this);
            this.animator = animator;
            this.directions = directions;
            this.currentDirection = 0;
            //build map
            SpriteRenderer[] spriteRenderers = target.GetComponentsInChildren<SpriteRenderer>(true);
            spriteMap = new Dictionary<string, SpriteRenderer>();
            foreach (var renderer in spriteRenderers)
            {
                spriteMap[renderer.name.ToLower()] = renderer;
            }

            allCollection = new Dictionary<string, PartPathMaping>();
            allCollection = collectons.SelectMany(avatarPartCollection => avatarPartCollection.Map)
             .ToDictionary(pair => pair.Key, pair => pair.Value);
#endif
#if UNITY_EDITOR && !SERVER
            debugAllCollectionMap = new List<DisplayName>();
            foreach (var item in allCollection.Keys)
            {
                debugAllCollectionMap.Add(new DisplayName(allCollection[item].Path));
            }
#endif
        }

        /*
       * NOTE:
       * This function updates the avatar when it's changed. If a certain part doesn't exist in the spriteMap:
       * - It checks if the missing part is a split item (like 'shoes' splitting into 'shoe_left' and 'shoe_right').
       * - If it's a split part, it searches for the corresponding left and right sprites in the spriteMap.
       * - If found, it updates the image. Otherwise, a warning is displayed.
       */
        public virtual void OnAvatarChanged(R playerId, IAvatarSet newAvatar)
        {
            // Exit if provided player ID doesn't match (e.g., "player123" != "player456").
            if (!GetId(playerId).Equals(GetId(avatarId)))
                return;

            // Retrieve all avatar parts (e.g., ["hat", "shoes", "shirt"]).
            var allParts = newAvatar.GetParts();
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
                        ClearSplitPart(part, d, newAvatar);
                    }
                    else
                    {
                        ClearPart(part, spriteMap[keyBase], newAvatar.Normalized[part] + "_" + d);
                    }
                }
                // Check the part for all 4 directions (0, 1, 2, 3).
                for (int d = 0; d < 4; d++)
                {
                    string keyBase = part + "_" + d; // e.g., "hat_0"
                    if (!spriteMap.ContainsKey(keyBase))
                    {
                        UpdateSplitPart(part, d, newAvatar);
                    }
                    else
                    {
                        collectionSwapper[NFTCollectionClassify(newAvatar.Normalized[part])](part, spriteMap[keyBase], newAvatar.Normalized[part] + "_" + d);
                    }
                }
            }
        }


        protected virtual void UpdateSplitPart(string part, int direction, IAvatarSet newAvatar)
        {
            var subpart = part.Substring(0, part.Length - 1); // e.g., "shoes" becomes "shoe".
            var directions = new[] { "_r", "_l" }; // Right and left identifiers.
            foreach (var dir in directions)
            {
                var partname = subpart + "_" + direction + dir; // e.g., "shoe_0_r"
                if (spriteMap.ContainsKey(partname))
                {
                    collectionSwapper[NFTCollectionClassify(newAvatar.Normalized[part])](part, spriteMap[partname], newAvatar.Normalized[part] + "_" + direction + dir);

                    //   (part, spriteMap[partname], newAvatar[part] + "_" + (direction + 1) + dir);
                }
                else
                {
                    Debug.LogWarning("not found BUX part:" + partname);
                }
            }
        }



        protected virtual void ClearSplitPart(string part, int direction, IAvatarSet newAvatar)
        {
            var subpart = part.Substring(0, part.Length - 1); // e.g., "shoes" becomes "shoe".
            var directions = new[] { "_r", "_l" }; // Right and left identifiers.
            foreach (var dir in directions)
            {
                var partname = subpart + "_" + direction + dir; // e.g., "shoe_0_r"
                if (spriteMap.ContainsKey(partname))
                {
                    ClearPart(part, spriteMap[partname], newAvatar.Normalized[part] + "_" + direction + dir);
                }
                else
                {
                    Debug.LogWarning("not found BUX part:" + partname);
                }
            }
        }


        protected virtual void ClearPart(string part, Object swapTarget, string idToPath)
        {
            if (swapTarget == null)
                return;
            var pref = ((SpriteRenderer)swapTarget).transform.Find("Prefab_Avatar_Part");
            if (pref != null)
            {
                pref.name = "Deleted";
                Destroy(pref.gameObject);
            }
             ((SpriteRenderer)swapTarget).sprite = null;
        }

        protected virtual string NFTCollectionClassify(string normalizedPart)
        {
            return normalizedPart.Split('/')[0].ToLower();
        }

        protected virtual void SwapImage(string part, Object swapTarget, string idToPath)
        {
            if (swapTarget == null)
                return;
            try
            {
                Texture2D t2d = (Texture2D)allCollection[(BaseFolder + idToPath).ToLower()].Data;
                Sprite loadedSprite = t2d.ToSprite();
                ((SpriteRenderer)swapTarget).sprite = loadedSprite;
            }
            catch
            {
                ((SpriteRenderer)swapTarget).sprite = null;
            }
        }



        protected virtual void SwapPrefab(string part, Object swapTarget, string idToPath)
        {
            if (swapTarget == null)
                return;

            try
            {
                GameObject prefab = (GameObject)allCollection[(BaseFolder + idToPath).ToLower()].Data;
                GameObject avatarPart = GameObject.Instantiate(prefab);
                avatarPart.transform.parent = ((SpriteRenderer)swapTarget).transform;
                avatarPart.transform.localPosition = Vector3.zero;
                avatarPart.transform.localRotation = Quaternion.Euler(Vector3.zero);
                avatarPart.name = "Prefab_Avatar_Part";
                SetLayerRecursively(avatarPart, avatarPart.transform.parent.gameObject.layer);
            }catch
            {

            }

        }

        public virtual void OnDirectionChanged(R playerId, int newDirection)
        {
            if (!GetId(playerId).Equals(GetId(avatarId)))
                return;

            //Deactive All children
            for (int i = 0; i < directions.Length; i++)
            {
                directions[i].gameObject.SetActive(false);
            }

            //Active Direction
            directions[newDirection % 4].gameObject.SetActive(true);
            if ((currentDirection % 4) != (newDirection % 4))
            {
                animator.ClearCurrentAnimationName();
            }

            currentDirection = newDirection;
        }

        public virtual void OnAnimationChanged(R playerId, IAnimationInfo newAnimation)
        {
            if (!GetId(playerId).Equals(GetId(avatarId)))
                return;

            animator.Speed = newAnimation.GetAnimationSpeed();
            if (newAnimation.GetAnimationAction() == PlayAction.Loop)
            {
                animator.Play(newAnimation.GetAnimationName().ToString(), true);
            }
            else
            if (newAnimation.GetAnimationAction() == PlayAction.Play)
            {
                animator.Play(newAnimation.GetAnimationName().ToString(), false);
            }
            else
            if (newAnimation.GetAnimationAction() == PlayAction.Pause)
            {
                animator.Stop(newAnimation.GetAnimationName().ToString());
            }
            else
            if (newAnimation.GetAnimationAction() == PlayAction.None)
            {
                animator.Play(newAnimation.GetAnimationName().ToString());
            }

            AdditionalAnimationChanged(newAnimation);
        }

        protected abstract void AdditionalAnimationChanged(IAnimationInfo newAnimation);


        public virtual void OnDirectionChanged(T playerId, int newDirection)
        {
            if (!playerId.Equals(GetId(avatarId)))
                return;

            //Deactive All children
            for (int i = 0; i < directions.Length; i++)
            {
                directions[i].gameObject.SetActive(false);
            }

            //Active Direction
            directions[newDirection % 4].gameObject.SetActive(true);
            if ((currentDirection % 4) != (newDirection % 4))
            {
                animator.ClearCurrentAnimationName();
            }
            currentDirection = newDirection;
        }

        public virtual void OnAvatarChanged(T playerId, IAvatarSet newAvatar)
        {
            // Exit if provided player ID doesn't match (e.g., "player123" != "player456").
            if (!playerId.Equals(GetId(avatarId)))
                return;

            // Retrieve all avatar parts (e.g., ["hat", "shoes", "shirt"]).
            var allParts = newAvatar.GetParts();
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
                        ClearSplitPart(part, d, newAvatar);
                    }
                    else
                    {
                        ClearPart(part, spriteMap[keyBase], newAvatar.Normalized[part] + "_" + d);
                    }
                }
                // Check the part for all 4 directions (0, 1, 2, 3).
                for (int d = 0; d < 4; d++)
                {
                    string keyBase = part + "_" + d; // e.g., "hat_0"
                    if (!spriteMap.ContainsKey(keyBase))
                    {
                        UpdateSplitPart(part, d, newAvatar);
                    }
                    else
                    {
                        collectionSwapper[NFTCollectionClassify(newAvatar.Normalized[part])](part, spriteMap[keyBase], newAvatar.Normalized[part] + "_" + d);
                    }
                }
            }
        }

        public virtual void OnAnimationChanged(T playerId, IAnimationInfo newAnimation)
        {
            if (!playerId.Equals(GetId(avatarId)))
                return;

            animator.Speed = newAnimation.GetAnimationSpeed();
            if (newAnimation.GetAnimationAction() == PlayAction.Loop)
            {
                animator.Play(newAnimation.GetAnimationName().ToString(), true);
            }
            else
            if (newAnimation.GetAnimationAction() == PlayAction.Play)
            {
                animator.Play(newAnimation.GetAnimationName().ToString(), false);
            }
            else
            if (newAnimation.GetAnimationAction() == PlayAction.Pause)
            {
                animator.Stop(newAnimation.GetAnimationName().ToString());
            }
            else
            if (newAnimation.GetAnimationAction() == PlayAction.None)
            {
                animator.Play(newAnimation.GetAnimationName().ToString());
            }

            AdditionalAnimationChanged(newAnimation);
        }




        public virtual void OnLocalChangeLayer(R id, int newLayer)
        {
            if (!GetId(id).Equals(GetId(avatarId)))
                return;

            SetLayerRecursively(gameObject, newLayer);
        }

        public virtual void OnLocalChangeLayer(T id, int newLayer)
        {
            if (!id.Equals(GetId(avatarId)))
                return;
            SetLayerRecursively(gameObject, newLayer);
        }

        // Recursive method to set the layer of a GameObject and all its children
        protected virtual void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null)
                return;

            foreach (string name in IgnoreLayerList) {
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

                SetLayerRecursively(child.gameObject, newLayer );
            }
        }

        public void Log(object log)
        {
#if DEVELOPMENT
            Debug.Log(log.ToString());
#endif
        }
    }

}