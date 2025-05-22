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
        public Vector2 velocity;
        public bool Grounded {  get; set; }
        public int Direction { get; set; } // 1 = left, -1 = right

        //player1.png желтые рамки игрока - хитбокс
        private readonly static Vector2 hitboxPosition = new Vector2(9, 10);
        private readonly static int hitboxWidth = 16;
        private readonly static int hitboxHeight = 22;

        public Rectangle PlayerRect
        {
            get
            {
                return new Rectangle(
                    rect.X + (int)hitboxPosition.X * SCALE,
                    rect.Y + (int)hitboxPosition.Y * SCALE,
                    hitboxWidth * SCALE,
                    hitboxHeight * SCALE
                    );
            }
        }

        public Player(Texture2D texture, Rectangle rect, Rectangle sourceRect)
            : base(texture, rect, sourceRect) 
        {
            changeY = new();
            velocity = new();
            Grounded = false;
            Direction = -1;
        }

        public void Update(KeyboardState keyState)
        {
            velocity.X = 0;
            velocity.Y += 0.5f;
            velocity.Y = Math.Min(10f, velocity.Y);
            int prevDirection = Direction;

            if (keyState.IsKeyDown(Keys.A))
            {
                velocity.X -= 5;
                Direction = 1;
            }
            if (keyState.IsKeyDown(Keys.D))
            {
                velocity.X += 5;
                Direction = -1;
            }

            if (Grounded && !spacePressed && keyState.IsKeyDown(Keys.Space))
            {
                spacePressed = true;
                velocity.Y = -12;
            }

            if (keyState.IsKeyUp(Keys.Space))
                spacePressed = false;

            if (Direction != prevDirection)
            {
                sourceRect.X += sourceRect.Width;
                sourceRect.Width = -sourceRect.Width;
            }
        }
    }
}
