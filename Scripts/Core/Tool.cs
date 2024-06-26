using UnityEngine;
using System.Collections.Generic;
using PixelMiner.Enums;
using System.Threading.Tasks;
using PixelMiner.Miscellaneous;
namespace PixelMiner.Core
{
    public class Tool : MonoBehaviour
    {
        public static event System.Action<Vector3Int, BlockID, byte, byte> OnTarget;
        private UnityEngine.Camera _mainCam;
        private Ray _ray;
        private RaycastHit _hit;
        private RayCasting _rayCasting;



        private void Start()
        {
            //_main = Main.Instance;
            _mainCam = UnityEngine.Camera.main;
      

            _rayCasting = GameObject.FindAnyObjectByType<RayCasting>();
        }

   

        private void Update()
        {
            _ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
            var rayDir = _ray.direction;
            if (_rayCasting.DDAVoxelRayCast(_mainCam.transform.position, rayDir, out RaycastVoxelHit hitVoxel, out RaycastVoxelHit preHitVoxel, 1000))
            {
                Vector3Int hitGlobalPosition = hitVoxel.point;
                DrawBounds.Instance.AddBounds(Main.Instance.GetBlockBounds(hitVoxel.point, Vector3Int.one), Color.red);

                //if (Input.GetMouseButtonDown(0))
                //{
                //    Main.Instance.PlaceRandomLightBlock(preHitVoxel.point);
                //}
                //else if(Input.GetMouseButtonDown(1))
                //{
                //    Main.Instance.TryRemoveBlock(hitGlobalPosition, out BlockID removedBlock, 100);
                //}
            }


            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 rayDirection = _ray.direction;
                if (_rayCasting.DDAVoxelRayCast(_mainCam.transform.position, rayDirection, out RaycastVoxelHit hit, out RaycastVoxelHit preHit))
                {
                    Vector3Int hitGlobalPosition = preHit.point;
                    if (Main.Instance.TryGetChunk(hitGlobalPosition, out Chunk chunk))
                    {
                        Vector3Int relPosition = chunk.GetRelativePosition(hitGlobalPosition);
                        Main.Instance.PlaceBlock(hitGlobalPosition, BlockID.Stone);
                        //Main.Instance.PlaceBlockDataQueue.Enqueue(new PlaceBlockData(hitGlobalPosition, BlockID.Stone));
                    }
                }
            }

        
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {

                _ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 rayDirection = _ray.direction;
                if (_rayCasting.DDAVoxelRayCast(_mainCam.transform.position, rayDirection, out RaycastVoxelHit hit, out RaycastVoxelHit preHit))
                {
                    Vector3Int hitGlobalPosition = hit.point;
                    if (Main.Instance.TryGetChunk(hitGlobalPosition, out Chunk chunk))
                    {
                        if(Main.Instance.TryRemoveBlock(hitGlobalPosition, out BlockID removedBlock, 100))
                        {
                
                        }
                    }
                }

            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 rayDirection = _ray.direction;
                if (_rayCasting.DDAVoxelRayCast(_mainCam.transform.position, rayDirection, out RaycastVoxelHit hit, out RaycastVoxelHit preHit))
                {
                    Vector3Int hitGlobalPosition = preHit.point;
                    if (Main.Instance.TryGetChunk(hitGlobalPosition, out Chunk chunk))
                    {
                        FluidNode waterNode = new FluidNode()
                        {
                            GlobalPosition = hitGlobalPosition,
                            Level = 7
                        };

                        //chunk.WaterSpreadingBfsQueue.Enqueue(waterNode);

                        Vector3Int relativePosition = chunk.GetRelativePosition(hitGlobalPosition);
                        chunk.SetBlock(relativePosition, BlockID.Water);
                        chunk.SetLiquidLevel(relativePosition, Water.MAX_WATER_LEVEL);

                        WaterSource waterSource = WaterSourcePool.Pool.Get();
                        waterSource.WaterSpreadingBfsQueue.Enqueue(waterNode);
                        Main.Instance.WaterSources.Add(waterSource);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 rayDirection = _ray.direction;
                if (_rayCasting.DDAVoxelRayCast(_mainCam.transform.position, rayDirection, out RaycastVoxelHit hit, out RaycastVoxelHit preHit))
                {
                    Vector3Int hitGlobalPosition = preHit.point;
                    if (Main.Instance.TryGetChunk(hitGlobalPosition, out Chunk chunk))
                    {
                        FluidNode lavaNode = new FluidNode()
                        {
                            GlobalPosition = hitGlobalPosition,
                            Level = 7
                        };

                        //main.SetBlock(startNode.GlobalPosition, BlockID.Lava);
                        //main.SetLiquidLevel(startNode.GlobalPosition, startNode.Level);
                        //chunk.LavaSpreadingBfsQueue.Enqueue(lavaNode);


                        Vector3Int relativePosition = chunk.GetRelativePosition(hitGlobalPosition);
                        chunk.SetBlock(relativePosition, BlockID.Lava);
                        chunk.SetLiquidLevel(relativePosition, Lava.MAX_LAVA_LEVEL);

                        chunk.UpdateMask |= UpdateChunkMask.RenderAll;

                        LavaSource newSource = LavaSourcePool.Pool.Get();
                        newSource.LavaSpreadingBfsQueue.Enqueue(lavaNode);

                        Main.Instance.LavaSources.Add(newSource);
                    }

                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 rayDirection = _ray.direction;
                if (_rayCasting.DDAVoxelRayCast(_mainCam.transform.position, rayDirection, out RaycastVoxelHit hit, out RaycastVoxelHit preHit))
                {
                    Vector3Int hitGlobalPosition = preHit.point;
                    if (Main.Instance.TryGetChunk(hitGlobalPosition, out Chunk chunk))
                    {
                        Main.Instance.PlaceBlock(hitGlobalPosition, BlockID.RedLight);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                _ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 rayDirection = _ray.direction;
                if (_rayCasting.DDAVoxelRayCast(_mainCam.transform.position, rayDirection, out RaycastVoxelHit hit, out RaycastVoxelHit preHit))
                {
                    Vector3Int hitGlobalPosition = preHit.point;
                    if (Main.Instance.TryGetChunk(hitGlobalPosition, out Chunk chunk))
                    {
                        Main.Instance.PlaceBlock(hitGlobalPosition, BlockID.GreenLight);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                _ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 rayDirection = _ray.direction;
                if (_rayCasting.DDAVoxelRayCast(_mainCam.transform.position, rayDirection, out RaycastVoxelHit hit, out RaycastVoxelHit preHit))
                {
                    Vector3Int hitGlobalPosition = preHit.point;
                    if (Main.Instance.TryGetChunk(hitGlobalPosition, out Chunk chunk))
                    {
                        Main.Instance.PlaceBlock(hitGlobalPosition, BlockID.BlueLight);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                _ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 rayDirection = _ray.direction;
                if (_rayCasting.DDAVoxelRayCast(_mainCam.transform.position, rayDirection, out RaycastVoxelHit hit, out RaycastVoxelHit preHit))
                {
                    Vector3Int hitGlobalPosition = preHit.point;
                    if (Main.Instance.TryGetChunk(hitGlobalPosition, out Chunk chunk))
                    {
                        Main.Instance.PlaceBlock(hitGlobalPosition, BlockID.Light);
                    }
                }
            }


      


        }

        public Vector3Int GlobalToRelativeBlockPosition(Vector3 globalPosition)
        {
            // Calculate the relative position within the chunk
            int relativeX = Mathf.FloorToInt(globalPosition.x) % Main.Instance.ChunkDimension[0];
            int relativeY = Mathf.FloorToInt(globalPosition.y) % Main.Instance.ChunkDimension[1];
            int relativeZ = Mathf.FloorToInt(globalPosition.z) % Main.Instance.ChunkDimension[2];

            // Ensure that the result is within the chunk's dimensions
            if (relativeX < 0) relativeX += Main.Instance.ChunkDimension[0];
            if (relativeY < 0) relativeY += Main.Instance.ChunkDimension[1];
            if (relativeZ < 0) relativeZ += Main.Instance.ChunkDimension[2];

            return new Vector3Int(relativeX, relativeY, relativeZ);
        }

     
     
    }
}

