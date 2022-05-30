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

    // Start is called before the first frame update
    void Start()
    {

        //GroundMesh = GetComponent<MeshRenderer>();
        CurrentMaterial = GroundMesh.material;

    }
    // Update is called once per frame
    void Update()
    {

    }
}
