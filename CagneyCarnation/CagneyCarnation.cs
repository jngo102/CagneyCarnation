using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Modding;
using UnityEngine;
using uObject = UnityEngine.Object;

namespace CagneyCarnation
{
    public class CagneyCarnation : Mod, ITogglableMod,ILocalSettings<SaveSettings>
    {
        public static CagneyCarnation Instance;
        
        public static string ArenaAssetsPath;
        public static Dictionary<string, AssetBundle> Bundles = new Dictionary<string, AssetBundle>();
        public static Dictionary<string, GameObject> GameObjects = new Dictionary<string, GameObject>();
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        
        public static AudioClip Music;
        
      
        public static SaveSettings _settings = new SaveSettings();
        public void OnLoadLocal(SaveSettings s) => _settings = s;
        public SaveSettings OnSaveLocal() => _settings;
        
        public override string GetVersion()
        {
            return "1.0.0";
        }

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Hornet_2", "Boss Holder/Hornet Boss 2"),
                ("GG_Hornet_2", "Boss Scene Controller"),
                ("GG_Hornet_2", "_SceneManager"),
            };
        }
        
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            GameObjects.Add("Hornet", preloadedObjects["GG_Hornet_2"]["Boss Holder/Hornet Boss 2"]);
            GameObjects.Add("BSC", preloadedObjects["GG_Hornet_2"]["Boss Scene Controller"]);
            GameObjects.Add("SM", preloadedObjects["GG_Hornet_2"]["_SceneManager"]);

            Instance = this;

            LoadAssets();

            Unload();
            
            ModHooks.AfterSavegameLoadHook += AfterSaveGameLoad;
            ModHooks.GetPlayerVariableHook += GetVariableHook;
            ModHooks.LanguageGetHook += LangGet;
            ModHooks.NewGameHook += AddComponent;
            ModHooks.SetPlayerVariableHook += SetVariableHook;
        }

        private void AfterSaveGameLoad(SaveGameData data) => AddComponent();

        private object GetVariableHook(Type t, string key, object orig)
        {
            if (key == "statueStateFlower")
            {
                return _settings.CompletionFlower;
            }
            
            return orig;
        }
        
        private string LangGet(string key, string sheettitle,string orig)
        {
            switch (key)
            {
                case "FLOWER_NAME": return "Cagney Carnation";
                case "FLOWER_DESC": return "Hostile god of the meadow.";
                default: return orig;
            }
        }
        
        private void AddComponent()
        {
            GameManager.instance.gameObject.AddComponent<ArenaFinder>();
            GameManager.instance.gameObject.AddComponent<SceneLoader>();
        }
        
        private object SetVariableHook(Type t, string key, object obj)
        {
            if (key == "statueStateFlower")
            {
                _settings.CompletionFlower = (BossStatue.Completion) obj;
            }
            
            return obj;
        }

        private void LoadAssets()
        {
            string flowerAssetsPath;
            switch (SystemInfo.operatingSystemFamily)
            {
                case OperatingSystemFamily.Windows:
                    ArenaAssetsPath = "arenawin";
                    flowerAssetsPath = "flowerwin";
                    break;
                case OperatingSystemFamily.Linux:
                    ArenaAssetsPath = "arenalin";
                    flowerAssetsPath = "flowerlin";
                    break;
                case OperatingSystemFamily.MacOSX:
                    ArenaAssetsPath = "arenamac";
                    flowerAssetsPath = "flowermac";
                    break;
                default:
                    Log("ERROR UNSUPPORTED SYSTEM: " + SystemInfo.operatingSystemFamily);
                    return;
            }
            
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (string res in asm.GetManifestResourceNames())
            {
                using (Stream s = asm.GetManifestResourceStream(res))
                {
                    if (s == null) continue;
                    byte[] buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                    s.Dispose();
                    
                    string bundleName = Path.GetExtension(res).Substring(1);
                    if (bundleName != flowerAssetsPath && bundleName != ArenaAssetsPath) continue;
                    Log("Loading bundle: " + bundleName);
                    Bundles[bundleName] = AssetBundle.LoadFromMemory(buffer);
                }
            }

            AssetBundle flowerBundle = Bundles[flowerAssetsPath];
            GameObjects["Flower"] = flowerBundle.LoadAsset<GameObject>("Cagney Carnation");
            Music = flowerBundle.LoadAsset<AudioClip>("MUS_Flower");
            Textures["Mugshot"] = flowerBundle.LoadAsset<Texture2D>("Flower Mugshot");
        }
        
        public void Unload()
        {
            ModHooks.AfterSavegameLoadHook -= AfterSaveGameLoad;
            ModHooks.GetPlayerVariableHook -= GetVariableHook;
            ModHooks.LanguageGetHook -= LangGet;
            ModHooks.SetPlayerVariableHook -= SetVariableHook;
            ModHooks.NewGameHook -= AddComponent;

            var finder = GameManager.instance?.gameObject.GetComponent<ArenaFinder>();
            if (finder == null)
            {
                return;
            }
            
            uObject.Destroy(finder);
        }
    }
}