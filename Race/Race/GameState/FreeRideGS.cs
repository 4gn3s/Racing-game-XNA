using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Race
{
    class FreeRideGS : GameState
    {
        public FreeRideGS(Game1 game) : base(game) { }

        public override void Update(GameTime gameTime)
        {
            game.UpdateGameInProgress(gameTime);
        }

        public override void Draw(GameTime gameTime) { }
    }
}
