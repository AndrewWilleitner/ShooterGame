// Projectile.cs
//Using declarations
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter_Game.Model
{
    public class Projectile
    {
        // Image representing the Projectile
        public Texture2D Texture;

        // Position of the Projectile relative to the upper left side of the screen
        public Vector2 Position;

        // State of the Projectile
        public bool Active;

        // The amount of damage the projectile can inflict to an enemy
        public int Damage;

        // if the projectile is for the slicer weapon
        public int SlicerID;

        // Represents the viewable boundary of the game
        Viewport viewport;



        // Get the width of the projectile ship
        public int Width
        {
            get { return Texture.Width; }
        }

        // Get the height of the projectile ship
        public int Height
        {
            get { return Texture.Height; }
        }

        // Determines how fast the projectile moves
        float projectileMoveSpeed;


        public void Initialize(Viewport viewport, Texture2D texture, Vector2 position, int slicerID, int damage)
        {
            Texture = texture;
            Position = position;
            SlicerID = slicerID;
            this.viewport = viewport;

            Active = true;

            //default 2
            Damage = damage;

            projectileMoveSpeed = 20f;
        }

        public void Update()
        {
            if (SlicerID == 0)
            {
                // Projectiles always move to the right
                Position.X += projectileMoveSpeed;
            } 
            else if (SlicerID == 1)
            {
                Position.Y += projectileMoveSpeed;
            }
            else if (SlicerID == 2)
            {
                Position.Y -= projectileMoveSpeed;
            }
            else if (SlicerID == 3)
            {
                Position.X += projectileMoveSpeed;
            }
            else if (SlicerID == 4)
            {
                Position.X -= projectileMoveSpeed;
            }
            else if (SlicerID == 5)
            {
                Position.Y += projectileMoveSpeed;
                Position.X += projectileMoveSpeed;
            }
            else if (SlicerID == 6)
            {
                Position.Y -= projectileMoveSpeed;
                Position.X += projectileMoveSpeed;
            }
            else if (SlicerID == 7)
            {
                Position.Y += projectileMoveSpeed;
                Position.X -= projectileMoveSpeed;
            }
            else if (SlicerID == 8)
            {
                Position.Y -= projectileMoveSpeed;
                Position.X -= projectileMoveSpeed;
            }

            // Deactivate the bullet if it goes out of screen
            if ((Position.X + Texture.Width / 2 > viewport.Width) ||
                (Position.X + Texture.Width / 2 < 0) ||
                (Position.Y + Texture.Height / 2 > viewport.Height) ||
                (Position.Y + Texture.Height / 2 < 0))
                Active = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, 0f,
            new Vector2(Width / 2, Height / 2), 1f, SpriteEffects.None, 0f);
        }
    }
}
