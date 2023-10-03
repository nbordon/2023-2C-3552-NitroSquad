using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.TP.Misc;
using TGC.MonoGame.TP.Misc.Colliders;
using TGC.MonoGame.TP.Misc.Gizmos;
using TGC.MonoGame.TP.Misc.Primitives;

namespace TGC.MonoGame.TP.Scenarios
{
    class TestScenario
	{
		public const string ContentFolder3D = "Models/";
		public const string ContentFolderEffects = "Effects/";
		public const string ContentFolderTextures = "Textures/";
		public const int NumberOfBoxColliders = 60;
		public const int NumberOfCylinderColliders = 183;
		// Basics
		private GraphicsDevice GraphicsDevice;
		private QuadPrimitive Quad;
		private Effect TilingEffect;
		private Random Random;
		private int SEED = 2;
		//public OrientedBoundingBox[] BoxColliders;
		//private int BoxColliderIndex = -1;
		public BaseCollider[] Colliders;
		private int ColliderIndex = -1;
		//public BoundingCylinder[] CylinderColliders;
		//private int CylinderColliderIndex = 0;
		public Gizmos Gizmos;
		private bool ShowGizmos;
		private float GizmosChangeCooldown = 0f;
		public Matrix MainCarStartRotation;
		public Vector3 MainCarStartPosition;
		// Floor
		private Matrix FloorWorld;
		private Texture2D FloorTexture;
		// Walls
		private Matrix[] WallWorldMatrices;
		private Texture2D WallTexture;
		//Boxes
		private CubePrimitive Box;
		private Matrix[] BoxWorldMatrices;
		private Texture2D BoxTexture;
		// Sofa
		private Matrix SofaWorld;
		private Model SofaModel;
		private Matrix[] SofaBoneTransforms;
		private Effect SofaEffect;
		private Texture2D SofaTexture;
		private Texture2D SofaNormalTexture;
		// Ambulances
		private Model AmbulanceModel;
		private Matrix[] AmbulanceWorldMatrices;
		private Matrix[] AmbulanceBoneTransforms;
		// Barriers
		private Effect CubeEffect;
		private CubePrimitive Barrier;
		private Matrix[] BarriersWorldMatrices;
		private Texture2D BarrierTexture;
		//Trees
		private Model TreeModel;
		private Matrix[] TreesWorldMatrices;
		private Matrix[] TreeBoneTransforms;
		//Power up
		public PowerUp[] PowerUps;
		//Tire Barriers
		private CylinderPrimitive TireBarrier;
		public Matrix[] TireBarriersWorldMatrices;
		private Texture2D TireBarrierTexture;

		public TestScenario(ContentManager content, GraphicsDevice graphicsDevice)
		{
			// Set basics
			GraphicsDevice = graphicsDevice;
			Quad = new QuadPrimitive(graphicsDevice);
			TilingEffect = content.Load<Effect>(ContentFolderEffects + "TextureTiling");
			Random = new Random(SEED);
			//BoxColliders = new OrientedBoundingBox[NumberOfBoxColliders];
			//CylinderColliders = new BoundingCylinder[NumberOfCylinderColliders];
			Colliders = new BaseCollider[NumberOfBoxColliders+NumberOfCylinderColliders];
			MainCarStartRotation = Matrix.CreateRotationY(-MathHelper.PiOver2);
			MainCarStartPosition = new Vector3(-1000f, 0f, 2000f);

			InitializeFloor(content);
			InitializeWalls(content);
			InitializeBoxes(content);
			InitializeSofa(content);
			InitializeAmbulances(content);
			InitializeBarriers(content);
			InitializeTrees(content);
			InitializePowerUps(content);
			InitializeTireBarriers(content); 
			InitializeGizmos(content);
		}

		public void Update(GameTime gameTime)
		{
			var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
			UpdatePowerUps(elapsedTime);
		}

		public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			DrawFloor(view, projection, cameraPosition);
			DrawWalls(view, projection, cameraPosition);
			DrawBoxes(view, projection, cameraPosition);
			DrawSofa(view, projection, cameraPosition);
			DrawAmbulances(view, projection, cameraPosition);
			DrawBarriers(view, projection, cameraPosition);
			DrawTrees(view, projection, cameraPosition);
			DrawPowerUps(view, projection, cameraPosition);
			DrawTireBarriers(view, projection, cameraPosition);
			DrawGizmos(view, projection, cameraPosition); 
		}

