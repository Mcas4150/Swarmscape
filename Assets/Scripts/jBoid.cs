using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jBoid : MonoBehaviour
{
    public Vector3 position { get; private set; }
    public Vector3 velocity { get; private set; }
    public Vector3 acceleration { get; private set; }

    public int[] neighborIndices { get; private set; }
    public int[] preyIndices { get; private set; }
    public Vector3 seperation { get; private set; }
    public Vector3 alignment { get; private set; }
    public Vector3 attraction { get; private set; }
    public Vector3 cohesion { get; private set; }
    public Vector3 boundary { get; private set; }

    private jFlock flock;



    /* Updates the boids transform component every frame based on the most
	recently generated simulation variables. */
    void Update()
    {
        // update the transform position based on the velocity.
        transform.localPosition = position;

        // points the boid in the direction of movement.
        transform.localRotation = Quaternion.LookRotation(velocity, Vector3.up);
    }



    /* Initializes the simulation variables. */
    public void SetupSimulation(jFlock flock)
    {
        this.flock = flock;

        position = UnityEngine.Random.insideUnitSphere.normalized * flock.spawnRadius;
        velocity = UnityEngine.Random.insideUnitSphere.normalized;
        acceleration = Vector3.zero;
    }



    /* Updates the simulation variables based on a few simple rules. */
    public void UpdateSimulation()
    {
        jBoid[] boids = flock.boids;
        jFlock prey = flock.prey;
        jBoid[] preyBoid = prey.boids;
        Food food = flock.food;
        FoodUnit[] foods = food.Foods;

        neighborIndices = GetNeighborIndices(ref boids);
        preyIndices = GetNeighborIndices(ref preyBoid);

        // reset the forces.
        seperation = Vector3.zero;
        alignment = Vector3.zero;
        cohesion = Vector3.zero;
        boundary = Vector3.zero;

        // and update them if there are neighbors influencing the boid.
        if (neighborIndices.Length > 0)
        {
            seperation = Seperation(ref boids, neighborIndices) * flock.seperationMultiplier;
            alignment = Alignment(ref boids) * flock.alignmentMultiplier;
            cohesion = Cohesion(ref boids) * flock.cohesionMultiplier;
        }

        if (flock.predator == true && preyIndices.Length > 0)
        {
            attraction = Attraction(ref preyBoid, preyIndices) * flock.attractionMultiplier;
        }
        else
        {
            seperation += Seperation(ref preyBoid, preyIndices) * flock.attractionMultiplier;
        }

        boundary = Boundary() * flock.boundaryMultiplier;

        // set the acceleration as the sum of the resulting forces.
        acceleration = (seperation + alignment + cohesion + attraction + boundary);

        // update the velocity based on the acceleration.
        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, flock.maxVelocity);

        // update the position based on the velocity.
        position += velocity;
    }



    /* Returns the indices of the neighbors of this boid. */
    int[] GetNeighborIndices(ref jBoid[] boids)
    {
        /* we don't know how many neighbors there are ahead of time, so we store them in 
		a variable length data structure initially. */
        List<int> indices = new List<int>();

        for (int i = 0; i < boids.Length; i++)
        {
            float dist = Vector3.Distance(position, boids[i].position);
            float angle = Vector3.Angle(transform.forward, boids[i].position - position);

            /* if the neighbor is not the current boid, and is within the neighbor distance and is
			visible based based on the boids field of view, then add it to the list of neighbors. */
            if (dist > 0 && dist <= flock.neighborDistance && angle < flock.fieldOfView / 2f)
            {
                indices.Add(i);
            }
        }

        // convert the List to an array and return it.
        int[] indicesArray = new int[indices.Count];
        for (int i = 0; i < indicesArray.Length; i++)
        {
            indicesArray[i] = indices[i];
        }

        return indicesArray;
    }

    int[] GetPreyIndices(ref jBoid[] preyBoid)
    {
        /* we don't know how many neighbors there are ahead of time, so we store them in 
		a variable length data structure initially. */
        List<int> indices = new List<int>();

        for (int i = 0; i < preyBoid.Length; i++)
        {
            float dist = Vector3.Distance(position, preyBoid[i].position);
            float angle = Vector3.Angle(transform.forward, preyBoid[i].position - position);

            /* if the neighbor is not the current boid, and is within the neighbor distance and is
			visible based based on the boids field of view, then add it to the list of neighbors. */
            if (dist > 0 && dist <= flock.neighborDistance && angle < flock.fieldOfView / 2f)
            {
                indices.Add(i);
            }
        }

        // convert the List to an array and return it.
        int[] indicesArray = new int[indices.Count];
        for (int i = 0; i < indicesArray.Length; i++)
        {
            indicesArray[i] = indices[i];
        }

        return indicesArray;
    }




    Vector3 Attraction(ref jBoid[] prey, int[] indices)
    {
        Vector3 force = Vector3.zero;

        foreach (int i in indices)
        {
            // get a vector pointing in the opposite direction of the neighbor
            Vector3 neighborDir = prey[i].position - position;
            float neighborDist = neighborDir.magnitude;
            neighborDir = Vector3.Normalize(neighborDir);

            // weight the vector by the distance squared
            neighborDir = neighborDir / (Mathf.Pow(neighborDist, 2f));
            force += neighborDir;
        }

        // we implement reynolds "force = desired velocity - current velocity"
        force = Vector3.Normalize(force);
        force = force * flock.maxVelocity;
        force = force - velocity;
        force = Vector3.ClampMagnitude(force, flock.maxSteeringForce);

        return force;
    }


    /* Steers a boid a certain distance from its neighbors to avoid collisions. */
    Vector3 Seperation(ref jBoid[] boids, int[] indices)
    {
        Vector3 force = Vector3.zero;

        foreach (int i in indices)
        {
            // get a vector pointing in the opposite direction of the neighbor
            Vector3 neighborDir = position - boids[i].position;
            float neighborDist = neighborDir.magnitude;
            neighborDir = Vector3.Normalize(neighborDir);

            // weight the vector by the distance squared
            neighborDir = neighborDir / (Mathf.Pow(neighborDist, 2f));
            force += neighborDir;
        }

        // we implement reynolds "force = desired velocity - current velocity"
        force = Vector3.Normalize(force);
        force = force * flock.maxVelocity;
        force = force - velocity;
        force = Vector3.ClampMagnitude(force, flock.maxSteeringForce);

        return force;
    }



    /* Steers a boid in the average direction of its neighbors. */
    Vector3 Alignment(ref jBoid[] boids)
    {
        Vector3 force = Vector3.zero;

        foreach (int i in neighborIndices)
        {
            force += boids[i].velocity;
        }

        force = Vector3.Normalize(force);
        force = force * flock.maxVelocity;
        force = force - velocity;
        force = Vector3.ClampMagnitude(force, flock.maxSteeringForce);

        return force;
    }



    /* Steers a boid toward the average position of its neighbors. */
    Vector3 Cohesion(ref jBoid[] boids)
    {
        Vector3 centerOfMass = Vector3.zero;

        foreach (int i in neighborIndices)
        {
            centerOfMass += boids[i].position;
        }
        centerOfMass = centerOfMass / (float)neighborIndices.Length;

        Vector3 force = centerOfMass - position;
        force = Vector3.Normalize(force);
        force = force * flock.maxVelocity;
        force = force - velocity;
        force = Vector3.ClampMagnitude(force, flock.maxSteeringForce);

        return force;
    }



    /* Steers a boid back towards the boundaryRadius, with a stronger
	force as the boid gets further out. */
    Vector3 Boundary()
    {
        float dist = Vector3.Distance(position, Vector3.zero);

        Vector3 force = Vector3.zero;
        if (dist > flock.boundaryRadius)
        {
            force = -position;
            force = Vector3.Normalize(force);
            force = force * flock.maxVelocity;
            force = force - velocity;
            force = Vector3.ClampMagnitude(force, flock.maxSteeringForce);

            // strengthen the force as the boid gets farther out, to encourage return
            Vector3 weak = force * Mathf.Abs(dist / flock.boundaryRadius);
            Vector3 strong = force * Mathf.Abs(dist - flock.boundaryRadius);
            float t = Mathf.Abs((dist - flock.boundaryRadius) / Mathf.Pow(flock.boundaryRadius, 2f));
            force = Vector3.Lerp(weak, strong, Mathf.Clamp(t, 0f, 1f));
        }

        return force;
    }
}





//private void runFromSharks()
//{
//    if (sharkDistance < comfort_distance)
//    {
//        float step = 10.0f * Time.deltaTime;
//        // set target direction
//        Vector3 targetDir = sharkPosition - transform.position;
//        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, -step, 0.0F);
//        transform.rotation = Quaternion.LookRotation(newDir);
//        rigidbody.AddForce(speed * Vector3.Normalize(transform.position - sharkPosition));
//    }
//}