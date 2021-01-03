using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace GreyFlare
{
    /*
     * These are all of the associated object classes for the engine.
     */
    public class Player
    {
        public string PlayerName;
        public string Facing; //Which way the player is facing.
        public Texture2D PlayerHead;
        public Texture2D PlayerBody;
        public float PlayerX;
        public float PlayerY;
        public Rectangle Rect; //Player HitBox
        public Rectangle RenderDim; //Rendering box
        public Rectangle StrikeBox; //Melee Strike Box
        public bool IsAttacking = false; // Checks if player is attacking.

        //Player's Skills & Quests
        public Dictionary<Skill, int> PlayerSkills = new Dictionary<Skill, int>(); //Actual skill, item stat modifier
        public List<Quest> QuestLog = new List<Quest>();

        //Player's Inventory & Currency
        public List<GameItem> Inventory = new List<GameItem>();
        public int Currency;

        //Player variables for Melee
        public float MeleeDelay = 15.0f;
        public float MDelayTimer = 0.0f;
        public float MeleeDistance = 1.2f;
        public Player(string name, Texture2D htx, Texture2D btx, float x, float y, int curr)
        {
            PlayerName = name;
            PlayerHead = htx;
            PlayerBody = btx;
            PlayerX = x;
            PlayerY = y;
            Rect = new Rectangle((int)PlayerX, (int)PlayerY - 10, 10, 40);
            RenderDim = new Rectangle((int)PlayerX / 2, (int)PlayerY / 2, 500, 500);
            StrikeBox = new Rectangle((int)PlayerX, (int)PlayerY, 10, 50);
            Facing = "S";
            Currency = curr;
        }
        public void Obtain(string i, int amt)
        {
            GameItem OItem = Game1.ItemList[i];
            if (OItem.Name != Game1.GameCurrency)
            {
                Inventory.Add(Game1.ItemList[i]);
            }
            else
            {
                Currency += amt;
            }
        }
        public void Drop(string i, int amt)
        {
            int a = Inventory.FindAll(x => x.Name == i).Count;
            if (a >= amt)
            {
                foreach (GameItem gi in Inventory.ToArray())
                {
                    if (gi.Name == i)
                    {
                        Inventory.Remove(gi);
                    }
                }
            }
        }
        public bool Spend(int i)
        {
            if (Currency >= i)
            {
                Currency -= i;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public class Diety
    {
        public string DietyName;
        public int Level;
        public List<Diety> Adversaries;
        public List<Diety> Comrades;
        public float X;
        public float Y;
        public Texture2D Sprite;
        public Diety(string n, int l, List<Diety> a, List<Diety> c, float x, float y, Texture2D s)
        {
            DietyName = n;
            Level = l;
            Adversaries = a;
            Comrades = c;
            X = x;
            Y = y;
            Sprite = s;
        }
    }
    public class Skill
    {
        public string SkillName;
        public int SkillLevel;
        public int SkillExperience;
        
        public Skill(string n, int l, int e)
        {
            SkillName = n;
            SkillLevel = l;
            SkillExperience = e;
        }
    }
    public class Quest
    {
        public string QuestName;
        public string QuestText;
        public List<Quest> RequiredQuests;
        public List<Skill> RequiredSkills;
        public List<GameItem> TurnInItems;
        public List<GameItem> Reward;
        public bool IsCompleted;
        public Quest(string n, string qt, List<Quest> q, List<Skill> s, List<GameItem> t, List<GameItem> r, bool ic)
        {
            QuestName = n;
            QuestText = qt;
            RequiredQuests = q;
            RequiredSkills = s;
            TurnInItems = t;
            Reward = r;
            IsCompleted = ic;
        }
    }
    public class GameItem
    {
        public string Name;
        public List<Skill> RequiredSkills;
        public Dictionary<Skill, int> StatModifiers;
        public GameItem(string n, List<Skill> rs, Dictionary<Skill, int> sm)
        {
            Name = n;
            RequiredSkills = rs;
            StatModifiers = sm;
        }
    }

    public class Tile
    {
        public int X;
        public int Y;
        public Rectangle Rect;
        public Texture2D Tex;
        public Tile(int x, int y, Texture2D tx)
        {
            X = x;
            Y = y;
            Tex = tx;
            Rect = new Rectangle(X, Y, tx.Width, tx.Height);
        }
    }
    public class WorldObject
    {
        public int X;
        public int Y;
        public Texture2D Tex;
        public Rectangle HitBox;
        public WorldObject(int x, int y, Texture2D tx)
        {
            X = x;
            Y = y;
            Tex = tx;
            HitBox = new Rectangle(X, Y, Tex.Width, Tex.Height);
        }
    }
    public class Spawn
    {
        public Npc Npc;
        public int X;
        public int Y;
        public List<Quest> AssocQuests;
        public Rectangle HitBox;
        public Spawn(Npc n, int x, int y, List<Quest> aq)
        {
            Npc = n;
            X = x;
            Y = y;
            HitBox = new Rectangle(x, y, Npc.Tex.Width, Npc.Tex.Height);
            AssocQuests = aq;
        }
        public List<Quest> GetQuests(Player p)
        {
            List<Quest> rList = new List<Quest>();
            if (AssocQuests != null && AssocQuests.Count > 0)
            {
                foreach (Quest q in AssocQuests)
                {
                    bool HasSkills = false;
                    bool HasQuestReq = false;
                    if (q.RequiredSkills.Count > 0)
                    {
                        foreach (Skill s in q.RequiredSkills)
                        {
                            if (p.PlayerSkills.ContainsKey(s))
                            {
                                HasSkills = true;
                            }
                        }
                    } else
                    {
                        HasSkills = true;
                    }

                    if (q.RequiredQuests.Count > 0)
                    {
                        foreach (Quest rq in q.RequiredQuests)
                        {
                            if (p.QuestLog.Contains(rq))
                            {
                                if (rq.IsCompleted)
                                {
                                    HasQuestReq = true;
                                }
                            }
                        }
                    } else
                    {
                        HasQuestReq = true;
                    }
                    if (HasSkills && HasQuestReq)
                    {
                        rList.Add(q);
                    }
                }
            }
            return rList;
        }
        public bool HasQuest(Player p)
        {
            if (AssocQuests != null)
            {
                if (AssocQuests.Count > 0)
                {
                    foreach (Quest q in AssocQuests)
                    {
                        bool HasSkills = false;
                        bool HasQuestReq = false;
                        if (q.RequiredSkills.Count > 0)
                        {
                            foreach (Skill s in q.RequiredSkills)
                            {
                                if (p.PlayerSkills.ContainsKey(s))
                                {
                                    HasSkills = true;
                                }
                            }
                        } else
                        {
                            HasSkills = true;
                        }

                        if (q.RequiredQuests.Count > 0)
                        {
                            foreach (Quest rq in q.RequiredQuests)
                            {
                                if (p.QuestLog.Contains(rq))
                                {
                                    if (rq.IsCompleted)
                                    {
                                        HasQuestReq = true;
                                    }
                                }
                            }
                        } else
                        {
                            HasQuestReq = true;
                        }
                        if (HasSkills && HasQuestReq)
                        {
                            return true;
                        }
                    }
                }
                return false;
            } else
            {
                return false;
            }
        }
    }
    public class Npc
    {
        public string NpcName;
        public Texture2D Tex;

        public Npc(string name, Texture2D tx)
        {
            NpcName = name;
            Tex = tx;

        }
    }
}
