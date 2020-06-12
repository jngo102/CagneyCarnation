using System.Collections;
using ModCommon;
using UnityEngine;

namespace CagneyCarnation
{
    public class SceneLoader : MonoBehaviour
    {
        private AssetBundle _bundle;

        public static GameObject SceneController;
        
        private void Awake()
        {
            On.GameManager.EnterHero += OnEnterHero;
        }

        private void OnEnterHero(On.GameManager.orig_EnterHero orig, GameManager gm, bool additiveGateSearch)
        {
            gm.UpdateSceneName();

            if (gm.sceneName == "FloralFury")
            {
                On.GameManager.RefreshTilemapInfo += OnRefreshTileMapInfo;
                On.HeroController.CanDoubleJump += DisableDoubleJump;
                On.HeroController.CanWallJump += DisableWallJump;
                On.HeroController.CanWallSlide += DisableWallSlide;

                SceneController = Instantiate(CagneyCarnation.GameObjects["BSC"]);
                SceneController.SetActive(true);
                ArenaFinder.BossLevel = SceneController.GetComponent<BossSceneController>().BossLevel;
                
                HeroController.instance.heroLight.sprite = null;
                HeroController.instance.vignette.sprite = null;

                GameObject scenery = GameObject.Find("_Scenery");
                foreach (SpriteRenderer sr in scenery.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    sr.material.shader = Shader.Find("Sprites/Default");
                }
                
                GameObject flower = Instantiate(CagneyCarnation.GameObjects["Flower"], new Vector2(56.5f, 22f), Quaternion.identity);
                flower.AddComponent<Flower>();
                
                GameObject platforms = GameObject.Find("Platforms");
                foreach (tk2dSprite sprite in platforms.GetComponentsInChildren<tk2dSprite>(true))
                {
                    foreach (tk2dSpriteDefinition spriteDef in sprite.Collection.spriteDefinitions)
                    {
                        spriteDef.material.shader = Shader.Find("Sprites/Default-ColorFlash");
                    }
                }
                
                orig(gm, false);
                
                HeroController.instance.transform.SetPosition2D(47f, 23.3f);
                
                StartCoroutine(CameraControl());

                return;
            }
            
            if (gm.sceneName == "GG_Workshop")
            {
                if (_bundle == null)
                {
                    _bundle = CagneyCarnation.Bundles[CagneyCarnation.ArenaAssetsPath];
                }
            
                GameCameras.instance.tk2dCam.ZoomFactor = 1;
                
                On.GameManager.RefreshTilemapInfo -= OnRefreshTileMapInfo;
                On.HeroController.CanDoubleJump -= DisableDoubleJump;
                On.HeroController.CanWallJump -= DisableWallJump;
                On.HeroController.CanWallSlide -= DisableWallSlide;
                
                orig(gm, false);
                
                return;
            }
            
            orig(gm, additiveGateSearch);
        }

        private void OnRefreshTileMapInfo(On.GameManager.orig_RefreshTilemapInfo orig, GameManager self, string targetScene)
        {
            orig(self, targetScene);

            if (targetScene == "FloralFury")
            {
                self.tilemap.width = 20;
                self.tilemap.height = 10;
                self.sceneWidth = 22;
                self.sceneHeight = 9.5f;
            }
        }

        private bool DisableDoubleJump(On.HeroController.orig_CanDoubleJump orig, HeroController hc)
        {
            return false;
        }
        
        private bool DisableWallJump(On.HeroController.orig_CanWallJump orig, HeroController hc)
        {
            return false;
        }
        
        private bool DisableWallSlide(On.HeroController.orig_CanWallSlide orig, HeroController hc)
        {
            return false;
        }
        
        private IEnumerator CameraControl()
        {
            yield return new WaitWhile(() => GameCameras.instance == null);

            GameCameras.instance.sceneParticles.DisableParticles();

            GameObject cameraLockZones = GameObject.Find("_Camera Lock Zones");
            GameObject cameraLockArea = cameraLockZones.FindGameObjectInChildren("CameraLockArea");
            var area = cameraLockArea.AddComponent<CameraLockArea>();
            var col = cameraLockArea.GetComponent<BoxCollider2D>();
            Bounds bounds = col.bounds;
            area.cameraXMin = bounds.min.x + 14.6f;
            area.cameraXMax = bounds.max.x - 14.6f;
            area.cameraYMin = bounds.min.y + 8.6f;
            area.cameraYMax = bounds.max.y - 8f;
            area.preventLookDown = true;
            area.preventLookUp = true;
            
            GameCameras.instance.tk2dCam.ZoomFactor = 1.175f;
        }
        
        private GameObject CreateGateway(string gateName, Vector2 pos, Vector2 size, Vector2 offset, string toScene, string entryGate, bool right, bool left, bool onlyOut, GameManager.SceneLoadVisualizations vis)
        {
            GameObject gate = new GameObject(gateName);
            gate.transform.parent = GameObject.Find("_Transition Gates").transform;
            gate.transform.SetPosition2D(pos);
            var tp = gate.AddComponent<TransitionPoint>();
            if (!onlyOut)
            {
                var bc = gate.AddComponent<BoxCollider2D>();
                bc.size = size;
                bc.isTrigger = true;
                tp.targetScene = toScene;
                tp.entryPoint = entryGate;
            }
            tp.alwaysEnterLeft = left;
            tp.alwaysEnterRight = right;
            GameObject respawnMarker = new GameObject("Hazard Respawn Marker");
            respawnMarker.transform.parent = tp.transform;
            Vector2 markerPos = respawnMarker.transform.position; 
            respawnMarker.transform.position = new Vector2(markerPos.x - 3f, markerPos.y);
            tp.respawnMarker = respawnMarker.AddComponent<HazardRespawnMarker>();
            tp.sceneLoadVisualization = vis;

            return gate;
        }

        private void OnDestroy()
        {
            On.GameManager.EnterHero -= OnEnterHero;
        }

        private void Log(object message) => Modding.Logger.Log("[Scene Loader] " + message);
    }
}