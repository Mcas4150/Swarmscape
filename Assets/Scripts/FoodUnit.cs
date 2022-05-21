using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;

public class FoodUnit : MonoBehaviour
{


    public Vector3 location
    {
        get { return food.transform.position; }
        set { food.transform.position = value; }
    }

    public event EventHandler<FoodDeathEventArgs> Death;



    [SerializeField] public float foodForceMagnitude => GameManager.Instance.foodForceMagnitude;
    public float boidMass => GameManager.Instance.boidMass;

    [SerializeField] private GameObject food;
    public Rigidbody body;

    [Range(5, 50)]
    [SerializeField]
    public float G = 9.8f;
    private float radius;
    public float mass;

    [Header("OSC Properties")]
    public OSCReceiver oscReceiver;
    public float health;


    void Start()
    {
        food = gameObject;

        body = food.GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.useGravity = false; // Remember to ignore gravity!
        body.isKinematic = true;
        health = 100;
        radius = health / 50f;
        // Place our mover at the specified spawn position relative
        // to the bottom of the sphere
        food.transform.position = body.position;
        // The default diameter of the sphere is one unit
        // This means we have to multiple the radius by two when scaling it up
        food.transform.localScale = 2 * radius * Vector3.one;

        // We need to calculate the mass of the sphere.
        // Assuming the sphere is of even density throughout,
        // the mass will be proportional to the volume.
        body.mass = (4f / 3f) * Mathf.PI * radius * radius * radius;
        mass = body.mass;


        StartCoroutine(FoodSize());
        //oscReceiver = gameObject.AddComponent<OSCReceiver>();
        //oscReceiver.LocalPort = 7500;
        //oscReceiver.Bind("/flucoma/xy", MessageReceived);

        //StartCoroutine(OSCInputStream());
    }

    public void FixedUpdate()
    {
        //radius = health/50f ;
        //body.mass = (4f / 3f) * Mathf.PI * radius * radius * radius;
        //food.transform.localScale =  radius * Vector3.one;
    }


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

    private IEnumerator OSCInputStream()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.03f);
            //SendPosition(myTransform.position.x, myTransform.position.y, myTransform.position.z);



            // Send message

        }
    }

    //protected void MessageReceived(OSCMessage message)
    //{
    //    Debug.Log("running");
    //    // Any code...
    //    Debug.Log(message);
    //    var newFoodX = message.Values[0].FloatValue;
    //    var newFoodY = message.Values[1].FloatValue;

    //    Flock.GenerateFood(newFoodX, newFoodY);
    //    //if (message.ToFloat(out var value))
    //    //{
    //    //    // Any code...
    //    //    Debug.Log(value);
    //    //}
    //}

    public Vector3 Attract(Transform m)
    {
        Vector3 force = body.position - m.position;
        float distance = force.magnitude;

        Eaten(distance);
        // Remember we need to constrain the distance so that our circle doesn't spin out of control
        distance = Mathf.Clamp(distance, 5f, 25f);

        force.Normalize();
        // float strength =  G * (body.mass * m.mass) / (distance * distance);
        //float strength =  G * (body.mass * 1.5f) / (distance * distance);
        float strength = G * (body.mass * boidMass) / (distance * distance);
        force *= strength;
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
