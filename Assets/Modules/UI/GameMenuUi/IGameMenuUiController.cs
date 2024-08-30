
namespace com.playbux.ui.gamemenu
{
    public interface IGameMenuUiController
    {
#if !SERVER

        public void ShowGameMenu();
        public void PlayerChangeID();

#endif
    }
}
