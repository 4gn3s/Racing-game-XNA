using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Race
{
    class ChaseCam : Camera
    {
        public Vector3 Target { get; private set; }

        public Vector3 FollowTargetPosition { get; private set; }
        public Vector3 FollowTargetRotation { get; private set; }

        public Vector3 PositionOffset { get; set; }
        public Vector3 TargetOffset { get; set; }

        public Vector3 RelativeCameraRotation { get; set; }

        float springiness=0.1f;

        public float Springiness
        {
            get { return springiness; }
            set { springiness = MathHelper.Clamp(value, 0, 1); }
        }

        private GameObject myTarget;
        public GameObject MyTarget
        {
            set { myTarget = value; }
        }

        public ChaseCam(Vector3 PositionOffset, Vector3 TargetOffset,
            Vector3 RelativeCameraRotation, GraphicsDevice graphicsDevice, GameObject target)
            : base(graphicsDevice)
        {
            this.PositionOffset = PositionOffset;
            this.TargetOffset = TargetOffset;
            this.RelativeCameraRotation = RelativeCameraRotation;

            this.myTarget = target;
        }

        public void Move(Vector3 NewFollowTargetPosition,
            Vector3 NewFollowTargetRotation)
        {
            this.FollowTargetPosition = NewFollowTargetPosition;
            this.FollowTargetRotation = NewFollowTargetRotation;
        }

        public void Rotate(Vector3 RotationChange)
        {
            this.RelativeCameraRotation += RotationChange;
        }

        public override void Update()
        {
            Vector3 combinedRotation = FollowTargetRotation +
                RelativeCameraRotation;
            Matrix rotation = Matrix.CreateFromYawPitchRoll(
                combinedRotation.Y, combinedRotation.X, combinedRotation.Z);
            Vector3 desiredPosition = FollowTargetPosition +
                Vector3.Transform(PositionOffset, rotation);//without the spring value
            Position = Vector3.Lerp(Position, desiredPosition, Springiness);
            Target = FollowTargetPosition +
                Vector3.Transform(TargetOffset, rotation);
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);
            View = Matrix.CreateLookAt(Position, Target, up);
            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            this.Up = up;
            this.Right = Vector3.Cross(forward, up);
        }

        public override void Update(GameTime gameTime)
        {
            Move(myTarget.Position, myTarget.Rotation);
            
            Update();
        }
    }
}
