using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;

public class Execute : MonoBehaviour
{

    public Species species;
    public OSCTransmitter transmitter;
    // Start is called before the first frame update


    void Start()
    {
        species.InitializeSpecies();
        OSCInit();
    }

    // Update is called once per frame


    public void OSCInit()
    {
        OSCMessage resetMessage = new OSCMessage("/play", OSCValue.Int(1));
        Debug.Log("play");
        transmitter.Send(resetMessage);
    }
}
