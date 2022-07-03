using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Older : MonoBehaviour
{
    // Start is called before the first frame update

    //public event EventHandler<BoidDeathEventArgs> Death;











    //var transmitter = gameObject.AddComponent<OSCTransmitter>();
    //transmitter.RemoteHost = "127.0.0.1";
    //// Set remote port;
    //transmitter.RemotePort = 8111;
    ////resetMessage = new OSCMessage("/play/", OSCValue.Int(1));
    //OSCMessage resetMessage = new("/organic/lifestate/1", OSCValue.Float(1));
    //Debug.Log("play");
    //transmitter.Send(resetMessage);


    //private IEnumerator HealthSize()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(0.25f);

    //        var healthRatio = health * 0.025f;
    //        //myTransform.localScale = new Vector3(healthRatio, healthRatio, healthRatio);

    //    }
    //}



    ////code with FOV

    //private Vector3 CalculateSeparation(List<FlockUnit> neighbors, float forceMagnitude)
    //{
    //    var force = Vector3.zero;
    //    //if (neighbors.Count == 0)
    //    //    return Vector3.zero;
    //    //int neighborsInFOV = 0;
    //    for (int i = 0; i < neighbors.Count; i++)
    //    {
    //        //if (IsInFOV(avoidanceNeighbors[i].transform.position))
    //        //{
    //        //neighborsInFOV++;
    //        force += Avoid(neighbors[i].transform.position);
    //        //}
    //    }

    //    //avoidanceVector /= neighborsInFOV;
    //    force = ApplyForce(force, forceMagnitude);
    //    //Debug.DrawLine(force, transform.position, Color.red);
    //    return force;
    //}









    //private Vector3 CalculateWander()
    //{
    //    var wanderTarget = UnityEngine.Random.onUnitSphere;
    //    return wanderTarget;
    //}

    //private Vector3 CalculateBorders() {

    //    var offsetToCenter = assignedFlock.transform.position - transform.position;
    //    float distanceFromCenter = offsetToCenter.magnitude;
    //    bool isNearCenter = (distanceFromCenter >= assignedFlock.boundsDistance * 0.95f);


    //    //Vector3 neighborDir = (neighbors[i].transform.position - transform.position);
    //    //float neighborDist = neighborDir.magnitude;
    //    neighborDir = CustomNormalize(neighborDir);
    //    // weight the vector by the distance squared
    //    neighborDir /= (Mathf.Pow(neighborDist, 2f));
    //    force += neighborDir;


    //    force = CustomNormalize(force);
    //    force *= totalSpeed;
    //    force -= currentMoveVector;
    //    force = Vector3.ClampMagnitude(force, forceMagnitude);
    //    return isNearCenter ? offsetToCenter.normalized : Vector3.zero;
    //}


    //public Vector3 Attract(Vector3 targetPosition, float maxForce)
    //{
    //    Vector3 force = myTransform.position - targetPosition;
    //    float distance = force.magnitude;
    //    distance = Mathf.Clamp(distance, 2f, 25f);
    //    //force.Normalize();
    //    force = CustomNormalize(force);

    //    float strength = G * (boidMass * boidMass) / (distance * distance);
    //    force *= strength;
    //    force = Vector3.ClampMagnitude(force, maxForce);
    //    return force;
    //}

    ///

    //public float AverageVelocity(Vector3 velocity)
    //{
    //    var averageVelocity = (Mathf.Abs(velocity.x) + Mathf.Abs(velocity.y) + Mathf.Abs(velocity.z)) * 0.33f;

    //    return averageVelocity;
    //}


    //public class BoidDeathEventArgs : EventArgs
    //{
    //    public FlockUnit BoidObject { get; set; }
    //    public string BreedObject { get; set; }
    //}



    //public class ExtensionMethods
    //{
    //    public float scale(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax)
    //    {

    //        float OldRange = (OldMax - OldMin);
    //        float NewRange = (NewMax - NewMin);
    //        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

    //        return (NewValue);
    //    }



    //public int FindOpenIndex(List<int> indexList, int listSize)
    //{
    //    int indexValue;
    //    int a = indexList.OrderBy(x => x).First();
    //    int b = indexList.OrderBy(x => x).Last();
    //    List<int> myList2 = Enumerable.Range(1, listSize).ToList();
    //    List<int> remaining = myList2.Except(indexList).ToList();
    //    indexValue = remaining.First();
    //    indexValue = Math.Clamp(indexValue, 1, listSize);

    //    return indexValue;
    //}
}
