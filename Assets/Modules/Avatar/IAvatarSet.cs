/*
Namespace: com.playbux.avatar

Interface: IAvatarSet

Properties & Methods:

string this[string part] { get; set; }: Indexer property ให้เราเข้าถึงหรือเปลี่ยนแปลงค่าบางประการของ Avatar โดยใช้ string เป็น key
string[] GetParts(): ดึงข้อมูลของส่วนต่างๆ ของ Avatar
string ToJSON(): แปลงและส่งข้อมูลของ Avatar ในรูปแบบ JSON string
IAvatarSet Normalized { get; }: ให้ IAvatarSet ที่ถูกปรับให้เป็นมาตรฐาน (Normalized)
สรุป: IAvatarSet จัดการข้อมูลหรือเปลี่ยนแปลงข้อมูลของ Avatar ในรูปแบบหรือส่วนต่างๆ โดยมีเมธอดสำหรับแปลงข้อมูลเป็น JSON และจัดการกับส่วนประกอบต่างๆ ของ Avatar
 */
namespace com.playbux.avatar
{
    public interface IAvatarSet
    {
        string this[string part] { get; set; }
        string[] GetParts();
        string ToJSON();
        string JSONForAPI();
        public IAvatarSet Normalized { get; }
        public string Payload { get; set; }
        public int Ticker { get; set; }
    }
}
