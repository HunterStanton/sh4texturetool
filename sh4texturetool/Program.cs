using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace sh4texturetool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("sh4texturetool\nA tool for extracting and reinjecting textures to and from Silent Hill 4 texture chunks.\nUsage:\nsh4bin <file.chunk> <output directory> - Unpacks textures into the specified output directory\nsh4bin <output directory> <file.chunk> - Packs textures into from the specified directory into a texture chunk.\nsh4bin analyze <file.chunk> - Analyzes a texture chunk and tells you information about it");
                return;
            }

            if (args[0] == "analyze")
            {

                // Open a filestream with the user selected file
                FileStream file = new FileStream(args[1], FileMode.Open);

                // Create a binary reader that will be used to read the file
                BinaryReader reader = new BinaryReader(file);

                // Grab the number of textures and palettes inside the chunk
                int textureCount = reader.ReadInt16();
                int paletteCount = reader.ReadInt16();

                Console.WriteLine("Number of textures inside the chunk: " + textureCount);
                Console.WriteLine("Number of palettes inside the chunk: " + paletteCount);

                // Always 0xC empty bytes, but might be technically used by something just always null in the files that shipped
                // Need to do some reversing to determine these
                reader.ReadBytes(0xC);

                // Calculate the relative offset that the offsets in the texture header are calculated by
                // We do this by skipping past all texture pointers, palette information, and texture information and then storing the offset
                // Not sure why they do it like this, but w/e lol

                // Read past the palettes and such
                reader.ReadBytes(0x4 * (textureCount + paletteCount));

                // Read past the palette/texture information
                // Palette data + texture data is 0x10 in size
                reader.ReadBytes(0x10 * (textureCount + paletteCount));

                // Save the texture offset for later calculation
                long texOffset = reader.BaseStream.Position;

                // Jump back to read the information for each texture
                reader.BaseStream.Position = 0x10;

                // Read texture size and positional data
                for (int i = 0; i < textureCount; i++)
                {
                    // Read the pointer to the current texture
                    int texturePointer = reader.ReadInt32();

                    Console.WriteLine("Texture file offset: " + texturePointer.ToString("X"));

                    // Save the original base stream position so we can return to it later to read the next texture's information
                    long originalBaseStreamPos = reader.BaseStream.Position;

                    // Go to the texture information pointer
                    reader.BaseStream.Position = texturePointer;

                    int imageWidth = reader.ReadInt32();
                    int imageHeight = reader.ReadInt32();

                    Console.WriteLine("Image Width: " + imageWidth);
                    Console.WriteLine("Image Height: " + imageHeight);

                    // Quick maths
                    // TODO: Maybe make this less dependent on math, but it doesn't seem like the pointers are there to do that. Oh well, this "just works"!
                    int imageHeaderPointer = ((0x10) + (0x4 * (textureCount + paletteCount)) + (0x10 * (textureCount + paletteCount))) + (i * 0x70);

                    reader.BaseStream.Position = imageHeaderPointer;

                    // Read empty bytes
                    reader.ReadBytes(0x20);

                    // Width/height repeated here
                    reader.ReadInt32();
                    reader.ReadInt32();

                    // Read image type
                    // Always going to be some form of DDS on Windows/Xbox, PS2 uses TM2 texture format so not sure what that uses
                    char[] imageType = reader.ReadChars(4);

                    Console.WriteLine("Image type: " + new string(imageType));

                    // Seems to be the texture + number of mipmaps
                    int numTextures = reader.ReadInt32();

                    Console.WriteLine("Number of mipmaps: " + (numTextures - 1));

                    // Read past unknown values
                    // TODO: Map these
                    reader.ReadBytes(0x20);

                    int imageDataPointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap1Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap2Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap3Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap4Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap5Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap6Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);

                    Console.WriteLine("Main image offset: " + imageDataPointer.ToString("X"));

                    int unknown = reader.ReadInt32();

                    // Return to the next texture pointer
                    reader.BaseStream.Position = originalBaseStreamPos;

                    Console.WriteLine("---------------------------");
                }

                Console.WriteLine("Texture chunk analyzed successfully!");

            }

            if (args[0] == "unpack")
            {
                // Open a filestream with the user selected file
                FileStream file = new FileStream(args[1], FileMode.Open);

                // Create a binary reader that will be used to read the file
                BinaryReader reader = new BinaryReader(file);

                // Grab the number of textures and palettes inside the chunk
                int textureCount = reader.ReadInt16();
                int paletteCount = reader.ReadInt16();

                Console.WriteLine("Number of textures inside the chunk: " + textureCount);
                Console.WriteLine("Number of palettes inside the chunk: " + paletteCount);

                // Always 0xC empty bytes, but might be technically used by something just always null in the files that shipped
                // Need to do some reversing to determine these
                reader.ReadBytes(0xC);

                // Calculate the relative offset that the offsets in the texture header are calculated by
                // We do this by skipping past all texture pointers, palette information, and texture information and then storing the offset
                // Not sure why they do it like this, but w/e lol

                // Read past the palettes and such
                reader.ReadBytes(0x4 * (textureCount + paletteCount));

                // Read past the palette/texture information
                // Palette data + texture data is 0x10 in size
                reader.ReadBytes(0x10 * (textureCount + paletteCount));

                // Save the texture offset for later calculation
                long texOffset = reader.BaseStream.Position;

                // Jump back to read the information for each texture
                reader.BaseStream.Position = 0x10;

                // Read texture size and positional data
                for (int i = 0; i < textureCount; i++)
                {

                    BinaryWriter imageWriter = new BinaryWriter(new FileStream(i+".dds", FileMode.Create));

                    // Read the pointer to the current texture
                    int texturePointer = reader.ReadInt32();

                    Console.WriteLine("Texture file offset: " + texturePointer.ToString("X"));

                    // Save the original base stream position so we can return to it later to read the next texture's information
                    long originalBaseStreamPos = reader.BaseStream.Position;

                    // Go to the texture information pointer
                    reader.BaseStream.Position = texturePointer;

                    int imageWidth = reader.ReadInt32();
                    int imageHeight = reader.ReadInt32();

                    Console.WriteLine("Image Width: " + imageWidth);
                    Console.WriteLine("Image Height: " + imageHeight);

                    // Quick maths
                    // TODO: Maybe make this less dependent on math, but it doesn't seem like the pointers are there to do that. Oh well, this "just works"!
                    int imageHeaderPointer = ((0x10) + (0x4 * (textureCount + paletteCount)) + (0x10 * (textureCount + paletteCount))) + (i * 0x70);

                    reader.BaseStream.Position = imageHeaderPointer;

                    // Read empty bytes
                    reader.ReadBytes(0x20);

                    // Width/height repeated here
                    int width2 = reader.ReadInt32();
                    int height2 = reader.ReadInt32();

                    // Read image type
                    // Always going to be some form of DDS on Windows/Xbox, PS2 uses TM2 texture format so not sure what that uses
                    char[] imageType = reader.ReadChars(4);

                    Console.WriteLine("Image type: " + new string(imageType));

                    // Seems to be the texture + number of mipmaps
                    int numTextures = reader.ReadInt32();
                    
                    // DDS pitch
                    int pitch = reader.ReadInt32();

                    Console.WriteLine("Number of mipmaps: " + (numTextures - 1));

                    // Read past unknown values
                    // TODO: Map these
                    reader.ReadBytes(0x1c);

                    int imageDataPointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap1Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap2Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap3Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap4Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap5Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap6Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);

                    Console.WriteLine("Main image offset: " + imageDataPointer.ToString("X"));

                    int unknown = reader.ReadInt32();

                    var imageData = new byte[mipMap1Pointer - imageDataPointer];

                    // Every image's pointer needs to be incremented by a value of 0x70 * imageIndex for whatever reason
                    reader.BaseStream.Position = imageDataPointer + (0x70 * i);

                    // Get the image data
                    imageData = reader.ReadBytes(mipMap1Pointer - imageDataPointer);

                    // Write DDS header
                    imageWriter.Write(0x20534444);

                    // Write DDS something?
                    imageWriter.Write(0x7c);

                    imageWriter.Write(0x081007);

                    imageWriter.Write(height2);
                    imageWriter.Write(width2);

                    imageWriter.Write(pitch);

                    // DDS volume
                    imageWriter.Write(0);

                    // Mip map level
                    imageWriter.Write(0);

                    // Write reserved section
                    imageWriter.Write(new byte[11 * 4]);

                    // DDS pixel format size (always 32)
                    imageWriter.Write(0x20);

                    // PF Flag
                    imageWriter.Write(0x4);

                    imageWriter.Write(imageType);

                    imageWriter.Write(new byte[0x14]);

                    // Caps
                    imageWriter.Write(0x1000);

                    imageWriter.Write(new byte[0x10]);

                    imageWriter.Write(imageData);

                    // Return to the next texture pointer
                    reader.BaseStream.Position = originalBaseStreamPos;

                    Console.WriteLine("---------------------------");

                    imageWriter.Close();
                }

                Console.WriteLine("Texture chunk analyzed successfully!");

            }

            if (args[0] == "pack")
            {
                // Create a new file
                FileStream file = new FileStream(args[2], FileMode.Create);

                // Create a binary writer that will write the new bin file
                BinaryWriter writer = new BinaryWriter(file);

                // Get the files inside the output directory
                string[] files = Directory.GetFiles(args[1]);

                // Grab the number of files inside the user's output directory
                int fileCount = files.Length;

                Console.WriteLine("Number of files in new .bin: " + fileCount);

                List<byte> binBody = new List<byte>();

                writer.Write(fileCount);

                // Leave enough space for 1024 files
                // TODO: Figure out why the game crashes when using any repacked bin file
                int tempLength = 0x300;

                // Loop through every bin chunk in the output directory and build a bin file from it
                foreach (string inputFile in files)
                {
                    // Write the file's offset into the new header
                    long length = new System.IO.FileInfo(inputFile).Length;

                    if (inputFile != files.First())
                    {
                        tempLength = Convert.ToInt32(length + tempLength);
                    }

                    // Write the offset of the file to the bin header
                    writer.Write(tempLength);

                    // Append the current bin chunk to the bin body
                    binBody.AddRange(File.ReadAllBytes(inputFile));

                }

                // Append extra bytes to pad the bin header to 0x2000
                writer.Write(new byte[0x300 - writer.BaseStream.Length]);

                // Write the bin body to the bin file
                writer.Write(binBody.ToArray());

                // Close the binary writer and file
                writer.Close();
                file.Close();

                Console.WriteLine(args[2] + " successfully created!");
            }
        }
    }
}