
using System;
using System.Collections;
using System.Collections.Generic;
using extOSC;
using UnityEngine;

public class FlockUnit : MonoBehaviour
{

    [SerializeField] public int flockSize => GameManager.Instance.flockSize;
    [SerializeField] public float shadowProb => GameManager.Instance.shadowProb;
    [SerializeField] public float respawnTime => GameManager.Instance.respawnTime;
    [SerializeField] public float deathMultiplier => GameManager.Instance.deathMultiplier;


    [SerializeField] public float cohesionDistance => GameManager.Instance.cohesionDistance;
    [SerializeField] public float alignmentDistance => GameManager.Instance.alignmentDistance;
    [SerializeField] public float avoidanceDistance => GameManager.Instance.avoidanceDistance;

    [SerializeField] public float cohesionWeight => GameManager.Instance.cohesionWeight;
    [SerializeField] public float alignmentWeight => GameManager.Instance.alignmentWeight;
    [SerializeField] public float avoidanceWeight => GameManager.Instance.avoidanceWeight;
    [SerializeField] public float dnaWeight => GameManager.Instance.dnaWeight;
    [SerializeField] public float generalWeight => GameManager.Instance.generalWeight;

    [SerializeField] public float maxForce => GameManager.Instance.maxForce;
    [SerializeField] public float boidMass => GameManager.Instance.boidMass;
    [SerializeField] public float attractForceMagnitude => GameManager.Instance.attractForceMagnitude;
    [SerializeField] public float smoothDamp => GameManager.Instance.smoothDamp;

    [SerializeField] public float driftX => GameManager.Instance.driftX;
    [SerializeField] public float driftY => GameManager.Instance.driftY;
    [SerializeField] public float driftZ => GameManager.Instance.driftZ;

    [SerializeField] public float trailTime => GameManager.Instance.trailTime;



    public Flock assignedFlock;
    public string breed;
    public int oscNumber;


    [Header("Health Values")]

    public float age;
    public float health;
    public float starterHealth;
    public int eatingState;
    public float FOVAngle;
    public float averageVelocity;
    public float foodDistance;
    public float currentSpeed;

    [Header("Neighbors")]
    public List<FlockUnit> cohesionNeighbors = new List<FlockUnit>();
    public List<FlockUnit> alignmentNeighbors = new List<FlockUnit>();
    public List<FlockUnit> avoidanceNeighbors = new List<FlockUnit>();


    [Header("Vector Values")]
    public Vector3 currentCohesionVector;
    public Vector3 currentAvoidanceVector;
    public Vector3 currentAlignmentVector;
    public Vector3 currentBoundsVector;
    public Vector3 currentSteerVector;
    public Vector3 attractionForce;
    public Vector3 foodForce;
    public Vector3 currentVelocity;



    public Vector3 acceleration;
    public Vector3 currentMoveVector;
    public Vector3 currentPosition;
    //public Vector3 location;
    //public Vector3 velocity;

    public Transform myTransform { get; set; }

    [Header("DNA")]
    public float speed;
    public float cohesionSpeed;
    public float midiNote;
    public float dnaCohesionDist;
    public float dnaAlignmentDist;
    public float dnaAvoidanceDist;
    public float dnaCohesionWeight;
    public float dnaAlignmentWeight;
    public float dnaAvoidanceWeight;



    [Header("OSC Properties")]

    public string oscAddress_positionX;
    public string oscAddress_positionY;
    public string oscAddress_positionZ;



    public OSCMessage message_newPositionX;
    public OSCMessage message_newPositionY;
    public OSCMessage message_newPositionZ;
    public OSCMessage velocityMessage;
    public OSCMessage message_newPosition;
    public OSCMessage healthMessage;
    public OSCMessage midiNoteMessage;
    public OSCMessage midiPlayMessage;
    public OSCTransmitter transmitter;

    private float G = 9.8f;


    DNAboid dna;

    public event EventHandler<BoidDeathEventArgs> Death;


    public Vector3 location
    {
        get { return transform.position; }
        set { transform.position = value; }
    }
    //public Vector3 velocity
    //{
    //    get { return body.velocity; }
    //    set { body.velocity = value; }
    //}


    private void Awake()
    {

        health = UnityEngine.Random.Range(10, 100);
        starterHealth = health;



    }

