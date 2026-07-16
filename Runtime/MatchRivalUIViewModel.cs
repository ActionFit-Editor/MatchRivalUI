using System;
using System.Collections.Generic;
using ActionFit.Content;

namespace ActionFit.MatchRival.UI
{
    public enum MatchRivalUIScreen
    {
        EventStart = 0,
        MatchStart = 1,
        Tutorial = 2,
        Match = 3,
        Win = 4,
        Lose = 5,
        RewardRoad = 6,
        EventEnd = 7,
    }

    public enum MatchRivalUIAction
    {
        None = 0,
        StartEvent = 1,
        StartMatch = 2,
        DismissTutorial = 3,
        AddBeans = 4,
        ForceWin = 5,
        ForceLose = 6,
        ClaimRoundReward = 7,
        ClaimBoxReward = 8,
        OpenRewardRoad = 9,
        Back = 10,
        EndEvent = 11,
        Close = 12,
    }

    public sealed class MatchRivalUIButtonModel
    {
        public MatchRivalUIButtonModel(
            MatchRivalUIAction action,
            string label,
            bool visible = true,
            bool interactable = true)
        {
            Action = action;
            Label = label ?? string.Empty;
            Visible = visible;
            Interactable = interactable;
        }

        public MatchRivalUIAction Action { get; }
        public string Label { get; }
        public bool Visible { get; }
        public bool Interactable { get; }

        public static MatchRivalUIButtonModel Hidden { get; } =
            new(MatchRivalUIAction.None, string.Empty, false, false);
    }

    public sealed class MatchRivalUIReward
    {
        public MatchRivalUIReward(string rewardId, long amount)
        {
            RewardId = rewardId ?? string.Empty;
            Amount = Math.Max(0L, amount);
        }

        public string RewardId { get; }
        public long Amount { get; }
    }

    public sealed class MatchRivalUIBoxReward
    {
        public MatchRivalUIBoxReward(
            int stage,
            IReadOnlyList<MatchRivalUIReward> rewards,
            bool claimed,
            bool available)
        {
            Stage = Math.Max(MatchRivalEngine.MinStage, Math.Min(MatchRivalEngine.MaxStage, stage));
            Rewards = rewards ?? Array.Empty<MatchRivalUIReward>();
            Claimed = claimed;
            Available = available;
        }

        public int Stage { get; }
        public IReadOnlyList<MatchRivalUIReward> Rewards { get; }
        public bool Claimed { get; }
        public bool Available { get; }
    }

    public sealed class MatchRivalUIProfile
    {
        public MatchRivalUIProfile(string id, string displayName, string profileId, string frameId)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            ProfileId = profileId ?? string.Empty;
            FrameId = frameId ?? string.Empty;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string ProfileId { get; }
        public string FrameId { get; }
    }

    /// <summary>Immutable rendering snapshot copied from public MatchRival engine reads.</summary>
    public sealed class MatchRivalUIViewModel
    {
        public MatchRivalUIViewModel(
            MatchRivalUIScreen screen,
            int stage,
            bool hard,
            int playerBeans,
            int rivalBeans,
            int requiredBeans,
            TimeSpan eventRemaining,
            TimeSpan rivalRemaining,
            MatchRivalResult result,
            bool tutorialDone,
            bool rewardServiceAvailable,
            string message,
            MatchRivalUIProfile rival,
            IReadOnlyList<MatchRivalUIReward> roundRewards,
            IReadOnlyList<MatchRivalUIBoxReward> boxRewards,
            MatchRivalUIButtonModel primary,
            MatchRivalUIButtonModel secondary,
            MatchRivalUIButtonModel tertiary)
        {
            Screen = screen;
            Stage = Math.Max(MatchRivalEngine.MinStage, Math.Min(MatchRivalEngine.MaxStage, stage));
            Hard = hard;
            RequiredBeans = Math.Max(1, requiredBeans);
            PlayerBeans = Math.Max(0, Math.Min(RequiredBeans, playerBeans));
            RivalBeans = Math.Max(0, Math.Min(RequiredBeans, rivalBeans));
            EventRemaining = eventRemaining < TimeSpan.Zero ? TimeSpan.Zero : eventRemaining;
            RivalRemaining = rivalRemaining < TimeSpan.Zero ? TimeSpan.Zero : rivalRemaining;
            Result = result;
            TutorialDone = tutorialDone;
            RewardServiceAvailable = rewardServiceAvailable;
            Message = message ?? string.Empty;
            Rival = rival;
            RoundRewards = roundRewards ?? Array.Empty<MatchRivalUIReward>();
            BoxRewards = boxRewards ?? Array.Empty<MatchRivalUIBoxReward>();
            Primary = primary ?? MatchRivalUIButtonModel.Hidden;
            Secondary = secondary ?? MatchRivalUIButtonModel.Hidden;
            Tertiary = tertiary ?? MatchRivalUIButtonModel.Hidden;
        }

