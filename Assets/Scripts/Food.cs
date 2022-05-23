using System.Collections;
using System.Collections.Generic;
using extOSC;
using UnityEngine;

public class Food : MonoBehaviour
{

    [SerializeField] private Ecosystem ecosystem;

    [Header("Spawn Setup")]
    [SerializeField] private FoodUnit foodUnit;


    public List<FoodUnit> Foods = new List<FoodUnit>();
    public List<int> FoodsIndex;
    public List<Vector3> OscFood = new List<Vector3>();

    public OSCReceiver oscReceiver;

    public int foodSize = 5;
    public int foodBounds = 75;

    public float scale(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax)
    {

        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        return (NewValue);
    }

    private void Awake()
    {

        oscReceiver.Bind("/flucoma/xyz", MessageReceived);

        FoodsIndex = new List<int>() { 1 };
    }

    private void Start()
    {

    }


    protected void MessageReceived(OSCMessage message)
    {

        var newFoodX = scale(message.Values[0].FloatValue, 0, 1, -foodBounds, foodBounds);
        var newFoodY = scale(message.Values[1].FloatValue, 0, 1, -foodBounds, foodBounds);
        var newFoodZ = scale(message.Values[2].FloatValue, 0, 1, -foodBounds, foodBounds);
        Vector3 newFood = new Vector3(newFoodX, newFoodY, newFoodZ);
        OscFood.Add(newFood);
        if (OscFood.Count < foodSize)
        {
            var random = new System.Random();
            int index = random.Next(OscFood.Count);
            GenerateFood(OscFood[index]);

        }

    }


    private void GenerateFood(Vector3 position)
    {

        var food = Instantiate(foodUnit, position, Quaternion.identity);
        var foodScript = food.GetComponent<FoodUnit>();
        foodScript.Death += OnFoodDeath;
        Foods.Add(food.GetComponent<FoodUnit>());
        food.transform.parent = this.transform;

    }


    public void OnFoodDeath(object sender, FoodDeathEventArgs e)
    {

        Foods.Remove(e.FoodObject);


        if (ecosystem.season == "spring" || ecosystem.season == "summer")
        {
            var random = new System.Random();
            int index = random.Next(OscFood.Count);
            GenerateFood(OscFood[index]);
        }
    }




}
