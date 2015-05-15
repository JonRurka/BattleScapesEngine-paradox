using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleScapesEngine
{
    public interface IChunk
    {
        bool Generated { get; }
        IVoxelBuilder Builder { get; }
        void Init(Vector3Int chunkPos, IPageController pageController);
        void createChunkBuilder();
        float[,] GenerateChunk(LibNoise.IModule module);
        void Render(bool renderOnly);
        void Close();
    }
}
