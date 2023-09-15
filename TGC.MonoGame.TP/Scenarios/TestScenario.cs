using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.TP.Primitives;

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
			float size = 3000f;

			// Floor
			FloorTexture = content.Load<Texture2D>(ContentFolderTextures + "grass"); //"asphalt_road");
			FloorWorld = Matrix.CreateScale(size);

			// Walls
			var scenarioSize = new Vector3(size, 1f, size / 6f);
			WallTexture = content.Load<Texture2D>(ContentFolderTextures + "wood");
			WallWorldMatrices = new Matrix[5] {
				// border limit walls
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(-Vector3.UnitZ * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(Vector3.UnitZ * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2)  * Matrix.CreateTranslation(Vector3.UnitX * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				Matrix.CreateScale(scenarioSize) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(-MathHelper.PiOver2) * Matrix.CreateTranslation(-Vector3.UnitX * scenarioSize.X + Vector3.UnitY * scenarioSize.Z),
				// inner walls
				Matrix.CreateScale(new Vector3(100f, 1f, 100f)) * Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateTranslation(Vector3.UnitZ * 100f + Vector3.UnitY * 100f)
			};

			// Sofa
			SofaWorld = Matrix.CreateScale(500f) * Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(-Vector3.UnitX * 2490f + -Vector3.UnitZ * 490f);
			SofaModel = content.Load<Model>(ContentFolder3D + "sofa");
			SofaBoneTransforms = new Matrix[SofaModel.Bones.Count];
			SofaModel.Root.Transform = SofaWorld;

			//Ambulances
			AmbulanceWorldMatrices = new Matrix[20];
			for (int i = 0; i < 20; i++)
			{
				AmbulanceWorldMatrices[i] = 
					Matrix.CreateScale(10f) * 
					Matrix.CreateRotationY(-MathHelper.PiOver2) * 
					Matrix.CreateTranslation((Vector3.UnitX * (size - 200f)) + (Vector3.UnitZ * 300f * (i < 10 ? -i : i-10)));
			}
			AmbulanceModel = content.Load<Model>(ContentFolder3D + "ambulance");
			AmbulanceBoneTransforms = new Matrix[AmbulanceModel.Bones.Count];
		}

		public void Draw(GameTime gameTime, Matrix view, Matrix projection)
		{
			var viewProjection = view * projection;

			// Floor
			TilingEffect.Parameters["Tiling"].SetValue(Vector2.One * 6f); // Vector2.One * amount where amount is the number of times the texture will be repeated
			TilingEffect.Parameters["Texture"].SetValue(FloorTexture);
			TilingEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * viewProjection);
			Quad.Draw(TilingEffect);

			// Walls
			var rasterizerState = GraphicsDevice.RasterizerState; // For drawing both sides of the Quad
			GraphicsDevice.RasterizerState = RasterizerState.CullNone; // For drawing both sides of the Quad
			TilingEffect.Parameters["Tiling"].SetValue(Vector2.One);
			TilingEffect.Parameters["Texture"].SetValue(WallTexture);
			for (int i = 0; i < WallWorldMatrices.Length - 1; i++)
			{
				TilingEffect.Parameters["WorldViewProjection"].SetValue(WallWorldMatrices[i] * viewProjection);
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
		}
	}
}
