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
        const float MaxVelocity = 10;
        const float WheelRadius = 18;
        const float TurnSpeed = 0.025f;

        const float Friction = 0.6f;
        const float maxAcceleration = 0.8f;
        const float maxRotationAcc = 1.0f;
        const float maxSpeed = 25.0f;
        const float brake = 0.8f;

        const float step = 1.0f;

        //const float EngineTorque = 10000.0f;
        //const float Mass =  1000.0f;
        //const float Gravity = 9.81f;
        //const float MaxSpeed = 300;
        //protected static float maxSpeed = MaxSpeed * 1.05f;
        //const float BrakeSlowdown = 1.0f; //means it takes 1 sec to stop
        //const float MaxRotationPerSecond = 1.25f;
        //const float
        //    MaxAcceleration = 6.75f,
        //    MinAcceleration = -3.25f;
        //protected static float maxAccelerationPerSecond =2.85f * 0.85f;

        //Vector3 forceOnCar = Vector3.Zero;
        Vector3 velocity = Vector3.Zero;

        public float FacingDirection
        {
            get { return facingDirection; }
        }
        
        float facingDirection;

        float turnAmount = 0;

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

            velocity = new Vector3(0, 0, 1);
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            turnAmount *= 0.95f;

            if (keyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.Left))
                turnAmount += 1;
            else if (keyState.IsKeyDown(Keys.D) || keyState.IsKeyDown(Keys.Right))
                turnAmount -= 1;
            else
                turnAmount = 0;

            //Vector3 frictionForce = Vector3.Zero;

            //if (velocity != Vector3.Zero)
            //{
            //    frictionForce -= velocity;
            //    frictionForce.Normalize();
            //    frictionForce *= Friction;
            //}
            

            Vector3 movement = Vector3.Zero;
            if (keyState.IsKeyDown(Keys.W) || keyState.IsKeyDown(Keys.Up)){
                movement.Z += step;
                movement *=maxAcceleration*10;
            }
            if (keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.Down)){
                movement.Z -= step;
                movement *= brake;
            }

            this.turnAmount = MathHelper.Clamp(turnAmount, -30, 30);

            //if (this.velocity.LengthSquared() < MaxVelocity / 4)
            //    this.turnAmount *= 0.25f; //zmniejsz szybkosc obrotu przy malej predkosci

            this.wheelSteerMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(turnAmount));

            //movement += frictionForce;

            this.facingDirection = this.Rotation.Y;
            this.facingDirection += MathHelper.Clamp(this.turnAmount, -1, 1) * TurnSpeed;
            this.Rotation = new Vector3(this.Rotation.X, this.facingDirection, this.Rotation.Z);

            this.orientation = Matrix.CreateRotationY(this.facingDirection);

            //Vector3 newVelocity = Vector3.Transform(limitVector(movement,maxAcceleration), orientation);
            ////newVelocity = limitVector(newVelocity, maxAcceleration);
            ////velocity += newVelocity;
            ////velocity = limitVector(velocity, maxSpeed);
            //velocity = newVelocity*25.0f;

            Vector3 newVelocity = Vector3.Transform(movement,this.orientation);
            //this.velocity += newVelocity;
            //this.velocity = limitVector(this.velocity, maxSpeed);
            //newVelocity *= MaxVelocity;

            //Vector3 newPosition = this.Position + this.velocity;//newVelocity;
            Vector3 newPosition = this.Position + newVelocity;// *gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            float distanceMoved = Vector3.Distance(this.Position, newPosition);
            float theta = distanceMoved / WheelRadius;
            int rollDirection = movement.Z > 0 ? 1 : -1;

            this.wheelRollMatrix *= Matrix.CreateRotationX(theta * rollDirection);

            this.Position = newPosition;
    //        float moveFactor = 0.25f;
    //        float maxRot = MaxRotationPerSecond * moveFactor * 1.25f;
    //        rotationChange *= 0.95f;

    //        if (keyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.Left))
    //            rotationChange += MaxRotationPerSecond * moveFactor / 2.5f;// 2;
    //        else if (keyState.IsKeyDown(Keys.D) || keyState.IsKeyDown(Keys.Right))
    //            rotationChange -= MaxRotationPerSecond * moveFactor / 2.5f;//2;
    //        else
    //            rotationChange = 0;

    //        // If we are staying or moving very slowly, limit rotation!
    //        if (speed < 10.0f)
    //            rotationChange *= 0.67f + 0.33f * speed / 10.0f;
    //        else
    //            rotationChange *= 1.0f + (speed - 10) / 100.0f;

    //        // Limit rotation change to MaxRotationPerSec * 1.5
    //        if (rotationChange > maxRot)
    //            rotationChange = maxRot;
    //        if (rotationChange < -maxRot)
    //            rotationChange = -maxRot;

    //        direction += new Vector3(0, rotationChange, 0);

    //        float newAccelerationForce = 0.0f;
    //        if (keyState.IsKeyDown(Keys.W) || keyState.IsKeyDown(Keys.Up))
    //            newAccelerationForce +=
    //                maxAccelerationPerSecond;// * moveFactor;
    //        // Down or right mouse button decelerates
    //        else if (keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.Down))
    //            newAccelerationForce -=
    //                maxAccelerationPerSecond;// * moveFactor;

    //        // Limit acceleration (but drive as fast forwards as possible if we
    //        // are moving backwards)
    //        if (speed > 0 &&
    //            newAccelerationForce > MaxAcceleration)
    //            newAccelerationForce = MaxAcceleration;
    //        if (newAccelerationForce < MinAcceleration)
    //            newAccelerationForce = MinAcceleration;

    //        forceOnCar +=
    //        direction * newAccelerationForce * (moveFactor * 85);

    //        float oldSpeed = speed;
    //        Vector3 speedChangeVector = forceOnCar / Mass;
    //        if (speedChangeVector.Length() > 0)
    //        {
    //            float speedApplyFactor =
    //                Vector3.Dot(Vector3.Normalize(speedChangeVector), direction);
    //            if (speedApplyFactor > 1)
    //                speedApplyFactor = 1;
    //            speed += speedChangeVector.Length() * speedApplyFactor;
    //        }
    //        // Apply friction. Basically we have 2 frictions that slow us down:
    //        // The friction from the contact of the wheels with the road (rolling
    //        // friction) and the air friction, which becomes bigger as we drive
    //        // faster. We need more force to overcome the resistances if we drive
    //        // faster. Our engine is strong enough to overcome the initial
    //        // car friction and air friction, but we want simulate that we need
    //        // more force to overcome the resistances at high speeds.

    //        float airFriction = AirFrictionPerSpeed * Math.Abs(speed);
    //        if (airFriction > MaxAirFriction)
    //            airFriction = MaxAirFriction;
    //        // Don't use ground friction if we are not on the ground.
    //        float groundFriction = CarFrictionOnRoad;

    //        forceOnCar *= 1.0f - (//0.033f* //
    //            //0.025f * // 0.01f * //Math.Min(0.1f, Math.Max(0.01f, moveFactor)) *
    //0.275f * 0.02125f *//BaseGame.MoveFactorPerSecond *
    //0.2f * // 20% for force slowdown
    //(groundFriction + airFriction));
    //        // Reduce the speed, but use very low values to make the game more fun!
    //        float noFrictionSpeed = speed;
    //        speed *= 1.0f - (0.01f * // moveFactor *
    //            //0.0033f * // 0.25% for speed slowdown (which is a lot for high speeds)
    //            0.1f * 0.02125f *//BaseGame.MoveFactorPerSecond *
    //            (groundFriction + airFriction));
    //        // Never change more than by 1
    //        if (speed < noFrictionSpeed - 1)
    //            speed = noFrictionSpeed - 1;

    //        bool backPressed = keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.Down);
    //        if (backPressed || keyState.IsKeyDown(Keys.Space))
    //        {
    //            float slowdown =
    //                1.0f - moveFactor *
    //                // Use only half if we just decelerate
    //                (backPressed ? BrakeSlowdown / 2 : BrakeSlowdown) *
    //                // Don't brake so much if we are already driving backwards
    //                (speed < 0 ? 0.33f : 1.0f);
    //            speed *= Math.Max(0, slowdown);
    //            // Limit to max. 100 mph slowdown per sec
    //            if (speed > oldSpeed + 100 * moveFactor)
    //                speed = (oldSpeed + 100 * moveFactor);
    //            if (speed < oldSpeed - 100 * moveFactor)
    //                speed = (oldSpeed - 100 * moveFactor);
    //        }

    //        // Calculate pitch depending on the force
    //            float speedChange = speed - oldSpeed;
    //        // Limit speed change, never apply more than 5 per sec.
    //            if (speedChange < -8 * moveFactor)
    //                speedChange = -8 * moveFactor;
    //            if (speedChange > 8 * moveFactor)
    //                speedChange = 8 * moveFactor;
    //            //carPitchPhysics.ChangePos(speedChange);


    //            // Limit speed
    //            if (speed > maxSpeed)
    //                speed = maxSpeed;
    //            if (speed < -maxSpeed)
    //                speed = -maxSpeed;

    //            // Apply speed and calculate new car position.
    //            this.Position += speed * direction * moveFactor * 1.75f;


            ////float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            ////Vector3 acceleration = Vector3.Zero;
            ////if (keyState.IsKeyDown(Keys.W) || keyState.IsKeyDown(Keys.Up))
            ////    acceleration.Z += EngineTorque / Mass;
            ////else if (keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.Down))
            ////    acceleration.Z -= EngineTorque / Mass;
            ////else
            ////    acceleration.Z -= EngineTorque / Mass;

            ////float steering = 0.0f;
            ////if (keyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.Left)) steering -= 1.0f;
            ////if (keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.Down)) steering += 1.0f;

            ////if (steering > wheelOrientation)
            ////    wheelOrientation += Math.Min(
            ////        steering - wheelOrientation,
            ////        (steering - wheelOrientation) * 5.0f * deltaTime);
            ////else
            ////    wheelOrientation -= Math.Min(
            ////        wheelOrientation - steering,
            ////        (wheelOrientation - steering) * 5.0f * deltaTime);
            ////Console.Out.WriteLine(wheelOrientation);

            ////wheelSteerMatrix = Matrix.CreateRotationY(wheelOrientation);
            ////facingDirection = this.Rotation.Y;
            ////facingDirection += wheelOrientation * TurnSpeed;
            ////this.Rotation = new Vector3(this.Rotation.X, facingDirection, this.Rotation.Z);

            //////Console.Out.WriteLine("A=" + acceleration);
            ////this.velocity += acceleration * deltaTime;
            //////Console.Out.WriteLine("V=" + this.velocity);
            ////this.Position += this.velocity * deltaTime;
            //////Console.Out.WriteLine("P=" + this.Position);


        }

        protected Vector3 limitVector(Vector3 v, float max)
        {
            if (v.LengthSquared() > max * max)
            {
                v.Normalize();
                v *= max;
            }

            return v;
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
