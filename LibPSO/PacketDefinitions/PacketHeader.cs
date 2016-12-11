using LibPSO.PsoServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LibPSO.PacketDefinitions
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketHeader
    {
        public ServerPacketType PacketType;
        public byte Flags;
        public UInt16 Length;

        public byte[] GetBytes(ClientType clientType)
        {
            switch (clientType)
            {
                case ClientType.Gamecube:
                case ClientType.Dreamcast:
                    return new byte[]
                    {
                        (byte)PacketType,
                        Flags,
                    }.Concat(Helper.GetBytes(Helper.LE16(Length)))
                    .ToArray();
                case ClientType.PC:
                    return Helper.GetBytes(Helper.LE16(Length))
                        .Concat(
                            new byte[]
                            {
                                (byte)PacketType,
                                Flags,
                            }
                        )
                        .ToArray();
                default:
                    throw new Exception("Unexpected Client Type.");
            }
        }

        public static PacketHeader FromBytes(byte[] bytes, ClientType type)
        {
            var res = new PacketHeader();
            switch (type)
            {
                case ClientType.Gamecube:
                case ClientType.Dreamcast:
                    res.PacketType = (ServerPacketType)bytes[0];
                    res.Flags = bytes[1];
                    res.Length = Helper.LE16(Helper.GetUInt16(bytes.Skip(2)));
                    break;
                case ClientType.PC:
                    res.Length = Helper.LE16(Helper.GetUInt16(bytes));
                    res.PacketType = (ServerPacketType)bytes[0 + 2];
                    res.Flags = bytes[1 + 2];
                    break;
                default:
                    break;
            }

            return res;
        }
    }
}
