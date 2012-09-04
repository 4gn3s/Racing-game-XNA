using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Race
{
    class FPSCounter : DrawableGameComponent
    {
        ContentManager contentManager;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        int frameRate = 0;
        int totalFrames = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        Vector2 position;

        public FPSCounter(Game game)
            : base(game)
        {
            contentManager = game.Content;

            position = new Vector2(10.0f, 10.0f);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = contentManager.Load<SpriteFont>("font");

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            contentManager.Unload();
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime >= TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = totalFrames;
                totalFrames = 0;
            }
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            totalFrames++;

            string str = string.Format("FPS: {0}", frameRate);

            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, str, position+Vector2.One, Color.Black);
            spriteBatch.DrawString(spriteFont, str, position, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);


        }
    }
}
