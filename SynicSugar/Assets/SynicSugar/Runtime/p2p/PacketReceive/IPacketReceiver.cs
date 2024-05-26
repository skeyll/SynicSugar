using System;
namespace SynicSugar.P2P{
    public interface IPacketReciver {
        void ConvertFromPacket(ref byte ch, ref string id, ref ArraySegment<byte> payload);
    }
}