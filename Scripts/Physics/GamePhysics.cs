using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using PixelMiner.Core;
using PixelMiner.Enums;
using PixelMiner.DataStructure;
using PixelMiner.Miscellaneous;
using Sirenix.OdinInspector;

namespace PixelMiner.Physics
{

  

    public class GamePhysics : SerializedMonoBehaviour
    {
        public static GamePhysics Instance { get; private set; }
        public Dictionary<Vector3Int, SpatialGrid> SpatialGrids = new();
        private Main _main;
        private DrawBounds _drawer;
        public int TotalEntities;
        private Vector3Int Dimension = new Vector3Int(64, 64, 64);

        public int AddEachFrame = 0;
        public int RemoveEachFrame = 0;





        private void Awake()
        {
            Instance = this;
            SpatialGrids = new();
            //SpatialGrids.Add(new Vector3Int(0, 0, 0), new SpatialGrid(Vector3Int.zero, Dimension));
        }

        public GameObject RedCubePrefab;
        public GameObject MyPhysicTestPrefab;
        public GameObject UnityPhysicTestPrefab;
        public LayerMask ItemLayer;

        public void AddBox(CustomBoxCollider box)
        {
            Vector3Int hasingKey = GetSptialHashing(box.transform.position);
            //Debug.Log(hasingKey);
            if (SpatialGrids.ContainsKey(hasingKey))
            {
                SpatialGrids[hasingKey].AddBox(box);
            }
            else
            {
                SpatialGrids.Add(hasingKey, new SpatialGrid(hasingKey, Dimension));
                SpatialGrids[hasingKey].AddBox(box);
            }
        }

        private Vector3Int GetSptialHashing(Vector3 globalPosition)
        {
            int x = Mathf.FloorToInt(globalPosition.x / Dimension.x);
            int y = Mathf.FloorToInt(globalPosition.y / Dimension.y);
            int z = Mathf.FloorToInt(globalPosition.z / Dimension.z);
            return new Vector3Int(x, y, z);
        }

        private void Start()
        {



            _main = Main.Instance;
            _drawer = DrawBounds.Instance;
            //Time.timeScale = 0.1f;



            // Test
            //int entityCount = 10000;
            //for (int i = 0; i < entityCount; i++)
            //{
            //    Vector3 randomPosition = new Vector3(Random.Range(-300, 300), Random.Range(-300, 300), Random.Range(-300, 300));
            //    var entityObject = Instantiate(Prefab, randomPosition, Quaternion.identity);
            //    AABB bound = GetBlockBound(randomPosition);
            //    DynamicEntity dEntity = new DynamicEntity(entityObject.transform, bound, Vector2.zero, ItemLayer);
            //    dEntity.Simulate = false;
            //    dEntity.SetConstraint(Constraint.X, true);
            //    dEntity.SetConstraint(Constraint.Y, true);
            //    dEntity.SetConstraint(Constraint.Z, true);
            //    HandleAddDynamicEntity(dEntity);
            //}


            //if (_addEntityQueue.Count > 0)
            //{
            //    while (_addEntityQueue.Count > 0)
            //    {
            //        HandleAddDynamicEntity(_addEntityQueue.Dequeue());
            //    }
            //}
        }



