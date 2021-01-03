using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Net;
using System.Data.Entity;
using System.Diagnostics;
using System.Data;
using System.Data.SQLite;
using System.Data.SQLite.EF6;
using MonoGame;
using Microsoft.Win32;
using System.IO;

namespace GreyFlare
{
    /*
     * Everything within the data handler is meant to process save/game data.
     * It's functions primarily exist to read and sort, or sort and save.
     */
    public partial class Game1 : Game
    {
        public static void LoadNPC()
        {
            NpcList.Clear();
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=npc.db"))
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM npc";
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {

                    while (rdr.Read())
                    {
                        if (rdr.HasRows)
                        {
                            string Name = rdr[1].ToString();
                            Texture2D tx = NpcSpriteList[rdr[2].ToString()];
                            NpcList.Add(new Npc(Name, tx));
                        }
                    }

                }
                cmd.CommandText = "SELECT * FROM spawn";
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while(rdr.Read())
                    {
                        if(rdr.HasRows)
                        {
                            string[] spltString = new string[] { "<>" };
                            List<Quest> AQuest = new List<Quest>();
                            if (rdr[4].ToString() != string.Empty)
                            {
                                string[] QuestString = rdr[4].ToString().Split(spltString, StringSplitOptions.RemoveEmptyEntries);
                                foreach(string sb in QuestString)
                                {
                                    AQuest.Add(QuestList.Find(x => x.QuestName == sb));
                                }
                            }
                            Npc n = NpcList.Find(x => x.NpcName == rdr[1].ToString());
                            Spawn s = new Spawn(n, Int32.Parse(rdr[2].ToString()), Int32.Parse(rdr[3].ToString()), AQuest);
                            SpawnList.Add(s);
                        }
                    }
                }
            }
        }
        public static void SaveGame()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=GreyPlayer.db"))
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Players SET head=@h, body=@b, x=@x, y=@y WHERE name=@n";
                cmd.Parameters.AddWithValue("@n", p.PlayerName);
                cmd.Parameters.AddWithValue("@h", p.PlayerHead.Name.Split('\\')[1]);
                cmd.Parameters.AddWithValue("@b", p.PlayerBody.Name.Split('\\')[1]);
                cmd.Parameters.AddWithValue("@x", p.PlayerX);
                cmd.Parameters.AddWithValue("@y", p.PlayerY);
                cmd.ExecuteNonQuery();

            }
        }
        public static void LoadQuests()
        {
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=greyflare.db"))
            {
                conn.Open();
                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Quests";
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        string name = rdr[1].ToString();
                        string desc = rdr[2].ToString();

                        string rqstring = rdr[3].ToString();
                        List<Quest> rqList = new List<Quest>();
                        string[] splstring = new string[] { "<>" };
                        if (rqstring != string.Empty)
                        {
                            string[] split = rqstring.Split(splstring, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in split)
                            {
                                rqList.Add(QuestList.Find(x => x.QuestName == s));
                            }
                        }

                        string rsstring = rdr[4].ToString();
                        List<Skill> rsList = new List<Skill>();
                        if (rsstring != string.Empty)
                        {
                            string[] split = rsstring.Split(splstring, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in split)
                            {
                                rsList.Add(SkillsList.Find(x => x.SkillName == s));
                            }
                        }

                        string ristring = rdr[5].ToString();
                        List<GameItem> riList = new List<GameItem>();
                        if (ristring != string.Empty)
                        {
                            string[] split = ristring.Split(splstring, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in split)
                            {
                                riList.Add(ItemList[s]);
                            }
                        }

                        string rwstring = rdr[6].ToString();
                        List<GameItem> rwList = new List<GameItem>();
                        if (rwstring != string.Empty)
                        {
                            string[] split = rwstring.Split(splstring, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in split)
                            {
                                rwList.Add(ItemList[s]);
                            }
                        }
                        bool iscomp = false;
                        if (rdr[7].ToString() == "true")
                        {
                            iscomp = true;
                        }
                        QuestList.Add(new Quest(name, desc, rqList, rsList, riList, rwList, iscomp));
                    }
                }
            }
        }
        public static void LoadMap()
        {

            TileList.Clear();
            //ObjectList.Clear();
            string FullFile = File.ReadAllText(@"C:\Users\Alex\Documents\TestMap.fmf");
            string[] TileData = FullFile.Substring(FullFile.IndexOf("<tile>"), FullFile.IndexOf("</tile>")).Replace("<tile>", "").Replace("</tile>", "").Split(';');
            foreach (string s in TileData)
            {
                if (s != "")
                {
                    string[] TileSplit = s.Split(':');
                    int X = Int32.Parse(TileSplit[0]);
                    int Y = Int32.Parse(TileSplit[1]);
                    Texture2D tx = TextureList[TileSplit[2].Replace("_", "")];
                    TileList.Add(new Tile(X, Y, tx));
                }
            }
            int FFA = FullFile.IndexOf("<obj>");
            int FFB = FullFile.IndexOf("</obj>");
            string[] ObjData = FullFile.Split(new string[] { "<obj>" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("</obj>", "").Split(';');
            foreach (string s in ObjData)
            {
                if (s != "")
                {
                    string[] TileSplit = s.Split(':');
                    int X = Int32.Parse(TileSplit[0]);
                    int Y = Int32.Parse(TileSplit[1]);
                    Texture2D tx = ObjTextureList[TileSplit[2].Replace("_", "").Split('\\')[1]];
                    ObjectList.Add(new WorldObject(X, Y, tx));
                }
            }
            try
            {
                string[] BndData = FullFile.Split(new string[] { "<bound>" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("</bound>", "").Split(';');
                foreach (string s in BndData)
                {
                    if (s != "")
                    {
                        string[] TileSplit = s.Split(':');
                        int X = Int32.Parse(TileSplit[0]);
                        int Y = Int32.Parse(TileSplit[1]);
                        Texture2D tx = TextureList["bound"];
                        BoundList.Add(new Tile(X, Y, tx));
                    }
                }
            } catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
