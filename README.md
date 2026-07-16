# ActionFit Match Rival UI

Optional project-neutral UI layer for `com.actionfit.match-rival`. It renders immutable engine
snapshots with UI Foundation components and routes input back to public engine commands. The
engine remains the only owner of schedule, progress, result, persistence, and reward state.

## Install

Install the private packages together after their repositories and catalog rows are published:

```json
{
  "dependencies": {
    "com.actionfit.match-rival": "https://github.com/ActionFitGames/MatchRival.git#0.1.1",
    "com.actionfit.match-rival.ui": "https://github.com/ActionFitGames/MatchRivalUI.git#0.1.0"
  }
}
```

This package directly depends on Content Core, Match Rival, ReferenceBinding 0.1.1,
UI Foundation 1.0.5, and UGUI 2.0.0.

## Standalone flow

Use `Tools/Package/ActionFit Match Rival UI/Create Demo` or add `MatchRivalBootstrap` to a
GameObject. The bootstrap supplies safe PlayerPrefs services and an active demo schedule. It can
exercise event start, tutorial, rival matching, bean progress, win/lose rewards, box rewards, and
event end without Cat Merge assets or services.

## Integration contract

- Use `MatchRivalUIViewModelFactory` to copy public engine reads into immutable UI data.
- Inject localization, audio, profile, reward rendering, animation, clock display, and view-host
  services through their narrow interfaces.
- Theme assets are optional. Cat Merge sprites, audio, fonts, materials, confetti, production
  prefabs, Addressables, and balance assets stay project-owned.
- `Refs` fields are private serialized descendant Components. Every field uses a stable
  `RequiredReference` code and exact-name `AutoWireChild`.
- AutoWire is an Editor authoring aid. Runtime presentation never searches children by name.
- CI and audits must use ReferenceBinding read-only validation; they must not apply or save refs.

The package intentionally has no DOTween, UniTask, Addressables, `Main`, project localization,
project sound, project profile, project inventory, or `Assembly-CSharp` dependency.

## Validation and release

Run the EditMode assembly `com.actionfit.match-rival.ui.Editor.Tests`, the Custom Package Manager
contract validator, and ReferenceBinding read-only validation. Repository creation, tags, catalog
rows, and publication remain separately approved manual steps.
