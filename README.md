

Stack: **Unity 6.2.2f1**, **OpenXR**, **XR Plug-in Management 4.5.1**, **XR Interaction Toolkit (XRI) 3.2.1**, **Input System**.

---

# [✅]🧱 Block 0 — Project & Packages (one-time)

**Goal:** Make sure the project has the correct XR + Input packages.

**Editor steps**

1. **Create / open** a 3D (URP OK) project.
2. **Project Settings → XR Plug-in Management**

   * PC (if you’ll test with Link): **OpenXR = ON**
   * Android (for Quest build later): **OpenXR = ON**
3. **Window → Package Manager (Unity Registry)**

   * Install **XR Interaction Toolkit** (3.2.1).
   * In XRI’s **Samples** (right pane): import **Starter Assets** and (optional) **XR Device Simulator**.
4. **Project Settings → Player → Other Settings**

   * **Active Input Handling** = **Input System (New)**.

**Why:** OpenXR is the runtime; XRI gives us Interactors/Interactables; Starter Assets give ready input actions.

**Mini-test:** Create an empty scene → *Play* (no errors about Input System or OpenXR).

**Common mistakes:**

* If you still have “Both” input systems enabled and get warnings, switch to **Input System (New)** only.

---

# [✅]🧱 Block 1 — Scene Scaffold

**Goal:** Basic XR scene that compiles.

**Editor steps**

1. **File → New Scene** (call it `Session6`).
2. Delete **Main Camera**.
3. **GameObject → XR → XR Origin (Action-based)**.
4. **GameObject → XR → XR Interaction Manager**.
5. **GameObject → UI → Event System**.
6. (Optional) **GameObject → XR → Device Simulator**.
7. Create **Empty** `VR` and drag the three XR objects under it (for organization).
8. **GameObject → 3D Object → Plane** (as floor) at (0,0,0).

**Why:** XR Origin has the tracked camera and controller anchors. Manager routes interactions. Event System enables UI.

**Mini-test:** *Play* with a headset connected (or with Device Simulator): no red errors.

**Common mistakes:**

* Don’t keep the **Main Camera**; XR Origin includes its own camera.

---

# [✅]🧱 Block 2 — Target Layer + Target Object

**Goal:** Make a simple object that our ray will be allowed to hit.

**Editor steps**

1. Top-right **Layers → Edit Layers…** → create user layer **“Target”**.
2. **GameObject → 3D Object → Cube**. Rename to **Target**.
3. Set **Target**’s **Layer = Target**.
4. **Add Component → XRSimpleInteractable** (on the Target).

**Why:** Layer filtering is a huge performance & clarity boost. We’ll make our ray only check **Target**.

**Mini-test:** The cube exists and shows **XRSimpleInteractable** in the Inspector.

**Common mistakes:**

* If you forget to put the cube on the **Target** layer, the ray (later) won’t register it.

---

# [✅ Added on Both ✋]🧱 Block 3 — Quick Ray (no code yet)

**Goal:** Prove the concept with a **built-in** ray so juniors see success immediately.

**Editor steps**

1. Expand **XR Origin (Action-based)** → find children **LeftHand Controller** and **RightHand Controller**.
2. On **RightHand Controller**: **Add Component → XR Ray Interactor**.
3. Also add **XR Interactor Line Visual** (to see the beam).
4. Set **XR Ray Interactor → Max Raycast Distance = 6**.
5. (We’ll set mask via code in the next block, but for a smoke test, leave defaults.)

**Mini-test:** *Play*; you should see a ray (always on) from right controller. It may hit lots of things—that’s ok for now.

**Why:** Immediate visual feedback builds confidence before coding.

**Common mistakes:**

* If the line doesn’t show, ensure **XR Interactor Line Visual** is enabled and the play camera is in view of something.

---

# [✅ Called script MySetupRayInteractors.cs]🧱 Block 4 — Configure Rays by Script

**Goal:** Add a manager that **adds/standardizes** rays on both controllers and restricts them to the **Target** layer.

**Create script** `SetupRayInteractors.cs` (in a **Scripts** folder) and **attach** it to an empty `RaycastManager` in the scene.
In the Inspector, drag **LeftHand Controller** and **RightHand Controller** onto the fields.