		public void Dispose()
		{
			Gizmos.Dispose();
			FloorTexture.Dispose();
			WallTexture.Dispose();
			BoxTexture.Dispose();
			TilingEffect.Dispose();
			SofaNormalTexture.Dispose();
			SofaTexture.Dispose();
			SofaEffect.Dispose();
			CubeEffect.Dispose();
			BarrierTexture.Dispose();
			TireBarrierTexture.Dispose();
		}

		#region Floor
		private void InitializeFloor(ContentManager content)
		{
			var size = 25000f;
			FloorTexture = content.Load<Texture2D>(ContentFolderTextures + "barcelona");
			FloorWorld = Matrix.CreateScale(size);
		}
		private void DrawFloor(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			TilingEffect.Parameters["Tiling"].SetValue(Vector2.One); // Vector2.One * amount where amount is the number of times the texture will be repeated
			TilingEffect.Parameters["Texture"].SetValue(FloorTexture);
			TilingEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * view * projection);
			Quad.Draw(TilingEffect);
		}
		#endregion Floor

		#region Walls
		private void InitializeWalls(ContentManager content)
		{
			var size = 25000f;
			var scenarioSize = new Vector3(size, 1f, 400f);
			WallTexture = content.Load<Texture2D>(ContentFolderTextures + "fence");
			WallWorldMatrices = new Matrix[] {
				// border limit walls
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(-Vector3.UnitZ * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(Vector3.UnitZ * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2)  * Matrix.CreateTranslation(Vector3.UnitX * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(-MathHelper.PiOver2) * Matrix.CreateTranslation(-Vector3.UnitX * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				// inner walls
				//Matrix.CreateScale(new Vector3(100f, 1f, 100f)) * Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateTranslation(Vector3.UnitZ * 100f + Vector3.UnitY * 100f)
			};
			Colliders[ColliderIndex + 1] = OrientedBoundingBox.FromAABB(new BoundingBox(new Vector3(-size - 0.25f, 0f, -size - 0.25f), new Vector3(size + 0.25f, size, -size + 0.25f)));
			Colliders[ColliderIndex + 2] = OrientedBoundingBox.FromAABB(new BoundingBox(new Vector3(-size - 0.25f, 0f, size - 0.25f), new Vector3(size + 0.25f, size, size + 0.25f)));
			Colliders[ColliderIndex + 3] = OrientedBoundingBox.FromAABB(new BoundingBox(new Vector3(size - 0.25f, 0f, -size - 0.25f), new Vector3(size + 0.25f, size, size + 0.25f)));
			Colliders[ColliderIndex + 4] = OrientedBoundingBox.FromAABB(new BoundingBox(new Vector3(-size - 0.25f, 0f, -size - 0.25f), new Vector3(-size + 0.25f, size, size + 0.25f)));
			ColliderIndex += 4;
		}
		private void DrawWalls(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			var rasterizerState = GraphicsDevice.RasterizerState; // For drawing both sides of the Quad
			GraphicsDevice.RasterizerState = RasterizerState.CullNone; // For drawing both sides of the Quad
			TilingEffect.Parameters["Tiling"].SetValue(Vector2.One);
			TilingEffect.Parameters["Texture"].SetValue(WallTexture);
			for (int i = 0; i < WallWorldMatrices.Length; i++)
			{
				TilingEffect.Parameters["WorldViewProjection"].SetValue(WallWorldMatrices[i] * view * projection);
				Quad.Draw(TilingEffect);
			}
			GraphicsDevice.RasterizerState = rasterizerState; // Restore the old RasterizerState
		}
		#endregion Walls

		#region Boxes
		private void InitializeBoxes(ContentManager content)
		{
			Vector3 boxSize = new Vector3(1000f, 50f, 500f);
			Box = new CubePrimitive(GraphicsDevice, boxSize);
			BoxTexture = content.Load<Texture2D>(ContentFolderTextures + "wood_box");
			//BoxColliderIndex++;
			//BoxWorldMatrices = new Matrix[] {
			//	Matrix.CreateTranslation(  3500f, boxSize.Y / 2f, -10500f),
			//	Matrix.CreateTranslation( -3500f, boxSize.Y / 2f,  10500f),
			//	Matrix.CreateTranslation( 27000f, boxSize.Y / 2f,  17000f),
			//	Matrix.CreateTranslation(-27000f, boxSize.Y / 2f, -17000f),
			//	Matrix.CreateTranslation(-11000f, boxSize.Y / 2f, -15000f),
			//	Matrix.CreateTranslation( 11000f, boxSize.Y / 2f,  15000f),
			//	Matrix.CreateTranslation( -7500f, boxSize.Y / 2f,  27000f),
			//	Matrix.CreateTranslation(  7500f, boxSize.Y / 2f, -27000f),
			//};
			//for (int i = 0; i < BoxWorldMatrices.Length; i++)
			//{
			//	BoxColliders[BoxColliderIndex + i] = OrientedBoundingBox.FromAABB(new BoundingBox(BoxWorldMatrices[i].Translation - boxSize / 2f, BoxWorldMatrices[i].Translation + boxSize / 2f));
			//}
			//BoxColliderIndex += BoxWorldMatrices.Length - 1;
		}

		private void DrawBoxes(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			//TilingEffect.Parameters["Texture"].SetValue(BoxTexture);
			//for (int i = 0; i < BoxWorldMatrices.Length; i++)
			//{
			//	TilingEffect.Parameters["WorldViewProjection"].SetValue(BoxWorldMatrices[i] * view * projection);
			//	Box.Draw(TilingEffect);
			//}
		}
		#endregion Boxes

		#region Sofa
		private void InitializeSofa(ContentManager content)
		{
			SofaWorld = Matrix.CreateScale(500f) * Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(-24000f, 50f, -20000f);
			SofaModel = content.Load<Model>(ContentFolder3D + "sofa");
			SofaBoneTransforms = new Matrix[SofaModel.Bones.Count];
			SofaModel.Root.Transform = SofaWorld;
			SofaEffect = content.Load<Effect>(ContentFolderEffects + "RacingCarShader");
			SofaTexture = content.Load<Texture2D>(ContentFolderTextures + "plain");
			SofaNormalTexture = content.Load<Texture2D>(ContentFolderTextures + "plain_normal");
			SofaEffect.Parameters["lightPosition"].SetValue(new Vector3(0.0f, 7500f, 0.0f));
			SofaEffect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
			SofaEffect.Parameters["diffuseColor"].SetValue(new Vector3(1f, 1f, 1f));
			SofaEffect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));
			SofaEffect.Parameters["Ka"].SetValue(0.1f);
			SofaEffect.Parameters["Kd"].SetValue(1.0f);
			SofaEffect.Parameters["Ks"].SetValue(0.8f);
			SofaEffect.Parameters["shininess"].SetValue(16.0f);
			foreach (var mesh in SofaModel.Meshes)
			{
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					meshPart.Effect = SofaEffect;
				}
			}
			Colliders[ColliderIndex + 1] = OrientedBoundingBox.FromAABB(new BoundingBox(SofaWorld.Translation - new Vector3(500f, 0f, 2500f), SofaWorld.Translation + new Vector3(2500f, 500f, -1500f)));
			Colliders[ColliderIndex + 2] = OrientedBoundingBox.FromAABB(new BoundingBox(SofaWorld.Translation - new Vector3(500f, 0f, 2500f), SofaWorld.Translation + new Vector3(500f, 500f, 0f)));
			ColliderIndex += 2;
		}
		private void DrawSofa(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			SofaModel.CopyAbsoluteBoneTransformsTo(SofaBoneTransforms);
			SofaEffect.Parameters["eyePosition"].SetValue(cameraPosition);
			SofaEffect.Parameters["ModelTexture"].SetValue(SofaTexture);
			SofaEffect.Parameters["NormalTexture"].SetValue(SofaNormalTexture);
			SofaEffect.Parameters["RoughnessTexture"].SetValue(SofaTexture);
			SofaEffect.Parameters["MetallicTexture"].SetValue(SofaTexture);
			SofaEffect.Parameters["AoTexture"].SetValue(SofaTexture);
			SofaEffect.Parameters["Tiling"].SetValue(Vector2.One);
			foreach (var mesh in SofaModel.Meshes)
			{
				var meshWorld = SofaBoneTransforms[mesh.ParentBone.Index];
				SofaEffect.Parameters["World"].SetValue(meshWorld);
				SofaEffect.Parameters["WorldViewProjection"].SetValue(meshWorld * view * projection);
				SofaEffect.Parameters["NormalWorldMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(meshWorld)));
				mesh.Draw();
				if (ShowGizmos) Gizmos.Draw();
			}
		}
		#endregion Sofa

		#region Ambulances
		private void InitializeAmbulances(ContentManager content)
		{
			var size = 25000f;
			ColliderIndex++;
			AmbulanceWorldMatrices = new Matrix[20];
			for (int i = 0; i < 20; i++)
			{
				AmbulanceWorldMatrices[i] =
					Matrix.CreateScale(17f) *
					Matrix.CreateRotationY(-MathHelper.PiOver2) *
					Matrix.CreateTranslation((Vector3.UnitX * (size - 400f)) + (Vector3.UnitZ * 300f * (i < 10 ? -i : i - 10)));
				Colliders[ColliderIndex + i] = OrientedBoundingBox.FromAABB(new BoundingBox(
					new Vector3(size - 650f, 0f, i < 10 ? 300f * -i + 125f : 300f * (i - 10) - 125f),
					new Vector3(size - 125f, 300f, i < 10 ? 300f * -i - 125f : 300f * (i - 10) + 125f)));
			}
			ColliderIndex += 20;
			AmbulanceModel = content.Load<Model>(ContentFolder3D + "ambulance");
			AmbulanceBoneTransforms = new Matrix[AmbulanceModel.Bones.Count];
		}
		private void DrawAmbulances(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			for (int i = 0; i < AmbulanceWorldMatrices.Length; i++)
			{
				AmbulanceModel.Root.Transform = AmbulanceWorldMatrices[i];
				AmbulanceModel.CopyAbsoluteBoneTransformsTo(AmbulanceBoneTransforms);
				foreach (var mesh in AmbulanceModel.Meshes)
				{
					var meshWorld = AmbulanceBoneTransforms[mesh.ParentBone.Index];
					foreach (BasicEffect effect in mesh.Effects)
					{
						effect.World = meshWorld;
						effect.View = view;
						effect.Projection = projection;
					}
					mesh.Draw();
				}
			}
		}
		#endregion Ambulances

		#region Barriers
		private void InitializeBarriers(ContentManager content)
		{
			Vector3 barrierSize = new Vector3(2000f, 150f, 100f);
			Barrier = new CubePrimitive(GraphicsDevice, barrierSize);
			CubeEffect = content.Load<Effect>(ContentFolderEffects + "CubeShader");
			BarrierTexture = content.Load<Texture2D>(ContentFolderTextures + "barrier");
			CubeEffect.Parameters["Tiling"].SetValue(Vector2.One);
			BarriersWorldMatrices = new Matrix[] {
				//normal
				Matrix.CreateTranslation(     0f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation(  2000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation( -2000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation(  4000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation( -4000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation(  6000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation( -6000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation(  8000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation( -8000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation( 10000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation(-10000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation( 12000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation(-12000f, barrierSize.Y / 2f,  3500f),
				Matrix.CreateTranslation(-17000f, barrierSize.Y / 2f,-13000f),
				Matrix.CreateTranslation(-19000f, barrierSize.Y / 2f,-13000f),
				Matrix.CreateTranslation(-21000f, barrierSize.Y / 2f,-13000f),
				Matrix.CreateTranslation( -6000f, barrierSize.Y / 2f,   900f),
				Matrix.CreateTranslation( -8000f, barrierSize.Y / 2f,   900f),
				Matrix.CreateTranslation(-10000f, barrierSize.Y / 2f,   900f),
				Matrix.CreateTranslation(-12000f, barrierSize.Y / 2f,   900f),
				Matrix.CreateTranslation( -1000f, barrierSize.Y / 2f,   900f),
				Matrix.CreateTranslation(  1000f, barrierSize.Y / 2f,   900f),
				Matrix.CreateTranslation(  3000f, barrierSize.Y / 2f,   900f),
				Matrix.CreateTranslation(  5000f, barrierSize.Y / 2f,   900f),
				Matrix.CreateTranslation(  7000f, barrierSize.Y / 2f,   900f),
				Matrix.CreateTranslation(  9000f, barrierSize.Y / 2f,   900f),
				Matrix.CreateTranslation( 11000f, barrierSize.Y / 2f,   900f),
				Matrix.CreateTranslation( 19000f, barrierSize.Y / 2f,-12000f),
				Matrix.CreateTranslation( 21000f, barrierSize.Y / 2f,-12000f),
				Matrix.CreateTranslation( 23000f, barrierSize.Y / 2f,-12000f),
				//rotated 90 degrees
				Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation( 24000f, barrierSize.Y / 2f, -5000f),
				Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation( 24000f, barrierSize.Y / 2f, -7000f),
				Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation( 24000f, barrierSize.Y / 2f, -9000f),
				Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation( 24000f, barrierSize.Y / 2f,-11000f),
			};
			//normal
			for (int i = 0; i < BarriersWorldMatrices.Length - 4; i++)
			{
				Colliders[ColliderIndex + i] = OrientedBoundingBox.FromAABB(new BoundingBox(BarriersWorldMatrices[i].Translation - barrierSize / 2f, BarriersWorldMatrices[i].Translation + barrierSize / 2f));
			}
			//rotated 90 degrees
			for (int i = BarriersWorldMatrices.Length - 4; i < BarriersWorldMatrices.Length; i++)
			{
				BoundingBox boundingBox = new BoundingBox(
						new Vector3(BarriersWorldMatrices[i].Translation.X - barrierSize.Z / 2f, 0f, BarriersWorldMatrices[i].Translation.Z - barrierSize.X / 2f),
						new Vector3(BarriersWorldMatrices[i].Translation.X + barrierSize.Z / 2f, barrierSize.Y, BarriersWorldMatrices[i].Translation.Z + barrierSize.X / 2f));
				Colliders[ColliderIndex + i] = OrientedBoundingBox.FromAABB(boundingBox);
			}
			ColliderIndex += BarriersWorldMatrices.Length;
		}

		private void DrawBarriers(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			CubeEffect.Parameters["Texture"].SetValue(BarrierTexture);
			for (int i = 0; i < BarriersWorldMatrices.Length; i++)
			{
				CubeEffect.Parameters["WorldViewProjection"].SetValue(BarriersWorldMatrices[i] * view * projection);
				Barrier.Draw(CubeEffect);
			}
		}
		#endregion Barriers

		#region Trees
		private void InitializeTrees(ContentManager content)
		{
			Vector3 treeSize = new Vector3(150f, 500f, 150f);
			TreeModel = content.Load<Model>(ContentFolder3D + "tree");
			TreeBoneTransforms = new Matrix[TreeModel.Bones.Count];
			TreesWorldMatrices = new Matrix[] {
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( -4000f, 0f, 10000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( -7000f, 0f, 10000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(-10000f, 0f, 10000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(-13000f, 0f, 10000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( -5000f, 0f, 15000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( -2000f, 0f, 15000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(  1000f, 0f, 15000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(  4000f, 0f, 15000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(  7000f, 0f, 15000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(  9000f, 0f, 15000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( 13000f, 0f,  5000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( 13000f, 0f,  8000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( 15000f, 0f,  6000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( 16000f, 0f,  8000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( 19000f, 0f,  6000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( 19000f, 0f,  9000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( 22000f, 0f,  5000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation( 22000f, 0f,  8000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(  1000f, 0f, -5000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(	 0f, 0f, -8000f),
				Matrix.CreateScale(0.5f) * Matrix.CreateTranslation(  3000f, 0f, -7000f),
			};
			for (int i = 0; i < TreesWorldMatrices.Length; i++)
			{
				Colliders[ColliderIndex + i] = new BoundingCylinder(TreesWorldMatrices[i].Translation, treeSize.X / 2f, treeSize.Y / 2f);
			}

			ColliderIndex += TreesWorldMatrices.Length;
		}
		private void DrawTrees(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			Vector3[] colors = new Vector3[]{ new Vector3(75f, 30f, 0f) / 255f, new Vector3(72f, 173f, 77f) / 255f };
			for (int i = 0; i < TreesWorldMatrices.Length; i++)
			{
				TreeModel.Root.Transform = TreesWorldMatrices[i];
				TreeModel.CopyAbsoluteBoneTransformsTo(TreeBoneTransforms);
				int color = 0;
				foreach (var mesh in TreeModel.Meshes)
				{
					var meshWorld = TreeBoneTransforms[mesh.ParentBone.Index];
					foreach (BasicEffect effect in mesh.Effects)
					{
						effect.DiffuseColor = colors[color];
						effect.World = meshWorld;
						effect.View = view;
						effect.Projection = projection;
					}
					mesh.Draw();
					color++;
				}
			}
		}
		#endregion Trees

		#region PowerUps
		private void InitializePowerUps(ContentManager content)
		{
			Vector3 size = new Vector3(250f, 250f, 250f);
			PowerUps = new PowerUp[] {
				new PowerUp (Matrix.CreateTranslation(-13500f, size.Y/2f,  1600f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation(-13500f, size.Y/2f,  2050f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation(-13500f, size.Y/2f,  2500f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation(-10000f, size.Y/2f,-11000f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation( 10000f, size.Y/2f, -5500f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation( 20000f, size.Y/2f, -9000f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation(  3500f, size.Y/2f, -10500f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation( -3500f, size.Y/2f,  10500f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation( 27000f, size.Y/2f,  17000f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation(-27000f, size.Y/2f, -17000f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation(-11000f, size.Y/2f, -15000f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation( 11000f, size.Y/2f,  15000f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation( -7500f, size.Y/2f,  27000f), size, GraphicsDevice, content),
				new PowerUp (Matrix.CreateTranslation(  7500f, size.Y/2f, -27000f), size, GraphicsDevice, content),
			};
		}

		private void UpdatePowerUps(float elapsedTime)
		{
			for (int i = 0; i < PowerUps.Length; i++)
			{
				PowerUps[i].Update(elapsedTime);
			}
		}

		private void DrawPowerUps(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			for (int i = 0; i < PowerUps.Length; i++)
			{
				PowerUps[i].Draw(view, projection, cameraPosition);
			}
		}
		#endregion PowerUps

		#region TireBarriers
		private void InitializeTireBarriers(ContentManager content)
		{
			Vector3 size = new Vector3(175f, 300f, 175f);
			TireBarrierTexture = content.Load<Texture2D>(ContentFolderTextures + "tire");
			TireBarrier = new CylinderPrimitive(GraphicsDevice, size.Y, size.X - 25f, 16);
			TireBarriersWorldMatrices = new Matrix[164];
			int index = 0;
			index = InitializeTireBarrierGroup(index, size, new Vector2(17000f, 4000f), 0);
			index = InitializeTireBarrierGroup(index, size, new Vector2(-7500f, -12000f), 1);
			index = InitializeTireBarrierGroup(index, size, new Vector2(-19000f, 0f), 2);
			ColliderIndex += index;
		}
		private int InitializeTireBarrierGroup(int index, Vector3 size, Vector2 startLocation, int orientation)
		{
			float incrementX = 0f;
			float incrementZ = 0f;
			for (int j = 0; j < 4; j++) // number of tires vertical
			{
				for (int i = 0; i < (15 - j); i++) // number of tires horizontal
				{
					switch (orientation) {
						case 0:
							incrementX = i * size.X + j * size.X / 2f;
							incrementZ = j * size.X;
							break;
						case 1:
							incrementX = i * size.X / 2f + j * size.X;
							incrementZ = i * size.X;
							break;
						case 2:
							incrementX = -j * size.X;
							incrementZ = i * size.X + j * size.X / 2f;
							break;
					}
					Vector3 translation = new Vector3(startLocation.X + incrementX, size.Y / 2f, startLocation.Y + incrementZ);
					TireBarriersWorldMatrices[index] = Matrix.CreateTranslation(translation);
					Colliders[ColliderIndex + index] = new BoundingCylinder(translation, size.X / 2f, size.Y / 2f);
					index++;
				}
			}
			return index;
		}

		private void DrawTireBarriers(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			CubeEffect.Parameters["Texture"].SetValue(TireBarrierTexture);
			for (int i = 0; i < TireBarriersWorldMatrices.Length; i++)
			{
				CubeEffect.Parameters["WorldViewProjection"].SetValue(TireBarriersWorldMatrices[i] * view * projection);
				TireBarrier.Draw(CubeEffect);
			}
		}
		#endregion TireBarriers

		#region Gizmos
		private void InitializeGizmos(ContentManager content)
		{
			Gizmos = new Gizmos();
			Gizmos.LoadContent(GraphicsDevice, content);
			ShowGizmos = false;
		}
		private void DrawGizmos(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			if (!ShowGizmos) return;
			Gizmos.UpdateViewProjection(view, projection);
			for (int i = 0; i < NumberOfBoxColliders; i++)
				Gizmos.DrawCube(Colliders[i].Center, Colliders[i].Extents * 2f, Color.Yellow);
			for (int i = NumberOfBoxColliders; i < Colliders.Length; i++)
				Gizmos.DrawCylinder(Colliders[i].Center, Matrix.Identity, new Vector3(150f, 300f, 150f) / 2f, Color.Yellow);
		}
		public void ChangeGizmosVisibility()
		{
			GizmosChangeCooldown = 0f;
			ShowGizmos = !ShowGizmos;
		}
		public bool IsAbleToChangeGizmosVisibility(GameTime gameTime)
		{
			GizmosChangeCooldown = MathF.Min(GizmosChangeCooldown + Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds), 0.3f);
			return GizmosChangeCooldown == 0.3f;
		}
		#endregion Gizmos
	}
}
