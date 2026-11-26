using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using NUnit;
using UnityEngine.Rendering;

public class SLChief : Agent
{
    public enum HeldItem
    {
        None,
        Onion,
        Dish,
        Soup
    }

    public HeldItem heldItem = HeldItem.None;
    public SLEnvController envController;
    public Transform Plane;

    public Circuit[] circuit;

    private float bounaryX;
    private float bounaryZ;

    private Rigidbody rb;
   //public Renderer agentRenderer;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        ResetAgent();
        envController.NotifyAgentEpiodeBegin();
    }

    void Start()
    {
        bounaryX = Plane.localScale.x * 5f;
        bounaryZ = Plane.localScale.z * 5f;
    }

    void FixedUpdate()
    {
        Vector3 localPos = transform.localPosition - Plane.localPosition;
        
        if (Mathf.Abs(localPos.x) > bounaryX || Mathf.Abs(localPos.z) > bounaryZ || transform.localPosition.y < -2f) // out of bounds
        {
            transform.localPosition = new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-2f, 2f));
            AddReward(-0.005f);
        }
    }
    
    // state coverage for continuous state space
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);

        sensor.AddObservation(envController.onion.transform.position-transform.localPosition);
        
        sensor.AddObservation(envController.dish.transform.position-transform.localPosition);

        sensor.AddObservation(envController.pot.transform.position-transform.localPosition);
    
        sensor.AddObservation(envController.counter.transform.position-transform.localPosition);

        sensor.AddObservation((float)heldItem);

        sensor.AddObservation(envController.PotHasOnion ? 1f : 0f);
        sensor.AddObservation(envController.PotHasDish ? 1f : 0f);
        sensor.AddObservation(envController.PotHasSoupReady ? 1f : 0f);
        
        sensor.AddObservation(envController.isServerd? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        DiscreteMovementWithAngle(actions.DiscreteActions);
        //ContinuousMovement(actions);
        AddReward(-1f / envController.MaxEnvSteps);
    }

    // continuous action movement
    public void ContinuousMovement(ActionBuffers actions)
    {
        float moveX = 2f * Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = 2f * Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        Vector3 move = new Vector3(moveX, 0f, moveZ) * 4f;
        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
    }

    // discrete action movement
    public void DiscreteMovement(ActionSegment<int> act)
    {
        // var dirToGo = Vector3.zero;
        // var rotateDir = Vector3.zero;

        // var action = act[0];
        // switch (action)
        // {
        //     case 1:
        //         dirToGo = transform.forward * 1f;
        //         break;
        //     case 2:
        //         dirToGo = transform.forward * -1f;
        //         break;
        //     case 3:
        //         rotateDir = transform.up * 1f;
        //         break;
        //     case 4:
        //         rotateDir = transform.up * -1f;
        //         break;
        // }
        // transform.Rotate(rotateDir, Time.deltaTime * 200f);
        // rb.AddForce(dirToGo * 2f, ForceMode.VelocityChange);

        // Stable grid movement for training

        Vector3 move = Vector3.zero;

        // Discrete movement mapping:
        // 0 = stay, 1 = up, 2 = down, 3 = right, 4 = left
        int action = act[0];

        switch (action)
        {
            case 0: // Up (forward)
                move = new Vector3(0f, 0f, 1f);
                break;

            case 1: // Down (backward)
                move = new Vector3(0f, 0f, -1f);
                break;

            case 2: // Right
                move = new Vector3(1f, 0f, 0f);
                break;

            case 3: // Left
                move = new Vector3(-1f, 0f, 0f);
                break;
        }

        // keep same speed logic as your continuous version
        move *= 4f;

        // Move using physics-based positioning
        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);

    }
    
    public void DiscreteMovementWithAngle(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        rb.AddForce(dirToGo * 2f, ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
    // === CHIEF1: pick up raw items ===
    if (collision.collider.CompareTag("Onion") && heldItem == HeldItem.None&&!envController.PotHasOnion)
    {
        heldItem = HeldItem.Onion;
        //AddReward(1.0f);
        return;
    }

    if (collision.collider.CompareTag("Dish") && heldItem == HeldItem.None &&!envController.PotHasDish)
    {
        heldItem = HeldItem.Dish;
        //AddReward(1.0f);
        return;
    }
  
    if (collision.collider.CompareTag("Pot"))
    {
            // put onion into pot
            if (heldItem == HeldItem.Onion || heldItem == HeldItem.Dish)
            {
                if (!envController.IsItemInPot(heldItem))
                {
                    envController.PutInPot(heldItem);
                    heldItem = HeldItem.None;

                    //AddReward(1.0f);
                }

                // If pot just became ready, call GenerateSoup (envController can also do this internally)
                if (envController.CheckSoupReady())
                {
                    envController.GenerateSoup();
                    envController.isCooked = true;
                }
            }

            // retrive soup from pot
            else if (heldItem == HeldItem.None && envController.PotHasSoupReady)
            {
                //AddReward(2.0f);
                heldItem = HeldItem.Soup;
                envController.PotHasSoupReady = false;
            }

            return;
    }

    // === CHIEF2: deliver to counter ===
    if (collision.collider.CompareTag("Counter"))
    {
            if (heldItem == HeldItem.Soup && !envController.isServerd)
            {
                heldItem = HeldItem.None;
                envController.ServeSuccess();
            }
            return;
    }
    
    if (collision.collider.CompareTag("wall"))
    {
        AddReward(-0.0001f);
    }
    }

    // continuoues action for testing with keyboard
    // public override void Heuristic(in ActionBuffers actionsOut)
    // {
    //     ActionSegment<float> action = actionsOut.ContinuousActions;
    //     action[0] = Input.GetAxisRaw("Horizontal");
    //     action[1] = Input.GetAxisRaw("Vertical");
    // }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    public void ResetAgent()
    {
        heldItem = HeldItem.None;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}


// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Unity.MLAgents;
// using Unity.MLAgents.Actuators;
// using Unity.MLAgents.Sensors;
// using NUnit;

// public class SLChief : Agent
// {
//     public enum HeldItem
//     {
//         None,
//         Onion,
//         Dish,
//         SoupReady
//     }

//     public HeldItem heldItem = HeldItem.None;
//     public SLCookingEnvController envController;

//     private Rigidbody rb;
//    //public Renderer agentRenderer;

//     public override void Initialize()
//     {
//         rb = GetComponent<Rigidbody>();
//     }

//     public override void OnEpisodeBegin()
//     {
//         ResetAgent();
//         envController.ResetEnv();
//         //transform.localPosition = new Vector3(Random.Range(-3f, 3f), 0.5f, Random.Range(-2f, 2f));
//     }

//     public override void CollectObservations(VectorSensor sensor)
//     {
//         sensor.AddObservation(transform.localPosition);
//         sensor.AddObservation(envController.onion.transform.position - transform.localPosition);
//         sensor.AddObservation(envController.dish.transform.position - transform.localPosition);
//         sensor.AddObservation(envController.pot.transform.position - transform.localPosition);
//         sensor.AddObservation(envController.counter.transform.position - transform.localPosition);
        
//         sensor.AddObservation((float)heldItem);

//         sensor.AddObservation(envController.hasOnion ? 1f : 0f);
//         sensor.AddObservation(envController.hasDish ? 1f : 0f);

//         sensor.AddObservation(envController.PotHasOnion ? 1f : 0f);
//         sensor.AddObservation(envController.PotHasDish ? 1f : 0f);
//         sensor.AddObservation(envController.PotHasSoupReady ? 1f : 0f);

//         //sensor.AddObservation(envController.isCooked ? 1f : 0f);
//         sensor.AddObservation(envController.isServerd ? 1f : 0f);
//     }

//     public override void OnActionReceived(ActionBuffers actions)
//     {
//         float moveX = 2f * Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
//         float moveZ = 2f * Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
//         Vector3 move = new Vector3(moveX, 0f, moveZ) * 4f;
//         rb.MovePosition(rb.position + move * Time.fixedDeltaTime);

//         AddReward(-1f / envController.MaxEnvSteps);
//     }

//     private void OnCollisionEnter(Collision collision)
//     {
//         if (collision.collider.CompareTag("Onion") && heldItem == HeldItem.None && !envController.hasOnion)
//         {
//             heldItem = HeldItem.Onion;
//             envController.hasOnion = true;
//             AddReward(1f);
//         }

//         else if (collision.collider.CompareTag("Dish") && heldItem == HeldItem.None && !envController.hasDish)
//         {
//             heldItem = HeldItem.Dish;
//             envController.hasDish = true;
//             AddReward(1f);
//         }

//         else if (collision.collider.CompareTag("Pot"))
//         {
//             if (heldItem == HeldItem.Dish || heldItem == HeldItem.Onion)
//             {
//                 if (!envController.IsItemInPot(heldItem))
//                 {
//                     envController.PutInPot(heldItem);
//                     AddReward(1f);
//                     heldItem = HeldItem.None;
//                 }

//                 if (envController.CheckSoupReady())
//                 {
//                     envController.GenerateSoup();
//                     AddReward(2f);
//                     envController.isCooked = true;
//                 }
//             }

//             else if (heldItem == HeldItem.None && envController.PotHasSoupReady)
//             {
//                 // Retrieve the soup from the pot
//                 heldItem = HeldItem.SoupReady;
//                 envController.PotHasSoupReady = false;
//             }
//         }

//         else if (collision.collider.CompareTag("Counter") && envController.isCooked)
//         {
//             Debug.Log($"{gameObject.name} 撞到了柜台，heldItem={heldItem}, isCooked={envController.isCooked}, isServerd={envController.isServerd}");

//             if (heldItem == HeldItem.SoupReady && !envController.isServerd)
//             {
//                 envController.ServeSuccess();
//                 heldItem = HeldItem.None;
//                 envController.agentRenderer.material.color = Color.cyan;
//             }
//         }

//         else if (collision.collider.CompareTag("wall"))
//         {
//             AddReward(-0.0001f);
//         }
//     }

//     public override void Heuristic(in ActionBuffers actionsOut)
//     {
//         ActionSegment<float> action = actionsOut.ContinuousActions;
//         action[0] = Input.GetAxisRaw("Horizontal");
//         action[1] = Input.GetAxisRaw("Vertical");
//     }

//     public void ResetAgent()
//     {
//         heldItem = HeldItem.None;
//         rb.linearVelocity = Vector3.zero;
//         rb.angularVelocity = Vector3.zero;
//     }
// }