/*
 * ### ไฟล์: `AvatarBoard`

**Namespace**: `com.playbux.avatar`

**Class**: `AvatarBoard`

**Implements**: `IAvatarBoard`

**Variables**:
- `playerAvatars`: จัดเก็บ `IAvatarSet` สำหรับทุก player ในบอร์ด
- `playerAnimations`: จัดเก็บ `IAnimationInfo` สำหรับทุก player ในบอร์ด
- `playerDirections`: จัดเก็บทิศทาง avatar ของทุก player ในบอร์ด
- `observers`: ลิสต์ของ `IAvatarBoardObserver` ที่จะได้รับแจ้งเมื่อมีการเปลี่ยนแปลงในบอร์ด

**Methods**:
1. `RegisterObserver(IAvatarBoardObserver observer)`: เพิ่ม observer เพื่อรับการแจ้งเมื่อ avatar board เปลี่ยนแปลง
2. `UnregisterObserver(IAvatarBoardObserver observer)`: ลบ observer จากระบบการแจ้งเตือน
3. `ChangePart(string playerId, EquipInfo assign)`: เปลี่ยนแปลงส่วนหนึ่งของ avatar ด้วย `EquipInfo`
4. `UpdateAvatarSet(string playerId, IAvatarSet newAvatar)`: ปรับปรุง avatar set ของ player
5. `UpdateAvatarAnimation(string playerId, IAnimationInfo newAnimation)`: ปรับปรุง animation ของ avatar ของ player
6. `UpdateAvatarDirection(string playerId, int newDirection)`: ปรับปรุงทิศทางของ avatar ของ player
7. `UpdateAvatar(string playerId, IAvatarSet newAvatar, IAnimationInfo newAnimation)`: ปรับปรุงทั้ง set และ animation ของ avatar ของ player
8. `NotifyObservers(string playerId, IAvatarSet newAvatar)`: แจ้ง observers หาก avatar ของ player ได้รับการอัปเดต
9. `NotifyObservers(string playerId, IAnimationInfo newAnimation)`: แจ้ง observers หาก animation ของ avatar ของ player ได้รับการอัปเดต
10. `NotifyObservers(string playerId, int newDirection)`: แจ้ง observers หากทิศทางของ avatar ของ player ได้รับการอัปเดต

**สรุป**:
- `AvatarBoard` คือ class ที่เกี่ยวกับการจัดการ avatars ใน "บอร์ด" โดยให้สามารถอัปเดตและติดตามการเปลี่ยนแปลงของ avatar สำหรับแต่ละ player ได้ โดยการใช้ observers.
- ยังมีเมธอดที่ใช้ในการแจ้งเตือน observers ทั้งหมดเมื่อมีการเปลี่ยนแปลงใด ๆ เกิดขึ้นภายใน class.
- ทำให้การจัดการ avatars และการติดตามการเปลี่ยนแปลงทำได้ง่ายและโปร่งใสมากขึ้น.
 */

