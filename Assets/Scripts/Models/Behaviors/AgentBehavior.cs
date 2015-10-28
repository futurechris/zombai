using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentBehavior
{
	protected List<Action> currentPlans = new List<Action>();

	[SerializeField]
	protected Agent _myself;

	protected float planUpdateStart = 0.0f;
	protected float planUpdateDelta = 0.0f;
	protected float planUpdateTime = 0.0f; // start off skipping slow calls by default
	protected float planUpdateCount = 1.0f; // fudge-y but saves a branch and a call
	protected bool planUpdateResult = false;

	protected float planUpdateBudget = 0.0f;

	protected float actionTotalTime = 0.00001f;
	protected float actionTotalCount = 1.0f; // fudge-y again

	public List<Action> getCurrentPlans()
	{
		return currentPlans;
	}

	public void addBudget(float allottedWorkUnits)
	{
		planUpdateBudget += allottedWorkUnits;
	}

	public float requestedActionBudget()
	{
		return actionTotalTime / actionTotalCount;
	}

	public void actionTimeTaken(float duration)
	{
		actionTotalTime += duration;
		actionTotalCount++;
	}

	// returns expected budget
	public float requestedPlanBudget()
	{
		if(planUpdateBudget >= (planUpdateTime / planUpdateCount))
		{
//			Debug.Log("Requesting: "+planUpdateBudget+" cost: "+(planUpdateTime / planUpdateCount));
			return (planUpdateTime / planUpdateCount);
		}
//		Debug.Log("Not Requesting: "+planUpdateBudget+" cost: "+(planUpdateTime / planUpdateCount));
		return float.MaxValue;
	}
	
	public bool updatePlan(WorldMap worldMap, float perfectVisionRange)
	{
		// TODO: Decide whether actual is more useful here than anticipated
		planUpdateBudget -= (planUpdateTime / planUpdateCount); // decrement by expected time rather than actual.

		planUpdateResult = false;

		planUpdateStart = Time.unscaledTime;

		// add percept-getting to the plan time
		if(worldMap != null)
		{
			worldMap.getPercepts(this._myself, perfectVisionRange);
		}

		planUpdateResult = executePlanUpdate();
		planUpdateDelta = (Time.realtimeSinceStartup - planUpdateStart);
//		Debug.Log("Delta: "+planUpdateDelta);
		planUpdateTime += planUpdateDelta;
		planUpdateCount++;

		return planUpdateResult;
	}

	public virtual bool executePlanUpdate()
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
				tempDistance = Vector2.Distance(percepts[i].locOne, _myself.Location);
				
				if( tempDistance < targetDistance)
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

	public virtual void setAgent(Agent newAgent)
	{
		_myself = newAgent;
	}
}
