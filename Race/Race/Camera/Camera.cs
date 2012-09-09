using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Race
{
    public abstract class Camera
    {
        public Matrix View
        {
            get { return view_; }
            protected set { view_ = value; generateFrustum(); }
        }

        public Matrix Projection
        {
            get { return projection_; }
            protected set { projection_ = value; generateFrustum(); }
        }

        public BoundingFrustum Frustum { get; private set; }

        public Vector3 Position { get; protected set; }
        public Vector3 Up { get; protected set; }
        public Vector3 Right { get; protected set; }

        protected GraphicsDevice graphicsDevice_;

        Matrix view_, projection_;

        public Camera(GraphicsDevice device)
        {
            this.graphicsDevice_ = device;

            generateProjectionMatrix(MathHelper.PiOver4);
        }

        public virtual void Update() {}

        public virtual void Update(GameTime gameTime) {}

        public bool IsInView(BoundingSphere sphere)
        {
            if (Frustum.Contains(sphere) != ContainmentType.Disjoint)
                return true;
            return false;
        }

        public bool IsInView(BoundingBox box)
        {
            if (Frustum.Contains(box) != ContainmentType.Disjoint)
                return true;
            return false;
        }

        public void ChangePosition(Vector3 newposition)
        {
            this.Position = newposition;
        }

        private void generateFrustum()
        {
            Frustum = new BoundingFrustum(View * Projection);
        }

        private void generateProjectionMatrix(float areaViewed)
        {
            PresentationParameters pres = graphicsDevice_.PresentationParameters;

            float aspectRatio = (float)pres.BackBufferWidth /
    (float)pres.BackBufferHeight;

            this.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), aspectRatio, 0.1f, 1000000.0f);
        }


    }
}
