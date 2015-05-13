using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleScapesEngine
{
    class ChunkController : IPageController
    {
        TerrainController terrainControll;

        public bool BuilderExists(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public bool BuilderGenerated(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public IVoxelBuilder GetBuilder(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public void UpdateChunk(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public void AddBlockType(BaseType _baseType, string _name, int[] _textures)
        {
            byte index = (byte)TerrainController.blockTypes.Count;
            TerrainController.blockTypes.Add(index, new BlockType(_baseType, index, _name, _textures));
            terrainControll.BlocksArray = TerrainController.GetBlockTypeArray(TerrainController.blockTypes.Values);
        }
    }
}
