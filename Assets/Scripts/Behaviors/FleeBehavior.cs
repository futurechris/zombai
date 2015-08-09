using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FleeBehavior : AgentBehavior
{
	// flee from nearest undead
	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		if(myself.getMoveInUse())
		{
			return false;
		}

		Action newAction;

		float enemyDistance = float.MaxValue;
		Vector2 enemyPosition = Vector2.zero;
		float tempDistance = 0.0f;
		for(int i=0; i<percepts.Count; i++)
		{
			if(percepts[i].living == AgentPercept.LivingState.UNDEAD )
			{
				tempDistance = Vector2.Distance(percepts[i].locOne, myself.getLocation());
				if( tempDistance < enemyDistance)
				{
					enemyDistance = tempDistance;
					enemyPosition = percepts[i].locOne;
				}
			}
		}
		
		if(enemyDistance < float.MaxValue && !myself.getMoveInUse())
		{
			Vector2 mirror = myself.getLocation() + (myself.getLocation() - enemyPosition);
			newAction = new Action(Action.ActionType.MOVE_TOWARDS);
			newAction.setTargetPoint(mirror);

			this.currentPlans.Clear();
			this.currentPlans.Add(newAction);

			return true;
		}

		return false;
	}

}
