using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace GreyFlare
{
    public partial class Game1 : Game
    {
        /*  
         *  This is the LoadContent class for MonoGame.
         * This class does a number of different things.
         * Primarily, it loads the SQLite/Data files, and then organizes that data.
         * It also set's up the graphics display.
         */
        protected override void LoadContent()
        {
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.PreferredBackBufferWidth = 1920;
            this.IsMouseVisible = true;
            graphics.ApplyChanges();
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            baseFont = Content.Load<SpriteFont>("BaseText");

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=greyflare.db"))
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();

                try
                {
                    cmd.CommandText = "SELECT 1 FROM Skills";
                    cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    cmd.CommandText = "CREATE TABLE Skills (id INTEGER PRIMARY KEY AUTOINCREMENT, skillname LONGTEXT, lvl INTEGER, xp INTEGER)";
                    cmd.ExecuteNonQuery();
                }
                try
                {
                    cmd.CommandText = "SELECT 1 FROM Quests";
                    cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    cmd.CommandText = "CREATE TABLE Quests (id INTEGER PRIMARY KEY AUTOINCREMENT, questname LONGTEXT, questtext LONGTEXT, reqquests LONGTEXT, reqskills LONGTEXT, reqitems LONGTEXT, rwd LONGTEXT, iscomp LONGTEXT)";
                    cmd.ExecuteNonQuery();
                }
            }

            string[] filelist = Directory.GetFiles(@"C:\Users\Alex\source\repos\GreyFlare\GreyFlare\Content\PlayerSprites", "*.png");
            foreach (string s in filelist)
            {
                string sz = s.Substring(s.LastIndexOf("\\")).Replace("\\", "");
                string TName = sz.Split('.')[0];
                string TNameB = TName.Replace("_", "");
                SpriteList.Add(TNameB, Content.Load<Texture2D>(@"PlayerSprites\" + TName));
            }
            filelist = Directory.GetFiles(@"C:\Users\Alex\source\repos\FlareEdit\FlareEdit\Content", "*.png");
            foreach (string s in filelist)
            {
                string sz = s.Substring(s.LastIndexOf("\\")).Replace("\\", "");
                string TName = sz.Split('.')[0];
                string TNameB = TName.Replace("_", "");
                TextureList.Add(TNameB, Content.Load<Texture2D>(TName));
            }
            string[] fileobjlist = Directory.GetFiles(@"C:\Users\Alex\source\repos\FlareEdit\FlareEdit\Content\WO", "*.png");
            foreach (string s in fileobjlist)
            {
                string sz = s.Substring(s.LastIndexOf("\\")).Replace("\\", "");
                string TName = sz.Split('.')[0];
                string TNameB = TName.Replace("_", "");
                ObjTextureList.Add(TNameB, Content.Load<Texture2D>(@"WO\" + TName));
            }
            string[] npcfile = Directory.GetFiles(@"C:\Users\Alex\source\repos\GreyFlare\GreyFlare\Content\NPC", "*.png");
            foreach (string s in npcfile)
            {
                string sz = s.Substring(s.LastIndexOf("\\")).Replace("\\", "");
                string TName = sz.Split('.')[0];
                string TNameB = TName.Replace("_", "");
                NpcSpriteList.Add(TNameB, Content.Load<Texture2D>(@"NPC\" + TName));
            }
            LoadQuests();
            LoadMap();
            LoadNPC();
            
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=GreyPlayer.db"))
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                object rn = null;
                try
                {
                    cmd.CommandText = "SELECT * FROM Players";
                    rn = cmd.ExecuteScalar().ToString();
                } catch(Exception ex)
                {

                }
                if (rn != null)
                {
                    cmd.CommandText = "SELECT * FROM Players";
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            Player pA = new Player(rdr[1].ToString(), SpriteList[rdr[2].ToString()], SpriteList[rdr[3].ToString()], float.Parse(rdr[4].ToString()), float.Parse(rdr[5].ToString()), Int32.Parse(rdr[6].ToString()));
                            
                            PlayerList.Add(pA);
                        }
                    }
                }


            }

            if (PlayerList.Count < 1)
            {
                using (SQLiteConnection conn = new SQLiteConnection("Data Source=GreyPlayer.db"))
                {
                    conn.Open();
                    try
                    {
                        SQLiteCommand cmd = conn.CreateCommand();
                        cmd.CommandText = "CREATE TABLE Players (id INTEGER PRIMARY KEY AUTOINCREMENT, name LONGTEXT, head LONGTEXT, body LONGTEXT, x LONGTEXT, y LONGTEXT, currency INTEGER)";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "CREATE TABLE Player_Quests (id INTEGER PRIMARY KEY AUTOINCREMENT, questname LONGTEXT, iscomp LONGTEXT)";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "CREATE TABLE Player_Inv (id INTEGER PRIMARY KEY AUTOINCREMENT, item LONGTEXT)";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "CREATE TABLE Player_Skills (id INTEGER PRIMARY KEY AUTOINCREMENT, skill LONGTEXT, xp LONGTEXT, lvl LONGTEXT)";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "INSERT INTO Players (name, head, body, x, y, currency) VALUES (@n, @h, @b, @x, @y, @c)";
                        cmd.Parameters.AddWithValue("@n", "Test");
                        cmd.Parameters.AddWithValue("@h", "LeatherHead");
                        cmd.Parameters.AddWithValue("@b", "MageBody1");
                        cmd.Parameters.AddWithValue("@x", "0");
                        cmd.Parameters.AddWithValue("@y", "0");
                        cmd.Parameters.AddWithValue("@c", "25");
                        cmd.ExecuteNonQuery();
                        PlayerList.Add(new Player("Test", SpriteList["LeatherHead"], SpriteList["MageBody1"], 10, 10, 25));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            p = PlayerList[0];
            CamX = PlayerList[0].PlayerX;
            CamY = PlayerList[0].PlayerY;

        }
    }
}
