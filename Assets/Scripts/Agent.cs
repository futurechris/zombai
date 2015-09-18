using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Agent
{
	//////////////////////////////////////////////////////////////////
	#region Agent Types/Defaults
	// Regularly used agent types to speed up initialization
	// and enable conversion between agent types
	public enum AgentType { CUSTOM, HUMAN, ZOMBIE, CORPSE, HUMAN_PLAYER, ZOMBIE_PLAYER };

	private static Color humanColor = Color.green;
	private static Color zombieColor = Color.red;
	private static Color corpseColor = Color.cyan;
	private static Color humanPlayerColor = Color.yellow;
	private static Color zombiePlayerColor = Color.magenta;

	#endregion Agent Types/Defaults
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Bookkeeping
	private	System.Guid guid				= System.Guid.NewGuid();
	private bool needsInitialization		= true;

	private AgentRenderer myRenderer;

	#endregion Bookkeeping
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Agent traits
	// Several of these wouldn't "const" correctly, so for now non-DRYly
	// duplicating the 'defaults' down in configureDefault()
	private AgentPercept.LivingState living	= AgentPercept.LivingState.ALIVE;
	private AgentType agentType				= AgentType.HUMAN;
	private AgentBehavior behavior			= new NoopBehavior();
	private Color agentColor				= Color.cyan;

	private Vector2 location				= Vector2.zero;
	private float direction					= 0.0f;
	private float fieldOfView				= 0.0f;
	private float sightRange				= 0.0f;
	private float convertRange				= 0.0f;

	private float speedMultiplier			= 1.0f;
	private float turnSpeedMultiplier		= 1.0f;

	// This is *definitely* going to need iteration. :P
	private bool moveInUse					= false;
	private bool lookInUse					= false;

	#endregion Agent traits
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Constructor/init

	public Agent(): this(AgentType.HUMAN){}

	public Agent(AgentType newType)
	{
		configureAs(newType,true);
		if(myRenderer != null)
		{
			myRenderer.recalculateFOVImage();
		}
	}

	#endregion MonoBehaviour methods
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Agent type definitions

	// since we don't seem to have defaults in whatever C# I'm getting via Mono...
	public void configureAs(AgentType newType)
	{
		configureAs(newType, false);
	}

	public void configureAs(AgentType newType, bool resetFirst)
	{
		if(resetFirst)
		{
			configureDefault();
		}

		needsInitialization = false;

		switch(newType)
		{
			case AgentType.CUSTOM:
				configureAsCustom();
				break;
			case AgentType.CORPSE:
				configureAsCorpse();
				break;
			case AgentType.HUMAN:
				configureAsHuman();
				break;
			case AgentType.HUMAN_PLAYER:
				configureAsPlayableHuman();
				break;
			case AgentType.ZOMBIE:
				configureAsZombie();
				break;
			case AgentType.ZOMBIE_PLAYER:
				configureAsPlayableZombie();
				break;
		}
	}

	private void configureAsCustom()
	{
		setAgentType(AgentType.CUSTOM);
	}

	private void configureAsCorpse()
	{
		setAgentColor(corpseColor);
		setIsAlive(AgentPercept.LivingState.DEAD);
		setAgentType(AgentType.CORPSE);

		setSightRange(0.0f);
		setFieldOfView(0.0f);
		setDirection(Random.Range(-180.0f, 180.0f));

		setBehavior(new NoopBehavior());
	}

	private void configureAsHuman()
	{
		setAgentColor(humanColor);
		setIsAlive(AgentPercept.LivingState.ALIVE);
		setAgentType(AgentType.HUMAN);
		setSightRange(36.0f);
		setFieldOfView(180.0f); // roughly full range of vision
		setDirection(Random.Range(-180.0f, 180.0f));
		setSpeedMultiplier(1.15f);
		
		FallThroughBehavior tempFTB = new FallThroughBehavior();
		tempFTB.addBehavior( new FleeBehavior() );
		tempFTB.addBehavior( new WanderBehavior() );
		tempFTB.addBehavior( new RandomLookBehavior() );
		
		setBehavior(tempFTB);
	}

	private void configureAsPlayableHuman()
	{
		setAgentColor(humanPlayerColor);
		setIsAlive(AgentPercept.LivingState.ALIVE);
		setAgentType(AgentType.HUMAN_PLAYER);
		setSightRange(49.0f); // longer range to help with reaction time
		setFieldOfView(135.0f); // slightly expanded vision range, same
		setDirection(Random.Range(-180.0f, 180.0f));
		setSpeedMultiplier(1.25f); // and slightly faster
		setTurnSpeedMultiplier(2.0f);
		
		FallThroughBehavior tempFTB = new FallThroughBehavior();
		tempFTB.addBehavior( new PlayerControlBehavior() );
		tempFTB.addBehavior( new RandomLookBehavior() ); // leaving this in for now, useful.
		
		setBehavior(tempFTB);
	}

	private void configureAsZombie()
	{
		setAgentColor(zombieColor);
		setIsAlive(AgentPercept.LivingState.UNDEAD);
		setAgentType(AgentType.ZOMBIE);
		setSightRange(25.0f);
		setFieldOfView(120.0f);
		setDirection(Random.Range(-180.0f, 180.0f));
		setSpeedMultiplier(1.0f);
		setConvertRange(2.0f);
		
		FallThroughBehavior tempFTB = new FallThroughBehavior();
		// General Zombie:
		tempFTB.addBehavior( new ZombifyBehavior() );
		tempFTB.addBehavior( new PursueBehavior() );
		tempFTB.addBehavior( new NecrophageBehavior() );
		tempFTB.addBehavior( new WanderBehavior() );
		tempFTB.addBehavior( new RandomLookBehavior() );

		// Boid Zombies
//		tempFTB.addBehavior( new BoidsBehavior());
//		tempFTB.addBehavior( new WanderBehavior() );
//		setFieldOfView(351.0f); // add a "tail wedge" to show direction, until something better is in
		
		setBehavior(tempFTB);
	}

	private void configureAsPlayableZombie()
	{
		setAgentColor(zombieColor);
		setIsAlive(AgentPercept.LivingState.UNDEAD);
		setAgentType(AgentType.ZOMBIE_PLAYER);
		setSightRange(36.0f); // slightly expanded, to account for reaction time and lack of overwhelming numbers
		setFieldOfView(135.0f); // super zombie
		setDirection(Random.Range(-180.0f, 180.0f));
		setSpeedMultiplier(1.15f); // same as a human
		setTurnSpeedMultiplier(2.0f); // just a lot more responsive this way
		setConvertRange(5.0f); // significant boost, to help account for lag/control timing
		
		FallThroughBehavior tempFTB = new FallThroughBehavior();
		tempFTB.addBehavior( new ZombifyBehavior() );
		tempFTB.addBehavior( new PlayerControlBehavior() );
		tempFTB.addBehavior( new RandomLookBehavior() );
		
		setBehavior(tempFTB);
	}
	


	private void configureDefault()
	{
		setIsAlive(AgentPercept.LivingState.ALIVE);
		setAgentType(AgentType.HUMAN);
		setBehavior(new NoopBehavior());
		setAgentColor(humanColor);

		setDirection(0.0f);
		setFieldOfView(0.0f);
		setSightRange(0.0f);
		setConvertRange(0.0f);

		setSpeedMultiplier(1.0f);
		setTurnSpeedMultiplier(1.0f);

		setMoveInUse(false);
		setLookInUse(false);
	}

	#endregion Agent type definitions
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Getter/Setters
	// Many of these may be fine as properties, but I'm expecting some/all to grow complex.

	public void setRenderer(AgentRenderer newRenderer)
	{
		myRenderer = newRenderer;
	}

	public System.Guid getGuid()
	{
		return guid;
	}

	public Color getAgentColor()
	{
		return agentColor;
	}
	public void setAgentColor(Color newColor)
	{
		agentColor = newColor;
		if(myRenderer != null)
		{
			myRenderer.updateColor();
		}
	}

	public AgentPercept.LivingState getIsAlive()
	{
		return living;
	}
	public void setIsAlive(AgentPercept.LivingState newState)
	{
		living = newState;
	}

	public AgentType getAgentType()
	{
		return agentType;
	}
	public void setAgentType(AgentType newType)
	{
		agentType = newType;
	}

	public AgentBehavior getBehavior()
	{
		return behavior;
	}
	public void setBehavior(AgentBehavior newBehavior)
	{
		behavior = newBehavior;
		newBehavior.setAgent(this);
	}

	public Vector2 getLocation()
	{
		return location;
	}
	public void setLocation(Vector2 newLocation)
	{
		location = newLocation;
		if(myRenderer != null)
		{
			myRenderer.updateLocation();
		}
	}

	public float getDirection()
	{
		return direction;
	}
	public void setDirection(float newDirection)
	{
		direction = newDirection;
		if(direction < 0)
		{
			direction += 360;
		}
		if(direction >= 360)
		{
			direction -= 360;
		}
		if(myRenderer != null)
		{
			myRenderer.recalculateFOVImage();
		}

	}

	public float getFieldOfView()
	{
		return fieldOfView;
	}
	public void setFieldOfView(float newFieldOfView)
	{
		fieldOfView = newFieldOfView;
		if(myRenderer != null)
		{
			myRenderer.recalculateFOVImage();
		}
	}

	public float getSightRange()
	{
		return sightRange;
	}

	public void setSightRange(float newRange)
	{
		float multiplier = newRange/16f; // magic number, but accurate.
		sightRange = newRange;
		if(myRenderer != null)
		{
			myRenderer.updateFOVScale();
		}
	}

	public float getSpeedMultiplier()
	{
		return speedMultiplier;
	}
	public void setSpeedMultiplier(float newMult)
	{
		speedMultiplier = newMult;
	}

	public float getTurnSpeedMultipler()
	{
		return turnSpeedMultiplier;
	}
	public void setTurnSpeedMultiplier(float newMult)
	{
		turnSpeedMultiplier = newMult;
	}

	public bool getMoveInUse()
	{
		return moveInUse;
	}
	public void setMoveInUse(bool newUseState)
	{
		moveInUse = newUseState;
	}

	public bool getLookInUse()
	{
		return lookInUse;
	}
	public void setLookInUse(bool newUseState)
	{
		lookInUse = newUseState;
	}

	public float getConvertRange()
	{
		return convertRange;
	}
	public void setConvertRange(float newRange)
	{
		convertRange = newRange;
	}
	#endregion MonoBehaviour methods
	//////////////////////////////////////////////////////////////////
}
