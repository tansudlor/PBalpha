namespace com.playbux.networking.mirror.collision
{
    public interface ITrigger<T>
    {
        bool IsTrigger(T other);
        void SetActive(bool enabled);
    }
}