using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LineWalker : MonoBehaviour
{
    public GameObject orchestrator;
    public List<Vector3> points;
    private float speed;
    private Vector3 currTarget;
    private int indexOfCurrTarget;
    private Vector3 normalizedDir;
    private bool isReady = false;
    private List<float> segmentDistances;
    private float totalDist;

    public void Init()
    {
        indexOfCurrTarget = 0;
        points = new List<Vector3>();
        segmentDistances = new List<float>();
        foreach (Transform transform in orchestrator.transform)
        {
            points.Add(transform.position);
        }
        currTarget = points[indexOfCurrTarget];
        normalizedDir = (currTarget - transform.position).normalized;
        totalDist = CalculateTotalDistance();
        PopulateSegmentDistancesNormalized();
        speed = totalDist / orchestrator.GetComponent<AudioSource>().clip.length;
    }

    public void SetReady()
    {
        isReady = true;
    }

    void Update()
    {
        if (!isReady)
            return;

        if (Vector3.Distance(currTarget, transform.position) <= .1f)
        {
            indexOfCurrTarget++;
            if (indexOfCurrTarget > points.Count)
            {
                enabled = false;
                print("Finished walking points");
                return;
            }
            currTarget = points[indexOfCurrTarget];
            normalizedDir = (currTarget - transform.position).normalized;
        }
        else
        {
            transform.position = transform.position + normalizedDir * speed * Time.deltaTime;
        }
    }

    private float CalculateTotalDistance()
    {
        float totalDist = 0f;
        float currDist;
        for (int i = 0; i < points.Count; i++)
        {
            if (i + 1 > points.Count - 1)
            {
                return totalDist;
            }
            else
            {
                currDist = Vector3.Distance(points[i], points[i + 1]);
                totalDist += currDist;
                segmentDistances.Add(currDist);
            }
        }

        return totalDist;
    }

    private void PopulateSegmentDistancesNormalized()
    {
        for (int i = 0; i < segmentDistances.Count; i++)
        {
            segmentDistances[i] = segmentDistances[i] / totalDist;
        }
    }

    public Vector3 GetPoint(float normalizedDist)
    {
        //run thru list, if normalized dist is between us and us + next, then we have the segment. if not, move to next but add all previous vals to it
        float startPoint = 0f;
        float nextPoint = segmentDistances[0];
        for (int i = 0; i < segmentDistances.Count; i++)
        {
            if (normalizedDist >= startPoint && normalizedDist <= nextPoint)
            {
                //print($"input: {normalizedDist} segment asked for is: {i} which is betwen {points[i]} and {points[i + 1]}");
                //get the normalized distance along our current segment rather than the full distance 
                float distAlongSegment = (normalizedDist - segmentDistances.Take(i).Sum()) / segmentDistances[i];
                //lerp between the two points of our current segment
                Vector3 pos = Vector3.Lerp(points[i], points[i + 1], distAlongSegment);
                return pos;
            }
            else
            {
                startPoint += segmentDistances[i];
                nextPoint += segmentDistances[i + 1];
            }
        }
        return Vector3.zero;
    }
}
