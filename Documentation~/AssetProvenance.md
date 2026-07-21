# MatchRival Asset Provenance

Original project root: `Assets/_Project/Content/MatchRival`.

The baseline contains 21 original prefab roles and 55 original visual files. Visual bytes are copied without image generation or transformation. Texture importer settings are preserved except for the GUID required for additive coexistence.

Project-owned MonoBehaviours are not redistributed as hidden project dependencies. They are removed from copied visual YAML; package presenters and engine commands own reusable behavior. Package-owned and immutable third-party component references remain intact.

The dependency closure additionally copies 44 referenced non-script assets beneath `Runtime/ProductionDependencies` and deterministically remaps their references.

AI-generated, synthesized, placeholder, redrawn, consolidated, and automatically substituted assets are prohibited. If any original cannot be included, packaging stops for an explicit per-asset decision.

Version `0.3.2` keeps the project and package Match prefabs aligned while intentionally setting only `Txt_Tutorial1` through `Txt_Tutorial5` to `Maskable = false`. The package's legacy localization event and the project prefab's `UI_Text` localization/outline configuration remain unchanged, as do the authored parent SoftMask, hierarchy, references, and staged tutorial behavior.
