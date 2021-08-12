using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
	private LineRenderer lr;

	void Start()
	{
		lr = GetComponent<LineRenderer>();
		lr.startWidth = .2f;
		lr.endWidth = .2f;
	}

	public void SetPoints(List<Vector3> points)
	{
		lr.positionCount = points.Count;
		lr.SetPositions(points.ToArray());
	}

	private void OnDisable()
	{
		lr.positionCount = 0;
	}

	private void OnDestroy()
	{
		lr.positionCount = 0;
	}
}

