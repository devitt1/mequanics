using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

using tools;
using pcs;

namespace operation
{

public class Bridge : Step
{
	protected bool m_align;
	protected VectorInt3 m_box1 = new VectorInt3();
	protected VectorInt3 m_box2 = new VectorInt3();
	protected Vector4 m_color2 = new Vector4();
		
	//
	//	
		
	public Bridge() : base(StepType.BRIDGE)
	{
	}

	public Bridge(bool align, VectorInt3 b1, VectorInt3 b2, Vector4 color2) : base(StepType.BRIDGE)
	{
		this.m_align = align;
		this.m_box1 = b1;
		this.m_box2 = b2;
		this.m_color2 = color2;
	}



	public override void Execute(Circuit circuit)
	{
	  UnityEngine.Debug.Log("executing bridge");

	  circuit.UnselectAll();

	  var b1 = circuit.GetPiece(m_align, m_box1);
	  System.Diagnostics.Debug.Assert(b1 != null);
	  System.Diagnostics.Debug.Assert(b1.GetType() == pcs.PieceType.BOX);
	  var b2 = circuit.GetPiece(m_align, m_box2);
	  System.Diagnostics.Debug.Assert(b2 != null);
	  System.Diagnostics.Debug.Assert(b1.GetType() == pcs.PieceType.BOX);

	  try
	  {
		circuit.Bridge((Box)b1, (Box)b2);
	  }
	  catch (QBException)
	  {
		System.Diagnostics.Debug.Assert(false);
	  }
	}


	public override Step GetInverse()
	{
	  return new Unbridge(m_align, m_box1, m_box2, m_color2);
	}




	protected override BinaryWriter Serialize(BinaryWriter bw)
	{
	  bw.Write( m_align);
	  Tools.Serialize(bw, m_box1);
	  Tools.Serialize(bw, m_box2);
	  Tools.Serialize(bw, m_color2);
	  return bw;
	}
	protected override BinaryReader Deserialize(BinaryReader br)
	{
	  m_align = br.ReadBoolean();
	  Tools.Deserialize(br, out m_box1);
	  Tools.Deserialize(br, out m_box2);
	  Tools.Deserialize(br, out m_color2);
	  return br;
	}



}

}

