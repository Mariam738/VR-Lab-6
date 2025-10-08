using UnityEngine;
using UnityEngine.InputSystem;


public class MyVRInputManager : MonoBehaviour
{
    [Header("Drag the XRI ... / Activate action here")]
    [SerializeField] private InputActionReference activateAction;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor ray;

    private void Awake()
    {
        ray = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
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