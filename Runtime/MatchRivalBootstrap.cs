using System;
using System.Collections.Generic;
using ActionFit.Content;
using ActionFit.Time;
using UnityEngine;

namespace ActionFit.MatchRival.UI
{
    /// <summary>Standalone composition root and public-engine command router.</summary>
    [AddComponentMenu("ActionFit/Match Rival Bootstrap")]
    public sealed class MatchRivalBootstrap : MonoBehaviour
    {
        public const string DefaultDemoContentId = "match-rival-ui-demo";

        [SerializeField] private MatchRivalPresentation presentationPrefab;
        [SerializeField] private string contentId = DefaultDemoContentId;
        [SerializeField] private bool initializeOnStart = true;

        private MatchRivalEngine _engine;
        private MatchRivalPresentation _presentation;
        private MatchRivalUIViewHostBase _viewHost;
        private MatchRivalUILocalizerBase _localizer;
        private bool _ownsPresentation;
        private bool _rendering;
        private bool _renderQueued;
        private float _nextRefreshTime;
        private MatchRivalUIScreen? _requestedScreen;
        private string _message = string.Empty;

        public event Action CloseRequested;

        public MatchRivalEngine Engine => _engine;
        public MatchRivalPresentation Presentation => _presentation;
        public bool IsInitialized => _engine != null && _presentation != null;
        public bool IsVisible => IsInitialized && _presentation.gameObject.activeSelf;

        private void Start()
        {
            if (initializeOnStart && !IsInitialized) InitializeDefault();
        }

        private void Update()
        {
            if (!IsVisible || UnityEngine.Time.unscaledTime < _nextRefreshTime) return;
            _nextRefreshTime = UnityEngine.Time.unscaledTime + _presentation.Config.RefreshIntervalSeconds;
            _engine.EvaluateTimeout();
            Render();
        }

        private void OnDestroy()
        {
            if (_engine != null) _engine.StateChanged -= HandleStateChanged;
            if (_presentation != null) _presentation.ActionRequested -= HandleActionRequested;
            if (_ownsPresentation && _viewHost != null && _presentation != null)
                _viewHost.Release(_presentation);
        }

        public void InitializeDefault(
            MatchRivalPresentation presentation = null,
            MatchRivalUILocalizerBase localizer = null,
            MatchRivalUIAudioBase audio = null,
            MatchRivalUIProfileProviderBase profileProvider = null,
            MatchRivalUIRewardRendererBase rewardRenderer = null,
            MatchRivalUIAnimationBase animation = null,
            MatchRivalUIClockDisplayBase clockDisplay = null,
            MatchRivalUIViewHostBase viewHost = null)
        {
            string safeContentId = string.IsNullOrWhiteSpace(contentId)
                ? DefaultDemoContentId
                : contentId.Trim();
            Initialize(
                CreateDefaultEngine(safeContentId),
                presentation,
                localizer,
                audio,
                profileProvider,
                rewardRenderer,
                animation,
                clockDisplay,
                viewHost);
        }

        public void Initialize(
            MatchRivalEngine engine,
            MatchRivalPresentation presentation = null,
            MatchRivalUILocalizerBase localizer = null,
            MatchRivalUIAudioBase audio = null,
            MatchRivalUIProfileProviderBase profileProvider = null,
            MatchRivalUIRewardRendererBase rewardRenderer = null,
            MatchRivalUIAnimationBase animation = null,
            MatchRivalUIClockDisplayBase clockDisplay = null,
            MatchRivalUIViewHostBase viewHost = null)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (IsInitialized) throw new InvalidOperationException("MatchRival UI is already initialized.");

            _engine = engine;
            _localizer = localizer ?? PassthroughMatchRivalUILocalizer.Instance;
            _viewHost = viewHost ?? DefaultMatchRivalUIViewHost.Instance;
            _presentation = presentation;
            if (_presentation == null)
            {
                _presentation = _viewHost.Create(presentationPrefab, transform);
                _ownsPresentation = true;
            }
            if (_presentation == null)
            {
                _engine = null;
                throw new InvalidOperationException("The MatchRival view host returned no presentation.");
            }

