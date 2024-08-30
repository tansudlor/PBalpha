/*
 * ### ไฟล์: `AvatarSet` 

**Namespace**: `com.playbux.avatar`

**Class**: `AvatarSet` (implements `IAvatarSet`)

**Properties**:
- `IAvatarSet Normalized`: ปรับค่าใน `parts` ให้มีรูปแบบและข้อมูลที่ถูกต้องและคืนค่าเป็น `IAvatarSet` ใหม่

**Methods**:
- `Reform(string key, string input)`: ปรับปรุงค่า string ตามกฎเจาะจงและคืนค่า string ที่ได้จัดรูปแบบแล้ว
- `AvatarSet()`: Constructor ที่เรียก `AssignDefaultSet` เพื่อกำหนดค่าเริ่มต้น
- `AvatarSet(string json)`: Constructor ที่จัดการกับ JSON string และนำข้อมูลจากนั้นไปใส่ใน `parts` (หาก JSON ผิดพลาดหรือไม่มีข้อมูลจะกำหนดค่าเริ่มต้นใหม่)
- `AssignDefaultSet()`: กำหนดค่าเริ่มต้นให้กับ `parts` โดยกำหนดค่า `null` ให้กับส่วนต่างๆ ของ Avatar
- `ToJSON()`: แปลง `parts` ให้เป็น JSON string และคืนค่า
- `GetParts()`: คืนค่า keys จาก `parts` เป็น array
- `string this[string part] { get; set; }`: Indexer สำหรับเข้าถึงค่าใน `parts` และทำการตรวจสอบก่อนที่จะคืนค่า

**สรุป**: 
- `AvatarSet` เป็น class ที่ใช้จัดการกับข้อมูล Avatar ผ่าน `Dictionary<string, string> parts` โดยการจัดการกับข้อมูลเหล่านี้ให้เป็น JSON, ปรับปรุงข้อมูลให้เป็นค่ามาตรฐาน, และทำการจัดการกับส่วนต่างๆ ของ Avatar.
- `Reform` และ `Normalized` ใช้สำหรับการปรับปรุงและจัดรูปแบบข้อมูลใน `parts` ให้ถูกต้องตามความต้องการ.
- มี constructors ที่อนุญาตให้สร้าง `AvatarSet` ด้วยข้อมูล JSON หรือให้กำหนดค่าเริ่มต้นใหม่
- สามารถเข้าถึง และแปลงข้อมูลไปยัง/มาจาก JSON ได้
- สามารถดึงข้อมูลทั้งหมดจาก `parts` ได้
 */



using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace com.playbux.avatar
{

    public class AvatarSet : IAvatarSet
    {
        private Dictionary<string, string> parts;

        public IAvatarSet Normalized
        {
            get
            {
                // Get a list of all the keys
                AvatarSet res = new AvatarSet();


                foreach (string key in parts.Keys)
                {
                    //if value == null change to default
                    if (parts[key] == null)
                    {
                        res[key] = "default";
                    }
                    else
                    {
                        res[key] = parts[key];
                    }

                    //Collection Classify
                    if (res[key].IndexOf("_") > -1) /// 1_PBU  --> PBU/hat/1 , 2_PBU --> PBU/shoes/2
                    {
                        res[key] = Reform(key, res[key]);
                    }
                    else
                    if (res[key].IndexOf("_") == -1)/// 1  --> PBN/hat/1  , default ---> PBN/hat/default
                    {
                        res[key] = res[key] + "_1";
                        res[key] = Reform(key, res[key]);
                    }

                }
                return res;
            }
        }

        public string Payload { get ; set ; }
        public int Ticker { get; set; }

        private string Reform(string key, string input)
        {
            string[] parts = input.Split('_');
            if (parts.Length == 2)
            {
                string result = parts[1] + "/" + key + "/" + parts[0];
                return result;
            }

            return input;
        }

        public AvatarSet()
        {
            AssignDefaultSet();
        }


        public AvatarSet(string json)
        {
            parts = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (parts == null)
            {
                AssignDefaultSet();
            }
        }

        private void AssignDefaultSet()
        {
            parts = new Dictionary<string, string>();
            parts["hat"] = null;
            parts["face"] = null;
            parts["head"] = null;
            parts["shirt"] = null;
            parts["pants"] = null;
            parts["shoes"] = null;
            parts["back"] = null;
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(parts);
        }


        public string JSONForAPI()
        {
            // API  FORMAT--->  COLLECTION_ID   //API
            // GAME FORMAT--->  ID_COLLECTION   //AvartarSet
            AvatarSet export = new AvatarSet();
            var property = parts.Keys.ToArray();
            for (int i = 0; i < property.Length; i++)
            {
                string equip = parts[property[i]];

                if (equip?.IndexOf("default") >= 0)// DEFAULT 
                {
                    equip = null; 
                }

                string reform = equip;
                if (!string.IsNullOrEmpty(equip))
                {
                    string[] factor = equip.Split('_');
                    reform = factor[1] + "_" + factor[0];  // to API FORM -> COLLECTION_ID , 51_1 -> 1_51
                }
                export.parts[property[i]] = reform;
                
            }
            return JsonConvert.SerializeObject(export.parts);
        }

        public string[] GetParts()
        {
            return parts.Keys.ToArray();
        }

        public string this[string part]
        {
            get
            {
                return parts.TryGetValue(part, out var value) ? value : "default";
            }
            set
            {
                parts[part] = value;
            }
        }
    }
}
