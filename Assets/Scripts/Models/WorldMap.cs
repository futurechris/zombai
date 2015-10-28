using UnityEngine;
using QuadTree;
using System.Collections;
using System.Collections.Generic;

public class WorldMap
{
	//////////////////////////////////////////////////////////////////
	#region Parameters & properties

	private int minimumStreetWidth 	= 5;

	private int agentTreeSplit 		= 10;
	private int agentDepthLimit 	= 15;

	private int buildingTreeSplit 	= 10;
	private int buildingDepthLimit 	= 15;

	#endregion Parameters & properties
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Bookkeeping
	
	private QuadTree<Agent> agentTree;
	private List<Agent> agents 	= new List<Agent>();
	private List<int> workUnits = new List<int>();

	// Obv. just a PH, but this is a list of the rectangular (axis-aligned) rectangular buildings
	//   filling up the world.
	private List<Rect> structures = new List<Rect>();
	private QuadTree<Rect> buildingTree;

	private List<WorldObject> worldObjects = new List<WorldObject>();
	// no need for QuadTree here just yet

	private float worldWidth 	= 0;
	private float worldHeight 	= 0;

	private bool agentStateChanged = true;
	private int livingCount 	= 0;
	private int undeadCount 	= 0;
	private int corpseCount 	= 0;
	private int survivorCount	= 0;

	private float _perceptTotalTime = 0.0f;
	private float _perceptTotalCount = 1.0f;

	#endregion Bookkeeping
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region World initialization, generation, etc.

	// Horrible. Temporary. Will refactor rendering/worldmap data-ness out soon.
	public WorldMap(int width, int height, int buildingCount)
	{
		initializeWorld(width,height,buildingCount);
	}
	
	// For now, just generates rectangular buildings
	// width/height in ... pixels?
	private void initializeWorld(float mapWidth, float mapHeight, int numBuildings)
	{
		worldWidth = mapWidth;
		worldHeight = mapHeight;

		agentTree = new QuadTree<Agent>(agentTreeSplit, agentDepthLimit, 0, 0, worldWidth, worldHeight);
		buildingTree = new QuadTree<Rect>(buildingTreeSplit, buildingDepthLimit, 0, 0, worldWidth, worldHeight);

		// Probably get something closer to a city by carving streets out 
		//   rather than trying to fill with boxes.
		float streetWidth = Mathf.Max(minimumStreetWidth, Mathf.Floor(worldWidth * 0.01f));

		// But for current testing, just making some random rectangles.
		for(int i=0; i<numBuildings; i++)
		{
			float xDim = Random.Range(3.0f*streetWidth, 10.0f*streetWidth);
			float yDim = Random.Range(3.0f*streetWidth, 10.0f*streetWidth);

			float xPos = Random.Range(0.0f, worldWidth-xDim);
			float yPos = Random.Range(0.0f, worldHeight-yDim);

			// For now just letting these be redundant
			Quad tempQuad = new Quad(xPos, yPos, xPos+xDim, yPos+yDim);
			Rect tempRect = new Rect(xPos, yPos, xDim, 		yDim);

			buildingTree.Insert(tempRect, ref tempQuad);
			structures.Add(tempRect);
		}
	}

	// Create agents, give them behaviors and locations, turn them loose.
	public List<Agent> populateWorld(int numLiving, int numCorpses, int numUndead)
	{
		Agent tempAgent;

		AgentBehavior tempAgentBehavior;
		FallThroughBehavior tempFTB;

		List<Agent> newAgents = new List<Agent>();

		for(int i=0; i<numLiving; i++)
		{
			tempAgent = new Agent(Agent.AgentType.HUMAN);
			tempAgent.Location = (getValidAgentPosition());

			agents.Add(tempAgent);
			newAgents.Add(tempAgent);

			livingCount++;
		}

		for(int i=0; i<numUndead; i++)
		{	
			tempAgent = new Agent(Agent.AgentType.ZOMBIE);
			tempAgent.Location = (getValidAgentPosition());

			agents.Add(tempAgent);
			newAgents.Add(tempAgent);

			undeadCount++;
		}

		for(int i=0; i<numCorpses; i++)
		{	
			tempAgent = new Agent(Agent.AgentType.CORPSE);
			tempAgent.Location = (getValidAgentPosition());

			agents.Add(tempAgent);
			newAgents.Add(tempAgent);

			corpseCount++;
		}
		updateAgentTree();
		return newAgents;
	}

