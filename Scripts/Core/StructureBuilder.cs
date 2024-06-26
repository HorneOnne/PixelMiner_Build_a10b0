using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelMiner.Enums;

namespace PixelMiner.Core
{
    /// <summary>
    /// Stupid way to build voxel structure in game (will change later).
    /// </summary>
    public class StructureBuilder : MonoBehaviour
    {
        public static StructureBuilder Instance { get; private set; }


        public List<StructureNode> LavaPool = new()
        {
            // Stone
            new StructureNode(new Vector3Int(2,0,0), BlockID.Stone),
            new StructureNode(new Vector3Int(3,0,0), BlockID.Stone),
            new StructureNode(new Vector3Int(4,0,0), BlockID.Stone),
            new StructureNode(new Vector3Int(1,0,1), BlockID.Stone),
            new StructureNode(new Vector3Int(5,0,1), BlockID.Stone),
            new StructureNode(new Vector3Int(1,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(5,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(6,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(7,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(8,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(0,0,3), BlockID.Stone),
            new StructureNode(new Vector3Int(9,0,3), BlockID.Stone),
            new StructureNode(new Vector3Int(0,0,4), BlockID.Stone),
            new StructureNode(new Vector3Int(10,0,4), BlockID.Stone),
            new StructureNode(new Vector3Int(0,0,5), BlockID.Stone),
            new StructureNode(new Vector3Int(10,0,5), BlockID.Stone),
            new StructureNode(new Vector3Int(0,0,6), BlockID.Stone),
            new StructureNode(new Vector3Int(10,0,6), BlockID.Stone),
            new StructureNode(new Vector3Int(0,0,7), BlockID.Stone),
            new StructureNode(new Vector3Int(11,0,7), BlockID.Stone),
            new StructureNode(new Vector3Int(1,0,8), BlockID.Stone),
            new StructureNode(new Vector3Int(4,0,8), BlockID.Stone),
            new StructureNode(new Vector3Int(12,0,8), BlockID.Stone),
            new StructureNode(new Vector3Int(2,0,9), BlockID.Stone),
            new StructureNode(new Vector3Int(3,0,9), BlockID.Stone),
            new StructureNode(new Vector3Int(4,0,9), BlockID.Stone),
            new StructureNode(new Vector3Int(12,0,9), BlockID.Stone),
            new StructureNode(new Vector3Int(5,0,10), BlockID.Stone),
            new StructureNode(new Vector3Int(12,0,10), BlockID.Stone),
            new StructureNode(new Vector3Int(6,0,11), BlockID.Stone),
            new StructureNode(new Vector3Int(7,0,11), BlockID.Stone),
            new StructureNode(new Vector3Int(8,0,11), BlockID.Stone),
            new StructureNode(new Vector3Int(9,0,11), BlockID.Stone),
            new StructureNode(new Vector3Int(10,0,11), BlockID.Stone),
            new StructureNode(new Vector3Int(11,0,11), BlockID.Stone),


            // Lava
            new StructureNode(new Vector3Int(2,0,1), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,1), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,1), BlockID.Lava),
             new StructureNode(new Vector3Int(2,0,2), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,2), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,2), BlockID.Lava),
            new StructureNode(new Vector3Int(1,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(1,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,4), BlockID.Lava),
             new StructureNode(new Vector3Int(1,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,5), BlockID.Lava),
             new StructureNode(new Vector3Int(1,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(1,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,10), BlockID.Lava),
        };
        public List<StructureNode> LavaPool2 = new()
        {
            new StructureNode(new Vector3Int(4,1,0), BlockID.SandMine),
            new StructureNode(new Vector3Int(11,1,0), BlockID.SandMine),
            new StructureNode(new Vector3Int(2,1,1), BlockID.SandMine),
            new StructureNode(new Vector3Int(4,0,1), BlockID.Stone),
            new StructureNode(new Vector3Int(4,1,1), BlockID.SandMine),
            new StructureNode(new Vector3Int(5,0,1), BlockID.Stone),
            new StructureNode(new Vector3Int(5,1,1), BlockID.SandMine),
            new StructureNode(new Vector3Int(6,0,1), BlockID.Stone),
            new StructureNode(new Vector3Int(7,0,1), BlockID.Stone),
            new StructureNode(new Vector3Int(8,0,1), BlockID.Stone),
            new StructureNode(new Vector3Int(9,0,1), BlockID.Stone),
            new StructureNode(new Vector3Int(10,0,1), BlockID.Stone),
            new StructureNode(new Vector3Int(10,1,1), BlockID.Sand),
            new StructureNode(new Vector3Int(11,0,1), BlockID.Stone),
            new StructureNode(new Vector3Int(13,0,1), BlockID.SandMine),
            new StructureNode(new Vector3Int(1,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(1,1,2), BlockID.SandMine),
            new StructureNode(new Vector3Int(2,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(3,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(4,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(4,1,2), BlockID.SandMine),
            new StructureNode(new Vector3Int(10,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(11,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(12,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(13,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(14,0,2), BlockID.Stone),
            new StructureNode(new Vector3Int(14,1,2), BlockID.SandMine),
            new StructureNode(new Vector3Int(2,0,3), BlockID.Stone),
            new StructureNode(new Vector3Int(9,0,3), BlockID.Stone),
            new StructureNode(new Vector3Int(13,0,3), BlockID.Stone),
            new StructureNode(new Vector3Int(13,1,3), BlockID.SandMine),
            new StructureNode(new Vector3Int(2,0,4), BlockID.Stone),
            new StructureNode(new Vector3Int(9,0,4), BlockID.Stone),
            new StructureNode(new Vector3Int(13,0,4), BlockID.Stone),
            new StructureNode(new Vector3Int(13,1,4), BlockID.SandMine),
            new StructureNode(new Vector3Int(2,0,5), BlockID.Stone),
            new StructureNode(new Vector3Int(3,0,5), BlockID.Stone),
            new StructureNode(new Vector3Int(4,0,5), BlockID.Stone),
            new StructureNode(new Vector3Int(5,0,5), BlockID.Stone),
            new StructureNode(new Vector3Int(13,0,5), BlockID.Stone),
            new StructureNode(new Vector3Int(1,0,6), BlockID.Stone),
            new StructureNode(new Vector3Int(1,1,6), BlockID.SandMine),
            new StructureNode(new Vector3Int(3,0,6), BlockID.Stone),
            new StructureNode(new Vector3Int(6,0,6), BlockID.Stone),
            new StructureNode(new Vector3Int(6,1,6), BlockID.SandMine),
            new StructureNode(new Vector3Int(14,0,6), BlockID.Stone),
            new StructureNode(new Vector3Int(0,0,7), BlockID.Stone),
            new StructureNode(new Vector3Int(1,0,7), BlockID.Stone),
            new StructureNode(new Vector3Int(1,1,7), BlockID.SandMine),
            new StructureNode(new Vector3Int(7,0,7), BlockID.Stone),
            new StructureNode(new Vector3Int(7,1,7), BlockID.SandMine),
            new StructureNode(new Vector3Int(15,0,7), BlockID.Stone),
            new StructureNode(new Vector3Int(14,0,7), BlockID.Stone),
            new StructureNode(new Vector3Int(15,1,7), BlockID.SandMine),
            new StructureNode(new Vector3Int(0,0,8), BlockID.Stone),
            new StructureNode(new Vector3Int(1,0,8), BlockID.Stone),
            new StructureNode(new Vector3Int(1,1,8), BlockID.SandMine),
            new StructureNode(new Vector3Int(8,0,8), BlockID.Stone),
            new StructureNode(new Vector3Int(8,1,8), BlockID.SandMine),
            new StructureNode(new Vector3Int(15,0,8), BlockID.Stone),
            new StructureNode(new Vector3Int(14,0,8), BlockID.Stone),
            new StructureNode(new Vector3Int(15,1,8), BlockID.SandMine),
            new StructureNode(new Vector3Int(0,0,9), BlockID.Stone),
            new StructureNode(new Vector3Int(1,0,9), BlockID.Stone),
            new StructureNode(new Vector3Int(1,1,9), BlockID.SandMine),
            new StructureNode(new Vector3Int(2,0,9), BlockID.Stone),
            new StructureNode(new Vector3Int(12,0,9), BlockID.Stone),
            new StructureNode(new Vector3Int(12,1,9), BlockID.SandMine),
            new StructureNode(new Vector3Int(14,0,9), BlockID.Stone),
            new StructureNode(new Vector3Int(15,0,9), BlockID.Stone),
            new StructureNode(new Vector3Int(15,1,9), BlockID.SandMine),
            new StructureNode(new Vector3Int(1,0,10), BlockID.Stone),
            new StructureNode(new Vector3Int(1,1,10), BlockID.SandMine),
            new StructureNode(new Vector3Int(11,0,10), BlockID.Stone),
            new StructureNode(new Vector3Int(13,0,10), BlockID.Stone),
            new StructureNode(new Vector3Int(13,1,10), BlockID.SandMine),
            new StructureNode(new Vector3Int(14,0,10), BlockID.Stone),
            new StructureNode(new Vector3Int(2,0,11), BlockID.Stone),
            new StructureNode(new Vector3Int(2,1,11), BlockID.SandMine),
            new StructureNode(new Vector3Int(10,0,11), BlockID.Stone),
            new StructureNode(new Vector3Int(13,0,11), BlockID.Stone),
            new StructureNode(new Vector3Int(2,0,12), BlockID.Stone),
            new StructureNode(new Vector3Int(10,0,12), BlockID.Stone),
            new StructureNode(new Vector3Int(13,0,12), BlockID.Stone),
            new StructureNode(new Vector3Int(2,0,13), BlockID.Stone),
            new StructureNode(new Vector3Int(12,0,13), BlockID.Stone),
            new StructureNode(new Vector3Int(13,0,13), BlockID.Stone),
            new StructureNode(new Vector3Int(3,0,14), BlockID.Stone),
            new StructureNode(new Vector3Int(4,0,14), BlockID.Stone),
            new StructureNode(new Vector3Int(5,0,14), BlockID.Stone),
            new StructureNode(new Vector3Int(10,0,14), BlockID.Stone),
            new StructureNode(new Vector3Int(11,0,14), BlockID.Stone),
            new StructureNode(new Vector3Int(12,0,14), BlockID.Stone),
            new StructureNode(new Vector3Int(5,0,15), BlockID.Stone),
            new StructureNode(new Vector3Int(6,0,15), BlockID.Stone),
            new StructureNode(new Vector3Int(7,0,15), BlockID.Stone),
            new StructureNode(new Vector3Int(8,0,15), BlockID.Stone),
            new StructureNode(new Vector3Int(9,0,15), BlockID.Stone),
            new StructureNode(new Vector3Int(10,0,15), BlockID.Stone),

            // Lava
            new StructureNode(new Vector3Int(5,0,2), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,2), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,2), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,2), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,2), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,2), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,13), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,13), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,13), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,13), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,13), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,13), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,13), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,13), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,13), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,14), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,14), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,14), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,14), BlockID.Lava),


            // Lava turret
            new StructureNode(new Vector3Int(13,1,5), BlockID.Stone),
            new StructureNode(new Vector3Int(13,2,5), BlockID.Stone),
            new StructureNode(new Vector3Int(13,3,5), BlockID.Stone),
            new StructureNode(new Vector3Int(13,4,5), BlockID.Stone),
            new StructureNode(new Vector3Int(13,5,5), BlockID.Lava),

        };
        public List<StructureNode> LavaPool3 = new()
        {
            new StructureNode(new Vector3Int(2,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(3,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(4,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(5,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(6,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(7,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(8,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(9,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(10,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(11,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(12,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(13,0,2), BlockID.Sand),
            new StructureNode(new Vector3Int(1,0,3), BlockID.Sand),
            new StructureNode(new Vector3Int(14,0,3), BlockID.Sand),
            new StructureNode(new Vector3Int(1,0,4), BlockID.Sand),
            new StructureNode(new Vector3Int(14,0,4), BlockID.Sand),
            new StructureNode(new Vector3Int(0,0,5), BlockID.Sand),
            new StructureNode(new Vector3Int(15,0,5), BlockID.Sand),
            new StructureNode(new Vector3Int(0,0,6), BlockID.Sand),
            new StructureNode(new Vector3Int(15,0,6), BlockID.Sand),
            new StructureNode(new Vector3Int(0,0,7), BlockID.Sand),
            new StructureNode(new Vector3Int(15,0,7), BlockID.Sand),
            new StructureNode(new Vector3Int(0,0,8), BlockID.Sand),
            new StructureNode(new Vector3Int(15,0,8), BlockID.Sand),
            new StructureNode(new Vector3Int(0,0,9), BlockID.Sand),
            new StructureNode(new Vector3Int(15,0,9), BlockID.Sand),
            new StructureNode(new Vector3Int(0,0,10), BlockID.Sand),
            new StructureNode(new Vector3Int(15,0,10), BlockID.Sand),
            new StructureNode(new Vector3Int(1,0,11), BlockID.Sand),
            new StructureNode(new Vector3Int(15,0,11), BlockID.Sand),
            new StructureNode(new Vector3Int(2,0,12), BlockID.Sand),
            new StructureNode(new Vector3Int(14,0,12), BlockID.Sand),
            new StructureNode(new Vector3Int(3,0,13), BlockID.Sand),
            new StructureNode(new Vector3Int(4,0,13), BlockID.Sand),
            new StructureNode(new Vector3Int(5,0,13), BlockID.Sand),
            new StructureNode(new Vector3Int(6,0,13), BlockID.Sand),
            new StructureNode(new Vector3Int(7,0,13), BlockID.Sand),
            new StructureNode(new Vector3Int(8,0,13), BlockID.Sand),
            new StructureNode(new Vector3Int(9,0,13), BlockID.Sand),
            new StructureNode(new Vector3Int(10,0,13), BlockID.Sand),
            new StructureNode(new Vector3Int(11,0,13), BlockID.Sand),
            new StructureNode(new Vector3Int(12,0,13), BlockID.Sand),
            new StructureNode(new Vector3Int(13,0,13), BlockID.Sand),


            // Border
            new StructureNode(new Vector3Int(4,0,4), BlockID.Sand),
            new StructureNode(new Vector3Int(5,0,4), BlockID.Sand),
            new StructureNode(new Vector3Int(10,0,4), BlockID.Sand),
            new StructureNode(new Vector3Int(11,0,4), BlockID.Sand),
            new StructureNode(new Vector3Int(4,0,5), BlockID.Sand),
            new StructureNode(new Vector3Int(6,0,5), BlockID.Sand),
            new StructureNode(new Vector3Int(9,0,5), BlockID.Sand),
            new StructureNode(new Vector3Int(11,0,5), BlockID.Sand),
            new StructureNode(new Vector3Int(4,0,6), BlockID.Sand),
            new StructureNode(new Vector3Int(7,0,6), BlockID.Sand),
            new StructureNode(new Vector3Int(8,0,6), BlockID.Sand),
            new StructureNode(new Vector3Int(11,0,6), BlockID.Sand),
            new StructureNode(new Vector3Int(5,0,7), BlockID.Sand),
            new StructureNode(new Vector3Int(10,0,7), BlockID.Sand),
            new StructureNode(new Vector3Int(4,0,8), BlockID.Sand),
            new StructureNode(new Vector3Int(11,0,8), BlockID.Sand),
            new StructureNode(new Vector3Int(4,0,9), BlockID.Sand),
            new StructureNode(new Vector3Int(5,0,9), BlockID.Sand),
            new StructureNode(new Vector3Int(10,0,9), BlockID.Sand),
            new StructureNode(new Vector3Int(11,0,9), BlockID.Sand),
            new StructureNode(new Vector3Int(6,0,10), BlockID.Sand),
            new StructureNode(new Vector3Int(9,0,10), BlockID.Sand),
            new StructureNode(new Vector3Int(7,0,11), BlockID.Sand),
            new StructureNode(new Vector3Int(8,0,11), BlockID.Sand),

            
            // Gold 
            new StructureNode(new Vector3Int(5,0,5), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(10,0,5), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(5,0,6), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(6,0,6), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(9,0,6), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(10,0,6), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(6,0,7), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(7,0,7), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(8,0,7), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(9,0,7), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(5,0,8), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(6,0,8), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(7,0,8), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(8,0,8), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(9,0,8), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(10,0,8), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(6,0,9), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(7,0,9), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(8,0,9), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(9,0,9), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(7,0,10), BlockID.GoldBlock),
            new StructureNode(new Vector3Int(8,0,10), BlockID.GoldBlock),

            // Lava
            new StructureNode(new Vector3Int(2,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,3), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,4), BlockID.Lava),
            new StructureNode(new Vector3Int(1,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(14,0,5), BlockID.Lava),
            new StructureNode(new Vector3Int(1,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(14,0,6), BlockID.Lava),
            new StructureNode(new Vector3Int(1,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(14,0,7), BlockID.Lava),
            new StructureNode(new Vector3Int(1,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(14,0,8), BlockID.Lava),
            new StructureNode(new Vector3Int(1,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(14,0,9), BlockID.Lava),
            new StructureNode(new Vector3Int(1,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(14,0,10), BlockID.Lava),
            new StructureNode(new Vector3Int(2,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(14,0,11), BlockID.Lava),
            new StructureNode(new Vector3Int(3,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(4,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(5,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(6,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(7,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(8,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(9,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(10,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(11,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(12,0,12), BlockID.Lava),
            new StructureNode(new Vector3Int(13,0,12), BlockID.Lava),


            // Lava Turret
             //new StructureNode(new Vector3Int(0,0,0), BlockID.SandMine),
             //new StructureNode(new Vector3Int(0,1,0), BlockID.SandMine),
             //new StructureNode(new Vector3Int(0,2,0), BlockID.SandMine),
             //new StructureNode(new Vector3Int(0,3,0), BlockID.SandMine),
             //new StructureNode(new Vector3Int(0,4,0), BlockID.Lava),

             //new StructureNode(new Vector3Int(15,0,0), BlockID.SandMine),
             //new StructureNode(new Vector3Int(15,1,0), BlockID.SandMine),
             //new StructureNode(new Vector3Int(15,2,0), BlockID.SandMine),
             //new StructureNode(new Vector3Int(15,3,0), BlockID.SandMine),
             //new StructureNode(new Vector3Int(15,4,0), BlockID.Lava),
        };

        private void Awake()
        {
            Instance = this;
        }

        public void BuildLavaPool(Vector3Int startGlobalPosition)
        {
            for (int i = 0; i < LavaPool.Count; i++)
            {
                Vector3Int offsetGlobalPosition = startGlobalPosition + LavaPool[i].GlobalPosition;
                Main.Instance.SetBlock(offsetGlobalPosition, LavaPool[i].BlockID);
                if (LavaPool[i].BlockID == BlockID.Lava)
                {
                    Main.Instance.SetLiquidLevel(offsetGlobalPosition, Lava.MAX_LAVA_LEVEL);
                }
            }
        }

        public void BuildLavaPool2(Vector3Int startGlobalPosition)
        {
            //for (int i = 0; i < LavaPool2.Count; i++)
            //{
            //    Vector3Int offsetGlobalPosition = startGlobalPosition + LavaPool2[i].GlobalPosition;
            //    Main.Instance.SetBlock(offsetGlobalPosition, LavaPool2[i].BlockID);
            //    if (LavaPool2[i].BlockID == BlockID.Lava)
            //    {
            //        Main.Instance.SetLiquidLevel(offsetGlobalPosition, Lava.MAX_LAVA_LEVEL);
            //    }
            //}

            for (int i = 0; i < LavaPool2.Count; i++)
            {
                Vector3Int offsetGlobalPosition = startGlobalPosition + LavaPool2[i].GlobalPosition;
                if (Main.Instance.TryGetChunk(offsetGlobalPosition, out Chunk chunk))
                {
                    Vector3Int relativePosition = chunk.GetRelativePosition(offsetGlobalPosition);
                    chunk.SetBlock(relativePosition, LavaPool2[i].BlockID);

                    if (LavaPool2[i].BlockID == BlockID.Lava)
                    {
                        chunk.SetLiquidLevel(relativePosition, Lava.MAX_LAVA_LEVEL);
                        if (Main.Instance.CanSpread(chunk, relativePosition))
                        {
                            //Debug.Log("can spread");
                            FluidNode lavaNode = new FluidNode()
                            {
                                GlobalPosition = offsetGlobalPosition,
                                Level = Lava.MAX_LAVA_LEVEL
                            };
                            LavaSource newSource = LavaSourcePool.Pool.Get();
                            newSource.LavaSpreadingBfsQueue.Enqueue(lavaNode);
                            Main.Instance.LavaSources.Add(newSource);
                        }

                    }
                }
            }
        }

        public void BuildLavaPool3(Vector3Int startGlobalPosition)
        {
            for (int i = 0; i < LavaPool3.Count; i++)
            {
                Vector3Int offsetGlobalPosition = startGlobalPosition + LavaPool3[i].GlobalPosition;
                if(Main.Instance.TryGetChunk(offsetGlobalPosition, out Chunk chunk))
                {
                    Vector3Int relativePosition = chunk.GetRelativePosition(offsetGlobalPosition);
                    chunk.SetBlock(relativePosition, LavaPool3[i].BlockID);

                    if (LavaPool3[i].BlockID == BlockID.Lava)
                    {
                        chunk.SetLiquidLevel(relativePosition, Lava.MAX_LAVA_LEVEL);
                        if(Main.Instance.CanSpread(chunk, relativePosition))
                        {
                            //Debug.Log("can spread");
                            FluidNode lavaNode = new FluidNode()
                            {
                                GlobalPosition = offsetGlobalPosition,
                                Level = Lava.MAX_LAVA_LEVEL
                            };
                            LavaSource newSource = LavaSourcePool.Pool.Get();
                            newSource.LavaSpreadingBfsQueue.Enqueue(lavaNode);
                            Main.Instance.LavaSources.Add(newSource);
                        }

                    }
                }
            }
        }
    }

    public struct StructureNode
    {
        public Vector3Int GlobalPosition;
        public BlockID BlockID;

        public StructureNode(Vector3Int globalPosition, BlockID blockID)
        {
            GlobalPosition = globalPosition;
            BlockID = blockID;
        }
    }
}
