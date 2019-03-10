using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cave.CRC.Tests
{
    [TestClass]
    public class TestCRC32
    {
        readonly static uint[] crc32a_table = {
            0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f,
            0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988,
            0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91, 0x1db71064, 0x6ab020f2,
            0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
            0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
            0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172,
            0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b, 0x35b5a8fa, 0x42b2986c,
            0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
            0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423,
            0xcfba9599, 0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
            0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190, 0x01db7106,
            0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
            0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x086d3d2d,
            0x91646c97, 0xe6635c01, 0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e,
            0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
            0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
            0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7,
            0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0,
            0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa,
            0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
            0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81,
            0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a,
            0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683, 0xe3630b12, 0x94643b84,
            0x0d6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
            0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
            0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc,
            0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5, 0xd6d6a3e8, 0xa1d1937e,
            0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
            0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55,
            0x316e8eef, 0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
            0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe, 0xb2bd0b28,
            0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
            0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a, 0x9c0906a9, 0xeb0e363f,
            0x72076785, 0x05005713, 0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38,
            0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
            0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
            0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69,
            0x616bffd3, 0x166ccf45, 0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2,
            0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc,
            0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
            0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693,
            0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94,
            0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
        };

        readonly static uint[] crc32b_table = {
            0X00000000, 0X04C11DB7, 0X09823B6E, 0X0D4326D9,
            0X130476DC, 0X17C56B6B, 0X1A864DB2, 0X1E475005,
            0X2608EDB8, 0X22C9F00F, 0X2F8AD6D6, 0X2B4BCB61,
            0X350C9B64, 0X31CD86D3, 0X3C8EA00A, 0X384FBDBD,
            0X4C11DB70, 0X48D0C6C7, 0X4593E01E, 0X4152FDA9,
            0X5F15ADAC, 0X5BD4B01B, 0X569796C2, 0X52568B75,
            0X6A1936C8, 0X6ED82B7F, 0X639B0DA6, 0X675A1011,
            0X791D4014, 0X7DDC5DA3, 0X709F7B7A, 0X745E66CD,
            0X9823B6E0, 0X9CE2AB57, 0X91A18D8E, 0X95609039,
            0X8B27C03C, 0X8FE6DD8B, 0X82A5FB52, 0X8664E6E5,
            0XBE2B5B58, 0XBAEA46EF, 0XB7A96036, 0XB3687D81,
            0XAD2F2D84, 0XA9EE3033, 0XA4AD16EA, 0XA06C0B5D,
            0XD4326D90, 0XD0F37027, 0XDDB056FE, 0XD9714B49,
            0XC7361B4C, 0XC3F706FB, 0XCEB42022, 0XCA753D95,
            0XF23A8028, 0XF6FB9D9F, 0XFBB8BB46, 0XFF79A6F1,
            0XE13EF6F4, 0XE5FFEB43, 0XE8BCCD9A, 0XEC7DD02D,
            0X34867077, 0X30476DC0, 0X3D044B19, 0X39C556AE,
            0X278206AB, 0X23431B1C, 0X2E003DC5, 0X2AC12072,
            0X128E9DCF, 0X164F8078, 0X1B0CA6A1, 0X1FCDBB16,
            0X018AEB13, 0X054BF6A4, 0X0808D07D, 0X0CC9CDCA,
            0X7897AB07, 0X7C56B6B0, 0X71159069, 0X75D48DDE,
            0X6B93DDDB, 0X6F52C06C, 0X6211E6B5, 0X66D0FB02,
            0X5E9F46BF, 0X5A5E5B08, 0X571D7DD1, 0X53DC6066,
            0X4D9B3063, 0X495A2DD4, 0X44190B0D, 0X40D816BA,
            0XACA5C697, 0XA864DB20, 0XA527FDF9, 0XA1E6E04E,
            0XBFA1B04B, 0XBB60ADFC, 0XB6238B25, 0XB2E29692,
            0X8AAD2B2F, 0X8E6C3698, 0X832F1041, 0X87EE0DF6,
            0X99A95DF3, 0X9D684044, 0X902B669D, 0X94EA7B2A,
            0XE0B41DE7, 0XE4750050, 0XE9362689, 0XEDF73B3E,
            0XF3B06B3B, 0XF771768C, 0XFA325055, 0XFEF34DE2,
            0XC6BCF05F, 0XC27DEDE8, 0XCF3ECB31, 0XCBFFD686,
            0XD5B88683, 0XD1799B34, 0XDC3ABDED, 0XD8FBA05A,
            0X690CE0EE, 0X6DCDFD59, 0X608EDB80, 0X644FC637,
            0X7A089632, 0X7EC98B85, 0X738AAD5C, 0X774BB0EB,
            0X4F040D56, 0X4BC510E1, 0X46863638, 0X42472B8F,
            0X5C007B8A, 0X58C1663D, 0X558240E4, 0X51435D53,
            0X251D3B9E, 0X21DC2629, 0X2C9F00F0, 0X285E1D47,
            0X36194D42, 0X32D850F5, 0X3F9B762C, 0X3B5A6B9B,
            0X0315D626, 0X07D4CB91, 0X0A97ED48, 0X0E56F0FF,
            0X1011A0FA, 0X14D0BD4D, 0X19939B94, 0X1D528623,
            0XF12F560E, 0XF5EE4BB9, 0XF8AD6D60, 0XFC6C70D7,
            0XE22B20D2, 0XE6EA3D65, 0XEBA91BBC, 0XEF68060B,
            0XD727BBB6, 0XD3E6A601, 0XDEA580D8, 0XDA649D6F,
            0XC423CD6A, 0XC0E2D0DD, 0XCDA1F604, 0XC960EBB3,
            0XBD3E8D7E, 0XB9FF90C9, 0XB4BCB610, 0XB07DABA7,
            0XAE3AFBA2, 0XAAFBE615, 0XA7B8C0CC, 0XA379DD7B,
            0X9B3660C6, 0X9FF77D71, 0X92B45BA8, 0X9675461F,
            0X8832161A, 0X8CF30BAD, 0X81B02D74, 0X857130C3,
            0X5D8A9099, 0X594B8D2E, 0X5408ABF7, 0X50C9B640,
            0X4E8EE645, 0X4A4FFBF2, 0X470CDD2B, 0X43CDC09C,
            0X7B827D21, 0X7F436096, 0X7200464F, 0X76C15BF8,
            0X68860BFD, 0X6C47164A, 0X61043093, 0X65C52D24,
            0X119B4BE9, 0X155A565E, 0X18197087, 0X1CD86D30,
            0X029F3D35, 0X065E2082, 0X0B1D065B, 0X0FDC1BEC,
            0X3793A651, 0X3352BBE6, 0X3E119D3F, 0X3AD08088,
            0X2497D08D, 0X2056CD3A, 0X2D15EBE3, 0X29D4F654,
            0XC5A92679, 0XC1683BCE, 0XCC2B1D17, 0XC8EA00A0,
            0XD6AD50A5, 0XD26C4D12, 0XDF2F6BCB, 0XDBEE767C,
            0XE3A1CBC1, 0XE760D676, 0XEA23F0AF, 0XEEE2ED18,
            0XF0A5BD1D, 0XF464A0AA, 0XF9278673, 0XFDE69BC4,
            0X89B8FD09, 0X8D79E0BE, 0X803AC667, 0X84FBDBD0,
            0X9ABC8BD5, 0X9E7D9662, 0X933EB0BB, 0X97FFAD0C,
            0XAFB010B1, 0XAB710D06, 0XA6322BDF, 0XA2F33668,
            0XBCB4666D, 0XB8757BDA, 0XB5365D03, 0XB1F740B4
        };

        void Check(IChecksum<uint> crc, byte[] data, uint value)
        {
            crc.Reset();
            crc.Update(data);
            Assert.AreEqual(value, crc.Value);
        }

        [TestMethod]
        public void Test_CRC32_Check()
        {
            uint u = CRC32.Reflect32(0x8F4F8040);
            Assert.AreEqual((uint)0x0201f2f1, u);
            Assert.AreEqual(0x8F4F8040, CRC32.Reflect32(u));
            {
                CRC32 crc32a = CRC32.Default;
                //check expected table against calculated table
                CollectionAssert.AreEqual(crc32a_table, crc32a.Table);
                //check crc test value
                Check(crc32a, Encoding.ASCII.GetBytes("123456789"), 0xcbf43926);
                Check(crc32a, Encoding.ASCII.GetBytes("Check123!"), 0x6c6e13dc);
            }

            {
                CRC32 crc32b = CRC32.BZIP2;
                //check expected table against calculated table
                CollectionAssert.AreEqual(crc32b_table, crc32b.Table);
                //check crc test value
                Check(crc32b, Encoding.ASCII.GetBytes("123456789"), 0xfc891918);
                Check(crc32b, Encoding.ASCII.GetBytes("Check123!"), 0x292C603E);
            }

            {
                CRC32 crc32c = CRC32.C;
                //check crc test value
                Check(crc32c, Encoding.ASCII.GetBytes("123456789"), 0xe3069283);
                Check(crc32c, Encoding.ASCII.GetBytes("Check123!"), 0x29AF14D2);
            }

            {
                CRC32 crc32d = CRC32.D;
                //check crc test value
                Check(crc32d, Encoding.ASCII.GetBytes("123456789"), 0x87315576);
                Check(crc32d, Encoding.ASCII.GetBytes("Check123!"), 0x74276C40);
            }

            {
                CRC32 crc32e = CRC32.MPEG2;
                //check crc test value
                Check(crc32e, Encoding.ASCII.GetBytes("123456789"), 0x0376e6e7);
                Check(crc32e, Encoding.ASCII.GetBytes("Check123!"), 0xD6D39FC1);
            }
        }
    }
}