        private HashSet<Chunk> _potentialChunkCollided = new();
        private List<CustomBoxCollider> _potientialBoxesCollided = new();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                Random.InitState(100);
                for (int i = 0; i < 100; i++)
                {
                    int randomX = Random.Range(0, 64);
                    int randomY = Random.Range(10, 64);
                    int randomZ = Random.Range(0, 64);

                    Instantiate(MyPhysicTestPrefab, new Vector3(randomX, randomY, randomZ), Quaternion.identity);
                    //Instantiate(UnityPhysicTestPrefab, new Vector3(randomX, randomY, randomZ), Quaternion.identity);
                }

            }
        }
        private void LateUpdate()
        {

            //if (Input.GetKeyDown(KeyCode.I))
            //{
            //    Vector3 offset = new Vector3(-0.5f, -0.5f, -0.5f);
            //    for (int i = 0; i < 10000; i++)
            //    {
            //        Vector3 randomPosition = new Vector3(Random.Range(-500, 500), Random.Range(300, 1000), Random.Range(-500, 500));
            //        var entityObject = Instantiate(Prefab, randomPosition, Quaternion.identity);
            //        AABB bound = GetBlockBound(randomPosition + offset);
            //        PhysicEntities dEntity = new DynamicEntity(entityObject.transform, bound, Vector2.zero, ItemLayer);
            //        dEntity.Mass = 10;
            //        dEntity.SetConstraint(Constraint.X, true);
            //        dEntity.SetConstraint(Constraint.Y, false);
            //        dEntity.SetConstraint(Constraint.Z, true);
            //        AddDynamicEntity(dEntity);
            //    }
            //}


            foreach (var grid in SpatialGrids.Values)
            {
       
                //_drawer.AddBounds(grid.Bounds, Color.green);
                //_drawer.AddBounds(grid.GetBounds(0, 0, 0), Color.blue);
                //for (int x = 0; x < grid.CellSize.x; x++)
                //{
                //    for (int y = 0; y < grid.CellSize.y; y++)
                //    {
                //        for (int z = 0; z < grid.CellSize.z; z++)
                //        {
                //            _drawer.AddBounds(grid.GetBounds(x, y, z), Color.blue);
                //        }
                //    }
                //}
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                //Chunks.Clear();
                //Chunks = SpatialGrids[Vector3Int.zero].GetChunksInsideBounds(SpatialGrids[Vector3Int.zero].Bounds);


                //GetPotentialCollided(SpatialGrids[Vector3Int.zero], new Vector3Int(0, 0, 0));
            }
        }
        List<Task> _updatePhysicsTask = new();
        private async void FixedUpdate()
        {
            // Test
            //_physicsBoxDrawerList.Clear();
            // -------------------------


            //foreach (var grid in SpatialGrids.Values)
            //{
            //    HandlePhysics(grid, Time.deltaTime);
            //}

            _updatePhysicsTask.Clear();
            foreach (var grid in SpatialGrids.Values)
            {
                int width = grid.CellSize.x;
                int height = grid.CellSize.y;
                int depth = grid.CellSize.z;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int z = 0; z < depth; z++)
                        {
                            _updatePhysicsTask.Add(HandlePhysicsTask(grid, x, y, z, Time.deltaTime));
                        }
                    }
                }
            }
            //Debug.Log(_updatePhysicsTask.Count);
            await Task.WhenAll(_updatePhysicsTask);

            // Sync position
            foreach (var grid in SpatialGrids.Values)
            {
               foreach (var boxes in grid.AABBBoxes)
                {
                    for(int i  = 0; i < boxes.Count; i++)
                    {
                        boxes[i].transform.position = boxes[i].AttachedEntity.Position;
                    }
                }
            }
        }


        private void HandlePhysics(SpatialGrid grid, float dTime)
        {
            int width = grid.CellSize.x;
            int height = grid.CellSize.y;
            int depth = grid.CellSize.z;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        int index = x + y * width + z * width * height;
                        List<CustomBoxCollider> boxes = grid.GetBoxes(index);
                        if (boxes.Count == 0) continue;
                        //List<Chunk> potentialChunkCollided = grid.GetChunksInsideBounds(grid.GetBounds(x,y,z));
                        GetPotentialCollided(grid, new Vector3Int(x, y, z), ref _potientialBoxesCollided, ref _potentialChunkCollided);

                        //Debug.Log($"Potential collide:    chunk: {_potentialChunkCollided.Count}    boxes: {_potientialBoxesCollided.Count}");
                        for (int i = 0; i < boxes.Count; i++)
                        {
                            CustomBoxCollider box = boxes[i];
                            PhysicEntities attachedEntity = box.AttachedEntity;
                            //if (!attachedEntity.Simulate) continue;

                            bool entityUpdateByEditor = false;
                            bool entityIdle;
                            Bounds broadPhaseBox;

                            //Vector3 currPos = box.Position;
                            //box.Position = box.transform.position;

                            //if (currPos != box.Position)
                            //{
                            //    //Debug.Log("Not update physics this entity due editor.");
                            //    entityUpdateByEditor = true;
                            //}




                            float remainingTimeY = 1.0f;
                            float remainingTimeX = 1.0f;
                            float remainingTimeZ = 1.0f;


                            // Y
                            // ------------------------------------
                            //if (!attachedEntity.GetConstraint(Constraint.Y))
                            //{
                            //    // Apply gravity
                            //    attachedEntity.AddVelocity(UnityEngine.Physics.gravity * dTime);
                            //    broadPhaseBox = AABBExtensions.GetSweptBroadphaseY(box.Bounds, new Vector3(0, attachedEntity.Velocity.y, 0) * dTime);

                            //    bool intersecChunk = false;
                            //    bool intersectOtherBox = false;
                            //    foreach (var chunk in _potentialChunkCollided)
                            //    {
                            //        Bounds cBounds = new Bounds(chunk.transform.position + chunk.SolidVoxelMesh.bounds.center, chunk.SolidVoxelMesh.bounds.size);
                            //        DrawBounds.Instance.AddBounds(cBounds, Color.cyan);
                            //        if (broadPhaseBox.Intersects(cBounds))
                            //        {
                            //            intersecChunk = true;
                            //            break;
                            //        }
                            //    }

                            //    for (int j = 0; j < _potientialBoxesCollided.Count; j++)
                            //    {
                            //        if (_potientialBoxesCollided[j] != boxes[i])
                            //        {
                            //            if (broadPhaseBox.Intersects(_potientialBoxesCollided[j].Bounds))
                            //            {
                            //                intersectOtherBox = true;
                            //                ResolveResolutionYDynamic(box, broadPhaseBox, _potientialBoxesCollided[j], depth, ref remainingTimeY);
                            //                //break;
                            //            }
                            //        }

                            //    }

                            //    if (intersecChunk)
                            //    {
                            //        ResolveResolutionYStatic(box, broadPhaseBox, dTime, ref remainingTimeY);
                            //    }
                            //    else
                            //    {
                            //        var physicEntity = box.AttachedEntity;
                            //        physicEntity.Position = box.transform.position;
                            //        physicEntity.Position.y += box.AttachedEntity.Velocity.y * dTime;
                            //    }

                            //    if (intersectOtherBox)
                            //    {
                            //        Debug.Log("Intersect other boxes");
                            //    }
                            //}
                            if (!attachedEntity.GetConstraint(Constraint.Y))
                            {
                                // Apply gravity
                                attachedEntity.AddVelocity(UnityEngine.Physics.gravity * dTime);
                                broadPhaseBox = AABBExtensions.GetSweptBroadphaseY(box.Bounds, new Vector3(0, attachedEntity.Velocity.y, 0) * dTime);

                                bool intersecChunk = false;
                                bool intersectOtherBox = false;
                                int intersectOtherBoxAtIndex = -1;
                                float normalY = 0;

                                foreach (var chunk in _potentialChunkCollided)
                                {
                                    Bounds cBounds = new Bounds(chunk.transform.position + chunk.SolidVoxelMesh.bounds.center, chunk.SolidVoxelMesh.bounds.size);
                                    DrawBounds.Instance.AddBounds(cBounds, Color.cyan);
                                    if (broadPhaseBox.Intersects(cBounds))
                                    {
                                        intersecChunk = true;
                                        break;
                                    }
                                }

                                for (int j = 0; j < _potientialBoxesCollided.Count; j++)
                                {
                                    if (_potientialBoxesCollided[j] != boxes[i])
                                    {
                                        if (broadPhaseBox.Intersects(_potientialBoxesCollided[j].Bounds))
                                        {
                                            intersectOtherBox = true;
                                            intersectOtherBoxAtIndex = j;
                                            break;
                                        }
                                    }
                                }

                                if (intersecChunk || intersectOtherBox)
                                {
                                    Vector3 minBP = broadPhaseBox.min;
                                    Vector3 maxBP = broadPhaseBox.max;
                                    float nearestCollisionTimeY = 1.0f;
                                    for (float yy = minBP.y; yy <= maxBP.y; yy++)
                                    {
                                        for (float zz = minBP.z; zz <= maxBP.z; zz++)
                                        {
                                            for (float xx = minBP.x; xx <= maxBP.x; xx++)
                                            {
                                                BlockID currBlock = _main.GetBlock(new Vector3(xx, yy, zz));
                                                Bounds bound = GetBlockBound(new Vector3(xx, yy, zz));

                                                //_physicsBoxDrawerList.Add(new System.Tuple<AABB, Color>(bound, Color.red));

                                                if (currBlock.IsSolidOpaqueVoxel() || currBlock.IsSolidTransparentVoxel())
                                                {
                                                    int axis = AABBExtensions.SweepTest(box.Bounds, bound, new Vector3(0, box.AttachedEntity.Velocity.y, 0) * dTime, out float t,
                                                        out float normalX, out normalY, out float normalZ);

                                                    if (t >= nearestCollisionTimeY) continue;
                                                    nearestCollisionTimeY = t;
                                                }
                                                //else if (currBlock == BlockID.Water)
                                                //{
                                                //    AABBExtensions.AABBOverlapVolumnCheck(dEntity.AABB, bound, out float w, out float h, out float d);
                                                //    //dEntity.AddVelocityY(2f * dEntity.Mass * h * Time.deltaTime);
                                                //    dEntity.AddVelocityY(2f * dEntity.Mass * _physicWaterFloatingCurve.Evaluate(h) * dTime);

                                                //}
                                            }
                                        }
                                    }

                                    //if(intersectOtherBoxAtIndex != -1)
                                    //{
                                    //    for (int j = intersectOtherBoxAtIndex; j < _potientialBoxesCollided.Count; j++)
                                    //    {
                                    //        var otherBox = _potientialBoxesCollided[j];
                                    //        if (otherBox != boxes[i])
                                    //        {
                                    //            if (broadPhaseBox.Intersects(otherBox.Bounds))
                                    //            {
                                    //                AABBExtensions.SweepTest(box.Bounds, otherBox.Bounds, new Vector3(0, box.AttachedEntity.Velocity.y, 0) * dTime, out float t,
                                    //                       out float normalX, out normalY, out float normalZ);

                                    //                if (t >= nearestCollisionTimeY) continue;
                                    //                nearestCollisionTimeY = t;
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    if (intersectOtherBox)
                                    {
                                        AABBExtensions.SweepTest(box.Bounds, _potientialBoxesCollided[intersectOtherBoxAtIndex].Bounds, new Vector3(0, box.AttachedEntity.Velocity.y, 0) * dTime, out float t,
                                                  out float normalX, out normalY, out float normalZ);

                                        if (t >= nearestCollisionTimeY || t < 0) continue;
                                        nearestCollisionTimeY = t;
                                    }



                                    float remainingTime = 1.0f - nearestCollisionTimeY;
                                    attachedEntity.Position = box.transform.position;
                                    attachedEntity.Position.y += box.AttachedEntity.Velocity.y * nearestCollisionTimeY * dTime;
                                    remainingTime = 1.0f - nearestCollisionTimeY;

                                    if (remainingTime > 0.0f)
                                    {
                                        attachedEntity.OnGround = true;
                                        if (normalY == -1)
                                        {
                                            // Bouncing
                                            attachedEntity.Velocity.y *= remainingTime;
                                            attachedEntity.Velocity.y = -attachedEntity.Velocity.y;
                                        }
                                    }
                                    else
                                    {
                                        attachedEntity.OnGround = false;
                                    }

                                    if (attachedEntity.OnGround)
                                    {
                                        attachedEntity.SetVelocityY(0);
                                    }

                                }
                                else
                                {
                                    var physicEntity = box.AttachedEntity;
                                    physicEntity.Position = box.transform.position;
                                    physicEntity.Position.y += box.AttachedEntity.Velocity.y * dTime;
                                }
                            }












                            // X
                            // ------------------------------------
                            //if (!dEntity.GetConstraint(Constraint.X))
                            //{
                            //    if (dEntity.Velocity.x != 0)
                            //    {
                            //        broadPhase = AABBExtensions.GetSweptBroadphaseX(dEntity.AABB, new Vector3(dEntity.Velocity.x, 0, 0) * dTime);
                            //        ResolveResolutionX(dEntity, broadPhase, dTime, ref remainingTimeX);
                            //    }
                            //}






                            // Z
                            // ------------------------------------
                            //if (!dEntity.GetConstraint(Constraint.Z))
                            //{
                            //    if (dEntity.Velocity.z != 0)
                            //    {
                            //        broadPhase = AABBExtensions.GetSweptBroadphaseZ(dEntity.AABB, new Vector3(0, 0, dEntity.Velocity.z) * dTime);
                            //        ResolveResolutionZ(dEntity, broadPhase, dTime, ref remainingTimeZ);
                            //    }
                            //}






                            //if (box.Position != box.transform.position || entityUpdateByEditor)
                            //{
                            //    //Debug.Log($"Entity move");
                            //    if (box.Leave != null)
                            //    {
                            //        if (box.Leave.Bound.Contains(box.Position) == false)
                            //        {
                            //            _removeEntityQueue.Enqueue(box);
                            //            _addEntityQueue.Enqueue(box);
                            //        }
                            //    }
                            //    else if (box.Root.Bound.Contains(box.Position) == false)
                            //    {
                            //        _removeEntityQueue.Enqueue(box);
                            //        _addEntityQueue.Enqueue(box);
                            //    }
                            //    entityIdle = false;
                            //}
                            //else
                            //{
                            //    entityIdle = true;
                            //}



                            //// Update physics position
                            //if (!entityUpdateByEditor)
                            //{
                            //    box.transform.position = box.Position;
                            //}
                            //else
                            //{
                            //    box.Position = box.transform.position;
                            //}


                            box.transform.position = box.AttachedEntity.Position;
                        }


                    }
                }
            }

        }
        private async Task HandlePhysicsTask(SpatialGrid grid, int frameX, int frameY, int frameZ, float dTime)
        {
            int width = grid.CellSize.x;
            int height = grid.CellSize.y;
            int depth = grid.CellSize.z;

            int index = frameX + frameY * width + frameZ * width * height;
            List<CustomBoxCollider> boxes = grid.GetBoxes(index);
            if (boxes.Count == 0) return;

            MyPhysicData data = MyPhysicDataPool.Pool.Get();
            GetPotentialCollided(grid, new Vector3Int(frameX, frameY, frameZ), ref data.PotientialBoxesCollided, ref data.PotentialChunkCollided);

            await Task.Run(() =>
            {
                Parallel.For(0, boxes.Count, (i) =>
                {
                    CustomBoxCollider box = boxes[i];
                    PhysicEntities attachedEntity = box.AttachedEntity;
                    Bounds broadPhaseBox;


                    // Y
                    // ------------------------------------                  
                    if (!attachedEntity.GetConstraint(Constraint.Y))
                    {
                        // Apply gravity
                        attachedEntity.AddVelocity(UnityEngine.Physics.gravity * dTime);
                        broadPhaseBox = AABBExtensions.GetSweptBroadphaseY(box.Bounds, new Vector3(0, attachedEntity.Velocity.y, 0) * dTime);

                        bool intersecChunk = false;
                        bool intersectOtherBox = false;
                        int intersectOtherBoxAtIndex = -1;
                        float normalY = 0;

                        foreach (var chunk in data.PotentialChunkCollided)
                        {
                            //Bounds cBounds = new Bounds(chunk.Position + chunk.SolidVoxelBounds.center, chunk.SolidVoxelBounds.size);
                            if (broadPhaseBox.Intersects(chunk.SolidVoxelBounds))
                            {
                                intersecChunk = true;
                                break;
                            }
                        }

                        for (int j = 0; j < data.PotientialBoxesCollided.Count; j++)
                        {
                            if (data.PotientialBoxesCollided[j].InstanceID == boxes[i].InstanceID) continue;
                            if (broadPhaseBox.Intersects(data.PotientialBoxesCollided[j].Bounds))
                            {
                                intersectOtherBox = true;
                                intersectOtherBoxAtIndex = j;
                                break;
                            }
                        }

                        if (intersecChunk || intersectOtherBox)
                        {
                            Vector3 minBP = broadPhaseBox.min;
                            Vector3 maxBP = broadPhaseBox.max;
                            float nearestCollisionTimeY = 1.0f;
                            for (float yy = minBP.y; yy <= maxBP.y; yy++)
                            {
                                for (float zz = minBP.z; zz <= maxBP.z; zz++)
                                {
                                    for (float xx = minBP.x; xx <= maxBP.x; xx++)
                                    {
                                        BlockID currBlock = _main.GetBlock(new Vector3(xx, yy, zz));
                                        Bounds bound = GetBlockBound(new Vector3(xx, yy, zz));

                                        //_physicsBoxDrawerList.Add(new System.Tuple<AABB, Color>(bound, Color.red));

                                        if (currBlock.IsSolidOpaqueVoxel() || currBlock.IsSolidTransparentVoxel())
                                        {
                                            int axis = AABBExtensions.SweepTest(box.Bounds, bound, new Vector3(0, attachedEntity.Velocity.y, 0) * dTime, out float t,
                                                out float normalX, out normalY, out float normalZ);

                                            if (!(t >= nearestCollisionTimeY || t < 0))
                                            {
                                                nearestCollisionTimeY = t;
                                            }
                                            
                                        }
                                    }
                                }
                            }


                            if (intersectOtherBox)
                            {
                                AABBExtensions.SweepTest(box.Bounds, data.PotientialBoxesCollided[intersectOtherBoxAtIndex].Bounds, new Vector3(0, attachedEntity.Velocity.y, 0) * dTime, out float t,
                                          out float normalX, out normalY, out float normalZ);

                                if (!(t >= nearestCollisionTimeY || t < 0))
                                {
                                    nearestCollisionTimeY = t;
                                }                              
                            }



                            float remainingTime = 1.0f - nearestCollisionTimeY;
                            //attachedEntity.Position = box.transform.position;
                            attachedEntity.Position.y += attachedEntity.Velocity.y * nearestCollisionTimeY * dTime;
                            remainingTime = 1.0f - nearestCollisionTimeY;

                            if (remainingTime > 0.0f)
                            {
                                attachedEntity.OnGround = true;
                                if (normalY == -1)
                                {
                                    // Bouncing
                                    attachedEntity.Velocity.y *= remainingTime;
                                    attachedEntity.Velocity.y = -attachedEntity.Velocity.y;
                                }
                            }
                            else
                            {
                                attachedEntity.OnGround = false;
                            }

                            if (attachedEntity.OnGround)
                            {
                                attachedEntity.SetVelocityY(0);
                            }

                        }
                        else
                        {
                            //physicEntity.Position = box.transform.position;
                            attachedEntity.Position.y += attachedEntity.Velocity.y * dTime;
                        }
                    }
                });

                //for (int i = 0; i < boxes.Count; i++)
                //{
                //CustomBoxCollider box = boxes[i];
                //PhysicEntities attachedEntity = box.AttachedEntity;
                //Bounds broadPhaseBox;


                //// Y
                //// ------------------------------------                  
                //if (!attachedEntity.GetConstraint(Constraint.Y))
                //{
                //    // Apply gravity
                //    attachedEntity.AddVelocity(UnityEngine.Physics.gravity * dTime);
                //    broadPhaseBox = AABBExtensions.GetSweptBroadphaseY(box.Bounds, new Vector3(0, attachedEntity.Velocity.y, 0) * dTime);

                //    bool intersecChunk = false;
                //    bool intersectOtherBox = false;
                //    int intersectOtherBoxAtIndex = -1;
                //    float normalY = 0;

                //    foreach (var chunk in data.PotentialChunkCollided)
                //    {
                //        //Bounds cBounds = new Bounds(chunk.Position + chunk.SolidVoxelBounds.center, chunk.SolidVoxelBounds.size);
                //        if (broadPhaseBox.Intersects(chunk.SolidVoxelBounds))
                //        {
                //            intersecChunk = true;
                //            break;
                //        }
                //    }

                //    for (int j = 0; j < data.PotientialBoxesCollided.Count; j++)
                //    {
                //        if (data.PotientialBoxesCollided[j].InstanceID == boxes[i].InstanceID) continue;
                //        if (broadPhaseBox.Intersects(data.PotientialBoxesCollided[j].Bounds))
                //        {
                //            intersectOtherBox = true;
                //            intersectOtherBoxAtIndex = j;
                //            break;
                //        }
                //    }

                //    if (intersecChunk || intersectOtherBox)
                //    {
                //        Vector3 minBP = broadPhaseBox.min;
                //        Vector3 maxBP = broadPhaseBox.max;
                //        float nearestCollisionTimeY = 1.0f;
                //        for (float yy = minBP.y; yy <= maxBP.y; yy++)
                //        {
                //            for (float zz = minBP.z; zz <= maxBP.z; zz++)
                //            {
                //                for (float xx = minBP.x; xx <= maxBP.x; xx++)
                //                {
                //                    BlockID currBlock = _main.GetBlock(new Vector3(xx, yy, zz));
                //                    Bounds bound = GetBlockBound(new Vector3(xx, yy, zz));

                //                    //_physicsBoxDrawerList.Add(new System.Tuple<AABB, Color>(bound, Color.red));

                //                    if (currBlock.IsSolidVoxel() || currBlock.IsTransparentVoxel())
                //                    {
                //                        int axis = AABBExtensions.SweepTest(box.Bounds, bound, new Vector3(0, box.AttachedEntity.Velocity.y, 0) * dTime, out float t,
                //                            out float normalX, out normalY, out float normalZ);

                //                        if (t >= nearestCollisionTimeY || t < 0) continue;
                //                        nearestCollisionTimeY = t;
                //                    }
                //                }
                //            }
                //        }


                //        if (intersectOtherBox)
                //        {
                //            AABBExtensions.SweepTest(box.Bounds, data.PotientialBoxesCollided[intersectOtherBoxAtIndex].Bounds, new Vector3(0, attachedEntity.Velocity.y, 0) * dTime, out float t,
                //                      out float normalX, out normalY, out float normalZ);

                //            if (t >= nearestCollisionTimeY || t < 0) continue;
                //            nearestCollisionTimeY = t;
                //        }



                //        float remainingTime = 1.0f - nearestCollisionTimeY;
                //        //attachedEntity.Position = box.transform.position;
                //        attachedEntity.Position.y += attachedEntity.Velocity.y * nearestCollisionTimeY * dTime;
                //        remainingTime = 1.0f - nearestCollisionTimeY;

                //        if (remainingTime > 0.0f)
                //        {
                //            attachedEntity.OnGround = true;
                //            if (normalY == -1)
                //            {
                //                // Bouncing
                //                attachedEntity.Velocity.y *= remainingTime;
                //                attachedEntity.Velocity.y = -attachedEntity.Velocity.y;
                //            }
                //        }
                //        else
                //        {
                //            attachedEntity.OnGround = false;
                //        }

                //        if (attachedEntity.OnGround)
                //        {
                //            attachedEntity.SetVelocityY(0);
                //        }

                //    }
                //    else
                //    {
                //        //physicEntity.Position = box.transform.position;
                //        attachedEntity.Position.y += attachedEntity.Velocity.y * dTime;
                //    }
                //}
                //}
            });

            MyPhysicDataPool.Pool.Release(data);
        }




        //private void ResolveResolutionX(PhysicEntities dEntity, AABB broadPhase, float dTime, ref float remainingTime)
        //{
        //    //_physicsBoxDrawerList.Add(new System.Tuple<AABB, Color>(broadPhase, Color.black));

        //    Vector3Int minBP = _main.GetBlockGPos(new Vector3(broadPhase.x, broadPhase.y, broadPhase.z));
        //    Vector3Int maxBP = _main.GetBlockGPos(new Vector3(broadPhase.x + broadPhase.w, broadPhase.y + broadPhase.h, broadPhase.z + broadPhase.d));
        //    int axis;
        //    int nearestAxis = -1;
        //    float nearestCollisionTimeX = 1;
        //    float normalX = -999;
        //    float normalY = -999;
        //    float normalZ = -999;


        //    for (int y = minBP.y; y <= maxBP.y; y++)
        //    {
        //        for (int z = minBP.z; z <= maxBP.z; z++)
        //        {
        //            for (int x = minBP.x; x <= maxBP.x; x++)
        //            {
        //                BlockID currBlock = _main.GetBlock(new Vector3(x, y, z));
        //                AABB bound = GetBlockBound(new Vector3(x, y, z));
        //                //_physicsBoxDrawerList.Add(new System.Tuple<AABB, Color>(bound, Color.red));
        //                if (currBlock.IsSolidVoxel() || currBlock.IsTransparentVoxel())
        //                {

        //                    axis = AABBExtensions.SweepTest(dEntity.AABB, bound, new Vector3(dEntity.Velocity.x, 0, 0) * dTime, out float t,
        //                        out normalX, out normalY, out normalZ);

        //                    if (t >= nearestCollisionTimeX) continue;

        //                    nearestAxis = axis;
        //                    nearestCollisionTimeX = t;
        //                }
        //            }
        //        }
        //    }


        //    dEntity.Position.x += dEntity.Velocity.x * nearestCollisionTimeX * dTime;
        //    remainingTime = 1.0f - nearestCollisionTimeX;
        //    if (remainingTime > 0.0f)
        //    {
        //        dEntity.Position.x = (float)System.Math.Round(dEntity.Position.x, 2);
        //    }
        //    else
        //    {

        //    }
        //}

        //private void ResolveResolutionZ(PhysicEntities dEntity, AABB broadPhase, float dTime, ref float remainingTime)
        //{
        //    //_physicsBoxDrawerList.Add(new System.Tuple<AABB, Color>(broadPhase, Color.black));

        //    Vector3Int minBP = _main.GetBlockGPos(new Vector3(broadPhase.x, broadPhase.y, broadPhase.z));
        //    Vector3Int maxBP = _main.GetBlockGPos(new Vector3(broadPhase.x + broadPhase.w, broadPhase.y + broadPhase.h, broadPhase.z + broadPhase.d));
        //    int axis;
        //    int nearestAxis = -1;
        //    float nearestCollisionTimeZ = 1;
        //    float normalX = -999;
        //    float normalY = -999;
        //    float normalZ = -999;


        //    for (int y = minBP.y; y <= maxBP.y; y++)
        //    {
        //        for (int z = minBP.z; z <= maxBP.z; z++)
        //        {
        //            for (int x = minBP.x; x <= maxBP.x; x++)
        //            {
        //                BlockID currBlock = _main.GetBlock(new Vector3(x, y, z));
        //                AABB bound = GetBlockBound(new Vector3(x, y, z));
        //                //_physicsBoxDrawerList.Add(new System.Tuple<AABB, Color>(bound, Color.blue));
        //                if (currBlock.IsSolidVoxel() || currBlock.IsTransparentVoxel())
        //                {
        //                    axis = AABBExtensions.SweepTest(dEntity.AABB, bound, new Vector3(0, 0, dEntity.Velocity.z) * dTime, out float t,
        //                        out normalX, out normalY, out normalZ);

        //                    if (t >= nearestCollisionTimeZ) continue;
        //                    nearestAxis = axis;
        //                    nearestCollisionTimeZ = t;
        //                }
        //            }
        //        }
        //    }

        //    dEntity.Position.z += dEntity.Velocity.z * nearestCollisionTimeZ * dTime;
        //    remainingTime = 1.0f - nearestCollisionTimeZ;
        //    if (remainingTime > 0.0f)
        //    {
        //        dEntity.Position.z = (float)System.Math.Round(dEntity.Position.z, 2);
        //    }
        //    else
        //    {

        //    }
        //}

        private void ResolveResolutionYStatic(CustomBoxCollider colliderBox, Bounds broadPhaseBox, float dTime, ref float remainingTime)
        {
            int axis;
            int nearestAxis = -1;
            float nearestNormal = -999;
            float nearestCollisionTimeY = 1;
            //float remainingTimeY;
            float normalX = -999;
            float normalY = -999;
            float normalZ = -999;

            Vector3 minBP = broadPhaseBox.min;
            Vector3 maxBP = broadPhaseBox.max;

            int yResolution = -999;

            for (float y = minBP.y; y <= maxBP.y; y++)
            {
                for (float z = minBP.z; z <= maxBP.z; z++)
                {
                    for (float x = minBP.x; x <= maxBP.x; x++)
                    {
                        BlockID currBlock = _main.GetBlock(new Vector3(x, y, z));
                        Bounds bound = GetBlockBound(new Vector3(x, y, z));

                        //_physicsBoxDrawerList.Add(new System.Tuple<AABB, Color>(bound, Color.red));

                        if (currBlock.IsSolidOpaqueVoxel() || currBlock.IsSolidTransparentVoxel())
                        {
                            axis = AABBExtensions.SweepTest(colliderBox.Bounds, bound, new Vector3(0, colliderBox.AttachedEntity.Velocity.y, 0) * dTime, out float t,
                                out normalX, out normalY, out normalZ);

                            if (t >= nearestCollisionTimeY) continue;
                            nearestCollisionTimeY = t;
                            nearestAxis = axis;
                            nearestNormal = normalY;
                        }
                        //else if (currBlock == BlockID.Water)
                        //{
                        //    AABBExtensions.AABBOverlapVolumnCheck(dEntity.AABB, bound, out float w, out float h, out float d);
                        //    //dEntity.AddVelocityY(2f * dEntity.Mass * h * Time.deltaTime);
                        //    dEntity.AddVelocityY(2f * dEntity.Mass * _physicWaterFloatingCurve.Evaluate(h) * dTime);

                        //}
                    }
                }
            }

            var physicEntity = colliderBox.AttachedEntity;
            physicEntity.Position = colliderBox.transform.position;
            physicEntity.Position.y += colliderBox.AttachedEntity.Velocity.y * nearestCollisionTimeY * dTime;

            //return;

            //colliderBox.Position.y += colliderBox.Velocity.y * nearestCollisionTimeY * dTime;
            remainingTime = 1.0f - nearestCollisionTimeY;

            if (remainingTime > 0.0f)
            {
                //if (normalY == -1)
                //{
                //    // Bouncing
                //    colliderBox.Velocity.y *= remainingTime;
                //    colliderBox.Velocity.y = -colliderBox.Velocity.y;
                //}
                //else
                //{
                //    colliderBox.Position.y = (float)System.Math.Round(colliderBox.Position.y, 2);
                //    colliderBox.OnGround = true;
                //}

                physicEntity.OnGround = true;
            }
            else
            {
                physicEntity.OnGround = false;
            }

            if (physicEntity.OnGround)
            {
                physicEntity.SetVelocityY(0);
            }
        }
        private void ResolveResolutionYDynamic(CustomBoxCollider colliderBox, Bounds broadPhaseBox, CustomBoxCollider otherBoxes, float dTime, ref float remainingTime)
        {
            float nearestCollisionTimeY = 1;
            //float remainingTimeY;



            int axis = AABBExtensions.SweepTest(colliderBox.Bounds, otherBoxes.Bounds, new Vector3(0, colliderBox.AttachedEntity.Velocity.y, 0) * dTime, out float t,
                               out float normalX, out float normalY, out float normalZ);
            nearestCollisionTimeY = t;



            var attachedEntity = colliderBox.AttachedEntity;
            attachedEntity.Position = colliderBox.transform.position;
            attachedEntity.Position.y += colliderBox.AttachedEntity.Velocity.y * nearestCollisionTimeY * dTime;

            //return;

            //colliderBox.Position.y += colliderBox.Velocity.y * nearestCollisionTimeY * dTime;
            remainingTime = 1.0f - nearestCollisionTimeY;
            if (remainingTime > 0.0f)
            {
                attachedEntity.OnGround = true;
                if (normalY == -1)
                {
                    // Bouncing
                    attachedEntity.Velocity.y *= remainingTime;
                    attachedEntity.Velocity.y = -attachedEntity.Velocity.y;
                }
            }
            else
            {
                attachedEntity.OnGround = false;
            }

            if (attachedEntity.OnGround)
            {
                attachedEntity.SetVelocityY(0);
            }
        }

        private void SweptTestY(CustomBoxCollider colliderBox, Bounds broadPhaseBox, CustomBoxCollider otherBoxes, float dTime, ref float remainingTime)
        {
            float nearestCollisionTimeY = 1.0f;


            int axis = AABBExtensions.SweepTest(colliderBox.Bounds, otherBoxes.Bounds, new Vector3(0, colliderBox.AttachedEntity.Velocity.y, 0) * dTime, out float t,
                               out float normalX, out float normalY, out float normalZ);
            nearestCollisionTimeY = t;
            var attachedEntity = colliderBox.AttachedEntity;
            attachedEntity.Position = colliderBox.transform.position;
            attachedEntity.Position.y += colliderBox.AttachedEntity.Velocity.y * nearestCollisionTimeY * dTime;
            remainingTime = 1.0f - nearestCollisionTimeY;

            if (remainingTime > 0.0f)
            {
                attachedEntity.OnGround = true;
                if (normalY == -1)
                {
                    // Bouncing
                    attachedEntity.Velocity.y *= remainingTime;
                    attachedEntity.Velocity.y = -attachedEntity.Velocity.y;
                }
            }
            else
            {
                attachedEntity.OnGround = false;
            }

            if (attachedEntity.OnGround)
            {
                attachedEntity.SetVelocityY(0);
            }
        }



        //private void HandleAddDynamicEntity(PhysicEntities entity)
        //{
        //    //_dynamicEntities.Add(entity);  
        //    Vector3Int octreeFrame = GetSpatialFrame(entity.Transform.position);
        //    if (WorldPhysicsEntities.TryGetValue(octreeFrame, out Octree octree))
        //    {
        //        bool canInsert = octree.Insert(entity);
        //    }
        //    else
        //    {
        //        AABB octreeBounds = GetOctreeBounds(octreeFrame);
        //        //Octree newOctree = new Octree();
        //        Octree newOctree = OctreeRootPool.Pool.Get();
        //        newOctree.Init(octreeBounds, OCTREE_CAPACITY, level: 0);
        //        WorldPhysicsEntities.Add(octreeFrame, newOctree);
        //        bool canInsert = newOctree.Insert(entity);
        //    }
        //}

        //private void HandleRemoveDynamicEntity(PhysicEntities entity)
        //{

        //    var root = entity.Root;
        //    if (root != null)
        //    {
        //        root.Remove(entity);
        //    }
        //    else
        //    {
        //        Debug.Log($"HandleRemoveDynamicEntity       Node = null");
        //    }

        //}

        //public void AddDynamicEntity(PhysicEntities entity)
        //{
        //    _addEntityQueue.Enqueue(entity);
        //}

        //public void RemoveDynamicEntity(PhysicEntities entity, System.Action afterRemoveCallback = null)
        //{
        //    _removeEntityQueue.Enqueue(entity);
        //    afterRemoveCallback?.Invoke();
        //}





        //private int GetAllEntitiesInOctreeNonAlloc(Vector3Int octreeFrame, ref List<PhysicEntities> entities)
        //{
        //    var octree = WorldPhysicsEntities[octreeFrame];
        //    //octree.Query(GetOctreeBounds(octreeFrame), ref entities);
        //    for (int i = 0; i < octree.AllEntities.Count; i++)
        //    {
        //        entities.Add(octree.AllEntities[i]);
        //    }


        //    return entities.Count;
        //}


        private Bounds GetBlockBound(Vector3 globalPosition)
        {
            float x = Mathf.FloorToInt(globalPosition.x) + 0.5f;
            float y = Mathf.FloorToInt(globalPosition.y) + 0.5f;
            float z = Mathf.FloorToInt(globalPosition.z) + 0.5f;

            return new Bounds(new Vector3(x, y, z), Vector3.one);
        }



        #region Girds

        private Vector3Int[] GetAdjacentNeighborsNonAlloc(Vector3Int position, Vector3Int[] neighborPosition)
        {
            int index = 0;

            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    for (int zOffset = -1; zOffset <= 1; zOffset++)
                    {
                        // Skip the case where all offsets are zero (current position)
                        if (xOffset == 0 && yOffset == 0 && zOffset == 0)
                            continue;

                        neighborPosition[index] = position + new Vector3Int(xOffset, yOffset, zOffset);
                        index++;
                    }
                }
            }

            return neighborPosition;
        }

        public void GetPotentialCollided(SpatialGrid grid, Vector3Int spatialGridCell, ref List<CustomBoxCollider> boxes, ref HashSet<Chunk> chunks)
        {
            boxes.Clear();
            chunks.Clear();


            foreach (var chunk in grid.GetChunksInsideBounds(grid.GetBounds(spatialGridCell.x, spatialGridCell.y, spatialGridCell.z)))
            {
                if (chunks.Contains(chunk) == false)
                {
                    chunks.Add(chunk);
                }
            }

            int width = grid.CellSize.x;
            int height = grid.CellSize.y;
            int depth = grid.CellSize.z;
            for (int x = spatialGridCell.x - 1; x <= spatialGridCell.x + 1; x++)
            {
                for (int y = spatialGridCell.y - 1; y <= spatialGridCell.y + 1; y++)
                {
                    for (int z = spatialGridCell.z - 1; z <= spatialGridCell.z + 1; z++)
                    {
                        if (x == spatialGridCell.x && y == spatialGridCell.y && z == spatialGridCell.z)
                        {
                            boxes.AddRange(grid.GetBoxes(x, y, z));
                            continue;
                        }

                        //continue;
                        if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= depth)
                        {

                            //Debug.Log("out side regular grid");
                            //Debug.Log(new Vector3Int(x, y, z));
                            //Instantiate(RedCubePrefab, new Vector3(x * gridSize.x, y * gridSize.y, z * gridSize.z), Quaternion.identity);

                            Vector3Int gridSize = new Vector3Int(16, 64, 16);
                            Vector3 worldPosition = new Vector3(x * gridSize.x, y * gridSize.y, z * gridSize.z);
                            Vector3Int spatialHashKey = GetSptialHashing(worldPosition);
                            if (SpatialGrids.TryGetValue(spatialHashKey, out SpatialGrid nbGrid))
                            {

                                int nbCellIndex = nbGrid.GetGridIndex(worldPosition, out int nbCellX, out int nbCellY, out int nbCellZ);
                                boxes.AddRange(grid.GetBoxes(nbCellIndex));

                                foreach (var chunk in grid.GetChunksInsideBounds(nbGrid.GetBounds(nbCellX, nbCellY, nbCellZ)))
                                {
                                    Debug.Log("A");
                                    if (chunks.Contains(chunk) == false)
                                    {
                                        Debug.Log("B");
                                        chunks.Add(chunk);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Inside regular grid
                            boxes.AddRange(grid.GetBoxes(x, y, z));
                        }

                    }
                }
            }
        }
        #endregion

    }
}
