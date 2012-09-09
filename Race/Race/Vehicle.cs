using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Race
{
    public class Vehicle : GameObject
    {
        protected Matrix wheelRollMatrix = Matrix.Identity;
        protected Matrix wheelSteerMatrix = Matrix.Identity;

        protected ModelBone leftBackWheelBone;
        protected ModelBone rightBackWheelBone;
        protected ModelBone leftFrontWheelBone;
        protected ModelBone rightFrontWheelBone;

        protected Matrix leftBackWheelTransform;
        protected Matrix rightBackWheelTransform;
        protected Matrix leftFrontWheelTransform;
        protected Matrix rightFrontWheelTransform;

        protected Track track;

        protected const float Mass = 1000.0f;
        protected const float WheelRadius = 18;

        string name;

        public int LapsLeft
        {
            set { lapsLeft = value; }
        }
        protected int lapsLeft;
        protected TimeSpan timeElapsed;

        protected float speed;
        protected float distance;

        GraphicsDevice graphicsDevice;

        public Vehicle(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice, Track track, string name, int laps)
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

            this.name = name;
            this.lapsLeft = laps;
            this.graphicsDevice = graphicsDevice;
        }

        public bool FinishedRace()
        {
            return lapsLeft == 0;
        }

        public TimeSpan GetResult()
        {
            return timeElapsed;
        }

        public string GetName()
        {
            return name;
        }

        public override void Update(GameTime gameTime)
        {
        }

        private void DrawModel(Matrix View, Matrix Projection, Matrix worldMatrix)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = View;
                    effect.Projection = Projection;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    //effect.Alpha = 1.0f;
                }
                mesh.Draw();
            }
        }

        private void DrawShadow(Matrix View, Matrix Projection, Matrix worldMatrix)
        {
            //BlendState blendState = new BlendState()
            //{
            //    AlphaSourceBlend = Blend.SourceAlpha,
            //    AlphaDestinationBlend = Blend.InverseSourceAlpha,
            //    ColorDestinationBlend = Blend.InverseSourceAlpha, // Required for Reach profile
            //};
            //graphicsDevice.BlendState = blendState;
            //graphicsDevice.BlendState = BlendState.AlphaBlend;
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = modelTransforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = View;
                    effect.Projection = Projection;

                    effect.EnableDefaultLighting();
                    //effect.DiffuseColor = Color.White.ToVector3();
                    //effect.LightingEnabled = true;
                    //effect.AmbientLightColor = new Vector3(1, 1, 1);

                    effect.PreferPerPixelLighting = true;
                    //effect.Alpha = 0.01f;
                }
                mesh.Draw();
            }
            //graphicsDevice.BlendState = BlendState.Opaque;
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

            DrawModel(View,Projection,worldMatrix);

            Matrix shadowMatrix = Matrix.CreateShadow(Vector3.Normalize(new Vector3(1, 1, 1)), new Plane(0, 1, 0, -1));
            worldMatrix *= shadowMatrix;

            DrawShadow(View, Projection, worldMatrix);
        }
    }
}
