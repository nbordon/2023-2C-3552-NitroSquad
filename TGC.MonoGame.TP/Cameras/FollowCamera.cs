using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP.Cameras
{
    /// <summary>
    /// Una camara que sigue objetos
    /// </summary>
    class FollowCamera
    {
        public Matrix Projection { get; private set; }

        public Matrix View { get; private set; }

        private float FrontVectorInterpolator { get; set; } = 0f;
		private Vector3 CurrentCameraPosition { get; set; } = Vector3.Forward;
		private Vector3 PreviousCameraPosition = Vector3.Forward;
		private int CameraType = 0;
		private float CameraChangeCooldown = 0f;
		private Viewport Viewport;

		/// <summary>
		/// Crea una FollowCamera que sigue a una matriz de mundo
		/// </summary>
		/// <param name="aspectRatio"></param>
		public FollowCamera(Viewport viewport)
        {
			Viewport = viewport;
			// Perspective camera
			// Uso 60° como FOV, aspect ratio, pongo las distancias a near plane y far plane en 0.1 y 100000 (mucho) respectivamente
			Projection = Matrix.CreatePerspectiveFieldOfView(MathF.PI / 3f, viewport.AspectRatio, 0.1f, 100000f);
		}

		/// <summary>
		/// Actualiza la Camara usando una matriz de mundo actualizada para seguirla
		/// </summary>
		/// <param name="gameTime">The Game Time to calculate framerate-independent movement</param>
		/// <param name="mainCarWorld">The World matrix to follow</param>
		public void Update(GameTime gameTime, Matrix targetWorld)
        {
            var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
			float cameraDepth; // que tan alta va a estar la camara respecto al auto
			float cameraHeight; // que tan alta va a estar la camara respecto al auto
			float angleThreshold = 0.75f; // que tanto se permite rotar la camara antes de acomodarla
			float angleFollowSpeed = 0.015f; // que tan rapido se va a acomodar la camara
			Vector3 newCameraPosition = targetWorld.Forward; // la posicion donde va a estar parada la camara
			Vector3 targetPosition = targetWorld.Translation; // la posicion donde apunta la camara

			switch (CameraType)
			{
				case 0:
					cameraDepth = 2500f;
					cameraHeight = 2500f;
					break;
				case 1:
					cameraDepth = 750f;
					cameraHeight = 100f;
					targetPosition += Vector3.Up * cameraHeight * 2f;
					break;
				case 2:
					cameraDepth = 750f;
					cameraHeight = 100f;
					targetPosition += Vector3.Up * cameraHeight * 2f;
					angleThreshold = 1f; // para no rotar la camara
					break;
				default: // para la vista isometrica
					newCameraPosition = Vector3.One;
					cameraDepth = 1000f;
					cameraHeight = 1000f;
					break;
			};

			if (CameraChangeCooldown == 0f) // Camera has been changed
			{
				CurrentCameraPosition = newCameraPosition; // pongo la camara donde deberia ir una vez que se cambio
				FrontVectorInterpolator = 1f; // para que la transicion sea inmediata
			}
			else
			{
				// Si el producto escalar entre el vector de la posicion anterior y el actual es mas grande que un limite
				if (Vector3.Dot(PreviousCameraPosition, newCameraPosition) > angleThreshold)
				{
					// muevo el Interpolator (desde 0 a 1) mas cerca de 1 sin que pase de 1
					FrontVectorInterpolator = MathF.Min(FrontVectorInterpolator + (elapsedTime * angleFollowSpeed), 1f);

					// Calculo el vector de la posicion a partir de la interpolacion
					// Esto mueve el vector de la posicion para igualar al vector de la posicion que sigo
					// En este caso uso la curva x^2 para hacerlo mas suave
					// Interpolator se convertira en 1 eventualmente
					CurrentCameraPosition = Vector3.Lerp(CurrentCameraPosition, newCameraPosition, FrontVectorInterpolator * FrontVectorInterpolator);
				}
				else
					FrontVectorInterpolator = 0f; // Si el angulo no pasa del limite, lo pongo de nuevo en cero
			}

            // Calculo la posicion de la camara tomando la posicion del auto mas un offset
            var offsetedPosition = targetPosition + CurrentCameraPosition * cameraDepth + Vector3.Up * cameraHeight;

            // Calcular el vector Adelante haciendo la resta entre el destino y el origen
            // y luego normalizandolo (Esta operacion es cara!) (La siguiente operacion necesita vectores normalizados)
            var position = targetPosition - offsetedPosition;
			position.Normalize();

            // Obtengo el vector Derecha asumiendo que la camara tiene el vector Arriba apuntando hacia arriba y no esta rotada en el eje X (Roll)
            var upPosition = Vector3.Cross(position, Vector3.Up);

            // Una vez que tengo la correcta direccion Derecha, obtengo la correcta direccion Arriba usando otro producto vectorial
            var cameraCorrectUp = Vector3.Cross(upPosition, position);

            // Calculo la matriz de Vista de la camara usando la Posicion, La Posicion a donde esta mirando, y su vector Arriba
            View = Matrix.CreateLookAt(offsetedPosition, targetPosition, cameraCorrectUp);

			PreviousCameraPosition = newCameraPosition;
		}

        public void ChangeCamera()
        {
            CameraChangeCooldown = 0f;
            CameraType++;
			if (CameraType == 3)
				Projection = Matrix.CreateOrthographic(Viewport.Width * 3f, Viewport.Height * 3f, 0.01f, 10000f); // Change to Orthographic camera
			else if (CameraType > 3)
			{
				Projection = Matrix.CreatePerspectiveFieldOfView(MathF.PI / 3f, Viewport.AspectRatio, 0.1f, 100000f); // Back to Perspective camera
				CameraType = 0;
			}
		}

		public bool IsAbleToChangeCamera(GameTime gameTime)
		{
			CameraChangeCooldown = MathF.Min(CameraChangeCooldown + Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds), 0.5f);
			return CameraChangeCooldown == 0.5f;
		}
	}
}
