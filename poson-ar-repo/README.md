# Digital Poson Heritage AR Experience

AR experience recreating the meeting of King Devanampiya Tissa and Arahat Mahinda Thero at Mihintale, triggered by scanning a QR code near a Poson thorana/kudu.

## Repo structure

```
.
├── webar/                    # Browser-based AR (no app install) — works today
│   ├── index.html            # QR landing page: opens native app, falls back to Web AR
│   └── mihintale-ar.html     # The actual AR view (model-viewer based)
│
├── unity-project/            # Native Unity AR Foundation app (in progress)
│   └── Assets/
│       ├── Scripts/          # All C# scripts (placement, QR, audio, UI, interaction)
│       ├── Scenes/           # Unity scenes (empty until you create them in-editor)
│       ├── Prefabs/          # Character/environment prefabs
│       ├── Models/           # Imported .fbx/.glb models from Blender
│       ├── Audio/            # Music, narration, SFX
│       └── Timeline/         # Unity Timeline assets driving the historical scene
│
├── docs/
│   └── Digital_Poson_Heritage_AR_Guide.md   # Full build guide (setup, Blender prompts, etc.)
│
├── .gitignore                # Unity-specific ignores (Library/, Temp/, build artifacts)
└── .gitattributes            # Git LFS rules for large binaries (models, audio, textures)
```

## Quick start

### Web AR (test in minutes, no install)
1. Push this repo to GitHub.
2. Enable GitHub Pages: Settings → Pages → Deploy from branch → `main` → `/ (root)`.
3. Your live demo will be at:
   `https://<your-username>.github.io/<repo-name>/webar/mihintale-ar.html`
4. Generate a QR code pointing at:
   `https://<your-username>.github.io/<repo-name>/webar/index.html`
5. Scan it with a phone — opens straight in the browser.

### Unity native app
1. Open `unity-project/` in Unity Hub (2022 LTS recommended).
2. Install AR Foundation + ARCore XR Plugin via Package Manager (see `docs/` guide, Section 3).
3. Scripts in `Assets/Scripts/` are already in place — wire them up to GameObjects per the comments at the top of each file.
4. Build → Android.

## Before your first commit

```bash
git init
git lfs install
git lfs track "*.glb" "*.fbx" "*.blend" "*.wav" "*.mp3" "*.png" "*.psd"
git add .
git commit -m "Initial scaffold: Web AR demo + Unity scripts + docs"
git remote add origin <your-repo-url>
git push -u origin main
```

`git lfs install` only needs to run once per machine. The `.gitattributes` file already lists the binary types to track — just make sure LFS is installed before you add any real 3D models or audio, or they'll bloat the repo permanently in your git history.

## Status / what's still placeholder
- `webar/mihintale-ar.html` uses a placeholder astronaut model — swap the `<model-viewer src="...">` once you export real `.glb` files from Blender.
- `unity-project/Assets/Scenes`, `Prefabs`, `Models`, `Audio`, `Timeline` are currently empty folders (git won't track empty folders — see note below).
- No compiled APK yet; that's a local Unity build step.

> Git doesn't track empty directories. Each empty folder above has a placeholder `.gitkeep` file so the structure survives your first commit — delete the `.gitkeep` once real files land in that folder.
