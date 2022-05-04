using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class TeleportLineToggle : MonoBehaviour
{
    public MagicPanel MagicPanel;
    bool teleStarted = false;
    bool targetsDisabled = false;
    public InputActionProperty teleportAction;
    // Start is called before the first frame update
    void Start()
    {
        teleportAction.action.performed += startTele;
        teleportAction.action.canceled += endTele;
    }

    // Update is called once per frame
    void Update()
    {
       
        if (!teleStarted || (MagicPanel.ZoomingLeft && MagicPanel.LeftToggled) || (MagicPanel.ZoomingRight && MagicPanel.RightToggled))
        {
            this.gameObject.GetComponent<LineRenderer>().widthMultiplier = 0;
            if (!targetsDisabled)
            {
                TeleportationTarget[] TeleportationAreas = FindObjectsOfType<TeleportationTarget>();
                for(int i = 0; i < TeleportationAreas.Length; i++)
                {
                    TeleportationAreas[i].GetComponent<MeshCollider>().enabled = false;
                }
                targetsDisabled = true;
            }
        }
        else
        {
            this.gameObject.GetComponent<LineRenderer>().widthMultiplier = 0.020f;
            if (targetsDisabled)
            {
                TeleportationTarget[] TeleportationAreas = FindObjectsOfType<TeleportationTarget>();
                for (int i = 0; i < TeleportationAreas.Length; i++)
                {
                    TeleportationAreas[i].GetComponent<MeshCollider>().enabled = true;
                }
                targetsDisabled = false;
            }
        }
        
            
    }

    void OnDestroy()
    {
        teleportAction.action.performed -= startTele;
        teleportAction.action.canceled -= endTele;
    }

    public void startTele(InputAction.CallbackContext context)
    {
        teleStarted = true;
    }
    public void endTele(InputAction.CallbackContext context)
    {
        teleStarted = false;
    }
}
