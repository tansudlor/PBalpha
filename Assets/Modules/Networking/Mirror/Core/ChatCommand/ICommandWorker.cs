namespace com.playbux.networking.mirror.core
{
    public interface ICommandWorker
    {
        CommandInstruction Instruction { get; }

        void Perform(int connectionId, params string[] parameters);
    }
}