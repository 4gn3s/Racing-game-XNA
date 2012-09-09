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

        #region CAMERAS

        Camera currentCamera;
        CameraType currentCameraType = CameraType.ChaseCam;

        List<Camera> cameras = new List<Camera>();

        bool changeCamTarget = false;
        int currentCamTarget = 0;

        bool changingCamera = false;

        private enum CameraType
        {
            ChaseCam = 0,
            FreeCam,
            TargetCam,
            InsideCam
        };

        #endregion

        #region ENVIRONMENT

        GameObject ground;
        Track track;
        //Sky sky;
        BillboardCross trees1;
        BillboardSystem trees2;
        BillboardSystem startSign;
        BillboardSystem clouds;

        const int trackWidth = 300;
        Vector2 startingPoint = new Vector2(-4000, 0);

        const float groundSize = 10000.0f;

        #endregion

        #region CARS

        List<Vehicle> vehicles = new List<Vehicle>();

        internal Car MyCar
        {
            get { return vehicles[0] as Car; }
        }
        
        const float carScale = 0.2f;
        const float opponentScale = 0.4f;
        const int opponentsCount = 2;

        #endregion

        #region INPUT

        MouseState lastMouseState;

        KeyboardState lastKeyboardState = new KeyboardState();
        KeyboardState currentKeyboardState = new KeyboardState();

        #endregion

        #region GAMESTATES

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

        #endregion

        const int lapsCount = 1;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            fbDeprofiler.DeProfiler.Run();

            Components.Add(new FPSCounter(this));

            //graphics.PreferredBackBufferWidth = 1280;
            //graphics.PreferredBackBufferHeight = 800;
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
    new Vector3(startingPoint, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track, "ME", lapsCount));

            for (int i = 1; i < opponentsCount + 1; i++)
            {
                if (i % 2 == 0)
                {
                    vehicles.Add(new Opponent(Content.Load<Model>("L200-C4D-RED/L200-FBX"), new Vector3(startingPoint.X + (i - 1) * trackWidth / opponentsCount, 0, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track, (i - 1) * trackWidth / opponentsCount, "red_" + i.ToString(), lapsCount, 0.85f));
                }
                else
                {
                    vehicles.Add(new Opponent(Content.Load<Model>("L200-C4D-GRE/L200-FBX"), new Vector3(startingPoint.X - i * trackWidth / opponentsCount, 0, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track, -i * trackWidth / opponentsCount, "green_" + i.ToString(), lapsCount, 0.83f));
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

            //List<Vector2> trackPoints = new List<Vector2>()
            //{
            //    startingPoint,
            //    new Vector2(-4000,-4000),
            //    new Vector2(0,-4000),
            //    new Vector2(0,0),
            //    startingPoint
            //};

            track = new Track(trackPoints, 25, trackWidth, 30, GraphicsDevice, Content);
            Vector2 directionOnTrack;
            startingPoint = track.TracePath(0, out directionOnTrack);
        }

        private void PrepareEnvironment()
        {
            ground = new GameObject(Content.Load<Model>("ground"),
new Vector3(0, -10f, 0), Vector3.Zero, Vector3.One, GraphicsDevice);

            //sky = new Sky(Content, GraphicsDevice, Content.Load<TextureCube>("clouds"));

            List<Vector3> treePositions = new List<Vector3>();
            Random r = new Random();

            for (int i = 0; i < 250; i++) 
            {
                Vector3 pos=new Vector3((float)r.NextDouble() * 20000 - groundSize, 0, (float)r.NextDouble() * 20000 - groundSize);
                if (!track.IsOnTrackOrRoadside(pos))
                    treePositions.Add(new Vector3(pos.X, 240, pos.Z));
                else
                    Console.WriteLine("is on track " + pos);
            }

            trees1 = new BillboardCross(GraphicsDevice, Content,
                Content.Load<Texture2D>("tree_billboard"), new Vector2(500),
                treePositions.ToArray());
            trees1.EnsureOcclusion = true;
            treePositions.Clear();
            for (int i = 0; i < 250; i++)
            {
                Vector3 pos = new Vector3((float)r.NextDouble() * 20000 - groundSize, 0, (float)r.NextDouble() * 20000 - groundSize);
                if (!track.IsOnTrackOrRoadside(pos))
                    treePositions.Add(new Vector3(pos.X, 240, pos.Z));
                else
                    Console.WriteLine("is on track " + pos);
            }

            trees2 = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("tree"), new Vector2(500), treePositions.ToArray());
            trees2.EnsureOcclusion = true;

            Vector3 [] position=new Vector3[1];
            position[0]=new Vector3(startingPoint, 0);
            startSign = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("checkerd"), new Vector2(800), position);

            Vector3[] cloudPositions = new Vector3[350];

            for (int i = 0; i < cloudPositions.Length; i++)
            {
                cloudPositions[i] = new Vector3(
                    r.Next(-6000, 6000),
                    r.Next(1000, 3000),
                    r.Next(-6000, 6000));
            }

            clouds = new BillboardSystem(GraphicsDevice, Content,
                Content.Load<Texture2D>("cloud2"), new Vector2(1000),
                cloudPositions);

            clouds.EnsureOcclusion = false;
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        private void LoadCameras()
        {
            Car car = vehicles[0] as Car;
            //TODO poprawic wlasciwosci kamer
            cameras.Add(new ChaseCam(new Vector3(0, 150, 400), new Vector3(0, 100, 0),
                new Vector3(0, MathHelper.Pi, 0), GraphicsDevice, car));
            cameras.Add(new FreeCam(new Vector3(-2500,1000,-2000), MathHelper.ToRadians(153), MathHelper.ToRadians(5),
                GraphicsDevice));
            cameras.Add(new TargetCam(new Vector3(1000, 0, -2000), Vector3.Zero, GraphicsDevice, car));
            cameras.Add(new InsideCam(new Vector3(0, 70, -50), new Vector3(0, 70, 0), new Vector3(0, 0, 0), GraphicsDevice, car));
        }

        private void ChangeCamera(CameraType type)
        {
            currentCameraType = type;
            currentCamera = new AnimationCam(currentCamera.View, cameras[(int)type].View, GraphicsDevice);
            changingCamera = true;
        }

        private void ParseInput(KeyboardState state)
        {
            if (state.IsKeyDown(Keys.F1))
            {
                ChangeCamera(CameraType.ChaseCam);
            }
            else if (state.IsKeyDown(Keys.F2))
            {
                ChangeCamera(CameraType.FreeCam);
            }
            else if (state.IsKeyDown(Keys.F3))
            {
                ChangeCamera(CameraType.TargetCam);
            }
            else if (state.IsKeyDown(Keys.F4))
            {
                ChangeCamera(CameraType.InsideCam);
            }
            else if (state.IsKeyDown(Keys.N))
            {
                if (currentCameraType == CameraType.ChaseCam)
                    changeCamTarget = true;
            }
            else if (state.IsKeyDown(Keys.P))
            {
                pauseKeyDown = true;
            }
            else if (state.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }
            else if (pauseKeyDown)
            {
                isPaused = !isPaused;
                this.IsMouseVisible = !this.IsMouseVisible;
                pauseKeyDown = false;
            }
            else if (changeCamTarget)
            {
                if (currentCameraType == CameraType.ChaseCam && currentCamera.GetType().ToString()!="Race.AnimationCam")
                {
                    currentCamTarget = (currentCamTarget + 1) % vehicles.Count;
                    (currentCamera as ChaseCam).MyTarget = vehicles[currentCamTarget];
                    changeCamTarget = false;
                }
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

                if (changingCamera)
                {
                    currentCamera.Update(gameTime);
                    (currentCamera as AnimationCam).SetUpAndRight(cameras[(int)currentCameraType].Up, cameras[(int)currentCameraType].Right);
                    if (Vector3.Distance(currentCamera.Position,cameras[(int)currentCameraType].Position)<30.0f || (currentCamera as AnimationCam).AnimatingDone)
                    {
                        currentCamera = cameras[(int)currentCameraType];
                        changingCamera = false;
                    }
                    else
                    {
                        (currentCamera as AnimationCam).From = currentCamera.View;
                        (currentCamera as AnimationCam).To = cameras[(int)currentCameraType].View;
                    }
                }

                base.Update(gameTime);
            }

            //this.Window.Title = vehicles[0].Position.ToString();
        }

        public void UpdateGameInProgress(GameTime gameTime)
        {
            UpdateGame(gameTime, vehicles);

            if (!(GameState is FreeRideGS) && CheckForGameFinished())
                GameState = new FinishedGS(this, vehicles);
        }

        public void UpdateGame(GameTime gameTime, List<Vehicle> activePlayers)
        {
            if (isPaused)
                return;

            foreach (Vehicle model in activePlayers)
                model.Update(gameTime);

            //CheckForCollisions();
            CheckIsOnGround();
        }

        private void CheckIsOnGround()
        {
            Car car=vehicles[0] as Car;
            float bound=groundSize-1000.0f;
            if (car.Position.X > bound)
                car.Position = new Vector3(bound, car.Position.Y, car.Position.Z);
            else if(car.Position.X < -bound)
                car.Position = new Vector3(-bound, car.Position.Y, car.Position.Z);
            if (car.Position.Z > bound)
                car.Position = new Vector3(car.Position.X, car.Position.Y, bound);
            else if (car.Position.Z < -bound)
                car.Position = new Vector3(car.Position.X, car.Position.Y, -bound);
        }

        private bool CheckForGameFinished()
        {
            foreach (Vehicle vehicle in vehicles)
                if (vehicle.FinishedRace())
                    return true;
            return false;
        }

        private void CheckForCollisions()
        {
            foreach (Vehicle car1 in vehicles)
            {
                foreach (Vehicle car2 in vehicles)
                {
                    if (!car1.Equals(car2) && car1.BoundingSphere.Intersects(car2.BoundingSphere))
                    {
                        this.Window.Title = "KOLIZJA";
                        return;
                    }
                }
            }
            this.Window.Title = "";
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightSteelBlue);
            DrawGameInProgress(gameTime);
            currentGameState.Draw(gameTime);

            base.Draw(gameTime);
        }

        public void DrawGameInProgress(GameTime gameTime)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            //sky.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);
            ground.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);
            trees1.Draw(currentCamera.View, currentCamera.Projection);
            trees2.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);
            clouds.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);
            startSign.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, Vector3.Right);
            track.Draw(currentCamera.View, currentCamera.Projection);

            foreach (Vehicle model in vehicles)
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
