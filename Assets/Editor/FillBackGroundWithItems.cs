using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FillBackGroundWithItems : MonoBehaviour
{
    [MenuItem("GenerateItems/GenerateCubes")]
    static void GenerateCubes()
    {
        Transform parent = GameObject.Find("Background").transform;
        List<Transform> points = GameObject.Find("Orchestrator").GetComponentsInChildren<Transform>().ToList();
        float maxDist = Mathf.NegativeInfinity;
        float currDist = 0f;
        foreach (Transform p1 in points)
        {
            foreach (Transform p2 in points)
            {
                currDist = Vector3.Distance(p1.transform.position, p2.transform.position);

                if (currDist > maxDist)
                    maxDist = currDist;
            }
        }
        print(maxDist);

        GameObject randomPrimitive;
        for (int i = 0; i < (int)maxDist; i++)
        {
            randomPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            randomPrimitive.transform.position = Random.insideUnitSphere * maxDist;
            randomPrimitive.transform.parent = parent;
        }
    }

    [MenuItem("GenerateItems/GenerateSpheres")]
    static void GenerateSpheres()
    {
        Transform parent = GameObject.Find("Background").transform;
        List<Transform> points = GameObject.Find("Orchestrator").GetComponentsInChildren<Transform>().ToList();
        float maxDist = Mathf.NegativeInfinity;
        float currDist = 0f;
        foreach (Transform p1 in points)
        {
            foreach (Transform p2 in points)
            {
                currDist = Vector3.Distance(p1.transform.position, p2.transform.position);

                if (currDist > maxDist)
                    maxDist = currDist;
            }
        }
        print(maxDist);

        GameObject randomPrimitive;
        for (int i = 0; i < (int)maxDist; i++)
        {
            randomPrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            randomPrimitive.transform.position = Random.insideUnitSphere * maxDist;
            randomPrimitive.transform.parent = parent;
        }
    }
}
