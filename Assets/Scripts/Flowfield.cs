using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flowfield : MonoBehaviour
{
    // Start is called before the first frame update
    // Declaring a 2D array of Vector2's
    private Vector3[,,] field;

    // How many columns and how many rows in the grid?
    private int columns, rows;

    // Resolution of grid relative to window width and height in pixels
    private int resolution;

    // Maximum bounds of the screen
    private Vector3 maximumPos;   // Declaring a 2D array of Vector2's

    // Update is called once per frame
    void Update()
    {

    }
}
