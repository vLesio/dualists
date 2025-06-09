using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
[RequireComponent(typeof(HandSteering))]
public class EnemyAgent : Agent
{
 private readonly Dictionary<int, int> _agentInputToPlayerControls = new()
    {
        { 0, 0 },
        { 1, 1 },
        { 2, -1 },
        { -1, -1 },
    };

    private readonly Dictionary<int, int> _playerInputToAgentControls = new()
    {
        { 0, 0 },
        { 1, 1 },
        { -1, 2 },
    };


    private HandSteering _handSteering;
    private GameManager _gameManager;

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
    public bool playerEuclideanPosition;
    public bool playerSpherePosition;
    public bool playerShieldEuclideanPosition;
    public bool playerShieldSpherePosition;
    public bool playerShieldRotation;    
    public bool playerGunEuclideanPosition;
    public bool playerGunSpherePosition;
    public bool playerGunRotation;

    [Header("What agent should be rewarded for")] 
    public bool shieldAimsAtGunReward;
    public bool shieldAimsAtGunRewardAmount;
    
    public bool gunNotAimsAtShieldReward;
    public bool gunNotAimsAtShieldRewardAmount;
    
    public bool bulletBlockedReward;
    public bool bulletBlockedRewardAmount;
    
    public bool hitPlayerReward;
    public bool hitPlayerRewardAmount;
    
    [Header("Important Vars")] 
    public string dupa;
    
    public bool IsActive { get; set; }
    
    public void Awake()
    {
        var managers = GameObject.Find("GameManager");
        _gameManager = managers.GetComponent<GameManager>();
        _handSteering = transform.GetComponent<HandSteering>();
    }

    public void Start()
    {
        RegisterEvents();
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

    public override void CollectObservations(VectorSensor sensor)
    {
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
        HandsDesiredActions handsDesiredActions = CalculateHandDesiredActions(actions);
        // _handSteering.PropagateHandsActions();
        //     new Vector2(
        //         _agentInputToPlayerControls[actions.DiscreteActions[0]],
        //         _agentInputToPlayerControls[actions.DiscreteActions[1]]
        //         )
        //     );

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
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) input.x -= 1f;
        if (Input.GetKey(KeyCode.S)) input.x += 1f;
        if (Input.GetKey(KeyCode.D)) input.y += 1f;
        if (Input.GetKey(KeyCode.A)) input.y -= 1f;

        discreteActions[0] = _playerInputToAgentControls[(int)input.x];
        discreteActions[1] = _playerInputToAgentControls[(int)input.y];
    }
    
    private HandsDesiredActions CalculateHandDesiredActions(ActionBuffers actions)
    {
        throw new NotImplementedException();
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
