using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private FoodUnit foodUnitPrefab;
    //[SerializeField] private int foodSize;
    //[SerializeField] private Vector3 spawnBounds;

    public int foodSize = 5;
    private Vector3 spawnBounds = new Vector3(10, 10, 10);

    public FoodUnit[] allFoodUnits { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        GenerateFoodUnits();
        StartCoroutine(FoodCounter());
    }

    // Update is called once per frame
    void Update()
    {

    }


    private IEnumerator FoodCounter()
    {
        while (true)
        {

            yield return new WaitForSeconds(10f);
            // foodSize *= 2;
        }
    }

    private void GenerateFoodUnits()
    {
        allFoodUnits = new FoodUnit[foodSize];
        for (int i = 0; i < foodSize; i++)
        {
            var randomVector = Random.insideUnitSphere;
            randomVector = new Vector3(randomVector.x * spawnBounds.x, randomVector.y * spawnBounds.y, randomVector.z * spawnBounds.z);
            var spawnPosition = transform.position + randomVector;
            var rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            allFoodUnits[i] = Instantiate(foodUnitPrefab, spawnPosition, rotation);


        }
    }
}
