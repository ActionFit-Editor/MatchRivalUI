using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ActionFit.Content;
using NUnit.Framework;
using ReferenceBinding;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.TestTools;

namespace ActionFit.MatchRival.UI.Tests
{
    public sealed class MatchRivalUITests
    {
        [TearDown]
        public void TearDown()
        {
            new PlayerPrefsContentStateStore().Delete(MatchRivalBootstrap.DefaultDemoContentId);
            foreach (MatchRivalBootstrap bootstrap in UnityEngine.Object.FindObjectsByType<MatchRivalBootstrap>(FindObjectsSortMode.None))
                UnityEngine.Object.DestroyImmediate(bootstrap.gameObject);
            foreach (MatchRivalPresentation presentation in UnityEngine.Object.FindObjectsByType<MatchRivalPresentation>(FindObjectsSortMode.None))
                UnityEngine.Object.DestroyImmediate(presentation.gameObject);
        }

        [Test]
        public void ViewModel_ClampsPresentationValues()
        {
            var model = new MatchRivalUIViewModel(
                MatchRivalUIScreen.Match,
                -1,
                false,
                -2,
                200,
                10,
                TimeSpan.FromSeconds(-1d),
                TimeSpan.FromSeconds(-1d),
                MatchRivalResult.None,
                false,
                true,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.That(model.Stage, Is.EqualTo(MatchRivalEngine.MinStage));
            Assert.That(model.PlayerBeans, Is.Zero);
            Assert.That(model.RivalBeans, Is.EqualTo(10));
            Assert.That(model.EventRemaining, Is.EqualTo(TimeSpan.Zero));
            Assert.That(model.Primary.Visible, Is.False);
        }

        [Test]
        public void Presentation_GeneratesFoundationFallbackWithoutMutatingInspectorRefs()
        {
            var root = new GameObject("Match Rival Presentation Test");
            try
            {
                var presentation = root.AddComponent<MatchRivalPresentation>();
                Assert.That(presentation.InspectorReferences.IsComplete, Is.False);

                presentation.Initialize();

                Assert.That(presentation.IsInitialized, Is.True);
                Assert.That(presentation.InspectorReferences.IsComplete, Is.False);
                Assert.That(root.GetComponentInChildren<UI_Text>(true), Is.Not.Null);
                Assert.That(root.GetComponentInChildren<UI_Button>(true), Is.Not.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Bootstrap_DefaultFlowRoutesOnlyThroughEngineCommands()
        {
            new PlayerPrefsContentStateStore().Delete(MatchRivalBootstrap.DefaultDemoContentId);
            var bootstrapObject = new GameObject("Match Rival Bootstrap Test");
            var presentationObject = new GameObject("Match Rival Presentation Flow Test");
            try
            {
                var bootstrap = bootstrapObject.AddComponent<MatchRivalBootstrap>();
                var presentation = presentationObject.AddComponent<MatchRivalPresentation>();
                bootstrap.InitializeDefault(presentation);

                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(MatchRivalUIScreen.EventStart));
                Click(presentation, "PrimaryButton");
                Assert.That(bootstrap.Engine.IsEventStarted, Is.True);
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(MatchRivalUIScreen.Tutorial));
                Click(presentation, "PrimaryButton");
                Assert.That(bootstrap.Engine.TutorialDone, Is.True);
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(MatchRivalUIScreen.MatchStart));
                Click(presentation, "PrimaryButton");
                Assert.That(bootstrap.Engine.IsMatchActive, Is.True);
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(MatchRivalUIScreen.Match));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bootstrapObject);
                UnityEngine.Object.DestroyImmediate(presentationObject);
            }
        }

