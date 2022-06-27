
using System;
using System.Collections;
using System.Collections.Generic;
using extOSC;
using UnityEngine;
using System.Threading.Tasks;

public class FlockUnit : MonoBehaviour
{

    [SerializeField] public int flockSize => GameManager.Instance.flockSize;
    [SerializeField] public float buildProb => GameManager.Instance.buildProb;
    [SerializeField] public float respawnTime => GameManager.Instance.respawnTime;
    [SerializeField] public float deathMultiplier => GameManager.Instance.deathMultiplier;

    [SerializeField] public float cohesionDistance => GameManager.Instance.cohesionDistance;
    [SerializeField] public float alignmentDistance => GameManager.Instance.alignmentDistance;
    [SerializeField] public float separationDistance => GameManager.Instance.separationDistance;
    [SerializeField] public float preyDistance => GameManager.Instance.preyDistance;
    [SerializeField] public float predatorDistance => GameManager.Instance.preyDistance;

    [SerializeField] public float cohesionWeight => GameManager.Instance.cohesionWeight;
    [SerializeField] public float alignmentWeight => GameManager.Instance.alignmentWeight;
    [SerializeField] public float separationWeight => GameManager.Instance.separationWeight;

    [SerializeField] public float wanderWeight => GameManager.Instance.wanderWeight;

    [SerializeField] public float dnaWeight => GameManager.Instance.dnaWeight;
    [SerializeField] public float globalWeight => GameManager.Instance.globalWeight;

    [SerializeField] public float masterSpeed => GameManager.Instance.masterSpeed;

    [SerializeField] public float borderForce => GameManager.Instance.borderForce;
    [SerializeField] public float agility => GameManager.Instance.agility;

    [SerializeField] public float smoothDamp => GameManager.Instance.smoothDamp;

    [SerializeField] public float driftX => GameManager.Instance.driftX;
    [SerializeField] public float driftY => GameManager.Instance.driftY;
    [SerializeField] public float driftZ => GameManager.Instance.driftZ;

    [SerializeField] public float trailTime => GameManager.Instance.trailTime;

    [SerializeField] public float flowfieldStrength => GameManager.Instance.flowfieldStrength;


    public Flock assignedFlock;
    public string breed;
    public int oscNumber;
    Boolean initialized = false;

    public Color myColor;


    [Header("Health Values")]

    public int age;
    public float health;
    public float starterHealth;
    public float preyEaten;
    public int eatingState;
    public float hunger;
    public float FOVAngle;
    public float averageVelocity;
    public float totalAgility;
    public float totalSpeed;
    public int foodMeal;
    public int boidMeal;

    public bool Carnivore;

    [Header("Neighbors")]
    public List<FlockUnit> cohesionNeighbors = new List<FlockUnit>();
    public List<FlockUnit> alignmentNeighbors = new List<FlockUnit>();
    public List<FlockUnit> separationNeighbors = new List<FlockUnit>();
    public List<FoodUnit> foodNeighbors = new List<FoodUnit>();
    public List<FlockUnit> preyNeighbors = new List<FlockUnit>();
    public List<FlockUnit> predatorNeighbors = new List<FlockUnit>();


    [Header("Vector Values")]
    [Header("Flocking")]
    public Vector3 Cohesion;
    public Vector3 Separation;
    public Vector3 Alignment;
    public Vector3 BoundsVector;
    public Vector3 wander;

    [Header("Feeding")]
    public Vector3 FoodVector;
    public Vector3 HuntVector;
    public Vector3 FleeVector;

    [Header("Flowfield")]
    public Vector3 FlowfieldVector;
    public Vector3 FlowfieldMultiplier;

    [Header("Position")]
    public Vector3 spawnPosition;
    public Vector3 currentVelocity;
    public Vector3 currentMoveVector;
    public Vector3 finalMoveVector;
    public Vector3 totalVelocity;
    public Vector3 smoothVelocity;
    public Vector3 smoothFixedVelocity;

    public Vector3 wanderPosition;

    [Header("DNA")]
    public float dnaSpeed;

    public float dnaCohesionDist;
    public float dnaAlignmentDist;
    public float dnaSeparationDist;
    public float dnaPreyDistance;
    public float dnaPredatorDistance;

