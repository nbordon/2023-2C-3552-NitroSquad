using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Cars;
using TGC.MonoGame.TP.Scenarios;

namespace TGC.MonoGame.TP
{

    /// <summary>
    ///     Esta es la clase principal del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
	{
		public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";
        private RacingCar MainCar;
		private TestScenario Scenario;
		private FollowCamera Camera;
		/// <summary>
		///     Constructor del juego.
		/// </summary>
		public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // Enciendo Back-Face culling.
            // Configuro Blend State a Opaco.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
            GraphicsDevice.RasterizerState = rasterizerState;
            GraphicsDevice.BlendState = BlendState.Opaque;

            // Configuro las dimensiones de la pantalla.
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 250;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 250;
            Graphics.ApplyChanges();

			// Creo una camara para seguir a nuestro auto.
			Camera = new FollowCamera(GraphicsDevice.Viewport);

			base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

			// La carga de contenido debe ser realizada aca.
			Scenario = new TestScenario(Content, GraphicsDevice);
			MainCar = new RacingCar(Content, GraphicsDevice, Scenario.MainCarStartRotation, Scenario.MainCarStartPosition);

			base.LoadContent();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logica de actualizacion del juego.
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit(); //Salgo del juego.
			if (Scenario.IsAbleToChangeGizmosVisibility(gameTime) && Keyboard.GetState().IsKeyDown(Keys.G)) Scenario.ChangeGizmosVisibility(); //Activo/Desactivo el gizmos
			if (Camera.IsAbleToChangeCamera(gameTime) && Keyboard.GetState().IsKeyDown(Keys.V)) Camera.ChangeCamera();

			// La logica debe ir aca.
			Scenario.Update(gameTime);
			MainCar.Update(gameTime, Scenario.Colliders, Scenario.PowerUps);

			// Actualizo la camara, enviandole la matriz de mundo del auto.
			Camera.Update(gameTime, MainCar.World);

			base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logica de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);

			Scenario.Draw(Camera.View, Camera.Projection, Camera.CurrentCameraPosition);
			MainCar.Draw(Camera.View, Camera.Projection, Camera.CurrentCameraPosition);
		}

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
			// Libero los recursos.
			Scenario.Dispose();
			Content.Unload();

            base.UnloadContent();
        }
    }
}