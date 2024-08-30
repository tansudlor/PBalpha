using System;
using System.Runtime.InteropServices;
namespace com.playbux.networking.mirror.core
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InputValue
    {
        public VerticalInput verticalInput;
        public HorizontalInput horizontalInput;
    }
}
