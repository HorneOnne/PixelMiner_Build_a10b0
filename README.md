# Pixel Miner (Build_a10b0)
## Overview
As a long-time lover of Minecraft, I always try to work on my own voxel game and it was a lot of fun, also a lot of work though.
I'm still working on this from time to time and plan to add many features like creatures, a crafting system, inventory,...

## Features
- Multithreading world generation/meshing/lighting using Task Parallel Library (TPL).
- Generation
  + Chunk-based system
  + Resource distribution using Poisson Disc sampling algorithm
  + Multifractal simplex noise generation, biomes, structures,...
  + Optimize transparent meshes using Greedy-meshing
- Can add or remove blocks.
- Block Light calculation
   + CPU base-lighting (Breadth-first search)
   + Support RGB color, as well as skylight
   + Voxel ambient occlusion
- Physics:
  + AABB collision detection
  + DDA voxel traversal
- 3D A* pathfinding
- Simple hotbar inventory
- Water/lave flow generation/removal from source and physics
