using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Fruit_Quest;

public class Level
{
    private Game1 game;
    private ContentManager content;
    private GraphicsDevice graphics;

    private TiledMap Map { get; set; }
    private Dictionary<string, Dictionary<Vector2, int>> layers;
    private Player player;
    private Vector2 playerPosition;
    private PlayerAbility ability;
    public bool canPlayerDashOnScreen => player.canDashOnScreen;
    public bool canPlayerDoubleJumpOnScreen => player.canDoubleJumpOnScreen;
    public FollowCamera camera;
    public bool IsPlayerDead { get; set; }
    private List<Sprite> Fruits { get; set; }
    private List<Sprite> Collected { get; set; }
    public int totalCollected => Collected.Count;
    public int totalFruits => Collected.Count + Fruits.Count;
    public bool IsComplete => Fruits.Count == 0;
    private List<Rectangle> collisions;
    private List<Rectangle> dangers;
    private Texture2D terrain, fruits, spikes, saw, hitboxes;
    
    private readonly int TILESIZE = 16 * Game1.SCALE;
    private readonly int SAW_OFFSET = 22 * Game1.SCALE;

    public Level(Game1 game, ContentManager content, GraphicsDevice graphics, string mapPath, PlayerAbility ability)
    {
        this.game = game;
        this.content = content;
        this.graphics = graphics;
        this.ability = ability;

        LoadContent();
        LoadMap(mapPath);
    }

    private void LoadContent()
    {
        terrain = content.Load<Texture2D>("terrain");
        fruits = content.Load<Texture2D>("fruits");
        hitboxes = content.Load<Texture2D>("hitboxes");
        spikes = content.Load<Texture2D>("spikes");
        saw = content.Load<Texture2D>("saw");
    }
    