            _presentation.Initialize(
                _localizer,
                audio,
                profileProvider,
                rewardRenderer,
                animation,
                clockDisplay);
            _presentation.ActionRequested += HandleActionRequested;
            _engine.StateChanged += HandleStateChanged;
            _engine.Restore();
            Render();
        }

        public void Show()
        {
            if (!IsInitialized) throw new InvalidOperationException("MatchRival UI is not initialized.");
            _presentation.Show();
            Render();
        }

        public static MatchRivalEngine CreateDefaultEngine(string contentId = DefaultDemoContentId)
        {
            ClockBase clock = SystemClock.Instance;
            MatchRivalCatalog catalog = CreateDemoCatalog();
            return new MatchRivalEngine(
                new PlayerPrefsContentStateStore(),
                new PlayerPrefsContentRewardService("com.actionfit.match-rival.ui.demo-rewards"),
                new SingleCatalogResolver(catalog),
                clock,
                TimeZoneInfo.Local,
                TimeZoneInfo.Local,
                new SystemMatchRivalRandom(),
                new LinearMatchRivalProgressCurveProvider(),
                new DefaultMatchRivalOpponentProvider(),
                string.IsNullOrWhiteSpace(contentId) ? DefaultDemoContentId : contentId,
                new AllowMatchRivalAccessPolicy(),
                new DemoSchedulePolicy());
        }

        private void HandleStateChanged(MatchRivalState state)
        {
            if (_rendering)
            {
                _renderQueued = true;
                return;
            }
            Render();
        }

        private void HandleActionRequested(MatchRivalUIAction action)
        {
            _requestedScreen = null;
            switch (action)
            {
                case MatchRivalUIAction.StartEvent:
                    _message = _engine.TryStartEvent()
                        ? Localize("match_rival.ui.status.event_started", "Event started.")
                        : Localize("match_rival.ui.status.event_unavailable", "The event is unavailable.");
                    break;
                case MatchRivalUIAction.DismissTutorial:
                    _engine.SetTutorialDone(true);
                    _message = Localize("match_rival.ui.status.tutorial_done", "Tutorial completed.");
                    break;
                case MatchRivalUIAction.StartMatch:
                    _message = _engine.StartMatch()
                        ? Localize("match_rival.ui.status.match_started", "Rival found.")
                        : Localize("match_rival.ui.status.match_unavailable", "The match could not start.");
                    break;
                case MatchRivalUIAction.AddBeans:
                    _message = _engine.AddBeans(_presentation.Config.DemoBeanAmount)
                        ? Localize("match_rival.ui.status.beans_added", "Beans added through the engine.")
                        : Localize("match_rival.ui.status.beans_unavailable", "Beans cannot be added now.");
                    break;
                case MatchRivalUIAction.ForceWin:
                    SetDemoResult(_engine.ForceWin(), MatchRivalResult.Win);
                    break;
                case MatchRivalUIAction.ForceLose:
                    SetDemoResult(_engine.ForceLose(), MatchRivalResult.Lose);
                    break;
                case MatchRivalUIAction.ClaimRoundReward:
                    ClaimRoundReward();
                    break;
                case MatchRivalUIAction.ClaimBoxReward:
                    ClaimFirstAvailableBox();
                    break;
                case MatchRivalUIAction.OpenRewardRoad:
                    _requestedScreen = MatchRivalUIScreen.RewardRoad;
                    break;
                case MatchRivalUIAction.Back:
                    break;
                case MatchRivalUIAction.EndEvent:
                    _engine.EndEvent();
                    _message = Localize("match_rival.ui.status.event_ended", "Event ended.");
                    break;
                case MatchRivalUIAction.Close:
                    _presentation.Hide();
                    CloseRequested?.Invoke();
                    return;
            }
            Render();
        }

        private void SetDemoResult(bool succeeded, MatchRivalResult result)
        {
            if (!succeeded)
            {
                _message = Localize("match_rival.ui.status.result_unavailable", "The result is unavailable.");
                return;
            }
            _message = result == MatchRivalResult.Win
                ? Localize("match_rival.ui.status.win", "Victory resolved by the engine.")
                : Localize("match_rival.ui.status.lose", "Defeat resolved by the engine.");
        }

