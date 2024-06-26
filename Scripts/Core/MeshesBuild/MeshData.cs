using UnityEngine;
using System.Collections.Generic;

namespace PixelMiner.Core
{
    public class MeshData
    {
        public List<Vector3> Vertices { get; private set; }
        public List<int> Triangles { get; private set; }
        public List<Vector3> UVs { get; private set; }
        public List<Vector2> UV2s { get; private set; }
        public List<Vector4> UV3s { get; private set; }
        public List<Vector4> UV4s { get; private set; }
        public List<Vector3> UV5s { get; private set; }
        public List<Color32> Colors { get; private set; }


        // Water
        public List<Vector4> WaterUV2s { get; private set; }
        public List<Vector2> WaterFlowDirMapUVs { get; private set; }
        public List<Vector2> WaterFlowingOffsetUVs { get; private set; }

        public MeshData()                                                                                                                                                                                                                                                                                                                             
        { 
            //Debug.Log("Create MeshData.cs");
            Vertices = new List<Vector3>(4);
            Triangles = new List<int>(6);
            UVs = new List<Vector3>(4);
            UV2s = new List<Vector2>(4);
            UV3s = new List<Vector4>(4);
            UV4s = new List<Vector4>(4);
            UV5s = new List<Vector3>(4);
            Colors = new List<Color32>(4);

            WaterUV2s = new(4);
            WaterFlowDirMapUVs = new(4);
            WaterFlowingOffsetUVs = new(4);
        }

        public void Add(List<Vector3> vertices, List<int> triangles, List<Color32> colors, List<Vector3> uvs, List<Vector2> uv2s, 
            List<Vector4> uv3s, List<Vector4> uv4s, List<Vector3> uv5s)
        {
            Vertices.AddRange(vertices);
            Triangles.AddRange(triangles);
            UVs.AddRange(uvs);
            UV2s.AddRange(uv2s);
            UV3s.AddRange(uv3s);
            UV4s.AddRange(uv4s);
            UV5s.AddRange(uv5s);
            Colors.AddRange(colors);
        }

        public void AddWaterMeshData(List<Vector3> vertices, List<int> triangles, List<Color32> colors, List<Vector3> uvs,
          List<Vector4> uv3s, List<Vector4> uv4s, List<Vector2> waterFlowmapDirUVs, List<Vector2> waterFlowingOffsetUVs)
        {
            Vertices.AddRange(vertices);
            Triangles.AddRange(triangles);
            UVs.AddRange(uvs);
            UV3s.AddRange(uv3s);
            UV4s.AddRange(uv4s);
            Colors.AddRange(colors);

            WaterFlowDirMapUVs.AddRange(waterFlowmapDirUVs);
            WaterFlowingOffsetUVs.AddRange(waterFlowingOffsetUVs);
        }



        //public void AddMeshData(MeshData meshData)
        //{
        //    Vertices.AddRange(meshData.Vertices);
        //    Triangles.AddRange(meshData.Triangles);
        //    UVs.AddRange(meshData.UVs);
        //    UV2s.AddRange(meshData.UV2s);
        //    UV3s.AddRange(meshData.UV3s);
        //    UV4s.AddRange(meshData.UV4s);
        //    UV5s.AddRange(meshData.UV5s);
        //    Colors.AddRange(meshData.Colors);
        //}

        public void Reset()
        {
            Vertices.Clear();
            Triangles.Clear();
            UVs.Clear();
            UV2s.Clear();
            UV3s.Clear();
            UV4s.Clear();
            UV5s.Clear();
            Colors.Clear();

            WaterUV2s.Clear();
            WaterFlowDirMapUVs.Clear();
            WaterFlowingOffsetUVs.Clear();
        }
    }
}
