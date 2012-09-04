using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Race
{
    class Sky : GameObject
    {
        Effect effect;
        GraphicsDevice graphics;

        public Sky(ContentManager Content,
            GraphicsDevice GraphicsDevice, TextureCube Texture)
            : base(Content.Load<Model>("skysphere_mesh"),
                Vector3.Zero, Vector3.Zero, new Vector3(100000),
                GraphicsDevice)
        {
            effect = Content.Load<Effect>("skysphere_effect");
            effect.Parameters["CubeMap"].SetValue(Texture);

            this.SetModelEffect(effect);

            this.graphics = GraphicsDevice;
        }

        public new void Draw(Matrix View, Matrix Projection,
            Vector3 CameraPosition)
        {
            graphics.DepthStencilState = DepthStencilState.None;
            this.Position = CameraPosition;

            Matrix baseWorld = Matrix.CreateScale(Scale)
                * Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z)
                * Matrix.CreateTranslation(Position);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index]
                    * baseWorld;

                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    Effect effect = meshPart.Effect;
                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(View);
                    effect.Parameters["Projection"].SetValue(Projection);
                    effect.Parameters["CameraPosition"].SetValue(CameraPosition);
                }

                mesh.Draw();
            }

            graphics.DepthStencilState = DepthStencilState.Default;
        }

        public void SetClipPlane(Vector4? Plane)
        {
            effect.Parameters["ClipPlaneEnabled"].SetValue(Plane.HasValue);

            if (Plane.HasValue)
                effect.Parameters["ClipPlane"].SetValue(Plane.Value);
        }

        public void SetModelEffect(Effect effect)
        {
            foreach (ModelMesh mesh in Model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    part.Effect = effect;
        }

    }

    public class MeshTag
    {
        public Vector3 Color;
        public Texture2D Texture;
        public float SpecularPower;
        public Effect CachedEffect = null;

        public MeshTag(Vector3 Color, Texture2D Texture, float SpecularPower)
        {
            this.Color = Color;
            this.Texture = Texture;
            this.SpecularPower = SpecularPower;
        }
    }
}
