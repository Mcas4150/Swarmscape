
using System;
using System.Collections;
using System.Collections.Generic;
using extOSC;
using UnityEngine;
using System.Threading.Tasks;

public class FlockUnit : MonoBehaviour
{

    public int flockSize => GameManager.Instance.flockSize;
    public float buildProb => GameManager.Instance.buildProb;
    public float respawnTime => GameManager.Instance.respawnTime;
    public float deathMultiplier => GameManager.Instance.deathMultiplier;

    public bool allNeighborsOn => GameManager.Instance.allNeighbors;
    public float allNeighborsDistance => GameManager.Instance.allNeighborsDistance;
    public float cohesionDistance => GameManager.Instance.cohesionDistance;
    public float alignmentDistance => GameManager.Instance.alignmentDistance;
    public float separationDistance => GameManager.Instance.separationDistance;
    public float preyDistance => GameManager.Instance.preyDistance;
    public float predatorDistance => GameManager.Instance.preyDistance;
    public float boundaryRadius => GameManager.Instance.boundaryRadius;

    public float cohesionWeight => GameManager.Instance.cohesionWeight;
    public float alignmentWeight => GameManager.Instance.alignmentWeight;
    public float separationWeight => GameManager.Instance.separationWeight;

    public float swarmMultiplier => GameManager.Instance.swarmWeight;

    public float boundaryWeight => GameManager.Instance.boundaryWeight;
    public float wanderWeight => GameManager.Instance.wanderWeight;

    public float dnaWeight => GameManager.Instance.dnaWeight;
    public float globalWeight => GameManager.Instance.globalWeight;

    public float masterSpeed => GameManager.Instance.masterSpeed;

    public float borderForce => GameManager.Instance.borderForce;
    public float agility => GameManager.Instance.agility;

    public float smoothDamp => GameManager.Instance.smoothDamp;
    public float hunger => GameManager.Instance.hunger;

    public float driftX => GameManager.Instance.driftX;
    public float driftY => GameManager.Instance.driftY;
    public float driftZ => GameManager.Instance.driftZ;

    public float trailTime => GameManager.Instance.trailTime;

    //public float flowfieldStrength => GameManager.Instance.flowfieldStrength;

    public bool edgeWrap => GameManager.Instance.edgeWrap;


    public Flock assignedFlock;
    public string breed;
    public int oscNumber;
    //Boolean initialized = false;

    // assign from flock
    //public Color myColor;
    //public Color unitNeighborColor;

    [Header("Health Values")]

    public int age;
    public float health;
    public float starterHealth;
    public float preyEaten;
    public int eatingState;
    private float swarmWeight;
    //public float hunger;
    public float FOVAngle;
    public float averageVelocity;
    public float totalAgility;
    public float totalSpeed;
    public int foodMeal;
    public int boidMeal;

    public bool Carnivore;

    [Header("Neighbors")]
    public List<FlockUnit> allNeighbors = new List<FlockUnit>();
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
    public Vector3 boundaryVector;
    private Vector3 BoundsVector;
    public Vector3 wander;

    [Header("Feeding")]
    public Vector3 FoodVector;
    public Vector3 HuntVector;
    public Vector3 FleeVector;

    [Header("Position")]
    public Vector3 totalVelocity;
    public Vector3 preTimeVelocity;
    public Vector3 noSpeedVelocity;

    private Vector3 spawnPosition;
    private Vector3 currentVelocity;

    //[Header("Flowfield")]
    //public Vector3 FlowfieldVector;
    //public Vector3 FlowfieldMultiplier;

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

    public Material trailMaterial;
    private Material myTrailMaterial;
    public Renderer rend;



    DNAboid dna;

    private void Awake()
    {

        starterHealth = health;

        myTrailMaterial = new Material(trailMaterial);
        TrailRenderer myTrailRenderer = GetComponent<TrailRenderer>();
        myTrailRenderer.material = myTrailMaterial;
        if (myTrailRenderer != null)
            myTrailRenderer.time = trailTime;

    }