    private void Start()
    {
        transmitter = assignedFlock.transmitter;
        myTransform = transform;
        //location = transform.position;
        //velocity = body.velocity;
        //delete?
        myTransform.localScale = new Vector3(3, 3, 3);


        //transmitter = gameObject.AddComponent<OSCTransmitter>();
        //transmitter.RemoteHost = "192.168.2.22";
        //transmitter.RemotePort = 7121;
        ////transmitter.RemotePort = 57120;
        //transmitter.UseBundle = false;

        oscAddress_positionX = "/" + breed + "/position/x/" + oscNumber;
        oscAddress_positionY = "/" + breed + "/position/y/" + oscNumber;
        oscAddress_positionZ = "/" + breed + "/position/z/" + oscNumber;


        midiNoteMessage = new OSCMessage("/" + breed + "/midi/note/" + oscNumber);
        midiPlayMessage = new OSCMessage("/" + breed + "/midi/play/" + oscNumber);
        healthMessage = new OSCMessage("/" + breed + "/health/" + oscNumber);

        velocityMessage = new OSCMessage("/" + breed + "/velocity/" + oscNumber);

        TrailRenderer Trail = gameObject.GetComponent<TrailRenderer>();

        if (Trail != null)
            Trail.time = trailTime;

        StartCoroutine(OSCSender());
        StartCoroutine(Respawn());
        StartCoroutine(HealthSize());
        StartCoroutine(CountAge());
        StartCoroutine(CheckAgent());
        StartCoroutine(CalcSlowStats());

    }


