

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
    //[SerializeField] public float boundsWeight => GameManager.Instance.boundsWeight;

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

    //public List<FlockUnit> queueList;

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
        //queueList = agentsAvailable.ToList();

    }


    public void InitializeFlock()
    {

        StartCoroutine(CheckFlock());
    }

    void OnApplicationQuit()
    {
        OSC_Init();
    }
    //************************************************************************************************************************************
    //Coroutines
    //************************************************************************************************************************************


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
                BirthAgents();
            }

        }
    }

    //************************************************************************************************************************************
    //OSC
    //************************************************************************************************************************************


    public void OSC_Init()
    {


        for (int i = 1; i <= flockSize; i++)
        {
            OSCMessage lifeStateMessage = new("/" + breed + "/lifestate/" + i, OSCValue.Float(0));
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


    //************************************************************************************************************************************
    //Initialization
    //************************************************************************************************************************************

    public void InitializeAgents()
    {
        //Debug.Log($"StarterDna: {JsonUtility.ToJson(starterDna)}");
        for (int i = 1; i <= flockSize; i++)
        {

            var randomVector = getRandomPositionTiered(boundsDistance);

            InitializeAgent(this, breed, i, randomVector, starterDna);
        }

    }


    public void InitializeAgent(Flock agentFlock, string agentBreed, int unitIndex, Vector3 position, DNAboid parentDNA)
    {

        var newAgent = Instantiate(flockUnitPrefab, position, getRandomRotation(), transform);


        newAgent.gameObject.SetActive(false);

        newAgent.AssignStats(agentFlock, agentBreed, parentDNA);

        newAgent.AssignID(unitIndex, breedColor);

        agentsAvailable.Enqueue(newAgent);

    }


    //************************************************************************************************************************************
    //Spawning
    //************************************************************************************************************************************

    public void SpawnAgent(Flock agentFlock, List<FlockUnit> agentFlockList, List<int> unitIndex, string agentBreed, Vector3 parentPosition, DNAboid parentDNA)
    {

        var newAgent = agentsAvailable.Dequeue();

        newAgent.AssignPosition(getSpawnPosition(parentPosition), getRandomRotation());

        newAgent.AssignStats(agentFlock, agentBreed, parentDNA);

        BirthAgent(newAgent);
        agentFlockList.Add(newAgent);


    }

    public void BirthAgents()
    {

        for (int i = 0; i < startAmount; i++)
        {
            var newAgent = agentsAvailable.Dequeue();
            BirthAgent(newAgent);
            Boids.Add(newAgent);
        }

    }

    public void BirthAgent(FlockUnit newAgent)
    {

        newAgent.InitializeBoid();
        newAgent.OSC_Birth();
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


    public Quaternion getRandomRotation()
    {
        return Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
    }

    public Vector3 getSpawnPosition(Vector3 parentPosition)
    {
        return parentPosition + (UnityEngine.Random.insideUnitSphere.normalized * spawnRadius);
    }


    public Vector3 getRandomPositionTiered(float boundsDistance)
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
        return new Vector3(randomPosition1, randomPositionY, randomPosition3);
    }

}

