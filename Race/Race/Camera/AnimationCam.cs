using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Race
{
    internal class AnimationCam : Camera
    {
        public Matrix From
        {
            set { from = value; }
        }
        Matrix from;
        public Matrix To
        {
            set { to = value; }
        }
        Matrix to;

        public bool AnimatingDone
        {
            get { return done; }
        }
        bool done = false;

        GraphicsDevice graphicsDevice;

        //const float duration = 1000.0f;
        //float time = 0.0f;
        const float animationSteps = 100.0f;
        float step = 0.0f;

        public AnimationCam(Matrix from, Matrix to, GraphicsDevice device)
            : base(device)
        {
            this.from = from;
            this.to = to;
            this.graphicsDevice = device;
        }

        public override void Update(GameTime gameTime)
        {
            //this.time += gameTime.ElapsedGameTime.Milliseconds;
            this.View = Matrix.Lerp(this.from, this.to, this.step / animationSteps);//this.time / duration);
            Vector3 scale;
            Quaternion rotation;
            Vector3 position;
            this.View.Decompose(out scale, out rotation, out position);
            this.Position = position;
            if (this.step>=animationSteps)//this.time > duration)
            {
                done = true;
                this.View = this.to;
            }
            this.step++;
        }

        public void SetUpAndRight(Vector3 up, Vector3 right)
        {
            this.Up = up;
            this.Right = right;
        }

    }
}