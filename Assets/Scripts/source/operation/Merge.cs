using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

using tools;
using pcs;

namespace operation
{

public class Merge : Move
{
	protected VectorInt3 m_brokenBound = new VectorInt3();
		
	//
	//

	public Merge() : base()
	{
	  m_type = StepType.MERGE;
	}
		
	public Merge(LinkedList<KeyValuePair<bool, VectorInt3>> selection, Direction dir, VectorInt3 brokenBound) : base(selection, dir, 5)
	{
		this.m_brokenBound = brokenBound;
	}

//	public Merge(IEnumerable<Piece> pieces, Direction dir, VectorInt3 brokenBound) : base(pieces, dir, 5)
//	{
//		this.m_brokenBound = brokenBound;
//	  	m_type = StepType.MERGE;
//	}
		
	//
	//
		
	public override void Execute(Circuit circuit)
	{
		UnityEngine.Debug.Log("executing merge in dir " + (int)m_direction);
			
		// Select spawned triple (and more ?)
	  	SelectPieces(circuit); 
	
		// Move spawned triple about 4 units towards the duplication source
	  	MakeMove(circuit, 0, 4); 

		// Beneighbor boxes between which the weak tube was removed ("brokenBound")
		Piece b1 = circuit.GetPiece(m_selection.First.Value.Key, m_brokenBound);
		Debug.Assert(b1 != null);
		Debug.Assert(b1.GetType() == PieceType.BOX);
		Piece b2 = circuit.GetPiece(m_selection.First.Value.Key, m_brokenBound + (-Tools.GetVectorFromDirection(m_direction)));
		Debug.Assert(b2 != null);
		Debug.Assert(b2.GetType() == PieceType.BOX);
		b1.SetNeighbor(Tools.InvertDirection(m_direction), b2);
		b2.SetNeighbor(m_direction, b1);

		// set the fixed boxes for the last step
		circuit.FlushFixedBoxes();
		if( m_fixedBoxes.ContainsKey(4)){
			foreach (var box in m_fixedBoxes[4]){
		  		circuit.AddFixedBox(box.Key, box.Value);
			}
		}
		circuit.SetMoveStep(Tools.DirectionIsPositive(m_direction) ? 1 : -1);

		// revert the duplication
		circuit.RevertDuplicate(m_direction);
		
		circuit.FlushFixedBoxes();
		circuit.SetMoveStep(0);
		
		circuit.UnselectAll();
	}


	public override Step GetInverse()
	{
		Duplicate inv = new Duplicate(m_selection, Tools.InvertDirection(m_direction), m_brokenBound);

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

