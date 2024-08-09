using System;
using Epic.OnlineServices;
namespace SynicSugar.P2P {
    public interface IGetPacket
    {
        bool GetPacketFromBuffer(ref byte ch, ref ProductUserId id, ref ArraySegment<byte> payload);
        bool GetSynicPacketFromBuffer(ref byte ch, ref ProductUserId id, ref ArraySegment<byte> payload);
    }
}