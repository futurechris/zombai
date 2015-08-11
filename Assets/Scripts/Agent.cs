using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Agent : MonoBehaviour
{
	public SpriteRenderer agentSprite;
	public Image fovImage;

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
		recalculateFOVImage();
	}
	
	void Update ()
	{	
	}

	#endregion MonoBehaviour methods
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
