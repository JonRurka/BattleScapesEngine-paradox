using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;

namespace BattleScapesEngine
{
    public static class VoxelConversions
    {
        public static Vector3Int[] GetCoords(Vector3 location)
        {
            Vector3Int[] result = new Vector3Int[2];
            result[0] = WorldPosToChunkCoord(location);
            //result[1] = ChunkCoordToSuperCoord(result[0]);
            result[1] = GlobalToLocalChunkCoords(result[0]);
            return result;
        }

        public static Vector3 ChunkCoordToWorld(Vector3Int location)
        {
            return new Vector3(location.x * VoxelSettings.ChunkSizeX / VoxelSettings.voxelsPerMeter - VoxelSettings.half, location.y * VoxelSettings.ChunkSizeY / VoxelSettings.voxelsPerMeter - VoxelSettings.half, location.z * VoxelSettings.ChunkSizeZ / VoxelSettings.voxelsPerMeter - VoxelSettings.half);
        }

        public static Vector3Int WorldPosToChunkCoord(Vector3 location)
        {
            return new Vector3Int((int)Math.Round(location.X / VoxelSettings.ChunkSizeX * VoxelSettings.voxelsPerMeter + VoxelSettings.half), (int)Math.Round(location.Y / VoxelSettings.ChunkSizeY * VoxelSettings.voxelsPerMeter + VoxelSettings.half), (int)Math.Round(location.Z / VoxelSettings.ChunkSizeZ * VoxelSettings.voxelsPerMeter + VoxelSettings.half));
        }

        public static Vector3Int GlobalToLocalChunkCoords(Vector3Int location)
        {
            throw new NotImplementedException();
            //Vector3Int super = ChunkCoordToSuperCoord(location);
            //return GlobalToLocalChunkCoords(super, location);
        }

        public static Vector3Int GlobalToLocalChunkCoords(Vector3Int super, Vector3Int location)
        {
            int x = (location.x - (super.x * VoxelSettings.maxChunksX));
            int y = (location.y - (super.y * VoxelSettings.maxChunksY_M));
            int z = (location.z - (super.z * VoxelSettings.maxChunksZ));
            return new Vector3Int(x, y, z);
        }

        public static Vector3Int LocalToGlobalChunkCoords(Vector3Int super, Vector3Int chunk)
        {
            int x = chunk.x + (super.x * VoxelSettings.maxChunksX);
            int y = chunk.y + (super.y * VoxelSettings.maxChunksY_M);
            int z = chunk.z + (super.z * VoxelSettings.maxChunksZ);
            return new Vector3Int(x, y, z);
        }

        public static Vector3Int GlobalVoxToSuperVoxCoord(Vector3Int SuperLocation, Vector3Int location)
        {
            int x = location.x - Math.Abs(SuperLocation.x * VoxelSettings.SuperSizeX);
            int y = location.y - Math.Abs(SuperLocation.y * VoxelSettings.SuperSizeY);
            int z = location.z - Math.Abs(SuperLocation.z * VoxelSettings.SuperSizeZ);
            return new Vector3Int(x, y, z);
        }

        public static Vector3Int GlobalVoxToLocalChunkVoxCoord(Vector3Int location)
        {
            Vector3Int ChunkCoord = VoxelToChunk(location);
            return GlobalVoxToLocalChunkVoxCoord(ChunkCoord, location);
        }

        public static Vector3Int GlobalVoxToLocalChunkVoxCoord(Vector3Int ChunkCoord, Vector3Int location)
        {
            int x = location.x - (ChunkCoord.x * VoxelSettings.ChunkSizeX) + Negative1IfNegative(0, 0);
            int y = location.y - (ChunkCoord.y * VoxelSettings.ChunkSizeY) + Negative1IfNegative(0, 0);
            int z = location.z - (ChunkCoord.z * VoxelSettings.ChunkSizeZ) + Negative1IfNegative(0, 0);
            //return new Vector3Int(location.x >= 0 ? x : x - 1, location.y >= 0 ? y : y - 1, location.z >= 0 ? z : z - 1);
            int offsetX = location.x == -1 ? VoxelSettings.ChunkSizeX - 1 : x;
            int offsetY = location.y == -1 ? VoxelSettings.ChunkSizeY - 1 : y;
            int offsetZ = location.z == -1 ? VoxelSettings.ChunkSizeZ - 1 : z;
            return new Vector3Int(offsetX == 20 ? 0 : offsetX, offsetY == 20 ? 0 : offsetY, offsetZ == 20 ? 0 : offsetZ);
        }