    private IEnumerator OSCSender()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);

            message_newPositionX = new OSCMessage(oscAddress_positionX, OSCValue.Float(myTransform.position.x));
            message_newPositionY = new OSCMessage(oscAddress_positionY, OSCValue.Float(myTransform.position.y));
            message_newPositionZ = new OSCMessage(oscAddress_positionY, OSCValue.Float(myTransform.position.z));

            //message_newPositionX.AddValue(OSCValue.Float(myTransform.position.x));
            //message_newPositionX.AddValue(OSCValue.Float(myTransform.position.x));
            //message_newPositionY.AddValue(OSCValue.Float(myTransform.position.y));
            //message_newPositionZ.AddValue(OSCValue.Float(myTransform.position.z));

            //midiNoteMessage.AddValue(OSCValue.Float(midiNote));
            //midiPlayMessage.AddValue(OSCValue.Float(eatingState));
            //healthMessage.AddValue(OSCValue.Float(health));

            //velocityMessage.AddValue(OSCValue.Float(currentVelocity.magnitude));



            transmitter.Send(message_newPositionX);
            transmitter.Send(message_newPositionY);
            transmitter.Send(message_newPositionZ);


            //transmitter.Send(midiNoteMessage);
            //transmitter.Send(midiPlayMessage);
            //transmitter.Send(healthMessage);


            //transmitter.Send(velocityMessage);

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

    private IEnumerator CalcSlowStats()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);

            averageVelocity = AverageVelocity(currentVelocity);

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


                OSCMessage message_resetPositionX = new OSCMessage(oscAddress_positionX, OSCValue.Float(0));
                OSCMessage message_resetPositionY = new OSCMessage(oscAddress_positionY, OSCValue.Float(0));
                OSCMessage message_resetPositionZ = new OSCMessage(oscAddress_positionZ, OSCValue.Float(0));

                //OSCMessage midiNoteMessage = new OSCMessage("/" + breed + "/midi/note/" + oscNumber, OSCValue.Float(0));
                //OSCMessage midiPlayMessage = new OSCMessage("/" + breed + "/midi/play/" + oscNumber, OSCValue.Float(0));
                //OSCMessage healthMessage = new OSCMessage("/" + breed + "/health/" + oscNumber, OSCValue.Float(0));

                //OSCMessage velocityMessage = new OSCMessage("/" + breed + "/velocity/" + oscNumber, OSCValue.Float(0));

                transmitter.Send(message_resetPositionX);
                transmitter.Send(message_resetPositionY);
                transmitter.Send(message_resetPositionZ);

                //transmitter.Send(midiNoteMessage);
                //transmitter.Send(midiPlayMessage);
                //transmitter.Send(healthMessage);

                //transmitter.Send(velocityMessage);

                // FIX THIS 
                //OSCMessage messageAddress = new OSCMessage("/" + breed + "/" + oscNumber);
                //messageAddress.AddValue(OSCValue.Float(0));
                //messageAddress.AddValue(OSCValue.Float(0));
                //messageAddress.AddValue(OSCValue.Float(0));
                //messageAddress.AddValue(OSCValue.Float(midiNote));
                //messageAddress.AddValue(OSCValue.Float(health));
                //messageAddress.AddValue(OSCValue.Float(0));
                //transmitter.Send(messageAddress);

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


    //void FixedUpdate()
    //{
    //    //body.velocity = new Vector3(
    //    //                  Mathf.Clamp(body.velocity.x, -speed, speed),
    //    //                  Mathf.Clamp(body.velocity.y, -speed, speed),
    //    //                  Mathf.Clamp(body.velocity.z, -speed, speed)
    //    //                  );

    //    //transform.rotation = Quaternion.LookRotation(body.velocity);
    //}

    //public void Flock(List<FlockUnit> boids)
    //{

    //    if (velocity == Vector3.zero)
    //        velocity = new Vector3(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5));
    //    //myTransform.forward = velocity;

    //    //if (velocity == Vector3.zero)
    //    //    velocity = transform.forward;
    //    //myTransform.forward = velocity;


    //    Vector3 sep = Separate(boids); // The three flocking rules
    //    Vector3 ali = Align(boids);
    //    Vector3 coh = Cohesion(boids);
    //    currentBoundsVector = CalculateBoundsVector() * assignedFlock.boundsWeight;


    //    sep *= avoidanceWeight;
    //    //sep *= separationScale; // Arbitrary weights for these forces (Try different ones!)
    //    ali *= alignmentWeight;
    //    coh *= cohesionWeight;

    //    ApplyForce(sep); // Applying all the forces
    //    ApplyForce(ali);
    //    ApplyForce(coh);
    //    ApplyForce(currentBoundsVector);


    //    CheckBounds(velocity); // To loop the world to the other side of the screen.
    //    CalculateBoundaries(myTransform.position);

    //    //LookForward(); // Make the boids face forward.
    //}

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


        currentCohesionVector = CalculateCohesionVector() * ((cohesionWeight * generalWeight) + (dnaCohesionWeight * dnaWeight));
        currentAvoidanceVector = CalculateAvoidanceVector() * ((avoidanceWeight * generalWeight) + (dnaAvoidanceWeight * dnaWeight));
        currentAlignmentVector = CalculateAlignmentVector() * ((alignmentWeight * generalWeight) + (dnaAlignmentWeight * dnaWeight));
        currentBoundsVector = CalculateBoundsVector() * assignedFlock.boundsWeight;

        var driftVector = new Vector3(driftX, driftY, driftZ);

        var moveVector = currentCohesionVector + currentAvoidanceVector + currentAlignmentVector + currentBoundsVector + attractionForce;

        moveVector = Vector3.SmoothDamp(myTransform.forward, moveVector, ref currentVelocity, smoothDamp);
        moveVector = moveVector.normalized * (speed + cohesionSpeed);



        //var moveVector = currentSteerVector + currentBoundsVector;
        //if (moveVector == Vector3.zero)
        //    moveVector = new Vector3(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(-2, 2));
        if (moveVector == Vector3.zero)
            moveVector = transform.forward;
        myTransform.forward = moveVector;

        //vector coloring
        // Color linecolor(string breed) => breed == "shadow" ? Color.cyan : Color.magenta;
        //Debug.DrawLine(myTransform.position, moveVector, linecolor(breed));
        currentMoveVector = moveVector;

        myTransform.position += (currentMoveVector + driftVector) * Time.deltaTime;
        //checkBounds(currentMoveVector);

        //currentSteerVector
        CalculateBoundaries(myTransform.position);
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
                if (currentNeighborDistanceSqr <= ((cohesionDistance * cohesionDistance * generalWeight) + (dnaCohesionDist * dnaCohesionDist * dnaWeight)))
                {
                    cohesionNeighbors.Add(currentUnit);
                }
                if (currentNeighborDistanceSqr <= ((avoidanceDistance * avoidanceDistance * generalWeight) + (dnaAvoidanceDist * dnaAvoidanceDist * dnaWeight)))
                {
                    avoidanceNeighbors.Add(currentUnit);
                }
                if (currentNeighborDistanceSqr <= ((alignmentDistance * alignmentDistance * generalWeight) + (dnaAlignmentDist * dnaAlignmentDist * dnaWeight)))
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

    //public Vector3 Separate(List<FlockUnit> boids)
    //{
    //    Vector3 sum = Vector3.zero;
    //    int count = 0;

    //    //float desiredSeperation = transform.localScale.x * 2;
    //    float desiredSeperation = avoidanceDistance;

    //    foreach (FlockUnit other in boids)
    //    {
    //        float d = Vector3.Distance(other.location, location);

    //        if ((d > 0) && (d < desiredSeperation))
    //        {
    //            Vector3 diff = location - other.location;
    //            diff.Normalize();

    //            diff /= d;

    //            sum += diff;
    //            count++;
    //        }
    //    }

    //    if (count > 0)
    //    {
    //        sum /= count;

    //        sum *= speed;

    //        Vector3 steer = sum - velocity;
    //        steer = Vector3.ClampMagnitude(steer, maxForce);


    //        return steer;
    //    }
    //    return Vector3.zero;
    //}

    //public Vector3 Cohesion(List<FlockUnit> boids)
    //{
    //    float neighborDist = cohesionDistance;
    //    Vector3 sum = Vector3.zero;
    //    int count = 0;
    //    foreach (FlockUnit other in boids)
    //    {
    //        float d = Vector3.Distance(location, other.location);
    //        if ((d > 0) && (d < neighborDist))
    //        {
    //            sum += other.location; // Adding up all the other's locations
    //            count++;
    //        }
    //    }
    //    if (count > 0)
    //    {
    //        sum /= count;
    //        /* Here we make use of the Seek() function we wrote in
    //         * Example 6.8. The target we seek is the average
    //         * location of our neighbors. */
    //        return Seek(sum);
    //    }
    //    else
    //    {
    //        return Vector3.zero;
    //    }
    //}


    //public Vector3 Align(List<FlockUnit> boids)
    //{
    //    float neighborDist = alignmentDistance; // This is an arbitrary value and could vary from boid to boid.

    //    /* Add up all the velocities and divide by the total to
    //     * calculate the average velocity. */
    //    Vector3 sum = Vector3.zero;
    //    int count = 0;
    //    foreach (FlockUnit other in boids)
    //    {
    //        float d = Vector3.Distance(location, other.location);
    //        if ((d > 0) && (d < neighborDist))
    //        {
    //            sum += other.velocity;
    //            count++; // For an average, we need to keep track of how many boids are within the distance.
    //        }
    //    }

    //    if (count > 0)
    //    {
    //        sum /= count;

    //        sum = sum.normalized * speed; // We desire to go in that direction at maximum speed.

    //        Vector3 steer = sum - velocity; // Reynolds's steering force formula.
    //        steer = Vector3.ClampMagnitude(steer, maxForce);
    //        return steer;
    //    }
    //    else
    //    {
    //        return Vector3.zero; // If we don't find any close boids, the steering force is Zero.
    //    }
    //}


    private Vector3 CalculateBoundsVector()
    {
        // also functions as "bounce"
        var offsetToCenter = assignedFlock.transform.position - myTransform.position;
        bool isNearCenter = (offsetToCenter.magnitude >= assignedFlock.boundsDistance * 0.9f);
        return isNearCenter ? offsetToCenter.normalized : Vector3.zero;
    }


    private void CalculateBoundaries(Vector3 position)
    {

        var clampX = Math.Clamp(position.x, -assignedFlock.boundsDistance, assignedFlock.boundsDistance);
        var clampY = Math.Clamp(position.y, -assignedFlock.boundsDistance, assignedFlock.boundsDistance);
        var clampZ = Math.Clamp(position.z, -assignedFlock.boundsDistance, assignedFlock.boundsDistance);
        myTransform.position = new Vector3(clampX, clampY, clampZ);

        //sphere
        //myTransform.position = Vector3.ClampMagnitude(position, assignedFlock.boundsDistance);
    }

    //private void CheckBounds(Vector3 currentVelocity)
    //{
    //    if (myTransform.position.x > assignedFlock.boundsDistance * 0.98 || myTransform.position.x < -assignedFlock.boundsDistance * 0.98)
    //    {
    //        //currentMoveVector = new Vector3(velocity.x * -1.0f, velocity.y, velocity.z);
    //        velocity = new Vector3(currentVelocity.x * -1.0f, currentVelocity.y, currentVelocity.z);
    //        //velocity.x *= -1.0f;
    //    }
    //    else if (myTransform.position.y > assignedFlock.boundsDistance * 0.98 || myTransform.position.y < -assignedFlock.boundsDistance * 0.98)
    //    {
    //        Debug.Log("Y exceeded");
    //        velocity = new Vector3(currentVelocity.x, currentVelocity.y * -1.0f, currentVelocity.z);
    //        //velocity.y *= -1.0f;
    //    }
    //    else if (myTransform.position.z > assignedFlock.boundsDistance * 0.98 || myTransform.position.z < -assignedFlock.boundsDistance * 0.98)
    //    {
    //        velocity = new Vector3(currentVelocity.x, currentVelocity.y, currentVelocity.z * -1.0f);
    //        //velocity.z *= -1.0f;
    //    }

    //}


    private bool IsInFOV(Vector3 position)
    {
        return Vector3.Angle(myTransform.forward, position - myTransform.position) <= FOVAngle;
    }


    //public Vector3 Seek(Vector3 targetPostion)
    //{
    //    Vector3 desired = targetPostion - myTransform.position;
    //    desired.Normalize();
    //    desired *= speed;
    //    //desired *= assignedFlock.maxSpeed;
    //    Vector3 steer = desired - body.velocity;
    //    steer.x = Mathf.Clamp(steer.x, -maxForce, maxForce);
    //    steer.y = Mathf.Clamp(steer.y, -maxForce, maxForce);
    //    steer.z = Mathf.Clamp(steer.z, -maxForce, maxForce);
    //    //currentSteerVector = steer;
    //    return steer;
    //}

    //public void Arrive(Vector3 targetPostion)
    //{
    //    float r = 10.0f;
    //    Vector3 desired = targetPostion - myTransform.position;

    //    float d = desired.magnitude;
    //    desired = desired.normalized;
    //    //Debug.Log(d);
    //    if (d < r)
    //    {
    //        float m = scale(d, 0, speed, 0, 3);
    //        desired *= m;
    //        //Debug.Log("near" + desired);

    //    }
    //    else
    //    {
    //        desired *= speed;
    //        //Debug.Log("far" + desired);
    //    }


    //    Vector3 steer = desired - body.velocity;
    //    steer.x = Mathf.Clamp(steer.x, -maxForce, maxForce);
    //    steer.y = Mathf.Clamp(steer.y, -maxForce, maxForce);
    //    steer.z = Mathf.Clamp(steer.z, -maxForce, maxForce);
    //    currentSteerVector = steer;
    //    ApplyForce(steer);
    //}

    //public void ApplyForce(Vector3 force)
    //{
    //    body.AddForce(force * Time.fixedDeltaTime, ForceMode.VelocityChange);
    //}



    public void ApplyAttractionForce(Vector3 force)
    {

        attractionForce += force;
        attractionForce = Vector3.ClampMagnitude(attractionForce, attractForceMagnitude);

    }

    public void Eater(Vector3 preyPosition)
    {
        Vector3 force = myTransform.position - preyPosition;
        float distance = force.magnitude;

        foodForce = force;
        foodDistance = distance;


        if (distance < 5f && distance > 3f)
        {
            //health += 5 * Time.deltaTime;
            health += 0.25f;

            eatingState = 1;
        }
        else
        {
            eatingState = 0;
        }

    }


    public Vector3 Attract(Vector3 targetPosition, float maxForceMagnitude)
    {
        Vector3 force = myTransform.position - targetPosition;
        float distance = force.magnitude;
        distance = Mathf.Clamp(distance, 2f, 25f);
        force.Normalize();

        float strength = G * (boidMass * boidMass) / (distance * distance);
        force *= strength;
        force = Vector3.ClampMagnitude(force, maxForceMagnitude);
        return force;
    }

    public void setDNA(DNAboid newDNA)
    {
        dna = newDNA;
        //speed = ExtensionMethods.map(dna.genes[0], 0, 1, 10, 0); // MaxSpeed an Size are now mapped to values according to the DNA
        //Debug.Log(scale(dna.genes[0], 0, 1, 5, 15));

        speed = (scale(dna.genes[0], 0f, 1f, assignedFlock.minSpeed, assignedFlock.maxSpeed));
        speed = Math.Clamp(speed, assignedFlock.minSpeed, assignedFlock.maxSpeed);



        dnaCohesionDist = (scale(dna.genes[1], 0, 1, 5, 100));
        dnaAvoidanceDist = (scale(dna.genes[2], 0, 1, 5, 100));
        dnaAlignmentDist = (scale(dna.genes[3], 0, 1, 5, 100));

        dnaCohesionWeight = (scale(dna.genes[4], 0, 1, 5, 100));
        dnaAvoidanceWeight = (scale(dna.genes[5], 0, 1, 5, 100));
        dnaAlignmentWeight = (scale(dna.genes[6], 0, 1, 5, 100));

        midiNote = scale(dna.genes[7], 0, 1, 1, 10);



        dnaCohesionDist = Math.Clamp(dnaCohesionDist, 5, 100);
        dnaAvoidanceDist = Math.Clamp(dnaAvoidanceDist, 5, 100);
        dnaAlignmentDist = Math.Clamp(dnaAlignmentDist, 5, 100);

        dnaCohesionWeight = Math.Clamp(dnaCohesionWeight, 5, 100);
        dnaAvoidanceWeight = Math.Clamp(dnaAvoidanceWeight, 5, 100);
        dnaAlignmentWeight = Math.Clamp(dnaAlignmentWeight, 5, 100);

        midiNote = Math.Clamp(midiNote, 1, 10);








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

        genes = new float[10];
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
