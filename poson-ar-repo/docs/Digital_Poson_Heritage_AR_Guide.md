# Digital Poson Heritage AR Experience
### Complete Technical Build Guide

---

## 0. Cultural Sensitivity Note (Read First)

Before building character models of King Devanampiya Tissa and especially **Arahat Mahinda Thero**, get informal review from a Buddhist temple/cultural body in Sri Lanka. Conventions to follow:

- Depict Arahat Mahinda in traditional saffron/ochre monastic robes, dignified posture, no exaggerated facial animation — avoid anything that could read as caricature.
- Avoid violence in the "hunting scene" — show the King with bow lowered or the moment he stops, not an animated kill. The historical point of the story is the King ceasing the hunt to listen, not the chase itself.
- Add a short text/audio disclaimer in the app: "This is an artistic recreation for educational purposes."

---

## 1. System Architecture Overview

```
[Physical Poson Thorana/Kudu]
        |
   [QR Code Sticker]
        |
        v
 User scans with phone camera
        |
        v
 Deep link opens installed app  -->  (Fallback) Web AR page if app not installed
        |
        v
   Unity AR Foundation App
        |
   ┌────┴─────────────────────┐
   |  AR Session + Plane      |
   |  Detection (ARCore)      |
   |  → Place Mihintale Scene |
   |  → Play Animation        |
   |  → Touch Interaction     |
   |  → Audio Narration       |
   |  → Bilingual UI          |
   └───────────────────────────┘
```

Two delivery tracks are described below: **Track A (native Unity APK)** for full quality, and **Track B (Web AR fallback)** using an 8th Wall / model-viewer style page for users who don't want to install an app. Most real deployments at a public Poson site should lead with Web AR (zero install, instant scan-and-view) and offer the Unity APK as an optional "full experience" download.

---

## 2. QR Code Entry System

### 2.1 What the QR code should encode

Use a **single smart URL**, not a raw deep link, so it works whether or not the app is installed:

```
https://poson-ar.yourdomain.lk/start?scene=mihintale
```

This URL points to a tiny landing page that:
1. Tries to open the app via a custom URL scheme / Android App Link (`posonar://scene/mihintale`).
2. If the app isn't installed (no response within ~1.5s), redirects to the Web AR fallback page.
3. Falls back further to a normal info page with a Play Store link if AR isn't supported on that device.

### 2.2 Landing page logic (plain JS, host this on any static site)

```html
<!DOCTYPE html>
<html>
<head><title>Digital Poson Heritage</title></head>
<body>
<script>
  const appLink = "posonar://scene/mihintale";
  const webARFallback = "https://poson-ar.yourdomain.lk/webar/mihintale.html";
  const playStoreLink = "https://play.google.com/store/apps/details?id=com.yourorg.posonar";

  let appOpened = false;
  window.addEventListener("blur", () => { appOpened = true; });

  window.location.href = appLink;

  setTimeout(() => {
    if (!appOpened) {
      window.location.href = webARFallback;
    }
  }, 1500);
</script>
<noscript>
  <a href="https://poson-ar.yourdomain.lk/webar/mihintale.html">Open AR Experience</a>
</noscript>
</body>
</html>
```

### 2.3 Generating the QR code

- Use any QR generator (qr-code-generator.com, or the `qrcode` Python package) pointed at the landing URL above.
- Print at minimum 6cm x 6cm with high error correction (Level H) since it will be outdoors near a thorana and may get partially obscured by decorations or weather.
- Add a small printed caption in Sinhala and English: "මෙය ස්කෑන් කර ඉතිහාසය බලන්න" / "Scan to see history come alive."

### 2.4 Android App Links setup (so the QR opens the app directly, no chooser dialog)

In Unity, after building, edit `AndroidManifest.xml` (Player Settings → Publishing Settings → Custom Manifest) to add:

```xml
<intent-filter android:autoVerify="true">
    <action android:name="android.intent.action.VIEW" />
    <category android:name="android.intent.category.DEFAULT" />
    <category android:name="android.intent.category.BROWSABLE" />
    <data android:scheme="https"
          android:host="poson-ar.yourdomain.lk"
          android:pathPrefix="/start" />
</intent-filter>
```

You also need a `assetlinks.json` file hosted at `https://poson-ar.yourdomain.lk/.well-known/assetlinks.json` proving you own both the domain and the app package — this is what lets Android skip the "open with" dialog.

