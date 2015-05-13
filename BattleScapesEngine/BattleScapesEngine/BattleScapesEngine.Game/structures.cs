using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;

namespace BattleScapesEngine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3Int : IEquatable<Vector3Int>
    {
        public int x;
        public int y;
        public int z;
        public Vector3Int(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public static Vector3Int[] Vector2ArrayToSVector2Int(Vector3[] inVector)
        {
            Vector3Int[] result = new Vector3Int[inVector.Length];
            for (int i = 0; i < inVector.Length; i++)
            {
                result[i] = inVector[i];
            }
            return result;
        }
        public static Vector3[] SVector3ArrayToVector3(Vector3Int[] inVector)
        {
            Vector3[] result = new Vector3[inVector.Length];
            for (int i = 0; i < inVector.Length; i++)
            {
                result[i] = inVector[i];
            }
            return result;
        }
        public static implicit operator Vector3Int(Vector3 inVector)
        {
            return new Vector3Int((int)inVector.X, (int)inVector.Y, (int)inVector.Z);
        }
        public static implicit operator Vector3(Vector3Int inVector)
        {
            return new Vector3(inVector.x, inVector.y, inVector.z);
        }
        public static bool operator ==(Vector3Int first, Vector3Int second)
        {
            return (first.x == second.x && first.y == second.y && first.z == second.z);
        }
        public static bool operator !=(Vector3Int first, Vector3Int second)
        {
            return (first.x != second.x || first.y != second.y || first.z != second.z);
        }
        public static Vector3Int operator +(Vector3Int first, Vector3Int second)
        {
            Vector3Int result = new Vector3Int();
            result.x = first.x + second.x;
            result.y = first.y + second.y;
            result.z = first.z + second.z;
            return result;
        }
        public static Vector3Int operator *(Vector3Int first, Vector3Int second)
        {
            Vector3Int result = new Vector3Int();
            result.x = first.x * second.x;
            result.y = first.y * second.y;
            result.z = first.z * second.z;
            return result;
        }
        public static float Distance(Vector3Int a, Vector3Int b)
        {
            int deltaX = b.x - a.x;
            int deltaY = b.y - a.y;
            int deltaZ = b.z - a.z;
            return (float)Math.Abs(Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY) + (deltaZ * deltaZ)));
        }
        public override bool Equals(object obj)
        {
            if (obj is Vector3Int)
            {
                return Equals((Vector3Int)obj);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return (x << 16) ^ (y << 8) ^ z;
        }
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }
        public bool Equals(Vector3Int other)
        {
            return (x == other.x && y == other.y && z == other.z);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2Int
    {
        public int x;
        public int y;
        public Vector2Int(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
        public static implicit operator Vector2(Vector2Int inVector)
        {
            return new Vector2(inVector.x, inVector.y);
        }
        public static implicit operator Vector2Int(Vector2 inVector)
        {
            return new Vector2Int((int)inVector.X, (int)inVector.Y);
        }
        public static Vector2Int[] Vector2ArrayToSVector2Int(Vector2[] inVector)
        {
            Vector2Int[] result = new Vector2Int[inVector.Length];
            for (int i = 0; i < inVector.Length; i++)
            {
                result[i] = inVector[i];
            }
            return result;
        }
        public static Vector2[] SVector3ArrayToVector3(Vector2Int[] inVector)
        {
            Vector2[] result = new Vector2[inVector.Length];
            for (int i = 0; i < inVector.Length; i++)
            {
                result[i] = inVector[i];
            }
            return result;
        }
        public static bool operator ==(Vector2Int first, Vector2Int second)
        {
            return (first.x == second.x && first.y == second.y);
        }
        public static bool operator !=(Vector2Int first, Vector2Int second)
        {
            return (first.x != second.x || first.y != second.y);
        }
        public static Vector2Int operator +(Vector2Int first, Vector2Int second)
        {
            Vector2Int result = new Vector2Int();
            result.x = first.x + second.x;
            result.y = first.y + second.y;
            return result;
        }
        public static Vector2Int operator *(Vector2Int first, Vector2Int second)
        {
            Vector2Int result = new Vector2Int();
            result.x = first.x * second.x;
            result.y = first.y * second.y;
            return result;
        }
        public static float Distance(Vector2Int a, Vector2Int b)
        {
            int deltaX = b.x - a.x;
            int deltaY = b.y - a.y;
            return (float)Math.Abs(Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY)));
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return (x << 8) ^ y;
        }
        public override string ToString()
        {
            return string.Format("({0}, {1})", x, y);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] UVs;
        public Vector2[] UVs2;
        public Vector2[] UVs3;
        public MeshData(Vector3[] _vertices, int[] _triangles, Vector2[] _UVs)
        {
            vertices = _vertices;
            triangles = _triangles;
            UVs = _UVs;
            UVs2 = null;
            UVs3 = null;
        }
        public MeshData(Vector3[] _vertices, int[] _triangles, Vector2[] _UVs, Vector2[] _UVs2, Vector2[] _UVs3)
        {
            vertices = _vertices;
            triangles = _triangles;
            UVs = _UVs;
            UVs2 = _UVs2;
            UVs3 = _UVs3;
        }
        public int GetSize()
        {
            int vertSize = vertices.Length * 12;
            int triSize = triangles.Length * 4;
            int uvSize = UVs.Length * 8;
            return vertSize + triSize + uvSize;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Block
    {
        public byte type;
        public float iso;
        public bool set;
        public Block(byte type)
        {
            this.type = type;
            this.iso = 0;
            this.set = false;
        }
        public Block(float iso)
        {
            this.iso = iso;
            this.type = 0;
            this.set = false;
        }
        public Block(byte type, float iso)
        {
            this.iso = iso;
            this.type = type;
            this.set = false;
        }
    }

    public struct BlockType
    {
        public BaseType baseType;
        public byte type;
        public string name;
        public int[] textureIndex;

        public BlockType(BaseType _baseType, byte _type, string _name, int[] _index)
        {
            baseType = _baseType;
            type = _type;
            name = _name;
            textureIndex = _index;
        }
        public override string ToString()
        {
            string UVs = string.Empty;
            for (int i = 0; i < 6; i++)
            {
                UVs += textureIndex[i];
                if (i != 5)
                {
                    UVs += ", ";
                }
            }
            return type.ToString() + "; " + baseType.ToString() + "; " + name + ", " + UVs;
        }
    }

    public enum BaseType
    {
        air,
        solid,
        prefab
    }
}
