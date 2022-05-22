

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using extOSC;
using UnityEngine;

public class Flock : MonoBehaviour
{
    [SerializeField] private Helpers helper;
    [SerializeField] public int flockSize => GameManager.Instance.flockSize;
    [SerializeField] public float mutualAttraction => GameManager.Instance.mutualAttraction;


    [Header("Spawn Setup")]
    [SerializeField] public string breed;
    [SerializeField] public int startAmount;


    [SerializeField] private FlockUnit flockUnitPrefab;
    public Flock EnemyFlock;
    public Food food;


    [SerializeField] private Vector3 spawnBounds;

    public float boundsDistance = 100;
    public float boundsWeight = 15;

    //[Range(0, 1)]
    //[SerializeField] public float mutualAttraction = 0;
    public float seekWeight = 1;
    //public float shadowSeekWeight = 1;


    [Header("Speed Setup")]
    [Range(0, 30)]
    [SerializeField] public float _minSpeed;
    public float minSpeed { get { return _minSpeed; } }

    [Range(0, 100)]
    [SerializeField] public float _maxSpeed;
    public float maxSpeed { get { return _maxSpeed; } }

    //[Header("Detection Distances")]

    //[Range(0, 10)]
    //[SerializeField] public float _cohesionDistance;
    //public float cohesionDistance { get { return _cohesionDistance; } }

    //[Range(0, 10)]
    //[SerializeField] public float _avoidanceDistance;
    //public float avoidanceDistance { get { return _avoidanceDistance; } }

    //[Range(0, 10)]
    //[SerializeField] public float _alignmentDistance;
    //public float alignmentDistance { get { return _alignmentDistance; } }



    //[Header("Behavior Weights")]

    //[Range(0, 10)]
    //[SerializeField] public float _cohesionWeight;
    //public float cohesionWeight { get { return _cohesionWeight; } }

    //[Range(0, 10)]
    //[SerializeField] public float _avoidanceWeight;
    //public float avoidanceWeight { get { return _avoidanceWeight; } }

    //[Range(0, 10)]
    //[SerializeField] public float _alignmentWeight;
    //public float alignmentWeight { get { return _alignmentWeight; } }



    [Header("Agents ")]
    public List<FlockUnit> Boids = new List<FlockUnit>();
    public List<int> BoidsIndex;



    [Header("OSC Properties")]
    public OSCTransmitter transmitter;

    //public OSCReceiver oscReceiver;
    public OSCMessage messageAddress2;
    public OSCMessage messageAddress3;
    public OSCMessage initMessage;


    Vector3 randomVector;
    Vector3 zeroVector;
    Vector3 spawnPosition;
    Quaternion rotation;

    DNAboid starterDna;



    private void Awake()
    {
        starterDna = new DNAboid();

        //ShadowBoidsIndex = new List<int>() { 0 };
        BoidsIndex = new List<int>() { 0 };
        zeroVector = new Vector3(0, 0, 0);

        //StopCoroutine(OSCInit());

    }

    private void Start()
    {
        _minSpeed = 6f;
        _maxSpeed = 100f;


        GenerateUnits();

        StartCoroutine(OSCInit());
        StopCoroutine(OSCInit());

        //randomVector = UnityEngine.Random.insideUnitSphere;
        //randomVector = new Vector3(randomVector.x * spawnBounds.x, randomVector.y * spawnBounds.y, randomVector.z * spawnBounds.z);

    }

    private void Update()
    {
        foreach (FlockUnit boid in Boids)
        {

            //boid.MoveUnit();
            Transform boidPosition = boid.myTransform;

            foreach (FoodUnit food in food.Foods)
            {
                Vector3 force = food.Attract(boidPosition.position);
                Transform foodTransform = food.transform;
                boid.ApplyAttractionForce(force, foodTransform);
            }

            if (mutualAttraction != 0)
            {
                foreach (FlockUnit enemy in EnemyFlock.Boids)
                {
                    Vector3 force = enemy.Attract(boidPosition);
                    Transform shadowPosition = enemy.transform;
                    boid.ApplyAttractionForce(mutualAttraction * seekWeight * force, shadowPosition);
                }

            }
        }

        //foreach (FlockUnit shadow in ShadowBoids)
        //{

        //    Transform shadowPosition = shadow.myTransform;
        //    foreach (FoodUnit food in food.Foods)
        //    {
        //        Vector3 force = food.Attract(shadowPosition.position);
        //        Transform foodPosition = food.transform;
        //        shadow.ApplyAttractionForce(force, foodPosition);
        //    }


        //    if (mutualAttraction != 0)
        //    {
        //        foreach (FlockUnit boid in Boids)
        //        {
        //            Vector3 force = boid.Attract(shadowPosition);
        //            Transform boidPosition = shadow.transform;
        //            shadow.ApplyAttractionForce(force * shadowSeekWeight * mutualAttraction, boidPosition);
        //        }

        //    }

        //}

    }


