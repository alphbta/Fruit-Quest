using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fruit_Quest
{
    internal class Sprite
    {
        private readonly int SCALE = Game1.SCALE;

        public Texture2D texture;
        public Vector2 position;
        public Rectangle Rect
        {
            get
            {
                return new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    texture.Width * SCALE,
                    texture.Height * SCALE
                    );
            }
        }


        public Sprite(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.position = position;
        }

        public virtual void Update(GameTime gameTime) { }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Rect, Color.White);
        }
    }
}
