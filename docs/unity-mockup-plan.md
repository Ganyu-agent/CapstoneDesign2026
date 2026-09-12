# Unity mockup plan

The first vertical slice is a quiet, portrait-oriented island screen. A single
directional light is kept low, like a full moon at dusk. The island, foliage,
and reward glow are generated from small procedural meshes in the Editor API so
the first build has no external binary dependency.

The initial screen follows the latest meeting direction:

- a calm floating island as the central visual reward;
- no score, ranking, streak pressure, or competitive language;
- a bottom navigation bar with 오늘의 활동, 섬, and 설정;
- five scrollable placeholder daily activities, presented as gentle steps;
- a settings panel with only the 설정 label for now;
- a raw sensor panel at the top left for Android hardware discovery.

## Editor generated assets

`CapstoneDesign.EditorTools.MockupProjectBuilder` creates:

- `Assets/Art/FloatingIsland.asset`, a low-poly rock/island mesh;
- `Assets/Art/RewardPlant.asset`, a small low-poly tree/bush mesh;
- `Assets/Materials/*`, URP Lit materials;
- `Assets/Scenes/MockupMain.unity`, the camera, one light, island, foliage,
  physics sphere, canvas, navigation, activity scroll view, settings panel,
  and sensor overlay;
- `Assets/Settings/MockupUrp.asset`, a lightweight URP pipeline asset.

When the optional Kenney files are present, the builder instantiates
`Assets/Art/External/KenneyNatureKit/tree_default.fbx` and
`Assets/Art/External/KenneyNatureKit/plant_bush.fbx` in the generated scene.
The procedural tree/bush remains the deterministic fallback when those imports
are unavailable.

The generated `.unity`, `.asset`, and `.mat` files are runtime outputs. They are
safe to regenerate from the Editor menu `Capstone Mockup/Generate All`.

## Visual validation

`VisualTestTools.CaptureMockupPreview` renders the `MockupCamera` into a PNG
through a `RenderTexture`. It does not use OS screenshots and it must run with a
display context but without `-nographics`. The worker's Intel Vulkan selection
is logged so a software fallback is visible in the Editor log.

## Optional external low-poly source

The procedural mesh is the deterministic fallback for CI. An optional art pass
may import the Kenney Nature Kit rock/bush assets under
`Assets/Art/External/KenneyNatureKit/`:

<https://kenney.nl/assets/nature-kit>

Kenney distributes the kit under CC0. The downloaded archive and any imported
binary files should be kept in that path and tracked with Git LFS if they are
large. `external_assets/README.md` records the source and replacement point;
the scene remains functional when the optional archive is absent.

## Test entry points

- `BuildTools.ValidateProject` validates the expected generated objects.
- `BuildTools.BuildAndroid` writes `artifacts/build/capstone-mockup.apk`.
- `VisualTestTools.CaptureMockupPreview` writes
  `/artifacts/visual/mockup-preview.png` when that mount exists.
- `AndroidTestMode` reads the optional `testScene` Android Intent extra and
  logs the selected mode (default: `GardenPreview`).
- `AndroidTestTools` is represented by the shell scripts in `scripts/`; the
  scripts skip cleanly when the A7 is offline.
