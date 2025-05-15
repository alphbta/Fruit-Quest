using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fruit_Quest
{
    public class FollowCamera
    {
        public Matrix Transform { get; private set; }
        public Vector2 Position { get; private set; }

        private Viewport _viewport;
        private float _smoothSpeed;

        public FollowCamera(Viewport viewport, float smoothSpeed)
        {
            _viewport = viewport;
            _smoothSpeed = smoothSpeed;
            Position = Vector2.Zero;
        }

        public void Update(Vector2 targetPosition, Point spriteSize)
        {
            var centeredTarget = targetPosition + new Vector2(spriteSize.X / 2f, spriteSize.Y / 2f);
            Vector2 desiredPosition = centeredTarget - new Vector2(_viewport.Width / 2f, _viewport.Height / 2f);
            Position = Vector2.Lerp(Position, desiredPosition, _smoothSpeed);

            Transform = Matrix.CreateTranslation(new Vector3(-Position, 0));
        }

        public void SetPosition(Vector2 targetPosition, Point spriteSize)
        {
            var centeredTarget = targetPosition + new Vector2(spriteSize.X / 2f, spriteSize.Y / 2f);
            Position = centeredTarget - new Vector2(_viewport.Width / 2f, _viewport.Height / 2f);
            Transform = Matrix.CreateTranslation(new Vector3(-Position, 0));
        }
    }
}
