using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Agent : MonoBehaviour
{
	// Regularly used agent types to speed up initialization
	// and enable conversion between agent types
	public enum AgentType { CUSTOM, HUMAN, ZOMBIE, CORPSE };

	public SpriteRenderer agentSprite;
	public Image fovImage;

	//////////////////////////////////////////////////////////////////
	#region Agent traits
	private	System.Guid guid				= System.Guid.NewGuid();
	private bool needsInitialization		= true;

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
	#region MonoBehaviour methods
	
	void Start ()
	{
		if(needsInitialization)
		{
			configureDefault();
			needsInitialization = false;
		}

		recalculateFOVImage();
	}
	
	void Update ()
	{	
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
		setAgentColor(Color.cyan);
		setIsAlive(AgentPercept.LivingState.DEAD);
		setSightRange(0.0f);
		setFieldOfView(0.0f);
		setDirection(Random.Range(-180.0f, 180.0f));

		setBehavior(new NoopBehavior());
	}

	private void configureAsHuman()
	{
		setAgentColor(Color.green);
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
		setAgentColor(Color.magenta);
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
		setAgentColor(Color.cyan);

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
	#region Helpers

	private void recalculateFOVImage()
	{
		// For now, just immediately setting this. Later would be nice to smooth the transition.
		// That goes for the actual vision cone as well - instant turning is a little strange.

		// The prefab for agents sets the 'fill origin' as being to the right. Fill is clockwise.
		// % to fill is easy, just the percentage of a circle represented by fieldOfView.
		fovImage.fillAmount = fieldOfView / 360.0f;

		// angle then is direction-half that?
		fovImage.rectTransform.rotation = Quaternion.identity; // reset rotation
		fovImage.rectTransform.Rotate(0.0f, 0.0f, (direction + (fieldOfView/2.0f)));
	}

	#endregion Helpers
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Getter/Setters
	// Many of these may be fine as properties, but I'm expecting some/all to grow complex.

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
		agentSprite.color = agentColor;
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
		this.transform.localPosition = newLocation;
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
		recalculateFOVImage();
	}

	public float getFieldOfView()
	{
		return fieldOfView;
	}
	public void setFieldOfView(float newFieldOfView)
	{
		fieldOfView = newFieldOfView;
		recalculateFOVImage();
	}

	public float getSightRange()
	{
		return sightRange;
	}
	public void setSightRange(float newRange)
	{
		float multiplier = newRange/16f; // magic number, but accurate.
		sightRange = newRange;
		fovImage.rectTransform.localScale = new Vector3(multiplier, multiplier, 1.0f);
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
