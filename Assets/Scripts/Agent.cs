using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour
{
	public SpriteRenderer agentSprite;

	//////////////////////////////////////////////////////////////////
	#region Agent traits

	private	System.Guid guid				= System.Guid.NewGuid();
	private Color agentColor 				= Color.cyan;
	private AgentPercept.LivingState living	= AgentPercept.LivingState.ALIVE;
	private AgentBehavior behavior			= new NoopBehavior();

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
	}
	
	void Update ()
	{	
	}

	#endregion MonoBehaviour methods
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
	}

	public float getFieldOfView()
	{
		return fieldOfView;
	}
	public void setFieldOfView(float newFieldOfView)
	{
		fieldOfView = newFieldOfView;
	}

	public float getSightRange()
	{
		return sightRange;
	}
	public void setSightRange(float newRange)
	{
		sightRange = newRange;
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
