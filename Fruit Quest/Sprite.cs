using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fruit_Quest
{
    internal class Sprite
    {
        private static readonly float SCALE = 4f;

        public Texture2D texture;
        public Vector2 position;
        public Rectangle Rect
        {
            get
            {
                return new Rectangle(
                    (int)position.X,
                    (int)position.Y,
                    texture.Width * (int)SCALE,
                    texture.Height * (int)SCALE
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