---

## 3. Unity Project Setup — Step by Step

### 3.1 Install prerequisites
1. Install **Unity Hub**, then Unity **2022 LTS** (AR Foundation 5.x is stable on this version).
2. In Unity Hub → install the Android Build Support module (with SDK, NDK, OpenJDK).
3. Create new project using the **3D (URP)** template — URP is required for good mobile AR lighting performance.

### 3.2 Install AR packages
Window → Package Manager → install:
- `AR Foundation` (5.x)
- `ARCore XR Plugin`
- `XR Plugin Management`

Edit → Project Settings → XR Plug-in Management → Android tab → enable **ARCore**.

### 3.3 Configure Player Settings (Android)
- Minimum API Level: 24 (Android 7.0) minimum for ARCore.
- Target API Level: highest installed.
- Scripting Backend: **IL2CPP**, Target Architecture: **ARM64**.
- Player Settings → Other Settings → uncheck "Auto Graphics API", set **OpenGLES3** only (Vulkan optional once tested).
- Package Name: `com.yourorg.posonar`.

### 3.4 Build the AR scene hierarchy
1. Right-click Hierarchy → XR → **AR Session**.
2. Right-click Hierarchy → XR → **AR Session Origin** (this contains the AR Camera — delete your default Main Camera).
3. Add component `AR Plane Manager` and `AR Raycast Manager` to the AR Session Origin.
4. Add an empty GameObject `MihintaleSceneRoot` — this is what gets placed on a detected plane.
5. Put the entire historical scene (terrain patch, stone pillars, trees, thorana, characters) as children of `MihintaleSceneRoot`, scaled down to roughly fit a 2m x 2m tabletop/floor footprint.

---

## 4. AR Environment Content

### 4.1 Environment elements to model/source
| Asset | Approach |
|---|---|
| Ground terrain patch (rock + dirt path) | Low-poly mesh, single 1024px tiling texture |
| Stone pillars (Mihintale-style) | Cylinder + carved cap, 200-400 tris each |
| Bo tree / forest trees | Use free SpeedTree-lite assets or simple billboard-cross trees for performance |
| Poson lanterns (kudu) & thorana arch | Modeled in Blender, paper-lantern emissive material |
| Ambient lighting | Baked lightmaps + one realtime Directional Light simulating dusk gold tone |

### 4.2 Lighting setup in Unity
- Use **URP Volume** with a warm color grading profile (amber/gold tint, slightly raised bloom) to evoke dusk/Poson Poya night atmosphere.
- Bake static environment lighting (Window → Rendering → Lighting → Generate Lighting) so mobile performance stays high; keep only the thorana lanterns as small realtime point lights with light cookies for a flicker effect.

---

## 5. Historical Animation Scene

### 5.1 Sequence design
1. **Establishing shot**: forest ambience, birds, deer grazing.
2. **Deer running** across the clearing (looping run cycle, then exits).
3. **King Devanampiya Tissa hunting**: walks in following the deer's path, bow drawn but lowered as he notices something.
4. **Arahat Mahinda Thero appears** on Mihintale rock — King startled, then bows respectfully.
5. **Dialogue moment** (the historic "Tathagatha" exchange) — represented via subtitle text + narrated audio, not literal animated speech, to keep the encounter dignified and avoid awkward lip-sync.
6. **Peaceful closing shot**: King kneeling, Mahinda Thero blessing gesture, lanterns fade in, fade to UI.

Keep total runtime 60–90 seconds, loopable, with a "skip to interaction" button.

### 5.2 Animation implementation
- Use Unity's **Animator Controller** with one controller per character (King, Mahinda, Deer).
- Each has an Animation Clip per state (Idle, Walk, Run, Bow, Blessing, Kneel).
- Drive scene progression with a `Timeline` (Unity Timeline package) — this lets you sequence camera-independent character animation, audio narration, and lighting changes on one track, and is much easier to edit than scripting each beat by hand.

---

## 6. Blender Asset Creation

