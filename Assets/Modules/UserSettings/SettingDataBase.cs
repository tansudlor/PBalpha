using UnityEngine;
using Newtonsoft.Json;
using System.IO;
namespace com.playbux.settings
{
    public interface ISettingDataBaseReader
    {
        SettingDataBase SettingDataBase { get; set; }
    }

    [SerializeField]
    public class SettingDataBase
    {
        [SerializeField]
        private uint screenSetting;
        [SerializeField]
        private AudioSettings audioSettings;
        [SerializeField]
        private DisplayNameSetting myNameDisplaySetting;
        [SerializeField]
        private DisplayNameSetting otherNameDisplaySetting;
        [SerializeField]
        private BubbleNumberSetting bubbleNumberSetting;

        public uint ScreenSetting { get => screenSetting; set => screenSetting = value; }
        public AudioSettings AudioSettings { get => audioSettings; set => audioSettings = value; }
        public DisplayNameSetting MyNameDisplaySetting { get => myNameDisplaySetting; set => myNameDisplaySetting = value; }
        public DisplayNameSetting OtherNameDisplaySetting { get => otherNameDisplaySetting; set => otherNameDisplaySetting = value; }
        public BubbleNumberSetting BubbleNumberSetting { get => bubbleNumberSetting; set => bubbleNumberSetting = value; }

        public static bool NotAllowWriteForThisType = true;
        public void SetBoardcast<T>(T dataname, object value)
        {
            this.GetType().GetProperty(dataname.ToString()).SetValue(this, value);   
        }

        public object GetData<T>(T dataname)
        {

            return this.GetType().GetProperty(dataname.ToString()).GetValue(this);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static void LoadSettingTo(ISettingDataBaseReader dataApply)
        {

            //var fileInfo = new FileInfo("SettingData", Application.persistentDataPath, "json");
            string fullPath = Path.Combine(Application.persistentDataPath, "SettingData.json");
            var json = File.ReadAllText(fullPath);
            dataApply.SettingDataBase = JsonConvert.DeserializeObject<SettingDataBase>(json);
           
        }

        public static void SaveSetting(SettingDataBase settingDataBase)
        {
            if (NotAllowWriteForThisType)
            {
                return;
            }
            string fullPath = Path.Combine(Application.persistentDataPath, "SettingData.json");
            File.WriteAllText(fullPath, JsonConvert.SerializeObject(settingDataBase));
            //IAsyncFileWriter<SettingDataBase> writer = new JSONFileWriter<SettingDataBase>(fileInfo);
            //await writer.Write(settingDataBase);

        }

    }

}
