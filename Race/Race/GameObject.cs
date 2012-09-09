using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Race
{
    public class GameObject
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public Model Model { get; private set; }
        protected Matrix[] modelTransforms;

        private GraphicsDevice graphicsDevice;

        private BoundingSphere boundingSphere;

        public BoundingSphere BoundingSphere
        {
            get
            {
                Matrix worldTransform = Matrix.CreateScale(Scale)
                    * Matrix.CreateTranslation(Position);

                BoundingSphere newSphere = boundingSphere;
                newSphere = newSphere.Transform(worldTransform);

                return newSphere;
            }
        }

        public GameObject(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice)
        {
            this.Model = Model;

            modelTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;

            this.graphicsDevice = graphicsDevice;

            createBoundingSphere();
        }

        private void createBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere();

            foreach (ModelMesh mesh in Model.Meshes)
            {
                BoundingSphere transformed = mesh.BoundingSphere.Transform(
                    modelTransforms[mesh.ParentBone.Index]);
                sphere = BoundingSphere.CreateMerged(sphere, transformed);
            }

            this.boundingSphere = sphere;
        }

        public virtual void Update(GameTime gameTime){ }

        public virtual void Draw(Matrix View, Matrix Projection, Vector3 Camera)
        {
            Matrix baseWorld = Matrix.CreateScale(Scale)
                * Matrix.CreateFromYawPitchRoll(
                    Rotation.Y, Rotation.X, Rotation.Z)
                * Matrix.CreateTranslation(Position);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index]
                    * baseWorld;
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    BasicEffect effect = (BasicEffect)meshPart.Effect;

                    effect.World = localWorld;
                    effect.View = View;
                    effect.Projection = Projection;

                    effect.EnableDefaultLighting();

                    mesh.Draw();
                }
            }
        }
    }

}