    //private IEnumerator OSCPlay()
    //{
    //    var initMessage = new OSCMessage("/new/");
    //    initMessage.AddValue(OSCValue.Float(1f));
    //    transmitter.Send(initMessage);
    //    Debug.Log(initMessage);
    //    yield return null;
    //}

    private IEnumerator OSCInit()
    {



        for (int i = 1; i <= flockSize; i++)
        {
            //var breed = "organic";
            var oscNumber = i;

            OSCMessage message_resetPositionX = new OSCMessage("/" + breed + "/position/x/" + i);
            OSCMessage message_resetPositionY = new OSCMessage("/" + breed + "/position/y/" + i);
            OSCMessage message_resetPositionZ = new OSCMessage("/" + breed + "/position/z/" + i);

            OSCMessage midiNoteMessage = new OSCMessage("/" + breed + "/midi/note/" + oscNumber);
            OSCMessage midiPlayMessage = new OSCMessage("/" + breed + "/midi/play/" + oscNumber);
            OSCMessage healthMessage = new OSCMessage("/" + breed + "/health/" + oscNumber);

            OSCMessage velocityMessage = new OSCMessage("/" + breed + "/velocity/" + oscNumber);


            message_resetPositionX.AddValue(OSCValue.Float(0));
            message_resetPositionY.AddValue(OSCValue.Float(0));
            message_resetPositionZ.AddValue(OSCValue.Float(0));

            midiNoteMessage.AddValue(OSCValue.Float(0));
            midiPlayMessage.AddValue(OSCValue.Float(0));
            healthMessage.AddValue(OSCValue.Float(0));


            velocityMessage.AddValue(OSCValue.Float(0));

            //Debug.Log(message_resetPosition);
            transmitter.Send(message_resetPositionX);
            transmitter.Send(message_resetPositionY);
            transmitter.Send(message_resetPositionZ);

            transmitter.Send(midiNoteMessage);
            transmitter.Send(midiPlayMessage);
            transmitter.Send(healthMessage);

            transmitter.Send(velocityMessage);



            yield return null;
        }

        //for (int i = 1; i <= flockSize; i++)
        //{
        //    var breed = "shadow";
        //    var oscNumber = i;

        //    OSCMessage message_resetPositionX = new OSCMessage("/shadow/position/x/" + i);
        //    OSCMessage message_resetPositionY = new OSCMessage("/shadow/position/y/" + i);
        //    OSCMessage message_resetPositionZ = new OSCMessage("/shadow/position/z/" + i);

        //    OSCMessage midiNoteMessage = new OSCMessage("/" + breed + "/midi/note/" + oscNumber);
        //    OSCMessage midiPlayMessage = new OSCMessage("/" + breed + "/midi/play/" + oscNumber);
        //    OSCMessage healthMessage = new OSCMessage("/" + breed + "/health/" + oscNumber);

        //    OSCMessage velocityMessage = new OSCMessage("/" + breed + "/velocity/" + oscNumber);



        //    message_resetPositionX.AddValue(OSCValue.Float(0));
        //    message_resetPositionY.AddValue(OSCValue.Float(0));
        //    message_resetPositionZ.AddValue(OSCValue.Float(0));

        //    midiNoteMessage.AddValue(OSCValue.Float(0));
        //    midiPlayMessage.AddValue(OSCValue.Float(0));
        //    healthMessage.AddValue(OSCValue.Float(0));

        //    velocityMessage.AddValue(OSCValue.Float(0));



        //    transmitter.Send(message_resetPositionX);
        //    transmitter.Send(message_resetPositionY);
        //    transmitter.Send(message_resetPositionZ);

        //    transmitter.Send(midiNoteMessage);
        //    transmitter.Send(midiPlayMessage);
        //    transmitter.Send(healthMessage);

        //    transmitter.Send(velocityMessage);

        //    yield return null;
        //}

        messageAddress3 = new OSCMessage("/play/");
        messageAddress3.AddValue(OSCValue.Float(1));
        Debug.Log(messageAddress3);
        transmitter.Send(messageAddress3);
    }

