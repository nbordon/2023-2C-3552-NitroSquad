using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TGC.MonoGame.TP.Misc;
using TGC.MonoGame.TP.Misc.Colliders;
using TGC.MonoGame.TP.Misc.Gizmos;

namespace TGC.MonoGame.TP.Cars
{
    class BaseCar
	{
		public const string ContentFolder3D = "Models/";
		public const string ContentFolderEffects = "Effects/";

		#region Properties
		//current state
		private int CurrentGear = 1;
		private float CurrentSpeed = 0f;
		private float CurrentWheelRotation = 0f;
		private float CurrentSteeringWheelRotation = 0f;
		private bool IsAccelerating = false;
		private bool IsBraking = false;
		private bool IsUsingBoost = false;
		private bool IsTurningLeft = false;
		private bool IsTurningRight = false;
		private bool IsJumping = false;
		private bool GodMode = false;
		private float GodModeCooldown = 0f;
		private float GizmosCooldown = 0f;

		//Power Ups
		private float RemainingBoost = 0f;  // in seconds
		private int RemainingMissiles = 0;
		private bool HasShield = false;

		//car specs
		public float DefaultSteeringSpeed;
		public float DefaultSteeringRotation;
		public float DefaultBrakingForce;
		public float DefaultJumpSpeed;
		public float DefaultBoostSpeed;
		public float[] MaxSpeed;
		public float[] Acceleration;
		public float MaxBoost = 0f;

		// todo: global
		public const float Gravity = 50f;

		public Effect Effect;
		public Model Model;
		public Matrix World;
		public Matrix Rotation = Matrix.Identity;
		public Matrix Scale = Matrix.Identity;
		public Vector3 Position = Vector3.Zero;
		public Vector3 Direction = Vector3.Backward;
		public Vector3 DirectionSpeed = Vector3.Backward;
		public OrientedBoundingBox BoundingBox;
		public Gizmos Gizmos;
		public bool ShowGizmos;

		public ModelBone FrontRightWheelBone;
		public ModelBone FrontLeftWheelBone;
		public ModelBone BackLeftWheelBone;
		public ModelBone BackRightWheelBone;
		public ModelBone CarBone;
		public Matrix FrontRightWheelTransform;
		public Matrix FrontLeftWheelTransform;
		public Matrix BackLeftWheelTransform;
		public Matrix BackRightWheelTransform;
		public Matrix CarTransform;
		public Matrix[] BoneTransforms;
		public Texture2D BaseTexture;
		public Texture2D NormalTexture;
		public Texture2D RoughnessTexture;
		public Texture2D MetallicTexture;
		public Texture2D AoTexture;
		#endregion Properties

		public BaseCar() { }

		public void Update(GameTime gameTime, BaseCollider[] colliders, PowerUp[] powerups)
		{
			SetKeyboardState(gameTime);
			var previousPosition = Position;
			var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
			if (Position.Y == 0f)
			{
				// para tener control sobre el auto hice que deba estar sobre el suelo, ninguna razon en particular, me gusto asi
				Drive(elapsedTime);
				Turn();
				if (IsJumping) Jump();
			}
			else
			{
				CurrentSpeed /= 1 + elapsedTime; // para que vaya desacelerando gradualemente
				DirectionSpeed -= Vector3.Up * Gravity;
			}

			// combino las velocidades horizontal y vertical
			DirectionSpeed = Direction * CurrentSpeed + Vector3.Up * DirectionSpeed.Y;
			Position += DirectionSpeed * elapsedTime;

			if (Position.Y < 0f)
			{
				// si quedara por debajo del suelo lo seteo en 0
				Position.Y = 0f;
				DirectionSpeed.Y = 0f;
			}

			if (Position != previousPosition)
				Position -= CheckForCollisions(Position - previousPosition, colliders, powerups);

			World = Scale * Rotation * Matrix.CreateTranslation(Position);
		}

