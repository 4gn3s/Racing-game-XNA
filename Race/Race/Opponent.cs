using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Race
{
    internal class Opponent : GameObject
    {
        private Track track;

        private float XTranslation;

        protected float distance=0.0f, speed=0.0f;
        protected float initialRotation=0.0f;
        protected Vector2 trackPosition=Vector2.Zero;
        protected Vector2 direction;

        const float Mass = 1000.0f;

        public Opponent(Model Model, Vector3 Position, Vector3 Rotation,
            Vector3 Scale, GraphicsDevice graphicsDevice, Track track, float XTranslation)
            : base(Model, Position, Rotation, Scale, graphicsDevice)
        {
            this.track = track;
            this.XTranslation = XTranslation;            
        }

        public override void Update(GameTime gameTime)
        {
            trackPosition = track.TracePath(distance, out direction);

            if (speed < 0)
                speed = 0;
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            speed = 0.85f * dt * Mass;
            distance += speed ; 
            //totalTrackLength += speed;

            float rot = initialRotation + (float)Math.Acos(direction.Y > 0 ? -direction.X : direction.X);

            if (direction.Y > 0)
                rot += MathHelper.Pi;
            rot += MathHelper.PiOver2;

            this.Position = new Vector3(trackPosition.X + XTranslation, 0, trackPosition.Y + XTranslation);
            this.Rotation = new Vector3(0, rot, 0);

            base.Update(gameTime);
        }
    }
}