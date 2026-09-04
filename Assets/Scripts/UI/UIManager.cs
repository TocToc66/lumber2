using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Lumber.Player;
using Lumber.Economy;
using Lumber.Shop;

namespace Lumber.UI
{
    /// Builds the entire HUD/controls/shop UI purely in code at runtime:
    /// crosshair, money/level/XP bar, virtual joystick, look pad, chop button,
    /// and a shop panel listing every axe tier.
    public class UIManager : MonoBehaviour
    {
        private Text moneyText;
        private Text levelText;
        private Slider xpSlider;
        private GameObject shopPanel;
        private Transform shopListParent;

        public void Build(FirstPersonController player, AxeTool axe, ShopManager shop)
        {
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
            BuildShopPanel(canvasGo.transform, shop);

            EconomyManager.Instance.OnMoneyChanged += UpdateMoney;
            ExperienceManager.Instance.OnXpChanged += UpdateXp;
            UpdateMoney(EconomyManager.Instance.Money);
            UpdateXp(ExperienceManager.Instance.Xp, ExperienceManager.Instance.XpToNextLevel, ExperienceManager.Instance.Level);
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
            var moneyRt = CreateRect(parent, "MoneyText", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(300, 60), new Vector2(20, -20));
            moneyText = moneyRt.gameObject.AddComponent<Text>();
            SetupText(moneyText, 32, TextAnchor.UpperLeft);

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

            // Push behind every other control so buttons/joystick on top always win touches.
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

        private void BuildShopPanel(Transform parent, ShopManager shop)
        {
            var toggleRt = CreateRect(parent, "ShopToggle", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(160, 50), new Vector2(0, -20));
            var toggleImg = toggleRt.gameObject.AddComponent<Image>();
            toggleImg.color = new Color(0.2f, 0.2f, 0.2f, 0.75f);
            var toggleBtn = toggleRt.gameObject.AddComponent<Button>();

            var toggleLabelRt = CreateRect(toggleRt, "Label", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var toggleLabel = toggleLabelRt.gameObject.AddComponent<Text>();
            SetupText(toggleLabel, 22, TextAnchor.MiddleCenter);
            toggleLabel.text = "BOUTIQUE";

            var panelRt = CreateRect(parent, "ShopPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560, 700), Vector2.zero);
            var panelImg = panelRt.gameObject.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);
            shopPanel = panelRt.gameObject;
            shopPanel.SetActive(false);

            toggleBtn.onClick.AddListener(() => shopPanel.SetActive(!shopPanel.activeSelf));

            var closeRt = CreateRect(panelRt, "Close", new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(50, 50), new Vector2(-10, -10));
            var closeImg = closeRt.gameObject.AddComponent<Image>();
            closeImg.color = new Color(0.7f, 0.15f, 0.15f, 1f);
            var closeBtn = closeRt.gameObject.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => shopPanel.SetActive(false));

            var closeLabelRt = CreateRect(closeRt, "X", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var closeLabel = closeLabelRt.gameObject.AddComponent<Text>();
            SetupText(closeLabel, 26, TextAnchor.MiddleCenter);
            closeLabel.text = "X";

            var titleRt = CreateRect(panelRt, "Title", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(0, 50), new Vector2(0, -10));
            var titleText = titleRt.gameObject.AddComponent<Text>();
            SetupText(titleText, 30, TextAnchor.MiddleCenter);
            titleText.text = "Haches";

            shopListParent = panelRt;
            RefreshShopRows(shop);

            shop.OnTierChanged += _ => RefreshShopRows(shop);
        }

        private void BuildShopRow(Transform parent, ShopManager shop, int index)
        {
            var tier = shop.tiers[index];
            float y = -80 - index * 90;
            var rowRt = CreateRect(parent, "Row" + index, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f), new Vector2(-40, 80), new Vector2(0, y));

            var rowImg = rowRt.gameObject.AddComponent<Image>();
            rowImg.color = new Color(1f, 1f, 1f, 0.06f);

            var nameRt = CreateRect(rowRt, "Name", new Vector2(0, 0), new Vector2(0.6f, 1), new Vector2(0, 0.5f), Vector2.zero, Vector2.zero);
            nameRt.offsetMin = new Vector2(20, nameRt.offsetMin.y);
            var nameText = nameRt.gameObject.AddComponent<Text>();
            SetupText(nameText, 22, TextAnchor.MiddleLeft);
            float chopsPerSec = 1f / Mathf.Max(0.01f, tier.cooldown);
            nameText.text = tier.tierName + "\nDegats " + tier.damage + " - Vitesse " + chopsPerSec.ToString("0.0") + "/s";

            var buyRt = CreateRect(rowRt, "Buy", new Vector2(0.62f, 0.15f), new Vector2(0.98f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var buyImg = buyRt.gameObject.AddComponent<Image>();
            var buyBtn = buyRt.gameObject.AddComponent<Button>();

            var buyLabelRt = CreateRect(buyRt, "Label", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var buyLabel = buyLabelRt.gameObject.AddComponent<Text>();
            SetupText(buyLabel, 20, TextAnchor.MiddleCenter);

            if (index <= shop.CurrentTierIndex)
            {
                buyImg.color = new Color(0.25f, 0.55f, 0.25f, 0.9f);
                buyLabel.text = "Equipee";
                buyBtn.interactable = false;
            }
            else if (index == shop.CurrentTierIndex + 1)
            {
                buyImg.color = new Color(0.75f, 0.6f, 0.15f, 0.9f);
                buyLabel.text = tier.cost + " $";
                buyBtn.onClick.AddListener(() => shop.TryBuyNext());
            }
            else
            {
                buyImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                buyLabel.text = "Verrouillee";
                buyBtn.interactable = false;
            }
        }

        private void RefreshShopRows(ShopManager shop)
        {
            for (int i = shopListParent.childCount - 1; i >= 0; i--)
            {
                var child = shopListParent.GetChild(i);
                if (child.name.StartsWith("Row"))
                    Destroy(child.gameObject);
            }

            for (int i = 0; i < shop.tiers.Count; i++)
                BuildShopRow(shopListParent, shop, i);
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
