using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
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
        private List<Rectangle> collisionsList;
        private List<Rectangle> collectibleList;
        private List<Rectangle> dangersList;
        private Texture2D terrain;
        private Texture2D fruits;
        private Texture2D saw;
        private Texture2D spikes;
        private Texture2D hitboxes;
        private FollowCamera camera;
        private KeyboardState keyboardState;

        public readonly static int SCALE = 4; //на сколько увеличиваются все объекты в игре
        private readonly static int TILESIZE = 16 * SCALE;
        private readonly static int SAW_OFFSET = 22 * SCALE;

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

            collisionsList = GetListOfRectFrom(collisions);
            collectibleList = GetListOfRectFrom(collectible);
            dangersList = GetListOfRectFrom(dangers);

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

            keyboardState = Keyboard.GetState();
            player.Update(keyboardState, collisionsList);
            
            foreach (var danger in dangersList)
            {
                if (danger.Intersects(player.playerRect))
                {
                    Exit();
                }
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

            player.Draw(_spriteBatch);

            DrawLayerDict(middle);
            DrawLayerDict(fore);
            DrawLayerDict(collisions);
            DrawLayerDict(dangers);
            DrawLayerDict(collectible);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawLayerDict(Dictionary<Vector2, int> layerDict)
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
                   width * SCALE,
                   height * SCALE);
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
    }
}
