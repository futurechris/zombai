﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentDirector : MonoBehaviour {

	//////////////////////////////////////////////////////////////////
	#region Parameters & properties

	public OverlayUpdater overlayUpdater;
	public WorldMapRenderer mapRenderer;

	public int worldWidth 				= 1024;
	public int worldHeight 				= 768;
	public int buildingCount 			= 100;

	public int initLivingCount 			= 1000;
	public int initUndeadCount 			= 1;
	public int initCorpseCount 			= 0;

	public bool spawnPlayerAgent		= false;

	public bool resetWhenAllDead 		= true;

	// Eventually these will be used to calculate how much time to allot to each agent's AI calcs
	private int	_targetFramerate 		= 15; // fps
	public 	int targetFramerate  		= 15;
	
	// how many pixels should be moved per second for an agent on the go.
	private float _moveSpeed 			= 10.0f;

	// how many degrees should be turned per second baseline
	private float _turnSpeed			= 60.0f;
	
	// general sim multiplier - currently just another multiplier on moveSpeed
	private float _simulationSpeed 		= 1.0f;
	
	// within this range, agent FOVs are 360-degree. Just to smooth out the overlap situation.
	private float _perfectVisionRange 	= 2.0f;

	private float _coincidentRange 		= 0.0001f;

	// boids params
	public float separationWeight 		= 1.0f;
	public float alignmentWeight 		= 1.0f;
	public float cohesionWeight 		= 1.0f;
	public float separationThreshold 	= 15.0f; // distance inside which separation weight triggers
	public float globalTargetWeight		= 1.0f;

	public int changeEvery				= 0;
	private int getCount				= 0;
	public Vector2 globalTarget			= Vector2.zero;

	#endregion Parameters & properties
	//////////////////////////////////////////////////////////////////

	////////////////////////////////////////
	#region Bookkeeping

	WorldMap worldMap;

	float cycles = 0;
	float timeAB = 0.0f;
	float timeBC = 0.0f;
	float timeCD = 0.0f;
	float timeDE = 0.0f;
	float timeEF = 0.0f;

	#endregion Bookkeeping
	////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Map State
	private bool paused	= false;
	#endregion Map State
	//////////////////////////////////////////////////////////////////
	
	//////////////////////////////////////////////////////////////////
	#region MonoBehavior methods & helpers

	// Use this for initialization
	void Start()
	{
		buildWorld();
	}

	private void buildWorld()
	{
		if(mapRenderer != null)
		{
			mapRenderer.purge();
		}
		worldMap = new WorldMap(worldWidth,worldHeight,buildingCount);
		List<Agent> population = worldMap.populateWorld(initLivingCount,initCorpseCount,initUndeadCount);
		
		mapRenderer.WorldMap = (worldMap);
		mapRenderer.instantiateWorld();
		
		ActionArbiter.Instance.WorldMap = (worldMap);
		if(overlayUpdater != null)
		{
			overlayUpdater.map = worldMap;
		}

		if(spawnPlayerAgent)
		{
			spawnAgent(Agent.AgentType.HUMAN_PLAYER);
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		float timeA = Time.realtimeSinceStartup;
		// 0. Update parameters
		updateParameters();

		if(Input.GetKeyDown(KeyCode.Z))
		{
			mapRenderer.instantiateAgents(worldMap.populateWorld(0,0,1));
			worldMap.updateAgentTree();
		}
		if(Input.GetKeyDown(KeyCode.H))
		{
			mapRenderer.instantiateAgents(worldMap.populateWorld(1,0,0));
			worldMap.updateAgentTree();
		}
		if(Input.GetKeyDown(KeyCode.C))
		{
			mapRenderer.instantiateAgents(worldMap.populateWorld(0,1,0));
			worldMap.updateAgentTree();
		}
		if(Input.GetKeyDown(KeyCode.E))
		{
			mapRenderer.instantiateObjects(worldMap.placeWorldObject(WorldObject.ObjectType.EXTRACT_POINT));
		}


		// 1. If world is paused, exit early
		if(paused)
		{
			return;
		}

		float timeB = Time.realtimeSinceStartup;
		// 2. Iterate over agents, ask them for action, run for deltaTime
		foreach(Agent agent in worldMap.getAgents())
		{
			executeAction(agent, Time.deltaTime);
		}

		float timeC = Time.realtimeSinceStartup;
		// 3. Action arbiter determines outcome of opposed actions
		ActionArbiter.Instance.resolveActions();

		float timeD = Time.realtimeSinceStartup;
		// 4. Update worldMap's quadTree
		worldMap.updateAgentTree();

		float timeE = Time.realtimeSinceStartup;
		// 5. Allocate time to agents to process percepts and update plans
		foreach(Agent agent in worldMap.getAgents())
		{
			// Currently the '1' work unit is meaningless - agents will just do their little
			// calculation and be done with it.
			if(		agent.LivingState == AgentPercept.LivingState.ALIVE
			   || 	agent.LivingState == AgentPercept.LivingState.UNDEAD)
			{
				agent.LookInUse = (false);
				agent.MoveInUse = (false);
				agent.Behavior.updatePlan( worldMap.getPercepts(agent, _perfectVisionRange), 1 );
			}
		}
		float timeF = Time.realtimeSinceStartup;
		cycles++;

		timeAB += (timeB-timeA);
		timeBC += (timeC-timeB);
		timeCD += (timeD-timeC);
		timeDE += (timeE-timeD);
		timeEF += (timeF-timeE);

		// comments are rough results of tests
		// Roughly one third spent on executeAction
		// Most of the remaining on planning
		// Good! Gives us space to improve with LOD.
		float avgAB = timeAB/cycles; // 0
		float avgBC = timeBC/cycles; // 32
		float avgCD = timeCD/cycles; // 0
		float avgDE = timeDE/cycles; // 1
		float avgEF = timeEF/cycles; // 67
		float total = (avgAB+avgBC+avgCD+avgDE+avgEF)/100.0f;

//		Debug.Log("Times:    "+timeAB+", "+timeBC+", "+timeCD+", "+timeDE+", "+timeEF);
//		Debug.Log("AvgTimes: "+avgAB+", "+avgBC+", "+avgCD+", "+avgDE+", "+avgEF);
//		Debug.Log("Percents: "+(avgAB/total)+", "+(avgBC/total)+", "+(avgCD/total)+", "+(avgDE/total)+", "+(avgEF/total));

		if(worldMap.getLivingCount() == 0 && resetWhenAllDead)
		{
			buildWorld();
		}
	}
	
	private void updateParameters()
	{
		if(targetFramerate != _targetFramerate)
		{
			_targetFramerate = targetFramerate;
		}
	}

	public void spawnAgent(Agent.AgentType type)
	{
		mapRenderer.instantiateAgents(worldMap.spawnOne(type));
		worldMap.updateAgentTree();
	}
	public void spawnAgent(Agent.AgentType type, Vector2 pos)
	{
		mapRenderer.instantiateAgents(worldMap.spawnOne(type, pos));
		worldMap.updateAgentTree();
	}

	#endregion MonoBehavior methods & helpers
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Agent Action
	private void executeAction(Agent agent, float duration)
	{
		List<Action> planList = agent.Behavior.getCurrentPlans();
		for(int i=0; i<planList.Count; i++)
		{
			switch(planList[i].Type)
			{
				case Action.ActionType.STAY:
					break;
					
				case Action.ActionType.MOVE_TOWARDS:
					moveAgentTowards( agent, planList[i].TargetPoint, duration);
					break;
					
				case Action.ActionType.TURN_TO_DEGREES:
					turnAgentTo( agent, planList[i].Direction, duration);
					break;
					
				case Action.ActionType.TURN_TOWARDS:
					turnAgentTowards( agent, planList[i].TargetPoint, duration);
					break;
					
				case Action.ActionType.CONVERT:
					ActionArbiter.Instance.requestAction(agent, planList[i].TargetAgent, ActionArbiter.ActionType.CONVERT);
					break;

				case Action.ActionType.EXTRACT:
					ActionArbiter.Instance.requestAction(agent, agent, ActionArbiter.ActionType.EXTRACT);
					break;
			}
		}
	}
	
	// attempt to move the agent as far as possible towards target,
	// given a duration of movement.
	private void moveAgentTowards(Agent agent, Vector2 target, float duration)
	{
		float intendedDistance = Vector2.Distance(agent.Location, target);
		float netSpeedMultiplier = duration * _moveSpeed * _simulationSpeed * agent.SpeedMultiplier;

		netSpeedMultiplier = Mathf.Clamp(netSpeedMultiplier, netSpeedMultiplier, intendedDistance);

		Vector2 newPoint = agent.Location + (target - agent.Location).normalized * netSpeedMultiplier;
		
		if(worldMap.isValidPosition(newPoint))
		{
			agent.Location = (newPoint);
		}
		else
		{
			// partial movement & wall-sliding go here
			Vector2 firstStop = nearestDirect(agent.Location, newPoint, 0.125f);
			
			// in 2D should only have one valid dimension here.
			// and technically, could get even more accurate by calling nearestDirect again.
			// TODO: Evaluate whether it's worth using nearestDirect here.
			Vector2 xTarget = new Vector2(newPoint.x, firstStop.y);
			Vector2 yTarget = new Vector2(firstStop.x, newPoint.y);
			if(worldMap.isValidPosition(xTarget))
			{
				agent.Location = (xTarget);
			}
			else if(worldMap.isValidPosition(yTarget))
			{
				agent.Location = (yTarget);
			}
		}
	}

	private void turnAgentTo(Agent agent, float newAngle, float duration)
	{
		agent.Direction = (Mathf.MoveTowardsAngle(agent.Direction, newAngle, _turnSpeed*duration*agent.TurnSpeedMultiplier));
	}
	
	private void turnAgentTowards(Agent agent, Vector2 point, float duration)
	{
		Vector2 delta = point - agent.Location;
		if(delta.magnitude < _coincidentRange)
		{
			return;
		}
		float newAngle = (90 - Mathf.Rad2Deg * Mathf.Atan2(delta.x, delta.y));

		turnAgentTo( agent, newAngle, duration );
	}
	
	private Vector2 nearestDirect(Vector2 start, Vector2 stop, float incrementPercent)
	{
		// technically these won't line up well, especially if incrementPercent
		// is something like 0.1, which doesn't play well with binary.
		// but Vector2.Lerp clamps [0,1] so at worst it's an extra iteration.
		Vector2 bestPoint = start;
		Vector2 tempPoint = start;
		for(float t = 0; t<=1.00001f; t+=incrementPercent)
		{
			tempPoint = Vector2.Lerp(start,stop,t);
			if(worldMap.isValidPosition(tempPoint))
			{
				bestPoint = tempPoint;
			}
		}
		return bestPoint;
	}

	#endregion Agent Action
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Getters/Setters
	// leaving these as non-properties for now
	// Because Unity doesn't show Properties in the inspector without a custom editor script
	// The point of the Boids parameters is to be tweaked in the inspector, not so much to
	// be long-term real values.
	
	public void setSimulationSpeed(float newSimSpeed)
	{
		_simulationSpeed = newSimSpeed;
	}

	public float getSeparationWeight()
	{
		return separationWeight;
	}
	public float getAlignmentWeight()
	{
		return alignmentWeight;
	}
	public float getCohesionWeight()
	{
		return cohesionWeight;
	}
	public float getSeparationThreshold()
	{
		return separationThreshold;
	}

	public Vector2 getGlobalTarget()
	{
		if(changeEvery > 0)
		{
			if(getCount >= changeEvery)
			{
				getCount = 0;
				globalTarget = new Vector2(Random.Range(0,worldWidth), Random.Range(0,worldHeight));
			}
		}
		getCount++;
		return globalTarget;
	}

	public float getGlobalTargetWeight()
	{
		return globalTargetWeight;
	}
	
	#endregion Getters/Setters
	//////////////////////////////////////////////////////////////////
}
