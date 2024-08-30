namespace com.playbux.io
{
    public interface IFileWriter<T>
    {
        bool Write(T data);
    }
}