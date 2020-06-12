using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using ModCommon.Util;
using UnityEngine;

namespace CagneyCarnation
{
    public class Flower : MonoBehaviour
    {
        private const int AttHP = 5000;
        private const int AttP2 = 2500;
        private const int AscHP = 6000;
        private const int AscP2 = 3000;

        private PlayMakerFSM _control;
        private EnemyDeathEffectsUninfected _de;
        private EnemyHitEffectsUninfected _he;
        private HealthManager _hm;
        private NonBouncer _nb;
        private SpriteFlash _sf;
        private GameObject _hornet;

        private void Awake()
        {
            gameObject.name = "Cagney Carnation";
            
            _control = gameObject.LocateMyFSM("Control");

            _hornet = CagneyCarnation.GameObjects["Hornet"];

            _de = gameObject.AddComponent<EnemyDeathEffectsUninfected>();
            _hm = gameObject.AddComponent<HealthManager>();
            _sf = gameObject.AddComponent<SpriteFlash>();
            _he = gameObject.AddComponent<EnemyHitEffectsUninfected>();
            _nb = GetComponent<NonBouncer>();

            AssignFields();

            On.HealthManager.TakeDamage += OnTakeDamage;
        }

        private IEnumerator Start()
        {
            yield return new WaitWhile(() => HeroController.instance == null);

            foreach (tk2dSprite sprite in GetComponentsInChildren<tk2dSprite>(true))
            {
                foreach (tk2dSpriteDefinition spriteDef in sprite.Collection.spriteDefinitions)
                {
                    spriteDef.material.shader = Shader.Find("Sprites/Default-ColorFlash");
                }
            }

            GameObject hc = HeroController.instance.gameObject;
            PlayMakerFSM spellControl = hc.LocateMyFSM("Spell Control");
            GameObject fireballParent = spellControl.GetAction<SpawnObjectFromGlobalPool>("Fireball 2", 3).gameObject.Value;
            PlayMakerFSM fireballCast = fireballParent.LocateMyFSM("Fireball Cast");
            GameObject audioPlayer = fireballCast.GetAction<AudioPlayerOneShotSingle>("Cast Right", 3).audioPlayer.Value;
            _control.Fsm.GetFsmGameObject("Audio Player").Value = audioPlayer;
            _control.Fsm.GetFsmInt("Boss Level").Value = ArenaFinder.BossLevel;
            
            GameObject stunEffect = _hornet.LocateMyFSM("Control").GetAction<SpawnObjectFromGlobalPool>("Stun Start", 3).gameObject.Value;
            _control.Fsm.GetFsmGameObject("Stun Effect").Value = stunEffect;
            
            GameObject platVines = gameObject.FindGameObjectInChildren("Plat Vines");
            platVines.FindGameObjectInChildren("Platform A").LocateMyFSM("FSM").Fsm.GetFsmGameObject("Audio Player").Value = audioPlayer;
            platVines.FindGameObjectInChildren("Platform B").LocateMyFSM("FSM").Fsm.GetFsmGameObject("Audio Player").Value = audioPlayer;
            platVines.FindGameObjectInChildren("Platform C").LocateMyFSM("FSM").Fsm.GetFsmGameObject("Audio Player").Value = audioPlayer;
            
            GameObject magicProjectiles = gameObject.FindGameObjectInChildren("Magic Projectiles");
            magicProjectiles.FindGameObjectInChildren("Boomerang").LocateMyFSM("Movement").Fsm.GetFsmGameObject("Audio Player").Value = audioPlayer;
            magicProjectiles.FindGameObjectInChildren("Acorn").LocateMyFSM("Control").Fsm.GetFsmGameObject("Hero").Value = hc;
            
            GameObject whitePollen = gameObject.FindGameObjectInChildren("White Pollen");
            whitePollen.AddComponent<Pollen>();

            GameObject seeds = gameObject.FindGameObjectInChildren("Seeds");
            GameObject blueSeed = seeds.FindGameObjectInChildren("Blue Seed");
            blueSeed.LocateMyFSM("Control").Fsm.GetFsmFloat("Platforms Y").Value = 25.0f;
            blueSeed.FindGameObjectInChildren("Mini Flower Vine").LocateMyFSM("Control").Fsm.GetFsmGameObject("Audio Player").Value = audioPlayer;
            blueSeed.FindGameObjectInChildren("Chomper Vine").LocateMyFSM("Control").Fsm.GetFsmGameObject("Audio Player").Value = audioPlayer;
            GameObject miniFlower = blueSeed.FindGameObjectInChildren("Mini Flower");
            miniFlower.AddComponent<MiniFlower>();
            GameObject chomper = blueSeed.FindGameObjectInChildren("Chomper");
            chomper.LocateMyFSM("FSM").Fsm.GetFsmGameObject("Audio Player").Value = audioPlayer;
            chomper.AddComponent<Chomper>();
            
            _control.InsertMethod("Phase Check", 0, () => _control.Fsm.GetFsmInt("HP").Value = _hm.hp);
            _control.InsertMethod("Lunging Bottom", 0, () => _nb.active = false);
            _control.InsertMethod("Lunge Bottom End", 0, () => _nb.active = true);
            _control.InsertCoroutine("GG Return", 0, DoDreamReturn);
            _control.InsertMethod("Death", 0, () => PlayerData.instance.isInvincible = true);

            _control.GetAction<RandomWait>("Dancing", 3).min = ArenaFinder.BossLevel > 0 ? 0.75f : 1.75f;
            _control.GetAction<RandomWait>("Dancing", 3).max = ArenaFinder.BossLevel > 0 ? 1f : 2f;
            
            _control.GetAction<RandomWait>("P2 Idle", 1).min = ArenaFinder.BossLevel > 0 ? 1f : 2f;
            _control.GetAction<RandomWait>("P2 Idle", 1).max = ArenaFinder.BossLevel > 0 ? 1.75f : 3f;
            
            _hm.hp = ArenaFinder.BossLevel > 0 ? AscHP : AttHP;
            _hm.hasSpecialDeath = true;
            _control.Fsm.GetFsmInt("HP").Value = ArenaFinder.BossLevel > 0 ? AscHP : AttHP;
            _control.Fsm.GetFsmInt("P2 HP").Value = ArenaFinder.BossLevel > 0 ? AscP2 : AttP2;
            
            StartMusic();
        }

