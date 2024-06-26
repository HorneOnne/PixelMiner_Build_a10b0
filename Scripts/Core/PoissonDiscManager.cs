using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;


namespace PixelMiner.Core
{
    public class PoissonDiscManager : MonoBehaviour
    {
        public static PoissonDiscManager Instance { get; private set; }
        public static readonly Vector2Int GRASS_DIMENSION = new Vector2Int(64, 64);
        public static readonly Vector2Int TREE_DIMENSION = new Vector2Int(128, 128);
        
        private FastNoiseLite _noiseControl;
        private const int PRIME1 = 73856093;
        private const int PRIME2 = 19349663;


        #region Properties
        public Vector2Int RegionSize { get; private set; }
        #endregion

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            RegionSize = new Vector2Int(Main.Instance.ChunkDimension.x, Main.Instance.ChunkDimension.z);
            //RegionSize = new Vector2Int(256, 256);
        }


        public void GetPoissonDiscData(Vector2Int dimension, Vector3 globalPosition, out int frameX, out int frameZ, out int cellX, out int cellZ)
        {
            frameX = Mathf.FloorToInt(globalPosition.x / dimension.x);
            frameZ = Mathf.FloorToInt(globalPosition.z / dimension.y);

            int x = Mathf.FloorToInt(globalPosition.x) % dimension.x;
            int z = Mathf.FloorToInt(globalPosition.z) % dimension.y;
            if (x < 0) x += dimension.x;
            if (z < 0) z += dimension.y;

            cellX = x / RegionSize.x;
            cellZ = z / RegionSize.y;


            //cellX = Mathf.FloorToInt((globalPosition.x % DIMENSION.x) / CellSize.x);
            //cellZ = Mathf.FloorToInt((globalPosition.z % DIMENSION.y) / CellSize.y);

            //Debug.Log($"GlobalPosition: {globalPosition}      Frame: {frameX} {frameZ}      Cell: {cellX}   {cellZ}");
        }


        public void GetCellBound(Vector2Int dimension, int frameX, int frameZ, int cellX, int cellZ, out Vector2 minBound, out Vector2 maxBound)
        {
            minBound = new Vector2(frameX * dimension.x + cellX * RegionSize.x, frameZ * dimension.y + cellZ * RegionSize.y);
            maxBound = new Vector2(frameX * dimension.x + (cellX * RegionSize.x + RegionSize.x), frameZ * dimension.y + (cellZ * RegionSize.y + RegionSize.y));
        }

        // Poisson Disc Reference: https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="frameX"> 1 frame = 256 unit x in DIMENSION. </param>
        /// <param name="frameZ"> 1 frame = 256 unit y in DIMENSION. </param>
        /// <param name="noise"></param>
        /// <param name="minDistance"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        //public async Task<List<Vector2Int>> Get2DPoissonDiscTask(int frameX, int frameZ, FastNoiseLite noise, float minDistance = 5, float maxDistance = 20)
        //{
        //    int k = 30; // limit of samples
        //    float cellSize = maxDistance / Mathf.Sqrt(2);

        //    int gridWidth = Mathf.CeilToInt(GRASS_DIMENSION.x / cellSize);
        //    int gridHeight = Mathf.CeilToInt(GRASS_DIMENSION.y / cellSize);
        //    float offsetFromEdge = minDistance / 2f;
        //    List<Vector2Int>[] grid = new List<Vector2Int>[gridWidth * gridHeight];

        //    Queue<Vector2Int> processList = new Queue<Vector2Int>();
        //    List<Vector2Int> samplePoints = new List<Vector2Int>();
        //    Vector2Int currPoint;
        //    bool found;


        //    await Task.Run(() =>
        //    {
        //        Parallel.For(0, gridHeight, (y) =>
        //        {
        //            for (int x = 0; x < gridWidth; x++)
        //            {
        //                grid[x + y * gridWidth] = new List<Vector2Int>();
        //            }
        //        });

