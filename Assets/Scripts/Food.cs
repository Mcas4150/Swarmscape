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

    public Color foodColor;
    public int foodSize;
    public int foodCount = 0;
    public bool initialized = false;

    public void InitializeFood(Vector3 position, int index)
    {

        var food = Instantiate(foodUnit, position, Quaternion.identity, transform);
        food.NumberIndex(index);
        food.gameObject.SetActive(false);
        food.GetComponent<MeshRenderer>().material.color = foodColor;
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
