using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System;
using TGC.MonoGame.TP.Misc.Gizmos;
using TGC.MonoGame.TP.Misc.Colliders;

namespace TGC.MonoGame.TP.Cars
{
    class RacingCar : BaseCar
	{
		public RacingCar(ContentManager content, GraphicsDevice graphicsDevice, Matrix startRotation,  Vector3 startPosition)
        {
			Gizmos = new Gizmos();
			Gizmos.LoadContent(graphicsDevice, content);
			ShowGizmos = false;

			Model = content.Load<Model>(ContentFolder3D + "racingcar/racingcar");
			BaseTexture = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_basecolor");
			NormalTexture = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_normal");
			RoughnessTexture = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_rougness");
			MetallicTexture = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_metallic");
			AoTexture = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_ao");
			Effect = content.Load<Effect>(ContentFolderEffects + "RacingCarShader");
			Rotation = startRotation;
			Position = startPosition;
			Direction = Vector3.Left;

			foreach (var mesh in Model.Meshes)
			{
				foreach (var meshPart in mesh.MeshParts)
					meshPart.Effect = Effect;
			}

			Effect.CurrentTechnique = Effect.Techniques["Full"];
			Effect.Parameters["lightPosition"].SetValue(new Vector3(0.0f, 7500f, 0.0f));
			Effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
			Effect.Parameters["diffuseColor"].SetValue(new Vector3(1f, 1f, 1f));
			Effect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));
			Effect.Parameters["Ka"].SetValue(0.5f);
			Effect.Parameters["Kd"].SetValue(0.8f);
			Effect.Parameters["Ks"].SetValue(1.0f);
			Effect.Parameters["shininess"].SetValue(5.0f);

			var temporaryCubeAABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
			BoundingBox = OrientedBoundingBox.FromAABB(temporaryCubeAABB);
			BoundingBox.Center = Vector3.One + startPosition;
			BoundingBox.Orientation = startRotation;

			FrontRightWheelBone = Model.Bones["WheelA"];
            FrontLeftWheelBone = Model.Bones["WheelB"];
            BackLeftWheelBone = Model.Bones["WheelC"];
            BackRightWheelBone = Model.Bones["WheelD"];
            CarBone = Model.Bones["Car"];
            CarTransform = CarBone.Transform;
            FrontLeftWheelTransform = FrontLeftWheelBone.Transform;
            FrontRightWheelTransform = FrontRightWheelBone.Transform;
            BackLeftWheelTransform = BackLeftWheelBone.Transform;
            BackRightWheelTransform = BackRightWheelBone.Transform;
            BoneTransforms = new Matrix[Model.Bones.Count];

			MaxBoost = 7.5f;
			DefaultSteeringSpeed = 0.02f;
			DefaultSteeringRotation = 25f;
			DefaultBrakingForce = 30f;
			DefaultJumpSpeed = 1000f;
			DefaultBoostSpeed = 20f;
			MaxSpeed = new float[6] { 800f, 0f, 900f, 1500f, 2000f, 3500f }; // R-N-1-2-3-4
			Acceleration = new float[6] { 15f, -3f, 20f, 15f, 7.5f, 2f }; // R-N-1-2-3-4
		}

        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			Effect.Parameters["ModelTexture"].SetValue(BaseTexture);
			Effect.Parameters["NormalTexture"].SetValue(NormalTexture);
			Effect.Parameters["RoughnessTexture"].SetValue(RoughnessTexture);
			Effect.Parameters["MetallicTexture"].SetValue(MetallicTexture);
			Effect.Parameters["AoTexture"].SetValue(AoTexture);
			Effect.Parameters["Tiling"].SetValue(Vector2.One);
			Effect.Parameters["eyePosition"].SetValue(cameraPosition);
			base.Draw(view, projection);
		}
    }
}