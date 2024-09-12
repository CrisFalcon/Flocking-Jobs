using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

public class FlockManager : MonoBehaviour
{
    List<Boid> Boids => Boid.allBoids;
    [SerializeField] FlockingData _flockData;
    
    NativeArray<BoidData> allBoidsData;
    FlockingJob _job;

    void Start()
    {
        allBoidsData = new NativeArray<BoidData>(Boids.Count, Allocator.Persistent);
        _job = new FlockingJob()
        {
            Boids = allBoidsData,
            FlockingData = _flockData,
            DeltaTime = Time.deltaTime
        };
    }

    void Update()
    {
        UpdateJob();

        _job.Schedule(Boids.Count, 64).Complete();

        for (int i = 0; i < Boids.Count; i++)
            Boids[i].UpdatePosition(allBoidsData[i]);
    }

    private void UpdateJob()
    {
        for (int i = 0; i < Boids.Count; i++)
            allBoidsData[i] = Boids[i].myData;

        _job.Boids = allBoidsData;
        _job.FlockingData = _flockData;
        _job.DeltaTime = Time.deltaTime;
    }

    private void OnDestroy() => allBoidsData.Dispose();
    
}

[BurstCompile]
internal struct FlockingJob : IJobParallelFor
{
    public NativeArray<BoidData> Boids;
    [ReadOnly] public FlockingData FlockingData;
    [ReadOnly] public float DeltaTime;

    public void Execute(int index)
    {
        BoidData current = Boids[index];

        if (CalculateCohesionAndAlignment(current, out float3 alignment, out float3 cohesion))
        {
            float3 newVelocity = alignment * FlockingData.alignmentWeight
                                + cohesion * FlockingData.cohesionWeight;

            if (CalculateSeparation(current, out float3 separation))
                newVelocity += separation * FlockingData.separationWeight;

            current.velocity = (current.velocity + newVelocity).Clamp(FlockingData.maxSpeed);
        }
        
        Boids[index] = current;
    }

    bool CalculateCohesionAndAlignment(BoidData agent, out float3 alignment, out float3 cohesion)
    {
        alignment = float3.zero;
        cohesion = float3.zero;
        int count = -1;
        foreach (var item in Boids)
        {
            if (math.distance(item.position, agent.position) > FlockingData.viewRadius) continue;
            count++;
            alignment += item.velocity;
            cohesion += item.position;
        }
        if (count <= 0) return false;

        alignment = CalculateSteering(alignment / count, agent);
        cohesion = CalculateSteering((cohesion / count) - agent.position, agent);
        return true;
    }

    bool CalculateSeparation(BoidData agent, out float3 separation)
    {
        separation = float3.zero;
        int count = 0;
        foreach (var item in Boids)
        {
            if (math.distance(item.position, agent.position) > FlockingData.separationRadius) continue;
            count++;
            separation += item.position - agent.position;
        }
        if (count == 0) return false;

        separation = CalculateSteering(separation * -1, agent);
        return true;
    }

    float3 CalculateSteering(float3 desired, BoidData agent)
    {
        if (desired.Equals(float3.zero)) return desired;
        desired = (math.normalize(desired) * FlockingData.maxSpeed) - agent.velocity;

        return desired.Clamp(FlockingData.maxForce * DeltaTime);
    }

}

public static class BurstExtensions
{
    public static float3 Clamp(this float3 vector, float value)
    {
        if (math.length(vector) > value)
            vector = math.normalize(vector) * value;

        return vector;
    }
}

[System.Serializable]
public struct FlockingData
{
    [SerializeField] public float maxSpeed;
    [SerializeField] public float maxForce;
    [SerializeField] public float viewRadius;
    [SerializeField] public float separationRadius;
    [SerializeField] public float separationWeight;
    [SerializeField] public float cohesionWeight;
    [SerializeField] public float alignmentWeight;
}