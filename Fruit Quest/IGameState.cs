using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fruit_Quest;

public interface IGameState
{
    void LoadContent();
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
}