    //private void OSCInit()
    //{

    //    Debug.Log("start");

    //    //for (int i = 1; i <= flockSize; i++)
    //    //{
    //    //    var breed = "organic";
    //    //    var oscNumber = i;

    //    //    OSCMessage message_resetPositionX = new OSCMessage("/organic/position/x/" + i);
    //    //    OSCMessage message_resetPositionY = new OSCMessage("/organic/position/y/" + i);
    //    //    OSCMessage message_resetPositionZ = new OSCMessage("/organic/position/z/" + i);

    //    //    OSCMessage midiNoteMessage = new OSCMessage("/" + breed + "/midi/note/" + oscNumber);
    //    //    OSCMessage midiPlayMessage = new OSCMessage("/" + breed + "/midi/play/" + oscNumber);
    //    //    OSCMessage healthMessage = new OSCMessage("/" + breed + "/health/" + oscNumber);

    //    //    OSCMessage velocityMessage = new OSCMessage("/" + breed + "/velocity/" + oscNumber);


    //    //    message_resetPositionX.AddValue(OSCValue.Float(0));
    //    //    message_resetPositionY.AddValue(OSCValue.Float(0));
    //    //    message_resetPositionZ.AddValue(OSCValue.Float(0));

    //    //    midiNoteMessage.AddValue(OSCValue.Float(0));
    //    //    midiPlayMessage.AddValue(OSCValue.Float(0));
    //    //    healthMessage.AddValue(OSCValue.Float(0));


    //    //    velocityMessage.AddValue(OSCValue.Float(0));

    //    //    //Debug.Log(message_resetPosition);
    //    //    transmitter.Send(message_resetPositionX);
    //    //    transmitter.Send(message_resetPositionY);
    //    //    transmitter.Send(message_resetPositionZ);

    //    //    transmitter.Send(midiNoteMessage);
    //    //    transmitter.Send(midiPlayMessage);
    //    //    transmitter.Send(healthMessage);

    //    //    transmitter.Send(velocityMessage);



    //    //    //yield return null;
    //    //}

    //    //for (int i = 1; i <= flockSize; i++)
    //    //{
    //    //    var breed = "shadow";
    //    //    var oscNumber = i;

    //    //    OSCMessage message_resetPositionX = new OSCMessage("/shadow/position/x/" + i);
    //    //    OSCMessage message_resetPositionY = new OSCMessage("/shadow/position/y/" + i);
    //    //    OSCMessage message_resetPositionZ = new OSCMessage("/shadow/position/z/" + i);

    //    //    OSCMessage midiNoteMessage = new OSCMessage("/" + breed + "/midi/note/" + oscNumber);
    //    //    OSCMessage midiPlayMessage = new OSCMessage("/" + breed + "/midi/play/" + oscNumber);
    //    //    OSCMessage healthMessage = new OSCMessage("/" + breed + "/health/" + oscNumber);

    //    //    OSCMessage velocityMessage = new OSCMessage("/" + breed + "/velocity/" + oscNumber);



    //    //    message_resetPositionX.AddValue(OSCValue.Float(0));
    //    //    message_resetPositionY.AddValue(OSCValue.Float(0));
    //    //    message_resetPositionZ.AddValue(OSCValue.Float(0));

    //    //    midiNoteMessage.AddValue(OSCValue.Float(0));
    //    //    midiPlayMessage.AddValue(OSCValue.Float(0));
    //    //    healthMessage.AddValue(OSCValue.Float(0));

    //    //    velocityMessage.AddValue(OSCValue.Float(0));



