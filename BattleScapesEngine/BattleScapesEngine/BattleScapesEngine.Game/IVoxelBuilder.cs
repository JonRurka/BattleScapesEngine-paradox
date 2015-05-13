using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;

namespace BattleScapesEngine
{
    public interface IVoxelBuilder : IDisposable
    {
        void SetBlockTypes(BlockType[] _blockTypeList, Rectangle[] _AtlasUvs);
        MeshData Render(bool renderOnly);
        float[,] Generate(int _seed, bool _enableCaves, float _amp, float _caveDensity, float _groundOffset, float _grassOffset);
        void SetBlock(int _x, int _y, int _z, Block block);
        Block GetBlock(int _x, int _y, int _z);
    }
}
