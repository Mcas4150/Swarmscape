using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC;

public class Foods : MonoBehaviour
{

    [SerializeField] public Food Food1;
    [SerializeField] public Food Food2;
    [SerializeField] public Food Food3;
    public List<Food> allFoods;

    public OSCReceiver oscReceiver;
    public int foodReceived = 50;
    public int foodBounds = 75;

    public float BorderBoundary = 100;


    public float scale(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax)
    {

        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        float NewValue = (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;

        return (NewValue);
    }

    private void Awake()
    {
        oscReceiver.Bind("/flucoma/total", TotalFood);
        oscReceiver.Bind("/flucoma/xyz", SeedFood);

        allFoods = new List<Food> { Food1, Food2, Food3 };
    }


    private void TotalFood(OSCMessage message)
    {
        foodReceived = message.Values[0].IntValue;
    }

    protected void SeedFood(OSCMessage message)
    {
        var newNumber = message.Values[0].IntValue;
        var newFoodX = scale(message.Values[1].FloatValue, 0f, 1f, -foodBounds, foodBounds);
        var newFoodY = scale(message.Values[2].FloatValue, 0f, 1f, -foodBounds, foodBounds);
        var newFoodZ = scale(message.Values[3].FloatValue, 0f, 1f, -foodBounds, foodBounds);
        Vector3 newPosition = new Vector3(newFoodX, newFoodY, newFoodZ);


        var FoodHeight = scale(message.Values[2].FloatValue, 0f, 1f, -BorderBoundary, BorderBoundary);


        if (FoodHeight >= BorderBoundary * 0.33f) { Food3.InitializeFood(newPosition, newNumber); }
        if (FoodHeight < BorderBoundary * 0.33f && FoodHeight >= BorderBoundary * -0.33f) { Food2.InitializeFood(newPosition, newNumber); }
        if (FoodHeight < BorderBoundary * -0.33f) { Food1.InitializeFood(newPosition, newNumber); }
    }
}
