using GonkKonkGame.Sprite_Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using tainicom.Aether.Physics2D;
using tainicom.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework.Content;
using System.Text;

namespace GonkKonkGame
{
    public class GonkKonk : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont spriteFont;

        private Player player;
        private bool Loser = false;

        List<Bullet> bullets = new List<Bullet>();
        List<Enemy> enemies = new List<Enemy>();
        List<Konk> konks = new List<Konk>();
        int score = 0;

        KeyboardState keyboardCurrent;
        KeyboardState keyboardPrior = Keyboard.GetState();

        System.Random random;
        float spawnTimer = 0.0f;

        public GonkKonk()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = Constants.GAME_WIDTH;
            _graphics.PreferredBackBufferHeight = Constants.GAME_HEIGHT;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            random = new System.Random(DateTime.Now.Millisecond);

            //declare the sprite classes
            player = new Player();
            konks.Add(new Konk(Content, 0));
            konks.Add(new Konk(Content, 1));
            konks.Add(new Konk(Content, 2));
            konks.Add(new Konk(Content, 3));
            konks.Add(new Konk(Content, 4));
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            player.LoadContent(Content);
            spriteFont = Content.Load<SpriteFont>("ComicSansMS30");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (!Loser)
            {
                player.Update(gameTime);

                //handle input for gun
                keyboardCurrent = Keyboard.GetState();
                if (keyboardCurrent.IsKeyDown(Keys.Space) && !(keyboardPrior.IsKeyDown(Keys.Space)))
                {
                    if (player.facingRight)
                    {
                        bullets.Add(new Bullet(Content, player.Position + new Vector2(90, -15), player.facingRight));
                    }
                    else
                    {
                        bullets.Add(new Bullet(Content, player.Position + new Vector2(-155, -15), player.facingRight));
                    }
                }
                keyboardPrior = keyboardCurrent;
            }
            if(spawnTimer <= 0)
            {
                enemies.Add(new Enemy(Content));
                spawnTimer = (float)random.Next(3, 11) / 10;
            }
            else
            {
                spawnTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            
            //handle bullet positions
            foreach (Bullet b in bullets)
            {
                b.Update(gameTime);
            }
            foreach (Enemy e in enemies)
            {
                e.Update(gameTime);
            }

            //check for collision between enemies and bullets
            List<Bullet> bToRemove = new List<Bullet>();
            List<Enemy> eToRemove = new List<Enemy>();
            foreach (Bullet b in bullets)
            {
                foreach (Enemy e in enemies)
                {
                    if (b.CollidesWith(e.CollisionBox))
                    {
                        bToRemove.Add(b);
                        eToRemove.Add(e);
                        score += 100;
                    }
                }
            }
            foreach (Bullet b in bToRemove)
            {
                bullets.Remove(b);
            }
            foreach (Enemy e in eToRemove)
            {
                enemies.Remove(e);
            }
            bToRemove.Clear();
            eToRemove.Clear();

            //check for collisions between enemies and konks
            List<Konk> kToRemove = new List<Konk>();
            foreach (Enemy e in enemies)
            {
                foreach(Konk k in konks)
                {
                    if (e.CollidesWith(k.CollisionBox))
                    {
                        eToRemove.Add(e);
                        kToRemove.Add(k);
                    }
                }
            }
            foreach (Konk k in kToRemove)
            {
                konks.Remove(k);
            }
            foreach (Enemy e in eToRemove)
            {
                enemies.Remove(e);
            }
            kToRemove.Clear();
            eToRemove.Clear();

            //check for loss condition
            if(konks.Count < 4)
            {
                Loser = true;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            StringBuilder sb = new StringBuilder();
            sb.Append("Score: ");
            sb.Append(score.ToString());
            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            if (!Loser)
            {
                _spriteBatch.DrawString(spriteFont, "Protect at least 4 of your precious Konks!", new Vector2(0, 0), Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                _spriteBatch.DrawString(spriteFont, sb.ToString(), new Vector2(5, 850), Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }
            else
            {
                _spriteBatch.DrawString(spriteFont, "YOU HAVE FAILED, press esc to exit", new Vector2(0, 0), Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                _spriteBatch.DrawString(spriteFont, sb.ToString(), new Vector2(5, 850), Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            }

            foreach(Bullet b in bullets)
            {
                b.Draw(gameTime, _spriteBatch);
            }
            foreach(Enemy e in enemies)
            {
                e.Draw(gameTime, _spriteBatch);
            }
            foreach(Konk k in konks)
            {
                k.Draw(gameTime, _spriteBatch);
            }
            player.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
