using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{




    public MeshRenderer GroundMesh;
    public Material CurrentMaterial;
    public Material SpringMaterial;
    public Material SummerMaterial;
    public Material FallMaterial;
    public Material WinterMaterial;

    public Color ColorSpring = Color.green;
    public Color ColorSummer = Color.red;
    public Color ColorFall = Color.gray;
    public Color ColorWinter = Color.cyan;

    public Color colorStart;
    public Color colorEnd;

    float duration = 90f;

    // Start is called before the first frame update
    void Start()
    {

        //GroundMesh = GetComponent<MeshRenderer>();
        CurrentMaterial = GroundMesh.material;

    }
    // Update is called once per frame
    void Update()
    {
        float lerp = Mathf.PingPong(Time.time, duration) / duration;
        CurrentMaterial.color = Color.Lerp(colorStart, colorEnd, lerp);
    }


    void ColorInterpolate()
    {

    }
}
