
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using tools;
using pcs;

namespace operation
{

public class DeletePiece : Step
{
	protected List<pcs.PieceDescriptor> m_pieces = new List<PieceDescriptor>();
		
	//
	//
		
	public DeletePiece() : base(StepType.DELETE_PIECE)
	{
	}
		
	//
	//
		
	public void AddPiece(pcs.PieceDescriptor p)
	{
	  	m_pieces.Add(p);
	}

	public override void Execute(Circuit circuit)
	{
	  	circuit.UnselectAll();

		foreach (PieceDescriptor p in m_pieces)
		{
			Piece piece = circuit.GetPiece(p.align, p.position);
			bool b = circuit.DeletePiece(p.align, p.position);
			piece.UninitVisual();
				
			System.Diagnostics.Debug.Assert(b);
		}
	}


	public override Step GetInverse()
	{
	  	var step = new CreatePiece();
	  	foreach (var p in m_pieces){
			step.AddPiece(p);
		}
	  	return step;
	}




	protected override BinaryWriter Serialize(BinaryWriter bw)
	{
		int nb = m_pieces.Count;
		bw.Write(nb);
		foreach (var p in m_pieces){
			bw.Write((int)p.type);
			bw.Write(p.align);
			Tools.Serialize(bw, p.position);
			bw.Write((int)p.axis);
			bw.Write(p.length);
			bw.Write(p.injectionId);
			Tools.Serialize(bw, p.color);
//			bw.Write((int)p.tags.Count);
//			foreach (uint t in p.tags){
//				bw.Write(t);
//			}
		}
		
		return bw;
	}
	protected override BinaryReader Deserialize(BinaryReader br)
	{
		int nb = br.ReadInt32();
		m_pieces.Clear();
		foreach(int i in Enumerable.Range(0, nb))
		{
			PieceDescriptor p = new PieceDescriptor();
			p.type = (PieceType)br.ReadInt32();
			p.align = br.ReadBoolean();
			Tools.Deserialize(br, out p.position);
			p.axis = (tools.Axis)br.ReadInt32();
			p.length = br.ReadInt32();
			p.injectionId = br.ReadInt32();
			Tools.Deserialize(br, out p.color);
			//		nb = br.ReadInt32();
			//		for (int i = 0; i < nb; ++i) {
			//		  uint t = br.ReadUInt32();
			//		  p.tags.Add(t);
			//		}
			m_pieces.Add(p);
		}
		
		return br;
	}



}

}


