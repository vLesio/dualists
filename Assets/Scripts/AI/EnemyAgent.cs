using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
[RequireComponent(typeof(HandSteering))]
[RequireComponent(typeof(SteeringTest))]
[RequireComponent(typeof(AIPlayerController))]
public class EnemyAgent : Agent
{
    private HandSteering _handSteering;
    private SteeringTest _steeringTest;
    private AIPlayerController _aiPlayerController;
    
    private IObservationCollector _selfObservationCollector;
    private IObservationCollector _enemyObservationCollector;
    

    private double _cumReward = 0;
    
    [SerializeField] private bool enableGradeLogging;
    [SerializeField] private bool enableDetailedGradeLogging;
    [SerializeField] private bool enableDetailedObservationLogging;

    [Header("What agent should observe")]
    // enemy observations
    public bool handsSpherePosition;
    public bool handsEuclideanGlobalPosition;
    public bool handsRotation;
    public bool handsEuclideanVelocity;
    public bool handsAngularVelocity;
    
    // player observations
    //public bool playerEuclideanPosition;
    public bool playerSpherePosition;
    //public bool playerShieldEuclideanPosition;
    //public bool playerShieldSpherePosition;
    //public bool playerGunEuclideanPosition;
    public bool playerGunSpherePosition;
    public bool playerGunRotation;
    public bool playerAimingAt;

    [Header("What agent should be rewarded for")] 
    public bool shieldAimsAtGunReward;
    public bool shieldAimsAtGunRewardAmount;
    
    public bool gunNotAimsAtShieldReward;
    public bool gunNotAimsAtShieldRewardAmount;
    
    public bool gunAimsAtPlayerReward;
    public bool gunAimsAtPlayerRewardAmount;
    
    public bool bulletBlockedReward;
    public bool bulletBlockedRewardAmount;
    
    public bool hitPlayerReward;
    public bool hitPlayerRewardAmount;
    
    
    public void Awake()
    {
        _handSteering = transform.GetComponent<HandSteering>();
        _aiPlayerController = transform.GetComponent<AIPlayerController>();
    }
    
    
    public void Start()
    {
        RegisterEvents();
        
        _selfObservationCollector = GetComponent<IObservationCollector>();
        _enemyObservationCollector = FindEnemyObservationCollector();
        if (_enemyObservationCollector is not null && enableDetailedGradeLogging)
        {
            Debug.LogError("[Enemy Agent] Enemy observation collector found!");
        }
    }

    [CanBeNull]
    IObservationCollector FindEnemyObservationCollector()
    {
        Transform gameManager = transform.parent; 
        if(gameManager.name != "GameManager")
        {
            Debug.LogError("[Enemy Agent] EnemyAgent must be a child of GameManager!");
            return null;
        }
        var allCollectors = gameManager.GetComponentsInChildren<MonoBehaviour>(true).OfType<IObservationCollector>();

        var observationCollectors = allCollectors.ToList();
        if (!observationCollectors.Any())
        {
            Debug.LogError("[Enemy Agent] No observation collectors found in GameManager!");
            return null;
        }

        if (observationCollectors.Count() > 2)
        {
            Debug.LogError("[Enemy Agent] More than 2 observation collectors found in GameManager! This is not supported.");
        }
        foreach (var collector in observationCollectors)
        {
            if (collector != _selfObservationCollector) 
                return collector;
        }
        Debug.LogError("[Enemy Agent] No enemy observation collector found! Please ensure the agent has an observation collector attached.");
        return null;
    }

    private void OnDestroy() {
        DeregisterEvents();
    }

    public void ResetAgentStateParameters()
    {
        _cumReward = 0;
    }

    public override void OnEpisodeBegin()
    {
        ResetAgentStateParameters();
    }

    /*public override void CollectObservations(VectorSensor sensor)
    {
        if (!GameManager.I.IsRunning())
            return;
        
        String detailedObservationLog = "Agent Detailed Observations:";
        foreach (HandObservation hand in _handSteering.GetHandsObservation().Hands) {
            if(handsSpherePosition)
            {
                var observation = hand.position.GetPositionObservation();
                sensor.AddObservation(observation);
                detailedObservationLog += $"\n\tAgent's {hand.handSide.ToString()} hand sphere position: {observation}\n";
            }
            if(handsEuclideanGlobalPosition)
            {
                var observation = hand.globalPosition;
                sensor.AddObservation(observation);
                detailedObservationLog += $"\n\tAgent's {hand.handSide.ToString()} hand global position: {observation}\n";
            }
            if(handsRotation)
            {
                var observation = hand.rotation;
                sensor.AddObservation(observation);
                detailedObservationLog += $"\n\tAgent's {hand.handSide.ToString()} hand rotation: {observation}\n";
            }
            if(handsEuclideanVelocity)
            {
                var observation = hand.velocity;
                sensor.AddObservation(observation);
                detailedObservationLog += $"\n\tAgent's {hand.handSide.ToString()} hand euclidean velocity: {observation}\n";
            }
            if(handsAngularVelocity)
            {
                var observation = hand.angularVelocity;
                sensor.AddObservation(observation);
                detailedObservationLog += $"\n\tAgent's {hand.handSide.ToString()} hand angular velocity: {observation}\n";
            }
        }

        //TODO: Add player observations

        if (enableDetailedObservationLogging)
        {
            Debug.Log(detailedObservationLog);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!GameManager.I.IsRunning())
            return;
        // Take actions from the neural network and apply them to the agent
        // 18 continuous actions, 1 discrete action with 2 states (don't shoot, shoot)
        _aiPlayerController.ParseNNActions(actions);
        float rewardSum = 0;

        String detailedGradeLog = "Detailed Grade Log:";
        
        // if (distanceToNearestHider) {
        //     float lowerDistanceToNearestHider = float.MaxValue;
        //     foreach (var hider in _dataReferenceCollector.GetAllHiders())
        //     {
        //         var distance = Vector3.Distance(hider.GetPosition(), _agentMovement.GetPosition());
        //         if (distance < lowerDistanceToNearestHider)
        //         {
        //             lowerDistanceToNearestHider = distance;
        //         }
        //     }
        //
        //     var distanceToNearestHiderRewardResult =  - (lowerDistanceToNearestHider / _diagonalMapLength) * distanceToNearestHiderReward;
        //     detailedGradeLog += $"\n\tdistance to nearest hider: {distanceToNearestHiderRewardResult}";
        //     rewardSum += distanceToNearestHiderRewardResult;
        // }
        
        AddReward(rewardSum);
        _cumReward +=  rewardSum;
        
        if (enableGradeLogging) {
            Debug.Log($"Agent was rewarded by: {rewardSum}. Got in total: {_cumReward}");
        }

        if (enableDetailedGradeLogging) {
            detailedGradeLog += $"\n\tAgent was rewarded by: {rewardSum}. Got in total: {_cumReward}";
            Debug.Log(detailedGradeLog);
        }
    }*/
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        var continuousActions = actionsOut.ContinuousActions;
        
        continuousActions[0] = 0;
        continuousActions[1] = 0;
        continuousActions[2] = 0;
        
        continuousActions[3] = 0;
        continuousActions[4] = 0;
        continuousActions[5] = 0;
        discreteActions[0] = 0;
    }
    
    private void RegisterEvents() {
        return;
    }

    private void DeregisterEvents() {
        return;
    }
    
    private void RegisterGameEnded(bool byTimeout) {
        EndEpisode();
        ResetAgentStateParameters();
        if (enableDetailedGradeLogging)
        {
            Debug.Log($"Episode was ended");
        }
    }
}
