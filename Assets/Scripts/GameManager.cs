using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public WormAgent agentA;
    public WormAgent agentB;

    public FoodSummonScript foodSummonScript;
    public Transform foodGroup;

    private bool episodeEnded = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    private bool environmentResetPending = false;

    void FixedUpdate()
    {
        if (foodGroup.childCount == 0 && episodeEnded.Equals(false))
        {
            SetWinAgent();
            episodeEnded = true;
            environmentResetPending = true;
        }
        TouchWallAgent();

        EnvironmentInitialize();
    }

    void Initialize()
    {
        float x = Random.Range((foodSummonScript.area_x - 0.4f) / 2, -(foodSummonScript.area_x - 0.4f) / 2);
        float y = Random.Range((foodSummonScript.area_y - 0.4f) / 2, -(foodSummonScript.area_y - 0.4f) / 2);
        agentA.transform.position = new Vector3(x, y, agentA.transform.position.z);

        //x = Random.Range((foodSummonScript.area_x - 0.4f) / 2, -(foodSummonScript.area_x - 0.4f) / 2);
        //y = Random.Range((foodSummonScript.area_y - 0.4f) / 2, -(foodSummonScript.area_y - 0.4f) / 2);
        //agentB.transform.position = new Vector3(x, y, agentB.transform.position.z);

        agentA.bodyCount = 0;
        //agentB.bodyCount = 0;
        WormManager.instance.DictSet();

        agentA.bodySegments.Clear();
        //agentB.bodySegments.Clear();

        ChildReset(foodGroup);
        ChildReset(agentA.bodyGroup);
        //ChildReset(agentB.bodyGroup);

        foodSummonScript.foodCount = Random.Range(2, 4);
        foodSummonScript.FoodSummon();

        episodeEnded = false;
    }

    void EnvironmentInitialize()
    {
        if (agentA.thisAgentBiginEpisode.Equals(true) && environmentResetPending)
        {
            Initialize();
            agentA.thisAgentBiginEpisode = false;
            environmentResetPending = false;
        }
        //if (agentA.thisAgentBiginEpisode.Equals(true) && agentB.thisAgentBiginEpisode.Equals(true) && environmentResetPending)
        //{
        //    Initialize();
        //    agentA.thisAgentBiginEpisode = false;
        //    agentB.thisAgentBiginEpisode = false;
        //    environmentResetPending = false;
        //}
    }

    void SetWinAgent()
    {
        agentA.AddReward(6f);
        agentA.EndEpisode();
        //if (WormManager.instance.WormID[agentA.wormID] > WormManager.instance.WormID[agentB.wormID])
        //{
        //    agentA.AddReward(6f);
        //}
        //else if(WormManager.instance.WormID[agentA.wormID] < WormManager.instance.WormID[agentB.wormID])
        //{
        //    agentB.AddReward(6f);
        //}
        //else
        //{
        //    agentA.AddReward(0f);
        //    agentB.AddReward(0f);
        //}
        //agentA.EndEpisode();
        //agentB.EndEpisode();
    }

    void ChildReset(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    void TouchWallAgent()
    {
        bool touched = false;

        if (agentA.touchWall.Equals(true))
        {
            agentA.AddReward(-6f);
            agentA.EndEpisode();
            //agentB.EndEpisode();

            touched = true;
        }
        //else if (agentB.touchWall.Equals(true))
        //{
        //    agentB.AddReward(-6f);
        //    agentA.EndEpisode();
        //    agentB.EndEpisode();

        //    touched = true;
        //}

        if (touched) environmentResetPending = true;
    }
}