        private void ClaimRoundReward()
        {
            int stage = _engine.Stage;
            MatchRivalResult result = _engine.PendingResult != MatchRivalResult.None
                ? _engine.PendingResult
                : _engine.CurrentResult;
            if (result != MatchRivalResult.None) _engine.PrepareResultReward(result);
            MatchRivalRoundClaimResult claim = _engine.ClaimPendingResultReward();
            if (!claim.Succeeded)
            {
                _message = Localize("match_rival.ui.status.reward_unavailable", "Reward is unavailable.");
                return;
            }
            if (result == MatchRivalResult.Win && stage >= MatchRivalEngine.MaxStage)
                _engine.MarkPendingEnd();
            _message = Localize("match_rival.ui.status.reward_claimed", "Reward claimed.");
        }

        private void ClaimFirstAvailableBox()
        {
            MatchRivalUIViewModel model = MatchRivalUIViewModelFactory.Create(_engine, DateTime.Now);
            for (int index = 0; index < model.BoxRewards.Count; index++)
            {
                MatchRivalUIBoxReward box = model.BoxRewards[index];
                if (!box.Available) continue;
                _message = _engine.ClaimBoxReward(box.Stage)
                    ? Localize("match_rival.ui.status.box_claimed", "Box reward claimed.")
                    : Localize("match_rival.ui.status.box_unavailable", "Box reward is unavailable.");
                return;
            }
            _message = Localize("match_rival.ui.status.box_unavailable", "Box reward is unavailable.");
        }

        private void Render()
        {
            if (!IsInitialized) return;
            if (_rendering)
            {
                _renderQueued = true;
                return;
            }

            do
            {
                _renderQueued = false;
                _rendering = true;
                try
                {
                    _presentation.Present(MatchRivalUIViewModelFactory.Create(
                        _engine,
                        DateTime.Now,
                        _requestedScreen,
                        _message));
                }
                finally
                {
                    _rendering = false;
                }
            }
            while (_renderQueued);
        }

        private string Localize(string key, string fallback) =>
            _localizer?.Get(key, fallback) ?? fallback ?? string.Empty;

        private static MatchRivalCatalog CreateDemoCatalog()
        {
            var difficulties = new List<MatchRivalDifficulty>();
            var orderBeans = new List<KeyValuePair<int, int>>
            {
                new(1, 5),
                new(2, 10),
                new(3, 15),
            };
            var roundRewards = new List<MatchRivalRoundRewards>();
            var boxRewards = new List<MatchRivalBoxRewards>();
            for (int stage = MatchRivalEngine.MinStage; stage <= MatchRivalEngine.MaxStage; stage++)
            {
                difficulties.Add(new MatchRivalDifficulty(stage, 30f, 40f, 20f, 30f, 20 + stage * 5));
                roundRewards.Add(new MatchRivalRoundRewards(
                    stage,
                    new[] { new ContentReward("coin", stage * 100L) },
                    new[] { new ContentReward("coin", stage * 25L) }));
                if (stage % 2 == 0)
                    boxRewards.Add(new MatchRivalBoxRewards(
                        stage,
                        new[] { new ContentReward("gem", stage) }));
            }
            return new MatchRivalCatalog(
                "match-rival-ui-demo-v1",
                "match-rival-ui-demo-balance-v1",
                difficulties,
                orderBeans,
                roundRewards,
                boxRewards);
        }

        private sealed class SingleCatalogResolver : MatchRivalCatalogResolverBase
        {
            private readonly MatchRivalCatalog _catalog;

            public SingleCatalogResolver(MatchRivalCatalog catalog)
            {
                _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            }

            public override MatchRivalCatalog Current => _catalog;

            public override bool TryResolve(
                string catalogVersion,
                string balanceRevision,
                out MatchRivalCatalog catalog)
            {
                bool matches = string.Equals(catalogVersion, _catalog.CatalogVersion, StringComparison.Ordinal)
                    && string.Equals(balanceRevision, _catalog.BalanceRevision, StringComparison.Ordinal);
                catalog = matches ? _catalog : null;
                return matches;
            }
        }

        private sealed class DemoSchedulePolicy : MatchRivalSchedulePolicyBase
        {
            public override bool IsEnabled => true;
            public override bool IsActiveDay(DayOfWeek dayOfWeek) => true;
            public override DateTime GetActiveWindowEnd(DateTime now) => now.AddDays(7d);
        }
    }
}