    //    //    transmitter.Send(message_resetPositionX);
    //    //    transmitter.Send(message_resetPositionY);
    //    //    transmitter.Send(message_resetPositionZ);

    //    //    transmitter.Send(midiNoteMessage);
    //    //    transmitter.Send(midiPlayMessage);
    //    //    transmitter.Send(healthMessage);

    //    //    transmitter.Send(velocityMessage);

    //    //    //yield return null;
    //    //}

    //    messageAddress3 = new OSCMessage("/play/");
    //    messageAddress3.AddValue(OSCValue.Float(1));
    //    Debug.Log(messageAddress3);
    //    transmitter.Send(messageAddress3);


    //}


    private void GenerateUnits()
    {
        Debug.Log($"StarterDna: {JsonUtility.ToJson(starterDna)}");
        for (int i = 0; i < startAmount; i++)
        {
            GenerateAgent(this, Boids, BoidsIndex, breed, zeroVector, starterDna);

        }

    }

    public void GenerateAgent(Flock agentFlock, List<FlockUnit> agentFlockList, List<int> unitIndex, string agentBreed, Vector3 position, DNAboid parentDNA)
    {


        DNAboid childDNA = parentDNA.copy(); // make a copy of the DNA
        childDNA.mutate(0.2f);


        spawnPosition = transform.position + position;
        rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

        //var agentPrefab = flockUnitPrefab;
        //if (agentBreed == "shadow")
        //{
        //    agentPrefab = shadowUnitPrefab;
        //}

        var agentNumber = FindOpenIndex(unitIndex, flockSize);


        var newAgent = Instantiate(flockUnitPrefab, spawnPosition, rotation);
        var newAgentScript = newAgent.GetComponent<FlockUnit>();

        newAgentScript.Death += OnBoidDeath;

        newAgent.AssignFlock(agentFlock);
        newAgent.breed = agentBreed;

        newAgent.NumberParticle(agentNumber);
        newAgent.setDNA(childDNA);

        //newAgent.InitializeSpeed(agentSpeed);
        //newAgent.setDNA(new DNAboid());
        //var agentSpeed = UnityEngine.Random.Range(_minSpeed, _maxSpeed);
        //var indexList = unitIndex;
        //agentFlock.Add(newAgent.GetComponent<FlockUnit>());


        unitIndex.Add(agentNumber);
        agentFlockList.Add(newAgent.GetComponent<FlockUnit>());
        newAgent.transform.parent = this.transform;

        //if (agentBreed == "shadow")
        //{
        //    ShadowBoidsIndex.Add(agentNumber);
        //    ShadowBoids.Add(newAgent.GetComponent<FlockUnit>());
        //    newAgent.transform.parent = shadowFolder.transform;

        //}
        //else if (agentBreed == "organic")
        //{
        //    BoidsIndex.Add(agentNumber);
        //    Boids.Add(newAgent.GetComponent<FlockUnit>());
        //    newAgent.transform.parent = organicFolder.transform;
        //}
    }


    public void OnBoidDeath(object sender, BoidDeathEventArgs e)
    {
        Boids.Remove(e.BoidObject);
        //if (e.BreedObject == "organic")
        //{
        //    Boids.Remove(e.BoidObject);
        //}
        //else if (e.BreedObject == "shadow")
        //{
        //    ShadowBoids.Remove(e.BoidObject);
        //}

    }

    public float scale(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax)
    {

        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        return (NewValue);
    }

    public int FindOpenIndex(List<int> indexList, int listSize)
    {
        int indexValue;
        int a = indexList.OrderBy(x => x).First();
        int b = indexList.OrderBy(x => x).Last();
        List<int> myList2 = Enumerable.Range(1, listSize).ToList();
        List<int> remaining = myList2.Except(indexList).ToList();
        indexValue = remaining.First();
        indexValue = Math.Clamp(indexValue, 1, listSize);

        return indexValue;
    }

    //public void SendOSC(string instName, float x, float y, float z, int oscNumber)
    //{
    //    var NoteEncode = new List<float> {x, y, z};
    //    OSCHandler.Instance.SendMessageToClient(("Max" + (oscNumber + 1)) , instName, NoteEncode);
    //}
}

