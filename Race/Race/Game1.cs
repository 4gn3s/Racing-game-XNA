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
        SpriteBatch spriteBatch;

        Camera currentCamera;
        CameraType currentCameraType = CameraType.ChaseCam;
        Track track;
        Sky sky;
        BillboardSystem trees;

        List<GameObject> objects = new List<GameObject>();
        List<Camera> cameras = new List<Camera>();
        
        MouseState lastMouseState;

        KeyboardState lastKeyboardState = new KeyboardState();
        KeyboardState currentKeyboardState = new KeyboardState();

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

            objects.Add(new Car(Content.Load<Model>("L200-C4D/L200-FBX"), 
                new Vector3(startingPoint, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track));

            objects.Add(new GameObject(Content.Load<Model>("ground"),
                new Vector3(0, -10f, 0), Vector3.Zero, Vector3.One, GraphicsDevice));

            for (int i = 1; i < OpponentsCount+1; i++)
            {
                if (i % 2 == 0)
                {
                    objects.Add(new Opponent(Content.Load<Model>("L200-C4D-RED/L200-FBX"), new Vector3(startingPoint.X + (i-1) * trackWidth / OpponentsCount, 0, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track, (i-1) * trackWidth / OpponentsCount));
                }
                else
                {
                    objects.Add(new Opponent(Content.Load<Model>("L200-C4D-GRE/L200-FBX"), new Vector3(startingPoint.X - i * trackWidth / OpponentsCount, 0, 0), Vector3.Zero, new Vector3(carScale), GraphicsDevice, track, -i * trackWidth / OpponentsCount));
                }

            }

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

            LoadCameras();
            currentCamera = cameras[0];

            lastMouseState = Mouse.GetState();
        }

        protected override void UnloadContent()
        {
        }

        private void LoadCameras()
        {
            //TODO poprawic wlasciwosci kamer
            cameras.Add(new ChaseCam(new Vector3(0, 150, 400), new Vector3(0, 100, 0),
                new Vector3(0, MathHelper.Pi, 0), GraphicsDevice, objects[0]));
            cameras.Add(new FreeCam(new Vector3(1000, 0, -2000), MathHelper.ToRadians(153), MathHelper.ToRadians(5),
                GraphicsDevice));
            cameras.Add(new TargetCam(new Vector3(1000, 0, -2000), Vector3.Zero, GraphicsDevice, objects[0]));
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
        }

        protected override void Update(GameTime gameTime)
        {
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            ParseInput(currentKeyboardState);

            foreach (Camera cam in cameras)
                cam.Update(gameTime);

            foreach (GameObject model in objects)
                model.Update(gameTime);

            this.Window.Title = objects[2].Position.ToString();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            sky.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);

            trees.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Up, currentCamera.Right);

            track.Draw(currentCamera.View, currentCamera.Projection);

            foreach (GameObject model in objects)
                if (currentCamera.IsInView(model.BoundingSphere))
                    model.Draw(currentCamera.View, currentCamera.Projection, currentCamera.Position);

            base.Draw(gameTime);
        }

    }
}
