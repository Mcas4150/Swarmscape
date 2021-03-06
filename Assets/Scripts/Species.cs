using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Species : MonoBehaviour
{
    [SerializeField] public Flock organics;
    [SerializeField] public Flock shadows;
    [SerializeField] public Flock second;
    [SerializeField] public Flock third;
    public List<Flock> allSpecies;


    private void Awake()
    {
        allSpecies = new List<Flock> { organics, shadows, second, third };




    }
    // Start is called before the first frame update
    public void InitializeSpecies()
    {

        Debug.Log("Species Start");


        foreach (Flock flock in allSpecies)
        {

            flock.OSC_Init();

        }

        foreach (Flock flock in allSpecies)
        {
            flock.InitializeAgents();
        }

        foreach (Flock flock in allSpecies)
        {
            flock.InitializeFlock();

        }

        foreach (Flock flock in allSpecies)
        {
            flock.BirthAgents();
        }



    }


}
