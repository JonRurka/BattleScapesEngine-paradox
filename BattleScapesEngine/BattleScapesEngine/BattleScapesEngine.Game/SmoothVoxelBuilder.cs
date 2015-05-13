using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LibNoise;
using LibNoise.Models;
using LibNoise.Modifiers;
using SiliconStudio.Core.Mathematics;

namespace BattleScapesEngine
{
    public class SmoothVoxelBuilder : IVoxelBuilder
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Neighbor
        {
            public bool Exists;
            public bool generated;
            public Vector3Int pagePositon;
            public IVoxelBuilder controller;
            public Neighbor(bool _exists, bool _generated, Vector3Int _location, IVoxelBuilder _controller)
            {
                Exists = _exists;
                generated = _generated;
                pagePositon = _location;
                controller = _controller;
            }
        }

        public Block[][][] blocks;
        public float[,] SurfaceData;
        public IPageController controller;
        public BlockType[] BlockTypes;
        public Rectangle[] AtlasUvs;
        public Neighbor[] neighbors;
        public bool Initialized;
        public bool deactivated;
        static int[] edgeTable = new int[256]
    {0x0 , 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
    0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
    0x190, 0x99 , 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
    0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
    0x230, 0x339, 0x33 , 0x13a, 0x636, 0x73f, 0x435, 0x53c,
    0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
    0x3a0, 0x2a9, 0x1a3, 0xaa , 0x7a6, 0x6af, 0x5a5, 0x4ac,
    0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
    0x460, 0x569, 0x663, 0x76a, 0x66 , 0x16f, 0x265, 0x36c,
    0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
    0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff , 0x3f5, 0x2fc,
    0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
    0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55 , 0x15c,
    0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
    0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc ,
    0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
    0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
    0xcc , 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
    0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
    0x15c, 0x55 , 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
    0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
    0x2fc, 0x3f5, 0xff , 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
    0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
    0x36c, 0x265, 0x16f, 0x66 , 0x76a, 0x663, 0x569, 0x460,
    0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
    0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa , 0x1a3, 0x2a9, 0x3a0,
    0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
    0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33 , 0x339, 0x230,
    0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
    0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99 , 0x190,
    0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
    0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0};
        static int[][] triTable = new int[256][]
    {
        new int[16]{-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1},
        new int[16]{8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1},
        new int[16]{3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1},
        new int[16]{4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1},
        new int[16]{4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1},
        new int[16]{9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1},
        new int[16]{10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1},
        new int[16]{5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1},
        new int[16]{5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1},
        new int[16]{8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1},
        new int[16]{2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1},
        new int[16]{2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1},
        new int[16]{11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1},
        new int[16]{5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1},
        new int[16]{11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1},
        new int[16]{11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1},
        new int[16]{2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1},
        new int[16]{6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1},
        new int[16]{3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1},
        new int[16]{6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1},
        new int[16]{6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1},
        new int[16]{8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1},
        new int[16]{7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1},
        new int[16]{3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1},
        new int[16]{0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1},
        new int[16]{9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1},
        new int[16]{8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1},
        new int[16]{5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1},
        new int[16]{0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1},
        new int[16]{6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1},
        new int[16]{10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1},
        new int[16]{1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1},
        new int[16]{0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1},
        new int[16]{3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1},
        new int[16]{6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1},
        new int[16]{9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1},
        new int[16]{8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1},
        new int[16]{3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1},
        new int[16]{10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1},
        new int[16]{10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1},
        new int[16]{2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1},
        new int[16]{7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1},
        new int[16]{2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1},
        new int[16]{1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1},
        new int[16]{11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1},
        new int[16]{8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1},
        new int[16]{0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1},
        new int[16]{7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1},
        new int[16]{7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1},
        new int[16]{10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1},
        new int[16]{0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1},
        new int[16]{7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1},
        new int[16]{6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1},
        new int[16]{4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1},
        new int[16]{10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1},
        new int[16]{8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1},
        new int[16]{1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1},
        new int[16]{10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1},
        new int[16]{10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1},
        new int[16]{9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1},
        new int[16]{7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1},
        new int[16]{3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1},
        new int[16]{7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1},
        new int[16]{3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1},
        new int[16]{6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1},
        new int[16]{9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1},
        new int[16]{1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1},
        new int[16]{4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1},
        new int[16]{7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1},
        new int[16]{6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1},
        new int[16]{0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1},
        new int[16]{6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1},
        new int[16]{0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1},
        new int[16]{11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1},
        new int[16]{6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1},
        new int[16]{5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1},
        new int[16]{9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1},
        new int[16]{1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1},
        new int[16]{10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1},
        new int[16]{0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1},
        new int[16]{11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1},
        new int[16]{9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1},
        new int[16]{7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1},
        new int[16]{2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1},
        new int[16]{9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1},
        new int[16]{9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1},
        new int[16]{1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1},
        new int[16]{0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1},
        new int[16]{10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1},
        new int[16]{2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1},
        new int[16]{0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1},
        new int[16]{0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1},
        new int[16]{9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1},
        new int[16]{5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1},
        new int[16]{5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1},
        new int[16]{8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1},
        new int[16]{9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1},
        new int[16]{1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1},
        new int[16]{3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1},
        new int[16]{4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1},
        new int[16]{9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1},
        new int[16]{11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1},
        new int[16]{2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1},
        new int[16]{9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1},
        new int[16]{3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1},
        new int[16]{1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1},
        new int[16]{4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1},
        new int[16]{0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1},
        new int[16]{1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        new int[16]{-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}};
        static Vector3Int[] offsets = new Vector3Int[8]
    {
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 0, 0),
        new Vector3Int(0, 1, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 1, 0),
    };