using Zenject;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace com.playbux.avatar
{
    public abstract class AvatarBoard<TKey, TValue> : IAvatarBoard<TKey, TValue>
    {
        protected Dictionary<TKey, IAvatarSet> avatars;
        protected Dictionary<TKey, IAnimationInfo> animations;
        protected Dictionary<TKey, int> directions;
        protected List<IAvatarBoardObserver<TKey, TValue>> observers;
        private Action onUpdateEquiped;
        public Action OnUpdateEquiped { get => onUpdateEquiped; set => onUpdateEquiped = value; }
        protected abstract TKey GetId(TValue referance);
        public void Log(object log)
        {
#if DEVELOPMENT
            Debug.Log(log.ToString());
#endif
        }

        public AvatarBoard()
        {
            avatars = new Dictionary<TKey, IAvatarSet>();
            animations = new Dictionary<TKey, IAnimationInfo>();
            directions = new Dictionary<TKey, int>();
            observers = new List<IAvatarBoardObserver<TKey, TValue>>();
        }

        public virtual void RegisterObserver(IAvatarBoardObserver<TKey, TValue> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }


        public virtual void SetLocalAvatarLayer(TKey id, int Layer)
        {
            foreach (var observer in observers)
            {
                observer.OnLocalChangeLayer(id, Layer);
            }
        }

        public virtual void SetLocalAvatarLayer(TValue id, int Layer)
        {
            foreach (var observer in observers)
            {
                observer.OnLocalChangeLayer(id, Layer);
            }
        }

        public virtual void UnregisterObserver(IAvatarBoardObserver<TKey, TValue> observer)
        {
            if (observers.Contains(observer))
                observers.Remove(observer);
        }

        public virtual void ChangePart(TValue playerId, EquipInfo assign)
        {
            if (string.IsNullOrEmpty(GetId(playerId)?.ToString()))
            {
                Log("Player ID should not be null or empty.");
                return;
            }

            if (!avatars.ContainsKey(GetId(playerId)))
            {
                avatars[GetId(playerId)] = new AvatarSet();
            }

            if (avatars[GetId(playerId)] == null)
            {
                Log($"Invalid part name: {assign.WareAt}");
                return;
            }

            avatars[GetId(playerId)][assign.WareAt.ToLower()] = assign.Id;
            NotifyObservers(playerId, avatars[GetId(playerId)]);
        }

        public virtual void UpdateAvatarSet(TValue playerId, IAvatarSet newAvatar)
        {
            if (string.IsNullOrEmpty(GetId(playerId)?.ToString()))
            {
                Log("Player ID should not be null or empty.");
                return;
            }
            avatars[GetId(playerId)] = newAvatar;
            NotifyObservers(playerId, newAvatar);
        }


        public virtual void UpdateAvatarAnimation(TValue playerId, IAnimationInfo newAnimation)
        {
            if (string.IsNullOrEmpty(GetId(playerId)?.ToString()))
            {
                Log("Player ID should not be null or empty.");
                return;
            }
            animations[GetId(playerId)] = newAnimation;
            NotifyObservers(playerId, newAnimation);

        }

        public virtual void UpdateAvatarDirection(TValue playerId, int newDirection)
        {
            if (string.IsNullOrEmpty(GetId(playerId)?.ToString()))
            {
                Log("Player ID should not be null or empty.");
                return;
            }

            if (!directions.ContainsKey(GetId(playerId)))
            {
                directions[GetId(playerId)] = newDirection;
                NotifyObservers(playerId, newDirection);
                return;
            }

            if (directions[GetId(playerId)] != newDirection)
            {
                directions[GetId(playerId)] = newDirection;
                NotifyObservers(playerId, newDirection);
            }
        }

        public virtual void UpdateAvatar(TValue playerId, IAvatarSet newAvatar = null, IAnimationInfo newAnimation = null)
        {
            if (string.IsNullOrEmpty(GetId(playerId)?.ToString()))
            {
                Log("Player ID should not be null or empty.");
                return;
            }

            if (newAvatar != null)
            {
                avatars[GetId(playerId)] = newAvatar;
                NotifyObservers(playerId, newAvatar);
            }

            if (newAnimation != null)
            {
                animations[GetId(playerId)] = newAnimation;
                NotifyObservers(playerId, newAnimation);
            }
        }

        public virtual void NotifyObservers(TValue playerId, IAvatarSet newAvatar)
        {
            foreach (var observer in observers)
            {
                observer.OnAvatarChanged(playerId, newAvatar);
            }
        }

        public virtual void NotifyObservers(TValue playerId, IAnimationInfo newAnimation)
        {
            foreach (var observer in observers)
            {
                observer.OnAnimationChanged(playerId, newAnimation);
            }
        }

        public virtual void NotifyObservers(TValue playerId, int newDirection)
        {
            foreach (var observer in observers)
            {
                observer.OnDirectionChanged(playerId, newDirection);
            }
        }

        public virtual void ChangePart(TKey playerId, EquipInfo assign)
        {
            if (string.IsNullOrEmpty(playerId?.ToString()))
            {
                Log("Player ID should not be null or empty.");
                return;
            }

            if (!avatars.ContainsKey(playerId))
            {
                avatars[playerId] = new AvatarSet();
            }

            if (avatars[playerId] == null)
            {
                Log($"Invalid part name: {assign.WareAt}");
                return;
            }
            avatars[playerId][assign.WareAt.ToLower()] = assign.Id;
            NotifyObservers(playerId, avatars[playerId]);
        }




        public virtual void UpdateAvatarSet(TKey playerId, IAvatarSet newAvatar)
        {
            if (string.IsNullOrEmpty(playerId?.ToString()))
            {
                Log("Player ID should not be null or empty.");
                return;
            }
            avatars[playerId] = newAvatar;
            NotifyObservers(playerId, newAvatar);
        }


        public virtual void UpdateAvatarAnimation(TKey playerId, IAnimationInfo newAnimation)
        {
            if (string.IsNullOrEmpty(playerId?.ToString()))
            {
                Log("Player ID should not be null or empty.");
                return;
            }
            animations[playerId] = newAnimation;
            NotifyObservers(playerId, newAnimation);

        }

        public virtual void UpdateAvatarDirection(TKey playerId, int newDirection)
        {
            if (string.IsNullOrEmpty(playerId?.ToString()))
            {
                Log("Player ID should not be null or empty.");
                return;
            }

            if (!directions.ContainsKey(playerId))
            {
                directions[playerId] = newDirection;
                NotifyObservers(playerId, newDirection);
                return;
            }

            if (directions[playerId] != newDirection)
            {
                directions[playerId] = newDirection;
                NotifyObservers(playerId, newDirection);
            }
        }

        public virtual void UpdateAvatar(TKey playerId, IAvatarSet newAvatar = null, IAnimationInfo newAnimation = null)
        {
            if (string.IsNullOrEmpty(playerId?.ToString()))
            {
                Log("Player ID should not be null or empty.");
                return;
            }

            if (newAvatar != null)
            {
                avatars[playerId] = newAvatar;
                NotifyObservers(playerId, newAvatar);
            }

            if (newAnimation != null)
            {
                animations[playerId] = newAnimation;
                NotifyObservers(playerId, newAnimation);
            }
        }

        public virtual void NotifyObservers(TKey playerId, IAvatarSet newAvatar)
        {
            foreach (var observer in observers)
            {
                observer.OnAvatarChanged(playerId, newAvatar);
            }
        }

        public virtual void NotifyObservers(TKey playerId, IAnimationInfo newAnimation)
        {
            foreach (var observer in observers)
            {
                observer.OnAnimationChanged(playerId, newAnimation);
            }
        }

        public virtual void NotifyObservers(TKey playerId, int newDirection)
        {
            foreach (var observer in observers)
            {
                observer.OnDirectionChanged(playerId, newDirection);
            }
        }
    }
}