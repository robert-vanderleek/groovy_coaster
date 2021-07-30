using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform posIndicator;
    public Vector3 offset = new Vector3(0, 20, 0);

    // Update is called once per frame
    void Update()
    {
        //keep tracked on player object
        transform.position = posIndicator.position + offset;
        //maybe rotate or something based on time?
    }
}
