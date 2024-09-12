using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public static Stage Instance { get; private set; }

    [SerializeField] float _width = 30;
    [SerializeField] float _height = 20;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Vector3 RandomPositionInStage() => new Vector3(Random.Range(_width, -_width), 0, Random.Range(_height, -_height));

    public void KeepTransformInStage(Transform user)
    {
        Vector3 pos = user.position;
        float w = _width / 2;
        float h = _height / 2;

        if (pos.z > h) pos.z = -h;
        if (pos.z < -h) pos.z = h;
        if (pos.x > w) pos.x = -w;
        if (pos.x < -w) pos.x = w;

        user.position = pos;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.right * _width + Vector3.forward * _height);
    }
}
