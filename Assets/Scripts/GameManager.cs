using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public WormAgent agentA;
    public WormAgent agentB;

    public FoodSummonScript foodSummonScript;
    public Transform foodGroup;

    private bool episodeEnded = false;

    void FixedUpdate()
    {
        // 벽 충돌 확인
        if (!episodeEnded)
        {
            if (agentA.touchWall) // || agentB.touchWall
            {
                HandleWallCollision();
            }
            else if (foodGroup.childCount == 0)
            {
                agentA.AddReward(6);
                HandleFoodDepleted();
            }
        }
    }

    void HandleWallCollision()
    {
        episodeEnded = true;

        if (agentA.touchWall)
        {
            agentA.AddReward(-6f);
        }

        //if (agentB.touchWall)
        //{
        //    agentB.AddReward(-6f);
        //}

        agentA.EndEpisode();
        //agentB.EndEpisode();

        ResetEnvironment();
    }

    void HandleFoodDepleted()
    {
        episodeEnded = true;

        //int scoreA = WormManager.instance.WormID[agentA.wormID];
        //int scoreB = WormManager.instance.WormID[agentB.wormID];

        //if (scoreA > scoreB)
        //{
        //    agentA.AddReward(12f); // 보상 중복 방지를 위해 6f + 6f 병합
        //}
        //else if (scoreB > scoreA)
        //{
        //    agentB.AddReward(12f);
        //}
        //else
        //{
        //    // 무승부: 보상 없음
        //}

        agentA.EndEpisode();
        //agentB.EndEpisode();

        ResetEnvironment();
    }

    void ResetEnvironment()
    {
        ResetAgent(agentA);
        //ResetAgent(agentB);
        ResetFood();

        episodeEnded = false;
    }

    void ResetAgent(WormAgent agent)
    {
        // 랜덤 위치
        float x = Random.Range(-(foodSummonScript.area_x - 0.4f) / 2, (foodSummonScript.area_x - 0.4f) / 2);
        float y = Random.Range(-(foodSummonScript.area_y - 0.4f) / 2, (foodSummonScript.area_y - 0.4f) / 2);
        agent.transform.position = new Vector3(x, y, agent.transform.position.z);

        // 방향 초기화
        Vector2 dir = Random.insideUnitCircle.normalized;
        agent.moveDirection = dir;

        // 상태 초기화
        agent.touchWall = false;
        agent.thisAgentBiginEpisode = false;

        // 몸통 제거
        ChildReset(agent.bodyGroup);
        agent.bodySegments.Clear();
        agent.bodyCount = 0;
    }

    void ResetFood()
    {
        // 기존 음식 제거
        ChildReset(foodGroup);

        // 스코어 초기화
        WormManager.instance.DictSet();

        // 새 음식 소환
        foodSummonScript.foodCount = Random.Range(3, 5);
        foodSummonScript.FoodSummon();
    }

    void ChildReset(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}
