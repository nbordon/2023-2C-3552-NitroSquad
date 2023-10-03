using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TGC.MonoGame.TP.Misc.Colliders;
using TGC.MonoGame.TP.Misc.Primitives;

namespace TGC.MonoGame.TP.Misc
{
    class PowerUp
	{
		public const string ContentFolder3D = "Models/";
		public const string ContentFolderEffects = "Effects/";
		public const string ContentFolderTextures = "Textures/";

		private CubePrimitive Cube;
		private Matrix WorldMatrix;
		private Matrix Rotation;
		public OrientedBoundingBox Collider;
		private Texture2D Texture;
		private Effect Effect;
		private Vector3 BoxSize;
		public PowerUpType Type = PowerUpType.Boost;
		private float HidingCooldown = 0f;
		public bool IsActive
		{
			get => HidingCooldown == 0f;
		}

		public PowerUp(Matrix worldMatrix, Vector3 boxSize, GraphicsDevice graphicsDevice, ContentManager content)
		{
			BoxSize = boxSize;
			Rotation = Matrix.CreateRotationY(1f);
			Texture = content.Load<Texture2D>(ContentFolderTextures + "powerup");
			Effect = content.Load<Effect>(ContentFolderEffects + "CubeShader");
			Effect.Parameters["Tiling"].SetValue(Vector2.One);
			Cube = new CubePrimitive(graphicsDevice, BoxSize);
			WorldMatrix = worldMatrix;
			Collider = OrientedBoundingBox.FromAABB(new BoundingBox(worldMatrix.Translation - BoxSize / 2f, worldMatrix.Translation + BoxSize / 2f));
		}

		public void Update(float elapsedTime)
		{
			Rotation *= Matrix.CreateRotationY(-MathHelper.PiOver2 * elapsedTime);
			if (IsActive)
			{
				WorldMatrix = Rotation * Matrix.CreateTranslation(WorldMatrix.Translation);
				Collider = OrientedBoundingBox.FromAABB(new BoundingBox(WorldMatrix.Translation - BoxSize / 2f, WorldMatrix.Translation + BoxSize / 2f));
			}
			else
				HidingCooldown = Math.Max(HidingCooldown-elapsedTime, 0f);
		}

		public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			if (!IsActive) return;

			Effect.Parameters["Texture"].SetValue(Texture);
			Effect.Parameters["WorldViewProjection"].SetValue(WorldMatrix * view * projection);
			Cube.Draw(Effect);
		}

		public void Hide()
		{
			HidingCooldown = 5f;
		}
	}

	public enum PowerUpType
	{
		Boost = 0,
		Missiles = 1,
		Shield = 2
	}
}
