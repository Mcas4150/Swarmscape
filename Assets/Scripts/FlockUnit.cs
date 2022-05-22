
using System;
using System.Collections;
using System.Collections.Generic;
using extOSC;
using UnityEngine;

public class FlockUnit : MonoBehaviour
{

    [SerializeField] public int flockSize => GameManager.Instance.flockSize;
    [SerializeField] public float smoothDamp => GameManager.Instance.smoothDamp;
    [SerializeField] public float attractForceMagnitude => GameManager.Instance.attractForceMagnitude;
    [SerializeField] public float respawnTime => GameManager.Instance.respawnTime;
    [SerializeField] public float deathMultiplier => GameManager.Instance.deathMultiplier;
    [SerializeField] public float driftX => GameManager.Instance.driftX;
    [SerializeField] public float driftY => GameManager.Instance.driftY;
    [SerializeField] public float driftZ => GameManager.Instance.driftZ;


    [SerializeField] public float cohesionDistance => GameManager.Instance.cohesionDistance;
    [SerializeField] public float alignmentDistance => GameManager.Instance.alignmentDistance;
    [SerializeField] public float avoidanceDistance => GameManager.Instance.avoidanceDistance;
    [SerializeField] public float cohesionWeight => GameManager.Instance.cohesionWeight;
    [SerializeField] public float alignmentWeight => GameManager.Instance.alignmentWeight;
    [SerializeField] public float avoidanceWeight => GameManager.Instance.avoidanceWeight;
    [SerializeField] public float shadowProb => GameManager.Instance.shadowProb;

    [SerializeField] public float boidMass => GameManager.Instance.boidMass;
    [SerializeField] public float attackForceMagnitude => GameManager.Instance.attackForceMagnitude;

    [SerializeField] public float trailTime => GameManager.Instance.trailTime;
    [SerializeField] private float FOVAngle;


    public Flock assignedFlock;

    public List<FlockUnit> cohesionNeighbors = new List<FlockUnit>();
    public List<FlockUnit> alignmentNeighbors = new List<FlockUnit>();
    public List<FlockUnit> avoidanceNeighbors = new List<FlockUnit>();


    [Header("Vector Values")]
    public Vector3 currentCohesionVector;
    public Vector3 currentAvoidanceVector;
    public Vector3 currentAlignmentVector;
    public Vector3 currentBoundsVector;
    public Vector3 currentPosition;
    public Vector3 currentVelocity;
    public float currentAverageVelocity;
    public Vector3 currentMoveVector;
    public Vector3 acceleration;
    public Vector3 attractionForce;
    public Transform myTransform { get; set; }


    [Header("OSC Properties")]
    public OSCMessage message_newPositionX;
    public OSCMessage message_newPositionY;
    public OSCMessage message_newPositionZ;
    public OSCMessage messageAddress;
    public OSCMessage velocityMessage;
    public OSCMessage message_newPosition;
    public OSCMessage healthMessage;
    public OSCMessage midiNoteMessage;
    public OSCMessage midiPlayMessage;
    public OSCTransmitter transmitter;
    public string breed;
    public int oscNumber;
    public int eatingState;
    public float health;
    public float age;
    public float starterHealth;
    public float speed;
    public float cohesionSpeed;
    public float midiNote;
    public float dnaCohesionDist;
    public float dnaAlignmentDist;
    public float dnaAvoidanceDist;
    public float dnaCohesionWeight;
    public float dnaAlignmentWeight;
    public float dnaAvoidanceWeight;
    public float foodDistance;
    public Vector3 foodForce;
    public float G = 9.8f;

    DNAboid dna;

    public event EventHandler<BoidDeathEventArgs> Death;


    private void Awake()
    {
        myTransform = transform;
        health = UnityEngine.Random.Range(10, 100);
        starterHealth = health;
        myTransform.localScale = new Vector3(3, 3, 3);

    }

