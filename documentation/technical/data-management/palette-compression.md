## Palette compression

![Image palette compression](https://upload.wikimedia.org/wikipedia/commons/thumb/f/f4/Indexed_palette.svg/150px-Indexed_palette.svg.png)

With the release of Minecraft 1.13, Mojang improved their compression for block data. The method is conceptually similar to color palette compression: the **distinct** items (colors) are compressed into an array, and pixels use variable-length indices to reference these items in said array. This eliminates the need to store the same data multiple times.

At its simplest, it is a combination of a buffer holding variable-bit-length indices, and a palette (array) into which these indices point, whose entries hold the actual block type & state data, including a reference counter, to track how often the block type is being used.

The magic of this technique is, that the fewer types of blocks are in a certain area, the less memory that area needs, both at runtime and when serialized. It is important to remember that a world of discrete voxels (eg. Minecraft), generally consists of large amounts of air and stone, with a few blocks mixed in; these regions thus only need one or two bits for every block stored. They are balanced out by regions of high entropy, like forests of any kind, player-built structures, and overall the 'surface' of the world.

This has a few notable advantages over "normal" block data/state compression methods (example: [VL32](https://eisenwave.github.io/voxel-compression-docs/file_formats/vl32.html)):
- Block data/state is "decoupled" from actual chunk data. This means that we are (in most cases) no longer limited by the memory footprint of a single voxel - a chunk of 32x32x32 voxels will take up about the same amount of memory whether a single voxel takes up 2 bytes or 8 bytes of memory.
- This reduces the memory footprint of blocks: a single block ideally takes up one single bit in memory. The common case is three to four bits, depending on how many distinct elements (block types) are there in a chunk. As a bonus, this allows faster transmission of block groups (chunks) across network connections.
- Practically infinite block types & states:
  As long as not every single block in a block group (chunk) is completely different from one another, the game can have as many types of blocks and their possible states as one wants (several tens of thousands).

Notes:
- Palette compression can usually be combined with other compression techniques (like [RLE](https://en.wikipedia.org/wiki/Run-length_encoding)) if required.
    - I'm not planning to implement run-length encoding in this project as I find it unnecessary, and it would have negative effects in high entropy (maximum block randomness) situations.
- Palette compression does not have to be used for just blocks: I'm planning on using it to compress chunk light data, as I will keep lighting data separate from actual block data.