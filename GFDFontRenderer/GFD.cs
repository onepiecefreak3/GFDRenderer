using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GFDFontRenderer.IO;
using System.IO;

namespace GFDFontRenderer.GFD
{
    public class Support
    {
        public enum Ident : int
        {
            NotFound,
            NotSupported,
            Version1 = 0x00010c06,
            Version2 = 0x00010f06
        }

        public static Ident Identify(string GFDPath)
        {
            if (!File.Exists(GFDPath))
                return Ident.NotFound;

            var br = new BinaryReaderX(File.OpenRead(GFDPath));
            br.BaseStream.Position = 4;
            var version = br.ReadUInt32();

            var SupVers = new List<uint> { 0x00010c06, 0x00010f06 };
            if (!SupVers.Contains(version))
                return Ident.NotSupported;

            return (Ident)version;
        }
    }

    public class GFDv1
    {
        #region Struct Version 1
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Headerv1
        {
            public Magic Magic;
            public uint Version;
            public int unk0;
            public int unk1;
            public int unk2;
            public int FontSize;
            public int FontTexCount;
            public int CharCount;
            public int FCount;
            public float BaseLine;
            public float DescentLine;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Entryv1
        {
            public uint Character;
            public byte TexID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            byte[] GlyphPos;
            public byte unk1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            byte[] GlyphSize;
            public byte CharWidth;
            public byte XCorrection;
            public byte YCorrection;
            public byte Padding;

            int GetInt(byte[] ba) => ba.Aggregate(0, (output, b) => (output << 8) | b);

            public int GlyphPosY => GetInt(GlyphPos.Reverse().ToArray()) >> 12;
            public int GlyphPosX => GetInt(GlyphPos.Reverse().ToArray()) & 0xFFF;

            public int GlyphHeight => GetInt(GlyphSize.Reverse().ToArray()) >> 12;
            public int GlyphWidth => GetInt(GlyphSize.Reverse().ToArray()) & 0xFFF;
        }
        #endregion

        public Headerv1 Header;
        public List<float> HeaderF = new List<float>();
        public string Name;
        public List<Entryv1> Characters = new List<Entryv1>();

        public GFDv1(string GFDPath)
        {
            using (var br = new BinaryReaderX(File.OpenRead(GFDPath)))
            {
                //Header
                Header = br.ReadStruct<Headerv1>();
                HeaderF = br.ReadMultiple<float>(Header.FCount);

                //Name
                var nameSize = br.ReadInt32();
                Name = br.ReadCStringA();

                //Character Entries
                Characters = br.ReadMultiple<Entryv1>(Header.CharCount);
            }
        }
    }

    public class GFDv2
    {
        #region Struct Version 2
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Headerv2
        {
            public Magic Magic;
            public uint Version;
            public int unk0;
            public int unk1;
            public int unk2;
            public int FontSize;
            public int FontTexCount;
            public int CharCount;
            public int unk3;
            public int FCount;
            public float MaxCharacterWidth;
            public float MaxCharacterHeight;
            public float BaseLine;
            public float DescentLine;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Entryv2
        {
            public uint Character;
            public byte TexID;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            byte[] GlyphPos;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            byte[] GlyphSize;
            public byte unk0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            byte[] CharSize;
            public byte unk1;
            public byte XCorrection;
            public byte YCorrection;
            public short EndMark;

            int GetInt(byte[] ba) => ba.Aggregate(0, (output, b) => (output << 8) | b);

            public int GlyphPosY => GetInt(GlyphPos.Reverse().ToArray()) >> 12;
            public int GlyphPosX => GetInt(GlyphPos.Reverse().ToArray()) & 0xFFF;

            public int GlyphHeight => GetInt(GlyphSize.Reverse().ToArray()) >> 12;
            public int GlyphWidth => GetInt(GlyphSize.Reverse().ToArray()) & 0xFFF;

            public int CharHeight => GetInt(CharSize.Reverse().ToArray()) >> 12;
            public int CharWidth => GetInt(CharSize.Reverse().ToArray()) & 0xFFF;
        }
        #endregion

        public Headerv2 Header;
        public List<float> HeaderF = new List<float>();
        public string Name;
        public List<Entryv2> Characters = new List<Entryv2>();

        public GFDv2(string GFDPath)
        {
            using (var br = new BinaryReaderX(File.OpenRead(GFDPath)))
            {
                //Header
                Header = br.ReadStruct<Headerv2>();
                HeaderF = br.ReadMultiple<float>(Header.FCount);

                //Name
                var nameSize = br.ReadInt32();
                Name = br.ReadCStringA();

                //Character Entries
                Characters = br.ReadMultiple<Entryv2>(Header.CharCount);
            }
        }
    }
}
