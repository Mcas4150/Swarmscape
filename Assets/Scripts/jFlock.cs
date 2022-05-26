

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jFlock : MonoBehaviour
{

    [Header("Simulation Settings")]  // these should not be changed at runtime
    public jBoid boidPrefab;
    public jFlock prey;
    public Food food;
    public bool predator;
    public bool herbivore;

    public int numBoids = 150;
    public float spawnRadius = 1f;
    public float boundaryRadius = 100.0f;

    [Header("Boid Settings")]

    public float maxVelocity = 1.75f;
    public float maxSteeringForce = 0.03f;
    public float seperationDistance = 35.0f;
    public float neighborDistance = 50.0f;
    [Range(0f, 360f)]
    public float fieldOfView = 300f;

    [Header("Force Multipliers")]
    public float seperationMultiplier = 1.5f;
    public float alignmentMultiplier = 1.0f;
    public float cohesionMultiplier = 1.0f;
    public float attractionMultiplier = 1.0f;
    public float boundaryMultiplier = 1.0f;

    public jBoid[] boids { get; private set; }


    void Start()
    {
        boids = new jBoid[numBoids];

        for (int i = 0; i < boids.Length; i++)
        {
            boids[i] = Instantiate(boidPrefab, transform.position, transform.rotation, transform);
            boids[i].SetupSimulation(this);
        }
    }

    void Update()
    {
        foreach (jBoid boid in boids)
        {
            boid.UpdateSimulation();
        }
    }
}