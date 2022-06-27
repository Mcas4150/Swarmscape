

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

    [SerializeField] public float boundsDistance => GameManager.Instance.boundsDistance;
    [SerializeField] public float boundsWeight => GameManager.Instance.boundsWeight;

    [Header("Spawn Setup")]
    [SerializeField] public string breed;
    [SerializeField] public int startAmount;
    [SerializeField] public float spawnRadius = 1f;
    [SerializeField] private FlockUnit flockUnitPrefab;
    public Flock EnemyFlock;
    public Flock PredatorFlock;
    public Flock PreyFlock;
    public Food food;

    public Queue<FlockUnit> agentsAvailable = new Queue<FlockUnit>();

    public Color breedColor;

    public bool Carnivore;
    public bool Herbivore;
    public bool Reproduce;
    public bool Builder;
    public bool Living;
    public float maxAge;
    public float eatAge;

    public int tier;
    public bool extinct;
    public int speciesAge;
    public int extinctionAge;
    public int extinctionResetAge = 30;
    public int reproduceAge = 10;

    [Header("Eating")]
    public float huntStrength = 1f;
    public float fleeStrength = 1f;
    public float foodStrength = 1f;
    public float flowForce = 0.3f;

    public float foodAgility;
    public float huntAgility;
    public float fleeAgility;



    [Header("Speed Setup")]
    [Range(0, 30)]
    [SerializeField] public float _minSpeed;
    public float minSpeed { get { return _minSpeed; } }

    [Range(0, 100)]
    [SerializeField] public float _maxSpeed;
    public float maxSpeed { get { return _maxSpeed; } }

    [Header("Agents ")]
    public List<FlockUnit> Boids = new List<FlockUnit>();
    public List<int> BoidsIndex;


    [Header("OSC Properties")]
    public OSCTransmitter transmitter;


    Quaternion rotation;

    DNAboid starterDna;
    public Flowfield flowfield;


    private void Awake()
    {
        starterDna = new DNAboid();
        BoidsIndex = new List<int>() { 0 };

    }

    private void Start()
    {

        StartCoroutine(OSCReset());
        StopCoroutine(OSCReset());

        StartCoroutine(CheckFlock());

        InitializeAgents();

        for (int i = 0; i < startAmount; i++)
        {
            SpawnAgent();
        }


    }

    //************************************************************************************************************************************
    //Coroutines
    //************************************************************************************************************************************

    private IEnumerator OSCReset()
    {

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage lifeStateMessage = new("/" + this.breed + "/lifestate/" + i, OSCValue.Float(0));
            transmitter.Send(lifeStateMessage);
        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage message_resetPositionXYZ = new("/" + breed + "/position/" + i);
            message_resetPositionXYZ.AddValue(OSCValue.Float(0));
            message_resetPositionXYZ.AddValue(OSCValue.Float(0));
            message_resetPositionXYZ.AddValue(OSCValue.Float(0));
            transmitter.Send(message_resetPositionXYZ);
        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage velocityMessage = new("/" + breed + "/velocity/" + i, OSCValue.Float(0));
            transmitter.Send(velocityMessage);
        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage healthMessage = new("/" + breed + "/health/" + i, OSCValue.Float(0));
            transmitter.Send(healthMessage);
        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage midiNoteMessage = new("/" + breed + "/midi/note/" + i, OSCValue.Float(0));
            OSCMessage midiPlayMessage = new("/" + breed + "/midi/play/" + i, OSCValue.Float(0));

            transmitter.Send(midiNoteMessage);
            transmitter.Send(midiPlayMessage);

        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage ageMessage = new("/" + breed + "/age/" + i, OSCValue.Int(0));
            transmitter.Send(ageMessage);
        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage foodMealMessage = new("/" + breed + "/foodMeal/" + i, OSCValue.Int(-1));
            transmitter.Send(foodMealMessage);

        }


        yield return null;
    }

    private IEnumerator CheckFlock()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);


            if (Boids.Count == 0)
            {
                extinct = true;
                speciesAge = 0;
                extinctionAge += 1;
            }
            else
            {
                extinct = false;
                speciesAge += 1;
                extinctionAge = 0;
            }

            if (extinctionAge == extinctionResetAge)
            {
                for (int i = 0; i < startAmount; i++)
                {
                    SpawnAgent();
                }
            }

        }
    }

    //************************************************************************************************************************************
    //Initialization
    //************************************************************************************************************************************

    private void InitializeAgents()
    {
        //Debug.Log($"StarterDna: {JsonUtility.ToJson(starterDna)}");
        for (int i = 1; i <= flockSize; i++)
        {
            var halfBounds = boundsDistance;
            var randomPosition1 = UnityEngine.Random.Range(-halfBounds, halfBounds);
            var randomPosition3 = UnityEngine.Random.Range(-halfBounds, halfBounds);
            var randomPositionY = tier switch
            {
                3 => UnityEngine.Random.Range(boundsDistance * 0.33f, boundsDistance - 5),
                2 => UnityEngine.Random.Range(-boundsDistance * 0.33f, boundsDistance * 0.33f),
                1 => UnityEngine.Random.Range(-boundsDistance + 5, -boundsDistance * 0.33f),
                _ => UnityEngine.Random.Range(-boundsDistance + 5, boundsDistance - 5),
            };
            var randomVector = new Vector3(randomPosition1, randomPositionY, randomPosition3);
            var spawnPosition = randomVector + (UnityEngine.Random.insideUnitSphere.normalized * spawnRadius);

            InitializeAgent(this, breed, BoidsIndex, randomVector, starterDna);
        }

    }

    public void InitializeAgent(Flock agentFlock, string agentBreed, List<int> unitIndex, Vector3 position, DNAboid parentDNA)
    {

        DNAboid childDNA = parentDNA.copy();
        childDNA.mutate(0.2f);


        //var spawnPosition = position + (UnityEngine.Random.insideUnitSphere.normalized * spawnRadius);
        rotation = Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));

        var newAgent = Instantiate(flockUnitPrefab, position, rotation, transform);
        newAgent.GetComponent<MeshRenderer>().material.color = breedColor;

        newAgent.gameObject.SetActive(false);
        newAgent.AssignFlock(agentFlock);
        newAgent.breed = agentBreed;
        newAgent.setDNA(childDNA);

        var agentNumber = FindOpenIndex(unitIndex, flockSize);
        newAgent.NumberParticle(agentNumber);
        unitIndex.Add(agentNumber);
        agentsAvailable.Enqueue(newAgent);

        //OSCMessage lifeStateMessage = new("/" + agentBreed + "/lifestate/" + agentNumber, OSCValue.Float(1));
        //transmitter.Send(lifeStateMessage);
        //agentFlockList.Add(newAgent);

    }

    //************************************************************************************************************************************
    //Spawning
    //************************************************************************************************************************************

    public void SpawnAgent(Flock agentFlock, List<FlockUnit> agentFlockList, List<int> unitIndex, string agentBreed, Vector3 position, DNAboid parentDNA)
    {

        var spawnPosition = position + (UnityEngine.Random.insideUnitSphere.normalized * spawnRadius);
        rotation = Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));

        var newAgent = agentsAvailable.Dequeue();

        DNAboid childDNA = parentDNA.copy();
        childDNA.mutate(0.2f);
        newAgent.setDNA(childDNA);
        newAgent.AssignFlock(agentFlock);
        newAgent.breed = agentBreed;
        newAgent.health = UnityEngine.Random.Range(50, 100);

        newAgent.gameObject.transform.position = spawnPosition;
        newAgent.gameObject.transform.rotation = rotation;
        newAgent.gameObject.SetActive(true);
        agentFlockList.Add(newAgent);

        // birth message here
        //unitIndex.Add(agentNumber);
        //OSCMessage lifeStateMessage = new("/" + agentBreed + "/lifestate/" + agentNumber, OSCValue.Float(1));
        //transmitter.Send(lifeStateMessage);

    }

    public void SpawnAgent()
    {
        var newAgent = agentsAvailable.Dequeue();
        newAgent.gameObject.SetActive(true);
        Boids.Add(newAgent);
    }


    //************************************************************************************************************************************
    //Utilities
    //************************************************************************************************************************************

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

    void OnApplicationQuit()
    {


        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage lifeStateMessage = new("/" + this.breed + "/lifestate/" + i, OSCValue.Float(0));
            transmitter.Send(lifeStateMessage);
        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage message_resetPositionXYZ = new("/" + breed + "/position/" + i);
            message_resetPositionXYZ.AddValue(OSCValue.Float(0));
            message_resetPositionXYZ.AddValue(OSCValue.Float(0));
            message_resetPositionXYZ.AddValue(OSCValue.Float(0));
            transmitter.Send(message_resetPositionXYZ);
        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage velocityMessage = new("/" + breed + "/velocity/" + i, OSCValue.Float(0));
            transmitter.Send(velocityMessage);
        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage healthMessage = new("/" + breed + "/health/" + i, OSCValue.Float(0));
            transmitter.Send(healthMessage);
        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage midiNoteMessage = new("/" + breed + "/midi/note/" + i, OSCValue.Float(0));
            OSCMessage midiPlayMessage = new("/" + breed + "/midi/play/" + i, OSCValue.Float(0));


            transmitter.Send(midiNoteMessage);
            transmitter.Send(midiPlayMessage);

        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage ageMessage = new("/" + breed + "/age/" + i, OSCValue.Int(0));
            transmitter.Send(ageMessage);
        }

        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage foodMealMessage = new("/" + breed + "/foodMeal/" + i, OSCValue.Int(-1));
            transmitter.Send(foodMealMessage);

        }
    }

}

