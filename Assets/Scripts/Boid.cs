using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class Boid : MonoBehaviour
{
    public Stage Stage => Stage.Instance;

    public static List<Boid> allBoids;

    public BoidData myData;

    private void Awake()
    {
        if (allBoids == null) allBoids = new List<Boid>();
        allBoids.Add(this);
    }

    private void Start()
    {
        transform.position = Stage.RandomPositionInStage();

        var randomVelocity = new float3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));
        myData = new BoidData(transform.position, math.normalize(randomVelocity) * 10);
    }

    public void UpdatePosition(BoidData data)
    {
        myData = data;
        transform.position = myData.position + myData.velocity * Time.deltaTime;
        transform.forward = myData.velocity;
        Stage.KeepTransformInStage(transform);
        myData.position = transform.position;
    }
}

public struct BoidData
{
    public float3 position;
    public float3 velocity;

    public BoidData(float3 position, float3 velocity)
    {
        this.position = position;
        this.velocity = velocity;
    }
}