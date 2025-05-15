using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;


namespace Fruit_Quest
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        

        Player player;
        private TiledMap level1;
        private Dictionary<Vector2, int> middle;
        private Dictionary<Vector2, int> fore;
        private Dictionary<Vector2, int> collisions;
        private Dictionary<Vector2, int> dangers;
        private Dictionary<Vector2, int> collectible;
        private Texture2D terrain;
        private Texture2D fruits;
        private Texture2D saw;
        private Texture2D spikes;
        private Texture2D hitboxes;
        private FollowCamera camera;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            using var stream = File.OpenRead("../../../Data/level1.tmj");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            level1 = JsonSerializer.Deserialize<TiledMap>(stream, options);

            foreach (var layer in level1.layers)
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
                    }
                }

                switch (layer.name)
                {
                    case "middle":
                        middle = new Dictionary<Vector2, int>(layerDict);
                        break;
                    case "fore":
                        fore = new Dictionary<Vector2, int>(layerDict);
                        break;
                    case "collisions":
                        collisions = new Dictionary<Vector2, int>(layerDict);
                        break;
                    case "hit_collectible":
                        collectible = new Dictionary<Vector2, int>(layerDict);
                        break;
                    case "hit_dangers":
                        dangers = new Dictionary<Vector2, int>(layerDict);
                        break;
                }
            }

            Texture2D texture = Content.Load<Texture2D>("player1");
            player = new Player(texture, new Vector2(500, 300));
            terrain = Content.Load<Texture2D>("terrain");
            fruits = Content.Load<Texture2D>("fruits");
            hitboxes = Content.Load<Texture2D>("hitboxes");
            spikes = Content.Load<Texture2D>("spikes");
            saw = Content.Load<Texture2D>("saw");
            camera = new FollowCamera(GraphicsDevice.Viewport, 0.2f);
            camera.SetPosition(player.position, new Point(player.Rect.Width, player.Rect.Height));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            var newPlayerPosition = player.UpdatePos(gameTime);
            Rectangle futureRectPlayer = new(
                (int)newPlayerPosition.X + 4 * (int)Player.hitboxPosition.X,
                (int)newPlayerPosition.Y + 4 * (int)Player.hitboxPosition.Y,
                player.playerRect.Width,
                player.playerRect.Height);

            bool collision = false;

            foreach (var tile in collisions)
            {
                Rectangle tileRect = new Rectangle(
                    (int)tile.Key.X * 64, (int)tile.Key.Y * 64, 64, 64);

                if (futureRectPlayer.Intersects(tileRect))
                {
                    collision = true;
                    break;
                }
            }

            if (!collision)
            {
                player.position = newPlayerPosition;
            }

            Vector2 playerPosition = player.position;
            camera.Update(playerPosition, new Point(player.Rect.Width, player.Rect.Height));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.Transform);

            int tileSize = 16;

            player.Draw(_spriteBatch);

            DrawLayerDict(middle, tileSize);
            DrawLayerDict(collisions, tileSize);
            DrawLayerDict(fore, tileSize);
            DrawLayerDict(dangers, tileSize);
            DrawLayerDict(collectible, tileSize);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawLayerDict(Dictionary<Vector2, int> layerDict, int tileSize)
        {
            foreach (var item in layerDict)
            {
                var pos = item.Key;
                var gid = item.Value;
                var tileset = GetTilesetForGid(gid, level1.tilesets);
                var localId = gid - tileset.firstgid;
                var columns = tileset.columns;
                var rows = tileset.rows;
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
                    (int)pos.X * tileSize * 4,
                    (int)pos.Y * tileSize * 4,
                    width * 4,
                    height * 4);
                }
                else
                {
                   destination = new Rectangle(
                   (int)(pos.X * tileSize) * 4,
                   (int)(pos.Y * tileSize - 22) * 4,
                   width * 4,
                   height * 4);
                }
                

                _spriteBatch.Draw(src, destination, sourceRect, Color.White);
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
    }
}