```csharp
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SetupRayInteractors : MonoBehaviour
{
    [Header("Assign controller GameObjects from XR Origin")]
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;

    [Header("Ray Tuning")]
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private float lineWidth = 0.01f;
    [SerializeField] private string raycastLayerName = "Target";

    private void Start()
    {
        ConfigureOne(leftController);
        ConfigureOne(rightController);
    }

    private void ConfigureOne(GameObject controllerGO)
    {
        if (!controllerGO)
        {
            Debug.LogWarning("[SetupRayInteractors] Missing controller reference.");
            return;
        }

        // Ensure ray exists, start disabled (perf-friendly)
        var ray = controllerGO.GetComponent<XRRayInteractor>();
        if (!ray) ray = controllerGO.AddComponent<XRRayInteractor>();
        ray.enabled = false;
        ray.maxRaycastDistance = maxDistance;

        // Restrict to the Target layer
        int layer = LayerMask.NameToLayer(raycastLayerName);
        ray.raycastMask = (layer == -1) ? ~0 : (1 << layer);

        // Visual line
        var line = controllerGO.GetComponent<XRInteractorLineVisual>();
        if (!line) line = controllerGO.AddComponent<XRInteractorLineVisual>();
        line.lineWidth = lineWidth;
        line.enabled = true;
    }
}
```

**Why:** “Separation of concerns.” One place controls both rays consistently (length, mask, visuals) and prevents missing components.

**Mini-test:** *Play* → no ray visible (by design). We’ll enable it via input in the next block.

**Common mistakes:**

* Forgetting to assign the **Left/Right controller** GameObjects in the Inspector.

---

# [✅ Called script MyVRInputManager.cs]🧱 Block 5 — Input-Driven Ray (Press to show)

**Goal:** Ray appears **only while a button is held** (event-driven; saves perf).

**Create script** `VRInputManager.cs`. **Add it** to **both** controllers (Left/Right).
**Assign InputActionReference** in Inspector:

