using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;

using tools;


/**
 * Cursor that appears on selection to move it
 */
public class CursorFlat
{
	protected Vector3 m_position = new Vector3();
	protected Vector3 m_positionOffset = new Vector3();
	
	protected CursorFlatCS m_visual;
	protected Dictionary<Direction, GameObject> m_mapArrows = new Dictionary<Direction, GameObject>();
	
	protected Axis m_axisNormal;
	protected Axis m_axisActive;
	
	
	//------------------------------
	
	
	public CursorFlat()
	{
		InitVisual();
		Show(false);
	}
	
	public void SetPosition(Vector3 pos)
	{
	  	m_position = pos;
	}
	
	public void SetPositionOffset(Vector3 pos)
	{
	  	m_positionOffset = pos;
	}
	
	/**
	 * Get the two points that compose an arrow
	 *
	 * The two points form an unitary vector. This is used to calculate the
	 * amplitude of a move by projecting it on the window space and comparing
	 * to the mouse move.
	 */
	public KeyValuePair<Vector3, Vector3> GetArrow(Axis a)
	{
		Vector3 posTip = m_position;
		switch (a)
		{
		case Axis.X:
			Vector3 tip = new Vector3(1, 0, 0) * m_visual.transform.localScale.x;
		  posTip += tip;
		  break;
		case Axis.Y:
		  posTip += new Vector3(0, 1, 0) * m_visual.transform.localScale.y;
		  break;
		case Axis.Z:
		  posTip += new Vector3(0, 0, 1) * m_visual.transform.localScale.z;
		  break;
		default:
		  System.Diagnostics.Debug.Assert(false);
		  break;
		}
		
	  return new KeyValuePair<Vector3, Vector3>(m_position, posTip);
	}

	public virtual void InitVisual()
	{		
		m_visual = PrefabHubCS.Instance.GetPrefabCursorFlat();
	
		// Tuck away model from scene tree root level
		Transform cursors = GameObject.Find ("cursors").transform;
		m_visual.transform.parent = cursors;
	}
	
	public void UpdateVisual(float offset)
	{
		m_visual.transform.localRotation = Quaternion.identity;
		m_visual.transform.position = Vector3.zero;
		m_visual.transform.localScale = Vector3.one;
		
		// move cursor to grid position
		m_visual.transform.Translate(m_position);
		
		// apply offset (non-zero when selection in translation)
		m_visual.transform.Translate(m_positionOffset * (1 - offset));
		
		// setup active axis arrow geometry 
		m_visual.SetAxes(m_axisActive, m_axisNormal);
	}
	
	public void Show(bool show)
	{
		Show(show, 0);
	}
	
	public void Show(bool show, float delay)
	{
		m_visual.Show(show, delay);
	}

	public void SetAxes(Axis active, Axis normal)
	{
		this.m_axisActive = active;
		this.m_axisNormal = normal;
		m_visual.SetAxes(m_axisActive, m_axisNormal);
	}
	
	public void SetAxisActive(Axis active)
	{
		m_axisActive = active;
		while(m_axisNormal == m_axisActive || m_axisNormal == Axis.INVALID){
			m_axisNormal = (Axis)((((int)m_axisNormal) + 1) % 4);
		}
		m_visual.SetAxes(m_axisActive, m_axisNormal);
	}
	
	public void SetNormal(Axis normal)
	{
		m_axisNormal = normal;
		if (m_axisNormal == m_axisActive){
			m_axisActive = Axis.INVALID;
		}
		m_visual.SetAxes(m_axisActive, m_axisNormal);
	}
	
	public void SetCursor(Vector3 position, Vector3 offset, Axis axisActive, Axis axisNormal)
	{
		SetPosition(position);
		SetPositionOffset(offset);
	}
	
}

