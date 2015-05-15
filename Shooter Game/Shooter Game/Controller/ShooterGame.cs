using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Shooter_Game.Model;
using Shooter_Game.View;



namespace Shooter_Game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Represents the player
        private Player player;

        // Keyboard states used to determine key presses
        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;

        // Gamepad states used to determine button presses
        private GamePadState currentGamePadState;
        private GamePadState previousGamePadState;

        // A movement speed for the player
        private float playerMoveSpeed;

        // Image used to display the static background
        Texture2D mainBackground;

        // Parallaxing Layers
        ParallaxingBackground bgLayer1;
        ParallaxingBackground bgLayer2;

        // Enemies
        Texture2D enemyTexture;
        List<Enemy> enemies;

        // The rate at which the enemies appear
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        // A random number generator
        Random random;

        Texture2D projectileTexture;
        List<Projectile> projectiles;

        // The rate of fire of the player laser
        TimeSpan fireTime;
        TimeSpan previousFireTime;

        Texture2D explosionTexture;
        List<Animation> explosions;

        // The sound that is played when a laser is fired
        SoundEffect laserSound;

        // The sound used when the player or an enemy dies
        SoundEffect explosionSound;

        // The music played during gameplay
        Song gameplayMusic;

        //Number that holds the player score
        int score;
        // The font used to display UI elements
        SpriteFont font;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            bgLayer1 = new ParallaxingBackground();
            bgLayer2 = new ParallaxingBackground();

            // Initialize the player class
            player = new Player();

            // Set a constant player move speed
            playerMoveSpeed = 8.0f;

            // Initialize the enemies list
            enemies = new List<Enemy>();

            // Set the time keepers to zero
            previousSpawnTime = TimeSpan.Zero;

            // Used to determine how fast enemy respawns
            enemySpawnTime = TimeSpan.FromSeconds(0.2f);

            // Initialize our random number generator
            random = new Random();

            projectiles = new List<Projectile>();

            // Set the laser to fire every quarter second
            fireTime = TimeSpan.FromSeconds(.15f);

            explosions = new List<Animation>();

            //Set player's score to zero
            score = 0;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the player resources
            Animation playerAnimation = new Animation();
            Texture2D playerTexture = Content.Load<Texture2D>("Images/shipAnimation");
            playerAnimation.Initialize(playerTexture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);

            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y
            + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
            player.Initialize(playerAnimation, playerPosition);

            // Load the parallaxing background
            bgLayer1.Initialize(Content, "bgLayer1", GraphicsDevice.Viewport.Width, -1);
            bgLayer2.Initialize(Content, "bgLayer2", GraphicsDevice.Viewport.Width, -2);

            enemyTexture = Content.Load<Texture2D>("Images/mineAnimation");

            projectileTexture = Content.Load<Texture2D>("Images/laser");

            explosionTexture = Content.Load<Texture2D>("Images/explosion");

            // Load the music
            gameplayMusic = Content.Load<Song>("Sounds/gameMusic");

            // Load the laser and explosion sound effect
            laserSound = Content.Load<SoundEffect>("Sounds/laserFire");
            explosionSound = Content.Load<SoundEffect>("Sounds/explosion");

            // Load the score font
            font = Content.Load<SpriteFont>("Fonts/gameFont");

            // Start the music right away
            PlayMusic(gameplayMusic);

            mainBackground = Content.Load<Texture2D>("Images/mainbackground");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if ((GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) ||
                (Keyboard.GetState().IsKeyDown(Keys.J)))
                this.Exit();

            // Save the previous state of the keyboard and game pad so we can determinesingle key/button presses
            previousGamePadState = currentGamePadState;
            previousKeyboardState = currentKeyboardState;

            // Read the current state of the keyboard and gamepad and store it
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Update the parallaxing background
            bgLayer1.Update();
            bgLayer2.Update();

            // Update the enemies
            UpdateEnemies(gameTime);

            // Update the collision
            UpdateCollision();

            // Update the projectiles
            UpdateProjectiles();

            // Update the explosions
            UpdateExplosions(gameTime);

            //Update the player
            UpdatePlayer(gameTime);

            base.Update(gameTime);
        }

        private void AddProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile();
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position, 0, 2);
            projectiles.Add(projectile);
        }

        private void AddSlicerProjectile(Vector2 position, int directionID)
        {
            Projectile projectile = new Projectile();
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position, directionID, 2);
            projectiles.Add(projectile);
        }

        private void AddWallProjectile(Vector2 position)
        {
            Projectile projectile = new Projectile();
            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position, 0, 9999);
            projectiles.Add(projectile);
        }

        private void UpdateProjectiles()
        {
            // Update the Projectiles
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Update();

                if (projectiles[i].Active == false)
                {
                    projectiles.RemoveAt(i);
                }
            }
        }

        private void AddExplosion(Vector2 position)
        {
            Animation explosion = new Animation();
            explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
            explosions.Add(explosion);
        }

        private void UpdateExplosions(GameTime gameTime)
        {
            for (int i = explosions.Count - 1; i >= 0; i--)
            {
                explosions[i].Update(gameTime);
                if (explosions[i].Active == false)
                {
                    explosions.RemoveAt(i);
                }
            }
        }

        private void UpdatePlayer(GameTime gameTime)
        {
            player.Update(gameTime);

            // Get Thumbstick Controls
            player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
            player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

            // Use the Keyboard / Dpad
            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
            currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                player.Position.X -= playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
            currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                player.Position.X += playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Up) ||
            currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                player.Position.Y -= playerMoveSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Down) ||
            currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                player.Position.Y += playerMoveSpeed;
            }

            // Make sure that the player does not go out of bounds
            player.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
            player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

            // Fire only every interval we set as the fireTime
            //       
            //       
            //      O--
            //       
            //       
            if ((gameTime.TotalGameTime - previousFireTime > fireTime) && 
                (Keyboard.GetState().IsKeyDown(Keys.Z)))
            {
                // Change fire time
                fireTime = TimeSpan.FromSeconds(.15f);

                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                AddProjectile(player.Position + new Vector2(player.Width / 2, 0));

                // Play the laser sound
                laserSound.Play();
            }
            // Fires THE BIG ONE!!
            //       --
            //       --
            //      O--
            //       --
            //       --
            if ((gameTime.TotalGameTime - previousFireTime > fireTime) &&
                (Keyboard.GetState().IsKeyDown(Keys.X)))
            {
                // change the fire time
                fireTime = TimeSpan.FromSeconds(.02f);

                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                AddProjectile(player.Position + new Vector2(player.Width / 2, 0));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 5));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 10));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 20));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 30));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 40));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 50));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 60));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 70));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 80));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 90));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 95));
                AddProjectile(player.Position + new Vector2(player.Width / 2, 100));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -5));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -10));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -20));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -30));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -40));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -50));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -60));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -70));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -80));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -90));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -95));
                AddProjectile(player.Position + new Vector2(player.Width / 2, -100));

                // Play the laser sound
                laserSound.Play();
            }
            // Fires THE SLICER
            //    \ | /
            //     \|/
            //    --O--
            //     /|\
            //    / | \
            if ((gameTime.TotalGameTime - previousFireTime > fireTime) &&
                (Keyboard.GetState().IsKeyDown(Keys.C)))
            {
                // Change fire time
                fireTime = TimeSpan.FromSeconds(.001f);

                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile with there direction id
                AddSlicerProjectile(player.Position + new Vector2(10, 0), 1);
                AddSlicerProjectile(player.Position + new Vector2(10, 2), 1);
                AddSlicerProjectile(player.Position + new Vector2(10, 4), 1);
                AddSlicerProjectile(player.Position + new Vector2(10, 6), 1);
                AddSlicerProjectile(player.Position + new Vector2(10, 8), 1);
                AddSlicerProjectile(player.Position + new Vector2(10, 10), 1);
                AddSlicerProjectile(player.Position + new Vector2(10, 12), 1);
                AddSlicerProjectile(player.Position + new Vector2(10, 14), 1);
                AddSlicerProjectile(player.Position + new Vector2(10, 16), 1);
                AddSlicerProjectile(player.Position + new Vector2(10, 18), 1);
                AddSlicerProjectile(player.Position + new Vector2(10, 20), 1);

                AddSlicerProjectile(player.Position + new Vector2(10, 0), 2);
                AddSlicerProjectile(player.Position + new Vector2(10, -2), 2);
                AddSlicerProjectile(player.Position + new Vector2(10, -4), 2);
                AddSlicerProjectile(player.Position + new Vector2(10, -6), 2);
                AddSlicerProjectile(player.Position + new Vector2(10, -8), 2);
                AddSlicerProjectile(player.Position + new Vector2(10, -10), 2);
                AddSlicerProjectile(player.Position + new Vector2(10, -12), 2);
                AddSlicerProjectile(player.Position + new Vector2(10, -14), 2);
                AddSlicerProjectile(player.Position + new Vector2(10, -16), 2);
                AddSlicerProjectile(player.Position + new Vector2(10, -18), 2);
                AddSlicerProjectile(player.Position + new Vector2(10, -20), 2);

                AddSlicerProjectile(player.Position + new Vector2(10, 0), 3);
                AddSlicerProjectile(player.Position + new Vector2(10, 2), 3);
                AddSlicerProjectile(player.Position + new Vector2(10, 4), 3);
                AddSlicerProjectile(player.Position + new Vector2(10, 6), 3);
                AddSlicerProjectile(player.Position + new Vector2(10, 8), 3);
                AddSlicerProjectile(player.Position + new Vector2(10, 10), 3);
                AddSlicerProjectile(player.Position + new Vector2(10, -2), 3);
                AddSlicerProjectile(player.Position + new Vector2(10, -4), 3);
                AddSlicerProjectile(player.Position + new Vector2(10, -6), 3);
                AddSlicerProjectile(player.Position + new Vector2(10, -8), 3);
                AddSlicerProjectile(player.Position + new Vector2(10, -10), 3);

                AddSlicerProjectile(player.Position + new Vector2(10, 0), 4);
                AddSlicerProjectile(player.Position + new Vector2(10, -2), 4);
                AddSlicerProjectile(player.Position + new Vector2(10, -4), 4);
                AddSlicerProjectile(player.Position + new Vector2(10, -6), 4);
                AddSlicerProjectile(player.Position + new Vector2(10, -8), 4);
                AddSlicerProjectile(player.Position + new Vector2(10, -10), 4);
                AddSlicerProjectile(player.Position + new Vector2(10, 2), 4);
                AddSlicerProjectile(player.Position + new Vector2(10, 4), 4);
                AddSlicerProjectile(player.Position + new Vector2(10, 6), 4);
                AddSlicerProjectile(player.Position + new Vector2(10, 8), 4);
                AddSlicerProjectile(player.Position + new Vector2(10, 10), 4);

                AddSlicerProjectile(player.Position + new Vector2(10, 0), 5);
                AddSlicerProjectile(player.Position + new Vector2(12, 2), 5);
                AddSlicerProjectile(player.Position + new Vector2(14, 4), 5);
                AddSlicerProjectile(player.Position + new Vector2(16, 6), 5);
                AddSlicerProjectile(player.Position + new Vector2(18, 8), 5);
                AddSlicerProjectile(player.Position + new Vector2(20, 10), 5);
                AddSlicerProjectile(player.Position + new Vector2(22, 12), 5);
                AddSlicerProjectile(player.Position + new Vector2(24, 14), 5);
                AddSlicerProjectile(player.Position + new Vector2(26, 16), 5);
                AddSlicerProjectile(player.Position + new Vector2(28, 18), 5);
                AddSlicerProjectile(player.Position + new Vector2(30, 20), 5);

                AddSlicerProjectile(player.Position + new Vector2(10, 0), 6);
                AddSlicerProjectile(player.Position + new Vector2(12, -2), 6);
                AddSlicerProjectile(player.Position + new Vector2(14, -4), 6);
                AddSlicerProjectile(player.Position + new Vector2(16, -6), 6);
                AddSlicerProjectile(player.Position + new Vector2(18, -8), 6);
                AddSlicerProjectile(player.Position + new Vector2(20, -10), 6);
                AddSlicerProjectile(player.Position + new Vector2(22, -12), 6);
                AddSlicerProjectile(player.Position + new Vector2(24, -14), 6);
                AddSlicerProjectile(player.Position + new Vector2(26, -16), 6);
                AddSlicerProjectile(player.Position + new Vector2(28, -18), 6);
                AddSlicerProjectile(player.Position + new Vector2(30, -20), 6);

                AddSlicerProjectile(player.Position + new Vector2(10, 0), 7);
                AddSlicerProjectile(player.Position + new Vector2(8, 2), 7);
                AddSlicerProjectile(player.Position + new Vector2(6, 4), 7);
                AddSlicerProjectile(player.Position + new Vector2(4, 6), 7);
                AddSlicerProjectile(player.Position + new Vector2(2, 8), 7);
                AddSlicerProjectile(player.Position + new Vector2(0, 10), 7);
                AddSlicerProjectile(player.Position + new Vector2(-2, 12), 7);
                AddSlicerProjectile(player.Position + new Vector2(-4, 14), 7);
                AddSlicerProjectile(player.Position + new Vector2(-6, 16), 7);
                AddSlicerProjectile(player.Position + new Vector2(-8, 18), 7);
                AddSlicerProjectile(player.Position + new Vector2(-10, 20), 7);

                AddSlicerProjectile(player.Position + new Vector2(10, 0), 8);
                AddSlicerProjectile(player.Position + new Vector2(8, -2), 8);
                AddSlicerProjectile(player.Position + new Vector2(6, -4), 8);
                AddSlicerProjectile(player.Position + new Vector2(4, -6), 8);
                AddSlicerProjectile(player.Position + new Vector2(2, -8), 8);
                AddSlicerProjectile(player.Position + new Vector2(0, -10), 8);
                AddSlicerProjectile(player.Position + new Vector2(-2, -12), 8);
                AddSlicerProjectile(player.Position + new Vector2(-4, -14), 8);
                AddSlicerProjectile(player.Position + new Vector2(-6, -16), 8);
                AddSlicerProjectile(player.Position + new Vector2(-8, -18), 8);
                AddSlicerProjectile(player.Position + new Vector2(-10, -20), 8);

                // Play the laser sound
                laserSound.Play();
            }

            // Fires THE WALL!!
            //       --  --
            //       --  --
            //      O--  --
            //       --  --
            //       --  --
            if ((gameTime.TotalGameTime - previousFireTime > fireTime) &&
                (Keyboard.GetState().IsKeyDown(Keys.V)))
            {
                // change the fire time
                fireTime = TimeSpan.FromSeconds(.5f);

                // Reset our current time
                previousFireTime = gameTime.TotalGameTime;

                // Add the projectile, but add it to the front and center of the player
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 0));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 5));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 10));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 15));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 20));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 25));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 30));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 35));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 40));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 45));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 50));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 55));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 60));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 65));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 70));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 75));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 80));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 85));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 90));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 95));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 100));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 105));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 110));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 115));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 120));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 125));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 130));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 135));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 140));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 145));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 150));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 155));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 160));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 165));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 170));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 175));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 180));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 185));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 190));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 195));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 200));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 205));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 210));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 215));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 220));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 225));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 230));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 235));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 240));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 245));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 250));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 255));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 260));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 265));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 270));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 275));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 280));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 285));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 290));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 295));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, 300));

                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -5));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -10));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -15));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -20));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -25));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -30));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -35));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -40));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -45));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -50));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -55));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -60));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -65));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -70));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -75));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -80));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -85));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -90));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -95));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -100));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -105));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -110));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -115));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -120));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -125));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -130));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -135));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -140));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -145));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -150));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -155));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -160));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -165));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -170));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -175));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -180));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -185));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -190));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -195));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -200));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -205));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -210));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -215));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -220));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -225));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -230));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -235));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -240));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -245));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -250));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -255));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -260));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -265));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -270));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -275));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -280));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -285));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -290));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -295));
                AddWallProjectile(player.Position + new Vector2(player.Width / 2, -300));

                // Play the laser sound
                laserSound.Play();
            }

            // reset score if player health goes to zero
            if (player.Health <= 0)
            {
                player.Health = 100;
                score = 0;
            }
        }

        private void AddEnemy()
        {
            // Create the animation object
            Animation enemyAnimation = new Animation();

            // Initialize the animation with the correct animation information
            enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

            // Randomly generate the position of the enemy
            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2, random.Next(100, GraphicsDevice.Viewport.Height - 100));

            // Create an enemy
            Enemy enemy = new Enemy();

            // Initialize the enemy
            enemy.Initialize(enemyAnimation, position);

            // Add the enemy to the active enemies list
            enemies.Add(enemy);
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            // Spawn a new enemy enemy every 1.5 seconds
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                // Add an Enemy
                AddEnemy();
                AddEnemy();
            }

            // Update the Enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Update(gameTime);

                if (enemies[i].Active == false)
                {
                    // If not active and health <= 0
                    if (enemies[i].Health <= 0)
                    {
                        // Add an explosion
                        AddExplosion(enemies[i].Position);
                        // Play the explosion sound
                        explosionSound.Play();
                        //Add to the player's score
                        score += enemies[i].Value;
                    }

                    enemies.RemoveAt(i);
                }
            }
        }

        private void UpdateCollision()
        {
            // Use the Rectangle's built-in intersect function to 
            // determine if two objects are overlapping
            Rectangle rectangle1;
            Rectangle rectangle2;

            // Only create the rectangle once for the player
            rectangle1 = new Rectangle((int)player.Position.X,
            (int)player.Position.Y,
            player.Width,
            player.Height);

            // Do the collision between the player and the enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                rectangle2 = new Rectangle((int)enemies[i].Position.X,
                (int)enemies[i].Position.Y,
                enemies[i].Width,
                enemies[i].Height);

                // Determine if the two objects collided with each
                // other
                if (rectangle1.Intersects(rectangle2))
                {
                    // Subtract the health from the player based on
                    // the enemy damage
                    player.Health -= enemies[i].Damage;

                    // Since the enemy collided with the player
                    // destroy it
                    enemies[i].Health = 0;

                    // If the player health is less than zero we died
                    if (player.Health <= 0)
                        player.Active = false;
                }

            }

            // Projectile vs Enemy Collision
            for (int i = 0; i < projectiles.Count; i++)
            {
                for (int j = 0; j < enemies.Count; j++)
                {
                    // Create the rectangles we need to determine if we collided with each other
                    rectangle1 = new Rectangle((int)projectiles[i].Position.X -
                    projectiles[i].Width / 2, (int)projectiles[i].Position.Y -
                    projectiles[i].Height / 2, projectiles[i].Width, projectiles[i].Height);

                    rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2,
                    (int)enemies[j].Position.Y - enemies[j].Height / 2,
                    enemies[j].Width, enemies[j].Height);

                    // Determine if the two objects collided with each other
                    if (rectangle1.Intersects(rectangle2))
                    {
                        enemies[j].Health -= projectiles[i].Damage;
                        projectiles[i].Active = false;
                    }
                }
            }
        }

        private void PlayMusic(Song song)
        {
            // Due to the way the MediaPlayer plays music,
            // we have to catch the exception. Music will play when the game is not tethered
            try
            {
                // Play the music
                MediaPlayer.Play(song);

                // Loop the currently playing song
                MediaPlayer.IsRepeating = true;
            }
            catch { }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.SkyBlue);

            // Start drawing
            spriteBatch.Begin();

            spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

            // Draw the moving background
            bgLayer1.Draw(spriteBatch);
            bgLayer2.Draw(spriteBatch);

            // Draw the Enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Draw(spriteBatch);
            }

            // Draw the Projectiles
            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].Draw(spriteBatch);
            }

            // Draw the explosions
            for (int i = 0; i < explosions.Count; i++)
            {
                explosions[i].Draw(spriteBatch);
            }

            // Draw the score
            spriteBatch.DrawString(font, "score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
            // Draw the player health
            spriteBatch.DrawString(font, "health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);

            //
            // All Drawing happens AFTER .begin() is called
            //

            // Draw the Player
            player.Draw(spriteBatch);

            // Stop drawing
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
