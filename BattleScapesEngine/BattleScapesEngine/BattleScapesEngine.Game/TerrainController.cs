using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Graphics;
using LibNoise;

namespace BattleScapesEngine
{
    public class TerrainController : Script
    {
        [DataMemberIgnore]
        public static TerrainController Instance;
        [DataMemberIgnore]
        public IPageController pageController;
        public int seed = 0;
        public Entity player;
        [DataMemberIgnore]
        public Texture[] AllCubeTextures;
        [DataMemberIgnore]
        public Texture textureAtlas;
        [DataMemberIgnore]
        public Rectangle[] AtlasUvs;
        [DataMemberIgnore]
        public BlockType[] BlocksArray;
        public int chunksInQueue;
        public int chunksGenerated;
        [DataMemberIgnore]
        public Logger SceneLogger;

        private bool _generating;

        [DataMemberIgnore]
        public static Dictionary<byte, BlockType> blockTypes;

        public override void Start()
        {
            base.Start();
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                //Debug.Log("Only one terrain controller allowed per scene. Destroying duplicate.");
                Destroy();
            }
            SceneLogger = GlobalLogger.GetLogger("SceneInstance");
            SceneLogger.Info("Terrain Controller started.");
            blockTypes = new Dictionary<byte, BlockType>();
            init();
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
            pageController = new ChunkController();
            generateOne();
        }

        public void generateOne()
        {
            SpawnChunk(new Vector3Int(0,0,0));
            GenerateChunk(new Vector3Int(0,0,0), new TerrainModule(seed));
        }

        public void GenerateSpherical(Vector3Int center)
        {
            Vector3Int[] chunkPositions = GetChunkLocationsAroundPoint(center);
            chunksInQueue = chunkPositions.Length;
            SpawnChunks(chunkPositions);
            GenerateChunks(chunkPositions);
        }

        public void GenerateChunks(Vector3Int[] chunkLocations)
        {
            //Loom.QueueAsyncTask(WorldThreadName, () =>
            //{
                //SafeDebug.Log("Generating " + chunkLocations.Length + " chunks.");
                TerrainModule module = new TerrainModule(VoxelSettings.seed);
                foreach (Vector3Int location3D in chunkLocations)
                {
                    GenerateChunk(location3D, module);
                    chunksGenerated++;
                }
                //Loom.QueueOnMainThread(() =>
                //{
                    _generating = false;
                    chunksInQueue = 0;
                    chunksGenerated = 0;
                //});
            //});
        }

        public void GenerateChunk(Vector3Int location3D, IModule module)
        {
            try
            {
                if (pageController.BuilderExists(location3D.x, location3D.y, location3D.z) && !pageController.BuilderGenerated(location3D.x, location3D.y, location3D.z))
                {
                    _generating = true;
                    pageController.GetChunk(location3D.x, location3D.y, location3D.z).GenerateChunk(module);
                    pageController.GetChunk(location3D.x, location3D.y, location3D.z).Render(false);
                }
            }
            catch (Exception e)
            {
                //SafeDebug.LogError(e.Message + "\nFunction: GenerateChunks" + "\n" + location3D.x + "," + location3D.y + "," + location3D.y, e);
            }
        }

        public void AddBlockType(BaseType _baseType, string _name, int[] _textures)
        {
            byte index = (byte)TerrainController.blockTypes.Count;
            TerrainController.blockTypes.Add(index, new BlockType(_baseType, index, _name, _textures));
            BlocksArray = TerrainController.GetBlockTypeArray(TerrainController.blockTypes.Values);
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

        public void SpawnChunks(Vector3Int[] locations)
        {
            for (int i = 0; i < locations.Length; i++)
                SpawnChunk(locations[i]);
        }

        public void SpawnChunk(Vector3Int location)
        {
            if (!pageController.BuilderExists(location.x, location.y, location.z))
            {
                SmoothChunk chunkInstance = new SmoothChunk();
                ScriptComponent scriptComp = new ScriptComponent();
                Entity chunkObj = new Entity();
                chunkObj.Transform.Parent = Entity.Transform;
                chunkObj.Name = string.Format("Chunk_{0}.{1}.{2}", location.x, location.y, location.z);
                chunkObj.Add<ScriptComponent>(ScriptComponent.Key, scriptComp);
                scriptComp.Scripts.Add(chunkInstance);
                chunkInstance.Init(new Vector3Int(location.x, location.y, location.z), pageController);
                pageController.Add(location, chunkInstance);
                
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
