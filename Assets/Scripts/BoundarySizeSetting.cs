using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundarySizeSetting : MonoBehaviour
{
    public GameObject ground;
    public int axisNumber;
    public int upSideNumber;

    BoxCollider2D collider;

    // Start is called before the first frame update
    void Start()
    {
        collider = this.gameObject.GetComponent<BoxCollider2D>();

        if(axisNumber == 0)
        {
            collider.size = new Vector2(1, ground.transform.localScale.x);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
