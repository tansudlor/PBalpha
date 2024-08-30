using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace com.playbux.io
{
    public class JSONFileReader<T> : IAsyncFileReader<T>
    {
        private readonly FileInfo fileInfo;

        public JSONFileReader(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        public async UniTask<T> Read(CancellationToken cancellationToken = default)
        {
            T obj = default;

            if (!Directory.Exists(fileInfo.FullPath))
                return obj;

            string json = await File.ReadAllTextAsync(fileInfo.FullPath, cancellationToken).AsUniTask();

            if (cancellationToken.IsCancellationRequested)
                return obj;

#if DEVELOPMENT
            Debug.Log($"Read JSON data from {fileInfo.FullPath} :{json}");
#endif

            obj = JsonConvert.DeserializeObject<T>(json);

            return obj;
        }
    }

}