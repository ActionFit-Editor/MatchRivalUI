using System;
using UnityEngine;

namespace ActionFit.MatchRival.UI
{
    [Serializable]
    public sealed class MatchRivalUITheme
    {
        [SerializeField] private Color backdrop = new(0.04f, 0.06f, 0.12f, 0.94f);
        [SerializeField] private Color panel = new(0.12f, 0.16f, 0.26f, 1f);
        [SerializeField] private Color player = new(0.31f, 0.82f, 0.46f, 1f);
        [SerializeField] private Color rival = new(0.95f, 0.37f, 0.31f, 1f);
        [SerializeField] private Color text = Color.white;
        [SerializeField] private Color secondaryText = new(0.72f, 0.78f, 0.88f, 1f);
        [SerializeField] private Color track = new(0.04f, 0.05f, 0.08f, 0.7f);
        [SerializeField] private Color primaryButton = new(0.25f, 0.63f, 0.96f, 1f);

        public Color Backdrop => backdrop;
        public Color Panel => panel;
        public Color Player => player;
        public Color Rival => rival;
        public Color Text => text;
        public Color SecondaryText => secondaryText;
        public Color Track => track;
        public Color PrimaryButton => primaryButton;
    }

    [CreateAssetMenu(
        fileName = "MatchRivalUITheme",
        menuName = "ActionFit/Match Rival/UI Theme")]
    public sealed class MatchRivalUIThemeAsset : ScriptableObject
    {
        [SerializeField] private MatchRivalUITheme theme = new();

        public MatchRivalUITheme Theme => theme ?? new MatchRivalUITheme();
    }
}
