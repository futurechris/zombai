﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AgentRenderer : MonoBehaviour {

	//////////////////////////////////////////////////////////////////
	#region Parameters & properties

	public Agent agent;
	public SpriteRenderer agentSprite;
	public Image fovImage;

	private static float fovMultiplier = 16.0f;

	#endregion Parameters & properties
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region MonoBehaviour methods & helpers
	// Use this for initialization
	//void Start(){}
	
	// Update is called once per frame
	//void Update(){}
	#endregion MonoBehaviour methods & helpers
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Getters & Setters

	public void setAgent(Agent newAgent)
	{
		agent = newAgent;
		agent.Renderer = (this);
		fullUpdate();
	}

	public void updateColor()
	{
		agentSprite.color = agent.AgentColor;
	}

	public void updateLocation()
	{
		this.transform.localPosition = agent.Location;
	}

	public void updateFOVScale()
	{
		if(fovImage == null || !fovImage.isActiveAndEnabled)
		{
			return;
		}
		float multiplier = agent.SightRange / fovMultiplier;
		fovImage.rectTransform.localScale = new Vector3(multiplier, multiplier, 1.0f);
		fovImage.SetAllDirty();
	}

	public void fullUpdate()
	{
		updateLocation();
		updateColor();
		updateFOVScale();
		recalculateFOVImage();
	}

	#endregion Getters & Setters
	//////////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////
	#region Helpers
	
	public void recalculateFOVImage()
	{
		// For now, just immediately setting this. Later would be nice to smooth the transition.
		// That goes for the actual vision cone as well - instant turning is a little strange.
		if(fovImage == null || !fovImage.isActiveAndEnabled)
		{
			return;
		}
		// The prefab for agents sets the 'fill origin' as being to the right. Fill is clockwise.
		// % to fill is easy, just the percentage of a circle represented by fieldOfView.
		fovImage.fillAmount = agent.FieldOfView / 360.0f;
		
		// angle then is direction-half that?
		fovImage.rectTransform.rotation = Quaternion.identity; // reset rotation
		fovImage.rectTransform.Rotate(0.0f, 0.0f, (agent.Direction + (agent.FieldOfView/2.0f)));
		fovImage.color = agent.AgentColor;

		fovImage.SetAllDirty();
	}

	#endregion Helpers
	//////////////////////////////////////////////////////////////////
}
