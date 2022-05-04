using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CometCollider : MonoBehaviour
{
    // Start is called before the first frame update
    public Text textbox;
    public GameObject colliding;
    public NearEarthComets NearEarthComets;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.GetComponent<CometReference>())
        //{
        //    Comet CollidedComet = other.gameObject.GetComponent<CometReference>().Comet;

        //    textbox.text = "Name: " + CollidedComet.Name + "\nPeriod: " + CollidedComet.Period;
        //    Debug.Log("TRIGGERED!!!");
        //    colliding = other.gameObject;
        //}
        //else
        //{

        //}
    }
    private void OnTriggerExit(Collider other)
    {

        //if (other.gameObject.GetComponent<CometReference>() && other.gameObject == colliding)
        //{
        //    textbox.text = "";
        //    colliding = null;
        //}
        //NearEarthComets.CometsNeedUpdate = true;

        
    }
}
