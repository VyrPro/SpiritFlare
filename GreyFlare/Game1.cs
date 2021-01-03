using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace GreyFlare
{
    
    public partial class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static float CamX = 0f;
        public static float CamY = 0f;
        public static SpriteFont baseFont;
        public static Dictionary<string, Texture2D> SpriteList = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Texture2D> TextureList = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Texture2D> ObjTextureList = new Dictionary<string, Texture2D>();
        public static List<Tile> TileList = new List<Tile>();
        public static List<Tile> BoundList = new List<Tile>();
        public static List<WorldObject> ObjectList = new List<WorldObject>();
        public static List<Player> PlayerList = new List<Player>();

        //NPCs
        public static Dictionary<string, Texture2D> NpcSpriteList = new Dictionary<string, Texture2D>();
        public static List<Npc> NpcList = new List<Npc>();
        public static List<Spawn> SpawnList = new List<Spawn>();
       
        public static Dictionary<string, GameItem> ItemList = new Dictionary<string, GameItem>();
        public static Dictionary<int, string> DiscussionList = new Dictionary<int, string>();

        public static List<Skill> SkillsList = new List<Skill>();
        public static List<Quest> QuestList = new List<Quest>();

        //Defines some easily-switchable names.
        public static string GameCurrency = "Gold";
        
        public static Player p;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        // >> LoadContent.cs Class Inserts Here <<
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            
            base.Initialize();
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public class Camera2d
        {
            protected float _zoom; // Camera Zoom
            public Matrix _transform; // Matrix Transform
            public Vector2 _pos; // Camera Position
            protected float _rotation; // Camera Rotation

            public Camera2d()
            {
                _zoom = 1.3f;
                _rotation = 0.0f;
                _pos = Vector2.Zero;
            }
            public float Zoom
            {
                get { return _zoom; }
                set { _zoom = value; if (_zoom < 0.1f) _zoom = 0.1f; } // Negative zoom will flip image
            }

            public float Rotation
            {
                get { return _rotation; }
                set { _rotation = value; }
            }

            // Auxiliary function to move the camera
            public void Move(Vector2 amount)
            {
                _pos += amount;
            }
            // Get set position
            public Vector2 Pos
            {
                get { return _pos; }
                set { _pos = value; }
            }
            public Matrix get_transformation(GraphicsDevice graphicsDevice)
            {
                _transform =       // Thanks to o KB o for this solution
                  Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0)) *
                                             Matrix.CreateRotationZ(Rotation) *
                                             Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                                             Matrix.CreateTranslation(new Vector3(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f, 0));
                return _transform;
            }
        }
        public static bool ColRight = false;
        public static bool ColLeft = false;
        public static bool ColTop = false;
        public static bool ColBot = false;
        public static bool IsDone = false;
        public static float AttackTimer = 0;
        protected override void Update(GameTime gameTime)
        {
            p.StrikeBox.X = (int)p.PlayerX;
            p.StrikeBox.Y = (int)p.PlayerY;
            
            Kb.GetState();
            if(Kb.HasBeenPressed(Keys.A))
            {
                if(!p.IsAttacking && p.MDelayTimer == 0)
                {
                    p.IsAttacking = true;
                }
            }
            if(Kb.HasBeenPressed(Keys.E))
            {
                foreach(Spawn s in SpawnList)
                {
                    if(s.HitBox.Intersects(p.StrikeBox))
                    {
                        if(s.HasQuest(p))
                        {
                            List<Quest> qList = s.GetQuests(p);
                            Console.WriteLine(qList.Count);
                            foreach(Quest q in qList)
                            {
                                Console.WriteLine(q.QuestName);
                            }
                        } else
                        {
                            Console.WriteLine("No Quest");
                        }
                    }
                }
            }
            if(AttackTimer >= 500)
            {
                p.IsAttacking = false;
                AttackTimer = 0;
                p.StrikeBox.Width = 10;
                p.StrikeBox.Height = 50;
                p.MDelayTimer = p.MeleeDelay;
            }
            if(p.MDelayTimer > 0)
            {
                p.MDelayTimer -= 1f;
            }
            Rectangle OldRect = p.Rect;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            p.Rect.Location = new Point((int)p.PlayerX, (int)p.PlayerY - 10);
            foreach (Tile t in BoundList)
            {
                if (t.Rect.Intersects(p.Rect))
                {
                    if(OldRect.Center.X < t.Rect.Center.X)
                    {
                        ColRight = true;
                    } else
                    {
                        ColLeft = true;
                    }
                    if(OldRect.Center.Y < t.Rect.Center.Y)
                    {
                        ColBot = true;
                    } else
                    {
                        ColTop = true;
                    }
                }
            }


            if (Kb.IsPressed(Keys.Right))
            {
                p.Facing = "E";
                if (!ColRight)
                {
                    p.PlayerX += 1.7f;
                } else
                {
                    if (!ColTop && !ColBot)
                    {
                        p.PlayerX -= 1.7f;
                    }
                }

            }
            if (Kb.IsPressed(Keys.Left))
            {
                p.Facing = "W";
                if (!ColLeft)
                {
                    p.PlayerX -= 1.7f;
                }
                if (ColLeft)
                {
                    if (!ColTop && !ColBot)
                    {
                        p.PlayerX += 1.7f;
                    }
                }
            }
            if (Kb.IsPressed(Keys.Up))
            {
                p.Facing = "N";
                if (!ColTop)
                {
                    p.PlayerY -= 1.7f;
                }
                if (ColTop)
                {
                    if(!ColLeft && !ColRight)
                    {
                        p.PlayerY += 1.7f;
                    }
                }
            }
            if (Kb.IsPressed(Keys.Down))
            {
                p.Facing = "S";
                if (!ColBot)
                {
                    p.PlayerY += 1.7f;
                }
                if(ColBot)
                {
                    if(!ColLeft && !ColRight)
                    {
                        p.PlayerY -= 1.7f;
                    }
                }
            }
            if (Kb.HasBeenPressed(Keys.S) && Kb.IsPressed(Keys.LeftControl))
            {
                SaveGame();
            }

            if (CamX < p.PlayerX)
            {
                CamX += 1.3f;
            }
            if (CamX > p.PlayerX)
            {
                CamX -= 1.3f;
            }
            if (CamY < p.PlayerY)
            {
                CamY += 1.3f;
            }
            if (CamY > p.PlayerY)
            {
                CamY -= 1.3f;
            }
            if (p.IsAttacking)
            {
                AttackTimer += 50;
                p.StrikeBox.Width += 10;
                p.StrikeBox.Height += 10;
                switch (p.Facing)
                {
                    case "N":
                        if (!ColTop)
                        {
                            p.PlayerY -= p.MeleeDistance;
                        }
                        break;
                    case "S":
                        if (!ColBot)
                        {
                            p.PlayerY += p.MeleeDistance;
                        }
                        break;
                    case "W":
                        if (!ColLeft)
                        {
                            p.PlayerX -= p.MeleeDistance;
                        }
                        break;
                    case "E":
                        if (!ColRight)
                        {
                            p.PlayerX += p.MeleeDistance;
                        }
                        break;
                }
            }
            ColLeft = false;
            ColRight = false;
            ColBot = false;
            ColTop = false;
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //Clears the graphics device; updates the invisible renderer and player's strike box.
            GraphicsDevice.Clear(Color.Black);
            Camera2d cam = new Camera2d();
            cam.Pos = new Vector2(CamX, CamY);
            p.RenderDim.X = (int)p.PlayerX + ((10 - p.RenderDim.Width) / 2);
            p.RenderDim.Y = (int)p.PlayerY + ((40 - p.RenderDim.Height) / 2);
            p.StrikeBox.X = (int)p.PlayerX + ((10 - p.StrikeBox.Width) / 2);
            p.StrikeBox.Y = (int)p.PlayerY + ((40 - p.StrikeBox.Height) / 2);

            //Begins the spriteBatch.
            spriteBatch.Begin(SpriteSortMode.Immediate,
                        BlendState.AlphaBlend,
                        null,
                        null,
                        null,
                        null,
                        cam.get_transformation(GraphicsDevice));

            //Draws map tiles in the TileList.
            foreach (Tile t in TileList)
            {
                if (t.Rect.Intersects(p.RenderDim))
                {
                    spriteBatch.Draw(t.Tex, new Vector2(t.X, t.Y), new Rectangle(0, 0, 25, 25), Color.White);
                }
            }

            //Draws the WorldObjects in the ObjectList.
            foreach (WorldObject w in ObjectList)
            {
                if (w.HitBox.Intersects(p.RenderDim))
                {
                    spriteBatch.Draw(w.Tex, new Vector2(w.X, w.Y), new Rectangle(0, 0, w.Tex.Width, w.Tex.Height), Color.White);
                }
            }

            //Draws each spawn in the SpawnList.
            foreach (Spawn s in SpawnList)
            {
                if (s.HitBox.Intersects(p.RenderDim) && !s.HitBox.Intersects(p.StrikeBox))
                {
                    spriteBatch.Draw(s.Npc.Tex, new Vector2(s.X, s.Y), new Rectangle(0, 0, s.Npc.Tex.Width, s.Npc.Tex.Height), Color.White);
                }
            }

            //Draws each invisible collider boundary in the BoundList.
            foreach (Tile t in BoundList)
            {
                spriteBatch.Draw(t.Tex, new Vector2(t.X, t.Y), t.Rect, Color.Black);
            }
            spriteBatch.Draw(p.PlayerHead, new Vector2(p.PlayerX, p.PlayerY - 10), new Rectangle(0, 0, p.PlayerHead.Width, p.PlayerBody.Height), Color.White);
            spriteBatch.Draw(p.PlayerBody, new Vector2(p.PlayerX, p.PlayerY), new Rectangle(0, 0, p.PlayerHead.Width, p.PlayerBody.Height), Color.White);
       
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
