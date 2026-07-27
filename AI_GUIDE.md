# ActionFit Match Rival UI AI Guide

## Package Identity

- Package ID: `com.actionfit.match-rival.ui`
- Repository: `https://github.com/ActionFit-Editor/MatchRivalUI.git`
- Current package version at generation time: `0.4.2`
- Unity version: `6000.2`

## Scope

This package owns the optional project-neutral Match Rival presentation, replaceable UI services, strict ReferenceBinding contract, and standalone bootstrap. Cat Merge Cafe owns production prefabs and images under `Assets/_Project/Content/MatchRival`.

The package does not ship duplicate production screens. It retains only runtime-reachable Resources icons, the shared Indicator, and their dependencies.

## Requested Router Entry

- `Packages/com.actionfit.match-rival.ui/AI_GUIDE.md` - ActionFit Match Rival UI owns the neutral presentation, strict ReferenceBinding contract, replaceable services, and runtime-reachable shared resources; Cat production visuals remain project-owned.

## Contracts

- UI package -> Match Rival engine -> Content Core.
- Do not add Cat types, `Main`, Addressables, project assets, DOTween, UniTask, or `Assembly-CSharp` references.
- `Refs` fields remain private serialized descendant Components with unique `RequiredReference` codes and exact `AutoWireChild` names.
- ReferenceBinding validation is read-only unless repair is separately approved.
- Do not restore retired production prefab/image copies without an explicit ownership migration.
- Version `0.4.2` retires unused package production visuals while preserving runtime APIs and reachable shared resources.

## Validation

- Run `com.actionfit.match-rival.ui.Editor.Tests` and the package contract validator.
- Confirm runtime assembly isolation and generated fallback behavior.
- Validate Cat production prefabs separately under `Assets/_Project/Content/MatchRival`.

Publishing, repository creation, tags, releases, and catalog writes require separate approval.
