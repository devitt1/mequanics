using UnityEngine;
using System.Collections.Generic;

using tools;

namespace pcs
{

/// \todo boxes should return special segments on GetSegment
public class Box : Tube
{
	public Box() : this(PieceType.BOX)
	{
	  m_length = 1;
	  // we have to initialize this to something
	  m_axis = Axis.X;
	}
		
	protected Box(PieceType type) : base(type)
	{
	  m_length = 1;
	  // we have to initialize this to something
	  m_axis = Axis.X;
	}

	public override KeyValuePair<VectorInt3, VectorInt3> GetCollisionBox()
	{
	  	return new KeyValuePair<VectorInt3, VectorInt3>(m_position, m_position + new VectorInt3(1));
	}
			
}

}



