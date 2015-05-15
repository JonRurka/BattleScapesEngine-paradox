using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Graphics;
using graphics = SiliconStudio.Paradox.Graphics;

namespace BattleScapesEngine
{
    public class SmoothChunk : Script, IChunk
    {

        public struct BlockChange
        {
            public Vector3Int position;
            public byte type;
            public BlockChange(Vector3Int position, byte type)
            {
                this.position = position;
                this.type = type;
            }
        }
        [DataMemberIgnore]
        public Vector3Int ChunkPosition;
        [DataMemberIgnore]
        public IVoxelBuilder builder;
        [DataMemberIgnore]
        public SmoothVoxelBuilder BuilderInstance;
        [DataMemberIgnore]
        public IPageController pageController;
        [DataMemberIgnore]
        public List<BlockChange> EditQueue;
        public int disappearDistance;
        public int size = 0;
        public int vertSize = 0;
        public int triSize = 0;

        object lockObj;

        bool enableTest = false;
        bool generated = false;
        bool rendered = false;

        Entity _player;
        ModelComponent _model;
        Material _mat;

        public bool Generated
        {
            get { return generated; }
        }

        [DataMemberIgnore]
        public IVoxelBuilder Builder
        {
            get { return builder; }
        }

        public override void Start()
        {
            base.Start();
            EditQueue = new List<BlockChange>();
            lockObj = new object();
            disappearDistance = VoxelSettings.radius;
            TerrainController.Instance.SceneLogger.Info(Name + " Created.");
        }

        public void Init(Vector3Int chunkPos, IPageController pageController)
        {
            ChunkPosition = chunkPos;
            this.pageController = pageController;
            Entity.Transform.Position = VoxelConversions.ChunkCoordToWorld(chunkPos);
            Entity.Add<ModelComponent>(ModelComponent.Key, _model);
            //_player = TerrainController.Instance.player;
            _mat = Asset.Load<Material>("materials/smoothChunk");
            createChunkBuilder();
        }

        public void createChunkBuilder()
        {
            builder = new SmoothVoxelBuilder(TerrainController.Instance.pageController,
                                        ChunkPosition,
                                        VoxelSettings.voxelsPerMeter,
                                        VoxelSettings.MeterSizeX,
                                        VoxelSettings.MeterSizeY,
                                        VoxelSettings.MeterSizeZ);
            builder.SetBlockTypes(TerrainController.Instance.BlocksArray, TerrainController.Instance.AtlasUvs);
            BuilderInstance = (SmoothVoxelBuilder)builder;
        }

        public float[,] GenerateChunk(LibNoise.IModule module)
        {
            float[,] result = null;
            result = ((SmoothVoxelBuilder)builder).Generate(module,
                                                     VoxelSettings.seed,
                                                     VoxelSettings.enableCaves,
                                                     VoxelSettings.amplitude,
                                                     VoxelSettings.caveDensity,
                                                     VoxelSettings.groundOffset,
                                                     VoxelSettings.grassOffset);
            generated = true;
            return result;
        }

        public float[,] GenerateChunk()
        {
            float[,] result = null;
            result = builder.Generate(VoxelSettings.seed,
                                     VoxelSettings.enableCaves,
                                     VoxelSettings.amplitude,
                                     VoxelSettings.caveDensity,
                                     VoxelSettings.groundOffset,
                                     VoxelSettings.grassOffset);
            generated = true;
            return result;
        }

        public void Render(bool renderOnly)
        {
            MeshData meshData = RenderChunk(renderOnly);
            // Loom

            Vertex[] verts = NormalUtils.CalculateNormals(meshData.triangles, meshData.vertices);
            
            Model mod = new Model();
            Mesh mesh = new Mesh();
            mod.Add(mesh);

            VertexPositionNormalTexture[] vertexes = new VertexPositionNormalTexture[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                vertexes[i] = new VertexPositionNormalTexture();
                vertexes[i].Position = verts[i].Position;
                vertexes[i].Normal = verts[i].Normal;
                vertexes[i].TextureCoordinate = verts[i].TextureCoords;
            }

            graphics.Buffer vertBuffer = graphics.Buffer.Vertex.New(GraphicsDevice, vertexes);
            graphics.Buffer indexBuffer = graphics.Buffer.Index.New<int>(GraphicsDevice, meshData.triangles);

            MeshDraw mDraw = new MeshDraw
            {
                PrimitiveType = PrimitiveType.TriangleList,
                VertexBuffers = new[] {new VertexBufferBinding(vertBuffer, VertexPositionNormalTexture.Layout, vertBuffer.ElementCount) },
                IndexBuffer = new IndexBufferBinding(indexBuffer, true, indexBuffer.ElementCount),
                DrawCount = indexBuffer.ElementCount
            };

            mesh.Draw = mDraw;
            mod.Materials.Add(_mat);
            _model.Model = mod;


            /*Mesh mesh = new Mesh();
            MeshDraw draw = new MeshDraw();
            draw.PrimitiveType = PrimitiveType.TriangleList;
            graphics.Buffer vertexBuff = graphics.Buffer.Vertex.New(GraphicsDevice, meshData.vertices);
            graphics.Buffer indexBuff = graphics.Buffer.New(GraphicsDevice, meshData.triangles, BufferFlags.IndexBuffer);
            draw.IndexBuffer = new IndexBufferBinding(indexBuff, true, meshData.triangles.Length);
            VertexDeclaration vertDec = new VertexDeclaration(VertexElement.Position<Vector3>());
            draw.VertexBuffers = new VertexBufferBinding[] { new VertexBufferBinding(vertexBuff, vertDec, meshData.vertices.Length) };
            mesh.Draw = draw;
            Model mod = new Model();
            mod.Add(mesh);
            _model.Model = mod;*/
        }

        public byte GetBlock(int x, int y, int z)
        {
            byte result = 0;
            if (builder != null)
            {
                result = builder.GetBlock(x, y, z).type;
            }
            return result;
        }

        public void Close()
        {
            builder.Dispose();
            builder = null;
            BuilderInstance = null;
        }

        private MeshData RenderChunk(bool renderOnly)
        {
            return builder.Render(renderOnly);
        }
    }
}
