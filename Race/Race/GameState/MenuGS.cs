using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Race
{
    class MenuGS : GameState
    {
        private Texture2D startBtnTex, freeBtnTex, controlBtnTex, exitBtnTex;
        private Texture2D startBtnTexS, freeBtnTexS, controlBtnTexS, exitBtnTexS;
        private Texture2D background;

        List<MenuButton> buttons = new List<MenuButton>();
        private const int btnsCount = 3;

        MouseState currentMouse, previousMouse;
        private bool MouseMoved
        {
            get
            {
                return currentMouse.X != previousMouse.X || currentMouse.Y != previousMouse.Y;
            }
        }
        private bool MouseLeftClick
        {
            get
            {
                return previousMouse.LeftButton == ButtonState.Pressed &&
                   currentMouse.LeftButton == ButtonState.Released;
            }
        }

        private Vector2 btnSize;

        public MenuGS(Game1 game)
            : base(game)
        {
            btnSize = new Vector2(200, 50);
            Vector2 middle = new Vector2(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);

            loadTextures();
            buttons.Add(new MenuButton(startBtnTex, startBtnTexS, "NG",new Vector2(middle.X-btnSize.X/2, middle.Y - 2*btnSize.Y)));
            buttons.Add(new MenuButton(freeBtnTex, freeBtnTexS, "FR", new Vector2(middle.X - btnSize.X / 2, middle.Y - btnSize.Y)));
            buttons.Add(new MenuButton(controlBtnTex, controlBtnTexS, "C", new Vector2(middle.X - btnSize.X / 2, middle.Y)));
            buttons.Add(new MenuButton(exitBtnTex, exitBtnTexS, "E", new Vector2(middle.X - btnSize.X / 2, middle.Y + btnSize.Y)));

            game.IsMouseVisible = true;
            previousMouse = currentMouse = Mouse.GetState();
        }

        private void loadTextures()
        {
            startBtnTex = game.Content.Load<Texture2D>("buttons/new_race");
            startBtnTexS = game.Content.Load<Texture2D>("buttons/new_race_selected");
            freeBtnTex = game.Content.Load<Texture2D>("buttons/free_drive");
            freeBtnTexS = game.Content.Load<Texture2D>("buttons/free_drive_selected");
            controlBtnTex = game.Content.Load<Texture2D>("buttons/controls");
            controlBtnTexS = game.Content.Load<Texture2D>("buttons/controls_selected");
            exitBtnTex = game.Content.Load<Texture2D>("buttons/exit");
            exitBtnTexS = game.Content.Load<Texture2D>("buttons/exit_selected");
        }

        public override void Update(GameTime gameTime)
        {
            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();
            if (MouseMoved)
            {
                foreach (MenuButton btn in buttons)
                    btn.Selected = btn.Bounds.Contains(currentMouse.X, currentMouse.Y);
            }
            if (MouseLeftClick)
            {
                MenuButton clicked = null;
                foreach (MenuButton btn in buttons)
                    if (btn.Bounds.Contains(currentMouse.X, currentMouse.Y))
                    {
                        clicked = btn;
                        break;
                    }
                if (clicked != null)
                {
                    switch (clicked.name)
                    {
                        case "NG":
                            game.IsMouseVisible = false;
                            game.MyCar.AutoDriving = true;
                            game.GameState = new BeginGameGS(game);
                            break;
                        case "FR":
                            game.IsMouseVisible = false;
                            game.MyCar.AutoDriving = false;
                            game.GameState = new FreeRideGS(game);
                            break;
                        case "C":
                            game.GameState = new ControlsGS(game);
                            break;
                        case "E":
                            game.Exit();
                            break;
                    }
                }
            }

        }

        public override void Draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.CornflowerBlue);
            game.spriteBatch.Begin();
            //game.spriteBatch.Draw(background, Vector2.Zero, Color.White);
            foreach (MenuButton btn in buttons)
                if (btn.Selected)
                    game.spriteBatch.Draw(btn.selectedTexture, btn.position, Color.White);
                else
                    game.spriteBatch.Draw(btn.texture, btn.position, Color.White);

            game.spriteBatch.End();
        }
    }
}
