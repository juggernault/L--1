using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Yohoho_Gangplank
{
    class Program
    {

        public static Menu Config;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Orbwalking.Orbwalker Orbwalker;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName.ToLower() != "gangplank") return;

            //Spell
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 25000);

            //Config Menu
            Config = new Menu("Gankplank", "Gankplank", true);
            var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            Config.AddSubMenu(TargetSelectorMenu);

            //Orbwalking
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo menu
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRC", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo1", "R if Hits >=").SetValue(new Slider(2, 1, 5)));

            //Harass
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q ").SetValue(true));

            //Misc Menu
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoW", "AutoW Remove CC").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("UseRKs", "Auto R KS").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("UseQKs", "Auto Q KS").SetValue(true));

            //DrawEmenu
            Config.AddSubMenu(new Menu("Draw", "DrawSettings"));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawQ", "Q Range").SetValue(false));

            //Add menu
            Config.AddToMainMenu();
            Game.PrintChat("ShimazakiHaruka : Gangplank");
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Config.Item("DrawQ").GetValue<bool>() && Q.Level > 0) Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Red);
            
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            KS();
            autoW();
            if (Orbwalker.ActiveMode ==  Orbwalking.OrbwalkingMode.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Harass();
            }
            
        }

        static void Combo()
        {
            var Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var useQ = Config.Item("UseQC").GetValue<bool>();
            var useE = Config.Item("UseEC").GetValue<bool>();
            var useR = Config.Item("UseRC").GetValue<bool>();
            var hitR = Config.Item("UseRCombo1").GetValue<Slider>().Value;

            if (Q.IsReady() && Player.Distance(Target) <= Q.Range && useQ)
            {
                Q.CastOnUnit(Target);
            }

            if (R.IsReady() && Player.Distance(Target) <= R.Range && useR)
            {
                R.CastIfWillHit(Target, hitR);
            }
            if (E.IsReady() && useE)
            {
                if (Player.Distance(Target) > Q.Range || Target.CountEnemysInRange(1000) > 2)
                {
                    E.Cast();
                }
            }

        }

        static void Harass()
        {
            var Target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (Q.IsReady() && Player.Distance(Target) <= Q.Range && Config.Item("UseQH").GetValue<bool>())
            {
                Q.CastOnUnit(Target);
            }

        }

        static void autoW(){
            if (!Config.Item("AutoW").GetValue<bool>()) return;

            if(Player.HasBuffOfType(BuffType.Taunt) || Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Snare) ||
                Player.HasBuffOfType(BuffType.Polymorph) || Player.HasBuffOfType(BuffType.Blind) || Player.HasBuffOfType(BuffType.Fear) || Player.HasBuffOfType(BuffType.Silence))
            {
                if (W.IsReady())
                {
                    W.Cast();
                }
            }
            

        }

        static void KS()
        {
            if (!Config.Item("UseRKs").GetValue<bool>() && !Config.Item("UseQKs").GetValue<bool>()) return;

            //Q KS
           foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.Distance(x) < Q.Range && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
           {
               if (target != null || !Config.Item("UseQKs").GetValue<bool>())
                {
                    //Q
                    if (Player.Distance(target.ServerPosition) <= Q.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health+20) 
                    {
                        if (Q.IsReady())
                        {
                            Q.CastOnUnit(target);
                            return;
                        }
                    }


                }
            }
            //End Q
            //R KS
           foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => Player.Distance(x) < R.Range && x.IsValidTarget() && x.IsEnemy && !x.IsDead))
           {
               if (target != null || Config.Item("UseRKs").GetValue<bool>())
               {
                   //R
                   if (Player.Distance(target.ServerPosition) <= R.Range &&
                       (Player.GetSpellDamage(target, SpellSlot.R)) > target.Health + 100) 
                   {
                       if (R.IsReady())
                       {
                           R.Cast(target);
                           return;
                       }
                   }


               }
           }
            //End R
            
        }

    }
}
