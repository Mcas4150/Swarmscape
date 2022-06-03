using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flowfield : MonoBehaviour
{
    // Start is called before the first frame update
    // Declaring a 2D array of Vector2's
    public int BoundsDistance = 200;

    // Resolution of grid relative to window width and height in pixels
    public int resolution = 1;

    public float flowForce;

    public Vector3[,,] field;
    public FastNoise fastNoise;
    // How many columns and how many rows in the grid?

    public int columns, rows, aisles;
    public int gridsize;
    public float increment;

    // Maximum bounds of the screen
    public float maximumPos;   // Declaring a 2D array of Vector2's

    private void Awake()
    {

        aisles = gridsize;
        columns = gridsize;
        rows = gridsize;
        //columns = BoundsDistance / resolution;
        //rows = BoundsDistance / resolution;
        //aisles = BoundsDistance / resolution;
        maximumPos = BoundsDistance / 2;

        field = new Vector3[columns, rows, aisles];
        //InitializeFlowField();
    }

    private void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {

    }


    public void OnDrawGizmos()
    {
        fastNoise = new FastNoise();
        float xOff = 0;
        for (int x = 0; x < columns; x++)
        {
            float yOff = 0;
            for (int y = 0; y < rows; y++)
            {
                float zOff = 0;
                for (int z = 0; z < aisles; z++)
                {
                    //float noise = 1;
                    float noise = fastNoise.GetSimplex(xOff, yOff + zOff) + 1;

                    Vector3 noiseDirection = new Vector3(Mathf.Cos(noise * Mathf.PI), Mathf.Sin(noise * Mathf.PI), Mathf.Cos(noise * Mathf.PI));
                    //Debug.Log(noiseDirection);
                    field[x, y, z] = Vector3.Normalize(noiseDirection);

                    Gizmos.color = new Color(noiseDirection.normalized.x, noiseDirection.normalized.y, noiseDirection.normalized.z, 1f);
                    Vector3 pos = new Vector3(x, y, z) + transform.position;
                    Vector3 endpos = pos + Vector3.Normalize(noiseDirection);
                    Gizmos.DrawLine(pos, endpos);
                    Gizmos.DrawSphere(endpos, 0.1f);
                    zOff += increment;
                    //field[x, y, z] = new Vector3(1, 0, 0);

                }
                yOff += increment;
            }
            xOff += increment;
        }
    }

    //public void InitializeFlowField()
    //{
    //    fastNoise = new FastNoise();
    //    float xOff = 0;
    //    for (int x = 0; x < columns; x++)
    //    {
    //        float yOff = 0;
    //        for (int y = 0; y < rows; y++)
    //        {
    //            float zOff = 0;
    //            for (int z = 0; z < aisles; z++)
    //            {
    //                //float noise = 1;
    //                float noise = fastNoise.GetSimplex(xOff, yOff + zOff) + 1;

    //                Vector3 noiseDirection = new Vector3(Mathf.Cos(noise * Mathf.PI), Mathf.Sin(noise * Mathf.PI), Mathf.Cos(noise * Mathf.PI));
    //                //Debug.Log(noiseDirection);
    //                field[x, y, z] = Vector3.Normalize(noiseDirection);

    //                //Gizmos.color = Color.white;
    //                Vector3 pos = new Vector3(x, y, z) + transform.position;
    //                Vector3 endpos = pos + Vector3.Normalize(noiseDirection);
    //                Debug.DrawLine(pos, endpos, Color.white);
    //                zOff += 1;
    //                //field[x, y, z] = new Vector3(1, 0, 0);

    //            }
    //            yOff += 1;
    //        }
    //        xOff += 1;
    //    }
    //}



    public Vector3 Lookup(Vector3 _lookUp)
    {
        float x = 0 + (_lookUp.x - -maximumPos) * ((columns - 1 - 0) / (maximumPos - -maximumPos));
        float y = 0 + (_lookUp.y - -maximumPos) * ((rows - 1 - 0) / (maximumPos - -maximumPos));
        float z = 0 + (_lookUp.z - -maximumPos) * ((aisles - 1 - 0) / (maximumPos - -maximumPos));

        // A method to return a Vector2 based on a location
        int column = (int)Mathf.Clamp(x, 0, columns - 1);
        int row = (int)Mathf.Clamp(y, 0, rows - 1);
        int aisle = (int)Mathf.Clamp(z, 0, aisles - 1);
        return field[column, row, aisle];
    }
}
