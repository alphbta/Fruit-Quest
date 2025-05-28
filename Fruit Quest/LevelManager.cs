using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Fruit_Quest;

public class LevelManager
{
    private Game1 game;
    private ContentManager content;
    private GraphicsDevice graphics;

    public int CurrentLevelIndex { get; private set; } = 0;
    private List<string> levels = new List<string> {"level1.tmj", "level2.tmj", "level3.tmj"};
    private List<PlayerAbility> abilities = new List<PlayerAbility> { PlayerAbility.Normal, PlayerAbility.DoubleJump, PlayerAbility.Dash };
    public int LevelCount => levels.Count;
    
    public Level CurrentLevel { get; private set; }

    public LevelManager(Game1 game, ContentManager content, GraphicsDevice graphics)
    {
        this.game = game;
        this.content = content;
        this.graphics = graphics;
    }

    public void LoadLevel(int index)
    {
        CurrentLevelIndex = index;
        var ability = abilities[index];
        var levelFile = "../../../Data/" + levels[index];
        CurrentLevel = new Level(game, content, graphics, levelFile, ability);
    }

    public void RestartCurrentLevel()
    {
        LoadLevel(CurrentLevelIndex);
    }
}