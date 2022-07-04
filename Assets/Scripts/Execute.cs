using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;

public class Execute : MonoBehaviour
{
    public OSCTransmitter transmitter;
    public Species species;
    public Foods foods;
    // Start is called before the first frame update


    void Start()
    {
        species.InitializeSpecies();
        foods.InitializeFoods();
        OSC_InitializeMax();
    }

    // Update is called once per frame

    public void OSC_InitializeMax()
    {
        OSCMessage resetMessage = new OSCMessage("/play", OSCValue.Int(1));
        Debug.Log("play");
        transmitter.Send(resetMessage);
    }

}
