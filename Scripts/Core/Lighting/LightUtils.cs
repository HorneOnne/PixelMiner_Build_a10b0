using PixelMiner.Enums;
using UnityEngine;
using System.Collections.Generic;

namespace PixelMiner.Core
{
    public class LightUtils : MonoBehaviour
    {
        public static LightUtils Instance { get; private set; }
        public const byte  MAX_LIGHT_INTENSITY = 15;

        private Dictionary<BlockID, byte> _lightResistanceMap = new Dictionary<BlockID, byte>
        {  
            { BlockID.DirtGrass, 15 },
            { BlockID.Dirt, 15 },
            { BlockID.Stone, 15 },
   
            { BlockID.Sand, 15 },
            { BlockID.SandMine, 15 },
            { BlockID.SnowDirtGrass, 15 },
   
          
            { BlockID.Wood, 15 },        
            { BlockID.PineWood, 15 },        
            { BlockID.Leaves, 3 },        
            { BlockID.PineLeaves, 3 },   
            

            { BlockID.Light, 1 },        
            { BlockID.Lava, 1 },        
        };

        private Dictionary<BlockID, LightColor> _lightMap = new Dictionary<BlockID, LightColor>
        {
            { BlockID.Light, new LightColor() {Red = 15, Green = 15, Blue = 10, Intensity = 1 } },
            { BlockID.RedLight, new LightColor() {Red = 15, Green = 0, Blue = 0, Intensity = 1 } },
            { BlockID.GreenLight, new LightColor() {Red = 0, Green = 15, Blue = 0, Intensity = 1 } },
            { BlockID.BlueLight, new LightColor() {Red = 0, Green = 0, Blue = 15, Intensity = 1 } },
            { BlockID.Lava, new LightColor() {Red = 15, Green = 6, Blue = 0, Intensity = 1 } },

        };


        public static byte[] BlocksLightResistance = new byte[(int)BlockID.Count];
        public static LightColor[] BlocksLight = new LightColor[(int)BlockID.Count];


        private void Awake()
        {
            Instance = this;

            // Light
            for (int i = 0; i < BlocksLight.GetLength(0); i++)
            {
                BlocksLight[i] = new LightColor() { Red = 0, Green = 0, Blue = 0 };
            }

            foreach (var b in _lightMap)
            {
                BlocksLight[(ushort)b.Key] = b.Value;
            }



            // Light resistance
            for (int i = 0; i < BlocksLightResistance.GetLength(0); i++)
            {
                BlocksLightResistance[i] = 1;
            }
            foreach (var opaqueValue in _lightResistanceMap)
            {
                BlocksLightResistance[(byte)opaqueValue.Key] = opaqueValue.Value;
            }
        }


    
        public static ushort GetLightData(byte sun, byte red, byte green, byte blue)
        {
            ushort lightData = 0;
            lightData = (ushort)((lightData & 0x0FFF) | (sun << 12));
            lightData = (ushort)((lightData & 0xF0FF) | (red << 8));
            lightData = (ushort)((lightData & 0xFF0F) | (green << 4));
            lightData = (ushort)((lightData & 0xFFF0) | blue);
            return lightData;
        }

        public static void SetSunLight(ref ushort lightData, byte sunLight)
        {
            lightData = (ushort)((lightData & 0x0FFF) | (sunLight << 12));
        }

        public static void SetRedLight(ref ushort lightData, byte red)
        {
            lightData = (ushort)((lightData & 0xF0FF) | (red << 8));
        }

        public static void SetGreenLight(ref ushort lightData, byte green)
        {
            lightData = (ushort)((lightData & 0xFF0F) | (green << 4));
        }

        public static void SetBlueLight(ref ushort lightData, byte blue)
        {
            lightData = (ushort)((lightData & 0xFFF0) | blue);
        }
    }
}


