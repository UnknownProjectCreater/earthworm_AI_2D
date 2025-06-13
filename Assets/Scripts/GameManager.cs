using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public WormAgent agentA;
    public WormAgent agentB;

    public FoodSummonScript foodSummonScript;
    public Transform foodGroup;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        EnvironmentInitialize();
    }

    void Initialize()
    {
        float x = Random.Range((foodSummonScript.area_x - 0.4f) / 2, -(foodSummonScript.area_x - 0.4f) / 2);
        float y = Random.Range((foodSummonScript.area_y - 0.4f) / 2, -(foodSummonScript.area_y - 0.4f) / 2);
        agentA.transform.position = new Vector3(x, y, agentA.transform.position.z);

        x = Random.Range((foodSummonScript.area_x - 0.4f) / 2, -(foodSummonScript.area_x - 0.4f) / 2);
        y = Random.Range((foodSummonScript.area_y - 0.4f) / 2, -(foodSummonScript.area_y - 0.4f) / 2);
        agentB.transform.position = new Vector3(x, y, agentB.transform.position.z);

        agentA.bodyCount = 0;
        agentB.bodyCount = 0;
        WormManager.instance.DictSet();

        agentA.bodySegments.Clear();
        agentB.bodySegments.Clear();

        for (int i = 0; i < foodGroup.childCount; i++)
        {
            Destroy(foodGroup.GetChild(i).gameObject);
        }

        for (int i = 0; i < agentA.bodyGroup.childCount; i++)
        {
            Destroy(agentA.bodyGroup.GetChild(i).gameObject);
        }
        for (int i = 0; i < agentB.bodyGroup.childCount; i++)
        {
            Destroy(agentB.bodyGroup.GetChild(i).gameObject);
        }

        foodSummonScript.GetComponent<FoodSummonScript>().foodCount = Random.Range(4, 5);
        foodSummonScript.FoodSummon();
    }

    void EnvironmentInitialize()
    {
        if (agentA.thisAgentBiginEpisode.Equals(true) && agentB.thisAgentBiginEpisode.Equals(true))
        {
            Initialize();
            agentA.thisAgentBiginEpisode = false;
            agentB.thisAgentBiginEpisode = false;
        }
    }
}