	public List<Agent> spawnOne(Agent.AgentType type)
	{
		return spawnOne(type, getValidAgentPosition());
	}

	public List<Agent> spawnOne(Agent.AgentType type, Vector2 pos)
	{
		List<Agent> newAgent = new List<Agent>();

		// for now just ignore spawns in buildings.
		if(!isValidPosition(pos))
		{
			return newAgent;
		}

		Agent tempAgent;
		tempAgent = new Agent(type);
		tempAgent.Location = (pos);

		agents.Add(tempAgent);
		newAgent.Add(tempAgent);

		switch(type)
		{
			case Agent.AgentType.HUMAN:
				livingCount++;
				break;
			case Agent.AgentType.HUMAN_PLAYER:
				livingCount++;
				break;
			case Agent.AgentType.ZOMBIE:
				undeadCount++;
				break;
			case Agent.AgentType.ZOMBIE_PLAYER:
				undeadCount++;
				break;
			case Agent.AgentType.CORPSE:
				corpseCount++;
				break;
			default:
				break;
		}
		updateAgentTree();
		return newAgent;
	}

	public List<WorldObject> placeWorldObject(WorldObject.ObjectType objType)
	{
		// TODO: Create a different function for computing valid places for a given object type
		return placeWorldObject(objType, getValidAgentPosition());
	}

	public List<WorldObject> placeWorldObject(WorldObject.ObjectType objType, Vector2 pos)
	{
		List<WorldObject> objList = new List<WorldObject>();

		if(!isValidPosition(pos))
		{
			return objList;
		}

		WorldObject tempObject;
		tempObject = new WorldObject(objType);
		tempObject.Location = pos;
		
		worldObjects.Add(tempObject);
		objList.Add(tempObject);

		return objList;
	}

	private Vector2 getValidAgentPosition()
	{
		Vector2 testPos = new Vector2(Random.Range(0.0f,worldWidth), Random.Range(0.0f,worldHeight));
		bool valid = false;
		int attempts = 0;
		int maxAttempts = 100; // heh
		while(!valid && (attempts < maxAttempts)){
			testPos = new Vector2(Random.Range(0.0f,worldWidth), Random.Range(0.0f,worldHeight));
			valid = isValidPosition( testPos );
			attempts++;
		}
		if(attempts >= maxAttempts)
		{
			Debug.LogError("Exceeded maximum attempts");
		}
		return testPos;
	}

	public bool isValidPosition(Vector2 testPos)
	{
		if(		testPos.x < 0
		   || 	testPos.x >= worldWidth
		   ||	testPos.y < 0
		   ||	testPos.y >= worldHeight)
		{
			return false;
		}

		List<Rect> collisions = new List<Rect>();
		if (buildingTree.SearchPoint(testPos.x, testPos.y, ref collisions))
		{
			return false;
		}

		return true;
	}

	#endregion World initialization, generation, etc.
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Agent-World Interactions
	
