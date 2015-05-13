using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleScapesEngine
{
    public static class VoxelSettings
    {
        // world settings.
        public static int seed = 0;
        public static int radius = 10;
        public static bool randomSeed = true;
        public static bool CircleGen = false;
        public static bool enableCaves = true;
        public static float caveDensity = 14;
        public static float amplitude = 100;
        public static float groundOffset = 10;
        public static float grassOffset = 4;
        public static int maxSuperChunksX = 1;
        public static int maxSuperChunksY = 1;
        public static int maxSuperChunksZ = 1;
        public static int ViewDistanceX = 5;
        public static int ViewDistanceY = 5;
        public static int ViewDistanceZ = 5;
        public static int ViewRadius = 2;

        // chunk settings.
        public static int voxelsPerMeter = 1;
        public static int MeterSizeX = 20;
        public static int MeterSizeY = 20;
        public static int MeterSizeZ = 20;
        public static int ChunkSizeX = MeterSizeX * voxelsPerMeter;
        public static int ChunkSizeY = MeterSizeY * voxelsPerMeter;
        public static int ChunkSizeZ = MeterSizeZ * voxelsPerMeter;
        public static float half = ((1f / (float)voxelsPerMeter) / 2f);

        // Super chunks settings.
        public static int maxChunksX = 10;
        public static int maxChunksY_M = 2;
        public static int maxChunksY_P = 3;
        public static int maxChunksZ = 10;
        public static int SuperSizeX = ChunkSizeX * maxChunksX;
        public static int SuperSizeY = ChunkSizeY * maxChunksY_M;
        public static int SuperSizeZ = ChunkSizeZ * maxChunksZ;
    }
}