    //private void Start()
    //{


    //    OSC_AssignTransmitter();
    //    OSC_Start();


    //    var healthRatio = health * 0.1f;
    //    transform.localScale = new Vector3(healthRatio, healthRatio, healthRatio);

    //    //OSC_Birth();

    //    //StartCoroutine(OSCBirth());
    //    StartCoroutine(OSCSender());
    //    StartCoroutine(Respawn());
    //    StartCoroutine(CalcObject());
    //    StartCoroutine(CheckAgent());


    //}

    public void InitializeBoid()
    {
        Enable();

        OSC_AssignTransmitter();
        OSC_Start();


        var healthRatio = health * 0.1f;
        transform.localScale = new Vector3(healthRatio, healthRatio, healthRatio);

        //OSC_Birth();

        //StartCoroutine(OSCBirth());
        StartCoroutine(OSCSender());
        StartCoroutine(Respawn());
        StartCoroutine(CalcObject());
        StartCoroutine(CheckAgent());
    }


    //************************************************************************************************************************************
    // Coroutines
    //************************************************************************************************************************************

    //private IEnumerator OSCBirth()
    //{
    //    while (initialized == false)
    //    {
    //        message_lifeState = new("/" + breed + "/lifestate/" + oscNumber, OSCValue.Float(1));
    //        transmitter.Send(message_lifeState);
    //        initialized = true;
    //        yield return null;
    //    }

    //}

