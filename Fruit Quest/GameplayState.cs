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
    Player player;
    private TiledMap level1Map;
    private Dictionary<string, Dictionary<Vector2, int>> layers;
    private List<Rectangle> collisionsList;
    private List<Rectangle> dangersList;
    private Texture2D terrain;
    private Texture2D fruits;
    private Texture2D saw;
    private Texture2D spikes;
    private Texture2D hitboxes;
    private FollowCamera camera;
    private KeyboardState keyboardState;
    private List<Sprite> spritesFruits;
    private List<Sprite> collected;
    private Game1 game;
    private Viewport viewport;
    private ContentManager content;
    
    private readonly int TILESIZE = 16 * Game1.SCALE;
    private readonly int SAW_OFFSET = 22 * Game1.SCALE;
    
    public GameplayState(Viewport viewport, ContentManager content, Game1 game)
    {
        this.game = game;
        this.viewport = viewport;
        this.content = content;
        camera = new FollowCamera(viewport, 0.2f);
        LoadLevel();
    }

    public void LoadContent()
    {
        terrain = content.Load<Texture2D>("terrain");
        fruits = content.Load<Texture2D>("fruits");
        hitboxes = content.Load<Texture2D>("hitboxes");
        spikes = content.Load<Texture2D>("spikes");
        saw = content.Load<Texture2D>("saw");
        
        LoadLevel();
    }

    public void Update(GameTime gameTime)
    {
        keyboardState = Keyboard.GetState();
        player.Update(keyboardState);

        player.rect.X += (int)player.velocity.X;
            
        foreach (var collision in collisionsList)
        {
            if (collision.Intersects(player.PlayerRect))
            {
                player.rect.X -= (int)player.velocity.X;
            }
        }

        player.rect.Y += (int)player.velocity.Y;

        player.Grounded = false;

        foreach (var collision in collisionsList)
        {
            if (collision.Intersects(player.PlayerRect))
            {
                player.rect.Y -= (int)player.velocity.Y;
                player.velocity.Y = 0;
                player.Grounded = true;
            }
        }

        foreach (var danger in dangersList)
        {
            if (danger.Intersects(player.PlayerRect))
            {
                RestartLevel();
                return;
            }
        }

        foreach (var fruit in spritesFruits)
        {
            if (fruit.rect.Intersects(player.PlayerRect))
            {
                collected.Add(fruit);
            }
        }

        foreach (var fruit in collected)
        {
            spritesFruits.Remove(fruit);
        }

        Vector2 playerPosition = new Vector2(player.rect.X, player.rect.Y);
        camera.Update(playerPosition, new Point(player.rect.Width, player.rect.Height));
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.Transform);

        player.Draw(spriteBatch);
            
        foreach (var layer in layers)
        {
            DrawLayerDict(layer.Value, spriteBatch);
        }

        foreach (var fruit in spritesFruits)
        {
            fruit.Draw(spriteBatch);
        }

        spriteBatch.End();
    }

    private void LoadLevel()
    {
        using var stream = File.OpenRead("../../../Data/level1.tmj");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        level1Map = JsonSerializer.Deserialize<TiledMap>(stream, options);
        spritesFruits = new List<Sprite>();
        collected = new List<Sprite>();
        layers = new Dictionary<string, Dictionary<Vector2, int>>();

        foreach (var layer in level1Map.layers)
        {
            Dictionary<Vector2, int> layerDict = new();
            for (int y = 0; y < layer.height; y++)
            {
                for (int x = 0; x < layer.width; x++)
                {
                    int idx = y * layer.width + x;
                    int gid = layer.data[idx];
                    if (gid == 0) continue;
                    layerDict[new Vector2(x, y)] = gid;
                    var tileset = GetTilesetForGid(gid, level1Map.tilesets);
                    if (tileset.image == "fruits")
                    {
                        var localId = gid - tileset.firstgid;
                        int tileX = localId % tileset.columns;
                        int tileY = localId / tileset.columns;
                        var fruitTexture = GetTextureForName(tileset.image);
                        var srcRect = new Rectangle(tileX * 16, tileY * 16, 16, 16);
                        var rect = new Rectangle(x * TILESIZE, y * TILESIZE, TILESIZE, TILESIZE);
                        var fruit = new Sprite(fruitTexture, rect, srcRect);
                        spritesFruits.Add(fruit);
                    }
                }
            }
            layers[layer.name] = layerDict;
        }

        collisionsList = GetListOfRectFrom(layers["collisions"]);
        dangersList = GetListOfRectFrom(layers["hit_dangers"]);

        Texture2D texture = content.Load<Texture2D>("player1");
        player = new Player(texture, new Rectangle(92, 760, 32 * 4, 32 * 4), new Rectangle(0, 0, 32, 32));
        camera = new FollowCamera(new Viewport(0, 0, 1280, 720), 0.2f);
        camera.SetPosition(new Vector2(player.rect.X, player.rect.Y),
            new Point(player.rect.Width,
                player.rect.Height));
    }
    
    private void DrawLayerDict(Dictionary<Vector2, int> layerDict, SpriteBatch spriteBatch)
        {
            if (layerDict == null) return;
            foreach (var item in layerDict)
            {
                var pos = item.Key;
                var gid = item.Value;
                var tileset = GetTilesetForGid(gid, level1Map.tilesets);
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
                Rectangle tileRect = new Rectangle(
                    (int)tile.Key.X * TILESIZE, (int)tile.Key.Y * TILESIZE, TILESIZE, TILESIZE);

                result.Add(tileRect);
            }

            return result;
        }

        private void RestartLevel()
        {
            LoadLevel();
        }
}