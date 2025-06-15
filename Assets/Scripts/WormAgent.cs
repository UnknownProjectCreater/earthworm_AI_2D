using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class WormAgent : Agent
{
    public Transform ground;
    public Transform bodyGroup;
    public Transform wormHead;
    public Transform foodGroup;
    public GameObject wormBodyPrefab;
    public GameObject foodPrefab;
    public int bodyCount = 5;
    public int MaxFoodCount;
    public float turnSpeed = 180f;
    public float speed = 5f;
    public float segmentDistance = 0.5f;
    public bool thisAgentBiginEpisode = false;
    public bool touchWall = false;
    Vector2 moveDirection;

    public List<Vector2> path = new List<Vector2>();
    public List<Transform> bodySegments = new List<Transform>();

    public int wormID;

    public WormManager wormManager;

    private void Start()
    {
        wormHead = this.transform;
        bodyGroup = transform.parent.GetChild(1);
    }

    private void Update()
    {
        BodyManage();
    }

    void FixedUpdate()
    {
        float pathResolution = segmentDistance * 0.3f;
        if (path.Count == 0 || Vector2.Distance(path[path.Count - 1], wormHead.position) > pathResolution)
        {
            path.Add(wormHead.position);
        }

        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        // 몸통 조각 이동
        MoveBodySegments();

        // 경로 길이 제한
        int maxPathLength = bodyCount * 10;
        if (path.Count > maxPathLength)
        {
            path.RemoveAt(0);
        }

        AddReward(-0.002f);
    }

    void MoveBodySegments()
    {
        for (int i = 0; i < bodySegments.Count; i++)
        {
            float distanceToFollow = segmentDistance * (i + 1);
            Vector2 targetPosition = GetPositionOnPath(distanceToFollow);

            float followSpeed = speed * 2f;
            bodySegments[i].position = Vector2.MoveTowards(
                bodySegments[i].position,
                targetPosition,
                followSpeed * Time.fixedDeltaTime
            );

            if (i > 0)
            {
                Vector2 direction = ((Vector2)bodySegments[i - 1].position - (Vector2)bodySegments[i].position).normalized;
                if (direction != Vector2.zero)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    bodySegments[i].rotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }
    }

    Vector2 GetPositionOnPath(float distance)
    {
        if (path.Count < 2) return wormHead.position;

        float currentDistance = 0f;
        for (int i = path.Count - 1; i > 0; i--)
        {
            float segmentLength = Vector2.Distance(path[i], path[i - 1]);
            currentDistance += segmentLength;

            if (currentDistance >= distance)
            {
                float overshoot = currentDistance - distance;

                if (segmentLength < 0.001f)
                    return path[i];   // 너무 가까우면 보간 없이 위치 반환

                float t = Mathf.Clamp01(overshoot / segmentLength);  // 안전하게 보간 비율 계산
                return Vector2.Lerp(path[i], path[i - 1], t);   // 부드럽게 위치 계산
            }
        }

        return bodySegments.Count > 0 ? bodySegments[bodySegments.Count - 1].position : wormHead.position;
    }

    void BodyCountSet()
    {
        bodyCount = (int)Mathf.Floor(wormManager.WormScore[wormID]);
    }

    void BodyManage()
    {
        BodyCountSet();

        if (bodyGroup.childCount < bodyCount)
        {
            for (int i = 0; i < bodyCount - bodyGroup.childCount; i++)
            {
                Vector3 spawnPosition = bodySegments.Count > 0
                ? bodySegments[bodySegments.Count - 1].position
                : wormHead.position;

                spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y, this.transform.position.z);

                GameObject segment = Instantiate(
                    wormBodyPrefab,
                    spawnPosition,
                    Quaternion.identity,
                    bodyGroup
                );
                segment.GetComponent<WormBodyScript>().wormBodyID = wormID;
                bodySegments.Add(segment.transform);
            }
        }

        else if (bodyGroup.childCount > bodyCount && bodySegments.Count > 0)
        {
            for (int i = 0; i < bodyGroup.childCount - bodyCount; i++)
            {
                Transform RemoveBody = bodyGroup.GetChild(bodyGroup.childCount - 1);
                int count = bodySegments.Count - 1;

                if (bodySegments.Count - 1 == -1) count = 0;

                bodySegments.RemoveAt(count);
                Destroy(RemoveBody.gameObject);
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        thisAgentBiginEpisode = true;
        touchWall = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            Destroy(other.gameObject);
            wormManager.WormScore[wormID] += other.GetComponent<FoodScript>().point;
            AddReward(2f);
        }
        else if (other.CompareTag("FoodPiece"))
        {
            AddReward(0.2f);
        }
        else if (other.CompareTag("Wall"))
        {
            AddReward(-6);
            touchWall = true;
        }
        else if (other.CompareTag("WormHead"))
        {

        }
        else if (other.GetComponent<WormBodyScript>().wormBodyID != wormID && other.CompareTag("WormBody"))
        {
            Debug.Log("A");
        }
    }

    float observationRadius = 15f;
    public int maxObjects = 10;

    public override void CollectObservations(VectorSensor sensor)
    {
        float halfwidth = ground.transform.localScale.x / 2;  // base scale 1 → 실제 크기 10
        float halfheight = ground.transform.localScale.y / 2;
        
        Vector3 pos = transform.position;

        float normx = (pos.x - ground.position.x) / halfwidth;
        float normy = (pos.y - ground.position.y) / halfheight;

        // 범위가 -1 ~ 1로 정규화됨
        sensor.AddObservation(new Vector2(normx, normy));

        float rad = transform.eulerAngles.z * Mathf.Deg2Rad;
        sensor.AddObservation(Mathf.Sin(rad));
        sensor.AddObservation(Mathf.Cos(rad));

        Collider2D[] nearObj = Physics2D.OverlapCircleAll(this.transform.position, observationRadius);

        int count = 0;

        float tagObserve = -1;

        Vector2 relativePos;

        GameObject nearestObj = null;
        float minDist = float.MaxValue;


        foreach (var obj in nearObj)
        {
            if (obj != null && obj.CompareTag("Food"))
            {
                float dist = Vector2.Distance(transform.position, obj.transform.position);

                if (minDist > dist)
                {
                    minDist = dist;
                    nearestObj = obj.gameObject;
                }
            }
        }
        if (nearestObj != null)
        {
            tagObserve = 1;
            relativePos = nearestObj.transform.position - transform.position;
            sensor.AddObservation(tagObserve);
            sensor.AddObservation(relativePos);
            RewardByDistance(nearestObj.gameObject, 2f, 0.03f);
            count++;
        }

        foreach (var obj in nearObj)
        {
            if (count >= maxObjects) break;

            if (obj != null && obj.CompareTag("Wall"))
            {
                tagObserve = 2;
                relativePos = obj.transform.position - transform.position;
                RewardByDistance(obj.gameObject, 2f, -0.03f);
            }

            else if (obj != null && obj.CompareTag("WormHead"))
            {
                tagObserve = 3;
                relativePos = obj.transform.position - transform.position;
            }

            else
            {
                continue;
            }

            sensor.AddObservation(tagObserve);
            sensor.AddObservation(relativePos);
            count++;
        }

        for (int i = count; i < maxObjects; i++)
        {
            tagObserve = -1;
            sensor.AddObservation(tagObserve);
            sensor.AddObservation(Vector2.zero);
        }
    }

    private Vector2 lastMoveDirection = Vector2.right;

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0]; // X축 이동 (-1 ~ 1)
        float moveY = actions.ContinuousActions[1]; // Y축 이동 (-1 ~ 1)

        Vector2 inputDir = new Vector2(moveX, moveY).normalized;

        if(inputDir.magnitude > 0.1f)
        {
            moveDirection = inputDir;
            lastMoveDirection = moveDirection;
        }
        else
        {
            moveDirection = lastMoveDirection;
        }

        int isSpeedUP = actions.DiscreteActions[0];
        if(isSpeedUP == 1 && wormManager.WormScore[wormID] >= 1)
        {
            Transform segment = null;
            if (bodySegments.Count == 0) segment = wormHead;
            else segment = bodySegments[bodySegments.Count - 1];

            StartCoroutine(wormManager.DecreaseScoreProcess(wormID, foodPrefab, segment, foodGroup));
            speed = 10;
        }
        else if(isSpeedUP == 0)
        {
            speed = 5;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Debug.Log($"Heuristic called for WormAgent: {gameObject.name}");
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal"); // A/D 또는 화살표 좌/우
        continuousActions[1] = Input.GetAxis("Vertical");   // W/S 또는 화살표 위/아래
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0; // 스페이스바로 속도 업
    }

    void RewardByDistance(GameObject target, float maxdetectdistance, float rewardconstant)
    {
        float currentdistance = Vector2.Distance(transform.position, target.transform.position);
        if (currentdistance < maxdetectdistance)
        {
            AddReward(rewardconstant * (1 - currentdistance / maxdetectdistance));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, observationRadius);
    }
}