    private IEnumerator OSCSender()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);

            OSC_Update();

        }
    }

    private IEnumerator Respawn()
    {
        while (true)
        {
            //yield return new WaitForSeconds(respawnTime * UnityEngine.Random.Range(0f, 3f));
            yield return new WaitForSeconds(0.5f);

            Reproduce();

        }
    }


    private IEnumerator CheckAgent()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            CheckHealth();
        }
    }

    private IEnumerator CalcObject()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            CalcAge();
            CalcTrailColor();
            CalcBoidSize();
        }
    }


    //************************************************************************************************************************************
    // OSC
    //************************************************************************************************************************************


    public void OSC_AssignTransmitter()
    {
        transmitter = assignedFlock.transmitter;
    }

    public void OSC_Start()
    {
        message_lifeState = new(CreateOSCAddress("lifestate"), OSCValue.Float(0));

        message_newPositionXYZ = new OSCMessage(CreateOSCAddress("position"));
        message_newPositionXYZ.AddValue(OSCValue.Float(transform.position.x));
        message_newPositionXYZ.AddValue(OSCValue.Float(transform.position.y));
        message_newPositionXYZ.AddValue(OSCValue.Float(transform.position.z));

        midiNoteMessage = new OSCMessage(CreateOSCAddress("midi/note"));
        midiPlayMessage = new OSCMessage(CreateOSCAddress("midi/play"), OSCValue.Float(0));
        healthMessage = new OSCMessage(CreateOSCAddress("health"));

        velocityMessage = new OSCMessage(CreateOSCAddress("velocity"), OSCValue.Float(0));

        foodMealMessage = new OSCMessage(CreateOSCAddress("foodMeal"), OSCValue.Int(-1));

        ageMessage = new OSCMessage(CreateOSCAddress("age"), OSCValue.Int(0));
    }


    public void OSC_Update()
    {
        message_newPositionXYZ.Values[0] = OSCValue.Float(transform.position.x);
        message_newPositionXYZ.Values[1] = OSCValue.Float(transform.position.y);
        message_newPositionXYZ.Values[2] = OSCValue.Float(transform.position.z);

        velocityMessage.Values[0] = OSCValue.Float(averageVelocity);

        //midiNoteMessage.AddValue(OSCValue.Float(midiNote));
        //healthMessage.AddValue(OSCValue.Float(health));

        transmitter.Send(message_newPositionXYZ);
        transmitter.Send(velocityMessage);

        //transmitter.Send(midiNoteMessage);
        //transmitter.Send(healthMessage);
    }


    public void OSC_Birth()
    {
        Debug.Log("birth");
        //message_lifeState = new("/" + breed + "/lifestate/" + oscNumber, OSCValue.Float(1));
        message_lifeState.Values[0] = OSCValue.Float(1);
        transmitter.Send(message_lifeState);
    }

    public void OSC_Death()
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

        message_lifeState.Values[0] = OSCValue.Float(0);
        transmitter.Send(message_lifeState);

        //transmitter.Send(midiNoteMessage);

        //transmitter.Send(healthMessage);

        foodMealMessage.Values[0] = OSCValue.Int(-1);
        transmitter.Send(foodMealMessage);

        ageMessage.Values[0] = OSCValue.Int(0);
        transmitter.Send(ageMessage);
    }

    public void OSC_Age(int age)
    {
        ageMessage.Values[0] = OSCValue.Int(age);
        transmitter.Send(ageMessage);
    }

    async void OSC_Eat()
    {
        midiPlayMessage.Values[0] = OSCValue.Float(1);
        transmitter.Send(midiPlayMessage);

        foodMealMessage.Values[0] = OSCValue.Int(foodMeal);
        transmitter.Send(foodMealMessage);

        await Task.Delay(200);

        midiPlayMessage.Values[0] = OSCValue.Float(0);
        transmitter.Send(midiPlayMessage);

    }


    //************************************************************************************************************************************
    //Initialization
    //************************************************************************************************************************************


    public void AssignStats(Flock agentFlock, string agentBreed, DNAboid parentDNA)
    {
        AssignFlock(agentFlock);
        breed = agentBreed;
        health = UnityEngine.Random.Range(50, 100);

        DNAboid childDNA = parentDNA.Copy();
        childDNA.Mutate(0.2f);
        SetDNA(childDNA);
    }

    public void AssignID(int unitIndex, Color breedColor)
    {
        GetComponent<MeshRenderer>().material.color = breedColor;
        AssignIndex(unitIndex);
    }

    public void AssignFlock(Flock flock)
    {
        assignedFlock = flock;
    }

    public void AssignIndex(int number)
    {
        oscNumber = number;
    }

    public void AssignPosition(Vector3 newPosition, Quaternion newRotation)
    {
        transform.SetPositionAndRotation(newPosition, newRotation);
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Reproduce()
    {
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

    public void SetDNA(DNAboid newDNA)
    {
        dna = newDNA;

        //speed = scale(dna.genes[0], 0f, 1f, assignedFlock.minSpeed, assignedFlock.maxSpeed);
        //speed = Math.Clamp(speed, assignedFlock.minSpeed, assignedFlock.maxSpeed);

        dnaSpeed = scale(dna.genes[0], 0f, 1f, assignedFlock.minSpeed, 20f);
        dnaSpeed = Math.Clamp(dnaSpeed, assignedFlock.minSpeed, 20f);

        dnaCohesionDist = (scale(dna.genes[1], 0, 1, 5, 100));
        dnaSeparationDist = (scale(dna.genes[2], 0, 1, 5, 100));
        dnaAlignmentDist = (scale(dna.genes[3], 0, 1, 5, 100));
        dnaPreyDistance = (scale(dna.genes[4], 0, 1, 5, 25));
        dnaPredatorDistance = (scale(dna.genes[4], 0, 1, 5, 25));

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

    public void CheckHealth()
    {
        if (assignedFlock.Living)
        {
            health -= deathMultiplier * Time.deltaTime;
        }

        if (Dead())
        {
            onDeath();

        }
    }

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
        OSC_Death();

        gameObject.SetActive(false);
        assignedFlock.Boids.Remove(this);
        //assignedFlock.BoidsIndex.Remove(oscNumber);
        assignedFlock.agentsAvailable.Enqueue(this);
    }

    public void CalcAge()
    {
        age += 1;
        OSC_Age(age);

    }

    public void CalcTrailColor()
    {
        var trailColor = Color.green;
        if (hunger >= 2) { trailColor = Color.red; }
        if (hunger >= 1 && hunger < 2) { trailColor = Color.yellow; }
        if (hunger >= 0 && hunger < 1) { trailColor = Color.green; }

        myTrailMaterial.SetColor("_EmissionColor", trailColor);
    }

    public void CalcBoidSize()
    {
        var healthRatio = health * 0.1f;
        healthRatio = Math.Clamp(healthRatio, 0, 10);
        var boidSize = new Vector3(healthRatio, healthRatio, healthRatio);
        transform.localScale = boidSize;
    }

    void Update()
    {
        MoveUnit();
        transform.localRotation = Quaternion.LookRotation(totalVelocity, Vector3.up);
    }

    public void MoveUnit()
    {
        ///************************************************************************************************************
        ///  Calculate Flock
        ///************************************************************************************************************

        if (allNeighborsOn) { FindAllNeighbors(); } else { FindNeighbors(); }


        totalSpeed = CalculateTotalParam(masterSpeed, dnaSpeed, swarmWeight);

        Cohesion = Vector3.zero;
        Separation = Vector3.zero;
        Alignment = Vector3.zero;
        wander = Vector3.zero;
        boundaryVector = Vector3.zero;

        if (allNeighborsOn)
        {

            Cohesion = CalculateCohesionVector(allNeighbors, agility) * CalculateTotalParam(cohesionWeight, dnaCohesionWeight);
            Separation = CalculateSeparation(allNeighbors, agility) * CalculateTotalParam(separationWeight, dnaSeparationWeight);
            Alignment = CalculateAlignment(allNeighbors, agility) * CalculateTotalParam(alignmentWeight, dnaAlignmentWeight);
        }
        else
        {

            Cohesion = CalculateCohesionVector(cohesionNeighbors, agility) * CalculateTotalParam(cohesionWeight, dnaCohesionWeight, swarmWeight);
            Separation = CalculateSeparation(separationNeighbors, agility) * CalculateTotalParam(separationWeight, dnaSeparationWeight);
            Alignment = CalculateAlignment(alignmentNeighbors, agility) * CalculateTotalParam(alignmentWeight, dnaAlignmentWeight);
        }


        CalculateSwarm();

        wander = CalculateWander(agility) * wanderWeight;

        ///************************************************************************************************************
        ///  Calculate Predator Prey
        ///************************************************************************************************************

        FindFlock(assignedFlock.PreyFlock.Boids, preyNeighbors, preyDistance, dnaPreyDistance);
        FindFlock(assignedFlock.PredatorFlock.Boids, predatorNeighbors, predatorDistance, dnaPredatorDistance);

        CalculateHunger();

        var huntAgility = CalculateTotalParam(assignedFlock.huntAgility, dnaFoodAgility);
        var fleeAgility = CalculateTotalParam(assignedFlock.fleeAgility, dnaFoodAgility);
        var foodAgility = CalculateTotalParam(assignedFlock.foodAgility, dnaFoodAgility);

        HuntVector = Vector3.zero;
        if (assignedFlock.Carnivore == true || this.Carnivore == true)
        {
            if (preyNeighbors.Count != 0)
            {
                HuntVector = CalculatePredatorVector(preyNeighbors, huntAgility) * CalculateTotalParam(assignedFlock.huntStrength, dnaHuntStrength, swarmWeight) * hunger;

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
            FoodVector = CalculateFoodVector(foodNeighbors, foodAgility) * CalculateTotalParam(assignedFlock.foodStrength, dnaFoodStrength, swarmWeight) * hunger;
            EatFood(foodNeighbors);
        }
        else
        {
            FoodVector = Vector3.zero;
        }

        //FlowfieldVector = CalculateFlowfield(assignedFlock.flowfield);
        //FlowfieldMultiplier = new Vector3(flowfieldStrength * 0.01f, flowfieldStrength * 0.01f, flowfieldStrength * 0.01f);
        //FlowfieldVector = Vector3.Scale(FlowfieldVector, FlowfieldMultiplier);

        //BoundsVector = CalculateBoundsVector() * assignedFlock.boundsWeight;

        ///************************************************************************************************************
        ///  Apply Forces
        ///************************************************************************************************************

        boundaryVector = Boundary(boundaryRadius, agility) * boundaryWeight;

        var driftVector = new Vector3(driftX, driftY, driftZ);
        var flockVector = Cohesion + Alignment + Separation;
        var eatVector = FoodVector + HuntVector + FleeVector;

        var acceleration = flockVector + eatVector + wander + BoundsVector + boundaryVector;

        var moveVector = Vector3.SmoothDamp(transform.forward, acceleration, ref currentVelocity, smoothDamp);

        //currentMoveVector = CheckBounds(moveVector);
        totalVelocity += moveVector;
        preTimeVelocity = totalVelocity;
        //noSpeedVelocity = (totalVelocity + driftVector) * (Time.deltaTime);
        totalVelocity = (totalVelocity + driftVector) * (Time.deltaTime * totalSpeed);

        var smoothFixedVelocity = Time.fixedDeltaTime * totalSpeed * (moveVector + driftVector);
        averageVelocity = smoothFixedVelocity.magnitude * 10;

        transform.position += totalVelocity;

        CalculateBoundaries(transform.position);
        if (!edgeWrap) CheckBounds();


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

            if (currentUnit != this)
            {

                float currentNeighborDistanceSqr = Vector3.SqrMagnitude(transform.position - currentUnit.transform.position);
                var maxCohesionDist = CalculateTotalParam(cohesionDistance * cohesionDistance, dnaCohesionDist * dnaCohesionDist);

                if (currentNeighborDistanceSqr <= maxCohesionDist)
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

    private void FindAllNeighbors()
    {
        allNeighbors.Clear();


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
                if (currentNeighborDistanceSqr <= allNeighborsDistance * allNeighborsDistance)
                {
                    allNeighbors.Add(currentUnit);
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

        for (int i = 0; i < neighbors.Count; i++)
        {
            force += neighbors[i].transform.position;
            //Debug.DrawLine(transform.position, neighbors[i].transform.position, Color.blue);
        }

        force /= neighbors.Count;
        force -= transform.position;


        force = ApplyForce(force, forceMagnitude);
        return force;
    }

    private Vector3 CalculateAlignment(List<FlockUnit> neighbors, float forceMagnitude)
    {
        var force = Vector3.zero;
        //var force = transform.forward;
        //var force = transform.forward;
        //if (neighbors.Count == 0)
        //    return transform.forward;

        for (int i = 0; i < neighbors.Count; i++)
        {
            force += neighbors[i].transform.forward;
            //Debug.DrawLine(transform.position, neighbors[i].transform.position, Color.red);

        }
        //alignmentVector /= neighborsInFOV;

        force = ApplyForce(force, forceMagnitude);
        return force;
    }

    private Vector3 CalculateSeparation(List<FlockUnit> neighbors, float forceMagnitude)
    {
        var force = Vector3.zero;

        for (int i = 0; i < neighbors.Count; i++)
        {
            force += Avoid(neighbors[i].transform.position);
            //Debug.DrawLine(transform.position, neighbors[i].transform.position, Color.magenta);
        }

        //avoidanceVector /= neighborsInFOV;
        force = ApplyForce(force, forceMagnitude);

        return force;
    }

    private Vector3 CalculateWander(float forceMagnitude)
    {
        var wanderTarget = UnityEngine.Random.onUnitSphere;

        wanderTarget *= totalSpeed;
        //Debug.DrawLine(transform.position, transform.position + wanderTarget, Color.white);
        wanderTarget -= totalVelocity;
        var force = Vector3.ClampMagnitude(wanderTarget, forceMagnitude);
        return force;

    }

    //************************************************************************************************************************************
    // Predator Prey
    //************************************************************************************************************************************

    private Vector3 CalculateFoodVector(List<FoodUnit> neighbors, float forceMagnitude)
    {

        var force = Vector3.zero;
        for (int i = 0; i < neighbors.Count; i++)
        {
            force += Seek(neighbors[i].transform.position);
        }

        force = ApplyForce(force, forceMagnitude);

        return force;
    }

    private Vector3 CalculatePredatorVector(List<FlockUnit> neighbors, float forceMagnitude)
    {
        var force = Vector3.zero;

        for (int i = 0; i < neighbors.Count; i++)
        {
            force += Seek(neighbors[i].transform.position);
        }

        force = ApplyForce(force, forceMagnitude);

        return force;
    }

    private Vector3 CalculateFlee(List<FlockUnit> neighbors, float forceMagnitude)
    {
        var force = Vector3.zero;

        for (int i = 0; i < neighbors.Count; i++)
        {

            force += Avoid(neighbors[i].transform.position);
        }

        force = ApplyForce(force, forceMagnitude);

        return force;
    }

    private void CalculateSwarm()
    {
        if (cohesionNeighbors.Count >= 3)
        {

            swarmWeight = swarmMultiplier;
            //hunger = 2;
            //totalSpeed *= 2f;
            //Carnivore = true;
        }
        else
        {
            swarmWeight = 1;
            //Carnivore = false;
        }

    }

    private void CalculateHunger()
    {
        //if (60 < health && health < 100) hunger = 0;
        //if (30 < health && health <= 60) hunger = 1;
        //if (0 < health && health <= 30) hunger = 2;

        if (hunger >= 2) { Carnivore = true; } else { Carnivore = false; }

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

        if (edgeWrap)
        {
            clampX = position.x;
            clampZ = position.z;

            if (position.x > assignedFlock.boundsDistance)
            {
                clampX = -assignedFlock.boundsDistance;
            }

            if (position.x < -assignedFlock.boundsDistance)
            {
                clampX = assignedFlock.boundsDistance;
            }

            if (position.z > assignedFlock.boundsDistance)
            {
                clampZ = -assignedFlock.boundsDistance;
            }

            if (position.z < -assignedFlock.boundsDistance)
            {
                clampZ = assignedFlock.boundsDistance;
            }

        }

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
            //Debug.Log(breed + oscNumber + "x exceeded");

            return new Vector3(currentVelocity.x * -1.0f, currentVelocity.y, currentVelocity.z);

        }
        else if (transform.position.y == assignedFlock.boundsDistance || transform.position.y == -assignedFlock.boundsDistance)
        {
            //Debug.Log(breed + oscNumber + "y exceeded");
            return new Vector3(currentVelocity.x, currentVelocity.y * -1.0f, currentVelocity.z);

        }
        else if (transform.position.z == assignedFlock.boundsDistance || transform.position.z == -assignedFlock.boundsDistance)
        {
            //Debug.Log(breed + oscNumber + "z exceeded");
            return new Vector3(currentVelocity.x, currentVelocity.y, currentVelocity.z * -1.0f);

        }
        else
        {
            return currentVelocity;
        }

    }

    private void CheckBounds()
    {

        if (transform.position.x == assignedFlock.boundsDistance || transform.position.x == -assignedFlock.boundsDistance)
        {
            //Debug.Log(breed + oscNumber + "x exceeded");

            totalVelocity = new Vector3(totalVelocity.x * -1.0f, totalVelocity.y, totalVelocity.z);

        }
        if (transform.position.y == assignedFlock.boundsDistance || transform.position.y == -assignedFlock.boundsDistance)
        {
            //Debug.Log(breed + oscNumber + "y exceeded");
            totalVelocity = new Vector3(totalVelocity.x, totalVelocity.y * -1.0f, totalVelocity.z);

        }
        if (transform.position.z == assignedFlock.boundsDistance || transform.position.z == -assignedFlock.boundsDistance)
        {
            //Debug.Log(breed + oscNumber + "z exceeded");
            totalVelocity = new Vector3(totalVelocity.x, totalVelocity.y, totalVelocity.z * -1.0f);

        }


    }

    private Vector3 Boundary(float boundaryRadius, float forceMagnitude)
    {
        float dist = Vector3.Distance(transform.position, Vector3.zero);

        Vector3 force = Vector3.zero;
        if (dist > boundaryRadius)
        {
            force = -transform.position;

            force = ApplyForce(force, forceMagnitude);

            // strengthen the force as the boid gets farther out, to encourage return
            Vector3 weak = force * Mathf.Abs(dist / boundaryRadius);
            Vector3 strong = force * Mathf.Abs(dist - boundaryRadius);
            float t = Mathf.Abs((dist - boundaryRadius) / Mathf.Pow(boundaryRadius, 2f));
            force = Vector3.Lerp(weak, strong, Mathf.Clamp(t, 0f, 1f));
        }

        return force;
    }

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
                OSC_Eat();
                //sendEat();
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
                OSC_Eat();
                //sendEat();



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

    //async void sendEat()
    //{
    //    midiPlayMessage.Values[0] = OSCValue.Float(1);
    //    transmitter.Send(midiPlayMessage);

    //    foodMealMessage.Values[0] = OSCValue.Int(foodMeal);
    //    transmitter.Send(foodMealMessage);

    //    await Task.Delay(200);

    //    midiPlayMessage.Values[0] = OSCValue.Float(0);
    //    transmitter.Send(midiPlayMessage);

    //}

    // ************************************************************************************************************************************
    // Flowfield + FOV
    //************************************************************************************************************************************

    //private Vector3 CalculateFlowfield(Flowfield flow)
    //{
    //    Vector3 desiredVelocity = flow.Lookup(transform.position);
    //    desiredVelocity = CustomNormalize(desiredVelocity);
    //    desiredVelocity *= totalSpeed;
    //    Vector3 steerVelocity = desiredVelocity - totalVelocity; // Steering is desired minus velocity
    //    Vector3.ClampMagnitude(steerVelocity, assignedFlock.flowForce);
    //    return steerVelocity;
    //}

    private bool IsInFOV(Vector3 position)
    {
        return Vector3.Angle(transform.forward, position - transform.position) <= FOVAngle;
    }

    // ************************************************************************************************************************************
    // Utility
    //************************************************************************************************************************************

    private Vector3 ApplyForce(Vector3 newForce, float forceMagnitude)
    {
        var force = CustomNormalize(newForce);
        force *= totalSpeed;
        force -= totalVelocity;
        force = Vector3.ClampMagnitude(force, forceMagnitude);
        return force;
    }

    private Vector3 Seek(Vector3 targetPosition)
    {
        Vector3 targetDir = (targetPosition - transform.position);
        float targetDist = targetDir.magnitude;
        targetDir = CustomNormalize(targetDir);
        // weight the vector by the distance squared
        targetDir /= (Mathf.Pow(targetDist, 2f));
        return targetDir;
    }

    private Vector3 Avoid(Vector3 targetPosition)
    {
        Vector3 targetDir = (transform.position - targetPosition);
        float targetDist = targetDir.magnitude;
        targetDir = CustomNormalize(targetDir);
        // weight the vector by the distance squared
        targetDir /= (Mathf.Pow(targetDist, 2f));
        return targetDir;
    }

    private float CalculateTotalParam(float flockParam, float dnaParam)
    {
        return (flockParam * globalWeight) + (dnaParam * dnaWeight);
    }

    private float CalculateTotalParam(float flockParam, float dnaParam, float swarmWeight)
    {
        return (flockParam * globalWeight) + (dnaParam * dnaWeight) * swarmWeight;
    }

    private string CreateOSCAddress(string parameter)
    {
        return "/" + breed + "/" + parameter + "/" + oscNumber;
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

    public void Mutate(float rate)
    {
        for (int i = 0; i < genes.Length; i++)
        {
            genes[i] += UnityEngine.Random.Range(-rate, rate); // We'll change each gene by + or - the rate at random
        }
    }

    public DNAboid Copy()
    {
        float[] newgenes = new float[genes.Length];
        newgenes = (float[])genes.Clone();
        return new DNAboid(newgenes);
    }

}


