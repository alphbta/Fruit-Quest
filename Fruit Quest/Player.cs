using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Fruit_Quest
{
    internal class Player : Sprite
    {
        private static readonly int SCALE = 4;
        
        //player1.png
        public static Vector2 hitboxPosition = new Vector2(9, 10);
        public static int hitboxWidth = 16;
        public static int hitboxHeight = 22;

        public Rectangle playerRect
        {
            get
            {
                return new Rectangle(
                    (int)position.X + (int)hitboxPosition.X * SCALE,
                    (int)position.Y + (int)hitboxPosition.Y * SCALE,
                    hitboxWidth * SCALE,
                    hitboxHeight * SCALE
                    );
            }
        }


        public Player(Texture2D texture, Vector2 position) : base(texture, position) { }

        public Vector2 UpdatePos(GameTime gameTime)
        {
            base.Update(gameTime);

            Vector2 direction = Vector2.Zero;

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                direction.Y -= 1;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                direction.X -= 1;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                direction.Y += 1;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                direction.X += 1;
            }

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            float speed = 5f;
            return position + direction * speed;
        }
    }
}
