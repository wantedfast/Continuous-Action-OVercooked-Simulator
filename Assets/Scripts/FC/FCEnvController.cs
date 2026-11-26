using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class FCCookingEnvController : MonoBehaviour
{
    [Header("Agents & Objects")]
    public List<FCChief> agentList = new List<FCChief>();
    public GameObject onion;
    public GameObject onion1;

    public GameObject dish;
    public GameObject pot;
    public GameObject pot1;
    public GameObject counter;

    public GameObject table;

    public GameObject c1;
    public GameObject c2;

    public GameObject c3;

    public GameObject c4;

    public struct Circuit
    {
        public bool hasItem;
        public FCChief.HeldItem heldItem;
    }

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
            M_agentGroup.GroupEpisodeInterrupted();
            ResetEnv();
        }
    }

    public void PutInPot(FCChief.HeldItem item)
    {

        if (PotHasDish && PotHasOnion)
        {
            Debug.Log("Pot already has soup ready, cannot added more!");
            return;
        }

        // if (OnionCount >= 3)
        // {
        // OnionCount = 3; // clamp
        // PotHasSoupReady = true;
        // //PotNeedsOnion = false;
        // }

        // Debug.Log($"🧅 Onion added to pot. Total onions in pot: {OnionCount}");

        if (item == FCChief.HeldItem.Onion && OnionCount < 1)
        {
            
                OnionCount++;
                Debug.Log($"🧅 Onion added to pot. Total onions: {OnionCount}");

                if (OnionCount >= 1)
                {
                    PotHasOnion = true;
                    OnionCount = 1;
                    Debug.Log("✅ Pot now has enough onions!");
                }
            
        }
        else if (item == FCChief.HeldItem.Dish && !PotHasDish)
        {
            PotHasDish = true;
            Debug.Log("Pot has dish");
        }

        if (PotHasDish && PotHasOnion)
        {
            Debug.Log("Pot can cook");
            PotHasSoupReady = true;
        }
    }

    public bool CheckSoupReady()
    {
        return PotHasSoupReady;
    }

    public void GenerateSoup()
    {
        //M_agentGroup.AddGroupReward(2f);// this method will add 2 reward to both agents automatically
        foreach (var agent in agentList)
        {
            agent.AddReward(2f);
        }

        agentRenderer1.material.color = Color.green;
    }

    // public bool IsItemInPot(FCChief.HeldItem item)
    // {
    //     if (item == FCChief.HeldItem.Onion)
    //         return PotHasOnion;
    //     else if (item == FCChief.HeldItem.Dish)
    //         return PotHasDish;
    //     return false;
    // }

    public void ServeSuccess()
    {
        //M_agentGroup.AddGroupReward(3f); // 上菜成功奖励

        foreach (var agent in agentList)
        {
            agent.AddReward(3f);
        }
        //M_agentGroup.AddGroupReward(3f);
        agentRenderer1.material.color = Color.cyan;

        isServerd = true;
        Debug.Log("🍽️ Served successfully!");
        Academy.Instance.StatsRecorder.Add
        (
            "ServeSuccessCount",   // 统计项名字
            1,                     // 每次+1
            StatAggregationMethod.Sum // 用Sum累加
        );
        // M_agentGroup.EndGroupEpisode();
        // ResetEnv();
    }

    public void ResetEnv()
    {
        stepCount = 0;
        // PotNeedsOnion = true;
        // PotNeedsDish = false;

        OnionCount = 0;
        PotHasDish = false;
        PotHasOnion = false;

        PotHasSoupReady = false;

        RetriveSoup = false;

        isCooked = false;
        isServerd = false;

        foreach (var agent in agentList)
        {
            RestLocal(agent);
        }
    }

    public void RestLocal(FCChief agent)
    { 
         if (agent.name == "Chief1")
            {
                agent.transform.localPosition = new Vector3(3.33f, 0.5f, Random.Range(3.42f, -0.15f));
            }
            else if (agent.name == "Chief2")
            {
                agent.transform.localPosition = new Vector3(-0.76f, 0.5f, Random.Range(3.28f, -0.33f));
            }
    }
}