using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Nocturne
{
    class Program
    {
        private static Menu Config;
        private static Orbwalking.Orbwalker Orbwalker;
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        


        //Credit to diabaths
        private static Items.Item _tiamat, _hydra, _blade, _bilge, _rand;
        private static SpellSlot _smiteSlot = SpellSlot.Unknown;
        private static Spell _smite;
        private static SpellSlot _igniteSlot;

        //Credits to Kurisu
        private static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        private static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        private static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        private static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Nocturne") return;

            Q = new Spell(SpellSlot.Q, 1200f);
            W = new Spell(SpellSlot.W, 0f);
            E = new Spell(SpellSlot.E, 425f);
            R = new Spell(SpellSlot.R, 25000f);

            Q.SetSkillshot(0.25f, 60f, 1600f, false, SkillshotType.SkillshotLine);
            E.SetTargetted(0.5f, 1700f);
            R.SetTargetted(0.75f, 2500f);

            _igniteSlot = Player.GetSpellSlot("SummonerDot");

            SetSmiteSlot();

            _bilge = new Items.Item(3144, 450f);
            _blade = new Items.Item(3153, 450f);
            _hydra = new Items.Item(3074, 250f);
            _tiamat = new Items.Item(3077, 250f);
            _rand = new Items.Item(3143, 490f);

            //Menu
            Config = new Menu("Nocturne", "Nocturne", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("smitecombo", "Use Smite in target")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("Paranoia", "Paranoia to Target in Range!").SetValue(new KeyBind("T".ToCharArray()[0],KeyBindType.Press)));

            //Harass
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEH", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("Hmana", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));

            //Laneclear & Jungle
            Config.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("useQHit", "Q Min hit").SetValue(new Slider(3, 6, 1)));

            Config.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJ", "Use Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJ", "Use W").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJ", "Use E").SetValue(true));



            //Items Credit to diabaths
            Config.AddSubMenu(new Menu("items", "items"));
            Config.SubMenu("items").AddSubMenu(new Menu("Offensive", "Offensive"));
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Tiamat", "Use Tiamat")).SetValue(true);
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Hydra", "Use Hydra")).SetValue(true);
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Bilge", "Use Bilge")).SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BilgeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Bilgemyhp", "Or your Hp < ").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items").SubMenu("Offensive").AddItem(new MenuItem("Blade", "Use Blade")).SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("BladeEnemyhp", "If Enemy Hp <").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Offensive")
                .AddItem(new MenuItem("Blademyhp", "Or Your  Hp <").SetValue(new Slider(85, 1, 100)));
            Config.SubMenu("items").AddSubMenu(new Menu("Deffensive", "Deffensive"));
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omen", "Use Randuin Omen"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Deffensive")
                .AddItem(new MenuItem("Omenenemys", "Randuin if enemys>").SetValue(new Slider(2, 1, 5)));     
            Config.SubMenu("items").AddSubMenu(new Menu("Potions", "Potions"));
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usehppotions", "Use Healt potion/Flask/Biscuit"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionhp", "If Health % <").SetValue(new Slider(35, 1, 100)));
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usemppotions", "Use Mana potion/Flask/Biscuit"))
                .SetValue(true);
            Config.SubMenu("items")
                .SubMenu("Potions")
                .AddItem(new MenuItem("usepotionmp", "If Mana % <").SetValue(new Slider(35, 1, 100)));

            //Smite ActiveJungle Credit to diabaths
            Config.AddSubMenu(new Menu("Smite", "Smite"));
            Config.SubMenu("Smite")
                .AddItem(
                    new MenuItem("Usesmite", "Use Smite(toggle)").SetValue(new KeyBind("H".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Config.SubMenu("Smite").AddItem(new MenuItem("Useblue", "Smite Blue Early ")).SetValue(true);
            Config.SubMenu("Smite")
                .AddItem(new MenuItem("manaJ", "Smite Blue Early if MP% <").SetValue(new Slider(35, 1, 100)));
            Config.SubMenu("Smite").AddItem(new MenuItem("Usered", "Smite Red Early ")).SetValue(true);
            Config.SubMenu("Smite")
                .AddItem(new MenuItem("healthJ", "Smite Red Early if HP% <").SetValue(new Slider(35, 1, 100)));

            //Misc
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoI", "Auto Ignite KS").SetValue(true));

            //Drawings
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "Draw R")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawRM", "Draw R Minimap")).SetValue(true);

            Config.AddToMainMenu();
            Game.PrintChat("<font color='#FF10FF'>NoobTurne by Shimazaki Haruka</font> Loaded.");
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
           Drawing.OnEndScene += Drawing_OnEndScene;

        }


        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (Player.IsDead) return;

            if (R.Level == 0) return;
            if (Config.Item("DrawRM").GetValue<bool>() && R.Level > 0)
            {
                Utility.DrawCircle(Player.Position, GetRRange(), System.Drawing.Color.Aqua, 1, 30, true);
            }
        }
        
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (Config.Item("DrawQ").GetValue<bool>() && Q.Level > 0) Utility.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Red);
            if (Config.Item("DrawE").GetValue<bool>() && E.Level > 0) Utility.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Green);
            if (Config.Item("DrawR").GetValue<bool>() && R.Level > 0) Utility.DrawCircle(Player.Position, GetRRange(), System.Drawing.Color.Yellow);
            
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            if (Config.Item("Paranoia").GetValue<KeyBind>().Active)
            {
                Paranoia();
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                   Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Laneclear();
                    JungleClear();
                    break;
            }


            if (Config.Item("Usesmite").GetValue<KeyBind>().Active)
            {
                Smiteuse();
            }
            Usepotion();

        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            
            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();
            Smiteontarget();
            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
            if (E.IsReady() && useE && target.IsValidTarget(E.Range))
            {
                E.CastOnUnit(target);
            }
            if (W.IsReady() && useW && Orbwalking.InAutoAttackRange(target))
            {
                W.Cast();
            }


            UseItemes();
        }

        private static void Paranoia()
        {
            var target = TargetSelector.GetTarget(GetRRange(), TargetSelector.DamageType.Physical);
            if (R.IsReady() && target.IsValidTarget(GetRRange()))
            {
                R.Cast();
                R.CastOnUnit(target);
            }
        }

        private static void Smiteontarget()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var usesmite = Config.Item("smitecombo").GetValue<bool>();
                var itemscheck = SmiteBlue.Any(i => Items.HasItem(i)) || SmiteRed.Any(i => Items.HasItem(i));
                if (itemscheck && usesmite &&
                    ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready &&
                    hero.IsValidTarget(_smite.Range))
                {
                    ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var useQ = Config.Item("UseQH").GetValue<bool>();
            var useE = Config.Item("UseEH").GetValue<bool>();

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
            if (E.IsReady() && useE && target.IsValidTarget(E.Range))
            {
                E.CastOnUnit(target);
            }
        }

        private static void Laneclear()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All);
            var useQ = Config.Item("UseQFarm").GetValue<bool>();
            var Qhit = Config.Item("useQHit").GetValue<Slider>().Value;

            if (Q.IsReady() && useQ)
            {
                var getQhit = Q.GetLineFarmLocation(allMinionsQ, Q.Width);

                if (getQhit.MinionsHit >= Qhit)
                {
                    Q.Cast(getQhit.Position);
                }               
            }
        }

        private static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var useQ = Config.Item("UseQJ").GetValue<bool>();
            var useW = Config.Item("UseWJ").GetValue<bool>();
            var useE = Config.Item("UseEJ").GetValue<bool>();

            if (mobs.Count > 0)
            {
                var mob = mobs[0];

                if (useQ && Q.IsReady() && Player.Distance(mob) < Q.Range)
                    {
                        Q.Cast(mob);
                    }

                if (E.IsReady() && useE && Player.Distance(mob) < E.Range)
                    {
                        E.CastOnUnit(mob);
                    }

                if (W.IsReady() && useW)
                {
                    W.Cast();
                }
                
            }
        }

        //New map Monsters Name By SKO
        private static void Smiteuse()
        {
            var jungle = false;
            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear){
                jungle = true;
            }

            if (ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) != SpellState.Ready) return;
            var useblue = Config.Item("Useblue").GetValue<bool>();
            var usered = Config.Item("Usered").GetValue<bool>();
            var health = (100 * (Player.Mana / Player.MaxMana)) < Config.Item("healthJ").GetValue<Slider>().Value;
            var mana = (100 * (Player.Mana / Player.MaxMana)) < Config.Item("manaJ").GetValue<Slider>().Value;
            string[] jungleMinions;
            if (Utility.Map.GetMap()._MapType.Equals(Utility.Map.MapType.TwistedTreeline))
            {
                jungleMinions = new string[] { "TT_Spiderboss", "TT_NWraith", "TT_NGolem", "TT_NWolf" };
            }
            else
            {
                jungleMinions = new string[]
                {
                    "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon",
                    "SRU_Baron", "Sru_Crab"
                };
            }
            var minions = MinionManager.GetMinions(Player.Position, 1000, MinionTypes.All, MinionTeam.Neutral);
            if (minions.Count() > 0)
            {
                int smiteDmg = GetSmiteDmg();

                foreach (Obj_AI_Base minion in minions)
                {
                    if (Utility.Map.GetMap()._MapType.Equals(Utility.Map.MapType.TwistedTreeline) &&
                        minion.Health <= smiteDmg &&
                        jungleMinions.Any(name => minion.Name.Substring(0, minion.Name.Length - 5).Equals(name)))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    if (minion.Health <= smiteDmg && jungleMinions.Any(name => minion.Name.StartsWith(name)) &&
                        !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && useblue && mana && minion.Health >= smiteDmg &&
                             jungleMinions.Any(name => minion.Name.StartsWith("SRU_Blue")) &&
                             !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && usered && health && minion.Health >= smiteDmg &&
                             jungleMinions.Any(name => minion.Name.StartsWith("SRU_Red")) &&
                             !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                }
            }
        }

        //Credits to metaphorce
        private static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, Smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {
                _smiteSlot = spell.Slot;
                _smite = new Spell(_smiteSlot, 700);
                return;
            }
        }

        //Credits to Kurisu
        private static string Smitetype()
        {
            if (SmiteBlue.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmitePlayerganker";
            }
            if (SmiteRed.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(i => Items.HasItem(i)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(i => Items.HasItem(i)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }

        //Credit to diabaths
        private static void UseItemes()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var iBilge = Config.Item("Bilge").GetValue<bool>();
                var iBilgeEnemyhp = hero.Health <=
                                    (hero.MaxHealth * (Config.Item("BilgeEnemyhp").GetValue<Slider>().Value) / 100);
                var iBilgemyhp = Player.Health <=
                                 (Player.MaxHealth * (Config.Item("Bilgemyhp").GetValue<Slider>().Value) / 100);
                var iBlade = Config.Item("Blade").GetValue<bool>();
                var iBladeEnemyhp = hero.Health <=
                                    (hero.MaxHealth * (Config.Item("BladeEnemyhp").GetValue<Slider>().Value) / 100);
                var iBlademyhp = Player.Health <=
                                 (Player.MaxHealth * (Config.Item("Blademyhp").GetValue<Slider>().Value) / 100);
                var iOmen = Config.Item("Omen").GetValue<bool>();
                var iOmenenemys = hero.CountEnemysInRange(450) >= Config.Item("Omenenemys").GetValue<Slider>().Value;
                var iTiamat = Config.Item("Tiamat").GetValue<bool>();
                var iHydra = Config.Item("Hydra").GetValue<bool>();


                if (hero.IsValidTarget(450) && iBilge && (iBilgeEnemyhp || iBilgemyhp) && _bilge.IsReady())
                {
                    _bilge.Cast(hero);

                }
                if (hero.IsValidTarget(450) && iBlade && (iBladeEnemyhp || iBlademyhp) && _blade.IsReady())
                {
                    _blade.Cast(hero);

                }
                if (iTiamat && _tiamat.IsReady() && hero.IsValidTarget(_tiamat.Range))
                {
                    _tiamat.Cast();

                }
                if (iHydra && _hydra.IsReady() && hero.IsValidTarget(_hydra.Range))
                {
                    _hydra.Cast();

                }
                if (iOmenenemys && iOmen && _rand.IsReady())
                {
                    _rand.Cast();

                }
            }         
        }

        //Credit to diabaths
        private static int GetSmiteDmg()
        {
            int level = Player.Level;
            int index = Player.Level / 5;
            float[] dmgs = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };
            return (int)dmgs[index];
        }


        //Credit to diabaths
        private static void Usepotion()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var iusehppotion = Config.Item("usehppotions").GetValue<bool>();
            var iusepotionhp = Player.Health <=
                               (Player.MaxHealth * (Config.Item("usepotionhp").GetValue<Slider>().Value) / 100);
            var iusemppotion = Config.Item("usemppotions").GetValue<bool>();
            var iusepotionmp = Player.Mana <=
                               (Player.MaxMana * (Config.Item("usepotionmp").GetValue<Slider>().Value) / 100);
            if (Utility.InFountain() || ObjectManager.Player.HasBuff("Recall")) return;

            if (Utility.CountEnemysInRange(800) > 0 ||
                (mobs.Count > 0 && Config.Item("ActiveJungle").GetValue<KeyBind>().Active && (Items.HasItem(1039) ||
                  SmiteRed.Any(i => Items.HasItem(i)) || SmitePurple.Any(i => Items.HasItem(i)) ||
                  SmiteBlue.Any(i => Items.HasItem(i)) || SmiteGrey.Any(i => Items.HasItem(i))
                     )))
            {
                if (iusepotionhp && iusehppotion &&
                     !(ObjectManager.Player.HasBuff("RegenerationPotion", true) ||
                       ObjectManager.Player.HasBuff("ItemCrystalFlask", true) ||
                       ObjectManager.Player.HasBuff("ItemMiniRegenPotion", true)))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }
                    else if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }
                    else if (Items.HasItem(2003) && Items.CanUseItem(2003))
                    {
                        Items.UseItem(2003);
                    }
                }


                if (iusepotionmp && iusemppotion &&
                    !(ObjectManager.Player.HasBuff("FlaskOfCrystalWater", true) ||
                      ObjectManager.Player.HasBuff("ItemCrystalFlask", true) ||
                      ObjectManager.Player.HasBuff("ItemMiniRegenPotion", true)))
                {
                    if (Items.HasItem(2041) && Items.CanUseItem(2041))
                    {
                        Items.UseItem(2041);
                    }
                    else if (Items.HasItem(2010) && Items.CanUseItem(2010))
                    {
                        Items.UseItem(2010);
                    }
                    else if (Items.HasItem(2004) && Items.CanUseItem(2004))
                    {
                        Items.UseItem(2004);
                    }
                }
            }
        }

        private static void KillSteal()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                var igniteDmg = Player.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
                if (hero.IsValidTarget(600) && Config.Item("UseIgnitekill").GetValue<bool>() && _igniteSlot != SpellSlot.Unknown &&
                    Player.Spellbook.CanUseSpell(_igniteSlot) == SpellState.Ready)
                {
                    if (igniteDmg > hero.Health)
                    {
                        Player.Spellbook.CastSpell(_igniteSlot, hero);
                    }
                }

            }
        }

        private static float GetRRange()
        {
            return 1250 + (750 * R.Level);
        }
    }
}
