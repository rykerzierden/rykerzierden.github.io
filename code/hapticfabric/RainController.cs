using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RainController : MonoBehaviour
{
    public List<ControllerPulser> ControllerPulsers = new List<ControllerPulser>();
    public List<ModuleGroup> ModuleGroups = new List<ModuleGroup>();
    public GameObject Fire;
    public GameObject Rain;
    public GameObject Steam;
    public bool Raining = false;
    public InputActionProperty A;
    // Start is called before the first frame update
    void Start()
    {
        A.action.performed += ToggleRain;
    }

    // Update is called once per frame
    void Update()
    {
        if(Raining)
        {
            foreach (HapticCollider hc in FindObjectsOfType<HapticCollider>())
            {
                if (hc.Activated)
                {
                    return;
                }

            }
            foreach (Grabber g in FindObjectsOfType<Grabber>())
            {
                if (g.UsingHaptics)
                {
                    return;
                }
            }
            foreach (ModuleGroup mg in ModuleGroups)
            {
                mg.ActivatePulse(ModuleGroup.PulseTypes.RAIN);
            }
            foreach(ControllerPulser cp in ControllerPulsers)
            {
                cp.ActivatePulse(ModuleGroup.PulseTypes.RAIN);
            }
        }
    }

    void ToggleRain(InputAction.CallbackContext context)
    {

        Raining = !Raining;
        if (Raining)
        {
            Fire.GetComponent<HapticCollidable>().PulseType = ModuleGroup.PulseTypes.RING;
            Steam.SetActive(true);
            Rain.SetActive(true);
        }
        else
        {
            foreach (HapticCollider hc in FindObjectsOfType<HapticCollider>())
            {
                if (hc.Activated)
                {
                    return;
                }

            }
            foreach (Grabber g in FindObjectsOfType<Grabber>())
            {
                if (g.UsingHaptics)
                {
                    return;
                }
            }
            Fire.GetComponent<HapticCollidable>().PulseType = ModuleGroup.PulseTypes.NONE;
            Steam.SetActive(false);
            Rain.SetActive(false);

            foreach (ModuleGroup mg in ModuleGroups)
            {
                mg.DeactivatePulse();
            }
            foreach (ControllerPulser cp in ControllerPulsers)
            {
                cp.DeactivatePulse();
            }
        }
    }
}
