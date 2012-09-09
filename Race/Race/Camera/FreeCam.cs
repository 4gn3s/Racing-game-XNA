using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Race
{
    class FreeCam : Camera
    {
        public float Yaw { get; set; }
        public float Pitch { get; set; }

        public Vector3 Target { get; private set; }

        private Vector3 translation;
        private MouseState lastMouseState;

        public FreeCam(Vector3 Position, float Yaw, float Pitch,
            GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            this.Position = Position;
            this.Yaw = Yaw;
            this.Pitch = Pitch;

            translation = Vector3.Zero;
            lastMouseState = Mouse.GetState();
        }

        public void Rotate(float YawChange, float PitchChange)
        {
            this.Yaw += YawChange;
            this.Pitch += PitchChange;
        }

        public void Move(Vector3 Translation)
        {
            this.translation += Translation;
        }

        public override void Update()
        {
            Matrix rotation = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, 0);
            translation = Vector3.Transform(translation, rotation);
            Position += translation;
            translation = Vector3.Zero;
            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            Target = Position + forward;
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);
            View = Matrix.CreateLookAt(Position, Target, up);
            this.Up=up;
            this.Right = Vector3.Cross(forward,up);
        }

        public override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            float deltaX = (float)lastMouseState.X - (float)mouseState.X;
            float deltaY = (float)lastMouseState.Y - (float)mouseState.Y;

            Rotate(deltaX * .01f, deltaY * .01f);

            Vector3 translation = Vector3.Zero;
            if (keyState.IsKeyDown(Keys.W)) translation += Vector3.Forward;
            if (keyState.IsKeyDown(Keys.S)) translation += Vector3.Backward;
            if (keyState.IsKeyDown(Keys.A)) translation += Vector3.Left;
            if (keyState.IsKeyDown(Keys.D)) translation += Vector3.Right;

            translation *= 3 * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Move(translation);
            Update();

            lastMouseState = mouseState;
        }
    }
}