        public Vector3[] locOffset;

        public int VoxelsPerMeter;
        public int ChunkMeterSizeX;
        public int ChunkMeterSizeY;
        public int ChunkMeterSizeZ;
        public int ChunkSizeX;
        public int ChunkSizeY;
        public int ChunkSizeZ;
        public float half;
        public float sideLength;

        public int blocksFilled = 0;
        public int generatedInside = 0;
        public int generatedOutside = 0;
        public int allParsed = 0;

        public Vector3Int location;

        public int seed;
        public float heightmapSize;
        public bool enableCaves;
        public float amp;
        public float caveDensity;
        public float groundOffset;
        public float grassOffset;

        public IModule NoiseModule;
        public IModule caveModule;
        public Noise2D NoisePlane;

        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        public string time;

        public SmoothVoxelBuilder(IPageController _controller)
        {
            controller = _controller;
            location = new Vector3Int();
            VoxelsPerMeter = 1;
            ChunkMeterSizeX = 10;
            ChunkMeterSizeY = 10;
            ChunkMeterSizeZ = 10;
            CalculateVariables();
        }

        public SmoothVoxelBuilder(IPageController _controller, Vector3Int _Location, int _VoxelsPerMeter, int _ChunkMeterSize)
        {
            controller = _controller;
            location = _Location;
            VoxelsPerMeter = _VoxelsPerMeter;
            ChunkMeterSizeX = _ChunkMeterSize;
            ChunkMeterSizeY = _ChunkMeterSize;
            ChunkMeterSizeZ = _ChunkMeterSize;
            CalculateVariables();
        }

        public SmoothVoxelBuilder(IPageController _controller, Vector3Int _Location, int _VoxelsPerMeter, int _ChunkMeterSizeX, int _ChunkMeterSizeY, int _ChunkMeterSizeZ)
        {
            controller = _controller;
            location = _Location;
            VoxelsPerMeter = _VoxelsPerMeter;
            ChunkMeterSizeX = _ChunkMeterSizeX;
            ChunkMeterSizeY = _ChunkMeterSizeY;
            ChunkMeterSizeZ = _ChunkMeterSizeZ;
            CalculateVariables();
        }

        public void SetBlockTypes(BlockType[] _blockTypeList, SiliconStudio.Core.Mathematics.Rectangle[] _AtlasUvs)
        {
            BlockTypes = _blockTypeList;
            AtlasUvs = _AtlasUvs;
        }

