using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;

public class FoodUnit : MonoBehaviour
{

    //[SerializeField] public float foodAttractWeight => GameManager.Instance.foodAttractWeight;
    [SerializeField] public float foodForceMagnitude => GameManager.Instance.foodForceMagnitude;
    //[SerializeField] public float BoidMass => GameManager.Instance.boidMass;

    [SerializeField] private GameObject food;

    private float G = 9.8f;
    public float health;
    private float radius;
    public float mass;

    public event EventHandler<FoodDeathEventArgs> Death;

    [Header("OSC Properties")]
    public OSCReceiver oscReceiver;


    public Vector3 position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    void Start()
    {
        food = gameObject;
        health = UnityEngine.Random.Range(50, 100);
        radius = health / 50f;
        food.transform.localScale = 4 * radius * Vector3.one;
        mass = (4f / 3f) * Mathf.PI * radius * radius * radius;
        StartCoroutine(FoodSize());
    }


    private IEnumerator FoodSize()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            radius = health / 50f;
            mass = (4f / 3f) * Mathf.PI * radius * radius * radius;
            food.transform.localScale = radius * Vector3.one;

            //var healthRatio = health * 0.025f;
            //myTransform.localScale = new Vector3(healthRatio, healthRatio, healthRatio);
        }
    }


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

        health -= 5;
        if (health < 10)
        {
            // Reseed();
            Death?.Invoke(this, new FoodDeathEventArgs { FoodObject = gameObject.GetComponent<FoodUnit>() });
            Destroy(this, 0.5f);
            Destroy(gameObject, 0.5f);
        }
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


public class FoodDeathEventArgs : EventArgs
{
    public FoodUnit FoodObject { get; set; }
}
