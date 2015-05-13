using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Graphics;

namespace BattleScapesEngine
{
    public class TerrainController : Script
    {
        public static TerrainController Instance;
        public IPageController pageController;
        public int seed = 0;
        public Entity player;
        public Texture[] AllCubeTextures;
        public Texture textureAtlas;
        public Rectangle[] AtlasUvs;
        public BlockType[] BlocksArray;

        public static Dictionary<byte, BlockType> blockTypes = new Dictionary<byte, BlockType>();
        public static SafeDictionary<Vector3Int, SmoothChunk> Chunks = new SafeDictionary<Vector3Int, SmoothChunk>();

        public override void Start()
        {
            base.Start();
            if (Instance != null)
            {
                Instance = this;
            }
            else
            {
                //Debug.Log("Only one terrain controller allowed per scene. Destroying duplicate.");
                //Destroy(this);
                Entity.Dispose();
            }
        }

        public static void Init()
        {
            Instance.init();
        }

        public void init()
        {
            textureAtlas = new Texture();
            //AtlasUvs = create texture atlas.
            if (VoxelSettings.randomSeed)
                VoxelSettings.seed = 0; // random.
            else
                VoxelSettings.seed = seed;
        }

        public void GenerateSpherical(Vector3Int center)
        {
            Vector3Int[] chunkPositions = GetChunkLocationsAroundPoint(center);

        }

        public Vector3Int[] GetChunkLocationsAroundPoint(Vector3Int center)
        {
            List<Vector3Int> chunksInSphere = new List<Vector3Int>();
            for (int i = 0; i < VoxelSettings.radius + 1; i++)
            {
                for (int x = center.x - i; x < center.x + i; x++)
                {
                    for (int z = center.z - i; z < center.z + i; z++)
                    {
                        for (int y = center.y + VoxelSettings.maxChunksY_M; y > center.y - VoxelSettings.maxChunksY_P; y--)
                        {
                            if (!pageController.BuilderExists(x, y, z) && GeometryUtils.IsInSphere(center, VoxelSettings.radius, new Vector3Int(x, center.y, z)) && !chunksInSphere.Contains(new Vector3Int(x, y, z)))
                                chunksInSphere.Add(new Vector3Int(x, y, z));
                        }
                    }
                }
            }
            return chunksInSphere.ToArray();
        }

        public void SpawnChunk(Vector3Int location)
        {
            if (!Chunks.ContainsKey(location))
            {
                SmoothChunk chunkInstance = new SmoothChunk();
                ScriptComponent scriptComp = new ScriptComponent();
                Entity chunkObj = new Entity();
                chunkObj.Transform.Parent = Entity.Transform;
                chunkObj.Name = string.Format("Chunk_{0}.{1}.{2}", location.x, location.y, location.z);
                chunkObj.Add<ScriptComponent>(ScriptComponent.Key, scriptComp);
                scriptComp.Scripts.Add(chunkInstance);
                chunkInstance.Init(new Vector3Int(location.x, location.y, location.z), pageController);
                Chunks.Add(new Vector3Int(location.x, location.y, location.z), chunkInstance);
                // spawn it somehow?
            }
        }

        public static BlockType[] GetBlockTypeArray(Dictionary<byte, BlockType>.ValueCollection collection)
        {
            BlockType[] types = new BlockType[collection.Count];
            int i = 0;
            foreach (BlockType _type in collection)
            {
                types[i++] = _type;
            }
            return types;
        }
    }
}
