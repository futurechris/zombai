using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControlBehavior : AgentBehavior
{
	// Get keyboard/touch input and convert it into a "plan"
	public override bool executePlanUpdate()
	{
		if(_myself.MoveInUse)
		{
			return false;
		}
		
		Action newMoveAction = null;
		Action newLookAction = null;

		// unlike other behaviors, for responsiveness, this should always clear immediately
		this.currentPlans.Clear();

		Vector2 lookVector = Vector2.zero;
		Vector2 moveVector = Vector2.zero;

		if(!_myself.LookInUse)
		{
			float hLookAxis = Input.GetAxis("LookHorizontal");
			float vLookAxis = Input.GetAxis("LookVertical");

			if(hLookAxis != 0 || vLookAxis != 0)
			{
				lookVector = new Vector2(hLookAxis, vLookAxis);
			}
		}

		if(!_myself.MoveInUse)
		{
			float hAxis = Input.GetAxis("Horizontal");
			float vAxis = Input.GetAxis("Vertical");

			if(hAxis != 0 || vAxis != 0)
			{
				moveVector = new Vector2(hAxis*200.0f, vAxis*200.0f);
			}
		}

		if(moveVector != Vector2.zero)
		{
			newMoveAction = new Action(Action.ActionType.MOVE_TOWARDS);
			newMoveAction.TargetPoint = (_myself.Location + moveVector);
			this.currentPlans.Add(newMoveAction);
		}
		else
		{
			newMoveAction = new Action(Action.ActionType.STAY);
			this.currentPlans.Add(newMoveAction);
		}
		if(lookVector != Vector2.zero)
		{
			newLookAction = new Action(Action.ActionType.TURN_TO_DEGREES);
			Vector2 netVector = lookVector;
			float angle = 90.0f - Mathf.Rad2Deg * Mathf.Atan2(netVector.x, netVector.y);

			newLookAction.Direction = (angle);
			this.currentPlans.Add(newLookAction);
		}
		else if(moveVector != Vector2.zero)
		{
			// if no specific look input, turn in the direction we're moving
			// should maybe merge this block with the lookVector block above.
			newLookAction = new Action(Action.ActionType.TURN_TO_DEGREES);
			Vector2 netVector = moveVector;
			float angle = 90.0f - Mathf.Rad2Deg * Mathf.Atan2(netVector.x, netVector.y);
			
			newLookAction.Direction = (angle);
			this.currentPlans.Add(newLookAction);
		}

		return (currentPlans.Count > 0);
	}
}