	// Agent perceives world through this function alone.
	public void getPercepts(Agent agent, float perfectVisionRange)
	{
		float startTime = Time.realtimeSinceStartup;
		agent.resetPercepts();

		AgentPercept tempPercept;

		List<Agent> nearbyAgents = new List<Agent>();
		Vector2 loc = agent.Location;
		float radius = agent.SightRange;
		Quad visibleArea = new Quad( loc.x-radius, loc.y-radius, loc.x+radius, loc.y+radius );

		agentTree.SearchArea(visibleArea, ref nearbyAgents);

		for(int i=0; i<nearbyAgents.Count; i++)
		{
			// For now not doing self-aware. No Skynet on *my* watch!
			if(agent.Guid == nearbyAgents[i].Guid)
			{
				continue;
			}
			if(canPerceivePosition(agent, nearbyAgents[i].Location, perfectVisionRange))
			{
				tempPercept 		= agent.getNextPercept();// new AgentPercept();
				tempPercept.type 	= AgentPercept.PerceptType.AGENT;
				tempPercept.locOne 	= nearbyAgents[i].Location;
				tempPercept.living 	= nearbyAgents[i].LivingState;
				tempPercept.facingDirection = nearbyAgents[i].Direction;

				// TODO: Agent object should not be part of percept, fix!
				// See AgentPercept.perceivedAgent for more.
				tempPercept.perceivedAgent = nearbyAgents[i];

				tempPercept 		= null;
			}
		}

		List<Rect> nearbyStructures = new List<Rect>();
		buildingTree.SearchArea(visibleArea, ref nearbyStructures);

		List<AgentPercept> structureList = null;
		for(int i=0; i<nearbyStructures.Count; i++)
		{
			perceiveStructure(agent, nearbyStructures[i]);
		}

		// TODO: Work out a smarter way to handle WorldObjects than point-based
		if(agent.Type == Agent.AgentType.HUMAN || agent.Type == Agent.AgentType.HUMAN_PLAYER)
		{
			for(int wo=0; wo<worldObjects.Count; wo++)
			{
				if(canPerceivePosition(agent, worldObjects[wo].Location, perfectVisionRange))
				{
					tempPercept = agent.getNextPercept();// new AgentPercept();
					tempPercept.type = AgentPercept.PerceptType.EXTRACT;
					tempPercept.locOne = worldObjects[wo].Location;
					tempPercept.living = AgentPercept.LivingState.INANIMATE;

					tempPercept = null;
				}
			}
		}
		_perceptTotalTime += (Time.realtimeSinceStartup-startTime);
		_perceptTotalCount++;
	}

	public bool canPerceivePosition(Agent who, Vector2 where, float perfectVisionRange)
	{
		// first, distance between who and where: <= vision distance?
		Vector2 delta = where - who.Location;
		
		if( delta.magnitude > who.SightRange)
		{
			return false;
		}

		// short-range hack
		if( delta.magnitude < perfectVisionRange)
		{
			return true;
		}

		// second: calculate angle between who and where, compare to
		//		   agent's facing, see if it's within FOV
		float whoToWhereAngle = 90 - Mathf.Rad2Deg * Mathf.Atan2(delta.x, delta.y);
		float shortest = Mathf.Abs(Mathf.DeltaAngle(whoToWhereAngle, who.Direction));

		if(shortest <= (who.FieldOfView/2.0f))
		{
			return true;
		}

		return false;
	}

	public void perceiveStructure(Agent who, Rect what)
	{
		// TODO: Calculate chunks of walls that are visible, set those for the agent
		return;
	}

	public void extractAgent(Agent who)
	{
		agents.Remove(who);
		updateAgentTree();
		who.extractSuccess();
		survivorCount++;
	}


	#endregion Agent-World Interactions
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Tree Helpers
	public void updateAgentTree()
	{
		// QuadTree.cs suggests clearing the tree every frame and rebuilding,
		// for cases like this where there are a lot of moving objects. Will
		// try that for now.
		agentTree.Clear();
		for(int i=0; i<agents.Count; i++)
		{
			agentTree.Insert(agents[i],agents[i].Location.x, agents[i].Location.y, 0,0);
		}
	}

	#endregion Tree Helpers
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Getters/Setters

	public List<Agent> getAgents()
	{
		return agents;
	}

	public List<Rect> getStructures()
	{
		return structures;
	}

	public int getLivingCount()
	{
		return livingCount;
	}

	public int getUndeadCount()
	{
		return undeadCount;
	}

	public int getCorpseCount()
	{
		return corpseCount;
	}

	public int getSurvivorCount()
	{
		return survivorCount;
	}

	public void agentCountChange(int deltaLiving, int deltaUndead, int deltaCorpse)
	{
		livingCount += deltaLiving;
		undeadCount += deltaUndead;
		corpseCount += deltaCorpse;
	}

	public float getWidth()
	{
		return worldWidth;
	}

	public float getHeight()
	{
		return worldHeight;
	}

	public float expectedPerceptCost()
	{
		return _perceptTotalTime / _perceptTotalCount;
	}

	#endregion Getters/Setters
	//////////////////////////////////////////////////////////////////
}
