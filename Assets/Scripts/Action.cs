using UnityEngine;
using System.Collections;

public class Action
{
	public enum ActionType { STAY, MOVE_TOWARDS, TURN_TO_DEGREES, TURN_BY_DEGREES, TURN_TOWARDS };

	private ActionType actionType = ActionType.STAY;

	private Vector2 targetPoint = Vector2.zero;
	private float   direction	= 0.0f;

	public Action(ActionType newActionType=ActionType.STAY)
	{
		actionType = newActionType;
	}

	public ActionType getActionType()
	{
		return actionType;
	}

	public bool getUsingMove()
	{
		switch(actionType)
		{
			case ActionType.STAY:
				return true;
			case ActionType.MOVE_TOWARDS:
				return true;
			default:
				return false;
		}
	}

	public bool getUsingLook()
	{
		switch(actionType)
		{
			case ActionType.TURN_BY_DEGREES:
				return true;
			case ActionType.TURN_TO_DEGREES:
				return true;
			case ActionType.TURN_TOWARDS:
				return true;
			default:
				return false;
		}
	}

	//////////////////////////////////////////////////////////////////
	#region Action parameter settings
	// Each action will use whichever of these are appropriate

	public void setTargetPoint(Vector2 newTarget)
	{
		targetPoint = newTarget;
	}

	public Vector2 getTargetPoint()
	{
		return targetPoint;
	}

	public void setDirection(float radians)
	{
		direction = radians;
		// clean mod
		if(direction < 0)
		{
			direction += 2.0f*Mathf.PI;
		}
		if(direction >= 2.0f*Mathf.PI)
		{
			direction -= 2.0f*Mathf.PI;
		}
	}

	public float getDirection()
	{
		return direction;
	}

	#endregion Action parameter settings
	//////////////////////////////////////////////////////////////////
}
