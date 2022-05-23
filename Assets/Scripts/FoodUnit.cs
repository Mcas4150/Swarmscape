using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;

public class FoodUnit : MonoBehaviour
{

    [SerializeField] public float foodAttractWeight => GameManager.Instance.foodAttractWeight;
    [SerializeField] public float foodForceMagnitude => GameManager.Instance.foodForceMagnitude;
    [SerializeField] public float BoidMass => GameManager.Instance.boidMass;

    [SerializeField] private GameObject food;

    public Rigidbody body;

    private float G = 9.8f;
    public float health;
    private float radius;
    public float mass;

    public event EventHandler<FoodDeathEventArgs> Death;

    [Header("OSC Properties")]
    public OSCReceiver oscReceiver;


    void Start()
    {
        food = gameObject;

        body = food.GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.useGravity = false;
        body.isKinematic = true;
        health = 100;
        radius = health / 50f;

        food.transform.position = body.position;
        food.transform.localScale = 2 * radius * Vector3.one;
        body.mass = (4f / 3f) * Mathf.PI * radius * radius * radius;
        mass = body.mass;

        StartCoroutine(FoodSize());
    }

    //public void FixedUpdate()
    //{
    //    //radius = health/50f ;
    //    //body.mass = (4f / 3f) * Mathf.PI * radius * radius * radius;
    //    //food.transform.localScale =  radius * Vector3.one;
    //}


    private IEnumerator FoodSize()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            radius = health / 50f;
            body.mass = (4f / 3f) * Mathf.PI * radius * radius * radius;
            food.transform.localScale = radius * Vector3.one;

            //var healthRatio = health * 0.025f;
            //myTransform.localScale = new Vector3(healthRatio, healthRatio, healthRatio);

        }
    }


    public Vector3 Attract(Vector3 targetPosition)
    {
        Vector3 force = body.position - targetPosition;
        float distance = force.magnitude;

        Eaten(distance);

        distance = Mathf.Clamp(distance, 5f, 25f);
        force.Normalize();

        // float strength =  G * (body.mass * m.mass) / (distance * distance);
        //float strength =  G * (body.mass * 1.5f) / (distance * distance);
        float strength = G * (body.mass * BoidMass) / (distance * distance);
        force *= strength * foodAttractWeight;
        //force *= strength;

        force = Vector3.ClampMagnitude(force, foodForceMagnitude);
        return force;

    }

    public void Eaten(float distance)
    {

        if (distance < 2f)
        {
            health -= 5;
            if (health < 10)
            {
                // Reseed();
                Death?.Invoke(this, new FoodDeathEventArgs { FoodObject = gameObject.GetComponent<FoodUnit>() });
                Destroy(this, 0.5f);
                Destroy(gameObject, 0.5f);
            }
        }
    }
}


public class FoodDeathEventArgs : EventArgs
{
    public FoodUnit FoodObject { get; set; }
}
