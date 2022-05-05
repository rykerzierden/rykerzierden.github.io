using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleDemo : MonoBehaviour
{
    //public ModuleGroup Left;
    public ModuleGroup Vib;
    public bool SirenPulse = false;
    public bool RingPulse = false;
    public bool HalfPulse = false;
    public bool NonePulse = false;
    public bool BackSweepPulse = false;
    public bool ForwardSweepPulse = false;
    public bool RainPulse = false;

    // Start is called before the first frame update
    void Start()
    {
        if(Vib == null)
        {
            Vib = this.GetComponentInParent<ModuleGroup>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SirenPulse)
        {
            Vib.ActivatePulse(ModuleGroup.PulseTypes.SIREN);
        }
        else if (RingPulse)
        {
            Vib.ActivatePulse(ModuleGroup.PulseTypes.RING);
        }
        else if (HalfPulse)
        {
            Vib.ActivatePulse(ModuleGroup.PulseTypes.HALF);
        }
        else if (NonePulse)
        {
            Vib.ActivatePulse(ModuleGroup.PulseTypes.NONE);
        }
        else if (BackSweepPulse)
        {
            Vib.ActivatePulse(ModuleGroup.PulseTypes.SWEEPBACK);
        }
        else if (ForwardSweepPulse)
        {
            Vib.ActivatePulse(ModuleGroup.PulseTypes.SWEEPFORWARD);
        }
        else if (RainPulse)
        {
            Vib.ActivatePulse(ModuleGroup.PulseTypes.RAIN);
        }
        else
        {
            Vib.DeactivatePulse();
        }
        //Test.Activate();
        //Left.Activate(4.5f);
        //Right.Activate(4.5f);
        //new WaitForSeconds(5f);
    }
}