    private void Start()
    {


        transmitter = gameObject.AddComponent<OSCTransmitter>();
        transmitter.RemoteHost = "127.0.0.1";
        transmitter.RemotePort = 57120;


        message_newPositionX = new OSCMessage("/" + breed + "/position/x/" + oscNumber);
        message_newPositionY = new OSCMessage("/" + breed + "/position/y/" + oscNumber);
        message_newPositionZ = new OSCMessage("/" + breed + "/position/z/" + oscNumber);

        midiNoteMessage = new OSCMessage("/" + breed + "/midi/note/" + oscNumber);
        midiPlayMessage = new OSCMessage("/" + breed + "/midi/play/" + oscNumber);
        healthMessage = new OSCMessage("/" + breed + "/health/" + oscNumber);

        velocityMessage = new OSCMessage("/" + breed + "/velocity/" + oscNumber);

        TrailRenderer Trail = gameObject.GetComponent<TrailRenderer>();

        if (Trail != null)
            Trail.time = trailTime;

        //StartCoroutine(OSCSender());
        StartCoroutine(Respawn());
        StartCoroutine(HealthSize());
        StartCoroutine(CountAge());
        StartCoroutine(CheckAgent());

    }



    private IEnumerator OSCSender()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            message_newPositionX.AddValue(OSCValue.Float(myTransform.position.x));
            message_newPositionY.AddValue(OSCValue.Float(myTransform.position.y));
            message_newPositionZ.AddValue(OSCValue.Float(myTransform.position.z));

            midiNoteMessage.AddValue(OSCValue.Float(midiNote));
            midiPlayMessage.AddValue(OSCValue.Float(eatingState));
            healthMessage.AddValue(OSCValue.Float(health));

            velocityMessage.AddValue(OSCValue.Float(currentVelocity.magnitude));


            transmitter.Send(midiNoteMessage);
            transmitter.Send(midiPlayMessage);
            transmitter.Send(healthMessage);


            transmitter.Send(message_newPositionX);
            transmitter.Send(message_newPositionY);
            transmitter.Send(message_newPositionZ);

