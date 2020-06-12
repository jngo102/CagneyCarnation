using UnityEngine;
using UnityEngine.SceneManagement;

namespace CagneyCarnation
{
    internal class ArenaFinder : MonoBehaviour
    {
        public static bool WonFight;
        public static int BossLevel;
        
        private void Awake()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        }

        private void SceneChanged(Scene prevScene, Scene nextScene)
        {
            if (prevScene.name == "FloralFury" && nextScene.name == "GG_Workshop")
            {
                GameCameras.instance.cameraFadeFSM.Fsm.SetState("FadeIn");
                PlayerData.instance.isInvincible = false;
            }

            if (nextScene.name == "FloralFury")
            {
                GameObject blankerWhite = GameObject.Find("Blanker White");
                blankerWhite.LocateMyFSM("Blanker Control").SendEvent("FADE OUT");
            }
            
            if (nextScene.name == "GG_Workshop")
            {
                CreateStatue();
            }
        }

        private void CreateStatue()
        {
            //Used 56's pale prince code here
            GameObject statue = Instantiate(GameObject.Find("GG_Statue_ElderHu"));
            statue.name = "GG_Statue_CagneyCarnation";
            statue.transform.SetPosition3D(41.0f, statue.transform.GetPositionY() + 0.5f, 0f);
            
            var scene = ScriptableObject.CreateInstance<BossScene>();
            scene.sceneName = "FloralFury";
            
            var bs = statue.GetComponent<BossStatue>();
            bs.bossScene = scene;
            bs.statueStatePD = "statueStateFlower";
            bs.SetPlaquesVisible(bs.StatueState.isUnlocked && bs.StatueState.hasBeenSeen);
            
            var details = new BossStatue.BossUIDetails();
            details.nameKey = details.nameSheet = "FLOWER_NAME";
            details.descriptionKey = details.descriptionSheet = "FLOWER_DESC";
            bs.bossDetails = details;
            
            foreach (Transform i in statue.transform)
            {
                if (i.name.Contains("door"))
                {
                    i.name = "door_dreamReturnGG_GG_Statue_CagneyCarnation";
                }
            }
            
            GameObject appearance = statue.transform.Find("Base").Find("Statue").gameObject;
            appearance.SetActive(true);
            SpriteRenderer sr = appearance.transform.Find("GG_statues_0006_5").GetComponent<SpriteRenderer>();
            sr.enabled = true;
            Texture2D flowerMugshot = CagneyCarnation.Textures["Mugshot"]; 
            sr.sprite = Sprite.Create(flowerMugshot, new Rect(0, 0, flowerMugshot.width, flowerMugshot.height), new Vector2(0.5f, 0.5f));
            sr.transform.SetPosition3D(sr.transform.GetPositionX(), sr.transform.GetPositionY(), 2f);
            
            GameObject inspect = statue.transform.Find("Inspect").gameObject;
            var tmp = inspect.transform.Find("Prompt Marker").position;
            inspect.transform.Find("Prompt Marker").position = new Vector3(tmp.x - 0.3f, tmp.y + 1.0f, tmp.z);
            inspect.SetActive(true);
            
            statue.transform.Find("Spotlight").gameObject.SetActive(true);

            if (WonFight)
            {
                WonFight = false;
                BossStatue.Completion temp = bs.StatueState;
                if (BossLevel == 0) temp.completedTier1 = true;
                else if (BossLevel == 1) temp.completedTier2 = true;
                else if (BossLevel == 2) temp.completedTier3 = true;
                if (temp.completedTier1 && temp.completedTier2 && !temp.seenTier3Unlock) temp.seenTier3Unlock = true;
                PlayerData.instance.currentBossStatueCompletionKey = bs.statueStatePD;
                bs.StatueState = temp;
                bs.SetPlaqueState(bs.StatueState, bs.altPlaqueL, bs.statueStatePD);
            }
            
            statue.SetActive(true);
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneChanged;
        }

        private void Log(object message) => Modding.Logger.Log("[Arena Finder] " + message);
    }    
}