        //        //float noiseX = (noise.GetNoise(frameX * _dimensions.x, frameZ * _dimensions.y) + 1.0f) / 2.0f;
        //        //float noiseY = (noise.GetNoise(frameX * _dimensions.x, frameZ * _dimensions.y) + 1.0f) / 2.0f;
        //        //Vector2Int firstPoint = new Vector2Int(Mathf.FloorToInt(noiseX * (_dimensions.x - 1)), Mathf.FloorToInt(noiseY * (_dimensions.y - 1)));
        //        Vector2Int firstPoint = new Vector2Int(Random.Range(0, GRASS_DIMENSION.x), Random.Range(0, GRASS_DIMENSION.y));

        //        InsertGrid(firstPoint);
        //        processList.Enqueue(firstPoint);
        //        samplePoints.Add(firstPoint);

        //        int attempt = 0;
        //        // Generate other points from points in processList
        //        while (processList.Count > 0)
        //        {
        //            currPoint = processList.Peek();
        //            found = false;

        //            for (int i = 0; i < k; i++)
        //            {
        //                //float noiseValue = (noise.GetNoise((frameX * _dimensions.x) + currPoint.x, (frameZ * _dimensions.y) + currPoint.y) + 1.0f) / 2.0f;
        //                float noiseValue = Random.Range(-100000, 100000);
        //                float distance = Mathf.Lerp(minDistance, maxDistance, noiseValue);

        //                Vector2Int newPoint = GenerateRandomPointAround(currPoint, (i + 1), distance);

        //                if (IsValid(newPoint, distance, offsetFromEdge))
        //                {
        //                    processList.Enqueue(newPoint);
        //                    samplePoints.Add(newPoint);
        //                    InsertGrid(newPoint);
        //                    found = true;
        //                    break;
        //                }
        //            }

        //            if (found == false)
        //            {
        //                processList.Dequeue();
        //            }

        //            if (attempt++ > 10000)
        //            {
        //                Debug.Log("Infinite loop");
        //                break;
        //            }
        //        }
        //    });



        //    void InsertGrid(Vector2Int point)
        //    {
        //        int maxCellX = Mathf.FloorToInt(point.x / cellSize);
        //        int maxCellY = Mathf.FloorToInt(point.y / cellSize);
        //        grid[maxCellX + maxCellY * gridWidth].Add(point);
        //    }


        //    Vector2Int GenerateRandomPointAround(Vector2Int point, int attempt, float minDistance)
        //    {
        //        //float noiseValue = (noise.GetNoise((point.x + frameX * _dimensions.x) * attempt, (point.y + frameZ * _dimensions.y) * attempt) + 1) / 2.0f;
        //        float noiseValue = Random.Range(-100000, 100000);

        //        float theta = noiseValue * Mathf.PI * 2f;
        //        // Generate random radius
        //        float newRadius = minDistance + noiseValue * minDistance;

        //        // Calculate new point
        //        int newX = Mathf.FloorToInt(point.x + newRadius * Mathf.Cos(theta));
        //        int newY = Mathf.FloorToInt(point.y + newRadius * Mathf.Sin(theta));

        //        Vector2Int newPoint = new Vector2Int(newX, newY);
        //        return newPoint;
        //    }

        //    bool IsValid(Vector2 point, float minDist, float offsetFromEdge)
        //    {
        //        if (point.x - offsetFromEdge < 0 || point.x + offsetFromEdge > GRASS_DIMENSION.x - 1 || point.y - offsetFromEdge < 0 || point.y + offsetFromEdge > GRASS_DIMENSION.y - 1) return false;

        //        int maxCellX = Mathf.FloorToInt(point.x / cellSize);
        //        int maxCellY = Mathf.FloorToInt(point.y / cellSize);

        //        int maxStartX = Mathf.Max(0, maxCellX - 1);
        //        int maxEndX = Mathf.Min(maxCellX + 1, gridWidth - 1);
        //        int maxStartY = Mathf.Max(0, maxCellY - 1);
        //        int maxEndY = Mathf.Min(maxCellY + 1, gridHeight - 1);
        //        Vector2 gridPoint;

        //        for (int y = maxStartY; y <= maxEndY; y++)
        //        {
        //            for (int x = maxStartX; x <= maxEndX; x++)
        //            {
        //                for (int i = 0; i < grid[x + y * gridWidth].Count; i++)
        //                {
        //                    gridPoint = grid[x + y * gridWidth][i];
        //                    float dist = (point - gridPoint).sqrMagnitude;
        //                    if (dist < minDist * minDist)
        //                    {
        //                        return false;
        //                    }
        //                }
        //            }
        //        }


