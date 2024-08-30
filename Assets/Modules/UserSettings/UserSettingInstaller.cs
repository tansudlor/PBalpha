using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace com.playbux.settings
{
    [CreateAssetMenu(menuName = "Playbux/Setting/UserSettingInstaller", fileName = "UserSettingInstaller")]
    public class UserSettingInstaller : ScriptableObjectInstaller<UserSettingInstaller>
    {
        [SerializeField]
        private TextAsset defaultSetting;
        //private SettingDataBase settingData;


        public override void InstallBindings()
        {
#if !SERVER
            BindClientSide();
#endif
        }

        private void BindClientSide()
        {

            SettingDataBase setting = JsonConvert.DeserializeObject<SettingDataBase>(defaultSetting.text);
            Container.Bind<SettingDataBase>().FromInstance(setting).AsSingle().NonLazy();


        }

    }
}