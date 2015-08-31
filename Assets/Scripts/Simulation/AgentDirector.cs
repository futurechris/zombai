using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentDirector : MonoBehaviour {

	//////////////////////////////////////////////////////////////////
	#region Parameters & properties

	public OverlayUpdater overlayUpdater;
	public WorldMapRenderer mapRenderer;

	public int worldWidth = 1024;
	public int worldHeight = 768;
	public int buildingCount = 100;

	// Eventually these will be used to calculate how much time to allot to each agent's AI calcs
	private int	_targetFramerate = 15; // fps
	public 	int targetFramerate  = 15;
	
	// how many pixels should be moved per second for an agent on the go.
	private float moveSpeed = 10.0f;
	
	// general sim multiplier - currently just another multiplier on moveSpeed
	private float simulationSpeed = 1.0f;
	
	// within this range, agent FOVs are 360-degree. Just to smooth out the overlap situation.
	private float perfectVisionRange = 2.0f;

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
		// hard-coding size for now - camera in scene is set to work for this size as well.
		configureCamera();
		worldMap = new WorldMap(worldWidth,worldHeight,buildingCount);
		worldMap.populateWorld(1000,10,10);

		mapRenderer.setWorldMap(worldMap);
		mapRenderer.instantiateWorld();

		ActionArbiter.Instance.setWorldMap(worldMap);
		if(overlayUpdater != null)
		{
			overlayUpdater.map = worldMap;
		}
	}
	
	// Update is called once per frame
	void Update()
	{
		float timeA = Time.realtimeSinceStartup;
		// 0. Update parameters
		updateParameters();

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
			if(		agent.getIsAlive() == AgentPercept.LivingState.ALIVE
			   || 	agent.getIsAlive() == AgentPercept.LivingState.UNDEAD)
			{
				agent.setLookInUse(false);
				agent.setMoveInUse(false);
				agent.getBehavior().updatePlan( worldMap.getPercepts(agent, perfectVisionRange), 1 );
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
	}
	
	private void updateParameters()
	{
		if(targetFramerate != _targetFramerate)
		{
			_targetFramerate = targetFramerate;
		}
	}

	#endregion MonoBehavior methods & helpers
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Agent Action
	private void executeAction(Agent agent, float duration)
	{
		List<Action> planList = agent.getBehavior().getCurrentPlans();
		for(int i=0; i<planList.Count; i++)
		{
			switch(planList[i].getActionType())
			{
				case Action.ActionType.STAY:
					break;
					
				case Action.ActionType.MOVE_TOWARDS:
					moveAgentTowards( agent, planList[i].getTargetPoint(), duration);
					break;
					
				case Action.ActionType.TURN_BY_DEGREES:
					turnAgentBy( agent, planList[i].getDirection());
					break;
					
				case Action.ActionType.TURN_TO_DEGREES:
					turnAgentTo( agent, planList[i].getDirection());
					break;
					
				case Action.ActionType.TURN_TOWARDS:
					turnAgentTowards( agent, planList[i].getTargetPoint());
					break;
					
				case Action.ActionType.CONVERT:
					ActionArbiter.Instance.requestAction(agent, planList[i].getTargetAgent(), ActionArbiter.ActionType.CONVERT);
					break;
			}
		}
	}
	
	// attempt to move the agent as far as possible towards target,
	// given a duration of movement.
	private void moveAgentTowards(Agent agent, Vector2 target, float duration)
	{
		float netSpeedMultiplier = duration * moveSpeed * simulationSpeed * agent.getSpeedMultiplier();
		Vector2 newPoint = agent.getLocation() + (target - agent.getLocation()).normalized * netSpeedMultiplier;
		
		if(worldMap.isValidPosition(newPoint))
		{
			agent.setLocation(newPoint);
		}
		else
		{
			// partial movement & wall-sliding go here
			Vector2 firstStop = nearestDirect(agent.getLocation(), newPoint, 0.125f);
			
			// in 2D should only have one valid dimension here.
			// and technically, could get even more accurate by calling nearestDirect again.
			// TODO: Evaluate whether it's worth using nearestDirect here.
			Vector2 xTarget = new Vector2(newPoint.x, firstStop.y);
			Vector2 yTarget = new Vector2(firstStop.x, newPoint.y);
			if(worldMap.isValidPosition(xTarget))
			{
				agent.setLocation(xTarget);
			}
			else if(worldMap.isValidPosition(yTarget))
			{
				agent.setLocation(yTarget);
			}
		}
	}
	
	private void turnAgentBy(Agent agent, float degrees)
	{
		agent.setDirection( agent.getDirection() + degrees );
	}
	
	private void turnAgentTo(Agent agent, float angle)
	{
		agent.setDirection( angle );
	}
	
	private void turnAgentTowards(Agent agent, Vector2 point)
	{
		Vector2 delta = point - agent.getLocation()/* - point*/;
		float angle = (90 - Mathf.Rad2Deg * Mathf.Atan2(delta.x, delta.y));
		
		turnAgentTo( agent, angle );
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
	
	public void setSimulationSpeed(float newSimSpeed)
	{
		simulationSpeed = newSimSpeed;
	}
	
	#endregion Getters/Setters
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Camera Helpers

	// This should probably be located in another class that manages camera
	// and screen events
	private void configureCamera()
	{
//		Camera.main.transform.position = new Vector3(worldWidth/2.0f, worldHeight/2.0f, -10.0f);
		Camera.main.transform.position = Vector3.zero;
		Camera.main.orthographicSize = worldHeight/2.0f;
	}

	#endregion Camera Helpers
	//////////////////////////////////////////////////////////////////
}
