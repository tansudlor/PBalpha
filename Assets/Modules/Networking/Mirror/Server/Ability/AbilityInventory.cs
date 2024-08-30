using Zenject;
using com.playbux.io;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;

namespace com.playbux.networking.server.ability
{
    public class AbilityInventory : IInitializable, ILateDisposable
    {
        private readonly IAsyncFileWriter<SerializedDictionary<string, uint[]>> dataWriter;
        private readonly IAsyncFileReader<SerializedDictionary<string, uint[]>> dataReader;

        private Dictionary<string, uint[]> data;

        public AbilityInventory(IOFacade<SerializedDictionary<string, uint[]>> ioFacade)
        {
            dataWriter = ioFacade.Writer;
            dataReader = ioFacade.Reader;
        }

        public void Initialize()
        {
            Read().Forget();
        }

        public void LateDispose()
        {
            Write().Forget();
        }

        public void Add(string uuid, uint abilityId)
        {
            HashSet<uint> abilityIds = new HashSet<uint>();

            if (!data.ContainsKey(uuid))
                data.Add(uuid, abilityIds.ToArray());

            abilityIds = data[uuid].ToHashSet();
            abilityIds.Add(abilityId);
            data[uuid] = abilityIds.ToArray();
            Write().Forget();
        }

        private async UniTaskVoid Read()
        {
            var abilityInventoryData = await dataReader.Read();
            abilityInventoryData ??= new SerializedDictionary<string, uint[]>();
            data = abilityInventoryData;
            Write().Forget();
        }

        private async UniTaskVoid Write()
        {
            var serializedDict = new SerializedDictionary<string, uint[]>();

            foreach (var keypair in data)
                serializedDict.Add(keypair.Key, keypair.Value);

            await dataWriter.Write(serializedDict);
        }
    }
}