    public float dnaCohesionWeight;
    public float dnaAlignmentWeight;
    public float dnaSeparationWeight;

    public float dnaHuntStrength;
    public float dnaFleeStrength;
    public float dnaFoodStrength;

    public float dnaHuntAgility;
    public float dnaFleeAgility;
    public float dnaFoodAgility;

    public float dna_agility;
    public float midiNote;

    [Header("OSC Properties")]

    public string oscAddress_positionXYZ;

    public OSCMessage message_lifeState;
    public OSCMessage message_newPositionXYZ;
    public OSCMessage velocityMessage;
    public OSCMessage eatingStateMessage;
    public OSCMessage healthMessage;
    public OSCMessage midiNoteMessage;
    public OSCMessage midiPlayMessage;
    public OSCMessage foodMealMessage;
    public OSCMessage ageMessage;
    public OSCTransmitter transmitter;

    DNAboid dna;

    //public event EventHandler<BoidDeathEventArgs> Death;

    private void Awake()
    {
        health = UnityEngine.Random.Range(10, 100);
        starterHealth = health;

    }

    private void Start()
    {
        transmitter = assignedFlock.transmitter;


        transform.localScale = new Vector3(3, 3, 3);

        oscAddress_positionXYZ = "/" + breed + "/position/" + oscNumber;


        message_newPositionXYZ = new OSCMessage(oscAddress_positionXYZ);
        message_newPositionXYZ.AddValue(OSCValue.Float(transform.position.x));
        message_newPositionXYZ.AddValue(OSCValue.Float(transform.position.y));
        message_newPositionXYZ.AddValue(OSCValue.Float(transform.position.z));

        //message_lifeState = new OSCMessage("/" + breed + "/lifestate/" + oscNumber, OSCValue.Float(1));
        midiNoteMessage = new OSCMessage("/" + breed + "/midi/note/" + oscNumber);
        midiPlayMessage = new OSCMessage("/" + breed + "/midi/play/" + oscNumber, OSCValue.Float(0));
        healthMessage = new OSCMessage("/" + breed + "/health/" + oscNumber);

        velocityMessage = new OSCMessage("/" + breed + "/velocity/" + oscNumber, OSCValue.Float(0));

        foodMealMessage = new OSCMessage("/" + breed + "/foodMeal/" + oscNumber, OSCValue.Int(-1));

        ageMessage = new OSCMessage("/" + breed + "/age/" + oscNumber, OSCValue.Int(0));

        TrailRenderer Trail = gameObject.GetComponent<TrailRenderer>();

        if (Trail != null)
            Trail.time = trailTime;


        StartCoroutine(OSCBirth());
        StartCoroutine(OSCSender());
        StartCoroutine(Respawn());
        //StartCoroutine(HealthSize());
        StartCoroutine(CountAge());
        StartCoroutine(CheckAgent());
        StartCoroutine(CalcSlowStats());

    }


    //************************************************************************************************************************************
    // Coroutines
    //************************************************************************************************************************************

    private IEnumerator OSCBirth()
    {
        while (initialized == false)
        {
            message_lifeState = new("/" + breed + "/lifestate/" + oscNumber, OSCValue.Float(1));
            transmitter.Send(message_lifeState);
            initialized = true;
            yield return null;
        }

    }

