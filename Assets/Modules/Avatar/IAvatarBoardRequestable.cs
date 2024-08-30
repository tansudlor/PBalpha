
namespace com.playbux.avatar
{
    public interface IAvatarBoardRequestable<T, R, CONNECTION>
    {
        IAvatarSet GetAvatarSet(T playerId, CONNECTION connection, bool updateSet);
        IAvatarSet GetAvatarSet(R playerId, CONNECTION connection, bool updateSet);
    }
}
