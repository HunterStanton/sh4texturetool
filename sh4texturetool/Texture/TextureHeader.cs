using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sh4texturetool.Texture
{
    class TextureHeader
    {
        public byte[] unknownBytes = new byte[0x20];
        public int height = 0;
        public int width = 0;

        // DXT1, DXT2, DXT3, DXT4, DXT5
        public char[] imageType = new char[4];

        // The number of mipmaps in the texture + the texture itself, max is 7
        public int numMipMaps = 0;

        // The DDS pitch
        public int pitch = 0;

        public byte[] unknownBytes2 = new byte[0x1c];

        public int textureOffset = 0;
        public int mipMap1Offset = 0;
        public int mipMap2Offset = 0;
        public int mipMap3Offset = 0;
        public int mipMap4Offset = 0;
        public int mipMap5Offset = 0;
        public int mipMap6Offset = 0;

        // This might be some sort of texture identifier that is referred to by the mesh - need to investigate
        public int unknown = 0;


    }
}
