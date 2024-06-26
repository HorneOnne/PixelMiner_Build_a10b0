using UnityEngine;
using System.Collections.Generic;
using PixelMiner.DataStructure;
using PixelMiner.Core;
namespace PixelMiner.Physics
{
    [System.Serializable]
    public class SpatialGrid
    {
        private Vector3Int _dimensions;
        private Chunk _attachedChunk;
        //private Vector3Int _gridSize = new Vector3Int(16, 64, 16);
        private Vector3Int _gridSize = new Vector3Int(16, 64, 16);
        [SerializeField] private List<CustomBoxCollider>[] _aabbBoxes;
        public List<CustomBoxCollider>[] AABBBoxes { get => _aabbBoxes; }


        public Bounds Bounds { get; private set; }
        public Vector3Int Dimensions { get => _dimensions; }
        public Vector3Int Frame { get; set; }
        public Vector3Int CellSize { get => new Vector3Int(_dimensions.x / _gridSize.x, _dimensions.y / _gridSize.y, _dimensions.z / _gridSize.z); }
        public Vector3Int GridSize { get => _gridSize; }

        public SpatialGrid(Vector3Int frame, Vector3Int dimensions)
        {
            _aabbBoxes = new List<CustomBoxCollider>[dimensions.x / _gridSize.x * dimensions.y / _gridSize.y *  dimensions.z / _gridSize.z];
         
            this.Frame = frame;
            this._dimensions = dimensions;
            int width = _dimensions.x / _gridSize.x;
            int height = _dimensions.y / _gridSize.y;
            int depth = _dimensions.z / _gridSize.z;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        int index = x + y * width + z * width * height;
                        _aabbBoxes[index] = new();
                    }
                }
            }

            Bounds = new Bounds(new Vector3(frame.x * dimensions.x + (dimensions.x / 2f), frame.y * dimensions.y + (dimensions.y / 2f), frame.z * dimensions.z + (dimensions.z / 2f)), _dimensions); 
        }

        public void AttachChunk(Chunk chunk)
        {
            _attachedChunk = chunk;
          
        }

        public void AddBox(CustomBoxCollider box)
        {
            int gridIndex = GetGridIndex(box.transform.position);
            _aabbBoxes[gridIndex].Add(box);
        }

        private void RemoveBox(CustomBoxCollider box)
        {
            int gridIndex = GetGridIndex(box.transform.position);
            _aabbBoxes[gridIndex].Remove(box);
        }


        public int GetGridIndex(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x / _gridSize.x);
            int y = Mathf.FloorToInt(position.y / _gridSize.y);
            int z = Mathf.FloorToInt(position.z / _gridSize.z);
            //Debug.Log($"{GridSize}  {CellSize}");
            //Debug.Log($"{x} {y} {z} {x + y * CellSize.x + z * CellSize.x * CellSize.y}");
            return x + y * CellSize.x + z * CellSize.x * CellSize.y;
        }
        public int GetGridIndex(Vector3 position, out int x, out int y, out int z)
        {
            x = Mathf.FloorToInt(position.x / _gridSize.x);
            y = Mathf.FloorToInt(position.y / _gridSize.y);
            z = Mathf.FloorToInt(position.z / _gridSize.z);
            //Debug.Log($"{GridSize}  {CellSize}");
            //Debug.Log($"{x} {y} {z} {x + y * CellSize.x + z * CellSize.x * CellSize.y}");
            return x + y * CellSize.x + z * CellSize.x * CellSize.y;
        }

        public Bounds GetBounds(int frameX, int frameY, int frameZ)
        {
            return new Bounds(Bounds.min + new Vector3Int(frameX * _gridSize.z, frameY * _gridSize.y, frameZ * _gridSize.z) + new Vector3(_gridSize.x / 2.0f, _gridSize.y / 2.0f, _gridSize.z / 2.0f), _gridSize);
        }

        public List<Chunk> GetChunksInsideBounds(Bounds bounds)
        {
            Vector3 minBP = bounds.min;
            Vector3 maxBP = bounds.max;
            List<Chunk> chunks = new List<Chunk>();
            for (int y = (int)minBP.y; y < maxBP.y; y+= 64)
            {
                for (int z = (int)minBP.z; z < maxBP.z; z+=16)
                {
                    for (int x = (int)minBP.x; x < maxBP.x; x+=16)
                    {
                        if(Main.Instance.TryGetChunk(new Vector3(x,y,z), out Chunk chunk))
                        {
                            chunks.Add(chunk);
                        }
                    }
                }
            }

            return chunks;
        }

        public List<CustomBoxCollider> GetBoxes(int x, int y, int z)
        {
            int index = x + y * CellSize.x + z * CellSize.x * CellSize.y;
            return _aabbBoxes[index];
        }
        public List<CustomBoxCollider> GetBoxes(int index)
        {
            return _aabbBoxes[index];
        }
    }
}
