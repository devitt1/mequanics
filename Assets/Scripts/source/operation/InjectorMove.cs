using UnityEngine;
using System.Diagnostics;
using System.IO;

using tools;
using pcs;

namespace operation
{

public class InjectorMove : Step
{
	protected bool m_startAlign;
	protected VectorInt3 m_start = new VectorInt3();
	protected bool m_endAlign;
	protected VectorInt3 m_end = new VectorInt3();
		
	//
	//	
		
	public InjectorMove() : base(StepType.MOVE_INJECTOR)
	{
	}
		
	public InjectorMove(bool startAlign, VectorInt3 start, bool endAlign, VectorInt3 end) : base(StepType.MOVE_INJECTOR)
	{
		this.m_startAlign = startAlign;
		this.m_start = start;
		this.m_endAlign = endAlign;
		this.m_end = end;
	}

	public override void Execute(Circuit circuit)
	{
		UnityEngine.Debug.Log("executing injector move");
		
		circuit.UnselectAll();
		
		var injector = circuit.GetPiece(m_startAlign, m_start);
		System.Diagnostics.Debug.Assert(injector != null);
		System.Diagnostics.Debug.Assert(injector.GetType() == pcs.PieceType.INJECTOR || injector.GetType() == pcs.PieceType.CAP);
		var tube = circuit.GetPiece(m_endAlign, m_end);
		System.Diagnostics.Debug.Assert(tube != null);
		System.Diagnostics.Debug.Assert(tube.GetType() == pcs.PieceType.TUBE);
		circuit.MoveInjector((Injector)injector, (Tube)tube, m_end);
	}

	public override Step GetInverse()
	{
	  return new InjectorMove(m_endAlign, m_end, m_startAlign, m_start);
	}
	

	protected override BinaryWriter Serialize(BinaryWriter bw)
	{
		
	  bw.Write(this.m_startAlign);
	  Tools.Serialize(bw, this.m_start);
	  bw.Write(this.m_endAlign);
	  Tools.Serialize(bw, this.m_end);
	  return bw;
	}
		
	protected override BinaryReader Deserialize(BinaryReader br)
	{
	  this.m_startAlign = br.ReadBoolean();
	  Tools.Deserialize(br, out this.m_start);
	  this.m_endAlign = br.ReadBoolean();
	  Tools.Deserialize(br, out this.m_end);
	  return br;
	}

}

}



