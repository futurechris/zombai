using UnityEngine;
using QuadTree;
using System.Collections;
using System.Collections.Generic;

public class WorldMap
{
	//////////////////////////////////////////////////////////////////
	#region Parameters & properties

	private int minimumStreetWidth 	= 5;

	private int agentTreeSplit 		= 50;
	private int agentDepthLimit 	= 15;

	private int buildingTreeSplit 	= 100;
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

	private float worldWidth 	= 0;
	private float worldHeight 	= 0;

	private bool agentStateChanged = true;
	private int livingCount 	= 0;
	private int undeadCount 	= 0;
	private int corpseCount 	= 0;

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
			tempAgent.setLocation(getValidAgentPosition());

			agents.Add(tempAgent);
			newAgents.Add(tempAgent);

			livingCount++;
		}

		for(int i=0; i<numUndead; i++)
		{	
			tempAgent = new Agent(Agent.AgentType.ZOMBIE);
			tempAgent.setLocation(getValidAgentPosition());

			agents.Add(tempAgent);
			newAgents.Add(tempAgent);

			undeadCount++;
		}

		for(int i=0; i<numCorpses; i++)
		{	
			tempAgent = new Agent(Agent.AgentType.CORPSE);
			tempAgent.setLocation(getValidAgentPosition());

			agents.Add(tempAgent);
			newAgents.Add(tempAgent);

			corpseCount++;
		}
		updateAgentTree();
		return newAgents;
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
	public List<AgentPercept> getPercepts(Agent agent, float perfectVisionRange)
	{
		List<AgentPercept> apList = new List<AgentPercept>();

		AgentPercept tempPercept;

		List<Agent> nearbyAgents = new List<Agent>();
		Vector2 loc = agent.getLocation();
		float radius = agent.getSightRange();
		Quad visibleArea = new Quad( loc.x-radius, loc.y-radius, loc.x+radius, loc.y+radius );

		agentTree.SearchArea(visibleArea, ref nearbyAgents);

		for(int i=0; i<nearbyAgents.Count; i++)
		{
			// For now not doing self-aware. No Skynet on *my* watch!
			if(agent.getGuid() == nearbyAgents[i].getGuid())
			{
				continue;
			}
			if(canPerceivePosition(agent, nearbyAgents[i].getLocation(), perfectVisionRange))
			{
				tempPercept 		= new AgentPercept();
				tempPercept.type 	= AgentPercept.PerceptType.AGENT;
				tempPercept.locOne 	= nearbyAgents[i].getLocation();
				tempPercept.living 	= nearbyAgents[i].getIsAlive();
				tempPercept.facingDirection = nearbyAgents[i].getDirection();

				// TODO: Agent object should not be part of percept, fix!
				// See AgentPercept.perceivedAgent for more.
				tempPercept.perceivedAgent = nearbyAgents[i];

				apList.Add(tempPercept);
				tempPercept 		= null;
			}
		}

		List<Rect> nearbyStructures = new List<Rect>();
		buildingTree.SearchArea(visibleArea, ref nearbyStructures);

		List<AgentPercept> structureList = null;
		for(int i=0; i<nearbyStructures.Count; i++)
		{
			structureList = perceiveStructure(agent, nearbyStructures[i]);
			if(structureList != null)
			{
				apList.AddRange(structureList);
			}
			structureList = null;
		}

		return apList;
	}

	public bool canPerceivePosition(Agent who, Vector2 where, float perfectVisionRange)
	{
		// first, distance between who and where: <= vision distance?
		Vector2 delta = where - who.getLocation();
		
		if( delta.magnitude > who.getSightRange())
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
		float shortest = Mathf.Abs(Mathf.DeltaAngle(whoToWhereAngle, who.getDirection()));

		if(shortest <= (who.getFieldOfView()/2.0f))
		{
			return true;
		}

		return false;
	}

	public List<AgentPercept> perceiveStructure(Agent who, Rect what)
	{
		// TODO: Calculate chunks of walls that are visible, return those.
		return null;
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
			agentTree.Insert(agents[i],agents[i].getLocation().x, agents[i].getLocation().y, 0,0);
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

	#endregion Getters/Setters
	//////////////////////////////////////////////////////////////////
}
