using System.Diagnostics;
using System.IO;

using pcs;
using tools;

namespace operation
{

public class Split : Step
{
	protected bool m_align;
	protected VectorInt3 m_tube = new VectorInt3();
	protected VectorInt3 m_position = new VectorInt3();
		
	//
	//
		
	public Split() : base(StepType.SPLIT)
	{
	}
		
	public Split(bool align, VectorInt3 tube, VectorInt3 pos) : base(StepType.SPLIT)
	{
		this.m_align = align;
		this.m_tube = tube;
		this.m_position = pos;
	}

	public override void Execute(Circuit circuit)
	{
	  UnityEngine.Debug.Log("executing split");

	  circuit.UnselectAll();

	  var piece = circuit.GetPiece(m_align, m_tube);
	  Debug.Assert(piece != null);
	  Debug.Assert(piece.GetType() == pcs.PieceType.TUBE);
	  circuit.SplitTube((Tube)piece, m_position);
	}


	public override Step GetInverse()
	{
	  return new Join(m_align, m_tube, m_position);
	}



	protected override BinaryWriter Serialize(BinaryWriter bw)
	{
	  bw.Write( m_align);
	  Tools.Serialize(bw, m_tube);
	  Tools.Serialize(bw, m_position);
	  return bw;
	}
	protected override BinaryReader Deserialize(BinaryReader br)
	{
	  m_align = br.ReadBoolean();
	  Tools.Deserialize(br, out m_tube);
	  Tools.Deserialize(br, out m_position);
	  return br;
	}



}

}

