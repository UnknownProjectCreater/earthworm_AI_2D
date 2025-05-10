using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class WormAgent : Agent
{
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

    private List<Vector2> path = new List<Vector2>();
    private List<Transform> bodySegments = new List<Transform>();
    public FoodSummonScript foodSummonScript;

    public int wormID;

    public WormManager wormManager;

    private void Start()
    {
        MaxFoodCount = foodSummonScript.foodCount;

        wormHead = this.transform;
        bodyGroup = transform.parent.GetChild(1);
    }

    private void Update()
    {
        BodyManage();

        SpeedUp();
    }

    void FixedUpdate()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // 경로 추가
        float pathResolution = segmentDistance * 0.3f;
        if (path.Count == 0 || Vector2.Distance(path[path.Count - 1], wormHead.position) > pathResolution)
        {
            path.Add(wormHead.position);
        }

        // 몸통 조각 이동
        MoveBodySegments();

        // 경로 길이 제한
        int maxPathLength = bodyCount * 10;
        if (path.Count > maxPathLength)
        {
            path.RemoveAt(0);
        }
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

    void SpeedUp()
    {
        if (Input.GetMouseButton(0) && wormManager.WormScore[wormID] >= 1)
        {
            Transform segment = null;
            if (bodySegments.Count - 1 == -1) segment = wormHead;
            else segment = bodySegments[bodySegments.Count - 1];

            StartCoroutine(wormManager.DecreaseScoreProcess(wormID, foodPrefab, segment, foodGroup));
            speed = 10;
        }
        else
        {
            speed = 5;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            Destroy(other.gameObject);
            wormManager.WormScore[wormID] += other.GetComponent<FoodScript>().point;
        }
        else if (other.CompareTag("Wall"))
        {

        }
        else if (other.CompareTag("WormHead"))
        {

        }
        else if (other.GetComponent<WormBodyScript>().wormBodyID != wormID && other.CompareTag("WormBody"))
        {
            Debug.Log("A");
        }
    }
}