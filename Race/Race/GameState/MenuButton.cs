using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Race
{
    class MenuButton
    {
        public Texture2D texture;
        public Texture2D selectedTexture;
        public Vector2 position;
        public string name;

        private bool selected;

        public bool Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            }
        }

        public MenuButton(Texture2D tex, Texture2D seltex, string na, Vector2 pos)
        {
            this.texture = tex;
            this.selectedTexture = seltex;
            this.position = pos;
            this.name = na;
        }
    }
}
