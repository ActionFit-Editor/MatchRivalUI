# ActionFit Match Rival UI AI Guide

## Package Identity

- Package ID: `com.actionfit.match-rival.ui`
- Display name: ActionFit Match Rival UI
- Repository: `https://github.com/ActionFit-Editor/MatchRivalUI.git`
- Repository visibility: Public
- Current package version at generation time: `0.3.2`
- Unity version: `6000.2`

## Scope

This package owns the optional project-neutral MatchRival UI projection, complete additive original
production prefab/image baseline, UI Foundation presentation, replaceable UI services, and standalone demo bootstrap. Read
`Packages/com.actionfit.match-rival/AI_GUIDE.md` before changing authoritative gameplay or reward
behavior.

## Dependency boundary

- UI package -> Match Rival engine -> Content Core.
- Only this optional UI layer may depend on UI Foundation, ReferenceBinding, and UGUI.
- Do not add Cat Merge types, `Main`, Addressables, project assets, DOTween, or UniTask.
- Project adapters may depend on this package; this package never depends on `Assembly-CSharp`.
- Tests and runtime integrations invoke `UI_Button` through its pointer/listener contract and must not use the removed native `UI_Button.Button` accessor.
- Original baseline bytes, importer settings, source mappings, and package SHA-256 values are recorded in `Documentation~/MigrationCoverage.md` and `AssetProvenance.md`.
- Version `0.3.1` disables the TMP Bold style in `Runtime/Prefabs/Icon/UI_MatchRival_Cell.prefab` and enables Extra Padding on every packaged TMP component while preserving hierarchy, references, text, materials, and GUIDs.
- Version `0.3.2` disables `Maskable` only on the five staged tutorial `TextMeshProUGUI` components in `Runtime/Prefabs/Popup/UI_MatchRival_Match.prefab`. This bypasses incompatible SoftMask material replacement while preserving the packaged legacy localization event, authored SoftMask parent, hierarchy, references, and progression behavior; the project production prefab keeps its `UI_Text` localization and outline settings.
- Never generate, redraw, synthesize, consolidate, omit, or automatically substitute production art. Project-only MonoBehaviour GUIDs are excluded from the clean-package prefab copy.

## Refs contract

- Nested types named `Refs` contain only mandatory descendant `Component` references.
- Fields are private `[SerializeField]` values with getter-only properties.
- Each field has unique `[RequiredReference("...")]` and exact ordinal
  `[AutoWireChild("GameObjectName")]` attributes.
- Owners enqueue `ReferenceBindingRequests.Enqueue(this)` only inside `#if UNITY_EDITOR`.
- `Assets` owns external assets and prefab assets; `Settings` owns scalars and configuration.
- AutoWire is Editor-only authoring support. Validation is read-only and never applies or saves.

## Safe validation

- Run `com.actionfit.match-rival.ui.Editor.Tests`.
- Run the package contract validator against `origin/dev_jewoo`.
- Run ReferenceBinding validation through its public read-only API.
- Confirm the runtime assembly has no forbidden project or optional-animation references.
- Publishing, repository creation, tags, releases, and catalog writes require separate approval.

## Project Router Registration

Requested router entry:

- `Packages/com.actionfit.match-rival.ui/AI_GUIDE.md` - ActionFit Match Rival UI owns the original production prefab/image baseline, optional UI Foundation presentation, strict ReferenceBinding Refs contract, replaceable services, and standalone engine-backed demo.
