using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Primitives;

namespace TGC.MonoGame.TP.Scenarios
{
    class TestScenario
	{
		public const string ContentFolder3D = "Models/";
		public const string ContentFolderEffects = "Effects/";
		public const string ContentFolderTextures = "Textures/";
		private QuadPrimitive Floor;
		private Matrix FloorWorld;
		private Effect TilingEffect;

		public TestScenario(ContentManager content, GraphicsDevice graphicsDevice) 
		{
			// Creates a floor
			var floorTexture = content.Load<Texture2D>(ContentFolderTextures + "stones");
			Floor = new QuadPrimitive(graphicsDevice);
			FloorWorld = Matrix.CreateScale(10000f) * Matrix.CreateTranslation(0f,1f,0f);
			TilingEffect = content.Load<Effect>(ContentFolderEffects + "TextureTiling");
			TilingEffect.Parameters["Texture"].SetValue(floorTexture);
			TilingEffect.Parameters["Tiling"].SetValue(Vector2.One * 50f);
		}

		public void Draw(GameTime gameTime, Matrix view, Matrix projection)
		{
			TilingEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * view * projection);
			Floor.Draw(TilingEffect);
		}
	}
}
