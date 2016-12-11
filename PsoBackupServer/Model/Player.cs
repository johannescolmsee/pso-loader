using LibPSO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsoBackupServer.Model
{
    public class Player
    {
        public byte item_count;
        public byte hpmats_used;
        public byte tpmats_used;
        public byte language;
        public Item[] items = new Item[30];
        public UInt16 atp;
        public UInt16 mst;
        public UInt16 evp;
        public UInt16 hp;
        public UInt16 dfp;
        public UInt16 ata;
        public UInt16 lck;
        public UInt16 unk1;
        public UInt32 unk2_1;
        public UInt32 unk2_2;
        public UInt32 level;
        public UInt32 exp;
        public UInt32 meseta;
        public byte[] name = new byte[16];
        public UInt32 unk3_1;
        public UInt32 unk3_2;
        public UInt32 name_color;
        public byte model;
        public byte[] unused = new byte[15];
        public UInt32 name_color_checksum;
        public byte section;
        public byte ch_class;
        public byte v2flags;
        public byte version;
        public UInt32 v1flags;
        public UInt16 costume;
        public UInt16 skin;
        public UInt16 face;
        public UInt16 head;
        public UInt16 hair;
        public UInt16 hair_r;
        public UInt16 hair_g;
        public UInt16 hair_b;
        public UInt32/*float*/ prop_x;
        public UInt32/*float*/ prop_y;
        public byte[] config = new byte[0x48];
        public byte[] techniques = new byte[0x14];
        public UInt32 padding;
        //crank start
        UInt32 cmode_unk1;              /* Flip the words for dc/pc! */
        UInt32[] times = new UInt32[9];
        byte[] unk2 = new byte[0xB0];
        byte[] string_bytes = new byte[0x0C];
        UInt32[] unk3 = new UInt32[0x18];
        UInt32[] battle = new UInt32[7];
        //crank end
        public UInt32[] unk4 = new UInt32[6];
        public byte[] infoboard = new byte[0xAC];
        public UInt32[] blacklist = new UInt32[30];
        public UInt32 autoreply_enabled;
        public byte[] autoreply;                   /* Always at least 4 bytes! */

        public static Player FromBytes(byte[] bytes)
        {
            var p = new Player();

            p.item_count = bytes[0];
            p.hpmats_used = bytes[1];
            p.tpmats_used = bytes[2];
            p.language = bytes[3];

            //30 items
            int offsetFirstItem = 4;
            int i;
            for (i = 0; i < 30; i++)
            {
                p.items[i] = new Item()
                {
                    equipped = /*Helper.LE16*/(Helper.GetUInt16(bytes.Skip(offsetFirstItem + i * 28).Take(2))),
                    tech = /*Helper.LE16*/(Helper.GetUInt16(bytes.Skip(offsetFirstItem + i * 28 + 2).Take(2))),
                    flags = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetFirstItem + i * 28 + 4).Take(4))),
                    data_l = new UInt32[]
                        {
                            /*Helper.LE32*/(Helper.GetUInt32(bytes.Skip(offsetFirstItem + i * 28 + 8).Take(4))),
                            /*Helper.LE32*/(Helper.GetUInt32(bytes.Skip(offsetFirstItem + i * 28 + 12).Take(4))),
                            /*Helper.LE32*/(Helper.GetUInt32(bytes.Skip(offsetFirstItem + i * 28 + 16).Take(4))),
                        },
                    item_id = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetFirstItem + i * 28 + 20).Take(4))),
                    data2_l = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetFirstItem + i * 28 + 24).Take(4))),
                };
            }
            var offsetAfterInv = offsetFirstItem + i * 28;
            p.atp = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterInv).Take(2)));
            p.mst = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterInv + 2).Take(2)));
            p.evp = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterInv + 4).Take(2)));
            p.hp = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterInv + 6).Take(2)));
            p.dfp = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterInv + 8).Take(2)));
            p.ata = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterInv + 10).Take(2)));
            p.lck = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterInv + 12).Take(2)));
            p.unk1 = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterInv + 14).Take(4)));
            p.unk2_1 = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetAfterInv + 16).Take(4)));
            p.unk2_2 = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetAfterInv + 20).Take(4)));
            p.level = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetAfterInv + 24).Take(4)));
            p.exp = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetAfterInv + 28).Take(4)));
            p.meseta = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetAfterInv + 32).Take(4)));
            p.name = bytes.Skip(offsetAfterInv + 36).Take(16).ToArray();

            var offsetAfterName = offsetAfterInv + 36 + 16;
            p.unk3_1 = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetAfterName + 0).Take(4)));
            p.unk3_2 = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetAfterName + 4).Take(4)));
            p.name_color = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetAfterName + 8).Take(4)));
            p.model = bytes.Skip(offsetAfterName + 12).First();
            p.unused = bytes.Skip(offsetAfterName + 12 + 1).Take(15).ToArray();
            var offsetAfterUnused = offsetAfterName + 12 + 16;

            p.name_color_checksum = /*Helper.LE32*/(Helper.GetUInt32(bytes.Skip(offsetAfterUnused + 0).Take(4)));
            p.section = bytes.Skip(offsetAfterUnused + 4).First();
            p.ch_class = bytes.Skip(offsetAfterUnused + 5).First();
            p.v2flags = bytes.Skip(offsetAfterUnused + 6).First();
            p.version = bytes.Skip(offsetAfterUnused + 7).First();
            p.v1flags = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetAfterUnused + 8).Take(4)));
            p.costume = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterUnused + 12).Take(2)));
            p.skin = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterUnused + 14).Take(2)));
            p.face = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterUnused + 16).Take(2)));
            p.head = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterUnused + 18).Take(2)));
            p.hair = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterUnused + 20).Take(2)));
            p.hair_r = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterUnused + 22).Take(2)));
            p.hair_g = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterUnused + 24).Take(2)));
            p.hair_b = Helper.LE16(Helper.GetUInt16(bytes.Skip(offsetAfterUnused + 26).Take(2)));
            p.prop_x = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetAfterUnused + 28).Take(4)));
            p.prop_y = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetAfterUnused + 32).Take(4)));

            var offsetConfig = offsetAfterUnused + 32 + 4;
            //config
            p.config = bytes.Skip(offsetConfig)
                .Take(0x48)
                .ToArray();

            p.techniques = bytes.Skip(offsetConfig + 0x48)
                .Take(0x14)
                .ToArray();

            p.padding = Helper.LE32(Helper.GetUInt32(bytes.Skip(offsetConfig + 0x48 + 0x14).Take(4)));

            var offset_cmodestart = offsetConfig + 0x48 + 0x14 + 4;
            //TODO: read cmode stuff as well
            return p;

        }

        public IEnumerable<byte> GetBytes()
        {
            yield return this.item_count;
            yield return this.hpmats_used;
            yield return this.tpmats_used;
            yield return this.language;

            //30 items
            for (int i = 0; i < 30; i++)
            {
                foreach (var b in Helper.GetBytes(this.items[i].equipped))
                {
                    yield return b;
                }
                foreach (var b in Helper.GetBytes(this.items[i].tech))
                {
                    yield return b;
                }
                foreach (var b in Helper.GetBytes(this.items[i].flags))
                {
                    yield return b;
                }
                foreach (var d in this.items[i].data_l)
                {
                    foreach (var b in Helper.GetBytes(d))
                    {
                        yield return b;
                    }
                }

                foreach (var b in Helper.GetBytes(this.items[i].item_id))
                {
                    yield return b;
                }
                foreach (var b in Helper.GetBytes(this.items[i].data2_l))
                {
                    yield return b;
                }
            }

            foreach (var b in Helper.GetBytes(this.atp))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.mst))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.evp))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.hp))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.dfp))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.ata))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.lck))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.unk1))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.unk2_1))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.unk2_2))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.level))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.exp))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.meseta))
            {
                yield return b;
            }
            foreach (var b in this.name)
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.unk3_1))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.unk3_2))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.name_color))
            {
                yield return b;
            }
            yield return this.model;

            foreach (var b in this.unused)
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.name_color_checksum))
            {
                yield return b;
            }
            yield return this.section;
            yield return this.ch_class;
            yield return this.v2flags;
            yield return this.version;
            foreach (var b in Helper.GetBytes(this.v1flags))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.costume))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.skin))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.face))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.head))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.hair))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.hair_r))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.hair_g))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.hair_b))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.prop_x))
            {
                yield return b;
            }
            foreach (var b in Helper.GetBytes(this.prop_y))
            {
                yield return b;
            }
            foreach (var b in this.config)
            {
                yield return b;
            }
            foreach (var b in this.techniques)
            {
                yield return b;
            }

            foreach (var b in Helper.GetBytes(this.padding))
            {
                yield return b;
            }
        }
    };
}
