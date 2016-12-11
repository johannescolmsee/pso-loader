using LibPSO.PacketDefinitions;
using LibPSO.PsoServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibPSO
{
    public class Packets
    {
        public static byte[] GetWelcomePacket(UInt32 serverkey, UInt32 clientkey, ClientType clientType, bool applyLittleEndianEncodingToKeys)
        {
            PacketHeader hdr = new PacketHeader();
            hdr.PacketType = ServerPacketType.Welcome;
            hdr.Length =
                4 // header
                +
                64 //copyright
                +
                4//server key
                +
                4;//client key
            var headerBytes = hdr.GetBytes(clientType);

            var crMessageBytes = Encoding.ASCII.GetBytes("DreamCast Port Map. Copyright SEGA Enterprises. 1999");
            var crMessagePadded = crMessageBytes.Concat(Enumerable.Range(0, 64 - crMessageBytes.Length).Select(x => (byte)0)).ToArray();

            if (applyLittleEndianEncodingToKeys)
            {
                serverkey = Helper.LE32(serverkey);
                clientkey = Helper.LE32(clientkey);
            }
            var serverKeyBytes = Helper.GetBytes(serverkey);
            var clientKeyBytes = Helper.GetBytes(clientkey);

            return headerBytes
                .Concat(crMessagePadded)
                .Concat(serverKeyBytes)
                .Concat(clientKeyBytes)
                .ToArray();
        }

        public static byte[] GetRedirectPacket(UInt32 ip, UInt16 port, ClientType clientType)
        {
            PacketHeader hdr = new PacketHeader();
            hdr.PacketType = ServerPacketType.Redirect;
            hdr.Length =
                4
                +
                4
                +
                4;
            var headerBytes = hdr.GetBytes(clientType);
            var ipbytes = Helper.GetBytes(ip);
            var portbytes = Helper.GetBytes(Helper.LE16(port));
            var paddingbytes = new byte[2];

            return headerBytes
                .Concat(ipbytes)
                .Concat(portbytes)
                .Concat(paddingbytes)
                .ToArray();
        }

        public static byte[] GetTimeStampPacket(ClientType clientType)
        {
            //new byte[] { 0xB1, 0x00, 0x20, 0x00, 0x32, 0x30, 0x31, 0x36, 0x3A, 0x31, 0x31, 0x3A, 0x30, 0x33, 0x3A, 0x20, 0x32, 0x31, 0x3A, 0x30, 0x35, 0x3A, 0x31, 0x38, 0x2E, 0x31, 0x30, 0x32, 0x00, 0x00, 0x00, 0x00 };
            //2016:11:03: 21:05:18.102
            var timestampBytes = Encoding.ASCII.GetBytes(
                String.Format(@"{0:yyyy:MM:dd: hh:mm:ss.fff}", DateTime.Now.ToUniversalTime())
                );
            PacketHeader hdr = new PacketHeader();
            hdr.PacketType = ServerPacketType.Timestamp;
            hdr.Length = (UInt16)
                (
                4//header
                +
                timestampBytes.Length
                +
                4//padding
                );
            var headerBytes = hdr.GetBytes(clientType);
            var paddingbytes = new byte[4];

            return headerBytes
                .Concat(timestampBytes)
                .Concat(paddingbytes)
                .ToArray();
        }

        public class MenuItem
        {
            public UInt32 MenuId { get; set; }
            public UInt32 ItemId { get; set; }
            public UInt16 Flags { get; set; }
            public string Name { get; set; }

            internal byte[] GetBytes(ClientType clientType)
            {
                if (clientType == ClientType.PC)
            {
                throw new NotImplementedException("ClientType PC not supported (yet?)");
            }
                var nameBytes = Encoding.ASCII.GetBytes(this.Name);
                byte[] fixedNameBytes = nameBytes;
                if (nameBytes.Length > 0x12)
                {
                    fixedNameBytes = nameBytes.Take(0x12).ToArray();
                }
                if (nameBytes.Length < 0x12)
                {
                    fixedNameBytes = nameBytes.Concat(Enumerable.Range(0, 0x12 - nameBytes.Length).Select(x => (byte)0)).ToArray();
                }
                fixedNameBytes[0x11] = 0;//0 terminating char

                var bytes = Helper.GetBytes(Helper.LE32(this.MenuId))
                    .Concat(Helper.GetBytes(Helper.LE32(this.ItemId)))
                    .Concat(Helper.GetBytes(Helper.LE16(this.Flags)))
                    .Concat(fixedNameBytes);
                return bytes.ToArray();
            }
        }

        public static byte[] GetBlockListMenuPacket(IEnumerable<MenuItem> menues, ClientType clientType)
        {
            if (clientType == ClientType.PC)
            {
                throw new NotImplementedException("ClientType PC not supported (yet?)");
            }
            //typedef struct dc_ship_list {
            //    dc_pkt_hdr_t hdr;           /* The flags field says how many entries */
            //    struct {
            //        uint32_t menu_id;
            //        uint32_t item_id;
            //        uint16_t flags;
            //        char name[0x12];
            //    } entries[0];
            //} PACKED dc_ship_list_pkt;

            var menuDbUSBytes = new MenuItem() { Name = "DATABASE/US", MenuId = 0x00040000, ItemId = 0, Flags = 0x0004 }.GetBytes(clientType);
            menuDbUSBytes[menuDbUSBytes.Length - 1] = 0x08;
            var menuItemByteArrays = menues
                .Select(x => x.GetBytes(clientType));

            PacketHeader hdr = new PacketHeader();
            hdr.PacketType = ServerPacketType.BlockListType;
            hdr.Flags = (byte)menues.Count();
            hdr.Length = (UInt16)
                (
                4//header
                +
                menuDbUSBytes.Length
                +
                menuItemByteArrays.Sum(x => x.Length)
                );
            var headerBytes = hdr.GetBytes(clientType);

            return headerBytes
                .Concat(menuDbUSBytes)
                .Concat(menuItemByteArrays.SelectMany(x => x))
                .ToArray();
        }

        public static byte[] GetMessageBoxPacket(String message, ClientType clientType)
        {
            var messageWithLanguageTagsAndEndGuard = "\tE" + message.Replace("\r", "") + '\0';
            if ((messageWithLanguageTagsAndEndGuard.Length % 4) != 0)
            {
                var totalLength = messageWithLanguageTagsAndEndGuard.Length + (4 - (messageWithLanguageTagsAndEndGuard.Length % 4));
                messageWithLanguageTagsAndEndGuard = messageWithLanguageTagsAndEndGuard.PadRight(totalLength, '\0');
            }


            PacketHeader hdr = new PacketHeader();
            hdr.PacketType = ServerPacketType.MessageBox;
            hdr.Length =
                (UInt16)
                (
                    4
                    +
                    messageWithLanguageTagsAndEndGuard.Length
                );
            var headerBytes = hdr.GetBytes(clientType);
            var messageBytes = messageWithLanguageTagsAndEndGuard.Select(x => (byte)x);

            return headerBytes
                .Concat(messageBytes)
                .ToArray();
        }

        public static byte[] GetUpdateCodePacket(byte[] code, ClientType clientType, byte flags = 0)
        {
            UInt32 codeLength = (UInt32)code.Length;
            var ucp = new UpdateCodePackage()
            {
                Header = new PacketHeader()
                {
                    PacketType = ServerPacketType.UpdateCodePacketType,
                    Flags = flags,
                    Length = (UInt16)(UpdateCodePackage.SIZE_WITHOUT_CODE + codeLength),
                },
                OffsetOffsetSavedEntryAddress = 0x24 + codeLength,
                UnknownHeader1 = 0x80000000,
                UnknownHeader2 = 0x2,
                EntryOffset = 0x4,
                Code = code,
                OffsetEntryPointCalculationCounter = 0x18 + codeLength,
                EntryPointCalculationCounter = 1,
                UnknownFooter1 = 0,
                UnknownFooter2 = 0,
                OffsetSavedEntryAddress = 0,
                OffsetCalculationEntry0 = 0,
                OffsetCalculationEntry1 = 0,
                OffsetCalculationEntry2 = 0,
                OffsetCalculationEntry3 = 0,
                OffsetCalculationEntry4 = 0,
                OffsetCalculationEntry5 = 0,
            };

            return ucp.GetBytes(clientType);
        }
    }
}
