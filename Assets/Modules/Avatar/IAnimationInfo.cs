/*
 * ### ไฟล์: `IAnimationInfo`

**Namespace**: `com.playbux.avatar`

**Interface**: `IAnimationInfo`

**Methods**:
- `float GetAnimationSpeed()`: คืนค่าความเร็วของ animation ในรูป float
- `PlayAction GetAnimationAction()`: คืนค่าการกระทำ (Action) ของ animation โดยสิ่งที่คืนค่ามาไม่ได้ถูกกำหนดไว้ในโค้ดนี้
- `ClipName GetAnimationName()`: คืนค่าชื่อของ animation clip โดยสิ่งที่คืนค่ามาไม่ได้ถูกกำหนดไว้ในโค้ดนี้

**สรุป**:
- `IAnimationInfo` ใช้เพื่อกำหนด method ที่ใช้ดึงข้อมูลที่เกี่ยวข้องกับ animation ของ avatar รวมถึง ความเร็ว, การเล่น, และชื่อของ animation clip.
 */

namespace com.playbux.avatar
{
    public interface IAnimationInfo
    {
        float GetAnimationSpeed();
        PlayAction GetAnimationAction();
        ClipName GetAnimationName();
    }
}