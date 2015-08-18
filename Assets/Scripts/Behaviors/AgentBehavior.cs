using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentBehavior
{
	protected List<Action> currentPlans = new List<Action>();

	protected Agent myself;

	public List<Action> getCurrentPlans()
	{
		return currentPlans;
	}

	public virtual void setAgent(Agent newAgent)
	{
		myself = newAgent;
	}

	public virtual bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		currentPlans.Clear();
		return true;
	}

	protected bool findNearestAgent(List<AgentPercept> percepts, AgentPercept.LivingState livingType, out Agent foundAgent)
	{
		float targetDistance = float.MaxValue;
		Vector2 targetPosition = Vector2.zero;

		float tempDistance = 0.0f;

		foundAgent = null;

		for(int i=0; i<percepts.Count; i++)
		{
			if(percepts[i].living == livingType )
			{
				tempDistance = Vector2.Distance(percepts[i].locOne, myself.getLocation());
				
				if( tempDistance < targetDistance && (percepts[i].perceivedAgent!=null))
				{
					targetDistance = tempDistance;
					targetPosition = percepts[i].locOne;
					foundAgent = percepts[i].perceivedAgent;
				}
			}
		}

		if(foundAgent != null)
		{
			return true;
		}

		return false;
	}
}
