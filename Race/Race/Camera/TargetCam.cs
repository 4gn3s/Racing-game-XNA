using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Race
{
    class TargetCam : Camera
    {
        public Vector3 Target { get; set; }

        private GameObject myTarget;

        public TargetCam(Vector3 position, Vector3 target, GraphicsDevice device, GameObject mytarget)
            : base(device)
        {
            this.Position = position;
            this.Target = target;
            this.myTarget = mytarget;
        }

        public override void Update()
        {
            this.View = Matrix.CreateLookAt(Position, Target, Vector3.Up);
        }

        public override void Update(GameTime gameTime)
        {
            this.Target = myTarget.Position;
            Update();
        }

    }
}
