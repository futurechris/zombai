using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UserInput : MonoBehaviour, IPointerClickHandler {

//	void Start(){}
//	void Update(){}

	// Mutually exclusive for now, priority is in the following order:
	// Left click for human,
	// Right for zombie,
	// Middle for corpse
	public void OnPointerClick(PointerEventData e)
	{
		if(e.button == PointerEventData.InputButton.Left)
		{
			FindObjectOfType<AgentDirector>().spawnAgent(Agent.AgentType.HUMAN, Camera.main.ScreenToWorldPoint(e.position));
		}
		else if(e.button == PointerEventData.InputButton.Right)
		{
			FindObjectOfType<AgentDirector>().spawnAgent(Agent.AgentType.ZOMBIE, Camera.main.ScreenToWorldPoint(e.position));
		}
		else if(e.button == PointerEventData.InputButton.Middle)
		{
			FindObjectOfType<AgentDirector>().spawnAgent(Agent.AgentType.CORPSE, Camera.main.ScreenToWorldPoint(e.position));
		}
	}
}
