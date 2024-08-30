using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace com.playbux.io
{
    public class JSONFileWriter<T> : IAsyncFileWriter<T>
    {
        private readonly FileInfo fileInfo;

        public JSONFileWriter(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        public async UniTask<bool> Write(T data, CancellationToken cancellationToken = default)
        {
            bool hasDirectory = Directory.Exists(fileInfo.Path);

            if (!hasDirectory)
            {
                try
                {
                    Directory.CreateDirectory(fileInfo.Path);
                }
                catch (Exception exception)
                {
#if DEVELOPMENT
                    Debug.Log(exception.Message);
#endif
                    return false;
                }
            }

            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

#if DEVELOPMENT
                Debug.Log($"Serialized: {json}\n{fileInfo.Path}");
#endif
                await File.WriteAllTextAsync(fileInfo.FullPath, json, cancellationToken).AsUniTask();

                return !cancellationToken.IsCancellationRequested;

            }
            catch (Exception exception)
            {
#if DEVELOPMENT
                Debug.Log(exception.Message);
#endif

                return false;
            }
        }
    }
}