using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

using pcs;
using tools;

namespace operation
{
	
public class RotateInjector : Step
{
	public RotateInjector(bool align, VectorInt3 coTarget, VectorInt3 box, VectorInt3 target, VectorInt3 sinjector, VectorInt3 einjector) : base(StepType.ROTATE_INJECTOR)
	{
		this.m_align = align;
		this.m_coTarget = coTarget;
		this.m_box = box;
		this.m_target = target;
		this.m_sinjector = sinjector;
		this.m_einjector = einjector;
	}
		
	public override void Execute(Circuit circuit)
	{
		circuit.UnselectAll();
		
		foreach (var p in new List<VectorInt3>() {m_coTarget, m_box, m_target, m_sinjector})
		{
			var piece = circuit.GetPiece(m_align, p);
			Debug.Assert(piece != null);
			circuit.SelectOnly(piece);
		}
		
		try
		{
			circuit.RotateInjector();
		}
		catch (System.Exception)
		{
			Debug.Assert(false);
		}
	}


	public override Step GetInverse()
	{
	  return new RotateInjector(m_align, m_coTarget, m_target, m_box, m_einjector, m_sinjector);
	}

	protected bool m_align;

	protected VectorInt3 m_coTarget = new VectorInt3();
	protected VectorInt3 m_box = new VectorInt3();
	protected VectorInt3 m_target = new VectorInt3();
	protected VectorInt3 m_sinjector = new VectorInt3();
	protected VectorInt3 m_einjector = new VectorInt3();

	protected RotateInjector() : base(StepType.ROTATE_INJECTOR)
	{
	}



	protected override BinaryWriter Serialize(BinaryWriter bw)
	{
	  bw.Write(m_align);
	  Tools.Serialize(bw, this.m_coTarget);
	  Tools.Serialize(bw, this.m_box);
	  Tools.Serialize(bw, this.m_target);
	  Tools.Serialize(bw, this.m_sinjector);
	  Tools.Serialize(bw, this.m_einjector);
	  return bw;
	}
	protected override BinaryReader Deserialize(BinaryReader br)
	{
	  this.m_align = br.ReadBoolean();
	  Tools.Deserialize(br, out this.m_coTarget);
	  Tools.Deserialize(br, out this.m_box);
	  Tools.Deserialize(br, out this.m_target);
	  Tools.Deserialize(br, out this.m_sinjector);
	  Tools.Deserialize(br, out this.m_einjector);
	  return br;
	}

}

}


