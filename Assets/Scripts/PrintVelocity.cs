using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintVelocity : MonoBehaviour
{
    public Vector3 Velocity;
    Vector3 lastPos;
    float speed;

    public float max = Mathf.NegativeInfinity;
    public float min = Mathf.Infinity;

    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        Velocity = transform.position - lastPos;
        lastPos = transform.position;
        speed = (Velocity / Time.deltaTime).magnitude;
        print(speed);
        if (speed > max)
        {
            max = speed;
        }
        if (speed < min && speed > .5f)
        {
            min = speed;
        }
    }
}
