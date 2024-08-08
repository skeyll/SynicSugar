using System;
namespace SynicSugar.P2P {
    public interface IPacketConvert
    {
        void ConvertFromPacket(ref byte ch, string id, ref ArraySegment<byte> payload);
    }
}