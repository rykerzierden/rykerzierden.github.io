using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chainsaw : MonoBehaviour
{
    public bool cutting = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Log>())
        {
            cutting = true;
        }
        if (other.GetComponent<Grabber>())
        {
            cutting = false;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        cutting = false;
    }
}
