using System.Linq;
using System.Reflection;
using ModCommon.Util;
using UnityEngine;

namespace CagneyCarnation
{
    public class MiniFlower : MonoBehaviour
    {
        private const int AttHP = 120;
        private const int AscHP = 150;

        private EnemyDeathEffectsUninfected _de;
        private EnemyHitEffectsUninfected _he;
        private HealthManager _hm;
        private SpriteFlash _sf;

        private PlayMakerFSM _movement;
        private PlayMakerFSM _shootControl;

        private GameObject _hornet;
        
        private void Awake()
        {
            _movement = gameObject.LocateMyFSM("Movement");
            _shootControl = gameObject.LocateMyFSM("Shoot Control");

            _de = gameObject.AddComponent<EnemyDeathEffectsUninfected>();
            _he = gameObject.AddComponent<EnemyHitEffectsUninfected>();
            _hm = gameObject.AddComponent<HealthManager>();
            _sf = gameObject.AddComponent<SpriteFlash>();
            gameObject.AddComponent<NonBouncer>();

            _hornet = CagneyCarnation.GameObjects["Hornet"];
            
            AssignFields();
            
            On.HealthManager.TakeDamage += OnTakeDamage;
        }

        private void Start()
        {
            _movement.InsertMethod("Tween Up", 0, () => _hm.IsInvincible = true);
            
            _shootControl.Fsm.GetFsmGameObject("Hero").Value = HeroController.instance.gameObject;
            
            _shootControl.InsertMethod("Wait", 0, () => _hm.IsInvincible = true);
            _shootControl.InsertMethod("Open", 0, () => _hm.IsInvincible = false);

            _hm.hp = ArenaFinder.BossLevel > 0 ? AscHP : AttHP;
            _hm.hasSpecialDeath = true;
        }

        private void OnTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (self.gameObject.name.Contains("Mini Flower"))
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
        
        private void OnDestroy()
        {
            On.HealthManager.TakeDamage -= OnTakeDamage;
        }

        private void Log(object message) => Modding.Logger.Log("[Mini Flower] " + message);
    }
}