# Pixel Miner (Build_a10b0)
## Overview
As a long-time lover of Minecraft, I always try to work on my own voxel game and it was a lot of fun, also a lot of work though.
I'm still working on this from time to time and plan to add many features like creatures, a crafting system, inventory,...

If you also love Minecraft like me and want to try something else I also made a Minecraft-like Inventory feel-like system. You can try at: https://github.com/HorneOnne/Item_Inventory_Crafting-system
## Features
- Multithreading world generation/meshing/lighting using Task Parallel Library (TPL).
- Generation
  + Chunk-based system
  + Resource distribution using Poisson Disc sampling algorithm
  + Multifractal simplex noise generation, biomes, structures,...
  + Optimize transparent meshes using Greedy-meshing
- Can add or remove blocks.
- Block Light calculation
   + 16-bit CPU base-lighting (Breadth-first search)
   + Support RGB color, as well as skylight
   + Voxel ambient occlusion
- Physics:
  + AABB collision detection
  + DDA voxel traversal
- 3D A* pathfinding
- Simple hotbar inventory
- Water/lave flow generation/removal from source and physics

## Contents
### Biomes
![image25](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/e6204105-5f73-4252-bd49-1ac83afd92e1)

![image26](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/dcdc9bb0-5bc4-448c-b4b3-3d71d39b92cd)

![image28](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/7d2ad992-c528-4524-a640-bb94f029d1d0)

![image30](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/6cdf6b8c-ee86-4cd5-9a08-e09a8e2424eb)

![image31](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/ebfdcb2e-99ed-4328-8eb4-e9d9d7736527)


### Water
![media1](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/3b2877cd-3834-499f-bb94-6282c63d6f0c)

![media2](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/a88161f9-bf75-4f82-a866-8a133fc3b32f)

### Lava
![media3](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/35caa63d-0644-4a7e-a35e-5c563facaa40)

![media4](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/acce521f-2e54-4627-a7d3-55745353bd01)


### Lighting
![0315 (1)](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/bcd5ccb0-6700-4ef8-8196-be0df741991f)


### Day/night cycles
![media7 (1)](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/8c8ec199-7ef3-4c81-9d55-3d1f10dc1dbd)


### 3D A* pathfinding
![0703](https://github.com/HorneOnne/PixelMiner_Build_a10b0/assets/65548001/72d693d7-1132-4ed2-8fc3-2be0de72b72d)

