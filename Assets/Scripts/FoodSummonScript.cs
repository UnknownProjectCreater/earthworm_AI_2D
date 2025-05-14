using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSummonScript : MonoBehaviour
{
    public GameObject ground;
    public GameObject foodPrefab;
    public GameObject foodGroup;
    public int foodCount;

    public float area_x;
    public float area_y;

    // Start is called before the first frame update
    void Start()
    {
        FoodSummon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FoodSummon()
    {
        area_x = (ground.transform.localScale.x / 2) - 0.5f;
        area_y = (ground.transform.localScale.y / 2) - 0.5f;

        if (foodCount > this.transform.childCount)
        {
            for (int i = 0; i < foodCount; i++)
            {
                float rX = Random.Range(-area_x, area_x);
                float rY = Random.Range(-area_y, area_y);

                GameObject food = Instantiate(foodPrefab, new Vector3(rX, rY, this.transform.position.z), Quaternion.identity, foodGroup.transform);

                food.GetComponent<FoodScript>().point = 1;
                Renderer foodPrefabRend = food.GetComponent<Renderer>();
                Color randomColor = new Color(Random.value, Random.value, Random.value);
                foodPrefabRend.material.color = randomColor;
            }
        }
    }
}