* Left → `XRI LeftHand Interaction / Activate`
* Right → `XRI RightHand Interaction / Activate`
  (These live in the **XRI Default Input Actions** asset you imported from Samples. Use the Project search to find them.)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class VRInputManager : MonoBehaviour
{
    [Header("Drag the XRI ... / Activate action here")]
    [SerializeField] private InputActionReference activateAction;

    private XRRayInteractor ray;

    private void Awake()
    {
        ray = GetComponent<XRRayInteractor>();
        if (!ray)
            Debug.LogError("[VRInputManager] XRRayInteractor is required on the same GameObject.");
    }

    private void OnEnable()
    {
        if (activateAction != null)
        {
            activateAction.action.performed += OnPerformed;
            activateAction.action.canceled  += OnCanceled;
            activateAction.action.Enable();
        }

        if (ray) ray.enabled = false; // start hidden
    }

    private void OnDisable()
    {
        if (activateAction != null)
        {
            activateAction.action.performed -= OnPerformed;
            activateAction.action.canceled  -= OnCanceled;
            activateAction.action.Disable();
        }
    }

    private void OnPerformed(InputAction.CallbackContext _)
    {
        if (ray) ray.enabled = true;
    }

    private void OnCanceled(InputAction.CallbackContext _)
    {
        if (ray) ray.enabled = false;
    }
}
```

**Why:** Juniors clearly see the mapping: “input event → enable ray”. No polling in `Update()`.

**Mini-test:** *Play*. Hold **Activate** (usually Trigger/Grip per binding) → ray shows; release → ray hides.

**Common mistakes:**

* Assigned the wrong action (e.g., “Select” instead of “Activate”).
* Action reference left **None** (ray never turns on).

---

# [✅]🧱 Block 6 — Visual Feedback on Hover

**Goal:** When the ray **hovers** the Target, it changes color.

**Create two materials** in your project: `DefaultMat` (any color), `HitMat` (different color).
**Create script** `SimpleTarget.cs` and **add it** to the **Target** cube.
Make sure **XRSimpleInteractable** is also on the Target.

```csharp
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SimpleTarget : MonoBehaviour
{
    [Header("Assign materials")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material hitMaterial;

    private MeshRenderer meshRenderer;
    private XRSimpleInteractable simple;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        simple = GetComponent<XRSimpleInteractable>();

        if (!simple)
            Debug.LogError("[SimpleTarget] Requires XRSimpleInteractable on the same GameObject.");
    }

    private void Start()
    {
        if (meshRenderer && defaultMaterial)
            meshRenderer.material = defaultMaterial;
    }

    private void OnEnable()
    {
        if (simple != null)
        {
            simple.hoverEntered.AddListener(OnHoverEntered);
            simple.hoverExited.AddListener(OnHoverExited);
        }
    }

    private void OnDisable()
    {
        if (simple != null)
        {
            simple.hoverEntered.RemoveListener(OnHoverEntered);
            simple.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs _)
    {
        if (meshRenderer && hitMaterial)
            meshRenderer.material = hitMaterial;
    }

    private void OnHoverExited(HoverExitEventArgs _)
    {
        if (meshRenderer && defaultMaterial)
            meshRenderer.material = defaultMaterial;
    }
}
```

**Why:** Students learn the event model: the **Interactable** fires **hover** events; our script reacts.

**Mini-test:** *Play*. Hold **Activate** to show the ray. Aim at **Target** → it switches to `HitMat`. Move away → reverts to `DefaultMat`.

**Common mistakes:**

* Forgot **XRSimpleInteractable** on the Target.
* Didn’t assign the materials in the Inspector.

---

# [✅]🧱 Block 7 — Device Simulator (No Headset)

**Goal:** Practice without a headset.

**Editor steps**

1. Ensure **XR Device Simulator** is in your scene.
2. *Play*. Use these keys (defaults):

   * **Right mouse**: look around
   * **WASD / QE**: move
   * **1/2**: switch devices [✍️T/Y worked for me]
   * **T**: “select/activate” action (shows the ray) [✍️ Left click worked for me]
   * **X**: swap between left/right hand [✍️tab worked for me]

**Why:** Great for students at home without a headset.

**Mini-test:** Press **T** → ray appears; hover the cube → color changes.

**Common mistakes:**

* Scene camera not pointed at the Target: move closer with WASD.

---

# 🧱 Block 8 — Nice Polishes (choose any)

**A) Hide Ray when nothing valid is hit**[❓🤷Could not do it]

* On each **XRRayInteractor**, enable **Hide Raycast Line If No Hit** (or set **Line Visual → “Only Show When Hit”** style by code).
  **Why:** Cleaner UX.

**B) Per-hand responsibilities (UI vs 3D)**[❓🤷 The layer at top of inspector]

* Put UI on a **UI** layer; set **Left ray mask = UI**, **Right ray mask = Target**. 
  **Why:** Prevents “one ray hitting everything.”

**C) Small reticle**[❓🤷 Reticle remain even when ray disapear]

* Add a tiny sphere at the hit point via **XR Interactor Line Visual → Reticle** to help depth perception.

---

## [✅]Folder & Hierarchy Snapshot (for juniors)

```
Assets/
  Materials/
    DefaultMat.mat
    HitMat.mat
  Scripts/
    SetupRayInteractors.cs
    VRInputManager.cs
    SimpleTarget.cs
  XRI Samples/ (auto from package)
Scenes/
  Session6.unity

Hierarchy
└─ VR
   ├─ XR Origin (Action-based)
   │  ├─ Camera Offset
   │  │  ├─ Main Camera (from XR Origin)
   │  │  ├─ LeftHand Controller  (has XRRayInteractor + Line Visual + VRInputManager)
   │  │  └─ RightHand Controller (has XRRayInteractor + Line Visual + VRInputManager)
   ├─ XR Interaction Manager
   ├─ Event System
   └─ XR Device Simulator (optional)
└─ RaycastManager (SetupRayInteractors)
└─ Plane (floor)
└─ Target (Cube, Layer=Target, XRSimpleInteractable, SimpleTarget) [❓🤷Target the one at top of inspector or teh interaction layer mask at XRSimpleInteractable]
```

---

## What students learn (explicitly)

* **Interactor vs Interactable**: the “hand” casts a ray; the “cube” responds.
* **Events, not polling**: `hoverEntered/Exited`, `performed/canceled`.
* **Separation of concerns**:

  * `SetupRayInteractors` = build/configure rays
  * `VRInputManager` = connect input → enable/disable
  * `SimpleTarget` = visual feedback logic
* **Safety**: start rays **disabled** and enable only on demand (perf win).
* **Filtering**: layers keep interactions intentional and fast.

---

If you want, I can package this as a **Markdown lab sheet** (with checkboxes) or a **GitHub README** formatted exactly like your Lab-Guide repo so you can teach while building live.

