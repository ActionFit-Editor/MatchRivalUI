using System;
using ReferenceBinding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ActionFit.MatchRival.UI
{
    /// <summary>UI Foundation presentation for immutable MatchRival engine projections.</summary>
    [AddComponentMenu("ActionFit/Match Rival Presentation")]
    public sealed class MatchRivalPresentation : MonoBehaviour
    {
        [Serializable]
        public sealed class Assets
        {
            [SerializeField] private MatchRivalUIThemeAsset themeAsset;

            public MatchRivalUIThemeAsset ThemeAsset => themeAsset;
        }

        [Serializable]
        public sealed class Settings
        {
            [SerializeField] private MatchRivalUITheme theme = new();
            [SerializeField, Min(0.05f)] private float refreshIntervalSeconds = 0.25f;
            [SerializeField, Min(1)] private int demoBeanAmount = 10;

            public MatchRivalUITheme Theme => theme ?? new MatchRivalUITheme();
            public float RefreshIntervalSeconds => Mathf.Max(0.05f, refreshIntervalSeconds);
            public int DemoBeanAmount => Mathf.Max(1, demoBeanAmount);
        }

        [Serializable]
        public sealed class Refs
        {
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_PANEL_MISSING"), AutoWireChild("Panel")]
            private UI_Rect panel;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_BACKDROP_MISSING"), AutoWireChild("Backdrop")]
            private UI_Image backdrop;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_TITLE_MISSING"), AutoWireChild("Title")]
            private UI_Text title;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_SCREEN_MISSING"), AutoWireChild("Screen")]
            private UI_Text screen;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_STATUS_MISSING"), AutoWireChild("Status")]
            private UI_Text status;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_TIMER_MISSING"), AutoWireChild("Timer")]
            private UI_Text timer;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_PLAYER_PROGRESS_MISSING"), AutoWireChild("PlayerProgress")]
            private UI_Image playerProgress;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_RIVAL_PROGRESS_MISSING"), AutoWireChild("RivalProgress")]
            private UI_Image rivalProgress;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_REWARDS_MISSING"), AutoWireChild("Rewards")]
            private UI_Text rewards;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_PRIMARY_BUTTON_MISSING"), AutoWireChild("PrimaryButton")]
            private UI_Button primaryButton;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_PRIMARY_LABEL_MISSING"), AutoWireChild("PrimaryLabel")]
            private UI_Text primaryLabel;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_SECONDARY_BUTTON_MISSING"), AutoWireChild("SecondaryButton")]
            private UI_Button secondaryButton;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_SECONDARY_LABEL_MISSING"), AutoWireChild("SecondaryLabel")]
            private UI_Text secondaryLabel;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_TERTIARY_BUTTON_MISSING"), AutoWireChild("TertiaryButton")]
            private UI_Button tertiaryButton;
            [SerializeField, RequiredReference("MATCH_RIVAL_UI_TERTIARY_LABEL_MISSING"), AutoWireChild("TertiaryLabel")]
            private UI_Text tertiaryLabel;

            internal Refs()
            {
            }

            internal Refs(
                UI_Rect panel,
                UI_Image backdrop,
                UI_Text title,
                UI_Text screen,
                UI_Text status,
                UI_Text timer,
                UI_Image playerProgress,
                UI_Image rivalProgress,
                UI_Text rewards,
                UI_Button primaryButton,
                UI_Text primaryLabel,
                UI_Button secondaryButton,
                UI_Text secondaryLabel,
                UI_Button tertiaryButton,
                UI_Text tertiaryLabel)
            {
                this.panel = panel;
                this.backdrop = backdrop;
                this.title = title;
                this.screen = screen;
                this.status = status;
                this.timer = timer;
                this.playerProgress = playerProgress;
                this.rivalProgress = rivalProgress;
                this.rewards = rewards;
                this.primaryButton = primaryButton;
                this.primaryLabel = primaryLabel;
                this.secondaryButton = secondaryButton;
                this.secondaryLabel = secondaryLabel;
                this.tertiaryButton = tertiaryButton;
                this.tertiaryLabel = tertiaryLabel;
            }

            public UI_Rect Panel => panel;
            public UI_Image Backdrop => backdrop;
            public UI_Text Title => title;
            public UI_Text Screen => screen;
            public UI_Text Status => status;
            public UI_Text Timer => timer;
            public UI_Image PlayerProgress => playerProgress;
            public UI_Image RivalProgress => rivalProgress;
            public UI_Text Rewards => rewards;
            public UI_Button PrimaryButton => primaryButton;
            public UI_Text PrimaryLabel => primaryLabel;
            public UI_Button SecondaryButton => secondaryButton;
            public UI_Text SecondaryLabel => secondaryLabel;
            public UI_Button TertiaryButton => tertiaryButton;
            public UI_Text TertiaryLabel => tertiaryLabel;

            public bool IsComplete => panel != null
                && backdrop != null
                && title != null
                && screen != null
                && status != null
                && timer != null
                && playerProgress != null
                && rivalProgress != null
                && rewards != null
                && primaryButton != null
                && primaryLabel != null
                && secondaryButton != null
                && secondaryLabel != null
                && tertiaryButton != null
                && tertiaryLabel != null;
        }

        [SerializeField] private Assets assets = new();
        [SerializeField] private Settings settings = new();
        [SerializeField] private Refs refs = new();

        private IMatchRivalUILocalizer _localizer;
        private IMatchRivalUIAudio _audio;
        private IMatchRivalUIProfileProvider _profileProvider;
        private IMatchRivalUIRewardRenderer _rewardRenderer;
        private IMatchRivalUIAnimation _animation;
        private IMatchRivalUIClockDisplay _clockDisplay;
        private Refs _runtimeRefs;
        private MatchRivalUIViewModel _currentModel;
        private MatchRivalUIAction _primaryAction;
        private MatchRivalUIAction _secondaryAction;
        private MatchRivalUIAction _tertiaryAction;
        private bool _initialized;

        public event Action<MatchRivalUIAction> ActionRequested;

        public MatchRivalUITheme Theme => assets?.ThemeAsset != null
            ? assets.ThemeAsset.Theme
            : settings?.Theme ?? new MatchRivalUITheme();
        public Settings Config => settings ?? new Settings();
        public MatchRivalUIViewModel CurrentModel => _currentModel;
        public bool IsInitialized => _initialized;
        public Refs InspectorReferences => refs;

#if UNITY_EDITOR
        private void OnValidate()
        {
            ReferenceBindingRequests.Enqueue(this);
        }
#endif

        private void OnDisable()
        {
            _animation?.Reset();
        }

        private void OnDestroy()
        {
            if (!_initialized || _runtimeRefs == null) return;
            _runtimeRefs.PrimaryButton.RemoveListener(HandlePrimary);
            _runtimeRefs.SecondaryButton.RemoveListener(HandleSecondary);
            _runtimeRefs.TertiaryButton.RemoveListener(HandleTertiary);
        }

        public void Initialize(
            IMatchRivalUILocalizer localizer = null,
            IMatchRivalUIAudio audio = null,
            IMatchRivalUIProfileProvider profileProvider = null,
            IMatchRivalUIRewardRenderer rewardRenderer = null,
            IMatchRivalUIAnimation animation = null,
            IMatchRivalUIClockDisplay clockDisplay = null)
        {
            if (_initialized) return;
            _localizer = localizer ?? PassthroughMatchRivalUILocalizer.Instance;
            _audio = audio ?? NullMatchRivalUIAudio.Instance;
            _profileProvider = profileProvider ?? DefaultMatchRivalUIProfileProvider.Instance;
            _rewardRenderer = rewardRenderer ?? TextMatchRivalUIRewardRenderer.Instance;
            _animation = animation ?? NullMatchRivalUIAnimation.Instance;
            _clockDisplay = clockDisplay ?? DefaultMatchRivalUIClockDisplay.Instance;
            _runtimeRefs = refs != null && refs.IsComplete ? refs : BuildDefaultView();
            _runtimeRefs.PrimaryButton.AddListener(HandlePrimary);
            _runtimeRefs.SecondaryButton.AddListener(HandleSecondary);
            _runtimeRefs.TertiaryButton.AddListener(HandleTertiary);
            ApplyTheme();
            _initialized = true;
        }

        public void Present(MatchRivalUIViewModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            Initialize();

            MatchRivalUIScreen previousScreen = _currentModel?.Screen ?? model.Screen;
            float previousProgress = _currentModel?.PlayerProgress ?? model.PlayerProgress;
            bool screenChanged = _currentModel == null || previousScreen != model.Screen;
            bool progressChanged = _currentModel != null
                && !Mathf.Approximately(previousProgress, model.PlayerProgress);
            bool rewardPresented = screenChanged
                && (model.Screen == MatchRivalUIScreen.Win || model.Screen == MatchRivalUIScreen.Lose);
            _currentModel = model;

            MatchRivalUIProfile player = _profileProvider.GetPlayerProfile()
                ?? DefaultMatchRivalUIProfileProvider.Instance.GetPlayerProfile();
            _runtimeRefs.Title.Text = Localize(MatchRivalUIKeys.Title, "Match Rival");
            _runtimeRefs.Screen.Text = GetScreenTitle(model.Screen);
            _runtimeRefs.Status.Text = BuildStatus(model, player);
            _runtimeRefs.Timer.Text = BuildTimer(model);
            _runtimeRefs.PlayerProgress.FillAmount = model.PlayerProgress;
            _runtimeRefs.RivalProgress.FillAmount = model.RivalProgress;
            _runtimeRefs.Rewards.Text = _rewardRenderer.Render(
                model.RoundRewards,
                model.BoxRewards,
                _localizer);
            ConfigureButton(
                _runtimeRefs.PrimaryButton,
                _runtimeRefs.PrimaryLabel,
                model.Primary,
                out _primaryAction);
            ConfigureButton(
                _runtimeRefs.SecondaryButton,
                _runtimeRefs.SecondaryLabel,
                model.Secondary,
                out _secondaryAction);
            ConfigureButton(
                _runtimeRefs.TertiaryButton,
                _runtimeRefs.TertiaryLabel,
                model.Tertiary,
                out _tertiaryAction);

            if (screenChanged)
            {
                _animation.ScreenChanged(previousScreen, model.Screen);
                _audio.Play(MatchRivalUIKeys.AudioScreen);
            }
            if (progressChanged)
            {
                _animation.ProgressChanged(previousProgress, model.PlayerProgress);
                _audio.Play(MatchRivalUIKeys.AudioProgress);
            }
            if (rewardPresented)
            {
                _animation.RewardPresented(model.Result);
                _audio.Play(MatchRivalUIKeys.AudioReward);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _animation?.Reset();
            gameObject.SetActive(false);
        }

        private void HandlePrimary() => Request(_primaryAction);
        private void HandleSecondary() => Request(_secondaryAction);
        private void HandleTertiary() => Request(_tertiaryAction);

        private void Request(MatchRivalUIAction action)
        {
            if (action != MatchRivalUIAction.None) ActionRequested?.Invoke(action);
        }

        private void ConfigureButton(
            UI_Button button,
            UI_Text label,
            MatchRivalUIButtonModel model,
            out MatchRivalUIAction action)
        {
            model ??= MatchRivalUIButtonModel.Hidden;
            button.gameObject.SetActive(model.Visible);
            if (model.Visible) button.SetInteractable(model.Interactable);
            label.Text = Localize("match_rival.ui." + model.Action.ToString().ToLowerInvariant(), model.Label);
            action = model.Visible && model.Interactable ? model.Action : MatchRivalUIAction.None;
        }

        private string GetScreenTitle(MatchRivalUIScreen screen)
        {
            return screen switch
            {
                MatchRivalUIScreen.EventStart => Localize(MatchRivalUIKeys.ScreenEventStart, "Event Start"),
                MatchRivalUIScreen.MatchStart => Localize(MatchRivalUIKeys.ScreenMatchStart, "Find Rival"),
                MatchRivalUIScreen.Tutorial => Localize(MatchRivalUIKeys.ScreenTutorial, "How To Play"),
                MatchRivalUIScreen.Match => Localize(MatchRivalUIKeys.ScreenMatch, "Match"),
                MatchRivalUIScreen.Win => Localize(MatchRivalUIKeys.ScreenWin, "Victory"),
                MatchRivalUIScreen.Lose => Localize(MatchRivalUIKeys.ScreenLose, "Defeat"),
                MatchRivalUIScreen.RewardRoad => Localize(MatchRivalUIKeys.ScreenRewardRoad, "Rewards"),
                MatchRivalUIScreen.EventEnd => Localize(MatchRivalUIKeys.ScreenEventEnd, "Event End"),
                _ => screen.ToString(),
            };
        }

        private string BuildStatus(MatchRivalUIViewModel model, MatchRivalUIProfile player)
        {
            string rivalName = model.Rival?.DisplayName ?? "Rival";
            string message = string.IsNullOrWhiteSpace(model.Message) ? string.Empty : "\n" + model.Message;
            return $"Stage {model.Stage} · {(model.Hard ? "Hard" : "Easy")}\n"
                + $"{player.DisplayName} {model.PlayerBeans}/{model.RequiredBeans}  VS  "
                + $"{rivalName} {model.RivalBeans}/{model.RequiredBeans}{message}";
        }

        private string BuildTimer(MatchRivalUIViewModel model)
        {
            TimeSpan remaining = model.Screen == MatchRivalUIScreen.Match
                ? model.RivalRemaining
                : model.EventRemaining;
            return _clockDisplay.Format(remaining);
        }

        private string Localize(string key, string fallback) =>
            _localizer?.Get(key, fallback) ?? fallback ?? string.Empty;

        private void ApplyTheme()
        {
            MatchRivalUITheme theme = Theme;
            _runtimeRefs.Backdrop.Color = theme.Backdrop;
            if (_runtimeRefs.Panel.TryGetComponent(out Image panelImage)) panelImage.color = theme.Panel;
            _runtimeRefs.Title.SetColor(theme.Text);
            _runtimeRefs.Screen.SetColor(theme.Player);
            _runtimeRefs.Status.SetColor(theme.Text);
            _runtimeRefs.Timer.SetColor(theme.SecondaryText);
            _runtimeRefs.Rewards.SetColor(theme.SecondaryText);
            _runtimeRefs.PlayerProgress.Color = theme.Player;
            _runtimeRefs.RivalProgress.Color = theme.Rival;
            _runtimeRefs.PrimaryButton.Color = theme.PrimaryButton;
        }

        private Refs BuildDefaultView()
        {
            var canvasObject = new GameObject(
                "MatchRivalCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            UI_Image backdrop = Create<UI_Image>("Backdrop", canvasObject.transform);
            Stretch(backdrop.RectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UI_Rect panel = Create<UI_Rect>("Panel", canvasObject.transform);
            Image panelImage = panel.gameObject.AddComponent<Image>();
            RectTransform panelRect = panel.RectTransform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(840f, 1280f);
            panelImage.raycastTarget = true;

            UI_Text title = CreateText("Title", panelRect, new Vector2(0f, 520f), 54);
            UI_Text screen = CreateText("Screen", panelRect, new Vector2(0f, 430f), 42);
            UI_Text status = CreateText("Status", panelRect, new Vector2(0f, 250f), 30);
            UI_Text timer = CreateText("Timer", panelRect, new Vector2(0f, 115f), 34);
            UI_Image playerProgress = CreateProgress("PlayerProgress", panelRect, new Vector2(0f, 10f));
            UI_Image rivalProgress = CreateProgress("RivalProgress", panelRect, new Vector2(0f, -55f));
            UI_Text rewards = CreateText("Rewards", panelRect, new Vector2(0f, -235f), 26);
            ((RectTransform)rewards.transform).sizeDelta = new Vector2(700f, 240f);

            UI_Button primaryButton = CreateButton("PrimaryButton", panelRect, new Vector2(0f, -430f));
            UI_Text primaryLabel = CreateButtonLabel("PrimaryLabel", primaryButton.transform);
            UI_Button secondaryButton = CreateButton("SecondaryButton", panelRect, new Vector2(-205f, -530f));
            UI_Text secondaryLabel = CreateButtonLabel("SecondaryLabel", secondaryButton.transform);
            UI_Button tertiaryButton = CreateButton("TertiaryButton", panelRect, new Vector2(205f, -530f));
            UI_Text tertiaryLabel = CreateButtonLabel("TertiaryLabel", tertiaryButton.transform);

            return new Refs(
                panel,
                backdrop,
                title,
                screen,
                status,
                timer,
                playerProgress,
                rivalProgress,
                rewards,
                primaryButton,
                primaryLabel,
                secondaryButton,
                secondaryLabel,
                tertiaryButton,
                tertiaryLabel);
        }

        private static T Create<T>(string objectName, Transform parent) where T : Component
        {
            var child = new GameObject(objectName, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.AddComponent<T>();
        }

        private static UI_Text CreateText(string objectName, RectTransform parent, Vector2 position, int size)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            UI_Text text = textObject.AddComponent<UI_Text>();
            RectTransform rect = (RectTransform)text.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(720f, 100f);
            text.SetSize(size);
            return text;
        }

        private static UI_Image CreateProgress(string objectName, RectTransform parent, Vector2 position)
        {
            UI_Image image = Create<UI_Image>(objectName, parent);
            RectTransform rect = image.RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(650f, 34f);
            image.Image.type = Image.Type.Filled;
            image.Image.fillMethod = Image.FillMethod.Horizontal;
            image.Image.fillOrigin = 0;
            return image;
        }

        private static UI_Button CreateButton(string objectName, RectTransform parent, Vector2 position)
        {
            UI_Button button = Create<UI_Button>(objectName, parent);
            RectTransform rect = button.RectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(360f, 86f);
            return button;
        }

        private static UI_Text CreateButtonLabel(string objectName, Transform parent)
        {
            var labelObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            UI_Text label = labelObject.AddComponent<UI_Text>();
            Stretch((RectTransform)label.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            label.SetSize(28);
            return label;
        }

        private static void Stretch(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
