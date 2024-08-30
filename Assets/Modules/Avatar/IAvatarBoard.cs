/*
 * ### ไฟล์: `IAvatarBoard`

**Namespace**: `com.playbux.avatar`

**Interface**: `IAvatarBoard`

**Methods**:
- `void RegisterObserver(IAvatarBoardObserver observer)`: ลงทะเบียน observer เพื่อรับประกาศเมื่อมีการเปลี่ยนแปลงใน avatar board
- `void UnregisterObserver(IAvatarBoardObserver observer)`: ยกเลิกการลงทะเบียน observer จาก avatar board
- `void ChangePart(string playerId, EquipInfo assign)`: เปลี่ยนส่วนที่จะแสดงของ avatar โดยใช้ข้อมูลจาก `EquipInfo`
- `void UpdateAvatarSet(string playerId, IAvatarSet newAvatar)`: ปรับปรุง set ของ avatar สำหรับ player ที่ระบุ
- `void UpdateAvatarAnimation(string playerId, IAnimationInfo newAnimation)`: ปรับปรุงข้อมูล animation ของ avatar สำหรับ player ที่ระบุ
- `void UpdateAvatarDirection(string playerId, int newDirection)`: ปรับปรุงทิศทางของ avatar สำหรับ player ที่ระบุ
- `void UpdateAvatar(string playerId, IAvatarSet newAvatar, IAnimationInfo newAnimation)`: ปรับปรุงทั้ง set และ animation ของ avatar สำหรับ player ที่ระบุ

**สรุป**: 
- `IAvatarBoard` กำหนดตัวอย่างการใช้งาน (interface) สำหรับ "Avatar Board" ที่อนุญาตให้โปรแกรมกรณ์มีความสามารถในการทำงานเบื้องต้นที่เกี่ยวข้องกับการจัดการ avatars ทั้งหมดที่มีในบอร์ด รวมถึงการเปลี่ยนแปลง, ปรับปรุง, และการติดตามการเปลี่ยนแปลง (ผ่าน observer).
 */

using System;
using System.Collections.Generic;

namespace com.playbux.avatar
{
    public interface IAvatarBoard<TKey, TValue> : IReportable
    {
        public Action OnUpdateEquiped { get; set; }
        void RegisterObserver(IAvatarBoardObserver<TKey, TValue> observer);
        void UnregisterObserver(IAvatarBoardObserver<TKey, TValue> observer);

        //ref
        void ChangePart(TValue playerId, EquipInfo assign);
        void UpdateAvatarSet(TValue playerId, IAvatarSet newAvatar);
        void UpdateAvatarAnimation(TValue playerId, IAnimationInfo newAnimation);
        void UpdateAvatarDirection(TValue playerId, int newDirection);
        void UpdateAvatar(TValue playerId, IAvatarSet newAvatar, IAnimationInfo newAnimation);
        //val
        void ChangePart(TKey playerId, EquipInfo assign);
        void UpdateAvatarSet(TKey playerId, IAvatarSet newAvatar);
        void UpdateAvatarAnimation(TKey playerId, IAnimationInfo newAnimation);
        void UpdateAvatarDirection(TKey playerId, int newDirection);
        void UpdateAvatar(TKey playerId, IAvatarSet newAvatar, IAnimationInfo newAnimation);
    }
}
