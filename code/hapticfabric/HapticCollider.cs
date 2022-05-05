using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class HapticCollider : MonoBehaviour
{
    public ModuleGroup ModuleGroup;
    public Grabber ThisHandGrabber;
    public ControllerPulser HapticController;
    public ModuleGroup.PulseTypes PulseType;
    public bool HasController = false;
    public bool HasModuleGroup = false;
    public bool Activated = false;
    public bool PulseDeactivated = false;
    // Start is called before the first frame update
    void Start()
    {
        if(!(ModuleGroup is null))
        {
            HasModuleGroup = true;
        }
        if(!(HapticController is null))
        {
            HasController = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ThisHandGrabber.UsingHaptics)
        {
            return;
        }
        if(!HasController && !HasModuleGroup)
        {
            return;
        }
        if (Activated)
        {
            if (HasModuleGroup)
            {
                //if(!ModuleGroup.PulseActivated)
                    ModuleGroup.ActivatePulse(PulseType);
            }
            if (HasController)
            {
                //if(!HapticController.PulseActivated)
                    HapticController.ActivatePulse(PulseType);
            }
        }
        else
        {
            if (HasModuleGroup && !PulseDeactivated && !FindObjectOfType<RainController>().Raining)
            {
                //if(!ModuleGroup.PulseActivated)
                ModuleGroup.DeactivatePulse();
            }
            if (HasController && !PulseDeactivated && !FindObjectOfType<RainController>().Raining)
            {
                //if(!HapticController.PulseActivated)
                HapticController.DeactivatePulse();
            }
            PulseDeactivated = true;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<HapticCollidable>())
        {
            Activated = other.GetComponent<HapticCollidable>().enabled;
            PulseType = other.GetComponent<HapticCollidable>().PulseType;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<HapticCollidable>())
        {
            Activated = false;
            PulseDeactivated = false;
            PulseType = ModuleGroup.PulseTypes.NONE;
        }

    }
}