        public static Vector3Int LocalChunkVoxToGlobalVoxCoord(Vector3Int Chunk, Vector3Int location)
        {
            int x = location.x + (Chunk.x / VoxelSettings.ChunkSizeX);
            int y = location.y + (Chunk.y / VoxelSettings.ChunkSizeY);
            int z = location.z + (Chunk.z / VoxelSettings.ChunkSizeZ);
            return new Vector3Int(x, y, z);
        }

        public static Vector3Int VoxelToChunk(Vector3Int location)
        {
            int x = (int)(((location.x - Negative1IfNegative(location.x, 0)) / VoxelSettings.ChunkSizeX)) + Negative1IfNegative(location.x, 0);
            int y = (int)(((location.y - Negative1IfNegative(location.y, 0)) / VoxelSettings.ChunkSizeY)) + Negative1IfNegative(location.y, 0);
            int z = (int)(((location.z - Negative1IfNegative(location.z, 0)) / VoxelSettings.ChunkSizeZ)) + Negative1IfNegative(location.z, 0);
            return new Vector3Int(x, y, z);
        }

        public static Vector3Int ChunkToVoxel(Vector3Int location)
        {
            return new Vector3Int((int)Math.Round((double)location.x * VoxelSettings.ChunkSizeX), (int)Math.Round((double)location.y * VoxelSettings.ChunkSizeY), (int)Math.Round((double)location.z * VoxelSettings.ChunkSizeZ));
        }

        public static Vector3 VoxelToWorld(Vector3Int location)
        {
            return VoxelToWorld(location.x, location.x, location.z);
        }

        public static Vector3 VoxelToWorld(int x, int y, int z)
        {
            float newX = (x / VoxelSettings.voxelsPerMeter) - VoxelSettings.half;
            float newY = (y / VoxelSettings.voxelsPerMeter) - VoxelSettings.half;
            float newZ = (z / VoxelSettings.voxelsPerMeter) - VoxelSettings.half;
            return new Vector3(newX, newY, newZ);
        }

        public static Vector3Int WorldToVoxel(Vector3 worldPos)
        {
            float xOffset = worldPos.X + VoxelSettings.half * VoxelSettings.voxelsPerMeter;
            float yOffset = worldPos.Y + VoxelSettings.half * VoxelSettings.voxelsPerMeter;
            float zOffset = worldPos.Z + VoxelSettings.half * VoxelSettings.voxelsPerMeter;
            int x = xOffset > 0 ? (int)(xOffset) : (int)(xOffset) - 1;
            int y = yOffset > 0 ? (int)(yOffset) : (int)(yOffset) - 1;
            int z = zOffset > 0 ? (int)(zOffset) : (int)(zOffset) - 1;
            //int x = (int)((worldPos.x + (VoxelSettings.half * GetEditValue(worldPos.x))) * VoxelSettings.voxelsPerMeter);
            //int y = (int)((worldPos.y + (VoxelSettings.half * GetEditValue(worldPos.y))) * VoxelSettings.voxelsPerMeter);
            //int z = (int)((worldPos.z + (VoxelSettings.half * GetEditValue(worldPos.z))) * VoxelSettings.voxelsPerMeter);
            return new Vector3Int(x, y, z);
        }

        private static int GetEditValue(float input)
        {
            if (input < 0)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        public static int Negative1IfNegative(int input, int defaultValue)
        {
            if (input < 0)
            {
                return -1;
            }
            else return defaultValue;
        }

        public static int Negative1IfZeroOrLess(int input, int defaultValue)
        {
            if (input <= 0)
            {
                return -1;
            }
            else return defaultValue;
        }
    }
}
