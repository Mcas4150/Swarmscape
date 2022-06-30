using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using extOSC;

public class FoodUnit : MonoBehaviour
{



    [SerializeField] public GameObject food;
    //[SerializeField] public ObjectPool<FoodUnit> myPool;

    public Food assignedFood;
    public float health;
    private float radius;
    public float mass;
    public Boolean meal = false;

    //public event EventHandler<FoodDeathEventArgs> Death;

    [Header("OSC Properties")]
    public OSCReceiver oscReceiver;
    public int oscIndex;

    private Vector3 randomVector;



    public Vector3 position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    private void Awake()
    {
        var random = UnityEngine.Random.Range(-1f, 1f);
        randomVector = new Vector3(random, random, random);
    }

    void Start()
    {
        food = gameObject;
        health = UnityEngine.Random.Range(50, 100);
        CalcFoodSize();
        //radius = health / 50f;
        //food.transform.localScale = 2 * radius * Vector3.one;
        //mass = (4f / 3f) * Mathf.PI * radius * radius * radius;
        StartCoroutine(FoodSize());
    }

    private void Update()
    {

        transform.Rotate(Time.deltaTime * (randomVector * 30f));
        //transform.localRotation = Quaternion.LookRotation(totalVelocity, Vector3.up);
    }

    public void NumberIndex(int index)
    {
        oscIndex = index;
    }

    //public void SetPool(ObjectPool<FoodUnit> pool) => myPool = pool;


    public void AssignFood(Food food)
    {
        assignedFood = food;
    }


    private IEnumerator FoodSize()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            CalcFoodSize();
        }
    }

    public void CalcFoodSize()
    {
        radius = health / 50f;
        mass = (4f / 3f) * Mathf.PI * radius * radius * radius;
        food.transform.localScale = 2 * radius * Vector3.one;
    }


    //public void Calc

    //public Vector3 Attract(Vector3 targetPosition)
    //{
    //    Vector3 force = transform.position - targetPosition;
    //    float distance = force.magnitude;
    //    //Eaten(distance);
    //    distance = Mathf.Clamp(distance, 2f, 25f);
    //    //force.Normalize();
    //    force = CustomNormalize(force);

    //    float strength = G * (mass * BoidMass) / (distance * distance);
    //    force *= strength * foodAttractWeight;
    //    //force *= strength;
    //    force = Vector3.ClampMagnitude(force, foodForceMagnitude);
    //    return force;
    //}



    public void Eaten()
    {
        //include foodsize only when eaten?
        food.SetActive(false);
        assignedFood.Foods.Remove(this);
        assignedFood.foodAvailable.Enqueue(this);


        //health -= 5;
        //if (health < 10)
        //{
        //    // Reseed();
        //    //Death?.Invoke(this, new FoodDeathEventArgs { FoodObject = gameObject.GetComponent<FoodUnit>() });
        //    //Destroy(this, 0.5f);
        //    //Destroy(gameObject, 0.5f);

        //    //myPool.Release(this);
        //    food.SetActive(false);
        //    assignedFood.Foods.Remove(this);
        //    assignedFood.foodAvailable.Enqueue(this);
        //}
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


//public class FoodDeathEventArgs : EventArgs
//{
//    public FoodUnit FoodObject { get; set; }
//}
