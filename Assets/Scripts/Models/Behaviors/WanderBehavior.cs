using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WanderBehavior : AgentBehavior
{
	float updateDelay = 1.0f;
	float nextUpdate = float.MinValue;
	float wanderDirection = Random.Range(0, 2.0f*Mathf.PI);
	float wanderDelta = 0.0f;

	float replotDelay = 10.0f;
	float nextReplot = float.MinValue;

	
	public override bool executePlanUpdate()
	{
		if(_myself.MoveInUse)
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
			nextReplot = Time.realtimeSinceStartup + replotDelay;
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
		newAction.TargetPoint = (_myself.Location + new Vector2(targetX,targetY));
		
		this.currentPlans.Add(newAction);
		
		nextUpdate = Time.realtimeSinceStartup + 0.5f + (Random.value * updateDelay);
		
		return true;
	}
}
