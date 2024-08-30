/*
ไฟล์โค้ด C# ที่คุณให้มามีคอนเทนต์ดังนี้:

Namespace: com.playbux.avatar

Interface: IAvatarBoardObserver

Methods:

OnDirectionChanged(string playerId, int newDirection): ใช้เพื่อรับข้อมูลและ/หรืออัปเดตเมื่อทิศทางของ Avatar มีการเปลี่ยนแปลง
OnAvatarChanged(string playerId, IAvatarSet newAvatar): ใช้เพื่อรับข้อมูลและ/หรืออัปเดตเมื่อ Avatar มีการเปลี่ยนแปลง
OnAnimationChanged(string playerId, IAnimationInfo newAnimation): ใช้เพื่อรับข้อมูลและ/หรืออัปเดตเมื่อ Animation ของ Avatar มีการเปลี่ยนแปลง
สรุป: Interface IAvatarBoardObserver ใช้เพื่อสังเกตการณ์ (observe) และ ตอบสนอง (respond) ต่อการเปลี่ยนแปลงที่เกิดขึ้นกับ avatar ในกระดาน (board) 
ทั้งเรื่องทิศทางการหัน, avatar ที่ถูกใช้งาน, และ animation ที่ถูกเล่น ในคอนเทกซต์นี้, "playerId" ใช้เพื่อระบุ avatar ที่ทำการเปลี่ยนแปลง, และพารามิเตอร์ที่เหลือใช้เพื่อขนส่งข้อมูลที่เกี่ยวข้องกับการเปลี่ยนแปลงนั้นๆ
*/

namespace com.playbux.avatar
{
    public interface IAvatarBoardObserver<TKey, TValue>
    {
        void OnDirectionChanged(TValue playerId, int newDirection);
        void OnAvatarChanged(TValue playerId, IAvatarSet newAvatar);
        void OnAnimationChanged(TValue playerId, IAnimationInfo newAnimation);
        void OnLocalChangeLayer(TValue playerId, int Layar);
        void OnDirectionChanged(TKey playerId, int newDirection);
        void OnAvatarChanged(TKey playerId, IAvatarSet newAvatar);
        void OnAnimationChanged(TKey playerId, IAnimationInfo newAnimation);
        void OnLocalChangeLayer(TKey playerId, int Layar);
    }
}
