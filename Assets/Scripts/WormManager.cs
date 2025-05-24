using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormManager : MonoBehaviour
{
    public static WormManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    bool isProcessing;

    public List<int> WormID = new List<int>();

    public Dictionary<int, float> WormScore = new Dictionary<int, float>();

    // Start is called before the first frame update
    void Start()
    {
        DictSet();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var id in WormID)
        {
            if (WormScore.ContainsKey(id) && WormScore[id] < 0)
            {
                WormScore[id] = 0;
            }
        }
    }

    public void DictSet()
    {
        WormScore[WormID[0]] = 0;
        WormScore[WormID[1]] = 0;
        WormScore[WormID[2]] = 0;
        WormScore[WormID[3]] = 0;
    }

    public IEnumerator DecreaseScoreProcess(int ID, GameObject food, Transform foodSpawnPosition, Transform foodSpawnGroup)
    {
        if (isProcessing) yield break;
        isProcessing = true;

        //WormScore[ID] -= 0.5f;
        //GameObject foodpiece = Instantiate(food, foodSpawnPosition.position, Quaternion.identity, foodSpawnGroup);
        //foodpiece.GetComponent<FoodScript>().point = 0.5f;
        //foodpiece.tag = "FoodPiece";
        //Renderer foodprefabrend = foodpiece.GetComponent<Renderer>();
        //Color randomcolor = new Color(Random.value, Random.value, Random.value);
        //foodprefabrend.material.color = randomcolor;
        //foodpiece.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        yield return new WaitForSeconds(0.5f);
        isProcessing = false;
    }
}
