

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using extOSC;
using UnityEngine;

public class Flock : MonoBehaviour
{

    [SerializeField] public int flockSize => GameManager.Instance.flockSize;


    [Header("Spawn Setup")]
    [SerializeField] private FlockUnit flockUnitPrefab;
    [SerializeField] private FlockUnit shadowUnitPrefab;
    public GameObject organicFolder;
    public GameObject shadowFolder;
    public GameObject foodPrefab;
    //[SerializeField] private int flockSize;
    [SerializeField] private Vector3 spawnBounds;
    //[SerializeField] public int foodSize => GameManager.Instance.foodSeedMax;
    public float boundsDistance = 100;
    public float boundsWeight = 15;

    [Range(0, 1)]
    [SerializeField] public float mutualAttraction = 0;
    public float organicSeekWeight = 1;
    public float shadowSeekWeight = 1;

    public int worldYear = 0;
    public int foodSize = 5;

    public string season = "spring";

    [Header("Speed Setup")]
    [Range(0, 30)]
    [SerializeField] public float _minSpeed;
    public float minSpeed { get { return _minSpeed; } }

    [Range(0, 100)]
    [SerializeField] public float _maxSpeed;
    public float maxSpeed { get { return _maxSpeed; } }

    [Header("Detection Distances")]

    [Range(0, 10)]
    [SerializeField] public float _cohesionDistance;
    public float cohesionDistance { get { return _cohesionDistance; } }

    [Range(0, 10)]
    [SerializeField] public float _avoidanceDistance;
    public float avoidanceDistance { get { return _avoidanceDistance; } }

    [Range(0, 10)]
    [SerializeField] public float _alignmentDistance;
    public float alignmentDistance { get { return _alignmentDistance; } }



    [Header("Behavior Weights")]

    [Range(0, 10)]
    [SerializeField] public float _cohesionWeight;
    public float cohesionWeight { get { return _cohesionWeight; } }

    [Range(0, 10)]
    [SerializeField] public float _avoidanceWeight;
    public float avoidanceWeight { get { return _avoidanceWeight; } }

    [Range(0, 10)]
    [SerializeField] public float _alignmentWeight;
    public float alignmentWeight { get { return _alignmentWeight; } }



    [Header("Agents ")]

    public List<FoodUnit> Foods = new List<FoodUnit>();
    public List<FlockUnit> Boids = new List<FlockUnit>();
    public List<FlockUnit> ShadowBoids = new List<FlockUnit>();
    public List<int> BoidsIndex;
    public List<int> ShadowBoidsIndex;
    public List<int> FoodsIndex;
    //public List<OSCMessage> OscFood = new List<OSCMessage>();
    public List<Vector3> OscFood = new List<Vector3>();



    [Header("OSC Properties")]
    public OSCTransmitter transmitter;

    public OSCReceiver oscReceiver;
    public OSCMessage messageAddress2;
    public OSCMessage messageAddress3;
    public OSCMessage initMessage;
    public Boolean initialized;

    Vector3 randomVector;
    Vector3 zeroVector;
    Vector3 spawnPosition;
    Quaternion rotation;

    DNAboid starterDna;


    float scale(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax)
    {

        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        return (NewValue);
    }

    private void Awake()
    {
        starterDna = new DNAboid();
        //flockSize = 16;

        ShadowBoidsIndex = new List<int>() { 0 };
        BoidsIndex = new List<int>() { 0 };
        FoodsIndex = new List<int>() { 1 };
        zeroVector = new Vector3(0, 0, 0);



        StartCoroutine(OSCInit());
        StopCoroutine(OSCInit());

    }

    private void Start()
    {
        //_minSpeed = 6f;
        //_maxSpeed = 15f;

        setSeason("spring");

        transmitter.RemoteHost = "127.0.0.1";
        //transmitter.RemotePort = 7000;
        transmitter.RemotePort = 57120;

        oscReceiver = gameObject.AddComponent<OSCReceiver>();
        oscReceiver.LocalPort = 7500;
        oscReceiver.Bind("/flucoma/xyz", MessageReceived);



        StartCoroutine(FoodCounter());
        StartCoroutine(CountWorldYear());

        GenerateUnits();
        // GenerateFoods();

        //randomVector = UnityEngine.Random.insideUnitSphere;
        //randomVector = new Vector3(randomVector.x * spawnBounds.x, randomVector.y * spawnBounds.y, randomVector.z * spawnBounds.z);




    }

