using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.TP.Misc;
using TGC.MonoGame.TP.Misc.Gizmos;

namespace TGC.MonoGame.TP.Scenarios
{
    class TestScenario
	{
		public const string ContentFolder3D = "Models/";
		public const string ContentFolderEffects = "Effects/";
		public const string ContentFolderTextures = "Textures/";
		// Basics
		private GraphicsDevice GraphicsDevice;
		private QuadPrimitive Quad;
		private Effect TilingEffect;
		private Random Random;
		private int SEED = 1;
		public BoundingBox[] Colliders;
		public Gizmos Gizmos;
		public bool ShowGizmos;
		private float GizmosChangeCooldown = 0f;
		// Floor
		private Matrix FloorWorld;
		private Texture2D FloorTexture;
		// Walls
		private Matrix[] WallWorldMatrices;
		private Texture2D WallTexture;
		// Sofa
		public Matrix SofaWorld;
		public Model SofaModel;
		public Matrix[] SofaBoneTransforms;
		// Ambulances
		public Matrix[] AmbulanceWorldMatrices;
		public Model AmbulanceModel;
		public Matrix[] AmbulanceBoneTransforms;

		public TestScenario(ContentManager content, GraphicsDevice graphicsDevice)
		{
			// Set basics
			GraphicsDevice = graphicsDevice;
			Quad = new QuadPrimitive(graphicsDevice);
			TilingEffect = content.Load<Effect>(ContentFolderEffects + "TextureTiling");
			TilingEffect.Parameters["Texture"].SetValue(FloorTexture);
			Random = new Random(SEED);
			float size = 3000f;
			Gizmos = new Gizmos();
			Gizmos.LoadContent(GraphicsDevice, content);
			ShowGizmos = false;

			// Floor
			FloorTexture = content.Load<Texture2D>(ContentFolderTextures + "grass"); //"asphalt_road");
			FloorWorld = Matrix.CreateScale(size);

			// Walls
			var scenarioSize = new Vector3(size, 1f, size / 6f);
			WallTexture = content.Load<Texture2D>(ContentFolderTextures + "wood");
			WallWorldMatrices = new Matrix[] {
				// border limit walls
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(-Vector3.UnitZ * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(Vector3.UnitZ * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2)  * Matrix.CreateTranslation(Vector3.UnitX * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(-MathHelper.PiOver2) * Matrix.CreateTranslation(-Vector3.UnitX * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				// inner walls
				Matrix.CreateScale(new Vector3(100f, 1f, 100f)) * Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateTranslation(Vector3.UnitZ * 100f + Vector3.UnitY * 100f)
			};
			Colliders = new BoundingBox[26];
			Colliders[0] = new BoundingBox(new Vector3(-size- 0.25f, 0f, -size - 0.25f), new Vector3(size + 0.25f, size, -size + 0.25f));
			Colliders[1] = new BoundingBox(new Vector3(-size - 0.25f, 0f, size - 0.25f), new Vector3(size + 0.25f, size, size + 0.25f));
			Colliders[2] = new BoundingBox(new Vector3(size - 0.25f, 0f, -size - 0.25f), new Vector3(size + 0.25f, size, size + 0.25f));
			Colliders[3] = new BoundingBox(new Vector3(-size - 0.25f, 0f, -size - 0.25f), new Vector3(-size + 0.25f, size, size + 0.25f));

			// Sofa
			SofaWorld = Matrix.CreateScale(500f) * Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(-Vector3.UnitX * 2470f + -Vector3.UnitZ * 470f);
			SofaModel = content.Load<Model>(ContentFolder3D + "sofa");
			SofaBoneTransforms = new Matrix[SofaModel.Bones.Count];
			SofaModel.Root.Transform = SofaWorld;
			foreach (var mesh in SofaModel.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.DiffuseColor = new Color((uint)Random.Next()).ToVector3();
				}
			}
			Colliders[4] = new BoundingBox(new Vector3(-size + 30f, 0f, -size + 30f), new Vector3(0f, 500f, -2000f));
			Colliders[5] = new BoundingBox(new Vector3(-size + 30f, 0f, -size + 30f), new Vector3(-2000f, 500f, -450f));

			//Ambulances
			AmbulanceWorldMatrices = new Matrix[20];
			for (int i = 0; i < 20; i++)
			{
				AmbulanceWorldMatrices[i] = 
					Matrix.CreateScale(17f) * 
					Matrix.CreateRotationY(-MathHelper.PiOver2) * 
					Matrix.CreateTranslation((Vector3.UnitX * (size - 400f)) + (Vector3.UnitZ * 300f * (i < 10 ? -i : i-10)));
				Colliders[6+i] = new BoundingBox(
					new Vector3(size - 650f, 0f, i < 10 ? 300f * -i + 125f : 300f * (i - 10) - 125f),
					new Vector3(size - 125f, 300f, i < 10 ? 300f * -i - 125f : 300f * (i - 10) + 125f));
			}
			AmbulanceModel = content.Load<Model>(ContentFolder3D + "ambulance");
			AmbulanceBoneTransforms = new Matrix[AmbulanceModel.Bones.Count];
		}

		public void Draw(GameTime gameTime, Matrix view, Matrix projection)
		{
			// Floor
			TilingEffect.Parameters["Tiling"].SetValue(Vector2.One * 6f); // Vector2.One * amount where amount is the number of times the texture will be repeated
			TilingEffect.Parameters["Texture"].SetValue(FloorTexture);
			TilingEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * view * projection);
			Quad.Draw(TilingEffect);

			// Walls
			var rasterizerState = GraphicsDevice.RasterizerState; // For drawing both sides of the Quad
			GraphicsDevice.RasterizerState = RasterizerState.CullNone; // For drawing both sides of the Quad
			TilingEffect.Parameters["Tiling"].SetValue(Vector2.One);
			TilingEffect.Parameters["Texture"].SetValue(WallTexture);
			for (int i = 0; i < WallWorldMatrices.Length - 1; i++)
			{
				TilingEffect.Parameters["WorldViewProjection"].SetValue(WallWorldMatrices[i] * view * projection);
				Quad.Draw(TilingEffect);
			}
			GraphicsDevice.RasterizerState = rasterizerState; // Restore the old RasterizerState

			// Sofa
			SofaModel.CopyAbsoluteBoneTransformsTo(SofaBoneTransforms);
			foreach (var mesh in SofaModel.Meshes)
			{
				var meshWorld = SofaBoneTransforms[mesh.ParentBone.Index];
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.World = meshWorld;
					effect.View = view;
					effect.Projection = projection;
				}
				mesh.Draw();
				if (ShowGizmos) Gizmos.Draw();
			}

			// Ambulances
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

			Gizmos.UpdateViewProjection(view, projection);
			for (int i = 0; ShowGizmos && i < Colliders.Length; i++)
				Gizmos.DrawCube(BoundingVolumesExtensions.GetCenter(Colliders[i]), BoundingVolumesExtensions.GetExtents(Colliders[i]) * 2f, Color.Yellow);
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
	}
}