### 6.1 Workflow
1. Block out characters in Blender using basic mesh humanoid base (or download a free low-poly base mesh — e.g. from Mixamo to also get free walk/run/idle animations).
2. Sculpt/adjust proportions, keep topology low-poly (3,000–6,000 tris per character is plenty for mobile AR).
3. UV unwrap → bake a single 2048px texture per character (diffuse + simple normal map).
4. Rig with Blender's auto-rig or import a Mixamo rig, retarget Mixamo animations (walk, run, idle, bow) onto your custom mesh via Blender's "Rig Retargeting" or by using Mixamo's auto-rigger directly on your model.
5. Export as **FBX** with embedded animations → import into Unity.

### 6.2 AI-assisted asset prompts (for AI 3D/texture generation tools, e.g. Meshy, Kaedim, or Blender AI texture add-ons)

> **King Devanampiya Tissa (royal hunter, 3rd century BCE Sri Lanka)**
> "Low-poly stylized 3D character, ancient Sri Lankan king in royal hunting attire, simple gold-trimmed sarong-style garment, bare chest with ornamental necklace, dignified calm facial expression, dark hair tied back, holding a traditional wooden bow, game-ready topology under 6000 triangles, clean UV layout, semi-realistic stylized art style suitable for cultural AR education app, no modern elements."

> **Arahat Mahinda Thero (Buddhist monk)**
> "Low-poly stylized 3D character of a serene Buddhist monk in flowing saffron-ochre robes, shaved head, calm compassionate expression, barefoot, peaceful upright standing pose with one hand raised in a gentle blessing gesture, respectful and dignified depiction suitable for an educational cultural app, game-ready topology under 5000 triangles, clean UV layout."

> **Deer**
> "Low-poly stylized 3D deer model, Sri Lankan spotted deer (chital-like), naturalistic proportions, mid-stride running pose reference, game-ready topology under 3000 triangles, simple two-tone tan and white fur texture, suitable for mobile AR rendering."

> **Mihintale environment elements**
> "Low-poly ancient stone pillar, weathered grey granite texture, simple carved capital, Sri Lankan ancient architecture style, game-ready under 400 triangles."
> "Low-poly Poson lantern (kudu), paper and bamboo frame structure, warm emissive glow material, traditional Sri Lankan festival decoration, under 200 triangles."
> "Low-poly Poson thorana arch, bamboo and cloth construction, decorative banana-leaf and lantern ornamentation, traditional festival arch silhouette, under 800 triangles."

