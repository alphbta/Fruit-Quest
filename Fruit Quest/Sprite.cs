using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fruit_Quest
{
    internal class Sprite
    {
        public Texture2D texture;
        public Rectangle rect;
        public Rectangle sourceRect;

        public Sprite(Texture2D texture, Rectangle rect, Rectangle sourceRect)
        {
            this.texture = texture;
            this.rect = rect;
            this.sourceRect = sourceRect;
        }

        public virtual void Update(GameTime gameTime) { }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rect, sourceRect, Color.White);
        }
    }
}
