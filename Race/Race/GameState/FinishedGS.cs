using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Race
{
    class FinishedGS : GameState
    {
        List<Vehicle> activePlayers = new List<Vehicle>();
        SortedList<TimeSpan, string> results = new SortedList<TimeSpan, string>();

        SpriteFont font;
        Vector2 textPosition = new Vector2(0, 0);

        public FinishedGS(Game1 game, List<Vehicle> vehicles)
            : base(game)
        {
            font = game.Content.Load<SpriteFont>("font");
            activePlayers = vehicles;
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            int i = 0;

            if (activePlayers.Count == 0)
                return;
            List<Vehicle> still_active = new List<Vehicle>();
            foreach (Vehicle vehicle in activePlayers)
                if (!vehicle.FinishedRace())
                    still_active.Add(vehicle);
                else
                    try
                    {
                        if (vehicle is Car)
                            vehicle.Rotation = new Vector3(vehicle.Rotation.X, vehicle.Rotation.Y + MathHelper.Pi, vehicle.Rotation.Z);
                        results.Add(vehicle.GetResult(), vehicle.GetName());
                    }
                    catch (ArgumentException)
                    {
                        results.Add(vehicle.GetResult() + TimeSpan.FromMilliseconds(i*50), vehicle.GetName());
                        i++;
                    }

            activePlayers = still_active;
            if (activePlayers.Count != 0)
            {
                game.UpdateGame(gameTime,activePlayers);
            }
            
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            textPosition = new Vector2(50,70);
            game.spriteBatch.Begin();
            game.spriteBatch.DrawString(font, "RESULTS", textPosition, Color.Green);
            textPosition += new Vector2(0, 40);
            for (int i = 0; i < results.Count; i++ )
            {
                string text = String.Format("{0}. ({1}mm) - {2} ", i,results.ElementAt(i).Key, results.ElementAt(i).Value );
                Vector2 textsize= font.MeasureString(text);
                var rect = new Texture2D(game.GraphicsDevice, 1, 1);
                rect.SetData(new[] { Color.Green });
                game.spriteBatch.Draw(rect, new Rectangle((int)textPosition.X, (int)textPosition.Y, (int)textsize.X, (int)textsize.Y), Color.Green);
                game.spriteBatch.DrawString(font, text, textPosition, Color.Black);
                textPosition += new Vector2(0, textsize.Y +  20);
            }
            game.spriteBatch.End();
            game.GraphicsDevice.BlendState = BlendState.Opaque;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
    }
}
