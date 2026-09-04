using UnityEngine;
using Lumber.Player;
using Lumber.World;
using Lumber.Economy;
using Lumber.Shop;
using Lumber.UI;
using Lumber.Rendering;

namespace Lumber.Core
{
    /// Top-level orchestrator: builds the environment, spawns the player, generates
    /// the forest, builds the UI, applies the PS1 render style, and handles saving.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private SaveData save;
        private EconomyManager economy;
        private ExperienceManager experience;
        private ForestGenerator forest;
        private ShopManager shop;
        private FirstPersonController player;
        private AxeTool axeTool;

        private float autoSaveTimer;
        private const float AutoSaveInterval = 15f;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ConfigureScreen();

            save = SaveManager.Load();

            BuildWorld();
            BuildManagers();
            SpawnPlayer();
            BuildForest();
            BuildUI();
            ApplyRenderStyle();
        }

        private void ConfigureScreen()
        {
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }

        private void BuildWorld()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.62f, 0.72f, 0.66f);
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 55f;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.5f, 0.48f);

            var sunGo = new GameObject("Sun");
            var light = sunGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.86f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.None;
            sunGo.transform.rotation = Quaternion.Euler(48f, -30f, 0f);

            var groundGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundGo.name = "Ground";
            groundGo.transform.localScale = new Vector3(20f, 1f, 20f);
            var groundMat = new Material(Shader.Find("Lumber/PS1FlatLit"));
            groundMat.color = new Color(0.32f, 0.5f, 0.28f);
            groundGo.GetComponent<Renderer>().sharedMaterial = groundMat;
        }

        private void BuildManagers()
        {
            economy = gameObject.AddComponent<EconomyManager>();
            economy.Init(save.money);

            experience = gameObject.AddComponent<ExperienceManager>();
            experience.Init(save.level, save.xp);

            shop = gameObject.AddComponent<ShopManager>();
        }

        private void SpawnPlayer()
        {
            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 1f, -3f);

            var cc = playerGo.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.radius = 0.35f;

            player = playerGo.AddComponent<FirstPersonController>();

            var pivotGo = new GameObject("CameraPivot");
            pivotGo.transform.SetParent(playerGo.transform, false);
            pivotGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            player.cameraPivot = pivotGo.transform;

            var camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(pivotGo.transform, false);
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 60f;
            camGo.AddComponent<AudioListener>();

            var axeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            axeGo.name = "AxeModel";
            Destroy(axeGo.GetComponent<Collider>());
            axeGo.transform.SetParent(camGo.transform, false);
            axeGo.transform.localPosition = new Vector3(0.35f, -0.3f, 0.6f);
            axeGo.transform.localRotation = Quaternion.Euler(20f, -10f, 10f);
            axeGo.transform.localScale = new Vector3(0.06f, 0.5f, 0.06f);
            var axeMat = new Material(Shader.Find("Lumber/PS1FlatLit"));
            axeMat.color = new Color(0.42f, 0.28f, 0.16f);
            axeGo.GetComponent<Renderer>().sharedMaterial = axeMat;

            axeTool = camGo.AddComponent<AxeTool>();
            axeTool.swingPivot = axeGo.transform;

            int tierIndex = Mathf.Clamp(save.axeTier, 0, 5);
            shop.Init(axeTool, economy, tierIndex);
        }

        private void BuildForest()
        {
            var forestGo = new GameObject("Forest");
            forest = forestGo.AddComponent<ForestGenerator>();

            var trunkMat = new Material(Shader.Find("Lumber/PS1FlatLit"));
            trunkMat.color = new Color(0.38f, 0.25f, 0.14f);
            var leafMat = new Material(Shader.Find("Lumber/PS1FlatLit"));
            leafMat.color = new Color(0.16f, 0.42f, 0.18f);

            forest.trunkMaterial = trunkMat;
            forest.leafMaterial = leafMat;
            forest.Generate(12345);

            foreach (var tree in forest.SpawnedTrees)
                tree.OnFelled += HandleTreeFelled;
        }

        private void HandleTreeFelled(Tree tree)
        {
            economy.Add(tree.woodValue);
            experience.AddXp(tree.xpValue);
        }

        private void BuildUI()
        {
            var uiGo = new GameObject("UI");
            var ui = uiGo.AddComponent<UIManager>();
            ui.Build(player, axeTool, shop);
        }

        private void ApplyRenderStyle()
        {
            var cam = Camera.main;
            if (cam != null)
                cam.gameObject.AddComponent<PS1RenderEffects>();
        }

        private void Update()
        {
            autoSaveTimer += Time.deltaTime;
            if (autoSaveTimer >= AutoSaveInterval)
            {
                autoSaveTimer = 0f;
                SaveNow();
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.touchCount == 0 && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
            {
                axeTool.TryChop();
            }
#endif
        }

        public void SaveNow()
        {
            save.money = economy.Money;
            save.level = experience.Level;
            save.xp = experience.Xp;
            save.axeTier = shop.CurrentTierIndex;
            SaveManager.Save(save);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) SaveNow();
        }

        private void OnApplicationQuit()
        {
            SaveNow();
        }
    }
}
