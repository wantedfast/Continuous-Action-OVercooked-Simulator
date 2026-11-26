using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class SLEnvController : MonoBehaviour
{
    [Header("Agents & Objects")]
    public List<SLChief> agentList = new List<SLChief>();
    public GameObject onion;
    //public GameObject onion1;

    public GameObject dish;
    public GameObject pot;
    public GameObject counter;

    public Renderer agentRenderer1;

    [Header("State Flags")]
    // public bool PotNeedsOnion = true;
    // public bool PotNeedsDish = false;
    public bool PotHasSoupReady = false;

    public bool PotHasDish = false;
    public bool PotHasOnion = false;
    public bool RetriveSoup = false;

    public int OnionCount = 0;

    public bool isCooked = false;

    public bool isServerd = false;

    [Header("Step Settings")]
    public int MaxEnvSteps = 15000;
    private int stepCount = 0;

    [Header("Group Reward Manager")]
    public SimpleMultiAgentGroup M_agentGroup;

    [Header("Last Agent Record")]
    public string lastAgentToPut = "";

     private bool resetDoneThisEpisode = false;
    private bool episodeEndedRecorded = false;  

    void Start()
    {
        M_agentGroup = new SimpleMultiAgentGroup();

        foreach (var agent in agentList)
        {
            M_agentGroup.RegisterAgent(agent);
            agent.envController = this;
        }
    }

    void Awake()
    {
        MaxEnvSteps = 10000;
    }

    void FixedUpdate()
    {
        stepCount++;

        if (stepCount >= MaxEnvSteps)
        {
            Debug.Log("🔚 Reached max step without success");
            RecordEpisodeEnd(false);
            
            M_agentGroup.GroupEpisodeInterrupted();
            ResetEnv();
            resetDoneThisEpisode = false;
        }
    }

    public void NotifyAgentEpiodeBegin()
    {
        if(!resetDoneThisEpisode)
        {
            ResetEnv();
            resetDoneThisEpisode = true;
        }
    }

    public void ServeSuccess()
    {
        //M_agentGroup.AddGroupReward(3f); // 上菜成功奖励

        foreach (var agent in agentList)
        {
            agent.AddReward(3f);
        }

        agentRenderer1.material.color = Color.cyan;

        isServerd = true;
        Debug.Log("🍽️ Served successfully!");
        
        RecordEpisodeEnd(true);
        M_agentGroup.EndGroupEpisode();
        resetDoneThisEpisode = false;
    }

    public void ResetEnv()
    {
        stepCount = 0;

        OnionCount = 0;
        PotHasDish = false;
        PotHasOnion = false;

        PotHasSoupReady = false;

        RetriveSoup = false;

        isCooked = false;
        isServerd = false;
        episodeEndedRecorded = false;

        foreach (var agent in agentList)
        {
            RestLocal(agent);
        }
    }

    private void RecordEpisodeEnd(bool success)
    {
        if (episodeEndedRecorded) return;
        episodeEndedRecorded = true;

        Academy.Instance.StatsRecorder.Add
        (
            "Completion time",
            stepCount * Time.fixedDeltaTime,
            StatAggregationMethod.Average                     // 记录当前步数

        );

        Academy.Instance.StatsRecorder.Add
        (
            "Successful rate",
            success ? 1 : 0,
            StatAggregationMethod.Average                     // 记录当前步数
        );
    }
    
    public void RestLocal(SLChief agent)
    { 
        agent.transform.localPosition = new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-2f, 2f));
    }

    public void PutInPot(SLChief.HeldItem item)
    {

        if (PotHasDish && PotHasOnion)
        {
            Debug.Log("Pot already has soup ready, cannot added more!");
            return;
        }

        if (item == SLChief.HeldItem.Onion && OnionCount < 3)
        {
            
                OnionCount++;
                Debug.Log($"🧅 Onion added to pot. Total onions: {OnionCount}");

                if (OnionCount >= 3)
                {
                    PotHasOnion = true;
                    OnionCount = 3;
                    Debug.Log("✅ Pot now has enough onions!");
                }
            
        }
        else if (item == SLChief.HeldItem.Dish && !PotHasDish)
        {
            PotHasDish = true;
            Debug.Log("Pot has dish");
        }

        // if (PotHasDish && PotHasOnion)
        // {
        //     Debug.Log("Pot can cook");
        //     PotHasSoupReady = true;
        // }
    }

    public bool CheckSoupReady()
    {
        return PotHasOnion&&PotHasDish && !PotHasSoupReady;
    }

    public void GenerateSoup()
    {
        //M_agentGroup.AddGroupReward(2f);// this method will add 2 reward to both agents automatically
        foreach (var agent in agentList)
        {
            agent.AddReward(2f);
        }

        agentRenderer1.material.color = Color.green;

        PotHasDish = false;
        PotHasOnion = false;
        PotHasSoupReady = true;

        OnionCount = 0;
    }

    public bool IsItemInPot(SLChief.HeldItem item)
    {
        if (item == SLChief.HeldItem.Onion)
            return PotHasOnion;
        else if (item == SLChief.HeldItem.Dish)
            return PotHasDish;
        return false;
    }
}


