using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class ACookingEnvController : MonoBehaviour
{
    [Header("Agents & Objects")]
    public List<AChief> agentList = new List<AChief>();
    public GameObject onion;
    public GameObject onion1;

    public GameObject dish;

    public GameObject dish1;
    public GameObject pot;
    public GameObject pot1;
    public GameObject counter;
      public GameObject counter1;
    public Renderer agentRenderer1;
    public Renderer agentRenderer2;

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

    public void NotifyAgentEpiodeBegin()
    {
        if(!resetDoneThisEpisode)
        {
            ResetEnv();
            resetDoneThisEpisode = true;
        }
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
        episodeEndedRecorded = false;

        foreach (var agent in agentList)
        {
            RestLocal(agent);
        }
    }

    public void PutInPot(AChief.HeldItem item)
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

        if (item == AChief.HeldItem.Onion && OnionCount < 3)
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
        else if (item == AChief.HeldItem.Dish && !PotHasDish)
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
        return PotHasOnion&&PotHasDish&&!PotHasSoupReady;
    }

    public void GenerateSoup(string name)
    {
        //M_agentGroup.AddGroupReward(2f);// this method will add 2 reward to both agents automatically
        foreach (var agent in agentList)
        {
            agent.AddReward(2f);
        }

        if (name == "Chief1")
        { agentRenderer1.material.color = Color.green; }

        else if (name == "Chief2")
        { agentRenderer2.material.color = Color.green; }

        PotHasDish = false;
        PotHasOnion = false;
        PotHasSoupReady = true;
        OnionCount = 0;
    }

    public bool IsItemInPot(AChief.HeldItem item)
    {
        if (item == AChief.HeldItem.Onion)
            return PotHasOnion;
        else if (item == AChief.HeldItem.Dish)
            return PotHasDish;
        return false;
    }

    public void ServeSuccess(string name)
    {
        //M_agentGroup.AddGroupReward(3f); // 上菜成功奖励

        foreach (var agent in agentList)
        {
            agent.AddReward(3f);
        }
        //M_agentGroup.AddGroupReward(3f);

        if (name == "Chief1")
        {
            agentRenderer1.material.color = Color.cyan;
        }
        else if (name == "Chief2")
        {
            agentRenderer2.material.color = Color.cyan;
        }

        isServerd = true;
        Debug.Log("🍽️ Served successfully!");

        RecordEpisodeEnd(true);
        M_agentGroup.EndGroupEpisode();
        resetDoneThisEpisode = false;
    }

    public void RestLocal(AChief agent)
    {
        if (agent.name == "Chief1")
        {
            agent.transform.localPosition = new Vector3(Random.Range(8.3f, 6.90f), 0.5f, Random.Range(3.27f, -0.03f));
        }
        else if (agent.name == "Chief2")
        {
            agent.transform.localPosition = new Vector3(Random.Range(0.06f, 2.66f), 0.5f, Random.Range(3.43f, 0.37f));
        }
    }
}