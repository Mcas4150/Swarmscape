using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;

public class Ecosystem : MonoBehaviour
{

    [SerializeField] private Flock OrganicFlock;
    [SerializeField] private Flock ShadowFlock;
    [SerializeField] private Foods foods;
    [SerializeField] private World world;

    public Boolean initialized = false;


    public int worldDay = 0;
    public int worldYear = 0;
    public string season = "spring";

    [Header("Season Properties")]
    public int bloomMax;
    public float bloomTime;

    [Header("OSC Properties")]
    public OSCTransmitter transmitter;




    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(CountWorldTime());
        //StartCoroutine(Bloom());
        setSeason("spring");
    }


    private IEnumerator Bloom()
    {
        while (true)
        {

            yield return new WaitForSeconds(bloomTime);

            foreach (Food food in foods.allFoods)
            {
                if (food.Foods.Count < bloomMax && food.foodAvailable.Count != 0)
                {
                    food.EnableFoods(1);
                }
            }

        }
    }

    private IEnumerator CountWorldTime()
    {
        while (true)
        {

            yield return new WaitForSeconds(1f);
            worldDay += 1;

            if (worldDay == 90)
            { setSeason("summer"); }
            else if (worldDay == 180)
            { setSeason("fall"); }
            else if (worldDay == 270)
            { setSeason("winter"); }
            else if (worldDay == 360)
            {
                worldYear++;
                setSeason("spring");
                worldDay = 0;

            }

        }
    }

    public void setSeason(string newSeason)
    {
        switch (newSeason)
        {
            case "spring":
                season = "spring";
                //if (food.Foods.Count < 1)
                //if (food.initialized) food.EnableFoods(food.foodSize);
                bloomTime = 0.5f;
                //StartCoroutine(Bloom(0.5f));
                StartCoroutine(Bloom());
                //StartCoroutine(Bloom());
                //world.ground.CurrentMaterial = world.ground.SpringMaterial;
                world.ground.colorStart = world.ground.ColorSpring;
                world.ground.colorEnd = world.ground.ColorSummer;
                //flockSize = 8;
                break;

            case "summer":
                season = "summer";

                bloomTime = 2;
                //food.EnableFoods(15);
                //StopCoroutine(Bloom(0.5f));
                //StartCoroutine(Bloom(5f));
                //world.ground.CurrentMaterial = world.ground.SummerMaterial;
                world.ground.colorStart = world.ground.ColorSummer;
                world.ground.colorEnd = world.ground.ColorFall;
                // flockSize = 16;
                break;

            case "fall":
                season = "fall";

                StopCoroutine(Bloom());
                ShadowFlock.Carnivore = true;
                //StopCoroutine(Bloom(5f));
                //world.ground.CurrentMaterial = world.ground.FallMaterial;
                world.ground.colorStart = world.ground.ColorFall;
                world.ground.colorEnd = world.ground.ColorWinter;
                break;

            case "winter":
                season = "winter";
                //world.ground.CurrentMaterial = world.ground.WinterMaterial;
                world.ground.colorStart = world.ground.ColorWinter;
                world.ground.colorEnd = world.ground.ColorSpring;
                break;
        }



    }


}