    private IEnumerator OSCSender()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);

            message_newPositionXYZ.Values[0] = OSCValue.Float(transform.position.x);
            message_newPositionXYZ.Values[1] = OSCValue.Float(transform.position.y);
            message_newPositionXYZ.Values[2] = OSCValue.Float(transform.position.z);

            //midiPlayMessage.Values[0] = (OSCValue.Float(eatingState));
            //midiNoteMessage.AddValue(OSCValue.Float(midiNote));

            //healthMessage.AddValue(OSCValue.Float(health));

            velocityMessage.Values[0] = OSCValue.Float(averageVelocity);

            transmitter.Send(message_newPositionXYZ);

            //transmitter.Send(midiNoteMessage);
            //transmitter.Send(midiPlayMessage);
            //transmitter.Send(healthMessage);


            transmitter.Send(velocityMessage);

        }
    }

    private IEnumerator Respawn()
    {
        while (true)
        {
            //yield return new WaitForSeconds(respawnTime * UnityEngine.Random.Range(0f, 3f));
            yield return new WaitForSeconds(0.5f);


            var allUnits = assignedFlock.Boids;
            spawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            var breedChance = UnityEngine.Random.Range(0f, 1f);

            if (allUnits.Count < flockSize)
            {


                if (assignedFlock.Reproduce && health > starterHealth * 1.25 && age > assignedFlock.reproduceAge)
                {

                    if (assignedFlock.Builder)
                    {

                        if (breedChance <= buildProb)
                        {
                            assignedFlock.EnemyFlock.SpawnAgent(assignedFlock.EnemyFlock, assignedFlock.EnemyFlock.Boids, assignedFlock.EnemyFlock.BoidsIndex, assignedFlock.EnemyFlock.breed, spawnPosition, dna);
                        }
                        else if (breedChance > buildProb && assignedFlock.Reproduce)
                        {
                            assignedFlock.SpawnAgent(assignedFlock, assignedFlock.Boids, assignedFlock.BoidsIndex, breed, spawnPosition, dna);
                        }
                    }

                    if (assignedFlock.Reproduce && assignedFlock.Builder == false)
                    {
                        assignedFlock.SpawnAgent(assignedFlock, assignedFlock.Boids, assignedFlock.BoidsIndex, breed, spawnPosition, dna);
                    }


                    health *= 0.5f;

                }



            }

            else if (allUnits.Count == flockSize && assignedFlock.Builder && assignedFlock.EnemyFlock.Boids.Count < 6)
            {
                assignedFlock.EnemyFlock.SpawnAgent(assignedFlock.EnemyFlock, assignedFlock.EnemyFlock.Boids, assignedFlock.EnemyFlock.BoidsIndex, assignedFlock.EnemyFlock.breed, spawnPosition, dna);
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
            ageMessage.Values[0] = OSCValue.Int(age);
            transmitter.Send(ageMessage);
        }

    }

    private IEnumerator CalcSlowStats()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);

            //averageVelocity = AverageVelocity(totalVelocity);
            //averageVelocity = totalVelocity.magnitude * 100;

        }
    }

    private IEnumerator CheckAgent()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            if (assignedFlock.Living)
            {
                health -= deathMultiplier * Time.deltaTime;
            }


            if (Dead())
            {
                message_lifeState.Values[0] = OSCValue.Float(0);
                transmitter.Send(message_lifeState);


                message_newPositionXYZ.Values[0] = OSCValue.Float(0);
                message_newPositionXYZ.Values[1] = OSCValue.Float(0);
                message_newPositionXYZ.Values[2] = OSCValue.Float(0);
                transmitter.Send(message_newPositionXYZ);

                velocityMessage.Values[0] = OSCValue.Float(0);
                transmitter.Send(velocityMessage);

                //OSCMessage midiNoteMessage = new OSCMessage("/" + breed + "/midi/note/" + oscNumber, OSCValue.Float(0));
                //OSCMessage midiPlayMessage = new OSCMessage("/" + breed + "/midi/play/" + oscNumber, OSCValue.Float(0));
                //OSCMessage healthMessage = new OSCMessage("/" + breed + "/health/" + oscNumber, OSCValue.Float(0));

                //OSCMessage velocityMessage = new OSCMessage("/" + breed + "/velocity/" + oscNumber, OSCValue.Float(0));
                midiPlayMessage.Values[0] = OSCValue.Float(0);
                transmitter.Send(midiPlayMessage);



                //transmitter.Send(midiNoteMessage);

                //transmitter.Send(healthMessage);

                foodMealMessage.Values[0] = OSCValue.Int(-1);
                transmitter.Send(foodMealMessage);

                ageMessage.Values[0] = OSCValue.Int(0);
                transmitter.Send(ageMessage);

                //Death?.Invoke(this, new BoidDeathEventArgs { BoidObject = gameObject.GetComponent<FlockUnit>(), BreedObject = breed });

                gameObject.SetActive(false);
                assignedFlock.Boids.Remove(this);
                //assignedFlock.BoidsIndex.Remove(oscNumber);
                assignedFlock.agentsAvailable.Enqueue(this);

                //Destroy(this, 0.05f);
                //Destroy(gameObject, 0.05f);

            }
        }
    }


    //************************************************************************************************************************************
    //Initialization
    //************************************************************************************************************************************

    public void AssignFlock(Flock flock)
    {
        assignedFlock = flock;
    }

    public void NumberParticle(int number)
    {
        oscNumber = number;


    }

    public void setDNA(DNAboid newDNA)
    {
        dna = newDNA;

        //speed = scale(dna.genes[0], 0f, 1f, assignedFlock.minSpeed, assignedFlock.maxSpeed);
        //speed = Math.Clamp(speed, assignedFlock.minSpeed, assignedFlock.maxSpeed);

        dnaSpeed = scale(dna.genes[0], 0f, 1f, assignedFlock.minSpeed, 5f);
        dnaSpeed = Math.Clamp(dnaSpeed, assignedFlock.minSpeed, 5f);

        dnaCohesionDist = (scale(dna.genes[1], 0, 1, 5, 100));
        dnaSeparationDist = (scale(dna.genes[2], 0, 1, 5, 100));
        dnaAlignmentDist = (scale(dna.genes[3], 0, 1, 5, 100));
        dnaPreyDistance = (scale(dna.genes[4], 0, 1, 5, 100));
        dnaPredatorDistance = (scale(dna.genes[4], 0, 1, 5, 100));

        dnaCohesionWeight = (scale(dna.genes[5], 0, 1, 1, 5));
        dnaSeparationWeight = (scale(dna.genes[6], 0, 1, 1, 5));
        dnaAlignmentWeight = (scale(dna.genes[7], 0, 1, 1, 5));


        dnaHuntStrength = (scale(dna.genes[8], 0, 1, 1, 5));
        dnaFleeStrength = (scale(dna.genes[9], 0, 1, 1, 5));
        dnaFoodStrength = (scale(dna.genes[10], 0, 1, 1, 5));


        dnaHuntAgility = (scale(dna.genes[11], 0, 1, 5, 25));
        dnaFleeAgility = (scale(dna.genes[12], 0, 1, 5, 25));
        dnaFoodAgility = (scale(dna.genes[13], 0, 1, 5, 25));

        dna_agility = scale(dna.genes[11], 0, 1, 5, 25);


        midiNote = scale(dna.genes[12], 0, 1, 1, 10);

        dnaCohesionDist = Math.Clamp(dnaCohesionDist, 5, 100);
        dnaSeparationDist = Math.Clamp(dnaSeparationDist, 5, 100);
        dnaAlignmentDist = Math.Clamp(dnaAlignmentDist, 5, 100);
        dnaPreyDistance = Math.Clamp(dnaPreyDistance, 5, 100);

        dnaCohesionWeight = Math.Clamp(dnaCohesionWeight, 1, 5);
        dnaSeparationWeight = Math.Clamp(dnaSeparationWeight, 1, 1);
        dnaAlignmentWeight = Math.Clamp(dnaAlignmentWeight, 1, 5);

        dna_agility = Math.Clamp(dna_agility, 1, 25);
        midiNote = Math.Clamp(midiNote, 1, 10);


    }

    ///

    //public float AverageVelocity(Vector3 velocity)
    //{
    //    var averageVelocity = (Mathf.Abs(velocity.x) + Mathf.Abs(velocity.y) + Mathf.Abs(velocity.z)) * 0.33f;

    //    return averageVelocity;
    //}

    public bool Dead()
    {
        if (health <= 0)
            return true;
        else if (age >= assignedFlock.maxAge)
            return true;
        else
            return false;
    }

    public void onDeath()
    {
        gameObject.SetActive(false);
        assignedFlock.Boids.Remove(this);
        //assignedFlock.BoidsIndex.Remove(oscNumber);
        assignedFlock.agentsAvailable.Enqueue(this);
    }

    void Update()
    {
        MoveUnit();
        //needed?
        transform.localRotation = Quaternion.LookRotation(currentMoveVector, Vector3.up);
    }

    public void MoveUnit()
    {
        FindNeighbors();

        FindFlock(assignedFlock.PreyFlock.Boids, preyNeighbors, preyDistance, dnaPreyDistance);
        FindFlock(assignedFlock.PredatorFlock.Boids, predatorNeighbors, predatorDistance, dnaPredatorDistance);

        var huntAgility = CalculateTotalParam(assignedFlock.huntAgility, dnaFoodAgility);
        var fleeAgility = CalculateTotalParam(assignedFlock.fleeAgility, dnaFoodAgility);
        var foodAgility = CalculateTotalParam(assignedFlock.foodAgility, dnaFoodAgility);

        totalSpeed = CalculateTotalParam(masterSpeed, dnaSpeed);

        Cohesion = CalculateCohesionVector(cohesionNeighbors, agility) * CalculateTotalParam(cohesionWeight, dnaCohesionWeight);
        Separation = CalculateSeparation(separationNeighbors, agility) * CalculateTotalParam(separationWeight, dnaSeparationWeight);
        Alignment = CalculateAlignment(alignmentNeighbors, agility) * CalculateTotalParam(alignmentWeight, dnaAlignmentWeight);

        CalculateSwarm();
        CalculateHunger();
        wander = CalculateWander() * wanderWeight;

        //FlowfieldVector = CalculateFlowfield(assignedFlock.flowfield);
        //FlowfieldMultiplier = new Vector3(flowfieldStrength * 0.01f, flowfieldStrength * 0.01f, flowfieldStrength * 0.01f);
        //FlowfieldVector = Vector3.Scale(FlowfieldVector, FlowfieldMultiplier);

        BoundsVector = CalculateBoundsVector() * assignedFlock.boundsWeight;

        HuntVector = Vector3.zero;
        if (assignedFlock.Carnivore == true || this.Carnivore == true)
        {
            if (preyNeighbors.Count != 0)
            {
                HuntVector = CalculatePredatorVector(preyNeighbors, huntAgility) * CalculateTotalParam(assignedFlock.huntStrength, dnaHuntStrength) * hunger;
                EatPrey(preyNeighbors);
            }
            else
            {
                HuntVector = Vector3.zero;
            }
        }

        FleeVector = Vector3.zero;
        if (predatorNeighbors.Count != 0)
        {
            FleeVector = CalculateFlee(predatorNeighbors, fleeAgility) * CalculateTotalParam(assignedFlock.fleeStrength, dnaFleeStrength);
        }
        else
        {
            FleeVector = Vector3.zero;
        }

        if (assignedFlock.Herbivore == true)
        {
            FindFood();
            FoodVector = CalculateFoodVector(foodNeighbors, foodAgility) * CalculateTotalParam(assignedFlock.foodStrength, dnaFoodStrength) * hunger;
            EatFood(foodNeighbors);
        }
        else
        {
            FoodVector = Vector3.zero;
        }

        var driftVector = new Vector3(driftX, driftY, driftZ);
        var flockVector = Cohesion + Alignment + Separation;
        var eatVector = FoodVector + HuntVector + FleeVector;

        var moveVector = flockVector + eatVector + wander + BoundsVector;


        moveVector = Vector3.SmoothDamp(transform.forward, moveVector, ref currentVelocity, smoothDamp);

        //if (moveVector == Vector3.zero)
        //    moveVector = transform.forward;
        //transform.forward = moveVector;

        moveVector = CheckBounds(moveVector);

        //currentMoveVector = CheckBounds(moveVector);
        currentMoveVector = moveVector;

        totalVelocity = (currentMoveVector + driftVector) * (Time.deltaTime * totalSpeed);
        smoothFixedVelocity = Time.fixedDeltaTime * totalSpeed * (currentMoveVector + driftVector);
        averageVelocity = smoothFixedVelocity.magnitude * 10;

        //finalMoveVector = CheckBounds(totalVelocity);
        finalMoveVector = totalVelocity;

        transform.position += finalMoveVector;
        CalculateBoundaries(transform.position);

        //finalMoveVector = CheckBounds(totalVelocity);
        //BoundsVector = CalculateBoundsVector() * assignedFlock.boundsWeight;
        //totalVelocity = currentMoveVector;


    }

    //************************************************************************************************************************************
    //neighbors
    //************************************************************************************************************************************

    private void FindNeighbors()
    {
        cohesionNeighbors.Clear();
        alignmentNeighbors.Clear();
        separationNeighbors.Clear();

        var allUnits = assignedFlock.Boids;

        foreach (FlockUnit boid in allUnits)
        {
            var currentUnit = boid;
            //if (currentUnit != this && currentUnit != null)
            {
                if (currentUnit == this) return;
                float currentNeighborDistanceSqr = Vector3.SqrMagnitude(currentUnit.transform.position - transform.position);

                //  Color linecolor(string breed) => breed == "shadow" ? Color.cyan : Color.magenta;
                // Debug.DrawLine(myTransform.position, currentUnit.myTransform.position, linecolor(breed));
                if (currentNeighborDistanceSqr <= CalculateTotalParam(cohesionDistance * cohesionDistance, dnaCohesionDist * dnaCohesionDist))
                {
                    cohesionNeighbors.Add(currentUnit);
                }
                if (currentNeighborDistanceSqr <= CalculateTotalParam(separationDistance * separationDistance, dnaSeparationDist * dnaSeparationDist))
                {
                    separationNeighbors.Add(currentUnit);
                }
                if (currentNeighborDistanceSqr <= CalculateTotalParam(alignmentDistance * alignmentDistance, dnaAlignmentDist * dnaAlignmentDist))
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

            float currentPreyDistanceSqr = Vector3.SqrMagnitude(currentUnit.transform.position - transform.position);

            //if (currentPreyDistanceSqr <= ((preyDistance * preyDistance * generalWeight) + (dnaPreyDistance * dnaPreyDistance * dnaWeight)))
            if (currentPreyDistanceSqr <= CalculateTotalParam(preyDistance * preyDistance, dnaPreyDistance * dnaPreyDistance))
            {
                foodNeighbors.Add(currentUnit);



            }

        }
    }

    private void FindFlock(List<FlockUnit> flockBoids, List<FlockUnit> neighbors, float distance, float dnaDistance)
    {
        neighbors.Clear();

        foreach (FlockUnit boid in flockBoids)
        {
            if (boid.age > 10)
            {
                float currentBoidDistanceSqr = Vector3.SqrMagnitude(boid.transform.position - transform.position);
                if (currentBoidDistanceSqr <= CalculateTotalParam(distance * distance, dnaDistance * dnaDistance))
                {
                    neighbors.Add(boid);

                }
            }

        }
    }


    //************************************************************************************************************************************
    // Flocking
    //************************************************************************************************************************************


    private Vector3 CalculateCohesionVector(List<FlockUnit> neighbors, float forceMagnitude)
    {
        var force = Vector3.zero;

        if (neighbors.Count == 0)
            return Vector3.zero;
        int neighborsInFOV = 0;
        for (int i = 0; i < neighbors.Count; i++)
        {
            //if (IsInFOV(cohesionNeighbors[i].transform.position))
            //{
            neighborsInFOV++;
            force += neighbors[i].transform.position;

        }

        force /= neighbors.Count;
        force -= transform.position;
        force = CustomNormalize(force);
        force -= currentMoveVector;
        force = Vector3.ClampMagnitude(force, forceMagnitude);
        //Debug.DrawLine(transform.position, force, Color.green);
        return force;
    }

    private Vector3 CalculateAlignment(List<FlockUnit> neighbors, float forceMagnitude)
    {
        var force = Vector3.zero;
        //var force = transform.forward;
        //var force = transform.forward;
        //if (neighbors.Count == 0)
        //    return transform.forward;
        int neighborsInFOV = 0;
        for (int i = 0; i < neighbors.Count; i++)
        {
            //if (IsInFOV(alignmentNeighbors[i].transform.position))
            //{
            neighborsInFOV++;
            force += neighbors[i].transform.forward;
            //}
        }
        //alignmentVector /= neighborsInFOV;
        force = CustomNormalize(force);
        force *= totalSpeed;
        force -= currentMoveVector;
        force = Vector3.ClampMagnitude(force, forceMagnitude);
        //Debug.DrawLine(transform.position, force, Color.blue);
        return force;
    }

    private Vector3 CalculateSeparation(List<FlockUnit> neighbors, float forceMagnitude)
    {
        var force = Vector3.zero;
        if (neighbors.Count == 0)
            return Vector3.zero;
        int neighborsInFOV = 0;
        for (int i = 0; i < neighbors.Count; i++)
        {
            //if (IsInFOV(avoidanceNeighbors[i].transform.position))
            //{
            neighborsInFOV++;
            Vector3 neighborDir = (transform.position - neighbors[i].transform.position);
            float neighborDist = neighborDir.magnitude;
            neighborDir = CustomNormalize(neighborDir);
            neighborDir /= (Mathf.Pow(neighborDist, 2f));
            force += neighborDir;
            //}
        }

        //avoidanceVector /= neighborsInFOV;
        force = CustomNormalize(force);
        force *= totalSpeed;
        force -= currentMoveVector;
        //Debug.DrawLine(force, transform.position, Color.red);
        force = Vector3.ClampMagnitude(force, forceMagnitude);

        return force;
    }


    //************************************************************************************************************************************
    // Predator Prey
    //************************************************************************************************************************************

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

            Vector3 neighborDir = (neighbors[i].transform.position - transform.position);
            float neighborDist = neighborDir.magnitude;
            neighborDir = CustomNormalize(neighborDir);
            // weight the vector by the distance squared
            neighborDir /= (Mathf.Pow(neighborDist, 2f));
            force += neighborDir;
            //}
        }

        force = CustomNormalize(force);
        force *= totalSpeed;
        force -= currentMoveVector;
        force = Vector3.ClampMagnitude(force, forceMagnitude);

        return force;
    }

    private Vector3 CalculatePredatorVector(List<FlockUnit> neighbors, float forceMagnitude)
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

            Vector3 neighborDir = (neighbors[i].transform.position - transform.position);
            float neighborDist = neighborDir.magnitude;
            neighborDir = CustomNormalize(neighborDir);
            // weight the vector by the distance squared
            neighborDir /= (Mathf.Pow(neighborDist, 2f));
            force += neighborDir;
            //}
        }

        force = CustomNormalize(force);
        force *= totalSpeed;
        force -= currentMoveVector;
        force = Vector3.ClampMagnitude(force, forceMagnitude);

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
            neighborDir = CustomNormalize(neighborDir);
            neighborDir = neighborDir / (Mathf.Pow(neighborDist, 2f));
            force += neighborDir;
            //}
        }

        //avoidanceVector /= neighborsInFOV;
        force = CustomNormalize(force);
        force = force * totalSpeed;
        force = force - currentMoveVector;
        force = Vector3.ClampMagnitude(force, forceMagnitude);
        //Debug.DrawLine(transform.position, force, Color.red);
        return force;
    }




    private void CalculateSwarm()
    {
        if (cohesionNeighbors.Count >= 3)
        {
            totalSpeed *= 2f;
            //Carnivore = true;
        }
        else
        {
            //Carnivore = false;
        }

    }

    private void CalculateHunger()
    {
        if (60 < health && health < 100) hunger = 0;
        if (30 < health && health <= 60) hunger = 1;
        if (0 < health && health <= 30) hunger = 2;

        if (hunger == 2) { Carnivore = true; } else { Carnivore = false; }
        //hunger = scale(health, 0f, 100f, 5f, 0f);
        //if(health)
    }

    private Vector3 CalculateWander()
    {
        var wanderTarget = UnityEngine.Random.onUnitSphere;
        //wanderPosition = wanderTarget + transform.position;
        return wanderTarget;

    }


    //************************************************************************************************************************************
    // Borders + Boundaries
    //************************************************************************************************************************************


    private Vector3 CalculateBoundsVector()
    {
        // also functions as "bounce"
        var offsetToCenter = assignedFlock.transform.position - transform.position;
        bool isNearCenter = (offsetToCenter.magnitude >= assignedFlock.boundsDistance * 0.99f);

        return isNearCenter ? offsetToCenter.normalized * borderForce : Vector3.zero;
        //return isNearCenter ? offsetToCenter.clam : Vector3.zero;
    }

    private void CalculateBoundaries(Vector3 position)
    {
        float clampX, clampZ;
        clampX = Math.Clamp(position.x, -assignedFlock.boundsDistance, assignedFlock.boundsDistance);
        clampZ = Math.Clamp(position.z, -assignedFlock.boundsDistance, assignedFlock.boundsDistance);

        var clampY = assignedFlock.tier switch
        {
            3 => Math.Clamp(position.y, 0, assignedFlock.boundsDistance),
            2 => Math.Clamp(position.y, -assignedFlock.boundsDistance * 0.5f, assignedFlock.boundsDistance * 0.5f),
            1 => Math.Clamp(position.y, -assignedFlock.boundsDistance, 0),
            _ => Math.Clamp(position.y, -assignedFlock.boundsDistance, assignedFlock.boundsDistance),
        };
        transform.position = new Vector3(clampX, clampY, clampZ);

        //sphere
        //myTransform.position = Vector3.ClampMagnitude(position, assignedFlock.boundsDistance);
    }

    private Vector3 CheckBounds(Vector3 currentVelocity)
    {

        if (transform.position.x == assignedFlock.boundsDistance || transform.position.x == -assignedFlock.boundsDistance)
        {
            Debug.Log(breed + oscNumber + "x exceeded");

            return new Vector3(currentVelocity.x * -1.0f, currentVelocity.y, currentVelocity.z);

        }
        else if (transform.position.y == assignedFlock.boundsDistance || transform.position.y == -assignedFlock.boundsDistance)
        {
            Debug.Log(breed + oscNumber + "y exceeded");
            return new Vector3(currentVelocity.x, currentVelocity.y * -1.0f, currentVelocity.z);

        }
        else if (transform.position.z == assignedFlock.boundsDistance || transform.position.z == -assignedFlock.boundsDistance)
        {
            Debug.Log(breed + oscNumber + "z exceeded");
            return new Vector3(currentVelocity.x, currentVelocity.y, currentVelocity.z * -1.0f);

        }
        else
        {
            return currentVelocity;
        }

    }

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


    // ************************************************************************************************************************************
    // Eating
    //************************************************************************************************************************************

    public void EatFood(List<FoodUnit> neighbors)
    {
        foreach (FoodUnit prey in neighbors)
        {

            var desiredDirection = transform.position - prey.transform.position;
            var desiredDistance = desiredDirection.magnitude;
            if (desiredDistance < 5f)
            {
                prey.meal = true;
                foodMeal = prey.oscIndex;
                health += prey.health * 0.1f;
                preyEaten += 1;
                prey.Eaten();
                sendEat();
            }

        }
    }

    public void EatPrey(List<FlockUnit> neighbors)
    {

        foreach (FlockUnit prey in neighbors)
        {
            //sqr magnitude

            var desiredDirection = transform.position - prey.transform.position;
            var desiredDistance = desiredDirection.magnitude;
            if (desiredDistance < 5f && prey.age > assignedFlock.eatAge)
            {

                boidMeal = prey.oscNumber;
                health += prey.health * 0.1f;
                prey.Eaten();
                sendEat();



            }

        }

    }

    public void Eaten()
    {
        health = 0;

        onDeath();
        //include foodsize only when eaten
        //health -= 1f * Time.deltaTime;
        //health -= 1f;

    }

    async void sendEat()
    {
        midiPlayMessage.Values[0] = OSCValue.Float(1);
        transmitter.Send(midiPlayMessage);

        foodMealMessage.Values[0] = OSCValue.Int(foodMeal);
        transmitter.Send(foodMealMessage);

        await Task.Delay(200);

        midiPlayMessage.Values[0] = OSCValue.Float(0);
        transmitter.Send(midiPlayMessage);

    }

    // ************************************************************************************************************************************
    // Flowfield + FOV
    //************************************************************************************************************************************

    private Vector3 CalculateFlowfield(Flowfield flow)
    {
        Vector3 desiredVelocity = flow.Lookup(transform.position);
        desiredVelocity = CustomNormalize(desiredVelocity);
        desiredVelocity *= totalSpeed;
        Vector3 steerVelocity = desiredVelocity - currentMoveVector; // Steering is desired minus velocity
        Vector3.ClampMagnitude(steerVelocity, assignedFlock.flowForce);
        return steerVelocity;
    }

    private bool IsInFOV(Vector3 position)
    {
        return Vector3.Angle(transform.forward, position - transform.position) <= FOVAngle;
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


    // ************************************************************************************************************************************
    // Utility
    //************************************************************************************************************************************

    private float CalculateTotalParam(float flockParam, float dnaParam)
    {
        return (flockParam * globalWeight) + (dnaParam * dnaWeight);
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

//public class BoidDeathEventArgs : EventArgs
//{
//    public FlockUnit BoidObject { get; set; }
//    public string BreedObject { get; set; }
//}

public class DNAboid
{
    public float[] genes;
    public DNAboid()
    {

        genes = new float[20];
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
