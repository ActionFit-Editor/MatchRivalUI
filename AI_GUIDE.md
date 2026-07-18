# ActionFit Match Rival UI AI Guide

## Package Identity

- Package ID: `com.actionfit.match-rival.ui`
- Display name: ActionFit Match Rival UI
- Repository: `https://github.com/ActionFit-Editor/MatchRivalUI.git`
- Repository visibility: Public
- Current package version at generation time: `0.1.4`
- Unity version: `6000.2`

## Scope

This package owns the optional project-neutral MatchRival UI projection, UI Foundation
presentation, replaceable UI services, and standalone demo bootstrap. Read
`Packages/com.actionfit.match-rival/AI_GUIDE.md` before changing authoritative gameplay or reward
behavior.

## Dependency boundary

- UI package -> Match Rival engine -> Content Core.
- Only this optional UI layer may depend on UI Foundation, ReferenceBinding, and UGUI.
- Do not add Cat Merge types, `Main`, Addressables, project assets, DOTween, or UniTask.
- Project adapters may depend on this package; this package never depends on `Assembly-CSharp`.

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

- `Packages/com.actionfit.match-rival.ui/AI_GUIDE.md` - ActionFit Match Rival UI owns the optional UI Foundation presentation, strict ReferenceBinding Refs contract, replaceable services, and standalone engine-backed demo.
