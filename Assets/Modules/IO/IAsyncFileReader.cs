using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.playbux.io
{
    [Serializable]
    public struct FileInfo
    {
        public string Name => name;
        public string Path => path;
        public string Extension => extension;
        public string FullPath => $"{path}/{name}.{extension}";

        [SerializeField]
        private string name;

        [SerializeField]
        private string path;

        [SerializeField]
        private string extension;

        public FileInfo(string name, string path, string extension)
        {
            this.name = name;
            this.path = path;
            this.extension = extension;
        }
    }

    public interface IAsyncFileReader<T>
    {
        UniTask<T> Read(CancellationToken cancellationToken = default);
    }
}