using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fruit_Quest;

public class Animation
{
    public Texture2D Texture { get; }
    public int FrameCount { get; }
    public float FrameTime { get; }
    private float timer;
    private int currentFrame;
    private int frameWidth;
    
    public Rectangle CurrentFrameRect => new Rectangle(currentFrame * frameWidth, 0, frameWidth, Texture.Height);

    public Animation(Texture2D texture, int frameCount, float frameTime)
    {
        Texture = texture;
        FrameCount = frameCount;
        FrameTime = frameTime;
        frameWidth = Texture.Width / frameCount;
    }

    public void Update(GameTime gameTime)
    {
        timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (timer >= FrameTime)
        {
            currentFrame++;
            currentFrame %= FrameCount;
            timer -= FrameTime;
        }
    }

    public void Reset()
    {
        currentFrame = 0;
        timer = 0f;
    }
}