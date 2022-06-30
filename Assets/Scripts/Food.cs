using System.Collections.Generic;
using extOSC;
using UnityEngine;
using UnityEngine.Pool;

public class Food : MonoBehaviour
{

    [SerializeField] private Ecosystem ecosystem;

    [Header("Spawn Setup")]
    [SerializeField] private FoodUnit foodUnit;

    public Queue<FoodUnit> foodAvailable = new Queue<FoodUnit>();
    public List<FoodUnit> Foods = new();
    public ObjectPool<FoodUnit> foodPool;
    //public OSCReceiver oscReceiver;

    public int foodSize;
    //public int foodBounds = 75;
    //public int foodReceived = 50;
    public int foodCount = 0;
    public bool initialized = false;

    //public float scale(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax)
    //{

    //    float OldRange = (OldMax - OldMin);
    //    float NewRange = (NewMax - NewMin);
    //    float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

    //    return (NewValue);
    //}

    //private void Awake()
    //{
    //    oscReceiver.Bind("/flucoma/total", TotalFood);
    //    oscReceiver.Bind("/flucoma/xyz", SeedFood);
    //}


    //private void TotalFood(OSCMessage message)
    //{
    //    foodReceived = message.Values[0].IntValue;
    //}


    //protected void SeedFood(OSCMessage message)
    //{
    //    var newNumber = message.Values[0].IntValue;
    //    var newFoodX = scale(message.Values[1].FloatValue, 0f, 1f, -foodBounds, foodBounds);
    //    var newFoodY = scale(message.Values[2].FloatValue, 0f, 1f, -foodBounds, foodBounds);
    //    var newFoodZ = scale(message.Values[3].FloatValue, 0f, 1f, -foodBounds, foodBounds);
    //    Vector3 newPosition = new Vector3(newFoodX, newFoodY, newFoodZ);

    //    var newFood = InitializeFood(newPosition, newNumber);
    //   foodCount++;
    //    foodAvailable.Enqueue(newFood);

    //    if (foodCount < foodSize) { EnableFoods(1); } 
    //}


    //public FoodUnit InitializeFood(Vector3 position, int index)
    //{

    //    var food = Instantiate(foodUnit, position, Quaternion.identity, transform);
    //    food.NumberIndex(index);
    //    food.gameObject.SetActive(false);
    //    food.AssignFood(this);

    //    return food;

    //}


    public void InitializeFood(Vector3 position, int index)
    {

        var food = Instantiate(foodUnit, position, Quaternion.identity, transform);
        food.NumberIndex(index);
        food.gameObject.SetActive(false);
        food.AssignFood(this);
        foodCount++;
        foodAvailable.Enqueue(food);

        if (foodCount < foodSize) { EnableFoods(1); }


    }



    public void EnableFoods(int number)
    {

        for (int i = 0; i < number; i++)
        {
            FoodUnit newFood = foodAvailable.Dequeue();
            newFood.health = UnityEngine.Random.Range(50, 100);
            newFood.CalcFoodSize();
            newFood.food.SetActive(true);
            Foods.Add(newFood);
        }

    }


}
