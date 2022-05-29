
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
    [SerializeField] public float preyDistance => GameManager.Instance.preyDistance;

    [SerializeField] public float cohesionWeight => GameManager.Instance.cohesionWeight;
    [SerializeField] public float alignmentWeight => GameManager.Instance.alignmentWeight;
    [SerializeField] public float avoidanceWeight => GameManager.Instance.avoidanceWeight;

    [SerializeField] public float dnaWeight => GameManager.Instance.dnaWeight;
    [SerializeField] public float generalWeight => GameManager.Instance.generalWeight;


    [SerializeField] public float masterSpeed => GameManager.Instance.masterSpeed;


    [SerializeField] public float foodForceMagnitude => GameManager.Instance.foodForceMagnitude;
    [SerializeField] public float maxSteeringForce => GameManager.Instance.maxSteeringForce;
    [SerializeField] public float smoothDamp => GameManager.Instance.smoothDamp;

    [SerializeField] public float driftX => GameManager.Instance.driftX;
    [SerializeField] public float driftY => GameManager.Instance.driftY;
    [SerializeField] public float driftZ => GameManager.Instance.driftZ;

    [SerializeField] public float trailTime => GameManager.Instance.trailTime;

    //[SerializeField] public float BoidMass => GameManager.Instance.boidMass;




    public Flock assignedFlock;
    public string breed;
    public int oscNumber;
    GameObject Prey;

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
    public List<FoodUnit> foodNeighbors = new List<FoodUnit>();
    public List<FlockUnit> preyNeighbors = new List<FlockUnit>();


    [Header("Vector Values")]
    public Vector3 Cohesion;
    public Vector3 Separation;
    public Vector3 Alignment;
    public Vector3 BoundsVector;
    public Vector3 FoodVector;
    public Vector3 PreyVector;
    public Vector3 PredatorVector;

    //public Vector3 foodForce;
    public Vector3 currentVelocity;

    public Vector3 currentMoveVector;
    public Vector3 spawnPosition;


    public Transform myTransform { get; set; }

    [Header("DNA")]
    public float speed;
    //public float cohesionSpeed;
    public float midiNote;
    public float dnaCohesionDist;
    public float dnaAlignmentDist;
    public float dnaAvoidanceDist;
    public float dnaPreyDistance;
    public float dnaCohesionWeight;
    public float dnaAlignmentWeight;
    public float dnaAvoidanceWeight;

    [Header("OSC Properties")]

    public string oscAddress_positionXYZ;

    public OSCMessage message_newPositionX;
    public OSCMessage message_newPositionY;
    public OSCMessage message_newPositionZ;
    public OSCMessage message_newPositionXYZ;
    public OSCMessage velocityMessage;
    public OSCMessage message_newPosition;
    public OSCMessage healthMessage;
    public OSCMessage midiNoteMessage;
    public OSCMessage midiPlayMessage;
    public OSCTransmitter transmitter;

    DNAboid dna;

    public event EventHandler<BoidDeathEventArgs> Death;


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

        oscAddress_positionXYZ = "/" + breed + "/position/" + oscNumber;

        message_newPositionXYZ = new OSCMessage(oscAddress_positionXYZ);
        message_newPositionXYZ.AddValue(OSCValue.Float(myTransform.position.x));
        message_newPositionXYZ.AddValue(OSCValue.Float(myTransform.position.y));
        message_newPositionXYZ.AddValue(OSCValue.Float(myTransform.position.z));

        midiNoteMessage = new OSCMessage("/" + breed + "/midi/note/" + oscNumber);
        midiPlayMessage = new OSCMessage("/" + breed + "/midi/play/" + oscNumber);
        healthMessage = new OSCMessage("/" + breed + "/health/" + oscNumber);

        velocityMessage = new OSCMessage("/" + breed + "/velocity/" + oscNumber);

        TrailRenderer Trail = gameObject.GetComponent<TrailRenderer>();

        if (Trail != null)
            Trail.time = trailTime;

        StartCoroutine(OSCSender());
        StartCoroutine(Respawn());
        //StartCoroutine(HealthSize());
        StartCoroutine(CountAge());
        StartCoroutine(CheckAgent());
        StartCoroutine(CalcSlowStats());

    }


    private IEnumerator OSCSender()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);

            message_newPositionXYZ.Values[0] = OSCValue.Float(myTransform.position.x);
            message_newPositionXYZ.Values[1] = OSCValue.Float(myTransform.position.y);
            message_newPositionXYZ.Values[2] = OSCValue.Float(myTransform.position.z);


            //midiNoteMessage.AddValue(OSCValue.Float(midiNote));
            //midiPlayMessage.AddValue(OSCValue.Float(eatingState));
            //healthMessage.AddValue(OSCValue.Float(health));

            //velocityMessage.AddValue(OSCValue.Float(currentVelocity.magnitude));

            transmitter.Send(message_newPositionXYZ);

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
                    spawnPosition = new Vector3(myTransform.position.x, myTransform.position.y, myTransform.position.z);
                    var breedChance = UnityEngine.Random.Range(0f, 1f);

                    if (breedChance <= shadowProb)
                    {
                        assignedFlock.EnemyFlock.GenerateAgent(assignedFlock.EnemyFlock, assignedFlock.EnemyFlock.Boids, assignedFlock.EnemyFlock.BoidsIndex, "shadow", spawnPosition, dna);

                    }
                    else if (breedChance > shadowProb)
                    {
                        assignedFlock.GenerateAgent(assignedFlock, assignedFlock.Boids, assignedFlock.BoidsIndex, "organic", spawnPosition, dna);
                    }
                    health *= 0.5f;

                }
            }
        }
    }

    //private IEnumerator HealthSize()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(0.25f);

    //        var healthRatio = health * 0.025f;
    //        //myTransform.localScale = new Vector3(healthRatio, healthRatio, healthRatio);

    //    }
    //}

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
                message_newPositionXYZ.Values[0] = OSCValue.Float(0);
                message_newPositionXYZ.Values[1] = OSCValue.Float(0);
                message_newPositionXYZ.Values[2] = OSCValue.Float(0);


                //OSCMessage midiNoteMessage = new OSCMessage("/" + breed + "/midi/note/" + oscNumber, OSCValue.Float(0));
                //OSCMessage midiPlayMessage = new OSCMessage("/" + breed + "/midi/play/" + oscNumber, OSCValue.Float(0));
                //OSCMessage healthMessage = new OSCMessage("/" + breed + "/health/" + oscNumber, OSCValue.Float(0));

                //OSCMessage velocityMessage = new OSCMessage("/" + breed + "/velocity/" + oscNumber, OSCValue.Float(0));


                transmitter.Send(message_newPositionXYZ);

                //transmitter.Send(midiNoteMessage);
                //transmitter.Send(midiPlayMessage);
                //transmitter.Send(healthMessage);

                //transmitter.Send(velocityMessage);


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
        transform.localRotation = Quaternion.LookRotation(currentMoveVector, Vector3.up);
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
        FindPrey();

        Cohesion = CalculateCohesionVector() * ((cohesionWeight * generalWeight) + (dnaCohesionWeight * dnaWeight));
        Separation = CalculateAvoidance() * ((avoidanceWeight * generalWeight) + (dnaAvoidanceWeight * dnaWeight));
        Alignment = CalculateAlignment() * ((alignmentWeight * generalWeight) + (dnaAlignmentWeight * dnaWeight));
        BoundsVector = CalculateBoundsVector() * assignedFlock.boundsWeight;

        if (assignedFlock.Carnivore == true)
        {
            PreyVector = CalculatePreyVector(preyNeighbors, foodForceMagnitude) * assignedFlock.preyWeight;
            EatPrey(preyNeighbors);
            PredatorVector = Vector3.zero;
        }
        else
        {
            PredatorVector = CalculateFlee(preyNeighbors, foodForceMagnitude) * assignedFlock.fleeWeight;
            PreyVector = Vector3.zero;

        }

        if (assignedFlock.Herbivore == true)
        {
            FindFood();
            FoodVector = CalculateFoodVector(foodNeighbors, foodForceMagnitude) * assignedFlock.foodWeight;
            EatFood(foodNeighbors);
        }
        else
        {
            FoodVector = Vector3.zero;
        }

        var driftVector = new Vector3(driftX, driftY, driftZ);

        var moveVector = Cohesion + BoundsVector + Alignment + FoodVector + PreyVector + PredatorVector + Separation;


        //moveVector = CustomNormalize(moveVector) * speed;
        moveVector = Vector3.SmoothDamp(transform.forward, moveVector, ref currentVelocity, smoothDamp);

        if (moveVector == Vector3.zero)
            moveVector = transform.forward;
        myTransform.forward = moveVector;

        currentMoveVector = moveVector;
        transform.position += (currentMoveVector + driftVector) * Time.deltaTime * ((masterSpeed * generalWeight) + (speed * dnaWeight));
        CalculateBoundaries(myTransform.position);

    }

    private void FindNeighbors()
    {
        cohesionNeighbors.Clear();
        alignmentNeighbors.Clear();
        avoidanceNeighbors.Clear();
        foodNeighbors.Clear();
        var allUnits = assignedFlock.Boids;

        foreach (FlockUnit boid in allUnits)
        {
            var currentUnit = boid;
            //if (currentUnit != this && currentUnit != null)
            {
                if (currentUnit == this) return;
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

    private void FindFood()
    {
        foodNeighbors.Clear();
        var allPrey = assignedFlock.food.Foods;

        foreach (FoodUnit prey in allPrey)
        {
            var currentUnit = prey;
            //if (currentUnit != this && currentUnit != null)
            {
                float currentPreyDistanceSqr = Vector3.SqrMagnitude(currentUnit.transform.position - transform.position);

                //if (currentPreyDistanceSqr <= ((preyDistance * preyDistance * generalWeight) + (dnaPreyDistance * dnaPreyDistance * dnaWeight)))
                if (currentPreyDistanceSqr <= ((preyDistance * preyDistance) + (dnaPreyDistance * dnaPreyDistance)))
                {
                    foodNeighbors.Add(currentUnit);



                }
            }
        }




    }

    private void FindPrey()
    {
        preyNeighbors.Clear();
        var allPrey = assignedFlock.EnemyFlock.Boids;

        foreach (FlockUnit prey in allPrey)
        {
            var currentUnit = prey;
            //if (currentUnit != this && currentUnit != null)
            {
                float currentPreyDistanceSqr = Vector3.SqrMagnitude(currentUnit.transform.position - transform.position);

                //if (currentPreyDistanceSqr <= ((preyDistance * preyDistance * generalWeight) + (dnaPreyDistance * dnaPreyDistance * dnaWeight)))
                if (currentPreyDistanceSqr <= ((preyDistance * preyDistance) + (dnaPreyDistance * dnaPreyDistance)))
                {
                    preyNeighbors.Add(currentUnit);

                }
            }
        }
    }


    private Vector3 CalculateCohesionVector()
    {
        var force = Vector3.zero;

        if (cohesionNeighbors.Count == 0)
            return Vector3.zero;
        int neighborsInFOV = 0;
        for (int i = 0; i < cohesionNeighbors.Count; i++)
        {
            //if (IsInFOV(cohesionNeighbors[i].transform.position))
            //{
            neighborsInFOV++;
            force += cohesionNeighbors[i].myTransform.position;
            //cohesionSpeed += cohesionNeighbors[i].speed;

            //}
        }
        //cohesionSpeed /= cohesionNeighbors.Count;

        //cohesionSpeed = Math.Clamp(speed, assignedFlock.minSpeed, assignedFlock.maxSpeed);

        //cohesionVector /= neighborsInFOV;
        force /= cohesionNeighbors.Count;
        force -= myTransform.position;
        force = force.normalized;
        force -= currentMoveVector;
        force = Vector3.ClampMagnitude(force, maxSteeringForce);
        //Debug.DrawLine(transform.position, cohesionVector, Color.green);
        return force;
    }


    private Vector3 CalculateAlignment()
    {
        var force = myTransform.forward;
        if (alignmentNeighbors.Count == 0)
            return myTransform.forward;
        int neighborsInFOV = 0;
        for (int i = 0; i < alignmentNeighbors.Count; i++)
        {
            //if (IsInFOV(alignmentNeighbors[i].transform.position))
            //{
            neighborsInFOV++;
            force += alignmentNeighbors[i].transform.forward;
            //}
        }
        //alignmentVector /= neighborsInFOV;
        //force /= alignmentNeighbors.Count;
        force = force.normalized;
        force *= speed;
        force -= currentMoveVector;
        force = Vector3.ClampMagnitude(force, maxSteeringForce);
        //Debug.DrawLine(transform.position, force, Color.blue);
        return force;
    }


    private Vector3 CalculateAvoidance()
    {
        var force = Vector3.zero;
        if (avoidanceNeighbors.Count == 0)
            return Vector3.zero;
        int neighborsInFOV = 0;
        for (int i = 0; i < avoidanceNeighbors.Count; i++)
        {
            //if (IsInFOV(avoidanceNeighbors[i].transform.position))
            //{
            neighborsInFOV++;
            Vector3 neighborDir = (transform.position - avoidanceNeighbors[i].transform.position);
            float neighborDist = neighborDir.magnitude;
            neighborDir = Vector3.Normalize(neighborDir);
            neighborDir = neighborDir / (Mathf.Pow(neighborDist, 2f));
            force += neighborDir;
            //}
        }

        //avoidanceVector /= neighborsInFOV;
        force = Vector3.Normalize(force);
        force = force * speed;
        force = force - currentMoveVector;
        force = Vector3.ClampMagnitude(force, maxSteeringForce);
        //Debug.DrawLine(transform.position, force, Color.red);
        return force;
    }


    private Vector3 CalculateFlee(List<FlockUnit> neighbors, float forceMagnitude)
    {
        var force = Vector3.zero;
        if (neighbors.Count == 0)
            return Vector3.zero;
        int neighborsInFOV = 0;
        for (int i = 0; i < neighbors.Count; i++)
        {
            //if (IsInFOV(neighbors[i].transform.position))
            //{
            neighborsInFOV++;
            Vector3 neighborDir = (transform.position - neighbors[i].transform.position);
            float neighborDist = neighborDir.magnitude;
            neighborDir = Vector3.Normalize(neighborDir);
            neighborDir = neighborDir / (Mathf.Pow(neighborDist, 2f));
            force += neighborDir;
            //}
        }

        //avoidanceVector /= neighborsInFOV;
        force = Vector3.Normalize(force);
        force = force * speed;
        force = force - currentMoveVector;
        force = Vector3.ClampMagnitude(force, forceMagnitude);
        //Debug.DrawLine(transform.position, force, Color.red);
        return force;
    }


    private Vector3 CalculateBoundsVector()
    {
        // also functions as "bounce"
        var offsetToCenter = assignedFlock.transform.position - myTransform.position;
        bool isNearCenter = (offsetToCenter.magnitude >= assignedFlock.boundsDistance * 0.9f);
        return isNearCenter ? offsetToCenter.normalized : Vector3.zero;
    }


    private Vector3 CalculateFoodVector(List<FoodUnit> neighbors, float forceMagnitude)
    {
        var force = Vector3.zero;

        if (neighbors.Count == 0)
            return Vector3.zero;
        int neighborsInFOV = 0;
        for (int i = 0; i < neighbors.Count; i++)
        {
            //if (IsInFOV(neighbors[i].transform.position))
            //{
            //    neighborsInFOV++;

            Vector3 neighborDir = (neighbors[i].transform.position - myTransform.position);
            float neighborDist = neighborDir.magnitude;
            neighborDir = Vector3.Normalize(neighborDir);
            // weight the vector by the distance squared
            neighborDir /= (Mathf.Pow(neighborDist, 2f));
            force += neighborDir;
            //}
        }

        force = Vector3.Normalize(force);
        force *= speed;
        force -= currentMoveVector;
        force = Vector3.ClampMagnitude(force, forceMagnitude);

        return force;
    }

    private Vector3 CalculatePreyVector(List<FlockUnit> neighbors, float forceMagnitude)
    {
        var force = Vector3.zero;

        if (neighbors.Count == 0)
            return Vector3.zero;
        int neighborsInFOV = 0;
        for (int i = 0; i < neighbors.Count; i++)
        {
            //if (IsInFOV(neighbors[i].transform.position))
            //{
            //    neighborsInFOV++;

            Vector3 neighborDir = (neighbors[i].transform.position - myTransform.position);
            float neighborDist = neighborDir.magnitude;
            neighborDir = Vector3.Normalize(neighborDir);
            // weight the vector by the distance squared
            neighborDir /= (Mathf.Pow(neighborDist, 2f));
            force += neighborDir;
            //}
        }

        force = Vector3.Normalize(force);
        force *= speed;
        force -= currentMoveVector;
        force = Vector3.ClampMagnitude(force, forceMagnitude);

        return force;
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



    public void EatFood(List<FoodUnit> neighbors)
    {

        foreach (FoodUnit prey in neighbors)
        {
            //sqr magnitude
            var desiredDirection = myTransform.position - prey.transform.position;
            var desiredDistance = desiredDirection.magnitude;
            if (desiredDistance < 5f)
            {
                //health += 5 * Time.deltaTime;
                prey.Eaten();
                health += 0.25f;
                eatingState = 1;
            }
            else
            {
                eatingState = 0;
            }

        }

    }

    public void EatPrey(List<FlockUnit> neighbors)
    {

        foreach (FlockUnit prey in neighbors)
        {
            //sqr magnitude
            var desiredDirection = myTransform.position - prey.transform.position;
            var desiredDistance = desiredDirection.magnitude;
            if (desiredDistance < 5f)
            {
                //health += 5 * Time.deltaTime;
                prey.Eaten();
                health += 0.25f;
                eatingState = 1;
            }
            else
            {
                eatingState = 0;
            }
        }

    }

    public void Eaten()
    {
        //include foodsize only when eaten
        //health -= 1f * Time.deltaTime;
        health -= 1f;
    }



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
        dnaPreyDistance = (scale(dna.genes[4], 0, 1, 5, 100));

        dnaCohesionWeight = (scale(dna.genes[5], 0, 1, 1, 5));
        dnaAvoidanceWeight = (scale(dna.genes[6], 0, 1, 1, 5));
        dnaAlignmentWeight = (scale(dna.genes[7], 0, 1, 1, 5));


        midiNote = scale(dna.genes[8], 0, 1, 1, 10);



        dnaCohesionDist = Math.Clamp(dnaCohesionDist, 5, 100);
        dnaAvoidanceDist = Math.Clamp(dnaAvoidanceDist, 5, 100);
        dnaAlignmentDist = Math.Clamp(dnaAlignmentDist, 5, 100);
        dnaPreyDistance = Math.Clamp(dnaPreyDistance, 5, 100);

        dnaCohesionWeight = Math.Clamp(dnaCohesionWeight, 1, 5);
        dnaAvoidanceWeight = Math.Clamp(dnaAvoidanceWeight, 1, 1);
        dnaAlignmentWeight = Math.Clamp(dnaAlignmentWeight, 1, 5);

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


    public static Vector3 CustomNormalize(Vector3 v)
    {
        double m = Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        if (m > 9.99999974737875E-06)
        {
            float fm = (float)m;
            v.x /= fm;
            v.y /= fm;
            v.z /= fm;
            return v;
        }
        else
            return Vector3.zero;
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
