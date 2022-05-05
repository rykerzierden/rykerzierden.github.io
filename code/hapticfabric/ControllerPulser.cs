using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using OVR;

public class ControllerPulser : MonoBehaviour
{
    public ModuleGroup.PulseTypes PulseType;
    public OVRInput.Controller ThisController;
    public bool TimeActivated;
    public bool PulseActivated;
    public float ActivationTime;
    public float OnTime = 0f;
    public float OffTime = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //OVRInput.Update();
        // Pulse Code
        if (PulseActivated)
        {

            if (TimeActivated)
            {
                ActivationTime -= Time.deltaTime;
                if (ActivationTime < 0f)
                {
                    PulseActivated = false;
                }
            }
            if (ActivationTime > 0f || !TimeActivated)
            {
                switch (PulseType)
                {
                    case ModuleGroup.PulseTypes.NONE:
                        OVRInput.SetControllerVibration(1f, 1.0f,ThisController);
                        break;
                    case ModuleGroup.PulseTypes.RAIN:
                        if (OnTime > 0f)
                        {
                            OVRInput.SetControllerVibration(1f, 1.0f, ThisController);
                            OnTime -= Time.deltaTime;
                        }
                        else if (OffTime > 0f)
                        {
                            OVRInput.SetControllerVibration(0f, 0f, ThisController);
                            OffTime -= Time.deltaTime;
                        }
                        else
                        {
                            OffTime = 0.5f*UnityEngine.Random.value;
                            OnTime = 0.1f * UnityEngine.Random.value;
                        }
                        break;
                    case ModuleGroup.PulseTypes.SIREN:
                        if (OnTime > 0f)
                        {
                            OVRInput.SetControllerVibration(1.0f, 1.0f, ThisController);
                            OnTime -= Time.deltaTime;
                        }
                        else if (OffTime > 0f)
                        {
                            OVRInput.SetControllerVibration(1.0f, 0.5f, ThisController);
                            OffTime -= Time.deltaTime;
                        }
                        else
                        {
                            OffTime = 0.5f;
                            OnTime = 0.5f;
                        }
                        break;
                    case ModuleGroup.PulseTypes.HALF:
                        OVRInput.SetControllerVibration(0.1f, 0.4f, ThisController);
                        break;
                    case ModuleGroup.PulseTypes.SWEEPBACK:
                        if (OnTime > 0f)
                        {
                            OVRInput.SetControllerVibration(1.0f, 1.0f, ThisController);
                            OnTime -= Time.deltaTime;
                        }
                        else
                        {
                            OnTime = 0.3f;
                        }
                        break;
                    case ModuleGroup.PulseTypes.SWEEPFORWARD:
                        if (OnTime > 0f)
                        {
                            OVRInput.SetControllerVibration(1.0f, 1.0f, ThisController);
                            OnTime -= Time.deltaTime;
                        }
                        else
                        {
                            OnTime = 0.3f;
                        }
                        break;
                }
            }
            // no longer pulsing, reset all the values
            if (!PulseActivated)
            {
                OnTime = 0f;
                OffTime = 0f;
                ActivationTime = 0f;
                OVRInput.SetControllerVibration(0f, 0f, ThisController);
            }
        }

    }
    public void Activate(float time)
    {
        ActivationTime = time;
        ActivatePulse(time, ModuleGroup.PulseTypes.NONE);
    }
    public void ActivatePulse(ModuleGroup.PulseTypes PulseType)
    {
        this.PulseType = PulseType;
        PulseActivated = true;
        TimeActivated = false;
    }
    public void ActivatePulse(float time, ModuleGroup.PulseTypes PulseType)
    {
        this.PulseType = PulseType;
        PulseActivated = true;
        ActivationTime = time;
    }
    public void DeactivatePulse()
    {
        OVRInput.SetControllerVibration(0f, 0f, ThisController);
        this.PulseType = ModuleGroup.PulseTypes.NONE;
        ActivationTime = 0f;
        PulseActivated = false;
    }
}
