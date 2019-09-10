# sh4texturetool
A tool for editing Silent Hill 4 texture chunks stored in .bin files. Is capable of extracting and replacing all textures stored in Silent Hill 4 .bin texture chunks extracted with [sh4bin](https://github.com/HunterStanton/sh4bin) in a way that as closely matches the original game as possible to avoid crashes.

# Usage
* sh4texturetool unpack <chunk.textures> <output_directory> - Extracts every DDS texture into an output directory
* sh4texturetool pack <input_directory> <chunk.textures> - Creates a valid texture chunk from a folder filled with DDS textures
* sh4texturetool analyze <chunk.textures> - Analyzes a texture chunk and prints information about the textures within

# Compatibility
sh4texturetool works with the PC version of Silent Hill 4. PS2/Xbox support is planned, however.

# Credits
* Hunter Stanton (@HunterStanton)
