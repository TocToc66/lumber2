using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Lumber.Player;
using Lumber.Economy;
using Lumber.Loot;
using Lumber.Progression;

namespace Lumber.UI
{
    /// Builds the entire HUD/controls/menu UI purely in code at runtime:
    /// crosshair, money/level/XP/equipped-axe/box HUD, virtual joystick, look pad,
    /// chop button, and a tabbed menu panel (Caisses / Bucheron / Camp).
    public class UIManager : MonoBehaviour
    {
        private Text moneyText;
        private Text axeText;
        private Text boxText;
        private Text levelText;
        private Slider xpSlider;

        private GameObject menuPanel;
        private Transform menuContent;
        private string currentTab = "caisses";
        private Text caissesCountLabel;
        private Button openBoxButton;

        private GameObject toast;
        private Text toastText;
        private float toastTimer;

        private InventoryManager inventory;
        private UpgradeManager upgrades;

        public void Build(FirstPersonController player, AxeTool axe, InventoryManager inventoryManager, UpgradeManager upgradeManager)
        {
            inventory = inventoryManager;
            upgrades = upgradeManager;

            EnsureEventSystem();

            var canvasGo = new GameObject("HUDCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            BuildCrosshair(canvasGo.transform);
            BuildTopBar(canvasGo.transform);
            BuildJoystick(canvasGo.transform, player);
            BuildLookPad(canvasGo.transform, player);
            BuildChopButton(canvasGo.transform, axe);
            BuildMenuPanel(canvasGo.transform);
            BuildToast(canvasGo.transform);

            EconomyManager.Instance.OnMoneyChanged += UpdateMoney;
            ExperienceManager.Instance.OnXpChanged += UpdateXp;
            inventory.OnInventoryChanged += RefreshHudAxe;
            inventory.OnBoxCountChanged += _ => RefreshHudBoxes();
            inventory.OnBoxOpened += HandleBoxOpened;

            UpdateMoney(EconomyManager.Instance.Money);
            UpdateXp(ExperienceManager.Instance.Xp, ExperienceManager.Instance.XpToNextLevel, ExperienceManager.Instance.Level);
            RefreshHudAxe();
            RefreshHudBoxes();
        }

        private void Update()
        {
            if (toastTimer > 0f)
            {
                toastTimer -= Time.deltaTime;
                if (toastTimer <= 0f)
                    toast.SetActive(false);
            }
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        private RectTransform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            return rt;
        }

        private void SetupText(Text text, int size, TextAnchor anchor)
        {
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void BuildCrosshair(Transform parent)
        {
            var rt = CreateRect(parent, "Crosshair", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(6, 6), Vector2.zero);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.85f);
        }

        private void BuildTopBar(Transform parent)
        {
            var moneyRt = CreateRect(parent, "MoneyText", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(320, 50), new Vector2(20, -18));
            moneyText = moneyRt.gameObject.AddComponent<Text>();
            SetupText(moneyText, 30, TextAnchor.UpperLeft);

            var axeRt = CreateRect(parent, "AxeText", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(320, 34), new Vector2(20, -60));
            axeText = axeRt.gameObject.AddComponent<Text>();
            SetupText(axeText, 20, TextAnchor.UpperLeft);

            var boxRt = CreateRect(parent, "BoxText", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(320, 30), new Vector2(20, -92));
            boxText = boxRt.gameObject.AddComponent<Text>();
            SetupText(boxText, 18, TextAnchor.UpperLeft);
            boxText.color = new Color(0.85f, 0.85f, 0.85f);

            var levelRt = CreateRect(parent, "LevelText", new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(220, 40), new Vector2(-20, -20));
            levelText = levelRt.gameObject.AddComponent<Text>();
            SetupText(levelText, 26, TextAnchor.UpperRight);

            var xpBg = CreateRect(parent, "XpBarBg", new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(220, 14), new Vector2(-20, -64));
            var xpBgImg = xpBg.gameObject.AddComponent<Image>();
            xpBgImg.color = new Color(0f, 0f, 0f, 0.5f);

            var sliderGo = new GameObject("XpSlider", typeof(RectTransform));
            sliderGo.transform.SetParent(xpBg, false);
            var sliderRt = sliderGo.GetComponent<RectTransform>();
            sliderRt.anchorMin = Vector2.zero;
            sliderRt.anchorMax = Vector2.one;
            sliderRt.offsetMin = Vector2.zero;
            sliderRt.offsetMax = Vector2.zero;

            xpSlider = sliderGo.AddComponent<Slider>();
            xpSlider.transition = Selectable.Transition.None;
            xpSlider.interactable = false;
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;

            var fillAreaRt = CreateRect(sliderRt, "FillArea", Vector2.zero, Vector2.one, new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            var fillRt = CreateRect(fillAreaRt, "Fill", Vector2.zero, Vector2.one, new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            var fillImg = fillRt.gameObject.AddComponent<Image>();
            fillImg.color = new Color(0.35f, 0.85f, 0.35f, 1f);

            xpSlider.fillRect = fillRt;
            xpSlider.targetGraphic = fillImg;
        }

        private void BuildJoystick(Transform parent, FirstPersonController player)
        {
            var bg = CreateRect(parent, "JoystickBg", new Vector2(0, 0), new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(160, 160), new Vector2(140, 160));
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.color = new Color(1f, 1f, 1f, 0.15f);

            var handle = CreateRect(bg, "JoystickHandle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(70, 70), Vector2.zero);
            var handleImg = handle.gameObject.AddComponent<Image>();
            handleImg.color = new Color(1f, 1f, 1f, 0.5f);

            var joystick = bg.gameObject.AddComponent<MobileJoystick>();
            joystick.background = bg;
            joystick.handle = handle;
            joystick.handleRange = 55f;
            joystick.target = player;
        }

        private void BuildLookPad(Transform parent, FirstPersonController player)
        {
            var pad = CreateRect(parent, "LookPad", new Vector2(0.35f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var img = pad.gameObject.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);

            var lookPad = pad.gameObject.AddComponent<LookPad>();
            lookPad.target = player;

            pad.SetAsFirstSibling();
        }

        private void BuildChopButton(Transform parent, AxeTool axe)
        {
            var btnRt = CreateRect(parent, "ChopButton", new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(140, 140), new Vector2(-120, 150));
            var img = btnRt.gameObject.AddComponent<Image>();
            img.color = new Color(0.8f, 0.25f, 0.2f, 0.85f);
            var btn = btnRt.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => axe.TryChop());

            var labelRt = CreateRect(btnRt, "Label", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var label = labelRt.gameObject.AddComponent<Text>();
            SetupText(label, 26, TextAnchor.MiddleCenter);
            label.text = "COUPER";
        }

        // ---------------------------------------------------------------
        // Menu panel: Caisses / Bucheron / Camp tabs
        // ---------------------------------------------------------------

        private void BuildMenuPanel(Transform parent)
        {
            var toggleRt = CreateRect(parent, "MenuToggle", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(160, 50), new Vector2(0, -30));
            var toggleImg = toggleRt.gameObject.AddComponent<Image>();
            toggleImg.color = new Color(0.2f, 0.2f, 0.2f, 0.75f);
            var toggleBtn = toggleRt.gameObject.AddComponent<Button>();

            var toggleLabelRt = CreateRect(toggleRt, "Label", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var toggleLabel = toggleLabelRt.gameObject.AddComponent<Text>();
            SetupText(toggleLabel, 22, TextAnchor.MiddleCenter);
            toggleLabel.text = "MENU";

            var panelRt = CreateRect(parent, "MenuPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(620, 760), Vector2.zero);
            var panelImg = panelRt.gameObject.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);
            menuPanel = panelRt.gameObject;
            menuPanel.SetActive(false);

            toggleBtn.onClick.AddListener(() =>
            {
                menuPanel.SetActive(!menuPanel.activeSelf);
                if (menuPanel.activeSelf) RefreshMenuContent();
            });

            var closeRt = CreateRect(panelRt, "Close", new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(50, 50), new Vector2(-10, -10));
            var closeImg = closeRt.gameObject.AddComponent<Image>();
            closeImg.color = new Color(0.7f, 0.15f, 0.15f, 1f);
            var closeBtn = closeRt.gameObject.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => menuPanel.SetActive(false));

            var closeLabelRt = CreateRect(closeRt, "X", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var closeLabel = closeLabelRt.gameObject.AddComponent<Text>();
            SetupText(closeLabel, 26, TextAnchor.MiddleCenter);
            closeLabel.text = "X";

            BuildTabButton(panelRt, "Caisses", "caisses", new Vector2(0f, 1f), new Vector2(1f / 3f, 1f));
            BuildTabButton(panelRt, "Bucheron", "bucheron", new Vector2(1f / 3f, 1f), new Vector2(2f / 3f, 1f));
            BuildTabButton(panelRt, "Camp", "camp", new Vector2(2f / 3f, 1f), new Vector2(1f, 1f));

            var contentRt = CreateRect(panelRt, "Content", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(-30, 90), new Vector2(0, -95));
            contentRt.offsetMin = new Vector2(15, 15);
            contentRt.offsetMax = new Vector2(-15, -95);
            contentRt.gameObject.AddComponent<RectMask2D>();
            menuContent = contentRt;

            RefreshMenuContent();
        }

        private void BuildTabButton(Transform parent, string label, string tabKey, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rt = CreateRect(parent, "Tab_" + tabKey, anchorMin, anchorMax, new Vector2(0.5f, 1f), new Vector2(-4, 60), new Vector2(0, -12));
            var img = rt.gameObject.AddComponent<Image>();
            img.color = tabKey == currentTab ? new Color(0.55f, 0.4f, 0.2f, 0.9f) : new Color(1f, 1f, 1f, 0.08f);
            img.name = "TabBg";
            var btn = rt.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                currentTab = tabKey;
                RefreshMenuContent();
            });

            var labelRt = CreateRect(rt, "Label", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var text = labelRt.gameObject.AddComponent<Text>();
            SetupText(text, 20, TextAnchor.MiddleCenter);
            text.text = label;
        }

        private void RefreshMenuContent()
        {
            if (menuContent == null) return;

            for (int i = menuContent.childCount - 1; i >= 0; i--)
                Destroy(menuContent.GetChild(i).gameObject);

            RefreshTabHighlights();

            if (currentTab == "caisses")
                BuildCaissesTab();
            else if (currentTab == "bucheron")
                BuildUpgradeTab(upgrades.character);
            else
                BuildUpgradeTab(upgrades.camp);
        }

        private void RefreshTabHighlights()
        {
            if (menuPanel == null) return;
            var panelT = menuPanel.transform;
            string[] keys = { "caisses", "bucheron", "camp" };
            foreach (var key in keys)
            {
                var tab = panelT.Find("Tab_" + key);
                if (tab == null) continue;
                var bg = tab.Find("TabBg");
                var img = bg != null ? bg.GetComponent<Image>() : tab.GetComponent<Image>();
                if (img != null)
                    img.color = key == currentTab ? new Color(0.55f, 0.4f, 0.2f, 0.9f) : new Color(1f, 1f, 1f, 0.08f);
            }
        }

        private void BuildCaissesTab()
        {
            var headerRt = CreateRect(menuContent, "CaissesHeader", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(0, 90), new Vector2(0, 0));
            caissesCountLabel = headerRt.gameObject.AddComponent<Text>();
            SetupText(caissesCountLabel, 22, TextAnchor.UpperLeft);

            var openRt = CreateRect(menuContent, "OpenBtn", new Vector2(1, 1), new Vector2(1, 1), new Vector2(1f, 1f), new Vector2(220, 56), new Vector2(0, -6));
            var openImg = openRt.gameObject.AddComponent<Image>();
            openBoxButton = openRt.gameObject.AddComponent<Button>();
            var openLabelRt = CreateRect(openRt, "Label", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var openLabel = openLabelRt.gameObject.AddComponent<Text>();
            SetupText(openLabel, 20, TextAnchor.MiddleCenter);
            openLabel.text = "Ouvrir une caisse";

            bool canOpen = inventory.BoxCount > 0;
            openImg.color = canOpen ? new Color(0.75f, 0.6f, 0.15f, 0.9f) : new Color(0.2f, 0.2f, 0.2f, 0.5f);
            openBoxButton.interactable = canOpen;
            openBoxButton.onClick.AddListener(() => inventory.OpenBox());

            RefreshCaissesHeader();

            var listTitleRt = CreateRect(menuContent, "ListTitle", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(0, 34), new Vector2(0, -100));
            var listTitle = listTitleRt.gameObject.AddComponent<Text>();
            SetupText(listTitle, 18, TextAnchor.UpperLeft);
            listTitle.color = new Color(0.8f, 0.8f, 0.8f);
            listTitle.text = "Haches trouvees (" + inventory.Axes.Count + ")";

            int index = 0;
            foreach (var axeInst in inventory.Axes)
            {
                BuildAxeRow(menuContent, axeInst, 150 + index * 78);
                index++;
            }
        }

        private void RefreshCaissesHeader()
        {
            if (caissesCountLabel != null)
                caissesCountLabel.text = "Caisses : " + inventory.BoxCount + " / " + inventory.MaxBoxCapacity;
        }

        private void BuildAxeRow(Transform parent, AxeInstance axeInst, float yOffset)
        {
            var rowRt = CreateRect(parent, "AxeRow", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(0, 70), new Vector2(0, -yOffset));
            var rowImg = rowRt.gameObject.AddComponent<Image>();
            bool equipped = axeInst.id == inventory.EquippedId;
            rowImg.color = equipped ? new Color(0.3f, 0.4f, 0.2f, 0.7f) : new Color(1f, 1f, 1f, 0.06f);

            var def = RarityInfo.Table[axeInst.rarity];

            var swatchRt = CreateRect(rowRt, "Swatch", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(16, 50), new Vector2(12, 0));
            var swatchImg = swatchRt.gameObject.AddComponent<Image>();
            swatchImg.color = def.color;

            var nameRt = CreateRect(rowRt, "Name", new Vector2(0, 0), new Vector2(0.62f, 1), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
            nameRt.offsetMin = new Vector2(36, nameRt.offsetMin.y);
            var nameText = nameRt.gameObject.AddComponent<Text>();
            SetupText(nameText, 19, TextAnchor.MiddleLeft);
            float chopsPerSec = 1f / Mathf.Max(0.01f, axeInst.cooldown);
            nameText.text = axeInst.axeName + " (" + def.label + ")\nDegats " + axeInst.damage + " - " + chopsPerSec.ToString("0.0") + "/s";
            nameText.color = def.color;

            var actionRt = CreateRect(rowRt, "Action", new Vector2(0.64f, 0.15f), new Vector2(0.98f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var actionImg = actionRt.gameObject.AddComponent<Image>();
            var actionLabelRt = CreateRect(actionRt, "Label", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var actionLabel = actionLabelRt.gameObject.AddComponent<Text>();
            SetupText(actionLabel, 18, TextAnchor.MiddleCenter);

            if (equipped)
            {
                actionImg.color = new Color(0.25f, 0.55f, 0.25f, 0.9f);
                actionLabel.text = "Equipee";
            }
            else
            {
                actionImg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                actionLabel.text = "Equiper";
                var btn = actionRt.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    inventory.Equip(axeInst.id);
                    RefreshMenuContent();
                });
            }
        }

        private void BuildUpgradeTab(System.Collections.Generic.List<UpgradeTrack> tracks)
        {
            int index = 0;
            foreach (var track in tracks)
            {
                BuildUpgradeRow(menuContent, track, index * 100);
                index++;
            }
        }

        private void BuildUpgradeRow(Transform parent, UpgradeTrack track, float yOffset)
        {
            var rowRt = CreateRect(parent, "UpgradeRow", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(0, 90), new Vector2(0, -yOffset));
            var rowImg = rowRt.gameObject.AddComponent<Image>();
            rowImg.color = new Color(1f, 1f, 1f, 0.06f);

            var nameRt = CreateRect(rowRt, "Name", new Vector2(0, 0), new Vector2(0.62f, 1), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
            nameRt.offsetMin = new Vector2(20, nameRt.offsetMin.y);
            var nameText = nameRt.gameObject.AddComponent<Text>();
            SetupText(nameText, 20, TextAnchor.MiddleLeft);
            nameText.text = track.label + " - Niveau " + track.level + "/" + track.maxLevel + "\n" + track.description;

            var buyRt = CreateRect(rowRt, "Buy", new Vector2(0.64f, 0.15f), new Vector2(0.98f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var buyImg = buyRt.gameObject.AddComponent<Image>();
            var buyLabelRt = CreateRect(buyRt, "Label", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var buyLabel = buyLabelRt.gameObject.AddComponent<Text>();
            SetupText(buyLabel, 19, TextAnchor.MiddleCenter);

            if (track.level >= track.maxLevel)
            {
                buyImg.color = new Color(0.25f, 0.55f, 0.25f, 0.9f);
                buyLabel.text = "MAX";
            }
            else
            {
                buyImg.color = new Color(0.75f, 0.6f, 0.15f, 0.9f);
                buyLabel.text = track.CostForNextLevel() + " $";
                var btn = buyRt.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    upgrades.TryUpgrade(track);
                    RefreshMenuContent();
                });
            }
        }

        // ---------------------------------------------------------------
        // Toast (box reveal)
        // ---------------------------------------------------------------

        private void BuildToast(Transform parent)
        {
            var rt = CreateRect(parent, "Toast", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(460, 100), new Vector2(0, 240));
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.85f);
            toast = rt.gameObject;
            toast.SetActive(false);

            var textRt = CreateRect(rt, "Text", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            toastText = textRt.gameObject.AddComponent<Text>();
            SetupText(toastText, 24, TextAnchor.MiddleCenter);
        }

        private void HandleBoxOpened(AxeInstance axeInst)
        {
            var def = RarityInfo.Table[axeInst.rarity];
            toastText.text = "Obtenu : " + axeInst.axeName + "\n(" + def.label + ")";
            toastText.color = def.color;
            toast.SetActive(true);
            toastTimer = 2.4f;

            if (menuPanel.activeSelf && currentTab == "caisses")
                RefreshMenuContent();
        }

        // ---------------------------------------------------------------
        // HUD refresh
        // ---------------------------------------------------------------

        private void RefreshHudAxe()
        {
            var eq = inventory.Equipped;
            if (eq == null || axeText == null) return;
            var def = RarityInfo.Table[eq.rarity];
            axeText.text = eq.axeName;
            axeText.color = def.color;
        }

        private void RefreshHudBoxes()
        {
            if (boxText != null)
                boxText.text = "Caisses : " + inventory.BoxCount + " / " + inventory.MaxBoxCapacity;
            RefreshCaissesHeader();
        }

        private void UpdateMoney(int money)
        {
            if (moneyText != null)
                moneyText.text = "$ " + money;
        }

        private void UpdateXp(int xp, int xpToNext, int level)
        {
            if (levelText != null)
                levelText.text = "Niveau " + level;
            if (xpSlider != null)
                xpSlider.value = xpToNext > 0 ? (float)xp / xpToNext : 0f;
        }
    }
}
