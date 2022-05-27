

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
    [SerializeField] public float boundsDistance => GameManager.Instance.boundsDistance;
    [SerializeField] public float boundsWeight => GameManager.Instance.boundsWeight;
    [SerializeField] public float preyWeight => GameManager.Instance.preyWeight;
    [SerializeField] public float attackForceMagnitude => GameManager.Instance.attackForceMagnitude;


    [Header("Spawn Setup")]
    [SerializeField] public string breed;
    [SerializeField] public int startAmount;
    [SerializeField] public float spawnRadius = 1f;

    [SerializeField] private FlockUnit flockUnitPrefab;
    public Flock EnemyFlock;
    public Food food;

    public float seekWeight = 1;

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

    Vector3 zeroVector;
    Quaternion rotation;

    DNAboid starterDna;


    private void Awake()
    {
        starterDna = new DNAboid();
        BoidsIndex = new List<int>() { 0 };
        zeroVector = new Vector3(0, 0, 0);

    }

    private void Start()
    {
        _minSpeed = 6f;
        _maxSpeed = 100f;

        StartCoroutine(OSCReset());
        StopCoroutine(OSCReset());

        GenerateUnits();


    }

    private void Update()
    {

        foreach (FlockUnit boid in Boids)
        {

            //boid.Flock(Boids);
            Vector3 boidPosition = boid.myTransform.position;
            foreach (FoodUnit food in food.Foods)
            {
                //boid.Arrive(food.transform.position);
                //Vector3 force = food.Attract(boidPosition);

                //Vector3 force = boid.Seek(food.transform.position);
                //boid.ApplyAttractionForce(force);

                boid.Eater(food);
            }

            if (mutualAttraction != 0)
            {
                foreach (FlockUnit enemy in EnemyFlock.Boids)
                {
                    Vector3 force = enemy.Attract(boidPosition, attackForceMagnitude);
                    boid.ApplyAttractionForce(mutualAttraction * seekWeight * force);
                    //boid.ApplyForce(boid.Seek(enemy.transform.position)); 
                    //boid.Prey(enemy.transform.position);
                }
            }
        }
    }

    private IEnumerator OSCReset()
    {

        OSCMessage resetMessage = new OSCMessage("/play/", OSCValue.Float(1.0f));
        Debug.Log(resetMessage);
        transmitter.Send(resetMessage);

        for (int i = 1; i <= flockSize; i++)
        {
            var oscNumber = i;


            OSCMessage message_resetPositionXYZ = new("/" + breed + "/position/" + i);
            message_resetPositionXYZ.AddValue(OSCValue.Float(0));
            message_resetPositionXYZ.AddValue(OSCValue.Float(0));
            message_resetPositionXYZ.AddValue(OSCValue.Float(0));

            OSCMessage midiNoteMessage = new("/" + breed + "/midi/note/" + oscNumber, OSCValue.Float(0));
            OSCMessage midiPlayMessage = new("/" + breed + "/midi/play/" + oscNumber, OSCValue.Float(0));
            OSCMessage healthMessage = new("/" + breed + "/health/" + oscNumber, OSCValue.Float(0));

            OSCMessage velocityMessage = new("/" + breed + "/velocity/" + oscNumber, OSCValue.Float(0));


            //message_resetPositionX.AddValue(OSCValue.Float(0));
            ////message_resetPositionY.AddValue(OSCValue.Float(0));
            ////message_resetPositionZ.AddValue(OSCValue.Float(0));

            ////midiNoteMessage.AddValue(OSCValue.Float(0));
            ////midiPlayMessage.AddValue(OSCValue.Float(0));
            ////healthMessage.AddValue(OSCValue.Float(0));


            ////velocityMessage.AddValue(OSCValue.Float(0));


            transmitter.Send(message_resetPositionXYZ);

            transmitter.Send(midiNoteMessage);
            transmitter.Send(midiPlayMessage);
            transmitter.Send(healthMessage);

            transmitter.Send(velocityMessage);

            yield return null;
        }


    }


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

        DNAboid childDNA = parentDNA.copy();
        childDNA.mutate(0.2f);


        var spawnPosition = position + (UnityEngine.Random.insideUnitSphere.normalized * spawnRadius);
        rotation = Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));

        var agentNumber = FindOpenIndex(unitIndex, flockSize);


        var newAgent = Instantiate(flockUnitPrefab, spawnPosition, rotation, transform);
        var newAgentScript = newAgent.GetComponent<FlockUnit>();

        newAgentScript.Death += OnBoidDeath;

        newAgent.AssignFlock(agentFlock);
        newAgent.breed = agentBreed;

        newAgent.NumberParticle(agentNumber);
        newAgent.setDNA(childDNA);

        unitIndex.Add(agentNumber);
        agentFlockList.Add(newAgent.GetComponent<FlockUnit>());
        //newAgent.transform.parent = this.transform;
    }

    public void OnBoidDeath(object sender, BoidDeathEventArgs e)
    {
        Boids.Remove(e.BoidObject);
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

}

