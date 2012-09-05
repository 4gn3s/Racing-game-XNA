using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Race
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        Camera currentCamera;
        CameraType currentCameraType = CameraType.ChaseCam;
        Track track;
        Sky sky;
        BillboardSystem trees;

        GameObject ground;
        List<GameObject> vehicles = new List<GameObject>();
        List<Camera> cameras = new List<Camera>();
        
        MouseState lastMouseState;

        KeyboardState lastKeyboardState = new KeyboardState();
        KeyboardState currentKeyboardState = new KeyboardState();

        public GameState GameState
        {
            get { return currentGameState; }
            set { currentGameState = value; }
        }
        GameState currentGameState;

        bool isPaused = false;
        bool pauseKeyDown = false;
        Texture2D pausedTex;
        public Rectangle pausedTexRect
        {
            get
            {
                return new Rectangle(GraphicsDevice.Viewport.Width/2-pausedTex.Width/2,GraphicsDevice.Viewport.Height/2 - pausedTex.Height/2,pausedTex.Width, pausedTex.Height);
            }
        }

        const int trackWidth = 300;
        Vector2 startingPoint = new Vector2(-4000, 0);

        const float carScale = 0.2f;
        const float opponentScale = 0.4f;

        const int OpponentsCount = 2;

        private enum CameraType
        {
            ChaseCam=0,
            FreeCam, 
            TargetCam//, FirstPersonCam
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            fbDeprofiler.DeProfiler.Run();

            Components.Add(new FPSCounter(this));
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            pausedTex = Content.Load<Texture2D>("paused");

            PrepareTrack();

            PrepareVehicles();

            PrepareEnvironment();

            LoadCameras();
            currentCamera = cameras[0];

            lastMouseState = Mouse.GetState();
            currentGameState = new MenuGS(this);
        }

        private void PrepareVehicles()
        {
            vehicles.Add(new Car(Content.Load<Model>("L200-C4D/L200-FBX"),
    new Vector3(startingPoint, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track));

            for (int i = 1; i < OpponentsCount + 1; i++)
            {
                if (i % 2 == 0)
                {
                    vehicles.Add(new Opponent(Content.Load<Model>("L200-C4D-RED/L200-FBX"), new Vector3(startingPoint.X + (i - 1) * trackWidth / OpponentsCount, 0, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track, (i - 1) * trackWidth / OpponentsCount));
                }
                else
                {
                    vehicles.Add(new Opponent(Content.Load<Model>("L200-C4D-GRE/L200-FBX"), new Vector3(startingPoint.X - i * trackWidth / OpponentsCount, 0, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track, -i * trackWidth / OpponentsCount));
                }

            }
        }

        private void PrepareTrack()
        {
            List<Vector2> trackPoints = new List<Vector2>() {
                startingPoint,
                new Vector2(-4000,2000),
                new Vector2(-2000,2000),
                new Vector2(-2000,4000),
                new Vector2(0, 4000),
                new Vector2(1000,2000),
                new Vector2(-1000,-2000),
                new Vector2(4000, -2000),
                new Vector2(4000, -4000),
                new Vector2(2000,-4000),
                new Vector2(0,-6000),
                new Vector2(-2000,-6000),
                new Vector2(-4000, -4000),
                startingPoint
            };

            track = new Track(trackPoints, 25, trackWidth, 30, GraphicsDevice, Content);
            Vector2 directionOnTrack;
            startingPoint = track.TracePath(0, out directionOnTrack);
        }

        private void PrepareEnvironment()
        {
            ground = new GameObject(Content.Load<Model>("ground"),
new Vector3(0, -10f, 0), Vector3.Zero, Vector3.One, GraphicsDevice);

            sky = new Sky(Content, GraphicsDevice, Content.Load<TextureCube>("clouds"));

            List<Vector3> treePositions = new List<Vector3>();
            //for (int i = 0; i < track.trackVertices.Length; i++)
            //    treePositions.Add(track.trackVertices[i].Position);
            Random r = new Random();

            for (int i = 0; i < 500; i++) // 500
            {
                treePositions.Add(new Vector3((float)r.NextDouble() * 20000 - 10000, 200, (float)r.NextDouble() * 20000 - 10000));
            }

            trees = new BillboardSystem(GraphicsDevice, Content,
                Content.Load<Texture2D>("tree_billboard"), new Vector2(500),
                treePositions.ToArray());

            trees.Mode = BillboardSystem.BillboardMode.Cylindrical;
            trees.EnsureOcclusion = true;
        }

        protected override void UnloadContent()
        {
        }

        private void LoadCameras()
        {
            //TODO poprawic wlasciwosci kamer
            cameras.Add(new ChaseCam(new Vector3(0, 150, 400), new Vector3(0, 100, 0),
                new Vector3(0, MathHelper.Pi, 0), GraphicsDevice, vehicles[0]));
            cameras.Add(new FreeCam(new Vector3(1000, 0, -2000), MathHelper.ToRadians(153), MathHelper.ToRadians(5),
                GraphicsDevice));
            cameras.Add(new TargetCam(new Vector3(1000, 0, -2000), Vector3.Zero, GraphicsDevice, vehicles[0]));
        }

        private void ChangeCamera(CameraType type)
        {
            currentCameraType = type;
            currentCamera = cameras[(int)type];
        }

        private void ParseInput(KeyboardState state)
        {
            if (state.IsKeyDown(Keys.R))
            {
                //car_.Reset();
                //camera.Reset();
            }
            else if (state.IsKeyDown(Keys.C))
            {
                ChangeCamera(CameraType.ChaseCam);
            }
            else if (state.IsKeyDown(Keys.F))
            {
                ChangeCamera(CameraType.FreeCam);
            }
            else if (state.IsKeyDown(Keys.T))
            {
                ChangeCamera(CameraType.TargetCam);
            }
            else if (state.IsKeyDown(Keys.P))
            {
                pauseKeyDown = true; 
            }
            else if (pauseKeyDown)
            {
                pauseKeyDown = false;
                isPaused = !isPaused;
            }
            else if (state.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            currentGameState.Update(gameTime);

            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            ParseInput(currentKeyboardState);

            if (!isPaused)
            {
                foreach (Camera cam in cameras)
                    cam.Update(gameTime);

                base.Update(gameTime);
            }
        }

        public void UpdateGameInProgress(GameTime gameTime)
        {
            if (isPaused)
                return;

            foreach (GameObject model in vehicles)
                model.Update(gameTime);

            CheckForCollisions();

            if (CheckForGameFinished())
                GameState = new FinishedGS(this);
        }

        private bool CheckForGameFinished()
        {
            return false;
        }

        private void CheckForCollisions()
        {
            foreach (GameObject car1 in vehicles)
            {
                foreach (GameObject car2 in vehicles)
                {
                    if (!car1.Equals(car2) && car1.BoundingSphere.Intersects(car2.BoundingSphere))
                    {
                        //alkjds;fjla
                    }
                }
            }            
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawGameInProgress(gameTime);
            currentGameState.Draw(gameTime);

            base.Draw(gameTime);
        }

        public void DrawGameInProgress(GameTime gameTime)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            sky.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);
            ground.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);
            trees.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);

            track.Draw(currentCamera.View, currentCamera.Projection);

            foreach (GameObject model in vehicles)
                if (currentCamera.IsInView(model.BoundingSphere))
                    model.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);

            if (isPaused)
                DrawPaused();
        }

        private void DrawPaused()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(pausedTex, pausedTexRect, Color.White);
            spriteBatch.End();
        }

    }
}