    private void LoadMap(string mapPath)
    {
        using var stream = File.OpenRead(mapPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Map = JsonSerializer.Deserialize<TiledMap>(stream, options);
        layers = new Dictionary<string, Dictionary<Vector2, int>>();
        Fruits = new List<Sprite>();
        Collected = new List<Sprite>();

        foreach (var layer in Map.layers)
        {
            if (layer.name == "player_start")
            {
                playerPosition = new Vector2(layer.objects[0].x, layer.objects[0].y);
            }
            Dictionary<Vector2, int> layerDict = new();
            for (int y = 0; y < layer.height; y++)
            {
                for (int x = 0; x < layer.width; x++)
                {
                    int idx = y * layer.width + x;
                    int gid = layer.data[idx];
                    if (gid == 0) continue;
                    layerDict[new Vector2(x, y)] = gid;
                    var tileset = GetTilesetForGid(gid, Map.tilesets);
                    if (tileset.image == "fruits")
                    {
                        var localId = gid - tileset.firstgid;
                        int tileX = localId % tileset.columns;
                        int tileY = localId / tileset.columns;
                        var fruitTexture = GetTextureForName(tileset.image);
                        var srcRect = new Rectangle(tileX * 16, tileY * 16, 16, 16);
                        var rect = new Rectangle(x * TILESIZE, y * TILESIZE, TILESIZE, TILESIZE);
                        var fruit = new Sprite(fruitTexture, rect, srcRect);
                        Fruits.Add(fruit);
                    }
                }
            }
            layers[layer.name] = layerDict;
        }

        collisions = GetListOfRectFrom(layers["collisions"]);
        dangers = GetListOfRectFrom(layers["hit_dangers"]);

        Texture2D texture = content.Load<Texture2D>("player1");
        Texture2D idle = content.Load<Texture2D>("player/Idle");
        Texture2D run = content.Load<Texture2D>("player/Run");
        Texture2D jump = content.Load<Texture2D>("player/Jump");
        Texture2D fall = content.Load<Texture2D>("player/Fall");
        Texture2D dash = content.Load<Texture2D>("player/Dash");
        Dictionary<String, Texture2D> animationsTextures = new()
        {
            ["Idle"] = idle,
            ["Run"] = run,
            ["Jump"] = jump,
            ["Fall"] = fall,
            ["Dash"] = dash,
        };
        player = new Player(texture, 
            new Rectangle(
            (int)playerPosition.X * Game1.SCALE,
            (int)playerPosition.Y * Game1.SCALE,
            32 * Game1.SCALE,
            32 * Game1.SCALE),
            new Rectangle(0, 0, 32, 32),
            animationsTextures);
        player.SetAbility(ability);
        camera = new FollowCamera(new Viewport(0, 0, 1280, 720), 0.2f);
        camera.SetPosition(new Vector2(player.rect.X, player.rect.Y),
            new Point(player.rect.Width,
                player.rect.Height));
    }

    public void Update(GameTime gameTime, KeyboardState keyboardState)
    {
        if (player.IsDashing)
        {
            float step = player.DashStep;

            while (player.dashDistanceRemaining > 0f)
            {
                float move = Math.Min(step, player.dashDistanceRemaining);
                player.rect.X += (int)(move * player.DashDirection);

                bool collided = false;
                foreach (var collision in collisions)
                {
                    if (collision.Intersects(player.PlayerRect))
                    {
                        player.rect.X -= (int)(move * player.DashDirection);
                        collided = true;
                        break;
                    }
                }

                player.dashDistanceRemaining -= move;

                if (collided)
                {
                    player.dashDistanceRemaining = 0f;
                    break;
                }
            }

            if (player.dashDistanceRemaining <= 0f)
            {
                player.IsDashing = false;
                player.dashDistanceRemaining = 0f;
            }

            return;
        }

        player.rect.X += (int)player.velocity.X;
            
        foreach (var collision in collisions)
        {
            if (collision.Intersects(player.PlayerRect))
            {
                player.rect.X -= (int)player.velocity.X;
            }
        }

        player.rect.Y += (int)player.velocity.Y;

        player.Grounded = false;
        // player on ground check
        Rectangle onePixelBelow = player.PlayerRect;
        onePixelBelow.Offset(0, 1);

        foreach (var collision in collisions)
        {
            if (collision.Intersects(player.PlayerRect))
            {
                var direction = player.velocity.Y > 0 ? 1 : -1;
                while (collision.Intersects(player.PlayerRect))
                    {
                        player.rect.Y -= direction;
                    }
                player.velocity.Y = 0;
            }
            if (collision.Intersects(onePixelBelow))
            {
                player.Grounded = true;
            }
        }

        foreach (var danger in dangers)
        {
            if (danger.Intersects(player.PlayerRect))
            {
                IsPlayerDead = true;
            }
        }

        foreach (var fruit in Fruits)
        {
            if (fruit.rect.Intersects(player.PlayerRect))
            {
                Collected.Add(fruit);
            }
        }

        foreach (var fruit in Collected)
        {
            Fruits.Remove(fruit);
        }

        Vector2 playerPosition = new Vector2(player.rect.X, player.rect.Y);
        camera.Update(playerPosition, new Point(player.rect.Width, player.rect.Height));
        player.Update(gameTime, keyboardState);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        player.Draw(spriteBatch);

        foreach (var layer in layers)
        {
            if (layer.Key == "collisions" || layer.Key == "hit_dangers")
                continue;
            DrawLayerDict(layer.Value, spriteBatch);
        }

        foreach (var fruit in Fruits)
        {
            fruit.Draw(spriteBatch);
        }
    }

    private void DrawLayerDict(Dictionary<Vector2, int> layerDict, SpriteBatch spriteBatch)
    {
        if (layerDict == null) return;
        foreach (var item in layerDict)
        {
            var pos = item.Key;
            var gid = item.Value;
            var tileset = GetTilesetForGid(gid, Map.tilesets);
            var localId = gid - tileset.firstgid;
            var columns = tileset.columns;
            var width = tileset.tilewidth;
            var height = tileset.tileheight;
            var src = GetTextureForName(tileset.image);

            int tileX = localId % columns;
            int tileY = localId / columns;

            Rectangle sourceRect = new Rectangle(
                tileX * width,
                tileY * height,
                width,
                height);

            Rectangle destination;

            if (tileset.image != "saw")
            {
                destination = new Rectangle(
                (int)pos.X * TILESIZE,
                (int)pos.Y * TILESIZE,
                TILESIZE,
                TILESIZE);
            }
            else
            {
                destination = new Rectangle(
                (int)(pos.X * TILESIZE),
                (int)(pos.Y * TILESIZE - SAW_OFFSET),
                width * Game1.SCALE,
                height * Game1.SCALE);
            }

            if (tileset.image != "fruits")
                spriteBatch.Draw(src, destination, sourceRect, Color.White);
        }
    }

    private Tileset GetTilesetForGid(int gid, List<Tileset> tilesets)
    {
        Tileset result = null;
        var best = 0;
        foreach (var ts in tilesets)
        {
            if (gid >= ts.firstgid && ts.firstgid > best)
            {
                result = ts;
                best = ts.firstgid;
            }
        }
        return result;
    }

    private Texture2D GetTextureForName(string name)
    {
        return name switch
        {
            "terrain" => terrain,
            "fruits" => fruits,
            "saw" => saw,
            "spikes" => spikes,
            "hitboxes" => hitboxes,
            _ => null
        };
    }

    private List<Rectangle> GetListOfRectFrom(Dictionary<Vector2, int> tiles)
    {
        List<Rectangle> result = new();
        foreach (var tile in tiles)
        {
            var tileset = GetTilesetForGid(tile.Value, Map.tilesets);
            var id = tile.Value - tileset.firstgid;
            int x = (int)tile.Key.X * TILESIZE;
            int y = (int)tile.Key.Y * TILESIZE;
            int width = TILESIZE;
            int height = TILESIZE;
            // spikes
            if (id == 9)
            {
                y += TILESIZE / 2;
                height /= 2;
            } 
            Rectangle tileRect = new Rectangle(x, y, width, height);
            result.Add(tileRect);
        }

        return result;
    }
}