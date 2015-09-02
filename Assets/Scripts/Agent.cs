using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Agent
{
	//////////////////////////////////////////////////////////////////
	#region Agent Types/Defaults
	// Regularly used agent types to speed up initialization
	// and enable conversion between agent types
	public enum AgentType { CUSTOM, HUMAN, ZOMBIE, CORPSE };

	private static Color humanColor = Color.green;
	private static Color zombieColor = Color.red;
	private static Color corpseColor = Color.cyan;

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
	private AgentBehavior behavior			= new NoopBehavior();
	private Color agentColor				= Color.cyan;

	private Vector2 location				= Vector2.zero;
	private float direction					= 0.0f;
	private float fieldOfView				= 0.0f;
	private float sightRange				= 0.0f;

	private float speedMultiplier			= 1.0f;

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
			case AgentType.ZOMBIE:
				configureAsZombie();
				break;
		}
	}

	private void configureAsCustom()
	{
		// for now, nothing?
	}

	private void configureAsCorpse()
	{
		setAgentColor(corpseColor);
		setIsAlive(AgentPercept.LivingState.DEAD);
		setSightRange(0.0f);
		setFieldOfView(0.0f);
		setDirection(Random.Range(-180.0f, 180.0f));

		setBehavior(new NoopBehavior());
	}

	private void configureAsHuman()
	{
		setAgentColor(humanColor);
		setIsAlive(AgentPercept.LivingState.ALIVE);
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

	private void configureAsZombie()
	{
		setAgentColor(zombieColor);
		setIsAlive(AgentPercept.LivingState.UNDEAD);
		setSightRange(25.0f);
		setFieldOfView(120.0f);
		setDirection(Random.Range(-180.0f, 180.0f));
		setSpeedMultiplier(1.0f);
		
		FallThroughBehavior tempFTB = new FallThroughBehavior();
		tempFTB.addBehavior( new ZombifyBehavior() );
		tempFTB.addBehavior( new PursueBehavior() );
		tempFTB.addBehavior( new NecrophageBehavior() );
		tempFTB.addBehavior( new WanderBehavior() );
		tempFTB.addBehavior( new RandomLookBehavior() );

		
		setBehavior(tempFTB);
	}

	private void configureDefault()
	{
		setIsAlive(AgentPercept.LivingState.ALIVE);
		setBehavior(new NoopBehavior());
		setAgentColor(humanColor);

		setDirection(0.0f);
		setFieldOfView(0.0f);
		setSightRange(0.0f);
		
		setSpeedMultiplier(1.0f);

		setMoveInUse(false);
		setLookInUse(false);

		// Not sure if this one should be part of defaults...
		// The intended use of configureDefault() is to remove agent
		// customizations that may no longer apply. E.g., if some human is
		// "well-rested" when they die, they might have a higher speed multiplier.
		// That doesn't necessarily carry through zombification, so we would want
		// to remove it.
		// But that doesn't mean they need to be teleported to Vector2.zero.
//		setLocation(Vector2.zero);

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

	#endregion MonoBehaviour methods
	//////////////////////////////////////////////////////////////////
}
