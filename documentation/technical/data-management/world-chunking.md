## World chunking

> Click the image below to see a video of the chunking system in action.

[![Video](http://img.youtube.com/vi/a8KrrGuT9Js/maxresdefault.jpg)](http://www.youtube.com/watch?v=a8KrrGuT9Js)

As usual with voxel engines, the actual block data is stored inside so-called "chunks" of blocks. This engine uses **cubic chunks** of 32x32x32 blocks.

Dividing the game world into chunks offers some advantages.
- A chunk-based architecture enables efficient rendering and optimization, allowing the engine to perform chunk culling to selectively update and render only the visible portions of the world. This drastically improves performance by minimizing the computational load and memory requirements.
- Dynamic world modification and streaming are made possible (infinite worlds!), as individual chunks can be loaded or unloaded based on the player's proximity.
- Simple multithreading/parallel processing of chunks. All the currently loaded chunks can be processed in 8 batches so that each thread in a batch always has a 3x3x3 chunk area to perform operations on. The processing order is `[x,y,z] [x±1,y,z] [x,y±1,z] [x,y,z±1] [x±1,y±1,z] [x±1,y,z±1] [x,y±1,z±1] [x±1,y±1,z±1]`.