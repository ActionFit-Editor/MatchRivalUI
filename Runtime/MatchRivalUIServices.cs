using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ActionFit.MatchRival.UI
{
    public interface IMatchRivalUILocalizer
    {
        string Get(string key, string fallback);
    }

    public interface IMatchRivalUIAudio
    {
        void Play(string cueId);
    }

    public interface IMatchRivalUIProfileProvider
    {
        MatchRivalUIProfile GetPlayerProfile();
    }

    public interface IMatchRivalUIRewardRenderer
    {
        string Render(
            IReadOnlyList<MatchRivalUIReward> roundRewards,
            IReadOnlyList<MatchRivalUIBoxReward> boxRewards,
            IMatchRivalUILocalizer localizer);
    }

    public interface IMatchRivalUIAnimation
    {
        void ScreenChanged(MatchRivalUIScreen previous, MatchRivalUIScreen current);
        void ProgressChanged(float previous, float current);
        void RewardPresented(MatchRivalResult result);
        void Reset();
    }

    public interface IMatchRivalUIClockDisplay
    {
        string Format(TimeSpan remaining);
    }

    public interface IMatchRivalUIViewHost
    {
        MatchRivalPresentation Create(MatchRivalPresentation prefab, Transform parent);
        void Release(MatchRivalPresentation presentation);
    }

    public sealed class PassthroughMatchRivalUILocalizer : IMatchRivalUILocalizer
    {
        public static PassthroughMatchRivalUILocalizer Instance { get; } = new();

        private PassthroughMatchRivalUILocalizer()
        {
        }

        public string Get(string key, string fallback) => fallback ?? string.Empty;
    }

    public sealed class NullMatchRivalUIAudio : IMatchRivalUIAudio
    {
        public static NullMatchRivalUIAudio Instance { get; } = new();

        private NullMatchRivalUIAudio()
        {
        }

        public void Play(string cueId)
        {
        }
    }

    public sealed class DefaultMatchRivalUIProfileProvider : IMatchRivalUIProfileProvider
    {
        public static DefaultMatchRivalUIProfileProvider Instance { get; } = new();

        private DefaultMatchRivalUIProfileProvider()
        {
        }

        public MatchRivalUIProfile GetPlayerProfile() =>
            new("player", "Player", string.Empty, string.Empty);
    }

    public sealed class TextMatchRivalUIRewardRenderer : IMatchRivalUIRewardRenderer
    {
        public static TextMatchRivalUIRewardRenderer Instance { get; } = new();

        private TextMatchRivalUIRewardRenderer()
        {
        }

        public string Render(
            IReadOnlyList<MatchRivalUIReward> roundRewards,
            IReadOnlyList<MatchRivalUIBoxReward> boxRewards,
            IMatchRivalUILocalizer localizer)
        {
            var builder = new StringBuilder();
            AppendRewards(builder, roundRewards, localizer);
            if (boxRewards != null)
            {
                for (int index = 0; index < boxRewards.Count; index++)
                {
                    MatchRivalUIBoxReward box = boxRewards[index];
                    if (builder.Length > 0) builder.AppendLine();
                    string state = box.Claimed ? "Claimed" : box.Available ? "Available" : "Locked";
                    builder.Append("Box ").Append(box.Stage).Append(" · ").Append(state);
                }
            }
            return builder.ToString();
        }

        private static void AppendRewards(
            StringBuilder builder,
            IReadOnlyList<MatchRivalUIReward> rewards,
            IMatchRivalUILocalizer localizer)
        {
            if (rewards == null) return;
            for (int index = 0; index < rewards.Count; index++)
            {
                if (builder.Length > 0) builder.Append("  |  ");
                MatchRivalUIReward reward = rewards[index];
                string name = localizer?.Get(
                    MatchRivalUIKeys.RewardName(reward.RewardId),
                    reward.RewardId) ?? reward.RewardId;
                builder.Append(name).Append(" x").Append(reward.Amount);
            }
        }
    }

    public sealed class NullMatchRivalUIAnimation : IMatchRivalUIAnimation
    {
        public static NullMatchRivalUIAnimation Instance { get; } = new();

        private NullMatchRivalUIAnimation()
        {
        }

        public void ScreenChanged(MatchRivalUIScreen previous, MatchRivalUIScreen current) { }
        public void ProgressChanged(float previous, float current) { }
        public void RewardPresented(MatchRivalResult result) { }
        public void Reset() { }
    }

    public sealed class DefaultMatchRivalUIClockDisplay : IMatchRivalUIClockDisplay
    {
        public static DefaultMatchRivalUIClockDisplay Instance { get; } = new();

        private DefaultMatchRivalUIClockDisplay()
        {
        }

        public string Format(TimeSpan remaining)
        {
            TimeSpan safe = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
            return safe.TotalDays >= 1d
                ? $"{(int)safe.TotalDays}d {safe.Hours:00}:{safe.Minutes:00}:{safe.Seconds:00}"
                : $"{(int)safe.TotalHours:00}:{safe.Minutes:00}:{safe.Seconds:00}";
        }
    }

    public sealed class DefaultMatchRivalUIViewHost : IMatchRivalUIViewHost
    {
        public static DefaultMatchRivalUIViewHost Instance { get; } = new();

        private DefaultMatchRivalUIViewHost()
        {
        }

        public MatchRivalPresentation Create(MatchRivalPresentation prefab, Transform parent)
        {
            if (prefab != null) return UnityEngine.Object.Instantiate(prefab, parent, false);
            var root = new GameObject("Match Rival Presentation");
            if (parent != null) root.transform.SetParent(parent, false);
            return root.AddComponent<MatchRivalPresentation>();
        }

        public void Release(MatchRivalPresentation presentation)
        {
            if (presentation == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(presentation.gameObject);
            else UnityEngine.Object.DestroyImmediate(presentation.gameObject);
        }
    }

    public static class MatchRivalUIKeys
    {
        public const string Title = "match_rival.ui.title";
        public const string AudioScreen = "match_rival.ui.audio.screen";
        public const string AudioProgress = "match_rival.ui.audio.progress";
        public const string AudioReward = "match_rival.ui.audio.reward";
        public const string ScreenEventStart = "match_rival.ui.screen.event_start";
        public const string ScreenMatchStart = "match_rival.ui.screen.match_start";
        public const string ScreenTutorial = "match_rival.ui.screen.tutorial";
        public const string ScreenMatch = "match_rival.ui.screen.match";
        public const string ScreenWin = "match_rival.ui.screen.win";
        public const string ScreenLose = "match_rival.ui.screen.lose";
        public const string ScreenRewardRoad = "match_rival.ui.screen.reward_road";
        public const string ScreenEventEnd = "match_rival.ui.screen.event_end";

        public static string RewardName(string rewardId) =>
            "match_rival.ui.reward." + (rewardId ?? string.Empty);
    }
}
