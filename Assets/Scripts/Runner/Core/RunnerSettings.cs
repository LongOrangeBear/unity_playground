using UnityEngine;

/// <summary>
/// Global settings for the Runner game. Tune values in Inspector.
/// </summary>
[CreateAssetMenu(fileName = "RunnerSettings", menuName = "Runner/Settings")]
public class RunnerSettings : ScriptableObject
{
    [Header("Player Movement")]
    [Tooltip("Forward running speed (units/sec)")]
    public float runSpeed = 20f;
    
    [Tooltip("Lane switch speed (units/sec)")]
    public float laneSwitchSpeed = 15f;
    
    [Tooltip("Distance between lanes")]
    public float laneWidth = 3f;
    
    [Tooltip("Maximum X position (lane bounds)")]
    public float maxX = 4.5f;
    
    [Header("Jump")]
    [Tooltip("Jump force")]
    public float jumpForce = 12f;
    
    [Tooltip("Gravity multiplier")]
    public float gravityMultiplier = 2.5f;
    
    [Header("Slide")]
    [Tooltip("Slide duration in seconds")]
    public float slideDuration = 0.8f;
    
    [Tooltip("Collider height during slide")]
    public float slideColliderHeight = 0.5f;
    
    [Header("World Generation")]
    [Tooltip("Length of each chunk")]
    public float chunkLength = 50f;
    
    [Tooltip("Number of chunks to keep spawned ahead")]
    public int chunksAhead = 3;
    
    [Tooltip("Number of chunks to keep behind before despawn")]
    public int chunksBehind = 1;
    
    [Header("Difficulty")]
    [Tooltip("Speed increase per 100 meters")]
    public float speedIncreaseRate = 0.5f;
    
    [Tooltip("Maximum run speed")]
    public float maxRunSpeed = 40f;
    
    [Header("Scoring")]
    [Tooltip("Points per meter traveled")]
    public int pointsPerMeter = 1;
    
    [Tooltip("Points per coin collected")]
    public int pointsPerCoin = 10;
}
