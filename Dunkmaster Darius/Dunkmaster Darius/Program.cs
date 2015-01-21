using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Dunkmaster_Darius
{
    class Program
    {

        public static Menu Config;
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Obj_AI_Hero Player = ObjectManager.Player;    
        public static SpellSlot igniteSlot;
        public static Items.Item tiamat, hydra;


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Darius") return;

            //Spell
            Q = new Spell(SpellSlot.Q, 425);
            W = new Spell(SpellSlot.W, 145);
            E = new Spell(SpellSlot.E, 550);
            R = new Spell(SpellSlot.R, 460);

            igniteSlot = Player.GetSpellSlot("SummonerDot");


            //item from
            hydra = new Items.Item(3074, 375f);
            tiamat = new Items.Item(3077, 375f);

            //Config Menu
            Config = new Menu("Dunkmaster", "Dunkmaster",true);

            //Lxorbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "LX_Orbwalker");
            xSLxOrbwalker.AddToMenu(orbwalkerMenu);
            Config.AddSubMenu(orbwalkerMenu);
            
            //Add the targer selector to the menu.
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Combo menu
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQC", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWC", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseEC", "Use E").SetValue(true));
            //Config.SubMenu("Combo").AddItem(new MenuItem("UseRC", "Use R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("MinE", "Min. Range Use E").SetValue(new Slider(100, 550, 0)));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItemC", "Use Item Combo").SetValue(true));

            //Harass
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q ").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEH", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Hmana", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("Harass").AddItem(new MenuItem("AutoQ", "Auto Q Harass MaxDmg Toggle").SetValue(new KeyBind("Y".ToCharArray()[0],
        KeyBindType.Toggle)));


            //LaneClear and Jungle Menu
            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQL", "Use Q").SetValue(false));
            //Config.SubMenu("LaneClear").AddItem(new MenuItem("UseWL", "Use W").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseEL", "Use E").SetValue(false));

            //Misc Menu
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("packet", "Use Packets (NFE)").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "Auto Interrupt").SetValue(true));

            //KS Menu
            Config.AddSubMenu(new Menu("Kill Steals", "KS"));
            Config.SubMenu("KS").AddItem(new MenuItem("UseRKs", "Auto R KS").SetValue(true));
            Config.SubMenu("KS").AddItem(new MenuItem("UseQKs", "Auto Q KS").SetValue(true));
            Config.SubMenu("KS").AddItem(new MenuItem("IgKs", "Use Ignite KS").SetValue(true));

            //DrawEmenu
            Config.AddSubMenu(new Menu("Draw", "DrawSettings"));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawQ", "Q Range").SetValue(false));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawW", "W Range").SetValue(false));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawEn", "Min E Range").SetValue(false));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawEx", "Max E Range").SetValue(false));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawR", "R Range").SetValue(false));
            Config.SubMenu("DrawSettings").AddItem(new MenuItem("DrawK", "Draw R Killable").SetValue(false));
            
            //add menu
            Config.AddToMainMenu();

            Game.PrintChat("AlienHack : Dunkmaster Darius");
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            xSLxOrbwalker.AfterAttack += AfterAttack;

        }

        static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {

            if (!unit.IsMe)
                return;

                if (Config.Item("UseWC").GetValue<bool>() && W.IsReady() && xSLxOrbwalker.InAutoAttackRange(target) && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo)
                {
                    W.Cast();
                    xSLxOrbwalker.ResetAutoAttackTimer();
                }
                if (Config.Item("UseItemC").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo)
                {
                    if (Utility.CountEnemysInRange(Player ,350) >= 1 && tiamat.IsReady() && !W.IsReady())
                    {
                        tiamat.Cast();
                        xSLxOrbwalker.ResetAutoAttackTimer();
                    }
                    if (Utility.CountEnemysInRange(Player, 350) >= 1 && hydra.IsReady() && !W.IsReady() )
                    {
                        hydra.Cast();
                        xSLxOrbwalker.ResetAutoAttackTimer();
                    }
                }

                return;
            
        }

        static void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("Interrupt").GetValue<bool>()) return;
            var target = unit;
            if (E.IsReady() && Player.Distance(target.Position) < R.Range)
            {
                E.Cast(unit, Packets());
            }
          
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            var minE = Config.Item("MinE").GetValue<Slider>().Value;
            if (Config.Item("DrawQ").GetValue<bool>() && Q.Level > 0) Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Red);
            if (Config.Item("DrawW").GetValue<bool>() && W.Level > 0) Utility.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Cyan);
            if (Config.Item("DrawEx").GetValue<bool>() && E.Level > 0) Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Green);
            if (Config.Item("DrawEn").GetValue<bool>() && E.Level > 0) Utility.DrawCircle(Player.Position, minE, System.Drawing.Color.Green);
            if (Config.Item("DrawR").GetValue<bool>() && R.Level > 0) Utility.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Yellow);
            if (Config.Item("DrawK").GetValue<bool>() && R.Level > 0)
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>())
                {
                   foreach (var buff in target.Buffs)
                    {
                        if (buff.Name == "dariushemo")
                        {
                            if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.R, 1) *
                                (1 + buff.Count / 5) - 1 > target.Health && !target.IsDead && target.IsEnemy)
                            {
                                Drawing.DrawText(target.HPBarPosition.X + 50, target.HPBarPosition.Y + 200, System.Drawing.Color.Red, "R Killable");
                            }
                        }
                        else if (Player.GetSpellDamage(target, SpellSlot.R) > target.Health && !target.IsDead && target.IsEnemy)
                        {
                            Drawing.DrawText(target.HPBarPosition.X + 50, target.HPBarPosition.Y + 200, System.Drawing.Color.Red, "R Killable");
                        }
                        
                    }

              
                }
            }
        }

        static void Game_OnGameUpdate(EventArgs args)
        {

            if (Player.IsDead) return;
            switch (xSLxOrbwalker.CurrentMode)
            {
                case xSLxOrbwalker.Mode.Combo:
                    Combo();
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    Harrass();
                    break;
                case xSLxOrbwalker.Mode.LaneClear:
                    LaneClear();
                    break;
            }

            if (Config.Item("AutoQ").GetValue<KeyBind>().Active)
            {
                var existsMana = ObjectManager.Player.MaxMana / 100 * Config.Item("Hmana").GetValue<Slider>().Value;
                if (Player.Mana >= existsMana)
                {
                    AutoQ();
                }
            }

            ExecuteKillsteal();

        }


        static void ExecuteKillsteal()
        {
            if (!Config.Item("UseRKs").GetValue<bool>() && !Config.Item("UseQKs").GetValue<bool>() && !Config.Item("IgKs").GetValue<bool>() ) return;

            foreach (var target in ObjectManager.Get<Obj_AI_Hero>())
            {

                if (Config.Item("UseQKs").GetValue<bool>() && !target.IsDead && Q.IsReady() && Player.Distance(target.Position) < Q.Range && Player.GetSpellDamage(target, SpellSlot.Q) > (target.Health+20))
                {
                    //Game.PrintChat("q5");
                    Q.Cast();
                }


                if (Config.Item("UseRKs").GetValue<bool>() && R.IsReady() && !target.IsDead && Player.Distance(target.Position) < R.Range )
                {
                    CastR(target);
                }

                if (igniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(igniteSlot)  == SpellState.Ready && ObjectManager.Player.Distance(target.Position) < 600 && Config.Item("IgKs").GetValue<bool>())
                {
                    if (ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(igniteSlot, target);
                    }
                }
            }
        }

        static void AutoQ()
        {
            if (!Q.IsReady()) return;

            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where Player.Distance(champ.ServerPosition) <= Q.Range && champ.IsEnemy select champ).ToList();


            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            //foreach (var target in nearChamps)
            //{

                //ignite
                if (Player.Distance(target) > 270 && Player.Distance(target) < Q.Range-10 && !target.IsDead && target.IsEnemy && target.IsValid)
                {
                    //Game.PrintChat("q1");
                    Q.Cast();
                }
           // }

           
          
        } 

       


        // R Calculate Credit TC-Crew
        static void CastR(Obj_AI_Base target)
        {
            if (!target.IsValidTarget(R.Range) || !R.IsReady()) return;

            if (!(ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q, 1) > target.Health ))
            {
                foreach (var buff in target.Buffs)
                {
                    if (buff.Name == "dariushemo")
                    {
                        if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.R, 1) *
                            (1 + buff.Count / 5) - 1 > (target.Health))
                        {
                            R.CastOnUnit(target, true);
                        }
                    }
                }
            }
            else if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.R, 1) - 15 >
                     (target.Health))
            {
                R.CastOnUnit(target, true);
            }
        }

        static void Combo()
        {
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            var minE = Config.Item("MinE").GetValue<Slider>().Value;

            if (Config.Item("UseEC").GetValue<bool>() && E.IsReady() && Player.Distance(target.Position) <= E.Range && Player.Distance(target.Position) >= minE )
            {
                E.Cast(target, Packets());
            }
            if (Config.Item("UseQC").GetValue<bool>() && Q.IsReady() && Player.Distance(target.Position) <= Q.Range)
            {
                //Game.PrintChat("q2");
                Q.Cast();
            }
         /*   if (Config.Item("UseRC").GetValue<bool>() && R.IsReady() && Player.Distance(target) <= R.Range)
            {
                CastR(target);
            }*/

        }

        static void Harrass()
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            var existsMana = ObjectManager.Player.MaxMana / 100 * Config.Item("Hmana").GetValue<Slider>().Value;


            if (Player.Mana >= existsMana)
            {
                if (Config.Item("UseQH").GetValue<bool>() && Q.IsReady() && Player.Distance(target.Position) <= Q.Range)
                {
                    //Game.PrintChat("q3");
                    Q.Cast();
                }
                if (Config.Item("UseEH").GetValue<bool>() && E.IsReady() && Player.Distance(target.Position) <= E.Range)
                {
                    E.Cast(target,Packets());
                }
            }


        }

        static void LaneClear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);

            if (minion.Count > 0)
            {
                var minions = minion[0];
                if (Config.Item("UseQL").GetValue<bool>() && Q.IsReady() && minions.IsValidTarget(Q.Range))
                {
                    //Game.PrintChat("q4");
                    Q.Cast();
                }
                if (Config.Item("UseEL").GetValue<bool>() && E.IsReady() && minions.IsValidTarget(E.Range))
                {
                    E.Cast(minions, Packets());
                }
            }

        }

        public static bool Packets()
        {
            return Config.Item("packet").GetValue<bool>();
        }

        

    }
}
