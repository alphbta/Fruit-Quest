using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Fruit_Quest
{
    internal class Player : Sprite
    {
        private readonly int SCALE = Game1.SCALE;

        private float changeY;
        private bool spacePressed = false;

        //player1.png желтые рамки игрока - хитбокс
        private readonly static Vector2 hitboxPosition = new Vector2(9, 10);
        private readonly static int hitboxWidth = 16;
        private readonly static int hitboxHeight = 22;

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


        public Player(Texture2D texture, Vector2 position) : base(texture, position) { changeY = new(); }

        public void Update(KeyboardState keyState, List<Rectangle> collisions)
        {
            float changeX = 0;

            if (keyState.IsKeyDown(Keys.A))
            {
                changeX -= 5;
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                changeX += 5;
            }

            position.X += changeX;

            foreach (var collision in collisions)
            {
                if (collision.Intersects(playerRect))
                {
                    position.X -= changeX;
                }
            }

            changeY += 0.5f;
            changeY = Math.Min(20f, changeY);

            if (!spacePressed && keyState.IsKeyDown(Keys.Space))
            {
                spacePressed = true;
                changeY = -12;
            }

            if (keyState.IsKeyUp(Keys.Space))
                spacePressed = false;

            position.Y += changeY;
            foreach (var collision in collisions)
            {
                if (collision.Intersects(playerRect))
                {
                    position.Y -= changeY;
                    changeY = 0;
                }
            }
        }
    }
}