		public void Draw(Matrix view, Matrix projection)
		{
			// Set the world matrix as the root transform of the model.
			Model.Root.Transform = World;

			// Calculate matrices based on the current animation position.
			var wheelRotationX = Matrix.CreateRotationX(CurrentWheelRotation);
			var steeringRotationY = Matrix.CreateRotationY(CurrentSteeringWheelRotation);

			// Apply matrices to the relevant bones.
			FrontLeftWheelBone.Transform = wheelRotationX * steeringRotationY * FrontLeftWheelTransform;
			FrontRightWheelBone.Transform = wheelRotationX * steeringRotationY * FrontRightWheelTransform;
			BackLeftWheelBone.Transform = wheelRotationX * BackLeftWheelTransform;
			BackRightWheelBone.Transform = wheelRotationX * BackRightWheelTransform;
			CarBone.Transform = CarTransform;

			// Look up combined bone matrices for the entire model.
			Model.CopyAbsoluteBoneTransformsTo(BoneTransforms);
			// For each mesh in the model,
			foreach (var mesh in Model.Meshes)
			{
				// Obtain the world matrix for that mesh (relative to the parent)
				var meshWorld = BoneTransforms[mesh.ParentBone.Index];
				Effect.Parameters["World"].SetValue(meshWorld);
				Effect.Parameters["WorldViewProjection"].SetValue(meshWorld * view * projection);
				Effect.Parameters["NormalWorldMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(meshWorld)));
				mesh.Draw();
				if (ShowGizmos) Gizmos.Draw();
			}
			if (ShowGizmos)
			{
				Gizmos.UpdateViewProjection(view, projection);
				Gizmos.DrawCube(Matrix.CreateScale(BoundingBox.Extents * 2f) * BoundingBox.Orientation * Matrix.CreateTranslation(BoundingBox.Center), Color.Red);
			}
		}

		#region Movement
		public void Drive(float elapsedTime)
		{
			if (CurrentSpeed < 0f)
			{
				Reverse(elapsedTime);
				return;
			}

			if (IsAccelerating)
			{
				if (IsUsingBoost && (GodMode || RemainingBoost > 0f))
				{
					CurrentSpeed += Acceleration[CurrentGear] * DefaultBoostSpeed;
					if (CurrentSpeed > MaxSpeed[CurrentGear] && CurrentGear < MaxSpeed.Length - 1) CurrentGear++;
					CurrentSpeed = CurrentSpeed > MaxSpeed[CurrentGear] * (DefaultBoostSpeed / 10f) ? MaxSpeed[CurrentGear] * (DefaultBoostSpeed / 10f) : CurrentSpeed;
					RemainingBoost = Math.Max(RemainingBoost - elapsedTime, 0f);
				}
				else
				{
					CurrentSpeed += Acceleration[CurrentGear];
					if (CurrentSpeed > MaxSpeed[CurrentGear] && CurrentGear < MaxSpeed.Length - 1) CurrentGear++;
					CurrentSpeed = CurrentSpeed > MaxSpeed[CurrentGear] ? MaxSpeed[CurrentGear] : CurrentSpeed;
				}
			}

			if (IsBraking)
			{
				CurrentSpeed -= DefaultBrakingForce;
				if (CurrentGear > 1 && CurrentSpeed < MaxSpeed[CurrentGear - 1]) CurrentGear--;
			}

			if (!IsAccelerating && !IsBraking)
			{
				CurrentSpeed /= 1 + elapsedTime; // para que vaya desacelerando gradualemente
				if (CurrentGear > 1 && CurrentSpeed < MaxSpeed[CurrentGear - 1]) CurrentGear--;
			}

			CurrentWheelRotation += ToRadians(CurrentSpeed / 10f);
		}

		public void Reverse(float elapsedTime)
		{
			if (IsAccelerating)
			{
				CurrentGear = 0;
				CurrentSpeed -= Acceleration[CurrentGear];
				CurrentSpeed = CurrentSpeed < -MaxSpeed[CurrentGear] ? -MaxSpeed[CurrentGear] : CurrentSpeed;
			}

			if (IsBraking)
			{
				CurrentSpeed += DefaultBrakingForce;
				if (CurrentSpeed > MaxSpeed[1]) CurrentGear++;
			}

			if (!IsAccelerating && !IsBraking)
			{
				CurrentSpeed /= 1 + elapsedTime; // para que vaya desacelerando gradualemente
				if (CurrentSpeed > MaxSpeed[1]) CurrentGear++;
			}

			CurrentWheelRotation += ToRadians(CurrentSpeed / 10f);
		}

		public void Turn()
		{
			if (IsTurningLeft && !IsTurningRight)
			{
				if (CurrentGear != 1)
				{
					Rotation *= Matrix.CreateRotationY(DefaultSteeringSpeed * (CurrentSpeed / MaxSpeed[CurrentGear]));
					Direction = Vector3.Transform(Vector3.Backward, Rotation);
				}
				CurrentSteeringWheelRotation = ToRadians(DefaultSteeringRotation);
			}
			else if (IsTurningRight && !IsTurningLeft)
			{
				if (CurrentGear != 1)
				{
					Rotation *= Matrix.CreateRotationY(-DefaultSteeringSpeed * (CurrentSpeed / MaxSpeed[CurrentGear]));
					Direction = Vector3.Transform(Vector3.Backward, Rotation);
				}
				CurrentSteeringWheelRotation = ToRadians(-DefaultSteeringRotation);
			}
			else CurrentSteeringWheelRotation = 0f;
		}

		public void Jump()
		{
			DirectionSpeed += Vector3.Up * DefaultJumpSpeed;
		}

		private Vector3 CheckForCollisions(Vector3 positionDelta, BaseCollider[] colliders, PowerUp[] powerups)
		{
			BoundingBox.Center += positionDelta;
			BoundingBox.Orientation = Rotation;
			// Check intersection for every active powerup
			for (var index = 0; index < powerups.Length; index++)
			{
				if (powerups[index].IsActive && BoundingBox.Intersects(powerups[index].Collider))
				{
					powerups[index].Hide();
					switch (powerups[index].Type)
					{
						case PowerUpType.Boost:
							RemainingBoost = Math.Min(RemainingBoost + 3f, MaxBoost);
							break;
						case PowerUpType.Missiles:
							RemainingMissiles = 3;
							break;
						case PowerUpType.Shield:
							HasShield = true;
							break;
					}
					return Vector3.Zero;
				}
				continue;
			}
			// Check intersection for every collider
			for (var index = 0; !GodMode && index < colliders.Length; index++)
			{
				if (BoundingBox.Intersects(colliders[index]))
				{
					BoundingBox.Center -= positionDelta * 1.5f;
					CurrentSpeed = - CurrentSpeed * 0.3f;
					CurrentGear = 1;
					return positionDelta * 1.5f;
				}
				continue;
			}
			return Vector3.Zero;
		}
		#endregion

		#region utils
		public void SetKeyboardState(GameTime gameTime)
		{
			KeyboardState keyboardState = Keyboard.GetState();
			bool goingForward = CurrentSpeed >= 0;
			IsAccelerating = goingForward ? keyboardState.IsKeyDown(Keys.W) : keyboardState.IsKeyDown(Keys.S);
			IsBraking = goingForward ? keyboardState.IsKeyDown(Keys.S) : keyboardState.IsKeyDown(Keys.W);
			IsTurningLeft = keyboardState.IsKeyDown(Keys.A);
			IsTurningRight = keyboardState.IsKeyDown(Keys.D);
			IsUsingBoost = keyboardState.IsKeyDown(Keys.LeftShift);
			IsJumping = keyboardState.IsKeyDown(Keys.Space);
			if (IsAbleToChangeGodMode(gameTime) && Keyboard.GetState().IsKeyDown(Keys.P)) EnableGodMode();
			if (IsAbleToChangeGizmosVisibility(gameTime) && Keyboard.GetState().IsKeyDown(Keys.G)) ChangeGizmosVisibility();
		}

		public float ToRadians(float angle)
		{
			return angle * (MathF.PI / 180f);
		}

		public void EnableGodMode()
		{
			GodMode = !GodMode;
			GodModeCooldown = 0f;
		}

		public bool IsAbleToChangeGodMode(GameTime gameTime)
		{
			GodModeCooldown = MathF.Min(GodModeCooldown + Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds), 0.5f);
			return GodModeCooldown == 0.5f;
		}
		public void ChangeGizmosVisibility()
		{
			ShowGizmos = !ShowGizmos;
			GizmosCooldown = 0f;
		}

		public bool IsAbleToChangeGizmosVisibility(GameTime gameTime)
		{
			GizmosCooldown = MathF.Min(GizmosCooldown + Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds), 0.5f);
			return GizmosCooldown == 0.5f;
		}
		#endregion
	}
}