        public MatchRivalUIScreen Screen { get; }
        public int Stage { get; }
        public bool Hard { get; }
        public int PlayerBeans { get; }
        public int RivalBeans { get; }
        public int RequiredBeans { get; }
        public TimeSpan EventRemaining { get; }
        public TimeSpan RivalRemaining { get; }
        public MatchRivalResult Result { get; }
        public bool TutorialDone { get; }
        public bool RewardServiceAvailable { get; }
        public string Message { get; }
        public MatchRivalUIProfile Rival { get; }
        public IReadOnlyList<MatchRivalUIReward> RoundRewards { get; }
        public IReadOnlyList<MatchRivalUIBoxReward> BoxRewards { get; }
        public MatchRivalUIButtonModel Primary { get; }
        public MatchRivalUIButtonModel Secondary { get; }
        public MatchRivalUIButtonModel Tertiary { get; }
        public float PlayerProgress => PlayerBeans / (float)RequiredBeans;
        public float RivalProgress => RivalBeans / (float)RequiredBeans;
    }

    /// <summary>Maps authoritative engine state to presentation-only immutable data.</summary>
    public static class MatchRivalUIViewModelFactory
    {
        public static MatchRivalUIViewModel Create(
            MatchRivalEngine engine,
            DateTime now,
            MatchRivalUIScreen? requestedScreen = null,
            string message = null)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));

            MatchRivalResult result = engine.PendingResult != MatchRivalResult.None
                ? engine.PendingResult
                : engine.CurrentResult;
            MatchRivalUIScreen screen = requestedScreen ?? ResolveScreen(engine, result);
            MatchRivalOpponent opponent = engine.State.Opponent;
            var rival = opponent == null
                ? null
                : new MatchRivalUIProfile(
                    opponent.Id,
                    opponent.DisplayName,
                    opponent.ProfileId,
                    opponent.FrameId);
            TimeSpan eventRemaining = engine.State.EventEndTicks <= 0L
                ? TimeSpan.Zero
                : TimeSpan.FromTicks(Math.Max(0L, engine.State.EventEndTicks - now.Ticks));

            IReadOnlyList<MatchRivalUIReward> roundRewards = BuildRoundRewards(engine, result);
            IReadOnlyList<MatchRivalUIBoxReward> boxRewards = BuildBoxRewards(engine);
            BuildActions(
                screen,
                engine,
                boxRewards,
                out MatchRivalUIButtonModel primary,
                out MatchRivalUIButtonModel secondary,
                out MatchRivalUIButtonModel tertiary);

            return new MatchRivalUIViewModel(
                screen,
                engine.Stage,
                engine.IsHard,
                engine.CollectedBeans,
                engine.RivalActualBeans,
                engine.RequiredBeans,
                eventRemaining,
                engine.RivalRemainingTime,
                result,
                engine.TutorialDone,
                engine.IsRewardServiceAvailable,
                message,
                rival,
                roundRewards,
                boxRewards,
                primary,
                secondary,
                tertiary);
        }

        private static MatchRivalUIScreen ResolveScreen(MatchRivalEngine engine, MatchRivalResult result)
        {
            if (!engine.IsEventStarted) return MatchRivalUIScreen.EventStart;
            if (engine.PendingEnd) return MatchRivalUIScreen.EventEnd;
            if (!engine.TutorialDone) return MatchRivalUIScreen.Tutorial;
            if (!engine.IsMatchActive) return MatchRivalUIScreen.MatchStart;
            if (result == MatchRivalResult.Win) return MatchRivalUIScreen.Win;
            if (result == MatchRivalResult.Lose) return MatchRivalUIScreen.Lose;
            return MatchRivalUIScreen.Match;
        }

        private static IReadOnlyList<MatchRivalUIReward> BuildRoundRewards(
            MatchRivalEngine engine,
            MatchRivalResult result)
        {
            if (result == MatchRivalResult.None) return Array.Empty<MatchRivalUIReward>();
            MatchRivalRoundRewards rewards = engine.Catalog.GetRoundRewards(engine.Stage);
            return CopyRewards(result == MatchRivalResult.Win ? rewards.WinRewards : rewards.LoseRewards);
        }

        private static IReadOnlyList<MatchRivalUIBoxReward> BuildBoxRewards(MatchRivalEngine engine)
        {
            var result = new List<MatchRivalUIBoxReward>();
            for (int stage = MatchRivalEngine.MinStage; stage <= MatchRivalEngine.MaxStage; stage++)
            {
                if (!engine.Catalog.TryGetBoxRewards(stage, out MatchRivalBoxRewards rewards)) continue;
                bool claimed = engine.IsBoxRewardClaimed(stage);
                bool available = !claimed
                    && engine.IsMatchActive
                    && engine.PendingResult == MatchRivalResult.None
                    && engine.CurrentResult == MatchRivalResult.Win
                    && stage <= engine.Stage;
                result.Add(new MatchRivalUIBoxReward(stage, CopyRewards(rewards.Rewards), claimed, available));
            }
            return result;
        }

        private static IReadOnlyList<MatchRivalUIReward> CopyRewards(IReadOnlyList<ContentReward> rewards)
        {
            if (rewards == null || rewards.Count == 0) return Array.Empty<MatchRivalUIReward>();
            var result = new List<MatchRivalUIReward>(rewards.Count);
            for (int index = 0; index < rewards.Count; index++)
            {
                ContentReward reward = rewards[index];
                result.Add(new MatchRivalUIReward(reward.RewardId, reward.Amount));
            }
            return result;
        }

        private static void BuildActions(
            MatchRivalUIScreen screen,
            MatchRivalEngine engine,
            IReadOnlyList<MatchRivalUIBoxReward> boxes,
            out MatchRivalUIButtonModel primary,
            out MatchRivalUIButtonModel secondary,
            out MatchRivalUIButtonModel tertiary)
        {
            primary = MatchRivalUIButtonModel.Hidden;
            secondary = MatchRivalUIButtonModel.Hidden;
            tertiary = new MatchRivalUIButtonModel(MatchRivalUIAction.Close, "Close");

            switch (screen)
            {
                case MatchRivalUIScreen.EventStart:
                    primary = new MatchRivalUIButtonModel(MatchRivalUIAction.StartEvent, "Start Event");
                    break;
                case MatchRivalUIScreen.MatchStart:
                    primary = new MatchRivalUIButtonModel(MatchRivalUIAction.StartMatch, "Find Rival");
                    secondary = new MatchRivalUIButtonModel(MatchRivalUIAction.OpenRewardRoad, "Rewards");
                    break;
                case MatchRivalUIScreen.Tutorial:
                    primary = new MatchRivalUIButtonModel(MatchRivalUIAction.DismissTutorial, "Continue");
                    break;
                case MatchRivalUIScreen.Match:
                    primary = new MatchRivalUIButtonModel(MatchRivalUIAction.AddBeans, "Add Beans");
                    secondary = new MatchRivalUIButtonModel(MatchRivalUIAction.ForceWin, "Finish");
                    tertiary = new MatchRivalUIButtonModel(MatchRivalUIAction.ForceLose, "Expire Rival");
                    break;
                case MatchRivalUIScreen.Win:
                case MatchRivalUIScreen.Lose:
                    primary = new MatchRivalUIButtonModel(
                        MatchRivalUIAction.ClaimRoundReward,
                        "Claim",
                        true,
                        engine.IsRewardServiceAvailable);
                    bool boxAvailable = false;
                    for (int index = 0; index < boxes.Count; index++)
                    {
                        if (boxes[index].Available)
                        {
                            boxAvailable = true;
                            break;
                        }
                    }
                    secondary = new MatchRivalUIButtonModel(
                        MatchRivalUIAction.ClaimBoxReward,
                        "Claim Box",
                        boxAvailable,
                        boxAvailable && engine.IsRewardServiceAvailable);
                    break;
                case MatchRivalUIScreen.RewardRoad:
                    primary = new MatchRivalUIButtonModel(MatchRivalUIAction.Back, "Back");
                    break;
                case MatchRivalUIScreen.EventEnd:
                    primary = new MatchRivalUIButtonModel(MatchRivalUIAction.EndEvent, "Finish Event");
                    tertiary = MatchRivalUIButtonModel.Hidden;
                    break;
            }
        }
    }
}
