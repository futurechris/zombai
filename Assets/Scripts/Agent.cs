using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour
{
	public SpriteRenderer agentSprite;

	//////////////////////////////////////////////////////////////////
	#region Agent traits

	private	System.Guid guid		= System.Guid.NewGuid();
	private Color agentColor 		= Color.cyan;
	private bool isAlive			= true;
	private AgentBehavior behavior	= new NoopBehavior();

	private Vector2 location		= Vector2.zero;
	private float direction			= 0;
	private float fieldOfView		= 0;
	private float sightRange		= 0;

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

	public bool getIsAlive()
	{
		return isAlive;
	}
	public void setIsAlive(bool newState)
	{
		isAlive = newState;
	}

	public AgentBehavior getBehavior()
	{
		return behavior;
	}
	public void setBehavior(AgentBehavior newBehavior)
	{
		behavior = newBehavior;
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
	#endregion MonoBehaviour methods
	//////////////////////////////////////////////////////////////////
}
