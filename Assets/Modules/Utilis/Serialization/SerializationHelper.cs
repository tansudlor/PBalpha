using UnityEngine;
using System.Runtime.InteropServices;

namespace com.playbux.utilis.serialization
{
    public static class SerializationHelper
    {
        public static int GetDataSize(this object input)
        {
            return Marshal.SizeOf(input);
        }

        public static byte[] ToBytes(this object input, int dataSize)
        {
            var output = new byte[dataSize];
            var ptr = Marshal.AllocHGlobal(dataSize);
            Marshal.StructureToPtr(input, ptr, false);
            Marshal.Copy(ptr, output, 0, dataSize);
            Marshal.FreeHGlobal(ptr);
            return output;
        }

        public static T FromBytes<T>(this byte[] bytes, int dataSize)
        {
            var ptr = Marshal.AllocHGlobal(dataSize);
            Marshal.Copy(bytes, 0, ptr, dataSize);
            var output = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return output;
        }
    }
}