// using System.Collections.Generic;
// using UnityEngine;
// using Unity.MLAgents;
// using Unity.MLAgents.Sensors;
// using Unity.VisualScripting;

// public class SLCookingEnvController : MonoBehaviour
// {
//     [Header("Agents & Objects")]
//     public List<SLChief> agentList = new List<SLChief>();
//     public GameObject onion;
//     public GameObject dish;
//     public GameObject pot;
//     public GameObject counter;

//     public Renderer agentRenderer;


//     [Header("State Flags")]
//     public bool hasOnion = false;
//     public bool hasDish = false;
//     public bool PotHasOnion = false;
//     public bool PotHasDish = false;
//     public bool PotHasSoupReady = false;


//     public bool isCooked = false;

//     public bool isServerd = false;

//     [Header("Step Settings")]
//     public int MaxEnvSteps = 15000;
//     private int stepCount = 0;

//     [Header("Group Reward Manager")]
//     public SimpleMultiAgentGroup M_agentGroup;

//     [Header("Last Agent Record")]
//     public string lastAgentToPut = "";

//     void Start()
//     {
//         M_agentGroup = new SimpleMultiAgentGroup();

//         foreach (var agent in agentList)
//         {
//             M_agentGroup.RegisterAgent(agent);
//         }

//         ResetEnv();
//     }

//     void Awake()
//     {
//         MaxEnvSteps = 10000;
//     }

//     void FixedUpdate()
//     {
//         stepCount++;

//         if (stepCount >= MaxEnvSteps)
//         {
//             Debug.Log("🔚 Reached max step without success");
//             M_agentGroup.GroupEpisodeInterrupted();
//             ResetEnv();
//         }
//     }

//     public void PutInPot(SLChief.HeldItem item)
//     {
//         if (item == SLChief.HeldItem.Onion && !PotHasOnion)
//         {
//             PotHasOnion = true;
//         }
//         else if (item == SLChief.HeldItem.Dish && !PotHasDish)
//         {
//             PotHasDish = true;
//         }

//         // if (PotHasOnion && PotHasDish && !isCooked)
//         // {
//         //     isCooked = true;
//         //     M_agentGroup.AddGroupReward(2f);

//         //     agentRenderer.material.color = Color.green; // Change color to indicate cooking success
//         //     return true;
//         // }

//         // return false;
//     }

//     public bool CheckSoupReady()
//     {
//         return PotHasOnion && PotHasDish && !PotHasSoupReady;
//     }

//     public void GenerateSoup()
//     {
//         M_agentGroup.AddGroupReward(2f);
//         agentRenderer.material.color = Color.green; // Change color to indicate cooking success

//         PotHasDish = false;
//         PotHasOnion = false;
//         PotHasSoupReady = true;
//     }

//     public bool IsItemInPot(SLChief.HeldItem item)
//     {
//         if (item == SLChief.HeldItem.Onion)
//             return PotHasOnion;
//         else if (item == SLChief.HeldItem.Dish)
//             return PotHasDish;
//         return false;
//     }

//     public void ServeSuccess()
//     {
//         M_agentGroup.AddGroupReward(3f); // 上菜成功奖励
//         isServerd = true;
//         Debug.Log("🍽️ Served successfully!");
//         Academy.Instance.StatsRecorder.Add
//         (
//             "ServeSuccessCount",   // 统计项名字
//             1,                     // 每次+1
//             StatAggregationMethod.Sum // 用Sum累加
//         );
//         // M_agentGroup.EndGroupEpisode();
//         // ResetEnv();
//     }

//     public void WallHitted()
//     {
//         // 只有锅完全空时才惩罚撞墙
//         if (!PotHasOnion && !PotHasDish)
//         {
//             Debug.Log("💥撞墙惩罚，锅空");
//             M_agentGroup.AddGroupReward(-1f);
//             M_agentGroup.EndGroupEpisode();
//             ResetEnv();
//         }
//         else
//         {
//             Debug.Log("🚧 撞墙但锅已有物品，不惩罚");
//         }
//     }

//     public void ResetEnv()
//     {
//         stepCount = 0;
//         hasOnion = false;
//         hasDish = false;

//         PotHasOnion = false;
//         PotHasDish = false;
//         PotHasSoupReady = false;
        
//         isCooked = false;
//         isServerd = false;
  
//         foreach (var agent in agentList)
//         {
//             agent.transform.localPosition = new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-2f, 2f));
//         }
//     }
// }