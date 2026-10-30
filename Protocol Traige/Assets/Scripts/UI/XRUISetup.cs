using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Zorgt ervoor dat alle World Space Canvassen automatisch de juiste camera,
/// TrackedDeviceGraphicRaycaster en het VR Virtuele Toetsenbord hebben.
/// </summary>
public class XRUISetup : MonoBehaviour
{
    private void Start()
    {
        InitialiseerCanvassen();
        ActiveerInteractors();
        InitialiseerVirtueelToetsenbord();
    }

    private void InitialiseerCanvassen()
    {
        Camera mainCam = Camera.main;

        Canvas[] alleCanvassen = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in alleCanvassen)
        {
            if (canvas.renderMode != RenderMode.WorldSpace)
                continue;

            // 1. Zorg dat de event camera is ingesteld op de XR Main Camera
            if (canvas.worldCamera == null && mainCam != null)
            {
                canvas.worldCamera = mainCam;
            }

            // 2. Vervang GraphicRaycaster door TrackedDeviceGraphicRaycaster
            GraphicRaycaster standaardRaycaster = canvas.GetComponent<GraphicRaycaster>();
            if (standaardRaycaster != null && !(standaardRaycaster is TrackedDeviceGraphicRaycaster))
            {
                Destroy(standaardRaycaster);
            }

            if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
            {
                var trackedRaycaster = canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
                trackedRaycaster.checkFor3DOcclusion = false; // Voorkom dat omringende colliders de UI blokkeren
            }
        }
    }

    private void ActiveerInteractors()
    {
        NearFarInteractor[] nearFarInteractors = FindObjectsByType<NearFarInteractor>(FindObjectsSortMode.None);
        foreach (var interactor in nearFarInteractors)
        {
            interactor.enableUIInteraction = true;
        }

        XRRayInteractor[] rayInteractors = FindObjectsByType<XRRayInteractor>(FindObjectsSortMode.None);
        foreach (var interactor in rayInteractors)
        {
            interactor.enableUIInteraction = true;
        }
    }

    private void InitialiseerVirtueelToetsenbord()
    {
        if (FindFirstObjectByType<VRVirtualKeyboard>() == null)
        {
            GameObject kbObj = new GameObject("VR_Keyboard_Manager");
            kbObj.AddComponent<VRVirtualKeyboard>();
        }
    }
}
