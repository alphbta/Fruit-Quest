using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Fruit_Quest;

public class GameplayState : IGameState
{
    private KeyboardState keyboardState;
    private Game1 game;
    private LevelManager levelManager;
    private int currentLevelIndex = 0;
    private SpriteFont font;
    
    public GameplayState(Game1 game)
    {
        this.game = game;
    }

    public void LoadContent()
    {
        levelManager = new LevelManager(game, game.Content, game.GraphicsDevice);
        levelManager.LoadLevel(0);
        font = game.Content.Load<SpriteFont>("Fonts/pixel_font");
    }

    public void Update(GameTime gameTime)
    {
        keyboardState = Keyboard.GetState();
        if (Keyboard.GetState().IsKeyDown(Keys.R) || levelManager.CurrentLevel.IsPlayerDead)
        {
            levelManager.RestartCurrentLevel();
            return;
        }

        if (levelManager.CurrentLevel.IsComplete)
        {
            currentLevelIndex++;
            if (currentLevelIndex < levelManager.LevelCount)
                levelManager.LoadLevel(currentLevelIndex);
        }
        
        levelManager.CurrentLevel.Update(gameTime, keyboardState);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: levelManager.CurrentLevel.camera.Transform);

        levelManager.CurrentLevel.Draw(spriteBatch);

        spriteBatch.End();

        spriteBatch.Begin();

        string fruitCount = $"{levelManager.CurrentLevel.totalCollected} / {levelManager.CurrentLevel.totalFruits}";
        spriteBatch.DrawString(font, fruitCount, new Vector2(20, 10), Color.White);
        if (levelManager.CurrentLevel.canPlayerDashOnScreen)
            spriteBatch.DrawString(font, "Shift", new Vector2(20, 58), Color.White);
        if (levelManager.CurrentLevel.canPlayerDoubleJumpOnScreen)
            spriteBatch.DrawString(font, "Double jump", new Vector2(20, 58), Color.White);

        spriteBatch.End();
        
    }

    public void RestartLevel()
    {
        levelManager.RestartCurrentLevel();
    }
}