        [Test]
        public void Bootstrap_WinFlowClaimsBoxBeforeRoundWithoutPendingTransactionConflict()
        {
            new PlayerPrefsContentStateStore().Delete(MatchRivalBootstrap.DefaultDemoContentId);
            var bootstrapObject = new GameObject("Match Rival Bootstrap Reward Test");
            var presentationObject = new GameObject("Match Rival Presentation Reward Test");
            try
            {
                var bootstrap = bootstrapObject.AddComponent<MatchRivalBootstrap>();
                var presentation = presentationObject.AddComponent<MatchRivalPresentation>();
                bootstrap.InitializeDefault(presentation);

                Click(presentation, "PrimaryButton");
                Click(presentation, "PrimaryButton");
                Click(presentation, "PrimaryButton");
                Click(presentation, "SecondaryButton");
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(MatchRivalUIScreen.Win));
                Click(presentation, "PrimaryButton");
                Assert.That(bootstrap.Engine.Stage, Is.EqualTo(2));

                Click(presentation, "PrimaryButton");
                Click(presentation, "SecondaryButton");
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(MatchRivalUIScreen.Win));
                Click(presentation, "SecondaryButton");
                Assert.That(bootstrap.Engine.IsBoxRewardClaimed(2), Is.True);
                Click(presentation, "PrimaryButton");
                Assert.That(bootstrap.Engine.Stage, Is.EqualTo(3));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bootstrapObject);
                UnityEngine.Object.DestroyImmediate(presentationObject);
            }
        }

        [Test]
        public void Bootstrap_CloseReopenLoseAndEventEndRoutesRemainReusable()
        {
            new PlayerPrefsContentStateStore().Delete(MatchRivalBootstrap.DefaultDemoContentId);
            var bootstrapObject = new GameObject("Match Rival Bootstrap Lifecycle Test");
            var presentationObject = new GameObject("Match Rival Presentation Lifecycle Test");
            try
            {
                var bootstrap = bootstrapObject.AddComponent<MatchRivalBootstrap>();
                var presentation = presentationObject.AddComponent<MatchRivalPresentation>();
                int closeCount = 0;
                bootstrap.CloseRequested += () => closeCount++;
                bootstrap.InitializeDefault(presentation);

                Click(presentation, "TertiaryButton");
                Assert.That(bootstrap.IsVisible, Is.False);
                Assert.That(closeCount, Is.EqualTo(1));
                bootstrap.Show();
                Assert.That(bootstrap.IsVisible, Is.True);

                Click(presentation, "PrimaryButton");
                Click(presentation, "PrimaryButton");
                Click(presentation, "PrimaryButton");
                Click(presentation, "TertiaryButton");
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(MatchRivalUIScreen.Lose));
                Click(presentation, "PrimaryButton");
                Assert.That(bootstrap.Engine.Stage, Is.EqualTo(MatchRivalEngine.MinStage));

                bootstrap.Engine.MarkPendingEnd();
                Assert.That(presentation.CurrentModel.Screen, Is.EqualTo(MatchRivalUIScreen.EventEnd));
                Click(presentation, "PrimaryButton");
                Assert.That(bootstrap.Engine.IsEventStarted, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bootstrapObject);
                UnityEngine.Object.DestroyImmediate(presentationObject);
            }
        }

        [Test]
        public void Presentation_UsesInjectedServicesAndResetsAnimationWhenHidden()
        {
            var root = new GameObject("Match Rival Presentation Services Test");
            try
            {
                var presentation = root.AddComponent<MatchRivalPresentation>();
                var localizer = new RecordingLocalizer();
                var audio = new RecordingAudio();
                var animation = new RecordingAnimation();
                presentation.Initialize(
                    localizer,
                    audio,
                    new FixedProfileProvider(),
                    new FixedRewardRenderer(),
                    animation);

                presentation.Present(CreateModel(MatchRivalUIScreen.EventStart, 0, MatchRivalResult.None));
                presentation.Present(CreateModel(MatchRivalUIScreen.Win, 5, MatchRivalResult.Win));

                UI_Text title = presentation.GetComponentsInChildren<UI_Text>(true)
                    .Single(text => text.gameObject.name == "Title");
                UI_Text rewards = presentation.GetComponentsInChildren<UI_Text>(true)
                    .Single(text => text.gameObject.name == "Rewards");
                Assert.That(title.Text, Is.EqualTo("localized:Match Rival"));
                Assert.That(rewards.Text, Is.EqualTo("rendered rewards"));
                Assert.That(audio.Cues, Does.Contain(MatchRivalUIKeys.AudioScreen));
                Assert.That(audio.Cues, Does.Contain(MatchRivalUIKeys.AudioProgress));
                Assert.That(audio.Cues, Does.Contain(MatchRivalUIKeys.AudioReward));
                Assert.That(animation.ScreenChanges, Is.EqualTo(2));
                Assert.That(animation.ProgressChanges, Is.EqualTo(1));
                Assert.That(animation.RewardPresentations, Is.EqualTo(1));

                presentation.Hide();
                Assert.That(animation.ResetCount, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PresentationRefs_UseStrictReferenceBindingContract()
        {
            Type refsType = typeof(MatchRivalPresentation.Refs);
            FieldInfo[] fields = refsType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(fields, Is.Not.Empty);
            var errorCodes = new HashSet<string>(StringComparer.Ordinal);

            foreach (FieldInfo field in fields)
            {
                Assert.That(field.IsPrivate, Is.True, field.Name);
                Assert.That(field.GetCustomAttribute<SerializeField>(), Is.Not.Null, field.Name);
                Assert.That(typeof(Component).IsAssignableFrom(field.FieldType), Is.True, field.Name);
                RequiredReferenceAttribute required = field.GetCustomAttribute<RequiredReferenceAttribute>();
                AutoWireChildAttribute autoWire = field.GetCustomAttribute<AutoWireChildAttribute>();
                Assert.That(required, Is.Not.Null, field.Name);
                Assert.That(autoWire, Is.Not.Null, field.Name);
                Assert.That(required.ErrorCode, Is.Not.Empty, field.Name);
                Assert.That(errorCodes.Add(required.ErrorCode), Is.True, required.ErrorCode);
                Assert.That(autoWire.ObjectName, Is.Not.Empty, field.Name);

                PropertyInfo property = refsType.GetProperty(
                    char.ToUpperInvariant(field.Name[0]) + field.Name.Substring(1),
                    BindingFlags.Instance | BindingFlags.Public);
                Assert.That(property, Is.Not.Null, field.Name);
                Assert.That(property.CanRead, Is.True, field.Name);
                Assert.That(property.SetMethod, Is.Null, field.Name);
            }
        }

        [Test]
        public void RuntimeAssembly_HasNoForbiddenProjectOrAnimationReferences()
        {
            string[] references = typeof(MatchRivalPresentation).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .ToArray();

            Assert.That(references, Does.Not.Contain("Assembly-CSharp"));
            Assert.That(references, Does.Not.Contain("DOTween"));
            Assert.That(references, Does.Not.Contain("UniTask"));
            Assert.That(references.Any(name => name.Contains("Addressables", StringComparison.Ordinal)), Is.False);
        }

        [Test]
        public void PlayerCompiledAssembly_ExcludesEditorOnlyOnValidateAndRequestCall()
        {
            string outputDirectory = Path.Combine(
                Path.GetTempPath(),
                "ActionFitMatchRivalPlayerScripts-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(outputDirectory);
            bool previousIgnoreFailingMessages = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            try
            {
                BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
                var settings = new ScriptCompilationSettings
                {
                    target = target,
                    group = BuildPipeline.GetBuildTargetGroup(target),
                    options = ScriptCompilationOptions.None
                };
                ScriptCompilationResult result = PlayerBuildInterface.CompilePlayerScripts(
                    settings,
                    outputDirectory);
                string assemblyPath = result.assemblies.Single(path =>
                    string.Equals(
                        Path.GetFileName(path),
                        "com.actionfit.match-rival.ui.dll",
                        StringComparison.Ordinal));
                if (!Path.IsPathRooted(assemblyPath))
                    assemblyPath = Path.Combine(outputDirectory, assemblyPath);
                string assemblyMetadata = Encoding.UTF8.GetString(File.ReadAllBytes(assemblyPath));

                Assert.That(assemblyMetadata, Does.Contain(nameof(MatchRivalPresentation)));
                Assert.That(assemblyMetadata, Does.Not.Contain("OnValidate"));
                Assert.That(assemblyMetadata, Does.Not.Contain(nameof(ReferenceBindingRequests)));
            }
            finally
            {
                LogAssert.ignoreFailingMessages = previousIgnoreFailingMessages;
                if (Directory.Exists(outputDirectory))
                    Directory.Delete(outputDirectory, true);
            }
        }

        [Test]
        public void DefaultServices_AreSafeAndDeterministic()
        {
            Assert.That(PassthroughMatchRivalUILocalizer.Instance.Get("key", "fallback"), Is.EqualTo("fallback"));
            Assert.That(DefaultMatchRivalUIClockDisplay.Instance.Format(TimeSpan.FromSeconds(61d)), Is.EqualTo("00:01:01"));
            Assert.That(
                TextMatchRivalUIRewardRenderer.Instance.Render(
                    new[] { new MatchRivalUIReward("coin", 10) },
                    Array.Empty<MatchRivalUIBoxReward>(),
                    PassthroughMatchRivalUILocalizer.Instance),
                Is.EqualTo("coin x10"));
        }

        private static void Click(MatchRivalPresentation presentation, string objectName)
        {
            UI_Button button = presentation.GetComponentsInChildren<UI_Button>(true)
                .FirstOrDefault(candidate => string.Equals(candidate.gameObject.name, objectName, StringComparison.Ordinal));
            Assert.That(button, Is.Not.Null, objectName);
            Assert.That(button.gameObject.activeSelf, Is.True, objectName);
            button.Button.onClick.Invoke();
        }

        private static MatchRivalUIViewModel CreateModel(
            MatchRivalUIScreen screen,
            int playerBeans,
            MatchRivalResult result)
        {
            return new MatchRivalUIViewModel(
                screen,
                1,
                false,
                playerBeans,
                2,
                10,
                TimeSpan.FromMinutes(1d),
                TimeSpan.FromSeconds(30d),
                result,
                true,
                true,
                string.Empty,
                new MatchRivalUIProfile("rival", "Rival", string.Empty, string.Empty),
                Array.Empty<MatchRivalUIReward>(),
                Array.Empty<MatchRivalUIBoxReward>(),
                MatchRivalUIButtonModel.Hidden,
                MatchRivalUIButtonModel.Hidden,
                MatchRivalUIButtonModel.Hidden);
        }

        private sealed class RecordingLocalizer : IMatchRivalUILocalizer
        {
            public string Get(string key, string fallback) => "localized:" + fallback;
        }

        private sealed class RecordingAudio : IMatchRivalUIAudio
        {
            internal List<string> Cues { get; } = new();

            public void Play(string cueId) => Cues.Add(cueId);
        }

        private sealed class FixedProfileProvider : IMatchRivalUIProfileProvider
        {
            public MatchRivalUIProfile GetPlayerProfile() =>
                new("player", "Player", string.Empty, string.Empty);
        }

        private sealed class FixedRewardRenderer : IMatchRivalUIRewardRenderer
        {
            public string Render(
                IReadOnlyList<MatchRivalUIReward> roundRewards,
                IReadOnlyList<MatchRivalUIBoxReward> boxRewards,
                IMatchRivalUILocalizer localizer) => "rendered rewards";
        }

        private sealed class RecordingAnimation : IMatchRivalUIAnimation
        {
            internal int ScreenChanges { get; private set; }
            internal int ProgressChanges { get; private set; }
            internal int RewardPresentations { get; private set; }
            internal int ResetCount { get; private set; }

            public void ScreenChanged(MatchRivalUIScreen previous, MatchRivalUIScreen current) =>
                ScreenChanges++;

            public void ProgressChanged(float previous, float current) => ProgressChanges++;

            public void RewardPresented(MatchRivalResult result) => RewardPresentations++;

            public void Reset() => ResetCount++;
        }
    }
}
