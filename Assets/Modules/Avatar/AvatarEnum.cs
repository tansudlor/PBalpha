/*
 * ### ไฟล์: `PlayAction`

**Namespace**: `com.playbux.avatar`

**Enum**: `PlayAction`

**Values**:
- `None`: ไม่มีการกระทำใดๆ
- `Pause`: หยุด animation
- `Play`: เล่น animation
- `Loop`: เล่น animation ในโหมดวนซ้ำ

**สรุป**: 
- `PlayAction` เป็น enum ที่ได้ถูกใช้เพื่อกำหนดค่าการกระทำที่เกี่ยวข้องกับการเล่น animation ในระบบ avatar โดยมี options เป็นการไม่ทำอะไร, หยุด, เล่น, และวนซ้ำ animation.
 */


namespace com.playbux.avatar
{
    public enum PlayAction
    {
        None,
        Pause,
        Play,
        Loop,
    }

}
