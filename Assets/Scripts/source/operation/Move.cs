using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

using tools;
using pcs;

namespace operation
{

public class Move : Step
{
	protected LinkedList<KeyValuePair<bool, VectorInt3>> m_selection = new LinkedList<KeyValuePair<bool, VectorInt3>>();
	protected Direction m_direction;
	protected uint m_length;
	public Dictionary<uint, HashSet<KeyValuePair<bool, VectorInt3>>> m_fixedBoxes = 
			new Dictionary<uint, HashSet<KeyValuePair<bool, VectorInt3>>>();

	//
	//
		
	public Move() : base(StepType.MOVE)
	{
	}
		
	public Move(LinkedList<KeyValuePair<bool, VectorInt3>> selection, Direction dir, uint len) : base(StepType.MOVE)
	{
		this.m_direction = dir;
		this.m_length = len;
		System.Diagnostics.Debug.Assert(len > 0);
		foreach (var p in selection){
			m_selection.AddLast(new KeyValuePair<bool, VectorInt3>(p.Key, p.Value - Tools.GetVectorFromDirection(dir) * (int)len));
		}
	}
		
	public Move(IEnumerator<Piece> begin, IEnumerator<Piece> end, Direction dir, uint len) : base(StepType.MOVE)
	{
		this.m_direction = dir;
		this.m_length = len;
		System.Diagnostics.Debug.Assert(begin != end);
		System.Diagnostics.Debug.Assert(len > 0);
			
	  	do {
			m_selection.AddLast(new KeyValuePair<bool, VectorInt3>(begin.Current.GetAlignment(), begin.Current.GetPosition() - Tools.GetVectorFromDirection(dir) * (int)len));
		} while (begin.MoveNext() && begin != end);
	}
		
	public Move(IEnumerable<Piece> pieces, Direction dir, uint len) : base(StepType.MOVE)
	{
		this.m_direction = dir;
		this.m_length = len;
		System.Diagnostics.Debug.Assert(len > 0);
	  	foreach (Piece p in pieces){
			m_selection.AddLast(new KeyValuePair<bool, VectorInt3>(p.GetAlignment(), p.GetPosition() - Tools.GetVectorFromDirection(dir) * (int)len));
		}
	}
		
		
	public void AddFixedBox(uint step, bool align, VectorInt3 pos)
	{
		if (!m_fixedBoxes.ContainsKey(step)){
			m_fixedBoxes[step] = new HashSet<KeyValuePair<bool, VectorInt3>>(new PairComparator<bool, VectorInt3>());
		}
		m_fixedBoxes[step].Add(new KeyValuePair<bool, VectorInt3>(align, pos));
	}

	public override void Execute(Circuit circuit)
	{
		Debug.WriteLine("undoing move in dir " + m_direction.ToString());

		SelectPieces(circuit);
		MakeMove(circuit, 0, m_length);
	}

	public override Step GetInverse()
	{
	  	Move inv = new Move(m_selection, Tools.InvertDirection(m_direction), m_length);

	  	foreach (var step in m_fixedBoxes){
			inv.m_fixedBoxes[m_length - step.Key] = step.Value;
		}

	  	return inv;
	}
		
	protected void SelectPieces(Circuit circuit)
	{
	  circuit.UnselectAll();

	  foreach (var p in m_selection)
	  {
		var piece = circuit.GetPiece(p.Key, p.Value);
		//assert(piece);
		// it may happen that a move creates new pieces, so these pieces doesn't
		// exist yet
		if (piece != null){
		  circuit.Select(piece);
		}
	  }
	}
		
	/**
	 * 	Instantaneously applies this move to the circuit (limited to partial moves fro to to). 
	 * */ 
	protected void MakeMove(Circuit circuit, uint fro, uint to)
	{
	  for (uint i = fro; i < to; ++i)
	  {
			circuit.FlushFixedBoxes(); // announce fixed boxes for this partial move (=step) to circuit
			if (m_fixedBoxes.ContainsKey(i)){
				var iter = m_fixedBoxes[i];
			  	foreach (var box in iter){
					circuit.AddFixedBox(box.Key, box.Value);
				}
			}
			circuit.SetMoveStep(Tools.DirectionIsPositive(m_direction) ? 1 : -1); 
			circuit.MoveSelection(m_direction); // trigger selection movement
	  }
	  circuit.FlushFixedBoxes();
	  circuit.SetMoveStep(0);
	}
		
			
	/**
	 * 
	 * */
	protected override BinaryWriter Serialize(BinaryWriter bw)
	{
		// Write selection
		bw.Write(m_selection.Count);
			
		foreach (var p in m_selection){
			bw.Write(p.Key);
			Tools.Serialize(bw, p.Value);
		}
			
		// Write movement vector
		bw.Write((int)m_direction);
		bw.Write(m_length);
		
		// Write fixed boxes for this move
		bw.Write(m_fixedBoxes.Count);
		foreach (var box in m_fixedBoxes){
			bw.Write(box.Key);
			bw.Write(box.Value.Count);
			foreach (var pos in box.Value)
			{
				bw.Write(pos.Key);
				Tools.Serialize(bw, pos.Value);
			}
		}
		
		return bw;
	}
		
	/**
	 * 
	 * */
	protected override BinaryReader Deserialize(BinaryReader br)
	{
		/// \todo asserts
		// piece selection for this move
		int sizeSelection = br.ReadInt32();
		for (int i = 0; i < sizeSelection; i++){
			bool align = br.ReadBoolean();
			VectorInt3 pos;
			Tools.Deserialize(br, out pos);
			
			m_selection.AddLast(new KeyValuePair<bool, VectorInt3>(align, pos));
	  	}
			
		// direction
		m_direction = (Direction)br.ReadInt32();
	 
		// distance
		m_length = br.ReadUInt32();

		// Fixed boxes
		int fixedBoxesSize = br.ReadInt32();
		for (uint i = 0; i < fixedBoxesSize; ++i)
		{
			uint stepNumber;
			stepNumber = br.ReadUInt32();
			if (!m_fixedBoxes.ContainsKey(stepNumber)) {
				m_fixedBoxes[stepNumber] = new HashSet<KeyValuePair<bool, VectorInt3>>(new tools.PairComparator<bool, VectorInt3>());		
			}
			
			int nbFixedBoxes = br.ReadInt32();
			for (uint j = 0; j < nbFixedBoxes; ++j)
			{
				bool align = br.ReadBoolean();
				VectorInt3 pos;
				Tools.Deserialize(br, out pos);
					
			  	m_fixedBoxes[stepNumber].Add(new KeyValuePair<bool, VectorInt3>(align, pos));
			}
		}

		return br;
	}
		
	//--------------------------------------

}

}