            transmitter.Send(velocityMessage);

        }
    }




    private IEnumerator Respawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnTime * UnityEngine.Random.Range(0f, 3f));

            //if (health > 50)
            //if (health > starterHealth * 1.5)
            if (breed == "organic")
            {
                //if (age == UnityEngine.Random.Range(2, 7) )
                //if (age > 2 && age < 7)
                //{
                var allUnits = assignedFlock.Boids;

                if (allUnits.Count < flockSize)
                {
                    currentPosition = new Vector3(myTransform.position.x, myTransform.position.y, myTransform.position.z);
                    var breedChance = UnityEngine.Random.Range(0f, 1f);

                    if (breedChance <= shadowProb)
                    {
                        assignedFlock.EnemyFlock.GenerateAgent(assignedFlock.EnemyFlock, assignedFlock.EnemyFlock.Boids, assignedFlock.EnemyFlock.BoidsIndex, "shadow", currentPosition, dna);

                    }
                    else if (breedChance > shadowProb)
                    {
                        assignedFlock.GenerateAgent(assignedFlock, assignedFlock.Boids, assignedFlock.BoidsIndex, "organic", currentPosition, dna);
                    }
                    health *= 0.5f;

                }
            }
        }
    }

    private IEnumerator HealthSize()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);

            var healthRatio = health * 0.025f;
            //myTransform.localScale = new Vector3(healthRatio, healthRatio, healthRatio);

        }
    }


    private IEnumerator CountAge()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            age += 1;

        }
    }


    private IEnumerator CheckAgent()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            if (breed == "organic")
            {
                health -= deathMultiplier * Time.deltaTime;
            }

            if (Dead())
            {

                // FIX THIS 
                messageAddress = new OSCMessage("/" + breed + "/" + oscNumber);
                messageAddress.AddValue(OSCValue.Float(0));
                messageAddress.AddValue(OSCValue.Float(0));
                messageAddress.AddValue(OSCValue.Float(0));
                messageAddress.AddValue(OSCValue.Float(midiNote));
                messageAddress.AddValue(OSCValue.Float(health));
                messageAddress.AddValue(OSCValue.Float(0));
                transmitter.Send(messageAddress);

                Death?.Invoke(this, new BoidDeathEventArgs { BoidObject = gameObject.GetComponent<FlockUnit>(), BreedObject = breed });

                assignedFlock.BoidsIndex.Remove(oscNumber);

                gameObject.SetActive(false);
                Destroy(this, 0.05f);
                Destroy(gameObject, 0.05f);

            }
        }
    }

    void Update()
    {
        MoveUnit();
    }


    public void AssignFlock(Flock flock)
    {
        assignedFlock = flock;
    }

    public void NumberParticle(int number)
    {
        oscNumber = number;
    }

    public float AverageVelocity(Vector3 velocity)
    {
        var averageVelocity = (Mathf.Abs(velocity.x) + Mathf.Abs(velocity.y) + Mathf.Abs(velocity.z)) / 3;

        return averageVelocity;
    }


    void applyForce(Vector3 force)
    {
        acceleration += force;
    }

    public bool Dead()
    {
        if (health <= 0)
            return true;
        else if (age >= 300)
            return true;
        else
            return false;
    }

    public void MoveUnit()
    {
        FindNeighbors();
        //  CalculateSpeed();

        currentCohesionVector = CalculateCohesionVector() * (cohesionWeight + dnaCohesionWeight);
        currentAvoidanceVector = CalculateAvoidanceVector() * (avoidanceWeight + dnaAvoidanceWeight);
        currentAlignmentVector = CalculateAlignmentVector() * (alignmentWeight + dnaAlignmentWeight);
        currentBoundsVector = CalculateBoundsVector() * assignedFlock.boundsWeight;
        var driftVector = new Vector3(driftX, driftY, driftZ);

        var moveVector = currentCohesionVector + currentAvoidanceVector + currentAlignmentVector + currentBoundsVector + attractionForce;

        moveVector = Vector3.SmoothDamp(myTransform.forward, moveVector, ref currentVelocity, smoothDamp);
        moveVector = moveVector.normalized * (speed + cohesionSpeed);
        if (moveVector == Vector3.zero)
            moveVector = transform.forward;
        myTransform.forward = moveVector;

        //vector coloring
        // Color linecolor(string breed) => breed == "shadow" ? Color.cyan : Color.magenta;
        //Debug.DrawLine(myTransform.position, moveVector, linecolor(breed));

        currentMoveVector = moveVector;

        myTransform.position += (currentMoveVector + driftVector) * Time.deltaTime;
        //checkBounds(currentMoveVector);
        CalculateBoundaries(myTransform.position);
        currentAverageVelocity = AverageVelocity(currentVelocity);
    }

    private void FindNeighbors()
    {
        cohesionNeighbors.Clear();
        alignmentNeighbors.Clear();
        avoidanceNeighbors.Clear();
        var allUnits = assignedFlock.Boids;

        foreach (FlockUnit boid in allUnits)
        {
            var currentUnit = boid;
            if (currentUnit != this && currentUnit != null)
            {

                float currentNeighborDistanceSqr = Vector3.SqrMagnitude(currentUnit.myTransform.position - myTransform.position);

                //  Color linecolor(string breed) => breed == "shadow" ? Color.cyan : Color.magenta;
                // Debug.DrawLine(myTransform.position, currentUnit.myTransform.position, linecolor(breed));
                if (currentNeighborDistanceSqr <= (cohesionDistance * cohesionDistance + dnaCohesionDist * dnaCohesionDist))
                {
                    cohesionNeighbors.Add(currentUnit);
                }
                if (currentNeighborDistanceSqr <= (avoidanceDistance * avoidanceDistance + dnaAvoidanceDist * dnaAvoidanceDist))
                {
                    avoidanceNeighbors.Add(currentUnit);
                }
                if (currentNeighborDistanceSqr <= (alignmentDistance * alignmentDistance + dnaAlignmentDist * dnaAlignmentDist))
                {
                    alignmentNeighbors.Add(currentUnit);
                }
            }
        }
    }

    private void CalculateSpeed()
    {
        //if (cohesionNeighbors.Count == 0)
        //    return;
        //speed = 0;
        for (int i = 0; i < cohesionNeighbors.Count; i++)
        {
            speed += cohesionNeighbors[i].speed;
        }
        speed /= cohesionNeighbors.Count;
        speed = Math.Clamp(speed, assignedFlock.minSpeed, assignedFlock.maxSpeed);
    }

    private Vector3 CalculateCohesionVector()
    {
        var cohesionVector = Vector3.zero;

        if (cohesionNeighbors.Count == 0)
            return Vector3.zero;
        int neighborsInFOV = 0;
        for (int i = 0; i < cohesionNeighbors.Count; i++)
        {
            if (IsInFOV(cohesionNeighbors[i].myTransform.position))
            {
                neighborsInFOV++;
                cohesionVector += cohesionNeighbors[i].myTransform.position;
                cohesionSpeed += cohesionNeighbors[i].speed;

            }
        }
        cohesionSpeed /= cohesionNeighbors.Count;

        cohesionSpeed = Math.Clamp(speed, assignedFlock.minSpeed, assignedFlock.maxSpeed);

        cohesionVector /= neighborsInFOV;
        cohesionVector -= myTransform.position;
        cohesionVector = cohesionVector.normalized;
        //Debug.DrawLine(myTransform.position, cohesionVector, Color.green);
        return cohesionVector;
    }

    private Vector3 CalculateAlignmentVector()
    {
        var alignmentVector = myTransform.forward;
        if (alignmentNeighbors.Count == 0)
            return myTransform.forward;
        int neighborsInFOV = 0;
        for (int i = 0; i < alignmentNeighbors.Count; i++)
        {
            if (IsInFOV(alignmentNeighbors[i].myTransform.position))
            {
                neighborsInFOV++;
                alignmentVector += alignmentNeighbors[i].myTransform.forward;

            }
        }

        alignmentVector /= neighborsInFOV;
        alignmentVector = alignmentVector.normalized;
        //Debug.DrawLine(myTransform.position, alignmentVector, Color.blue);
        return alignmentVector;
    }

    private Vector3 CalculateAvoidanceVector()
    {
        var avoidanceVector = Vector3.zero;
        if (avoidanceNeighbors.Count == 0)
            return Vector3.zero;
        int neighborsInFOV = 0;
        for (int i = 0; i < avoidanceNeighbors.Count; i++)
        {
            if (IsInFOV(avoidanceNeighbors[i].myTransform.position))
            {
                neighborsInFOV++;

                avoidanceVector += (myTransform.position - avoidanceNeighbors[i].myTransform.position);

            }
        }

        avoidanceVector /= neighborsInFOV;
        avoidanceVector = avoidanceVector.normalized;
        //Debug.DrawLine(myTransform.position, avoidanceVector, Color.red);
        return avoidanceVector;
    }

    private Vector3 CalculateBoundsVector()
    {
        // also functions as "bounce"
        var offsetToCenter = assignedFlock.transform.position - myTransform.position;
        bool isNearCenter = (offsetToCenter.magnitude >= assignedFlock.boundsDistance * 0.9f);
        return isNearCenter ? offsetToCenter.normalized : Vector3.zero;
    }


    private void CalculateBoundaries(Vector3 position)
    {
        //myTransform.position = Math.Clamp(position.x, -10f, 10f);
        myTransform.position = Vector3.ClampMagnitude(myTransform.position, assignedFlock.boundsDistance);
    }

    //private void checkBounds(Vector3 velocity)
    //{
    //    if (myTransform.position.x < -29.0f)
    //    {
    //        //currentMoveVector = new Vector3(velocity.x * -1.0f, velocity.y, velocity.z);
    //        currentMoveVector = new Vector3(currentMoveVector.x * -1.0f, currentMoveVector.y, currentMoveVector.z);
    //    }
    //    if (myTransform.position.y > 10.0f || myTransform.position.y < -10.0f)
    //    {
    //        //currentMoveVector = new Vector3(velocity.x, velocity.y * -1.0f, velocity.z);
    //        currentMoveVector.y *= -1.0f;
    //    }
    //    if (myTransform.position.z > 10.0f || myTransform.position.z < -10.0f)
    //    {
    //       //currentMoveVector = new Vector3(velocity.x, velocity.y, velocity.z * -1.0f);
    //        currentMoveVector.z *= -1.0f;
    //    }

    //}


    private bool IsInFOV(Vector3 position)
    {
        return Vector3.Angle(myTransform.forward, position - myTransform.position) <= FOVAngle;
    }

    public void ApplyAttractionForce(Vector3 force, Transform foodTransform)
    {
        // rb.AddForce(force, ForceMode.Force);
        attractionForce += force;
        attractionForce = Vector3.ClampMagnitude(attractionForce, attractForceMagnitude);
        Eater(foodTransform);

    }

    public void Eater(Transform foodTransform)
    {
        Vector3 force = myTransform.position - foodTransform.position;
        float distance = force.magnitude;

        foodForce = force;
        foodDistance = distance;


        if (distance < 5f && distance > 4.5f)
        {
            //health += 5 * Time.deltaTime;
            health += 0.25f;

            eatingState = 1;
        }
        else
        {
            eatingState = 0;
        }
        //else if (distance >= 2f)
        //{
        //    eatingState = 0;
        //}
    }


    public Vector3 Attract(Transform target)
    {
        Vector3 force = myTransform.position - target.position;
        float distance = force.magnitude;

        //Eaten(distance);
        // Remember we need to constrain the distance so that our circle doesn't spin out of control
        distance = Mathf.Clamp(distance, 5f, 25f);

        force.Normalize();
        // float strength =  G * (body.mass * m.mass) / (distance * distance);
        //float strength =  G * (body.mass * 1.5f) / (distance * distance);
        float strength = G * (boidMass * boidMass) / (distance * distance);
        force *= strength;
        force = Vector3.ClampMagnitude(force, attackForceMagnitude);
        return force;
    }

    public void setDNA(DNAboid newDNA)
    {
        dna = newDNA;
        //speed = ExtensionMethods.map(dna.genes[0], 0, 1, 10, 0); // MaxSpeed an Size are now mapped to values according to the DNA
        //Debug.Log(scale(dna.genes[0], 0, 1, 5, 15));
        speed = (scale(dna.genes[0], 0f, 1f, assignedFlock.minSpeed, assignedFlock.maxSpeed));
        midiNote = scale(dna.genes[0], 0, 1, 1, 10);

        dnaCohesionDist = (scale(dna.genes[0], 0, 1, 5, 100));
        dnaAvoidanceDist = (scale(dna.genes[0], 0, 1, 5, 100));
        dnaAlignmentDist = (scale(dna.genes[0], 0, 1, 5, 100));

        dnaCohesionWeight = (scale(dna.genes[0], 0, 1, 5, 100));
        dnaAvoidanceWeight = (scale(dna.genes[0], 0, 1, 5, 100));
        dnaAlignmentWeight = (scale(dna.genes[0], 0, 1, 5, 100));

        speed = Math.Clamp(speed, assignedFlock.minSpeed, assignedFlock.maxSpeed);
        midiNote = Math.Clamp(midiNote, 1, 10);


        dnaCohesionDist = Math.Clamp(dnaCohesionDist, 5, 100);
        dnaAvoidanceDist = Math.Clamp(dnaAvoidanceDist, 5, 100);
        dnaAlignmentDist = Math.Clamp(dnaAlignmentDist, 5, 100);

        dnaCohesionWeight = Math.Clamp(dnaCohesionWeight, 5, 100);
        dnaAvoidanceWeight = Math.Clamp(dnaAvoidanceWeight, 5, 100);
        dnaAlignmentWeight = Math.Clamp(dnaAlignmentWeight, 5, 100);
        //this.speed = scale(dna.genes[0], 0, 1, 10, 0);

        //size = ExtensionMethods.map(dna.genes[0], 0, 1, 0, 2);

        //gameObject.transform.localScale *= size;
    }


    public float scale(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax)
    {

        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        return (NewValue);
    }
}

public class BoidDeathEventArgs : EventArgs
{
    public FlockUnit BoidObject { get; set; }
    public string BreedObject { get; set; }
}

public class DNAboid
{
    public float[] genes;
    public DNAboid()
    {

        genes = new float[1];
        for (int i = 0; i < genes.Length; i++)
        {
            genes[i] = UnityEngine.Random.Range(0f, 1f);
        }
    }

    public DNAboid(float[] genes_)
    {
        genes = genes_;
    }

    public void mutate(float rate)
    {
        for (int i = 0; i < genes.Length; i++)
        {
            genes[i] += UnityEngine.Random.Range(-rate, rate); // We'll change each gene by + or - the rate at random
        }
    }

    public DNAboid copy()
    {
        float[] newgenes = new float[genes.Length];
        newgenes = (float[])genes.Clone();
        return new DNAboid(newgenes);
    }
}


//public class ExtensionMethods
//{
//    public float scale(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax)
//    {

//        float OldRange = (OldMax - OldMin);
//        float NewRange = (NewMax - NewMin);
//        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

//        return (NewValue);
//    }