For texturing, AI texture tools (e.g. Meshy texture generation, or Blender's built-in AI-assisted texture paint add-ons) can take these prompts plus the unwrapped UV mesh to generate diffuse maps directly.

### 6.3 Import settings in Unity
- Scale Factor: 1 (set correctly in Blender export, meters).
- Animation Type: Humanoid (for Mixamo-rigged characters, this enables retargeting/blending).
- Materials: Use **URP/Lit** shader, compress textures to ASTC for Android.

---

## 7. C# Scripts

### 7.1 AR Object Placement Script

```csharp
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

[RequireComponent(typeof(ARRaycastManager))]
public class ARScenePlacer : MonoBehaviour
{
    [SerializeField] private GameObject mihintaleScenePrefab;
    [SerializeField] private GameObject placementIndicator;

    private ARRaycastManager raycastManager;
    private GameObject spawnedScene;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (spawnedScene != null) return; // already placed, stop scanning

        // Use screen center for placement reticle
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                PlaceScene(hitPose);
            }
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    void PlaceScene(Pose pose)
    {
        spawnedScene = Instantiate(mihintaleScenePrefab, pose.position, pose.rotation);
        placementIndicator.SetActive(false);

        var sceneController = spawnedScene.GetComponent<PosonSceneController>();
        sceneController?.BeginExperience();
    }
}
```

### 7.2 QR Scanning Integration (in-app scanner, for users who launch the app directly without an external camera scan)

Uses the lightweight `ZXing.Net` library (import via NuGet/UnityNuGet or the free "QR Code Scanner" asset).

```csharp
using UnityEngine;
using UnityEngine.UI;
using ZXing;

public class QRScannerController : MonoBehaviour
{
    [SerializeField] private RawImage cameraPreview;
    [SerializeField] private string expectedSceneCode = "mihintale";

    private WebCamTexture camTexture;
    private IBarcodeReader reader = new BarcodeReader();
    private bool sceneLoaded = false;

    void Start()
    {
        camTexture = new WebCamTexture();
        cameraPreview.texture = camTexture;
        camTexture.Play();
    }

    void Update()
    {
        if (sceneLoaded || camTexture == null || !camTexture.didUpdateThisFrame) return;

        try
        {
            var result = reader.Decode(camTexture.GetPixels32(), camTexture.width, camTexture.height);
            if (result != null && result.Text.Contains(expectedSceneCode))
            {
                sceneLoaded = true;
                camTexture.Stop();
                UnityEngine.SceneManagement.SceneManager.LoadScene("ARMihintaleScene");
            }
        }
        catch { /* no QR in frame this tick — ignore */ }
    }
}
```

*Note: in the primary flow (Section 2), QR scanning happens via the phone's native camera app and a URL — this in-app scanner is an optional secondary entry point if you want QR scanning to happen entirely inside the app.*

### 7.3 Animation/Timeline Controller

```csharp
using UnityEngine;
using UnityEngine.Playables;

public class PosonSceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private AudioSource narrationSource;
    [SerializeField] private AudioClip narrationClip;

    public void BeginExperience()
    {
        timelineDirector.time = 0;
        timelineDirector.Play();

        if (narrationClip != null)
        {
            narrationSource.clip = narrationClip;
            narrationSource.Play();
        }
    }

    public void PauseExperience()
    {
        timelineDirector.Pause();
        narrationSource.Pause();
    }

    public void ResumeExperience()
    {
        timelineDirector.Resume();
        narrationSource.UnPause();
    }

    public void RestartExperience()
    {
        timelineDirector.time = 0;
        timelineDirector.Play();
        narrationSource.Stop();
        narrationSource.Play();
    }
}
```

### 7.4 Touch Interaction Script (tap an object for info)

```csharp
using UnityEngine;

public class TapInfoTarget : MonoBehaviour
{
    [TextArea] public string infoTextEnglish;
    [TextArea] public string infoTextSinhala;
    public AudioClip infoAudioClip;
}

public class TouchInteractionManager : MonoBehaviour
{
    [SerializeField] private Camera arCamera;
    [SerializeField] private InfoPanelUI infoPanel;
    [SerializeField] private AudioSource sfxSource;

    void Update()
    {
        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        Ray ray = arCamera.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            var target = hit.collider.GetComponent<TapInfoTarget>();
            if (target != null)
            {
                infoPanel.ShowInfo(target.infoTextEnglish, target.infoTextSinhala);
                if (target.infoAudioClip != null)
                {
                    sfxSource.clip = target.infoAudioClip;
                    sfxSource.Play();
                }
            }
        }
    }
}
```

### 7.5 Audio Controller

```csharp
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource narrationSource;
    [SerializeField] private Button muteButton;
    [SerializeField] private Sprite mutedIcon, unmutedIcon;
    [SerializeField] private Image muteButtonIcon;

    private bool isMuted = false;

    public void ToggleMute()
    {
        isMuted = !isMuted;
        musicSource.mute = isMuted;
        narrationSource.mute = isMuted;
        muteButtonIcon.sprite = isMuted ? mutedIcon : unmutedIcon;
    }

    public void SetMusicVolume(float v) => musicSource.volume = v;
    public void SetNarrationVolume(float v) => narrationSource.volume = v;
}
```

### 7.6 UI Manager (bilingual + panel control)

```csharp
using UnityEngine;
using TMPro;

public enum AppLanguage { English, Sinhala }

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject arHud;
    [SerializeField] private InfoPanelUI infoPanel;
    [SerializeField] private TMP_Text languageToggleLabel;

    public static AppLanguage CurrentLanguage = AppLanguage.English;

    public void OnStartARButton()
    {
        startPanel.SetActive(false);
        arHud.SetActive(true);
    }

    public void ToggleLanguage()
    {
        CurrentLanguage = CurrentLanguage == AppLanguage.English
            ? AppLanguage.Sinhala
            : AppLanguage.English;
        languageToggleLabel.text = CurrentLanguage == AppLanguage.English ? "EN" : "සිං";
        // Broadcast to any localized text components to refresh
        BroadcastMessage("RefreshLanguage", SendMessageOptions.DontRequireReceiver);
    }
}

public class InfoPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text bodyText;

    public void ShowInfo(string english, string sinhala)
    {
        panelRoot.SetActive(true);
        bodyText.text = UIManager.CurrentLanguage == AppLanguage.English ? english : sinhala;
    }

    public void Hide() => panelRoot.SetActive(false);
}
```

---

## 8. UI Design Guidance

- **Color palette**: saffron/gold (#E8A33D), deep maroon (#6B1F2A), cream background (#F7EFE2) — evokes temple flags and Poson lantern tones without being garish.
- **Typography**: pair a clean English sans-serif (e.g. Poppins) with a Sinhala-supporting font (e.g. Noto Sans Sinhala) — set both in TextMeshPro with a Sinhala-compatible font asset (TMP requires generating a custom font atlas for Sinhala glyphs via Window → TextMeshPro → Font Asset Creator using a Noto Sans Sinhala TTF).
- **Screens needed**: Splash/Start screen (app icon, "Start AR" button, language toggle) → AR scanning/placement screen (reticle + "move phone to find a surface" hint) → AR HUD (mute icon, info icon, restart icon, captions toggle) → Info panel (slide-up sheet with text + small image + audio play button).

---

## 9. Audio Content

| Type | Spec |
|---|---|
| Background music | Calm instrumental with traditional Sri Lankan instruments (e.g. flute/sitar-adjacent tones), looped, low volume under narration |
| Narration | Recorded voiceover (English + Sinhala versions) describing the Mahinda–Tissa meeting, ~250-300 words covering the full scene |
| SFX | Forest ambience (birds, wind, leaves), soft bell/chime on UI taps, gentle whoosh on scene placement |

Keep all audio compressed (Vorbis, ~96kbps) and set narration clips to "Streaming" load type in Unity Import Settings to reduce memory footprint.

---

## 10. Building & Testing

1. File → Build Settings → switch platform to **Android**.
2. Add your AR scene(s) to "Scenes in Build".
3. Player Settings: confirm package name, minimum API 24, IL2CPP + ARM64 as set in Section 3.3.
4. Connect an ARCore-supported Android phone via USB, enable Developer Mode + USB debugging.
5. Click **Build and Run**.
6. Test checklist: plane detection works in varied lighting, tap targets register accurately, narration syncs with Timeline, language toggle updates all text, app handles loss/reacquisition of camera tracking gracefully (don't let the scene jump or duplicate).
7. For the QR flow specifically: test by physically printing the QR and scanning with the stock Android camera app to confirm the deep link → app launch → scene autoload chain works end-to-end.

---

## 11. Optimization for Mid-Range Android Phones

- Keep total scene under ~80K triangles and ~15 draw calls; combine static meshes (pillars, ground, thorana) into as few meshes as possible via mesh combining.
- Texture atlas: pack all character + environment textures into 1–2 shared atlases (2048px max) to cut draw calls.
- Use **baked lighting** for everything except 1 realtime light; avoid realtime shadows on more than the main character.
- Set Quality Settings → mobile tier to cap shadow distance (~15m) and disable soft shadows.
- Use LOD Groups on trees/pillars if duplicated multiple times in the scene.
- Cap target frame rate at 30fps for AR (`Application.targetFrameRate = 30`) — battery/thermal stability matters more than 60fps for handheld AR sessions.
- Compress all textures to ASTC 6x6 for Android.
- Profile with Unity's built-in Profiler + the Android GPU/ARCore performance overlay before final release; aim for under 150MB APK size by compressing audio and using ASTC textures throughout.

---

## 12. Suggested Build Order (Practical Roadmap)

1. Unity project + AR Foundation plane detection working with a placeholder cube (1–2 days).
2. Block-out environment (pillars, ground, thorana) with placeholder grey materials (2–3 days).
3. Character models from Blender, rigged + basic animations imported (this is the longest phase — budget 1–2 weeks per character if modeling from scratch, much less if using AI-assisted base meshes + Mixamo retargeting).
4. Timeline sequencing of the historical scene (3–4 days).
5. UI, audio, bilingual text, touch interaction (3–5 days).
6. QR code + deep link + landing page (1–2 days).
7. Optimization pass + device testing across 3+ phone models (3–5 days).
8. Web AR fallback build if pursuing Track B (separate effort, typically using model-viewer.js or 8th Wall — ask if you want this expanded into its own guide).

---

*This guide gives you a working skeleton across every requested system. The biggest remaining variable is asset production time (3D modeling + animation) — starting with AI-assisted base meshes and Mixamo animation retargeting will save the most time versus modeling everything from scratch in Blender.*
