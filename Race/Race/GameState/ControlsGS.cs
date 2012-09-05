using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Race
{
    class ControlsGS : GameState
    {
        MouseState currentMouse, previousMouse;
        private bool MouseLeftClick
        {
            get
            {
                return previousMouse.LeftButton == ButtonState.Pressed &&
                   currentMouse.LeftButton == ButtonState.Released;
            }
        }
        SpriteFont font;

        public ControlsGS(Game1 game) : base(game)
        {
            font = game.Content.Load<SpriteFont>("countdownFont");
            currentMouse = previousMouse = Mouse.GetState();
        }

        public override void Update(GameTime gameTime)
        {
            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();
            if (MouseLeftClick)
                game.GameState = new MenuGS(game);
        }

        public override void Draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.CornflowerBlue);
            game.spriteBatch.Begin();
            //game.spriteBatch.Draw(background, Vector2.Zero, Color.White);
            string text="CONTROLS\n\n WSAD,ARROW KEYS- move\n P-pause\n ESC-exit";
            Vector2 textSize = font.MeasureString(text);
            Vector2 margins = new Vector2(game.GraphicsDevice.Viewport.Width - textSize.X, game.GraphicsDevice.Viewport.Height - textSize.Y);
            Vector2 textPosition = new Vector2(margins.X/2, margins.Y/2);

            game.spriteBatch.DrawString(font, text, textPosition, Color.White);

            game.spriteBatch.End();
        }
    }
}
