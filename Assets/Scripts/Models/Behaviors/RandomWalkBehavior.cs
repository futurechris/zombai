using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomWalkBehavior : AgentBehavior
{
	float updateDelay = 1.0f;
	float nextUpdate = float.MinValue;

	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(_myself.MoveInUse)
		{
			return false;
		}

		// only update every so often
		if(nextUpdate > Time.time)
		{
			return true;
		}

		currentPlans.Clear();

		// pick a random angle in radians and walk in that direction
		float a = Random.Range(0, 2.0f*Mathf.PI);

		float distance = 1000;
		float targetX = distance * Mathf.Cos(a);
		float targetY = distance * Mathf.Sin(a);

		Action newAction = new Action(Action.ActionType.MOVE_TOWARDS);
		newAction.TargetPoint = (_myself.Location + new Vector2(targetX,targetY));

		this.currentPlans.Add(newAction);

		nextUpdate = Time.time + 0.5f + (Random.value * updateDelay);

		return true;
	}
}
