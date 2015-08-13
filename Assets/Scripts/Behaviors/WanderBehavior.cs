using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WanderBehavior : AgentBehavior
{
	float updateDelay = 1.0f;
	float nextUpdate = 0;
	float wanderDirection = 0.0f;
	float wanderDelta = 0.0f;

	float replotDelay = 10.0f;
	float nextReplot = 0;

	
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(myself.getMoveInUse())
		{
			return false;
		}
		
		// only update every so often
		if(nextUpdate > Time.realtimeSinceStartup)
		{
			return true;
		}
		
		currentPlans.Clear();

		float a = wanderDirection;
		if(nextReplot <= Time.realtimeSinceStartup)
		{
			wanderDirection = Random.Range(0, 2.0f*Mathf.PI);
			nextReplot += replotDelay;
		}

		wanderDelta += (Random.value * 0.1f) - 0.05f;
		if(wanderDelta > 10.0f)
		{
			wanderDelta = 10.0f;
		}
		if(wanderDelta < -10.0f)
		{
			wanderDelta = -10.0f;
		}
		a += wanderDelta;



		float distance = 1000;
		float targetX = distance * Mathf.Cos(a);
		float targetY = distance * Mathf.Sin(a);
		
		Action newAction = new Action(Action.ActionType.MOVE_TOWARDS);
		newAction.setTargetPoint(myself.getLocation() + new Vector2(targetX,targetY));
		
		this.currentPlans.Add(newAction);
		
		nextUpdate += 0.5f + (Random.value * updateDelay);
		
		return true;
	}
}
