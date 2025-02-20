using UnityEngine;
using System.Diagnostics;
using System.IO;

using tools;

namespace operation
{

public class Unbridge : Step
{
	protected bool m_align;
	protected VectorInt3 m_box1 = new VectorInt3();
	protected VectorInt3 m_box2 = new VectorInt3();
	protected Vector4 m_color2 = new Vector4();
	
		//
		//
		
	public Unbridge() : base(StepType.UNBRIDGE)
	{
	}

	public Unbridge(bool align, VectorInt3 b1, VectorInt3 b2, Vector4 color2) : base(StepType.UNBRIDGE)
	{
		this.m_align = align;
		this.m_box1 = b1;
		this.m_box2 = b2;
		this.m_color2 = color2;
	}

	

	public override void Execute(Circuit circuit)
	{
	  UnityEngine.Debug.Log("executing unbridge");

	  circuit.UnselectAll();

	  var piece = circuit.GetPiece(m_align, m_box1);
	  System.Diagnostics.Debug.Assert(piece != null);
	  System.Diagnostics.Debug.Assert(piece.GetType() == pcs.PieceType.BOX);
	  circuit.Select(piece);
	  piece = circuit.GetPiece(m_align, m_box2);
	  System.Diagnostics.Debug.Assert(piece != null);
	  System.Diagnostics.Debug.Assert(piece.GetType() == pcs.PieceType.BOX);
	  circuit.Select(piece);

	  bool ret = circuit.UnbridgeBoxes(m_color2);
	  System.Diagnostics.Debug.Assert(ret);
	}

	public override Step GetInverse()
	{
	  return new Unbridge(m_align, m_box1, m_box2, m_color2);
	}


	protected override BinaryWriter Serialize(BinaryWriter bw)
	{
	  bw.Write(m_align);
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


