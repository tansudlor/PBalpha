namespace com.playbux.io
{
    public class IOFacade<T>
    {
        public IAsyncFileReader<T> Reader { get; }
        public IAsyncFileWriter<T> Writer { get; }

        public IOFacade(IAsyncFileReader<T> reader, IAsyncFileWriter<T> writer)
        {
            Reader = reader;
            Writer = writer;
        }
    }
}
