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

        health = 100;
        radius = health / 50f;


        food.transform.localScale = 2 * radius * Vector3.one;
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


    public Vector3 Attract(Vector3 targetPosition)
    {
        Vector3 force = food.transform.position - targetPosition;
        float distance = force.magnitude;

        Eaten(distance);

        distance = Mathf.Clamp(distance, 2f, 25f);
        force.Normalize();

        float strength = G * (mass * BoidMass) / (distance * distance);
        force *= strength * foodAttractWeight;
        //force *= strength;

        force = Vector3.ClampMagnitude(force, foodForceMagnitude);
        return force;

    }

    public void Eaten(float distance)
    {

        if (distance < 5f)
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
