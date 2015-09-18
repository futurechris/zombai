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
	
	protected bool findNearestPercept(List<AgentPercept> percepts, 
	                                  AgentPercept.LivingState livingType,
	                                  AgentPercept.PerceptType perceptType,
	                                  out AgentPercept nearest)
	{
		float targetDistance = float.MaxValue;
		Vector2 targetPosition = Vector2.zero;
		
		float tempDistance = 0.0f;
		
		nearest = null;
		
		for(int i=0; i<percepts.Count; i++)
		{
			if(percepts[i].living == livingType && percepts[i].type == perceptType)
			{
				tempDistance = Vector2.Distance(percepts[i].locOne, myself.getLocation());
				
				if( tempDistance < targetDistance && (percepts[i].perceivedAgent!=null))
				{
					targetDistance = tempDistance;
					targetPosition = percepts[i].locOne;
					nearest = percepts[i];
				}
			}
		}
		
		if(nearest != null)
		{
			return true;
		}

		if(percepts.Count > 0)
		{
			// prefer to if-and-assign than to allocate.
			nearest = percepts[0];
			return false;
		}

		nearest = new AgentPercept();
		return false;
	}

	protected bool findNearestAgent(List<AgentPercept> percepts, AgentPercept.LivingState livingType, out Agent foundAgent)
	{
		AgentPercept resultAgent;
		bool result = findNearestPercept(percepts, livingType, AgentPercept.PerceptType.AGENT, out resultAgent);
		if(result)
		{
			if(resultAgent.perceivedAgent != null)
			{
				foundAgent = resultAgent.perceivedAgent;
				return true;
			}
			else
			{
				foundAgent = new Agent();
				return false;
			}
		}
		else
		{
			foundAgent = new Agent();
			return false;
		}
	}
}