        //        return true;
        //    }

        //    return samplePoints;
        //}





        public async Task GetRandom2DPoissonDiscTask(PoissonDiscData poissonDiscData, Vector2Int dimension, Vector3 globalPosition, float minDistance = 5, float maxDistance = 20)
        {
            GetPoissonDiscData(dimension, globalPosition, out int frameX, out int frameZ, out int cellX, out int cellZ);
            GetCellBound(dimension, frameX, frameZ, cellX, cellZ, out Vector2 minBounds, out Vector2 maxBounds);
            Vector2 minLargeBounds = new Vector2(frameX * dimension.x, frameZ * dimension.y);
            Vector2 maxLargeBounds = new Vector2(frameX * dimension.x + dimension.x, frameZ * dimension.y + dimension.y);


            int k = 30; // limit of samples
            float cellSize = maxDistance / Mathf.Sqrt(2);
            int gridWidth = Mathf.CeilToInt(dimension.x / cellSize);
            int gridHeight = Mathf.CeilToInt(dimension.y / cellSize);
            float offsetFromEdge = minDistance / 2f;
            List<Vector2Int>[] grid = new List<Vector2Int>[gridWidth * gridHeight];
  

            //Debug.Log($"GridSize: {gridWidth} {gridHeight}");
            //Debug.Log($"Frame: {frameZ} {frameZ}");
            //Debug.Log($"Cell: {cellX} {cellZ}");

            Vector2Int currPoint;
            bool found;
            int seed = frameX * PRIME1 + frameZ * PRIME2;
            System.Random rand = new System.Random(seed);



            await Task.Run(() =>
            {
                Parallel.For(0, gridHeight, (y) =>
                {
                    for (int x = 0; x < gridWidth; x++)
                    {
                        grid[x + y * gridWidth] = new List<Vector2Int>();
                    }
                });


                Vector2Int firstPoint = new Vector2Int(Mathf.FloorToInt(RandomRange(minLargeBounds.x + offsetFromEdge, maxLargeBounds.x - offsetFromEdge)), Mathf.FloorToInt(RandomRange(minLargeBounds.y + offsetFromEdge, maxLargeBounds.y + offsetFromEdge)));
                InsertGrid(firstPoint);
                poissonDiscData.ProcessList.Enqueue(firstPoint);
                poissonDiscData.SamplePoints.Add(firstPoint);

                if(IsInsideRange(firstPoint, minBounds, maxBounds))
                {
                    poissonDiscData.SamplesResult.Add(firstPoint);
                }


                int attempt = 0;
                // Generate other points from points in processList
                while (poissonDiscData.ProcessList.Count > 0)
                {
                    currPoint = poissonDiscData.ProcessList.Peek();
                    found = false;

                    for (int i = 0; i < k; i++)
                    {
                        float randomValue = (float)rand.NextDouble();
                        float distance = Mathf.Lerp(minDistance, maxDistance, randomValue);

                        Vector2Int newPoint = GenerateRandomPointAround(currPoint, (i + 1), distance);

                        if (IsValid(newPoint, distance, offsetFromEdge))
                        {
                            poissonDiscData.ProcessList.Enqueue(newPoint);
                            poissonDiscData.SamplePoints.Add(newPoint);
                            InsertGrid(newPoint);
                            found = true;


                            if (IsInsideRange(newPoint, minBounds, maxBounds))
                            {
                                poissonDiscData.SamplesResult.Add(newPoint);
                            }

                            break;
                        }
                    }

                    if (found == false)
                    {
                        poissonDiscData.ProcessList.Dequeue();
                    }

                    if (attempt++ > 10000)
                    {
                        Debug.LogWarning("Something went wrong. Very high loop!!!");
                        break;
                    }
                }

                //Debug.Log($"Poisson Disc attempt: {attempt}");
            });



            void InsertGrid(Vector2Int point)
            {
                float x = point.x % dimension.x;
                float z = point.y % dimension.y;
                if (x < 0) x += dimension.x;
                if (z < 0) z += dimension.y;
                int maxCellX = Mathf.FloorToInt(x / cellSize);
                int maxCellY = Mathf.FloorToInt(z / cellSize);
                grid[maxCellX + maxCellY * gridWidth].Add(point);


                //int maxCellX = Mathf.FloorToInt(point.x / cellSize);
                //int maxCellY = Mathf.FloorToInt(point.y / cellSize);
                //grid[maxCellX + maxCellY * gridWidth].Add(point);
            }

            float RandomRange(float minValue, float maxValue)
            {
                float randomValue = (float)rand.NextDouble() * (maxValue - minValue) + minValue;
                return randomValue;
            }


            Vector2Int GenerateRandomPointAround(Vector2Int point, int attempt, float minDistance)
            {
                float randomValue = (float)rand.NextDouble();

                float theta = randomValue * Mathf.PI * 2f;
                // Generate random radius
                float newRadius = minDistance + randomValue * minDistance;

                // Calculate new point
                int newX = Mathf.FloorToInt(point.x + newRadius * Mathf.Cos(theta));
                int newY = Mathf.FloorToInt(point.y + newRadius * Mathf.Sin(theta));

                Vector2Int newPoint = new Vector2Int(newX, newY);
                return newPoint;
            }

            bool IsValid(Vector2 point, float minDist, float offsetFromEdge)
            {
                if (point.x - offsetFromEdge < minLargeBounds.x || point.x + offsetFromEdge > maxLargeBounds.x - 1 || point.y - offsetFromEdge < minLargeBounds.y || point.y + offsetFromEdge > maxLargeBounds.y - 1) return false;

                float unsignX = point.x % dimension.x;
                float unsignZ = point.y % dimension.y;
                if (point.x < 0) unsignX += dimension.x;
                if (point.y < 0) unsignZ += dimension.y;
                int maxCellX = Mathf.FloorToInt(unsignX / cellSize);
                int maxCellY = Mathf.FloorToInt(unsignZ / cellSize);


                int maxStartX = Mathf.Max(0, maxCellX - 1);
                int maxEndX = Mathf.Min(maxCellX + 1, gridWidth - 1);
                int maxStartY = Mathf.Max(0, maxCellY - 1);
                int maxEndY = Mathf.Min(maxCellY + 1, gridHeight - 1);
                Vector2 gridPoint;

                for (int y = maxStartY; y <= maxEndY; y++)
                {
                    for (int x = maxStartX; x <= maxEndX; x++)
                    {
                        for (int i = 0; i < grid[x + y * gridWidth].Count; i++)
                        {
                            gridPoint = grid[x + y * gridWidth][i];
                            float dist = (point - gridPoint).sqrMagnitude;
                            if (dist < minDist * minDist)
                            {
                                return false;
                            }
                        }
                    }
                }


                return true;
            }


            bool IsInsideRange(Vector2 point, Vector2 min, Vector2 max)
            {
                return !(point.x < min.x || point.x >= max.x || point.y < min.y || point.y >= max.y);
            }
        }
    }

    public class PoissonDiscData
    {
        public Queue<Vector2Int> ProcessList;
        public List<Vector2Int> SamplePoints;
        public List<Vector2Int> SamplesResult;

        public PoissonDiscData()
        {
            ProcessList = new();
            SamplePoints = new();
            SamplesResult = new();
        }



        public void Clear()
        {
            ProcessList.Clear();
            SamplePoints.Clear();
            SamplesResult.Clear();
        }
    }

    public static class PoissonDiscDataPool
    {
        public static bool CollectionChecks = true;
        public static int MaxPoolSize = 10;

        private static UnityEngine.Pool.ObjectPool<PoissonDiscData> _pool;
        public static UnityEngine.Pool.ObjectPool<PoissonDiscData> Pool
        {
            get
            {
                if (_pool == null)
                {
                    _pool = new UnityEngine.Pool.ObjectPool<PoissonDiscData>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, maxSize: MaxPoolSize);
                }
                return _pool;
            }
        }

        private static PoissonDiscData CreatePooledItem()
        {
            PoissonDiscData data = new PoissonDiscData();
            return data;
        }


        // Called when an item is returned to the pool using Release
        private static void OnReturnedToPool(PoissonDiscData data)
        {
            data.Clear();
        }

        // Called when an item is taken from the pool using Get
        private static void OnTakeFromPool(PoissonDiscData data)
        {

        }


        private static void OnDestroyPoolObject(PoissonDiscData data)
        {

        }
    }
}

