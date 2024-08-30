using System.Threading;
using Cysharp.Threading.Tasks;
namespace com.playbux.io
{
    public interface IAsyncFileWriter<T>
    {
        UniTask<bool> Write(T data, CancellationToken cancellationToken = default);
    }
}