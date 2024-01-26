## World chunking

> Click the image below to see a video of the chunking system in action.

[![Video](http://img.youtube.com/vi/a8KrrGuT9Js/maxresdefault.jpg)](http://www.youtube.com/watch?v=a8KrrGuT9Js)

### Overview
As usual with voxel engines, the actual block data is stored inside so-called "chunks" of blocks. This engine uses ~~**cubic chunks** of 32x32x32 blocks~~
> [UPDATE: Cubic chunks are no longer used.](#cubic-vs-stacked-chunks)
> TLDR: The engine now uses 32x512x32 block chunks, that contain 16 stacked 32x32x32 **_sub-chunks_**.

### Advantages
Dividing the game world into chunks offers some advantages.
- A chunk-based architecture enables efficient rendering and optimization, allowing the engine to perform chunk culling to selectively update and render only the visible portions of the world. This drastically improves performance by minimizing the computational load and memory requirements.
- Dynamic world modification and streaming are made possible (infinite worlds!), as individual chunks can be loaded or unloaded based on the player's proximity.
- Simple multithreading/parallel processing of chunks. All the currently loaded chunks can be processed in 4 batches so that each thread in a batch always has a 3x3 chunk area to perform operations on.

### Cubic vs stacked chunks:

Cubic chunks are no longer used. Instead the engine now uses 32x512x32 block chunks, that contain 16 stacked 32x32x32 **_sub-chunks_**. This essentially means that the old chunks are now called sub-chunks, and no longer store the actual block data.

The reason for this is that cubic chunks make development way more cumbersome than it needs to be, with no real gameplay benefit; they might not even be compatible AT ALL with the current implementation of the lighting & world generation systems:
- The lighting system is based on the assumption that skylight propagates in a single direction (downwards) from the top of the world, which is not possible with cubic chunks, since there is no "top" or "bottom" to the world.
- Same issue with the world generation system, which is based on the assumption that the world has finite height and is generated from the top down, which is not possible with cubic chunks.

Currently the only real benefit of cubic chunks is that they allow for infinite world height, but that's not really a priority right now, and it's not even clear if it's possible to implement with the current systems.
