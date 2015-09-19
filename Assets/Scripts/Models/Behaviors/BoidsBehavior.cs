using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoidsBehavior : AgentBehavior {

	public override bool updatePlan(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		Action newMoveAction;
		Action newLookAction;

		float separationWeight = GameObject.FindObjectOfType<AgentDirector>().getSeparationWeight();
		float alignmentWeight  = GameObject.FindObjectOfType<AgentDirector>().getAlignmentWeight();
		float cohesionWeight = GameObject.FindObjectOfType<AgentDirector>().getCohesionWeight();
		float separationThreshold = GameObject.FindObjectOfType<AgentDirector>().getSeparationThreshold();

		// global bit is a temporary hack to test boids behavior in the absence of other stimuli
		Vector2 globalTarget = GameObject.FindObjectOfType<AgentDirector>().getGlobalTarget() - _myself.Location;
		float globalTargetWeight = GameObject.FindObjectOfType<AgentDirector>().getGlobalTargetWeight();


		Agent tempAgent;
		// make sure there's a nearby agent to respond to
		bool found = findNearestAgent(percepts, AgentPercept.LivingState.UNDEAD, out tempAgent);

		if(found && !_myself.MoveInUse && !_myself.LookInUse)
		{
			Vector2 separationVector = calculateSeparationVector(percepts,allottedWorkUnits, separationThreshold);
			Vector2 alignmentVector = calculateAlignmentVector(percepts,allottedWorkUnits);
			Vector2 cohesionVector = calculateCohesionVector(percepts,allottedWorkUnits);

			Vector2 moveVector = 
					  separationWeight * separationVector
					+ alignmentWeight * alignmentVector
					+ cohesionWeight * cohesionVector
					+ globalTargetWeight * globalTarget;


			newMoveAction = new Action(Action.ActionType.MOVE_TOWARDS);
			newMoveAction.TargetPoint = (_myself.Location+moveVector);
		
			newLookAction = new Action(Action.ActionType.TURN_TOWARDS);
			newLookAction.TargetPoint = (_myself.Location+moveVector);


			currentPlans.Clear();
			this.currentPlans.Add(newMoveAction);
			this.currentPlans.Add(newLookAction);
			
			return true;
		}
		return false;
	}

	// position we'd like to move/turn towards to alleviate "excess cohesion"
	private Vector2 calculateSeparationVector(List<AgentPercept> percepts, int allottedWorkUnits, float separationThreshold)
	{
		Vector2 result = Vector2.zero;

		for(int i=0; i<percepts.Count; i++)
		{
			if(percepts[i].living == AgentPercept.LivingState.UNDEAD )
			{
				float tempDistance = Vector2.Distance(percepts[i].locOne, _myself.Location);
				
				if( tempDistance < separationThreshold )
				{
					// Following the "mirror delta" approach for Rule 2 here: http://www.vergenet.net/~conrad/boids/pseudocode.html
					result += _myself.Location - percepts[i].locOne;
				}
			}
		}

		return result;
	}

	// position we'd like to turn towards (gradually) to align our direction with others'
	private Vector2 calculateAlignmentVector(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		float numAlignments = 0;
		float alignmentSum = 0.0f;

		for(int i=0; i<percepts.Count; i++)
		{
			if(percepts[i].living == AgentPercept.LivingState.UNDEAD )
			{
				alignmentSum += percepts[i].facingDirection;
				numAlignments++;
			}
		}

		if(numAlignments == 0)
		{
			return Vector2.zero;
		}

		alignmentSum /= numAlignments;
		alignmentSum *= Mathf.Deg2Rad;

		// compute vector from that alignment
		// kind of unnecessary since we're just going to reverse it in the execution, but keeps consistency between boid rules
		float distance = 100;
		float targetX = distance * Mathf.Cos(alignmentSum);
		float targetY = distance * Mathf.Sin(alignmentSum);
		return new Vector2(targetX, targetY);
	}

	// position we'd like to move towards to alleviate "excess separation"
	private Vector2 calculateCohesionVector(List<AgentPercept> percepts, int allottedWorkUnits)
	{
		Vector2 result = Vector2.zero;
		int agentCount = 0;

		for(int i=0; i<percepts.Count; i++)
		{
			if(percepts[i].living == AgentPercept.LivingState.UNDEAD )
			{
				result += percepts[i].locOne;
				agentCount++;
			}
		}

		if(agentCount == 0)
		{
			return Vector2.zero;
		}


		return (result / agentCount) - _myself.Location;
	}

	/*
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
	*/

}
