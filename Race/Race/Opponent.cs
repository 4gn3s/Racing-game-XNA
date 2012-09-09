using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Race
{
    internal class Opponent : Vehicle
    {
        private float XTranslation;

        protected float initialRotation=0.0f;
        protected Vector2 trackPosition=Vector2.Zero;
        protected Vector2 direction;

        float myCoefficient;

        public Opponent(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice, Track track, float XTranslation, string name, int laps, float coef)
            : base(Model, Position, Rotation, Scale, graphicsDevice, track, name, laps)
        {

            this.XTranslation = XTranslation;
            this.myCoefficient = coef;
        }

        public override void Update(GameTime gameTime)
        {
            trackPosition = track.TracePath(distance, out direction);
            if (speed < 0)
                speed = 0;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            speed = this.myCoefficient * dt * Mass;
            distance += speed ;
            timeElapsed += gameTime.ElapsedGameTime;
            if (distance >= track.TrackLength)
            {
                distance = 0;
                lapsLeft--;
            }

            float rot = initialRotation + (float)Math.Acos(direction.Y > 0 ? -direction.X : direction.X);

            if (direction.Y > 0)
                rot += MathHelper.Pi;
            rot += MathHelper.PiOver2;

            Vector3 newPosition = new Vector3(trackPosition.X + XTranslation, 0, trackPosition.Y + XTranslation);
            this.Rotation = new Vector3(0, rot, 0);

            float distanceMoved = Vector3.Distance(this.Position, newPosition);
            //if (Math.Abs(speed * dt - distanceMoved) > 70.0f)
            //{
            //    float amt = speed * dt / distanceMoved;
            //    Vector2 tmp = Vector2.Lerp(new Vector2(Position.X, Position.Z), trackPosition, amt);
            //    newPosition = new Vector3(tmp.X, this.Position.Y, tmp.Y);
            //    //Console.WriteLine(newPosition + "  " + Position);
            //}
            float theta = distanceMoved / WheelRadius;
            int rollDirection = speed > 0 ? 1 : -1;

            //this.wheelSteerMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(2*rot));
            this.wheelRollMatrix *= Matrix.CreateRotationX(theta * rollDirection);

            this.Position = newPosition;
            
        }

    }
}