    private void Update()
    {
        foreach (FlockUnit boid in Boids)
        {

            //boid.MoveUnit();
            Transform boidPosition = boid.myTransform;
            //  enumeration error
            foreach (FoodUnit food in Foods)
            {
                Vector3 force = food.Attract(boidPosition);
                Transform foodPosition = food.transform;
                boid.ApplyAttractionForce(force, foodPosition);
                //food.FixedUpdate();

            }

            if (mutualAttraction != 0)
            {
                foreach (FlockUnit shadow in ShadowBoids)
                {
                    Vector3 force = shadow.Attract(boidPosition);
                    Transform shadowPosition = shadow.transform;
                    boid.ApplyAttractionForce(force * organicSeekWeight * mutualAttraction, shadowPosition);
                }


                //}
                //boid.MoveUnit();

            }
        }

        foreach (FlockUnit shadow in ShadowBoids)
        {

            Transform shadowPosition = shadow.myTransform;
            foreach (FoodUnit food in Foods)
            {
                Vector3 force = food.Attract(shadowPosition);
                Transform foodPosition = food.transform;
                shadow.ApplyAttractionForce(force, foodPosition);
                //food.FixedUpdate();
            }
            //shadow.MoveUnit();

            if (mutualAttraction != 0)
            {
                foreach (FlockUnit boid in Boids)
                {
                    Vector3 force = boid.Attract(shadowPosition);
                    Transform boidPosition = shadow.transform;
                    shadow.ApplyAttractionForce(force * shadowSeekWeight * mutualAttraction, boidPosition);
                }


                //}
                //boid.MoveUnit();

            }

        }

        //if (worldYear == 90)
        //{setSeason("summer"); }
        //else if (worldYear == 180)
        //{ setSeason("fall"); }
        //else if (worldYear == 270)
        //{ setSeason("winter"); }


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
            var breed = "organic";
            var oscNumber = i;

            OSCMessage message_resetPositionX = new OSCMessage("/organic/position/x/" + i);
            OSCMessage message_resetPositionY = new OSCMessage("/organic/position/y/" + i);
            OSCMessage message_resetPositionZ = new OSCMessage("/organic/position/z/" + i);

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

        for (int i = 1; i <= flockSize; i++)
        {
            var breed = "shadow";
            var oscNumber = i;

            OSCMessage message_resetPositionX = new OSCMessage("/shadow/position/x/" + i);
            OSCMessage message_resetPositionY = new OSCMessage("/shadow/position/y/" + i);
            OSCMessage message_resetPositionZ = new OSCMessage("/shadow/position/z/" + i);

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



            transmitter.Send(message_resetPositionX);
            transmitter.Send(message_resetPositionY);
            transmitter.Send(message_resetPositionZ);

            transmitter.Send(midiNoteMessage);
            transmitter.Send(midiPlayMessage);
            transmitter.Send(healthMessage);

            transmitter.Send(velocityMessage);

            yield return null;
        }

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



    private IEnumerator FoodCounter()
    {
        while (true)
        {

            yield return new WaitForSeconds(10f);
            // foodSize *= 2;
        }
    }

    private IEnumerator CountWorldYear()
    {
        while (true)
        {

            yield return new WaitForSeconds(1f);
            worldYear += 1;

            //if (worldYear == 90)
            //{ setSeason("summer"); }
            //else if (worldYear == 180)
            //{ setSeason("fall"); }
            //else if (worldYear == 270)
            //{ setSeason("winter"); }

        }
    }




    protected void MessageReceived(OSCMessage message)
    {

        //OscFood.Add(message);
        var newFoodX = scale(message.Values[0].FloatValue, 0, 1, -20, 20);
        var newFoodY = scale(message.Values[1].FloatValue, 0, 1, -20, 20);
        var newFoodZ = scale(message.Values[2].FloatValue, 0, 1, -20, 20);
        Vector3 newFood = new Vector3(newFoodX, newFoodY, newFoodZ);
        OscFood.Add(newFood);
        if (OscFood.Count < foodSize)
        {
            var random = new System.Random();
            int index = random.Next(OscFood.Count);
            GenerateFood(OscFood[index]);
        }
        // Debug.Log(newFoodY);
        //GenerateFood(newFoodX, newFoodY);
        //if (message.ToFloat(out var value))
        //{
        //    // Any code...
        //    Debug.Log(value);
        //}
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

    public void GenerateAgent(List<FlockUnit> agentFlock, List<int> unitIndex, string agentBreed, Vector3 position, DNAboid parentDNA)
    {


        DNAboid childDNA = parentDNA.copy(); // make a copy of the DNA
        childDNA.mutate(0.2f);


        spawnPosition = transform.position + position;
        rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

        var agentPrefab = flockUnitPrefab;
        if (agentBreed == "shadow")
        {
            agentPrefab = shadowUnitPrefab;
        }

        var agentNumber = FindOpenIndex(unitIndex, flockSize);



        var newAgent = Instantiate(agentPrefab, spawnPosition, rotation);
        var newAgentScript = newAgent.GetComponent<FlockUnit>();

        newAgentScript.Death += OnBoidDeath;

        newAgent.AssignFlock(this);
        newAgent.breed = agentBreed;

        newAgent.NumberParticle(agentNumber);
        newAgent.setDNA(childDNA);

        //newAgent.InitializeSpeed(agentSpeed);
        //newAgent.setDNA(new DNAboid());
        //var agentSpeed = UnityEngine.Random.Range(_minSpeed, _maxSpeed);
        //var indexList = unitIndex;
        //agentFlock.Add(newAgent.GetComponent<FlockUnit>());


        if (agentBreed == "shadow")
        {
            ShadowBoidsIndex.Add(agentNumber);
            ShadowBoids.Add(newAgent.GetComponent<FlockUnit>());
            newAgent.transform.parent = shadowFolder.transform;

        }
        else if (agentBreed == "organic")
        {
            BoidsIndex.Add(agentNumber);
            Boids.Add(newAgent.GetComponent<FlockUnit>());
            newAgent.transform.parent = organicFolder.transform;
        }
    }


    private void GenerateUnits()
    {
        Debug.Log($"StarterDna: {JsonUtility.ToJson(starterDna)}");
        for (int i = 0; i < 2; i++)
        {
            GenerateAgent(Boids, BoidsIndex, "organic", zeroVector, starterDna);

        }

    }


    private void GenerateFood(Vector3 position)
    {

        var food = Instantiate(foodPrefab, position, Quaternion.identity);
        var foodScript = food.GetComponent<FoodUnit>();
        foodScript.Death += OnFoodDeath;
        Foods.Add(food.GetComponent<FoodUnit>());

    }

    public void setSeason(string newSeason)
    {
        switch (newSeason)
        {
            case "spring":
                season = "spring";
                //flockSize = 8;
                break;

            case "summer":
                season = "summer";
                // flockSize = 16;
                break;

            case "fall":
                season = "fall";
                break;

            case "winter":
                season = "winter";
                break;
        }



    }


    public void OnFoodDeath(object sender, FoodDeathEventArgs e)
    {

        Foods.Remove(e.FoodObject);


        var random = new System.Random();
        int index = random.Next(OscFood.Count);

        if (season == "spring" || season == "summer")
        {
            GenerateFood(OscFood[index]);
        }
    }

    public void OnBoidDeath(object sender, BoidDeathEventArgs e)
    {

        if (e.BreedObject == "organic")
        {
            Boids.Remove(e.BoidObject);
        }
        else if (e.BreedObject == "shadow")
        {
            ShadowBoids.Remove(e.BoidObject);
        }

    }

    //public void SendOSC(string instName, float x, float y, float z, int oscNumber)
    //{
    //    var NoteEncode = new List<float> {x, y, z};
    //    OSCHandler.Instance.SendMessageToClient(("Max" + (oscNumber + 1)) , instName, NoteEncode);
    //}
}

