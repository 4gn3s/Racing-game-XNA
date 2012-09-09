using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Race
{
    internal class Car : Vehicle
    {
        const float TurnSpeed = 0.025f;
        const float Brake = 0.97f;
        const float Friction = 0.99f;

        public float FacingDirection
        {
            get { return facingDirection; }
        }
        float facingDirection;
        float turnAmount = 0;

        Matrix orientation = Matrix.Identity;

        bool autoDriving = false;
        public bool AutoDriving
        {
            get { return autoDriving; }
            set { autoDriving = value; }
        }

        public Car(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice, Track track, string name, int laps)
            : base(Model, Position, Rotation, Scale, graphicsDevice,track, name, laps)
        {
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyState.IsKeyDown(Keys.Up))
                speed = 50.0f * dt * Mass;
            else if (keyState.IsKeyDown(Keys.Down))
                speed = -20.0f * dt * Mass;
            else
                speed *= Friction;

            if (keyState.IsKeyDown(Keys.Space))
                speed *= Brake;

            if (autoDriving)
            {
                timeElapsed += gameTime.ElapsedGameTime;
                distance += speed * dt;
                if (distance >= track.TrackLength)
                {
                    distance -= track.TrackLength;
                    lapsLeft--;
                }
                Vector2 direction;
                Vector2 trackPosition = track.TracePath(distance, out direction);
               // Console.WriteLine("trackpos" + trackPosition + " dist" + distance);
                float rotation = (float)Math.Acos(direction.Y > 0 ? -direction.X : direction.X);
                if (direction.Y > 0)
                    rotation += MathHelper.Pi;
                rotation += MathHelper.PiOver2;
                this.wheelSteerMatrix = Matrix.CreateRotationY(0);
                this.Rotation = new Vector3(this.Rotation.X, rotation, this.Rotation.Z);
                Vector3 newPosition = new Vector3(trackPosition.X, this.Position.Y, trackPosition.Y);
                float distanceMoved = Vector3.Distance(this.Position, newPosition);
                //if (distanceMoved > 50.0f)
                //    Console.WriteLine(trackPosition.ToString() + " " + distance + "AAAAAAAAAAAAAAAAAAAA");
                //else
                //    Console.WriteLine(trackPosition.ToString() + " " + distance);
                //Console.WriteLine("dist" +distanceMoved);
                //Console.WriteLine("s*dt"  + speed * dt);
                //if (Math.Abs(speed * dt - distanceMoved) > 10.0f)
                //{
                //    float amt = speed * dt / distanceMoved;
                //    Vector2 tmp =Vector2.Lerp(new Vector2(Position.X, Position.Z), trackPosition,amt);
                //    newPosition = new Vector3(tmp.X, this.Position.Y, tmp.Y);
                //    //Console.WriteLine(newPosition + "  " + Position);
                //}
                float theta = distanceMoved / WheelRadius;
                int rollDirection = speed > 0 ? 1 : -1;
                this.wheelRollMatrix *= Matrix.CreateRotationX(theta * rollDirection);
                this.Position = newPosition;
                //Console.WriteLine(this.Position.ToString());
            }
            else
            {
                if (keyState.IsKeyDown(Keys.Left))
                    turnAmount += MathHelper.ToRadians(5) * dt * 50.0f;
                else if (keyState.IsKeyDown(Keys.Right))
                    turnAmount -= MathHelper.ToRadians(5) * dt * 50.0f;

                turnAmount *= 0.9f;

                if (!track.IsOnTrack(this.Position))
                {
                    if (!track.IsOnTrackOrRoadside(this.Position))
                        speed *= 0.5f;
                    else
                        speed *= 0.7f;
                }

                this.facingDirection = this.Rotation.Y;
                this.facingDirection += MathHelper.Clamp(this.turnAmount, -1, 1) * TurnSpeed;

                this.wheelSteerMatrix = Matrix.CreateRotationY(turnAmount);

                this.Rotation = new Vector3(this.Rotation.X, this.facingDirection, this.Rotation.Z);

                this.orientation = Matrix.CreateRotationY(this.facingDirection);
                Vector3 newPosition = this.Position + Vector3.Transform(new Vector3(0, 0, speed * dt), this.orientation);
                float distanceMoved = Vector3.Distance(this.Position, newPosition);
                float theta = distanceMoved / WheelRadius;
                int rollDirection = speed > 0 ? 1 : -1;
                this.wheelRollMatrix *= Matrix.CreateRotationX(theta * rollDirection);

                this.Position = newPosition;
            }
        }



    }
}
