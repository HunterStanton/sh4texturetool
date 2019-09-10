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

                    Directory.CreateDirectory(args[2]);
                    BinaryWriter imageWriter = new BinaryWriter(new FileStream(args[2] + "/" + i + ".dds", FileMode.Create));

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

                    // Hack to support non-mipmapped images
                    // TODO: Get rid of this at some point
                    long mainPointerLocation = reader.BaseStream.Position;


                    int mipMap1Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap2Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap3Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap4Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap5Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);
                    int mipMap6Pointer = reader.ReadInt32() + Convert.ToInt32(texOffset);

                    Console.WriteLine("Main image offset: " + imageDataPointer.ToString("X"));

                    int unknown = reader.ReadInt32();

                    var imageData = new byte[0];


                    // If there is one texture, this is a non-mipmapped image
                    // Common examples of these are loading screen images, UI elements, Henry, and his weapons
                    if (numTextures == 1)
                    {
                        // If we're not at the end of the chunk
                        if (i != textureCount -1)
                        {
                            // Read until the next texture pointer
                            reader.ReadBytes(0x50);

                            imageData = new byte[(height2 * width2) * 4];

                            // Every image's pointer needs to be incremented by a value of 0x70 * imageIndex for whatever reason
                            reader.BaseStream.Position = imageDataPointer + (0x70 * i);

                            // Get the image data
                            imageData = reader.ReadBytes(imageData.Length);
                        }
                        else
                        {
                            imageData = new byte[(height2 * width2) * 4];

                            // Every image's pointer needs to be incremented by a value of 0x70 * imageIndex for whatever reason
                            reader.BaseStream.Position = imageDataPointer + (0x70 * i);

                            // Get the image data
                            imageData = reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length - imageDataPointer));
                        }
                    }
                    else
                    {
                        // Every image's pointer needs to be incremented by a value of 0x70 * imageIndex for whatever reason
                        reader.BaseStream.Position = imageDataPointer + (0x70 * i);

                        imageData = reader.ReadBytes(Convert.ToInt32(mipMap1Pointer - imageDataPointer));
                    }


                    // DDS Writing
                    // A lot of this shit is hardcoded and very likely wrong such as the flags. The only things that are 100% certain are the pitch, reserved, and height/width.
                    // Everything else is hardcoded af, but every texture I've tried seems to dump fine, so it "just works"
                    // Structure ripped from https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header

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

    

                    if (imageType[0] != 21)
                    {
                        // PF Flag
                        imageWriter.Write(0x4);
                        imageWriter.Write(imageType);
                        imageWriter.Write(new byte[0x14]);
                    }
                    else
                    {
                        // PF Flag
                        imageWriter.Write(0x41);
                        imageWriter.Write(0x00);
                        imageWriter.Write(0x20);
                        imageWriter.Write(0xFF0000);
                        imageWriter.Write(0x00FF00);
                        imageWriter.Write(0x0000FF);
                        imageWriter.Write(0xFF000000);
                    }


                    // Caps
                    imageWriter.Write(0x1000);

                    imageWriter.Write(new byte[0x10]);

                    imageWriter.Write(imageData);

                    // Return to the next texture pointer
                    reader.BaseStream.Position = originalBaseStreamPos;

                    Console.WriteLine("---------------------------");

                    imageWriter.Close();
                }

                Console.WriteLine("Textures extracted from chunk!");

            }

            if (args[0] == "pack")
            {
                // Create a new file
                FileStream file = new FileStream(args[2], FileMode.Create);

                // Create a binary writer that will write the new texture chunk
                BinaryWriter writer = new BinaryWriter(file);

                // Get the DDS textures inside the output directory
                string[] files = Directory.GetFiles(args[1]);

                var sortedFiles = files.CustomSort().ToList();

                // Grab the number of textures inside the user's output directory
                short fileCount = Convert.ToInt16(files.Length);

                Console.WriteLine("Number of files in new texture chunk: " + fileCount);

                // Textures + palette count
                writer.Write(fileCount);
                writer.Write(fileCount);

                List<Texture.PaletteInfo> paletteInfos = new List<Texture.PaletteInfo>( fileCount);
                List<Texture.TextureHeader> textureHeaders = new List<Texture.TextureHeader>(fileCount);
                List<Texture.TextureInfo> textureInfos = new List<Texture.TextureInfo>(fileCount);
                List<Texture.Texture> texturePixels = new List<Texture.Texture>(fileCount);

                int previousImageSizes = 0;

                // Loop through every bin chunk in the output directory and build a bin file from it
                for(var i = 0;i < sortedFiles.Count;i++)
                {

                    Texture.TextureHeader header = new Texture.TextureHeader();

                    Texture.Texture pixels = new Texture.Texture();

                    Texture.TextureInfo info = new Texture.TextureInfo();

                    textureHeaders.Add(header);
                    textureInfos.Add(info);
                    texturePixels.Add(pixels);

                    // Create a new binary reader so we can read the information stored in the DDS file
                    BinaryReader reader = new BinaryReader(new FileStream(sortedFiles[i], FileMode.Open));

                    reader.BaseStream.Position = 0xc;

                    int height = reader.ReadInt32();
                    int width = reader.ReadInt32();
                    int pitch = reader.ReadInt32();

                    reader.BaseStream.Position = 0x54;

                    char[] imageType = reader.ReadChars(4);

                    // Get the pixels
                    reader.BaseStream.Position = 0x80;

                    byte[] imagePixels = reader.ReadBytes(Convert.ToInt32(reader.BaseStream.Length - reader.BaseStream.Position));

                    // Fill out the texture objects to the best of our ability

                    info.height = height;
                    info.width = width;

                    header.height = height;
                    header.width = width;
                    header.pitch = pitch;
                    header.imageType = imageType;

                    pixels.pixelData = imagePixels.ToList();

                    // Maths to calculate where the data will be
                    // PLEASE SAVE ME FROM THESE EQUATIONS!!!
                    header.textureOffset = ((0x70 * (fileCount)) + (i * 0x70)) + previousImageSizes;

                    previousImageSizes = imagePixels.Length + previousImageSizes;


                    // Close the reader, it's useless now
                    reader.Close();
                }

                writer.Write(new byte[0xC]);

                // More maths to calculate texture/palette pointers before they've even been written
                // TODO: Fix this ugly hack
                for (var i = 0; i < fileCount * 2; i++)
                {
                    writer.Write(((0x4 * (fileCount * 2)) + (i * 0x10)) + 0x10);
                }

                // Write texture infos
                for (var i = 0; i < fileCount; i++)
                {
                    // SH4 stores size as w x h instead of h x w that DDS files store
                    writer.Write(textureInfos[i].width);
                    writer.Write(textureInfos[i].height);
                    writer.Write(new byte[0x8]);
                }

                // Write palettes
                for (var i = 0; i < fileCount; i++)
                {
                    writer.Write(new byte[0xC]);

                    // Palette points to...itself for some odd reason + 0x60
                    writer.Write(((0x10 * fileCount) + (0x10 * i)) + (0x50 * i));
                } 

                // Write texture headers
                for (var i = 0; i < fileCount; i++)
                {
                    writer.Write(new byte[0x20]);
                    writer.Write(textureHeaders[i].width);
                    writer.Write(textureHeaders[i].height);
                    if (textureHeaders[i].imageType[0] == 00)
                    {
                        writer.Write(0x15);

                        // 
                        writer.Write(0x1);

                        writer.Write((textureHeaders[i].height * textureHeaders[i].width) * 4);
                    }
                    else
                    {
                        writer.Write(Encoding.UTF8.GetBytes(textureHeaders[i].imageType));

                        // Skip mipmaps for now, game will work without them anyway as all A8R8G8B8 textures don't have mipmaps either
                        writer.Write(0x1);

                        writer.Write((textureHeaders[i].pitch));
                    }

                    // Write the unknowns
                    writer.Write(new byte[0x1c]);

                    if (fileCount % 2 == 1)
                    {
                        if (fileCount == 1)
                        {
                            writer.Write((textureHeaders[i].textureOffset) - ((0x80 * i)) + 0x58);
                        }
                        else
                        {
                            writer.Write((textureHeaders[i].textureOffset + 0x8) - ((0x80 * i)));
                        }
                    }
                    else
                    {
                        writer.Write((textureHeaders[i].textureOffset) - ((0x80 * i)));
                    }

                    writer.Write(new byte[0x1c]);
                }

                for(var i=0;i< fileCount;i++)
                {
                    if (fileCount == 1)
                    {
                        writer.Write(new byte[0x58]);
                        writer.Write(texturePixels[i].pixelData.ToArray());
                        writer.Write(new byte[0x60]);
                    }
                    else
                    {
                        // If the file count is even
                        if (fileCount % 2 == 1 && i == 0)
                        {
                            writer.Write(new byte[0x8]);
                            writer.Write(texturePixels[i].pixelData.ToArray());
                            writer.Write(new byte[0x60]);
                        }
                        else
                        {
                            writer.Write(texturePixels[i].pixelData.ToArray());
                            writer.Write(new byte[0x60]);
                        }
                    }
                }

                // Close the binary writer and file
                writer.Close();
                file.Close();

                Console.WriteLine(args[2] + " successfully created!");
            }
        }
    }
}