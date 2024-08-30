namespace com.playbux.identity
{
    public interface IIdentityObserver
    {
        void OnUpdateProfile(IdentityDetail detail);
    }
}