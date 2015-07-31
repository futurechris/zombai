using System.Collections;

public class Action
{
	public enum ActionType { STAY, MOVE_TOWARDS, TURN_BY_DEGREES, TURN_TOWARDS };

	private ActionType actionType = ActionType.STAY;

	public Action(ActionType newActionType=ActionType.STAY)
	{
		actionType = newActionType;
	}
}
