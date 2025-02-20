using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using pcs;
using tools;

namespace operation
{

public class CreatePiece : Step
{
	protected List<pcs.PieceDescriptor> m_pieces = new List<pcs.PieceDescriptor>();
		
		//
		//
		
	public CreatePiece() : base(StepType.CREATE_PIECE)
	{
	}

	public void AddPiece(pcs.PieceDescriptor p)
	{
	  m_pieces.Add(p);
	}



	public override void Execute(Circuit circuit)
	{
		circuit.UnselectAll();
		
		Dictionary<VectorInt3, Box>[] boxMap = {
			new Dictionary<VectorInt3, pcs.Box>(new tools.Vec3Hash()), 
			new Dictionary<VectorInt3, pcs.Box>(new tools.Vec3Hash())
		};

		foreach (var p in m_pieces)  {
			try
			{
			  	if (p.type != pcs.PieceType.BOX){
					circuit.CreatePiece(boxMap, p);
				}
			}
			catch (ex.InvalidValue e)
			{
				UnityEngine.Debug.LogError(e.Message);
			  	System.Diagnostics.Debug.Assert(false);
			}
		}
	}


	public override Step GetInverse()
	{
		var step = new DeletePiece();
		foreach (var p in m_pieces){
			step.AddPiece(p);
		}
	  return step;
	}





	protected override BinaryWriter Serialize(BinaryWriter bw)
	{
	  int nb = m_pieces.Count;
	  bw.Write(nb);
	  foreach (var p in m_pieces)
	  {
		bw.Write((int)p.type);
		bw.Write(p.align);
		Tools.Serialize(bw, p.position);
		bw.Write((int)p.axis);
		bw.Write(p.length);
		bw.Write(p.injectionId);
		Tools.Serialize(bw, p.color);
//		bw.Write(p.tags.Count);
//			foreach (var t in p.tags){
//		  		bw.Write(t);
//			}
	  	}

	  return bw;
	}
	protected override BinaryReader Deserialize(BinaryReader br)
	{
	  int nb = br.ReadInt32();
	  m_pieces.Capacity = nb;
	  foreach (var p in m_pieces)
	  {
		p.type = (pcs.PieceType)br.ReadInt32();
		p.align = br.ReadBoolean();
		Tools.Deserialize(br, out p.position);
		p.axis = (tools.Axis)br.ReadInt32();
		p.length = br.ReadInt32();
		p.injectionId = br.ReadInt32();
		Tools.Deserialize(br, out p.color);
//		nb = br.ReadInt32();
//		for (int i = 0; i < nb; ++i)
//		{
//		  uint t = br.ReadUInt32();
//		  p.tags.insert(t);
//		}
	  }

	  return br;
	}



}

}
