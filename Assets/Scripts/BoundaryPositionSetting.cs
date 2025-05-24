using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryPositionSetting : MonoBehaviour
{
    public Transform ground;
    public Transform topBoundary;
    public Transform bottomBoundary;
    public Transform leftBoundary;
    public Transform rightBoundary;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 center = ground.position;
        Vector3 size = ground.GetComponent<Renderer>().bounds.size;

        // Y축이 위쪽, X축이 좌우 방향
        topBoundary.position = new Vector3(center.x, center.y + Mathf.Round(size.y / 2f), center.z);
        bottomBoundary.position = new Vector3(center.x, center.y - Mathf.Round(size.y / 2f), center.z);
        leftBoundary.position = new Vector3(center.x - Mathf.Round(size.x / 2f), center.y, center.z);
        rightBoundary.position = new Vector3(center.x + Mathf.Round(size.x / 2f), center.y, center.z);

        topBoundary.localScale = new Vector2(ground.localScale.x / 2f, topBoundary.localScale.y);
        bottomBoundary.localScale = new Vector2(ground.localScale.x / 2f, bottomBoundary.localScale.y);
        leftBoundary.localScale = new Vector2(leftBoundary.localScale.x, ground.localScale.y / 2f);
        rightBoundary.localScale = new Vector2(rightBoundary.localScale.x, ground.localScale.y / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
