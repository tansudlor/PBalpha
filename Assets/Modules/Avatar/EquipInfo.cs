/*
 * ### ไฟล์: `EquipInfo`

**Namespace**: `com.playbux.avatar`

**Class**: `EquipInfo`

**Properties**:
- `string WareAt`: ที่ตั้งหรือสถานที่ที่อุปกรณ์ถูกนำไปใช้งาน
- `string Id`: รหัสประจำอุปกรณ์

**Constructors**:
- `EquipInfo(string wareAt, string id)`: รับข้อมูลที่ตั้งและรหัสประจำอุปกรณ์เพื่อตั้งค่าให้กับ properties

**สรุป**: 
- `EquipInfo` ใช้เพื่อจัดเก็บข้อมูลของอุปกรณ์ หรือ items ที่จะถูกใช้งาน/ประดับในระบบ avatar, โดยจัดเก็บข้อมูลเกี่ยวกับที่ตั้ง (หรือบริเวณที่สามารถใส่อุปกรณ์ได้) และรหัสประจำอุปกรณ์นั้นๆ.
 */
using System;

namespace com.playbux.avatar
{
    public struct EquipInfo : IEquatable<EquipInfo>
    {
        public EquipInfo(string wareAt, string id)
        {
            this.WareAt = wareAt;
            this.Id = id;
        }

        public string WareAt;
        public string Id;

        public bool Equals(EquipInfo other)
        {
            return other.WareAt == WareAt && other.Id == Id;
        }

        public override bool Equals(object obj)
        {
            return obj is EquipInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(WareAt, Id);
        }
    }
}