        public MeshData Render(bool renderOnly)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> UVs = new List<Vector2>();
            if (Initialized)
            {
                watch.Start();
                SetSurroundingChunks();
                int cx = location.x;
                int cy = location.y;
                int cz = location.z;

                int xStart = cx * ChunkSizeX;
                int xEnd = cx * ChunkSizeX + ChunkSizeX;

                int yStart = cy * ChunkSizeY;
                int yEnd = cy * ChunkSizeY + ChunkSizeY;

                int zStart = cz * ChunkSizeZ;
                int zEnd = cz * ChunkSizeZ + ChunkSizeZ;

                int x = 0;
                int y = 0;
                int z = 0;

                int superLocX = 0;
                int superLocY = 0;
                int superLocZ = 0;
                try
                {
                    for (superLocX = xStart, x = 0; x < ChunkSizeX; superLocX++, x++)
                    {
                        for (superLocZ = zStart, z = 0; z < ChunkSizeZ; superLocZ++, z++)
                        {
                            for (superLocY = yStart, y = 0; y < ChunkSizeY; superLocY++, y++)
                            {
                                if (!deactivated)
                                {
                                    Vector3 worldPos = new Vector3(x * sideLength, y * sideLength, z * sideLength);
                                    Vector3Int globalPos = new Vector3Int(superLocX, superLocY, superLocZ);
                                    Vector3Int localPos = new Vector3Int(x, y, z);
                                    Vector4[] grid = new Vector4[8];
                                    for (int i = 0; i < grid.Length; i++)
                                        grid[i] = GetVector4(worldPos + locOffset[i], localPos + offsets[i], globalPos + offsets[i], !renderOnly);
                                    RenderBlock(grid, 0, vertices, triangles, UVs, null);
                                }
                                else
                                    return new MeshData(vertices.ToArray(), triangles.ToArray(), UVs.ToArray());
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    /*SafeDebug.LogError("Message: " + e.Message + ", \nFunction: Render,\nLocation: " + location +
                                        string.Format("\nglobal: {0}/{1}, {2}/{3}, {4}/{5}\nlocal: {6}/{7}, {8}/{9}, {10}/{11}",
                                        superLocX, VoxelSettings.SuperSizeX, superLocY, VoxelSettings.SuperSizeY, superLocZ, VoxelSettings.SuperSizeZ, x, ChunkSizeX, y, ChunkSizeY, z, ChunkSizeZ), e);*/
                }
            }
            watch.Stop();
            time = watch.Elapsed.ToString();
            return new MeshData(vertices.ToArray(), triangles.ToArray(), UVs.ToArray());
        }

        public void RenderBlock(Vector4[] grid, float isoLevel, List<Vector3> vertices, List<int> triangles, List<Vector2> uv, int[] _textureIndex)
        {
            int cubeIndex = 0;
            Vector3[] vertList = new Vector3[12];

            if (grid[0].W > isoLevel) cubeIndex |= 1;
            if (grid[1].W > isoLevel) cubeIndex |= 2;
            if (grid[2].W > isoLevel) cubeIndex |= 4;
            if (grid[3].W > isoLevel) cubeIndex |= 8;
            if (grid[4].W > isoLevel) cubeIndex |= 16;
            if (grid[5].W > isoLevel) cubeIndex |= 32;
            if (grid[6].W > isoLevel) cubeIndex |= 64;
            if (grid[7].W > isoLevel) cubeIndex |= 128;

            if (edgeTable[cubeIndex] == 0)
                return;

            if ((edgeTable[cubeIndex] & 1) != 0)
                vertList[0] = VertexInterp(isoLevel, grid[0], grid[1]);
            if ((edgeTable[cubeIndex] & 2) != 0)
                vertList[1] = VertexInterp(isoLevel, grid[1], grid[2]);
            if ((edgeTable[cubeIndex] & 4) != 0)
                vertList[2] = VertexInterp(isoLevel, grid[2], grid[3]);
            if ((edgeTable[cubeIndex] & 8) != 0)
                vertList[3] = VertexInterp(isoLevel, grid[3], grid[0]);
            if ((edgeTable[cubeIndex] & 16) != 0)
                vertList[4] = VertexInterp(isoLevel, grid[4], grid[5]);
            if ((edgeTable[cubeIndex] & 32) != 0)
                vertList[5] = VertexInterp(isoLevel, grid[5], grid[6]);
            if ((edgeTable[cubeIndex] & 64) != 0)
                vertList[6] = VertexInterp(isoLevel, grid[6], grid[7]);
            if ((edgeTable[cubeIndex] & 128) != 0)
                vertList[7] = VertexInterp(isoLevel, grid[7], grid[4]);
            if ((edgeTable[cubeIndex] & 256) != 0)
                vertList[8] = VertexInterp(isoLevel, grid[0], grid[4]);
            if ((edgeTable[cubeIndex] & 512) != 0)
                vertList[9] = VertexInterp(isoLevel, grid[1], grid[5]);
            if ((edgeTable[cubeIndex] & 1024) != 0)
                vertList[10] = VertexInterp(isoLevel, grid[2], grid[6]);
            if ((edgeTable[cubeIndex] & 2048) != 0)
                vertList[11] = VertexInterp(isoLevel, grid[3], grid[7]);


            for (int i = 0; triTable[cubeIndex][i] != -1; i += 3)
            {
                int triCount = triangles.Count;
                vertices.Add(vertList[triTable[cubeIndex][i]]);
                vertices.Add(vertList[triTable[cubeIndex][i + 1]]);
                vertices.Add(vertList[triTable[cubeIndex][i + 2]]);

                triangles.Add(triCount + 2);
                triangles.Add(triCount + 1);
                triangles.Add(triCount);
            }
        }

        public float[,] Generate(IModule module, int _seed, bool _enableCaves, float _amp, float _caveDensity, float _groundOffset, float _grassOffset)
        {
            try
            {
                NoiseModule = module;
                seed = _seed;
                enableCaves = _enableCaves;
                amp = _amp;
                caveDensity = _caveDensity;
                groundOffset = _groundOffset;
                grassOffset = _grassOffset;

                RidgedMultifractal _caves = new RidgedMultifractal();
                _caves.Seed = _seed;
                _caves.Frequency = 0.35;
                caveModule = _caves;

                Vector2Int bottomLeft = new Vector2(location.x * ChunkSizeX, location.z * ChunkSizeZ);
                Vector2Int topRight = new Vector2(location.x * ChunkSizeX + ChunkSizeX, location.z * ChunkSizeZ + ChunkSizeZ);

                SetSurfaceData(bottomLeft, topRight);
            }
            catch (Exception e)
            {
                //SafeDebug.LogError(string.Format("{0}\nFunction: Generate\n Chunk: {1}", e.Message, location.ToString()), e);
            }

            return SurfaceData;
        }

        public float[,] Generate(int _seed, bool _enableCaves, float _amp, float _caveDensity, float _groundOffset, float _grassOffset)
        {
            try
            {
                seed = _seed;
                enableCaves = _enableCaves;
                amp = _amp;
                caveDensity = _caveDensity;
                groundOffset = _groundOffset;
                grassOffset = _grassOffset;

                Vector2Int bottomLeft = new Vector2(location.x * ChunkSizeX, location.z * ChunkSizeZ);
                Vector2Int topRight = new Vector2(location.x * ChunkSizeX + ChunkSizeX, location.z * ChunkSizeZ + ChunkSizeZ);

                MazeGen mountainTerrain = new MazeGen(ChunkSizeX / 2, 4, ChunkMeterSizeY / 2, seed, 2, 1);

                NoiseModule = mountainTerrain;

                //NoisePlane = new LibNoise.Models.Plane(NoiseModule);

                //SetSurfaceData(bottomLeft, topRight);
            }
            catch (Exception e)
            {
                //SafeDebug.LogError(e.Message + "\nFunction: Generate, Chunk: " + location.ToString(), e);
            }

            return SurfaceData;
        }

        public float GetIsoValue(int LocalPositionX, int LocalPositionY, int LocalPositionZ, int globalX, int globalY, int globalZ, bool generate)
        {
            return GetIsoValue(new Vector3Int(LocalPositionX, LocalPositionY, LocalPositionZ), new Vector3Int(globalX, globalY, globalZ), generate);
        }

        public Vector4 GetVector4(Vector3 world, Vector3Int local, Vector3Int global, bool generate)
        {
            //Vector3 origin = new Vector3(ChunkSizeX / 2, ChunkSizeY / 2, ChunkSizeX / 2);
            //return new Vector4(world.x, world.y, world.z, Vector3.Distance(origin, world));

            Vector3Int chunk = VoxelConversions.VoxelToChunk(global);
            if (generate)
            {

                if (IsInBounds(local.x, local.y, local.z))
                {
                    return new Vector4(world.X, world.Y, world.Z, GetIsoValue(local, global, generate));
                }
                else
                {
                    //return new Vector4(world.x, world.y, world.z, 5);
                    return new Vector4(world.X, world.Y, world.Z, GetIsoValue(local, global));
                }
            }
            else
            {
                if (chunk != location)
                {
                    Vector3Int chunklocalVoxel = VoxelConversions.GlobalVoxToLocalChunkVoxCoord(chunk, global);
                    IVoxelBuilder builder = controller.GetBuilder(chunk.x, chunk.y, chunk.z);
                    if (builder != null)
                    {
                        return new Vector4(world.X, world.Y, world.Z, builder.GetBlock(chunklocalVoxel.x, chunklocalVoxel.y, chunklocalVoxel.z).iso);
                    }
                    else
                    {
                        return new Vector4(world.X, world.Y, world.Z, -1);
                    }
                }
                else
                {
                    return new Vector4(world.X, world.Y, world.Z, GetIsoValue(local, global, generate));
                }
            }
        }

        public float GetIsoValue(Vector3Int LocalPosition, Vector3Int globalLocation, bool generate)
        {
            try
            {
                if (generate && !IsBlockSet(LocalPosition.x, LocalPosition.y, LocalPosition.z))
                {
                    generatedInside++;
                    float generatedValue = GetIsoValue(LocalPosition.x, LocalPosition.y, LocalPosition.z, globalLocation.x, globalLocation.y, globalLocation.z);
                    SetBlock(LocalPosition.x, LocalPosition.y, LocalPosition.z, new Block(1, generatedValue));
                    MarkAsSet(LocalPosition.x, LocalPosition.y, LocalPosition.z);
                }
                return GetBlock(LocalPosition.x, LocalPosition.y, LocalPosition.z).iso;
            }
            catch (Exception e)
            {
                //SafeDebug.LogError(string.Format("Message: {0}\nglobalX={1}, globalZ={2}\nGenerate: {3}", e.Message, globalLocation.x, globalLocation.z, generate.ToString()), e);
                return 0;
            }
        }

        public float GetIsoValue(int LocalPositionX, int LocalPositionY, int LocalPositionZ, int globalX, int globalY, int globalZ)
        {
            return GetIsoValue(new Vector3Int(LocalPositionX, LocalPositionY, LocalPositionZ), new Vector3Int(globalX, globalY, globalZ));
        }

        public float GetIsoValue(Vector3Int LocalPosition, Vector3Int globalLocation)
        {
            float result = -1;
            try
            {
                float surfaceHeight = GetSurfaceHeight(LocalPosition.x, LocalPosition.z) + VoxelSettings.groundOffset;
                result = surfaceHeight - globalLocation.y;

                if (enableCaves && Noise(caveModule, globalLocation.x, globalLocation.y, globalLocation.z, 16.0 * VoxelsPerMeter, 17.0, 1.0) > caveDensity)
                    result = 0;
            }
            catch (Exception e)
            {
                //SafeDebug.LogError(string.Format("Message: {0}\nglobalX={1}, globalZ={2}\nlocalX={3}/{4}, localZ={5}/{6}",
                //    e.Message, globalLocation.x, globalLocation.z, LocalPosition.x, SurfaceData.GetLength(0), LocalPosition.z, SurfaceData.GetLength(1)), e);
            }
            return result;
        }

        public void MarkAsSet(int _x, int _y, int _z)
        {
            if (!deactivated)
                //if (IsInBounds(_x, _y, _z))
                //{
                blocks[_x][_y][_z].set = true;
            /*}
            else
            {
                SafeDebug.LogError(string.Format("Error: {0}/{1}, {2}/{3}, {4}/{5} is out of range", _x, ChunkSizeX, _y, ChunkSizeY, _z, ChunkSizeZ));
            }*/
        }

        public void SetBlock(int _x, int _y, int _z, Block block)
        {
            if (!deactivated)
            {
                //if (IsInBounds(_x, _y, _z))
                //{
                blocks[_x][_y][_z].type = block.type;
                blocks[_x][_y][_z].iso = block.iso;
                /*}
                else
                {
                    SafeDebug.LogError(string.Format("Error: {0}/{1}, {2}/{3}, {4}/{5} is out of range. Function: SetBlock", _x, ChunkSizeX, _y, ChunkSizeY, _z, ChunkSizeZ));
                }*/
            }
        }

        public Block GetBlock(int _x, int _y, int _z)
        {
            if (!deactivated)
            {
                //if (IsInBounds(_x, _y, _z))
                //{
                return blocks[_x][_y][_z];
                /*}
                else
                {
                    SafeDebug.LogError(string.Format("Error: {0}/{1}, {2}/{3}, {4}/{5} is out of range. Function: GetBlock", _x, ChunkSizeX, _y, ChunkSizeY, _z, ChunkSizeZ));
                }*/
            }
            return default(Block);
        }

        public void SetValueValue(int _x, int _y, int _z, float value)
        {
            if (!deactivated)
            {
                //if (IsInBounds(_x, _y, _z))
                //{
                blocks[_x][_y][_z].iso = value;
                /*}
                else
                {
                    SafeDebug.LogError(string.Format("Error: {0}/{1}, {2}/{3}, {4}/{5} is out of range. Function: SetBlock", _x, ChunkSizeX, _y, ChunkSizeY, _z, ChunkSizeZ));
                }*/
            }
        }

        public float GetBlockValue(int _x, int _y, int _z)
        {
            if (!deactivated)
            {
                //if (IsInBounds(_x, _y, _z))
                //{
                return blocks[_x][_y][_z].iso;
                /*}
                else
                {
                    SafeDebug.LogError(string.Format("Error: {0}/{1}, {2}/{3}, {4}/{5} is out of range. Function: GetBlock", _x, ChunkSizeX, _y, ChunkSizeY, _z, ChunkSizeZ));
                }*/
            }
            return 0;
        }

        public bool IsBlockSet(int _x, int _y, int _z)
        {
            if (!deactivated)
                //if (IsInBounds(_x, _y, _z))
                //{
                return blocks[_x][_y][_z].set;
            /*}
            else
            {
                SafeDebug.LogError(string.Format("Error: {0}/{1}, {2}/{3}, {4}/{5} is out of range. Function: IsBlockSet", _x, ChunkSizeX, _y, ChunkSizeY, _z, ChunkSizeZ));
                return false;
            }*/
            return false;
        }

        public float GetSurfaceHeight(int LocalX, int LocalZ)
        {
            return SurfaceData[LocalX + 1, LocalZ + 1]; ;
        }

        public int Noise(IModule module, int _x, int _y, int _z, double _scale, double _height, double _power)
        {
            double rValue = 0;
            if (module != null)
            {
                rValue = module.GetValue(((double)_x) / _scale, ((double)_y) / _scale, ((double)_z) / _scale);
                rValue *= _height;

                if (_power != 0)
                {
                    rValue = System.Math.Pow((float)rValue, (float)_power);
                }
            }
            return (int)rValue;
        }

        public void Dispose()
        {
            deactivated = true;
            if (NoisePlane != null)
            {
                NoisePlane.Clear();
                NoisePlane.Dispose();
            }
            NoisePlane = null;
            NoiseModule = null;
            caveModule = null;
        }

        private bool IsInBounds(int x, int y, int z)
        {
            return ((x <= ChunkSizeX - 1) && x >= 0) && ((y <= ChunkSizeY - 1) && y >= 0) && ((z <= ChunkSizeZ - 1) && z >= 0);
        }

        private Vector3 VertexInterp(float isoLevel, Vector4 p1, Vector4 p2)
        {
            Vector3 point = new Vector3();
            if (System.Math.Abs(isoLevel - p1.W) < 0.00001f)
                return p1.XYZ();
            if (System.Math.Abs(isoLevel - p2.W) < 0.00001f)
                return p2.XYZ();
            if (System.Math.Abs(p1.W - p2.W) < 0.00001f)
                return p1.XYZ();
            float mu = (isoLevel - p1.W) / (p2.W - p1.W);
            point.X = p1.X + mu * (p2.X - p1.X);
            point.Y = p1.Y + mu * (p2.Y - p1.Y);
            point.Z = p1.Z + mu * (p2.Z - p1.Z);
            return point;
        }

        private void SetSurfaceData(Vector2Int bottomLeft, Vector2Int topRight)
        {
            try
            {
                for (int noiseX = bottomLeft.x - 1, x = 0; noiseX < topRight.x + 1; noiseX++, x++)
                {
                    for (int noiseZ = bottomLeft.y - 1, z = 0; noiseZ < topRight.y + 1; noiseZ++, z++)
                    {
                        SurfaceData[x, z] = GetHeight(noiseX, noiseZ);
                    }
                }
            }
            catch (Exception e)
            {
                //SafeDebug.LogError(e.Message + "\nFunction: SetSurfaceData", e);
            }
        }

        private float GetHeight(int x, int y)
        {
            if (NoiseModule != null)
                return (((float)NoiseModule.GetValue((double)x / 150, 0, (double)y / 150) * amp) + groundOffset);
            return 0;
        }

        private void CalculateVariables()
        {
            ChunkSizeX = ChunkMeterSizeX * VoxelsPerMeter;
            ChunkSizeY = ChunkMeterSizeY * VoxelsPerMeter;
            ChunkSizeZ = ChunkMeterSizeZ * VoxelsPerMeter;
            half = ((1.0f / (float)VoxelsPerMeter) / 2.0f);
            sideLength = 1.0f / (float)VoxelsPerMeter;
            locOffset = new Vector3[8]
        {
            new Vector3(0,0,sideLength),
            new Vector3(sideLength,0,sideLength),
            new Vector3(sideLength,0,0),
            new Vector3(0,0,0),
            new Vector3(0,sideLength,sideLength),
            new Vector3(sideLength,sideLength,sideLength),
            new Vector3(sideLength,sideLength,0),
            new Vector3(0,sideLength,0),
        };
            AllocateBlockArray(ChunkSizeX, ChunkSizeY, ChunkSizeZ);
            SetSurroundingChunks();
            Initialized = true;
        }

        private void SetSurroundingChunks()
        {
            neighbors = new Neighbor[6];
            Setpage(0, new Vector3Int(location.x, location.y + 1, location.z));
            Setpage(1, new Vector3Int(location.x, location.y - 1, location.z));
            Setpage(2, new Vector3Int(location.x, location.y, location.z + 1));
            Setpage(3, new Vector3Int(location.x, location.y, location.z - 1));
            Setpage(4, new Vector3Int(location.x + 1, location.y, location.z));
            Setpage(5, new Vector3Int(location.x - 1, location.y, location.z));
        }

        private void Setpage(int index, Vector3Int pageLoc)
        {
            if (controller.BuilderExists(pageLoc.x, pageLoc.y, pageLoc.z))
            {
                IVoxelBuilder _builder = controller.GetBuilder(pageLoc.x, pageLoc.y, pageLoc.z);
                if (_builder != null)
                {
                    neighbors[index] = new Neighbor(true, controller.BuilderGenerated(pageLoc.x, pageLoc.y, pageLoc.z), pageLoc, _builder);
                }
                else
                {
                    //SafeDebug.LogError("Controller says chunk builder exists, but chunk builder is null!.");
                    neighbors[index] = new Neighbor(false, false, pageLoc, null);
                }
            }
            else
            {
                neighbors[index] = new Neighbor(false, false, pageLoc, null);
            }
        }

        private void AllocateBlockArray(int sizeX, int sizeY, int sizeZ)
        {
            blocks = new Block[sizeX][][];
            SurfaceData = new float[sizeX + 2, sizeZ + 2];
            for (int i = 0; i < sizeX; ++i)
            {
                blocks[i] = new Block[sizeY][];
                for (int j = 0; j < sizeY; ++j)
                {
                    blocks[i][j] = new Block[sizeZ];
                }
            }
        }
    }
}