        private void OnTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (self.gameObject.name.Contains("Cagney Carnation"))
            {
                _he.RecieveHitEffect(hitInstance.Direction);
                _sf.flashFocusHeal();
            }

            orig(self, hitInstance);
        }
        
        private void AssignFields()
        {
            EnemyDeathEffectsUninfected de = _hornet.GetComponent<EnemyDeathEffectsUninfected>();
            foreach (FieldInfo fi in typeof(EnemyDeathEffectsUninfected).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                fi.SetValue(_de, fi.GetValue(de));
            }
            
            EnemyHitEffectsUninfected he = _hornet.GetComponent<EnemyHitEffectsUninfected>();
            foreach (FieldInfo fi in typeof(EnemyHitEffectsUninfected).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                fi.SetValue(_he, fi.GetValue(he));
            }

            HealthManager hm = _hornet.GetComponent<HealthManager>();
            foreach (FieldInfo fi in typeof(HealthManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.Name.Contains("Prefab")))
            {
                fi.SetValue(_hm, fi.GetValue(hm));
            }
        }

        private void StartMusic()
        {
            MusicCue musicCue = ScriptableObject.CreateInstance<MusicCue>();
            List<MusicCue.MusicChannelInfo> channelInfos = new List<MusicCue.MusicChannelInfo>();
            MusicCue.MusicChannelInfo channelInfo = new MusicCue.MusicChannelInfo();
            channelInfo.SetAttr("clip", CagneyCarnation.Music);
            channelInfos.Add(channelInfo);
            musicCue.SetAttr("channelInfos", channelInfos.ToArray());
            GameManager.instance.AudioManager.ApplyMusicCue(musicCue, 0, 0, false);
        }

        private IEnumerator DoDreamReturn()
        {
            ArenaFinder.WonFight = true;
            
            var bsc = SceneLoader.SceneController.GetComponent<BossSceneController>();
            GameObject transition = Instantiate(bsc.transitionPrefab);
            PlayMakerFSM transitionsFSM = transition.LocateMyFSM("Transitions");
            transitionsFSM.SetState("Out Statue");
            yield return new WaitForSeconds(1.0f);
            bsc.DoDreamReturn();
        }
        
        private void OnDestroy()
        {
            On.HealthManager.TakeDamage -= OnTakeDamage;
        }
        
        private void Log(object message) => Modding.Logger.Log("[Flower] " + message);
    }
}