using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Race
{
    class Car : GameObject
    {
        const float WheelRadius = 18;
        const float TurnSpeed = 0.025f;
        const float Brake = 0.97f;
        const float Mass = 1000.0f;
        const float Friction = 0.99f;

        public float FacingDirection
        {
            get { return facingDirection; }
        }
        float facingDirection;
        float turnAmount = 0;
        float speed;

        Matrix orientation = Matrix.Identity;
        Matrix wheelRollMatrix = Matrix.Identity;
        Matrix wheelSteerMatrix = Matrix.Identity;

        ModelBone leftBackWheelBone;
        ModelBone rightBackWheelBone;
        ModelBone leftFrontWheelBone;
        ModelBone rightFrontWheelBone;

        Matrix leftBackWheelTransform;
        Matrix rightBackWheelTransform;
        Matrix leftFrontWheelTransform;
        Matrix rightFrontWheelTransform;

        Track track;

        public Car(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice, Track track)
            : base(Model, Position, Rotation, Scale, graphicsDevice)
        {
            this.track = track;

            leftBackWheelBone = Model.Bones["Left_Rear"];
            rightBackWheelBone = Model.Bones["Right_Rear"];
            leftFrontWheelBone = Model.Bones["Left_Front"];
            rightFrontWheelBone = Model.Bones["Right_Front"];

            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (keyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.Left))
                turnAmount += MathHelper.ToRadians(5)*dt*50.0f;
            else if (keyState.IsKeyDown(Keys.D) || keyState.IsKeyDown(Keys.Right))
                turnAmount -= MathHelper.ToRadians(5)*dt*50.0f;

            turnAmount *= 0.9f;

            if (keyState.IsKeyDown(Keys.W) || keyState.IsKeyDown(Keys.Up))
            {
                speed = 50.0f * dt * Mass;
            }
            else if (keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.Down))
            {
                speed = -20.0f * dt * Mass;
            }
            else
                speed *= Friction;

            if (keyState.IsKeyDown(Keys.Space))
                speed *= Brake;

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
            //var currentHeadingOffset = (wheelBaseLength / 2f) *
            //                    new Vector2(
            //                        (float)Math.Cos(carHeading),
            //                        (float)Math.Sin(carHeading));
            //var frontWheel = carLocation + currentHeadingOffset;
            //var backWheel = carLocation - currentHeadingOffset;

            //var nextHeadingOffset = carSpeed *
            //                dt *
            //                new Vector2(
            //                    (float)Math.Cos(carHeading),
            //                    (float)Math.Sin(carHeading));

            //var nextHeadingPlusSteeringOffset = carSpeed *
            //                dt *
            //                new Vector2(
            //                    (float)Math.Cos(carHeading + steeringAngle),
            //                    (float)Math.Sin(carHeading + steeringAngle));

            //backWheel += nextHeadingOffset;
            //frontWheel += nextHeadingPlusSteeringOffset;

            //carLocation = (frontWheel + backWheel) / 2f;
            //carHeading = (float)Math.Atan2(frontWheel.Y - backWheel.Y, frontWheel.X - backWheel.X);
        }

        public override void Draw(Matrix View, Matrix Projection, Vector3 Camera)
        {
            leftBackWheelBone.Transform = wheelRollMatrix * leftBackWheelTransform;
            rightBackWheelBone.Transform = wheelRollMatrix * rightBackWheelTransform;
            leftFrontWheelBone.Transform = wheelRollMatrix * wheelSteerMatrix * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = wheelRollMatrix * wheelSteerMatrix * rightFrontWheelTransform;

            Model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Matrix worldMatrix = Matrix.CreateScale(Scale)
                * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z)
                * Matrix.CreateTranslation(Position);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = View;
                    effect.Projection = Projection;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }
    }
}
