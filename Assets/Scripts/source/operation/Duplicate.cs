using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

using tools;
using pcs;

namespace operation
{

public class Duplicate : Move
{
	protected VectorInt3 m_brokenBound = new VectorInt3();
		
	//	
	//	
	
	public Duplicate() : base()
	{
	  	m_type = StepType.MOVE_DUPLICATE;
	}
		
	public Duplicate(LinkedList<KeyValuePair<bool, VectorInt3>> selection, Direction dir, VectorInt3 brokenBound) : base(selection, dir, 5)
	{
		this.m_brokenBound = brokenBound;
	  	m_type = StepType.MOVE_DUPLICATE;
	}
		
	public Duplicate(IEnumerable<Piece> pieces, Direction dir, VectorInt3 brokenBound) : base(pieces, dir, 5)
	{
		this.m_brokenBound = brokenBound;
	  	m_type = StepType.MOVE_DUPLICATE;
	}
		
	//
	//

	public override void Execute(Circuit circuit)
	{
		circuit.UnselectAll();

		foreach (var p in m_selection)
		{
			Piece piece = circuit.GetPiece(p.Key, p.Value);
			Debug.Assert(piece != null);
			circuit.Select(piece);
		}

		try
		{
			circuit.DuplicateSelection(m_direction, true);
		}
		catch (QBException e)
		{
			UnityEngine.Debug.LogError(e.GetMessage());
			System.Diagnostics.Debug.Assert(false);
		}

	  	MakeMove(circuit, 1, 5);
	}

	public override Step GetInverse()
	{
		Merge inv = new Merge(m_selection, Tools.InvertDirection(m_direction), m_brokenBound);

		foreach (var step in m_fixedBoxes){
			inv.m_fixedBoxes[m_length - step.Key] = step.Value;
		}

		return inv;
	}

	protected override BinaryWriter Serialize(BinaryWriter bw)
	{
		base.Serialize(bw);
		
		Tools.Serialize(bw, m_brokenBound);
		
		return bw;
	}
	
	protected override BinaryReader Deserialize(BinaryReader br)
	{
		base.Deserialize(br);
		
		Tools.Deserialize(br, out m_brokenBound);
		
		return br;
	}

}

}

