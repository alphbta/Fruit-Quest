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
        
        private bool spacePressed = false;
        private bool shiftPressed = false;
        public Vector2 velocity;
        public bool Grounded {  get; set; }
        private int Direction { get; set; } // -1 = left, 1 = right
        private PlayerAbility ability;
        private bool canDoubleJump;
        private bool hasDoubleJumped;
        public bool IsDashing {  get; set; }
        public float DashStep => 3f;
        public float dashDistanceRemaining = 0f;
        public int DashDirection { get; private set; }
        private float dashCooldown = 1f; // Cooldown for dash ability
        private float dashTimer = 0f; // Timer to track dash cooldown
        public bool canDashOnScreen => dashTimer <= 0f && ability == PlayerAbility.Dash; // Check if dash is available
        public bool canDoubleJumpOnScreen => ability == PlayerAbility.DoubleJump && !hasDoubleJumped;

        private static readonly Vector2 hitboxPosition = new Vector2(9, 10);
        private static readonly int hitboxWidth = 16;
        private static readonly int hitboxHeight = 22;
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
        
        private Dictionary<string, Animation> animations;
        private string currentAnimation;

        public Player(Texture2D texture, Rectangle rect, Rectangle sourceRect, Dictionary<string, Texture2D> animationTextures)
            : base(texture, rect, sourceRect) 
        {
            velocity = new();
            Grounded = false;
            Direction = 1;
            animations = new Dictionary<string, Animation>
            {
                ["Idle"] = new Animation(animationTextures["Idle"], 11, 0.1f),
                ["Run"] = new Animation(animationTextures["Run"], 12, 0.06f),
                ["Jump"] = new Animation(animationTextures["Jump"], 1, 0.1f),
                ["Fall"] = new Animation(animationTextures["Fall"], 1, 0.1f),
                ["Dash"] = new Animation(animationTextures["Dash"], 1, 0.1f),
            };
            currentAnimation = "Idle";
        }

        public void Update(GameTime gameTime, KeyboardState keyState)
        {
            if (!IsDashing)
            {
                velocity.X = 0;
                velocity.Y += 0.5f;
                velocity.Y = Math.Min(10f, velocity.Y);

                if (keyState.IsKeyDown(Keys.A))
                {
                    velocity.X -= 5;
                    Direction = -1;
                }
                if (keyState.IsKeyDown(Keys.D))
                {
                    velocity.X += 5;
                    Direction = 1;
                }

                if (!spacePressed && keyState.IsKeyDown(Keys.Space))
                {
                    if (Grounded)
                    {
                        velocity.Y = -12;
                        spacePressed = true;
                        hasDoubleJumped = false;
                    }
                    else if (canDoubleJump && !hasDoubleJumped)
                    {
                        velocity.Y = -12;
                        spacePressed = true;
                        hasDoubleJumped = true;
                    }
                }

                if (keyState.IsKeyUp(Keys.Space))
                    spacePressed = false;

                if (Grounded)
                    hasDoubleJumped = false;

                if (ability == PlayerAbility.Dash && !shiftPressed && keyState.IsKeyDown(Keys.LeftShift) && dashTimer <= 0f)
                {
                    shiftPressed = true;
                    StartDash();
                }

                if (keyState.IsKeyUp(Keys.LeftShift))
                    shiftPressed = false;

                if (dashTimer > 0f)
                {
                    dashTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }

            if (IsDashing)
                SetAnimation("Dash");
            else if (!Grounded)
                SetAnimation(velocity.Y > 0 ? "Fall" : "Jump");
            else if (velocity.X != 0)
                SetAnimation("Run");
            else
                SetAnimation("Idle");
            
            animations[currentAnimation].Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var effect = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            spriteBatch.Draw(
                animations[currentAnimation].Texture,
                rect,
                animations[currentAnimation].CurrentFrameRect,
                Color.White,
                0f,
                Vector2.Zero,
                effect,
                0f);
        }

        public void SetAbility(PlayerAbility ability)
        {
            this.ability = ability;
            canDoubleJump = ability == PlayerAbility.DoubleJump;
        }

        private void StartDash()
        {
            IsDashing = true;
            dashDistanceRemaining = 110f;
            DashDirection = Direction;
            dashTimer = dashCooldown; // Reset the dash cooldown timer
        }
        
        private void SetAnimation(string name)
        {
            if (currentAnimation != name)
            {
                currentAnimation = name;
                animations[name].Reset();
            }
        }
    }

    public enum PlayerAbility
    {
        Normal,
        DoubleJump,
        Dash
    }
}
