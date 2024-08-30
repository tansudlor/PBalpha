/*
 * ### ไฟล์: `ClipName`

**Namespace**: `com.playbux.avatar`

**Enum**: `ClipName`

**Values**:
- `Idle`: สถานะไม่ทำอะไร/ยืนหยุด
- `Walk`: สถานะกำลังเดิน

**สรุป**: 
- `ClipName` เป็น enum ที่ได้ถูกใช้เพื่อกำหนดชื่อของ animation clips ในระบบ avatar โดยมี options เป็นการอยู่ในสถานะว่างเปล่า (Idle) และการเดิน (Walk).
 */


using System.Collections.Generic;
namespace com.playbux.avatar
{
    public enum ClipName
    {
        Idle,
        Walk,
    }
}
