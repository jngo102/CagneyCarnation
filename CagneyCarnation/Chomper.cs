using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CagneyCarnation
{
    public class Chomper : MonoBehaviour
    {
        private const int AttHP = 35;
        private const int AscHP = 65;
        
        private EnemyDeathEffectsUninfected _de;
        private EnemyHitEffectsUninfected _he;
        private HealthManager _hm;
        private SpriteFlash _sf;

        private GameObject _hornet;
        
        private PlayMakerFSM _fsm;
        
        private void Awake()
        {
            _fsm = gameObject.LocateMyFSM("FSM");
            
            _de = gameObject.AddComponent<EnemyDeathEffectsUninfected>();
            _he = gameObject.AddComponent<EnemyHitEffectsUninfected>();
            _hm = gameObject.AddComponent<HealthManager>();
            _sf = gameObject.AddComponent<SpriteFlash>();
            _sf.enabled = true;
            gameObject.AddComponent<NonBouncer>();

            _hornet = CagneyCarnation.GameObjects["Hornet"];

            AssignFields();
            
            On.HealthManager.TakeDamage += OnTakeDamage;
        }

        private void Start()
        {
            _fsm.Fsm.GetFsmGameObject("Hero").Value = HeroController.instance.gameObject;

            _hm.hp = ArenaFinder.BossLevel > 0 ? AscHP : AttHP;
            _hm.hasSpecialDeath = true;
        }

        private void OnTakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            if (self.gameObject.GetInstanceID() == gameObject.GetInstanceID())
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

        private void Log(object message) => Modding.Logger.Log("[Chomper] " + message);
    }
}