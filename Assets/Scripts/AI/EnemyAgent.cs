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
    public bool enemySphereHitboxPositions;
    
    public bool enemyShieldSpherePosition;
    //public bool enemyShieldRotation; // NOT IMPLEMENTED
    //public bool enemyShieldVelocity;
    //public bool enemyShieldAngularVelocity;
    
    public bool enemyGunSpherePosition;
    public bool enemyGunRotation;
    public bool enemyGunVelocity;
    public bool enemyGunAngularVelocity;
    
    public bool enemyAimingAt;
    
    public bool enemyGunUsable;
    public bool enemyGunAmmoPercentage;
    
    // player observations
    public bool playerSphereHitboxPositions;

    public bool playerShieldSpherePosition;
    //public bool playerShieldRotation; // NOT IMPLEMENTED
    //public bool playerShieldVelocity;
    //public bool playerShieldAngularVelocity;
    
    public bool playerGunSpherePosition;
    public bool playerGunRotation;
    public bool playerGunVelocity;
    public bool playerGunAngularVelocity;
    
    public bool playerAimingAt;
    
    public bool playerGunUsable;
    public bool playerGunAmmoPercentage;

    [Header("What agent should be rewarded for")] 
    public bool enemyGunAimsAtShieldReward;
    public float enemyGunAimsAtShieldRewardAmount;
    
    public bool playerGunNotAimsAtShieldReward;
    public float playerGunNotAimsAtShieldRewardAmount;
    
    public bool playerGunAimsAtEnemyReward;
    public float playerGunAimsAtEnemyRewardAmount;
    
    public bool hitEnemyReward;
    public float hitEnemyRewardAmount;
    
    public bool playerDeathPenalty;
    public float playerDeathPenaltyAmount;
    
    public bool playerOutOfAmmoPenalty;
    public float playerOutOfAmmoPenaltyAmount;
    
    public bool playerAmmoPercentageReward;
    public float playerAmmoPercentageRewardAmount;
    
    public bool timeoutPenaltyReward;
    public float timeoutPenaltyRewardAmount;
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

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!GameManager.I.IsRunning())
            return;

        var selfObservations =  _selfObservationCollector.CollectObservations(_handSteering.GlobalPositionSphereCenterPoint);
        var enemyObservations = _enemyObservationCollector.CollectObservations(_handSteering.GlobalPositionSphereCenterPoint);
        
        /// ENEMY OBSERVATIONS
        
        string enemyObservationLog = "Enemy Detailed Observations:";

        if (enemySphereHitboxPositions && enemyObservations.Hitboxes != null)
        {
            foreach (var hitbox in enemyObservations.Hitboxes)
            {
                foreach (var vertex in hitbox.Vertices)
                {
                    sensor.AddObservation(vertex.AsSphericalVector());
                    enemyObservationLog += $"\n\tEnemy hitbox vertex position: {vertex}";
                }
            }
        }

        foreach (HandObservation hand in enemyObservations.HandObservations.Hands)
        {
            if (hand.handSide == HandSide.right) // Gun
            {
                if (enemyGunSpherePosition)
                {
                    var observation = hand.position.AsSphericalVector();
                    sensor.AddObservation(observation);
                    enemyObservationLog += $"\n\tEnemy's GUN (right hand) sphere position: {hand.position}";
                }
                if (enemyGunRotation)
                {
                    sensor.AddObservation(hand.rotation);
                    enemyObservationLog += $"\n\tEnemy's GUN (right hand) rotation: {hand.rotation}";
                }
                if (enemyGunVelocity)
                {
                    sensor.AddObservation(hand.velocity);
                    enemyObservationLog += $"\n\tEnemy's GUN (right hand) velocity: {hand.velocity}";
                }
                if (enemyGunAngularVelocity)
                {
                    sensor.AddObservation(hand.angularVelocity);
                    enemyObservationLog += $"\n\tEnemy's GUN (right hand) angular velocity: {hand.angularVelocity}";
                }
            }
            else if (hand.handSide == HandSide.left) // Shield
            {
                if (enemyShieldSpherePosition)
                {
                    var observation = hand.position.AsSphericalVector();
                    sensor.AddObservation(observation);
                    enemyObservationLog += $"\n\tEnemy's SHIELD (left hand) sphere position: {hand.position}";
                }
            }
        }

        if (enemyAimingAt)
        {
            sensor.AddObservation((int)enemyObservations.AimingAt);
            enemyObservationLog += $"\n\tEnemy is aiming at: {enemyObservations.AimingAt.ToString()}";
        }

        if (enemyGunUsable)
        {
            sensor.AddObservation(enemyObservations.IsGunUsable);
            enemyObservationLog += $"\n\tEnemy's GUN is usable: {enemyObservations.IsGunUsable}";
        }
        if( enemyGunAmmoPercentage)
        {
            sensor.AddObservation(enemyObservations.GunAmmoPercentage);
            enemyObservationLog += $"\n\tEnemy's GUN ammo percentage: {enemyObservations.GunAmmoPercentage}";
        }
        
        /// PLAYER OBSERVATIONS
        
        string playerObservationLog = "Player Detailed Observations:";

        if (playerSphereHitboxPositions && selfObservations.Hitboxes != null)
        {
            foreach (var hitbox in selfObservations.Hitboxes)
            {
                foreach (var vertex in hitbox.Vertices)
                {
                    sensor.AddObservation(vertex.AsSphericalVector());
                    playerObservationLog += $"\n\tPlayer hitbox vertex position (spherical): {vertex}";
                }
            }
        }

        foreach (HandObservation hand in selfObservations.HandObservations.Hands)
        {
            if (hand.handSide == HandSide.right) // Gun
            {
                if (playerGunSpherePosition)
                {
                    var observation = hand.position.AsSphericalVector();
                    sensor.AddObservation(observation);
                    playerObservationLog += $"\n\tPlayer's GUN (right hand) sphere position: {hand.position}";
                }
                if (playerGunRotation)
                {
                    sensor.AddObservation(hand.rotation);
                    playerObservationLog += $"\n\tPlayer's GUN (right hand) rotation: {hand.rotation}";
                }
                if (playerGunVelocity)
                {
                    sensor.AddObservation(hand.velocity);
                    playerObservationLog += $"\n\tPlayer's GUN (right hand) velocity: {hand.velocity}";
                }
                if (playerGunAngularVelocity)
                {
                    sensor.AddObservation(hand.angularVelocity);
                    playerObservationLog += $"\n\tPlayer's GUN (right hand) angular velocity: {hand.angularVelocity}";
                }
            }
            else if (hand.handSide == HandSide.left) // Shield
            {
                if (playerShieldSpherePosition)
                {
                    var observation = hand.position.AsSphericalVector();
                    sensor.AddObservation(observation);
                    playerObservationLog += $"\n\tPlayer's SHIELD (left hand) sphere position: {hand.position}";
                }
            }
        }

        if (playerAimingAt)
        {
            sensor.AddObservation((int)selfObservations.AimingAt);
            playerObservationLog += $"\n\tPlayer is aiming at: {selfObservations.AimingAt.ToString()}";
        }

        if (playerGunUsable)
        {
            sensor.AddObservation(selfObservations.IsGunUsable);
            playerObservationLog += $"\n\tPlayer's GUN is usable: {selfObservations.IsGunUsable}";
        }
        if (playerGunAmmoPercentage)
        {
            sensor.AddObservation(selfObservations.GunAmmoPercentage);
            playerObservationLog += $"\n\tPlayer's GUN ammo percentage: {selfObservations.GunAmmoPercentage}";
        }
        
        if (enableDetailedObservationLogging)
        {
            Debug.Log(enemyObservationLog.ToString());
            Debug.Log(playerObservationLog.ToString());
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
        
    // TODO: hitEnemyReward
    // TODO: playerDeathPenalty
    // TODO: timeoutPenaltyReward
    
    var selfObservations =  _selfObservationCollector.CollectObservations(_handSteering.GlobalPositionSphereCenterPoint);
    var enemyObservations = _enemyObservationCollector.CollectObservations(_handSteering.GlobalPositionSphereCenterPoint);
        
        if (enemyGunAimsAtShieldReward) {
            var enemyGunAimsAtShieldRewardResult = enemyObservations.AimingAt == HitType.Shield ? enemyGunAimsAtShieldRewardAmount : 0;
            detailedGradeLog += $"\n\tenemy gun aims at shield reward: {enemyGunAimsAtShieldRewardResult}";
            rewardSum += enemyGunAimsAtShieldRewardResult;
        }

        if (playerGunNotAimsAtShieldReward)
        {
            var playerGunNotAimsAtShieldRewardResult = (selfObservations.AimingAt != HitType.Shield) ? playerGunNotAimsAtShieldRewardAmount : 0;
            detailedGradeLog += $"\n\tplayer gun not aims at shield reward: {playerGunNotAimsAtShieldRewardResult}";
            rewardSum += playerGunNotAimsAtShieldRewardResult;
        }
        
        if (playerGunAimsAtEnemyReward)
        {
            var playerGunAimsAtEnemyRewardResult = (selfObservations.AimingAt == HitType.Player) ? playerGunAimsAtEnemyRewardAmount : 0;
            detailedGradeLog += $"\n\tplayer gun aims at enemy reward: {playerGunAimsAtEnemyRewardResult}";
            rewardSum += playerGunAimsAtEnemyRewardResult;
        }
        
        if (playerOutOfAmmoPenalty)
        {
            var playerOutOfAmmoPenaltyResult = (selfObservations.GunAmmoPercentage == 0.0f) ? playerOutOfAmmoPenaltyAmount : 0;
            detailedGradeLog += $"\n\tplayer runs out of ammo penalty: {playerOutOfAmmoPenaltyResult}";
            rewardSum -= playerOutOfAmmoPenaltyResult;
        }
        
        if (playerAmmoPercentageReward)
        {
            var playerAmmoPercentageRewardResult = selfObservations.GunAmmoPercentage * playerAmmoPercentageRewardAmount;
            detailedGradeLog += $"\n\tplayer ammo percentage reward: {playerAmmoPercentageRewardResult}";
            rewardSum += playerAmmoPercentageRewardResult;
        }
        
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
