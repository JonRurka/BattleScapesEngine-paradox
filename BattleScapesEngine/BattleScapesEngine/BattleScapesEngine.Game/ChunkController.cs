using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleScapesEngine
{
    public class ChunkController : IPageController
    {
        public SafeDictionary<Vector3Int, IChunk> Chunks;

        public ChunkController()
        {
            Chunks = new SafeDictionary<Vector3Int, IChunk>();
        }

        public bool BuilderExists(int x, int y, int z)
        {
            return Chunks.ContainsKey(new Vector3Int(x, y, z));
        }

        public bool BuilderGenerated(int x, int y, int z)
        {
            if (BuilderExists(x, y, z))
                return Chunks[new Vector3Int(x, y, z)].Generated;
            return false;
        }

        public IVoxelBuilder GetBuilder(int x, int y, int z)
        {
            IVoxelBuilder result = null;
            if (BuilderExists(x, y, z))
            {
                result = Chunks[new Vector3Int(x, y, z)].Builder;
            }
            return result;
        }

        public IChunk GetChunk(int x, int y, int z)
        {
            IChunk result = null;
            if (BuilderExists(x, y, z))
            {
                result = Chunks[new Vector3Int(x, y, z)];
            }
            return result;
        }

        public void UpdateChunk(int x, int y, int z)
        {
            if (BuilderExists(x, y, z))
            {
                Chunks[new Vector3Int(x, y, z)].Render(true);
            }
        }

        public void Add(Vector3Int location, IChunk chunk)
        {
            if (!BuilderExists(location.x, location.y, location.z))
                Chunks.Add(location, chunk);
        }
    }
}
