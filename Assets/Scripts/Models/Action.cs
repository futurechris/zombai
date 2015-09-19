using UnityEngine;
using System.Collections;

public class Action
{
	public enum ActionType { STAY, MOVE_TOWARDS, TURN_TO_DEGREES, TURN_TOWARDS,
							 CONVERT, EXTRACT };

	//////////////////////////////////////////////////////////////
	#region Properties (Parameters)
	// Each action will use whichever of these are appropriate

	[SerializeField]
	private ActionType _type = ActionType.STAY;
	public ActionType Type { get { return _type; } set { _type = value; } }
	
	[SerializeField]
	private Vector2 _targetPoint = Vector2.zero;
	public Vector2 TargetPoint { get { return _targetPoint; } set { _targetPoint = value; } }

	[SerializeField]
	private float   _direction	= 0.0f;
	public float Direction
	{
		get { return _direction; } 
		set { 
			_direction = value;
			// clean mod
			if(_direction < 0)
			{
				_direction += 360.0f;
			}
			if(_direction >= 360.0f)
			{
				_direction -= 360.0f;
			}
		}
	}

	[SerializeField]
	private Agent	_targetAgent = null;
	public Agent TargetAgent { get { return _targetAgent; } set { _targetAgent = value; } }

	#endregion Properties (Parameters)
	//////////////////////////////////////////////////////////////

	public Action(ActionType newActionType=ActionType.STAY)
	{
		Type = newActionType;
	}

	public bool getUsingMove()
	{
		switch(Type)
		{
			case ActionType.STAY:
				return true;
			case ActionType.MOVE_TOWARDS:
				return true;
			case ActionType.CONVERT:
				return false; // forcing them to stop moving would make convert much less reliable
			case ActionType.EXTRACT:
				return true; // basically a movement action, and extract point shouldn't be a moving target
			default:
				return false;
		}
	}

	public bool getUsingLook()
	{
		switch(Type)
		{
			case ActionType.TURN_TO_DEGREES:
				return true;
			case ActionType.TURN_TOWARDS:
				return true;
			default:
				return false;
		}
	}
}
