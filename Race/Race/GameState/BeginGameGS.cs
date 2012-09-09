using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Race
{
    class BeginGameGS : GameState
    {
        private bool animationFinished = false;
        private TimeSpan totalTimeElapsed = new TimeSpan();

        SpriteFont font;
        string text = "";
        float textScale = 1.0f;
        Vector2 textPosition = Vector2.Zero;

        public BeginGameGS(Game1 game) : base(game){
            font = game.Content.Load<SpriteFont>("countdownFont");
            ChangeTextPosition();
        }

        public override void Update(GameTime gameTime)
        {
            if (!animationFinished)
            {
                totalTimeElapsed += gameTime.ElapsedGameTime;

                textScale = 50.0f - (totalTimeElapsed.Milliseconds % 1000) / 20;
                ChangeTextPosition();

                switch (totalTimeElapsed.Seconds)
                {
                    case 0:
                        text = "3";
                        break;
                    case 1:
                        text = "2";
                        break;
                    case 2:
                        text = "1";
                        break;
                    default:
                        animationFinished = true;
                        break;
                }

            }
            else
                game.UpdateGameInProgress(gameTime);
        }

        private void ChangeTextPosition()
        {
            Vector2 textSize = font.MeasureString(text) * textScale;
            textPosition =new Vector2((game.GraphicsDevice.Viewport.Width - textSize.X)/2 ,
                (game.GraphicsDevice.Viewport.Height - textSize.Y)/2 );
        }

        public override void Draw(GameTime gameTime)
        {
            if (!animationFinished)
            {
                game.spriteBatch.Begin();
                game.spriteBatch.DrawString(font, text, textPosition, Color.White, 0.0f, Vector2.Zero, textScale, SpriteEffects.None, 0.0f);
                game.spriteBatch.End();
                game.GraphicsDevice.BlendState = BlendState.Opaque;
                game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
        }
    }
}
