using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneEnable : MonoBehaviour
{
    public InputActionProperty TeleportR;
    public InputActionProperty TeleportL;
    public GameObject RightController;
    public GameObject LeftController;
    // Start is called before the first frame update
    void Start()
    {
        TeleportR.action.performed += ShowPlaneAndLaser;
        TeleportL.action.performed += ShowPlaneAndLaser;
        TeleportR.action.canceled += HidePlaneAndLaser;
        TeleportL.action.canceled += HidePlaneAndLaser;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowPlaneAndLaser(InputAction.CallbackContext context)
    {
        Behaviour TeleportLineR = (Behaviour)RightController.GetComponent("XRInteractorLineVisual");
        Behaviour TeleportLineL = (Behaviour)LeftController.GetComponent("XRInteractorLineVisual");
        TeleportLineL.enabled = true;
        TeleportLineR.enabled = true;
        this.gameObject.GetComponent<MeshRenderer>().enabled = true;

    }
    void HidePlaneAndLaser(InputAction.CallbackContext context)
    {
        Behaviour TeleportLineR = (Behaviour)RightController.GetComponent("XRInteractorLineVisual");
        Behaviour TeleportLineL = (Behaviour)LeftController.GetComponent("XRInteractorLineVisual");
        TeleportLineL.enabled = false;
        TeleportLineR.enabled = false;
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
}
