using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

using tools;

namespace pcs
{

public class Tube : Piece
{
	public Tube() : base(PieceType.TUBE)
	{
	}

	protected Tube(PieceType type) : base(type)
	{
	}
		
	public override KeyValuePair<VectorInt3, VectorInt3> GetCollisionBox()
	{
		VectorInt3 posMin = m_position; 
		VectorInt3 posMax = m_position;
		  switch (m_axis)
		  {
			case Axis.X:
			  posMax += new VectorInt3(m_length, 1, 1);
			  break;
			case Axis.Y:
			  posMax += new VectorInt3(1, m_length, 1);
			  break;
			case Axis.Z:
			  posMax += new VectorInt3(1, 1, m_length);
			  break;
			default:
			  System.Diagnostics.Debug.Assert(false);
			  break;
		  }
	  	return new KeyValuePair<VectorInt3, VectorInt3>(posMin, posMax);
	}

	/**
	*	
	*/
	public override void UpdateVisual(float t)
	{
		if (m_cs == null){
			return;
		}
			
		Transform transMesh = m_cs.GetTransformMesh();
			
		m_cs.transform.localRotation = Quaternion.identity;
		m_cs.transform.position = Vector3.zero;
		transMesh.localScale = Vector3.one;
		
		// move model to grid position
		m_cs.transform.Translate(m_position.ToVector3());
			
		// offset to dual space if applicable 
	  	if (m_align){
			m_cs.transform.Translate(Circuit.s_offsetSpaces);
		}

		// apply offset (non-zero for pieces in translation)
		m_cs.transform.Translate(m_positionOff * (1 - t));
		
		// squeeze
		if (m_squeezeDirection != Direction.INVALID){
			if (Tools.DirectionIsPositive(m_squeezeDirection)){
				Vector3 sca = new Vector3(1f, 1f, 1f) - (Tools.GetVectorFromDirection(m_squeezeDirection).ToVector3()) * (1 - t);
				transMesh.localScale = Vector3.Scale(transMesh.localScale, sca);
			} else {
				Vector3 sca = new Vector3(1f, 1f, 1f) + (Tools.GetVectorFromDirection(m_squeezeDirection).ToVector3()) * t;
				transMesh.localScale = Vector3.Scale(transMesh.localScale, sca);
			}
		}
			
		// scale tube length (for tubes being compressed or stretched)
		float scaleLength = (m_length + m_lengthOff * (1 - t));
		switch (m_axis) {
		case Axis.X:
			transMesh.localScale = 
				Vector3.Scale(transMesh.localScale, new Vector3 (scaleLength, 1, 1));	
			break;
		case Axis.Y:
			transMesh.localScale = 
				Vector3.Scale(transMesh.localScale, new Vector3(1, scaleLength, 1));
			break;
		case Axis.Z:
			transMesh.localScale = 
				Vector3.Scale(transMesh.localScale, new Vector3(1, 1, scaleLength));
			break;
		default:
			System.Diagnostics.Debug.Assert (false);
			break;
		}
			
		UpdateShaderConstants(t);
	}
	
	protected override void TweakVisual()
	{
		SetColorVisual(m_color);
	}
		
}
	
	
/**
 * 	Fully functional Tube that is simply not rendered. 
 * */
public class WeakTube : Tube
{
	public override void InitVisual()
	{
	}
		
	public override void UpdateVisual(float offset)
	{	
		//TODO3 prototype renders WeakTube as wireframe model here
	}
}
	
}



