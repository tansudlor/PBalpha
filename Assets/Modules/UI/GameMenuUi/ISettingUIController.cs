using com.playbux.settings;

namespace com.playbux.ui.gamemenu
{
    public interface ISettingUIController
    {
        public void OpenSetting();
        public void CloseSetting();
        SettingDataBase SettingDataBase { get; set; }
    }
}
