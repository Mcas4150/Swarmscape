using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ecosystem : MonoBehaviour
{

    [SerializeField] private Flock OrganicFlock;
    [SerializeField] private Flock ShadowFlock;
    [SerializeField] private Food food;
    [SerializeField] private World world;

    public int worldYear = 0;
    public string season = "spring";

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CountWorldYear());
        setSeason("spring");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator CountWorldYear()
    {
        while (true)
        {

            yield return new WaitForSeconds(1f);
            worldYear += 1;

            if (worldYear == 90)
            { setSeason("summer"); }
            else if (worldYear == 180)
            { setSeason("fall"); }
            else if (worldYear == 270)
            { setSeason("winter"); }
            else if (worldYear == 360)
            {
                setSeason("Spring");
                worldYear = 0;

            }

        }
    }

    public void setSeason(string newSeason)
    {
        switch (newSeason)
        {
            case "spring":
                season = "spring";
                world.ground.CurrentMaterial = world.ground.SpringMaterial;
                //flockSize = 8;
                break;

            case "summer":
                season = "summer";
                world.ground.CurrentMaterial = world.ground.SummerMaterial;

                // flockSize = 16;
                break;

            case "fall":
                season = "fall";
                world.ground.CurrentMaterial = world.ground.FallMaterial;
                break;

            case "winter":
                season = "winter";
                world.ground.CurrentMaterial = world.ground.WinterMaterial;
                break;
        }



    }


}
