using UnityEngine;

using tools;

namespace pcs
{
public class Injector : Piece
{
	public Injector () : base(PieceType.INJECTOR)
	{
		m_length = 4;
	}
	
	protected Injector (PieceType p) : base(p)
	{
		m_length = 4;
	}
	
	public override void UpdateVisual (float t)
	{
		if (m_cs == null){
			return;
		}
			
		Transform transMesh = m_cs.GetTransformMesh();
			
		//reset transform //TODO3 don't reset and recalculate every frame! 
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
		
		// rotate and scale model
		float scaleLength = (m_length + m_lengthOff * (1 - t));
		transMesh.localScale = Vector3.Scale(transMesh.localScale, new Vector3 (scaleLength, 1, 1));	
		switch (m_axis) {
		case Axis.X:			
//			m_cs.transform.Translate(0, .5f, .5f);
			break;
		case Axis.Y:
			m_cs.transform.Translate(1.0f, 0f, 0f);
			m_cs.transform.Rotate(new Vector3 (0, 0, 90.0f), Space.Self);
			break;
		case Axis.Z:
			m_cs.transform.Translate(1.0f, 0f, 0f);
			m_cs.transform.Rotate(new Vector3 (0, -90.0f, 0), Space.Self);
			break;
		default:
			System.Diagnostics.Debug.Assert (false);
			break;
		}
			
//		transMesh.localScale = Vector3.Scale(transMesh.localScale, new Vector3(4f, 1f, 1f));
			
		UpdateShaderConstants(t);
	}

	protected override void TweakVisual()
	{	
		SetColorVisual(m_color);
	}
		
				
	public override void InitPhysics()
	{	
		UpdatePhysics();	
	}
		
		
	public override bool CheckSqueezing()
	{
		m_squeezing = m_length < 4;
			
		// Compute collision point (actually squeezing point)
	  	if (m_squeezing) {
			Vector3 coll = m_position.ToVector3();
			if (m_align){
			 	coll += Circuit.s_offsetSpaces;
			}
			coll += Circuit.s_offsetSpaces;
			switch (m_axis) {
			  case Axis.X:
				  coll.x += 1;
				  break;
			  case Axis.Y:
				  coll.y += 1;
				  break;
			  case Axis.Z:
				  coll.z += 1;
				  break;
			  default:
				System.Diagnostics.Debug.Assert(false);
				break;
			}	
			m_collPoints.Add(coll);
		}
	
		return m_squeezing;
	}
}

}
