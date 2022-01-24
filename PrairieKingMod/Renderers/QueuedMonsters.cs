using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrairieKingUIEnhancements
{
    class QueuedMonsters : Renderable
    {
        private Dictionary<int, int> counts = new Dictionary<int, int>();
        private int total = 0;
        private int remaining = 0;
        private bool avoidFlicker = false;
        private int frames = 0;
        private static readonly int minTotal = 3;

        public override void Tick(Config config, AbigailGame game)
        {
            if (!config.MonsterQueueCounts && (!config.MonsterQueueTimer || AbigailGame.waveTimer > 0))
            {
                return;
            }

            if (AbigailGame.monsters.Count == 0 && remaining == 0)
            {
                total = 0;
                avoidFlicker = true;
                frames = 0;
                return;
            }           

            counts = new Dictionary<int, int>();
            remaining = 0;
            foreach (List<Point> q in game.spawnQueue)
            {
                foreach (Point s in q)
                {
                    if (counts.ContainsKey(s.X))
                    {
                        counts[s.X] += s.Y;
                    }
                    else
                    {
                        counts[s.X] = s.Y;
                    }
                    remaining += s.Y;
                }
            }
            if (total < remaining)
            {
                total = remaining;
            }

            if (avoidFlicker)
            {
                if (remaining > minTotal)
                {
                    frames++;
                    if (frames > 120)
                    {
                        avoidFlicker = false;
                    }
                }
                else
                {
                    frames = 0;
                }
            }
            else if (remaining == 0)
            {
                avoidFlicker = true;
                frames = 0;
            }
        }

        public override void Render(SpriteBatch b, Config config, AbigailGame game)
        {
            if (config.MonsterQueueTimer && AbigailGame.waveTimer <= 0 && remaining > 0 && total != 0)
            {
                float percent = (float)remaining / total;
                b.Draw(
                    Game1.staminaRect,
                    new Rectangle(
                        (int)AbigailGame.topLeftScreenCoordinate.X + 30,
                        (int)AbigailGame.topLeftScreenCoordinate.Y - AbigailGame.TileSize / 2 + 3,
                        (int)((float)(16 * AbigailGame.TileSize - 30) * percent),
                        AbigailGame.TileSize / 4),
                    percent < 0.2 ? new Color(147, 177, 38) : new Color(188, 51, 74));
            }

            if (config.MonsterQueueCounts && remaining > 0 && !avoidFlicker)
            {
                Vector2 offset = new Vector2(
                    config.FixNumberOverflow ? 24 : 0,
                    config.ShowDeathCounter ? 4 : 3);

                foreach (var kvp in counts.OrderByDescending(x => x.Value))
                {
                    b.Draw(
                        Game1.mouseCursors,
                        AbigailGame.topLeftScreenCoordinate - new Vector2(AbigailGame.TileSize * 2 + offset.X, -AbigailGame.TileSize * offset.Y - 18),
                        new Rectangle(352 + (kvp.Key * 2) * 16, 1712, 16, 16),
                        Color.White,
                        0f,
                        Vector2.Zero,
                        3f,
                        SpriteEffects.None,
                        0.5f);
                    b.DrawString(
                        Game1.smallFont,
                        "x" + ToShortString(kvp.Value),
                        AbigailGame.topLeftScreenCoordinate - new Vector2(AbigailGame.TileSize + offset.X, -AbigailGame.TileSize * offset.Y - AbigailGame.TileSize / 4 - 18),
                        Color.White);
                    offset.Y++;
                }
            }
        }
    }
}
