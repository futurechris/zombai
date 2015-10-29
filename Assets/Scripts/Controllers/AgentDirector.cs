using UnityEngine;
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
	private float _targetFrameFreq 		= 1.0f/30.0f; // 1 / fps
	public 	float targetFrameFreq  		= 1.0f/30.0f;

	private float _UATtotalTime			= 0.0f; // init to expecting it's instantaneous
	private float _UATtotalCount		= 1.0f;
	private bool _UATneeded				= true;

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

	private int prevEndIdx				= 0;


	#endregion Parameters & properties
	//////////////////////////////////////////////////////////////////

	////////////////////////////////////////
	#region Bookkeeping

	WorldMap worldMap;

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
			mapRenderer.instantiateObjects(worldMap.placeWorldObject(WorldObject.ObjectType.EXTRACT_POINT));
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		// 0. Update parameters
		updateParameters();

		handleKeyInput();

		// 1. If world is paused, exit early
		if(paused)
		{
			return;
		}

		// 2. Iterate over agents, ask them for action, run for deltaTime
		foreach(Agent agent in worldMap.getAgents())
		{
			executeAction(agent, Time.deltaTime/*Time.realtimeSinceStartup + targetFrameFreq*/);
		}

		// 3. Action arbiter determines outcome of opposed actions
		ActionArbiter.Instance.resolveActions();

		float timeLeft = targetFrameFreq - (Time.realtimeSinceStartup - Time.unscaledTime); // how much time is left over if we're to hit target freq

		// 4. Update worldMap's quadTree
		float timeStart = Time.realtimeSinceStartup;
		if(timeLeft > (_UATtotalTime/_UATtotalCount) && _UATneeded)
		{
			worldMap.updateAgentTree();
			// maintain rough estimate of how long this has been taking
			_UATtotalTime += Time.realtimeSinceStartup-timeStart;
			_UATtotalCount++;
			_UATneeded = false;
		}

		// 5. Allocate time to agents to process percepts and update plans
		timeLeft = targetFrameFreq - (Time.realtimeSinceStartup - Time.unscaledTime);

		updateAgentPlans(timeLeft);

		if(worldMap.getLivingCount() == 0 && resetWhenAllDead)
		{
			buildWorld();
		}
	}

	private void updateAgentPlans(float timeLeft)
	{
		float endTime = Time.realtimeSinceStartup+timeLeft; // when we reach endTime, can't plan anymore.

		float timePerAgent = timeLeft;
		float startTime = 0.0f;
		float perceptTime = 0.0f;
		float requestTime = 0.0f;
		List<Agent> agentList = worldMap.getAgents();
		int listSize = agentList.Count;

		if(listSize != 0)
		{
			timePerAgent /= (float)(listSize);
		}
		else
		{
			return;
		}

		Agent agent;
		for(int idxOffset=0; idxOffset<listSize; idxOffset++)
		{
//			Debug.Log("Checking agent "+((prevEndIdx+idxOffset)%listSize));
			agent = agentList[(prevEndIdx+idxOffset)%listSize];
			// Currently the '1' work unit is meaningless - agents will just do their little
			// calculation and be done with it.
			if(		agent.LivingState == AgentPercept.LivingState.ALIVE
			   || 	agent.LivingState == AgentPercept.LivingState.UNDEAD)
			{
				agent.LookInUse = false;
				agent.MoveInUse = false;
				agent.Behavior.addBudget(timePerAgent);
				requestTime = agent.Behavior.requestedPlanBudget();

				if(Time.realtimeSinceStartup+requestTime < endTime)
				{
					agent.Behavior.updatePlan( worldMap, _perfectVisionRange );
					_UATneeded = true;
				}
			}
			if(Time.realtimeSinceStartup > endTime)
			{
				Debug.Log("Last PEI: "+prevEndIdx+" @ "+idxOffset+" # "+timeLeft);
				prevEndIdx = (prevEndIdx+idxOffset)%listSize;
				Debug.Log("New PEI: "+prevEndIdx+" size: "+listSize);
				return;
			}
		}
	}

	private void handleKeyInput()
	{
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
	}

	private void updateParameters()
	{
		if(targetFrameFreq != _targetFrameFreq)
		{
			_targetFrameFreq = targetFrameFreq;
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

		if(agent.Behavior.requestedActionBudget()*planList.Count > duration)
		{
			return;
		}

		float startTime = 0.0f;

		for(int i=0; i<planList.Count; i++)
		{
			startTime = Time.realtimeSinceStartup;
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
			agent.Behavior.actionTimeTaken(Time.realtimeSinceStartup-startTime); // Somewhat wonky with the STAY in there
		}
	}
	
	// attempt to move the agent as far as possible towards target,
	// given a duration of movement.
	private void moveAgentTowards(Agent agent, Vector2 target, float duration)
	{
//		float intendedDistance = Vector2.Distance(agent.Location, target);
		float netSpeedMultiplier = duration * _moveSpeed * _simulationSpeed * agent.SpeedMultiplier;

		netSpeedMultiplier = Mathf.Clamp(netSpeedMultiplier, netSpeedMultiplier, Vector2.Distance(agent.Location, target));

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
