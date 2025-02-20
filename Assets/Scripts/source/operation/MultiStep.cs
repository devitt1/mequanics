using System.Collections.Generic;
using System.IO;

using pcs;

namespace operation
{

public class MultiStep : Step
{
	protected LinkedList<Step> m_steps = new LinkedList<Step>();
		
	public MultiStep() : base(StepType.MULTISTEP)
	{
	}

	public override void Execute(Circuit circuit)
	{
	  foreach (var step in m_steps){
		step.Execute(circuit);
			}
	}

	public override Step GetInverse()
	{
	  var inv = new MultiStep();
	  foreach (var step in m_steps){
		inv.m_steps.AddFirst(step.GetInverse());
		}
	  return inv;
	}

	public void PushStep(Step step)
	{
	  m_steps.AddLast(step);
	}


	public bool IsEmpty()
	{
	  return m_steps.Count == 0;
	}



	protected override BinaryWriter Serialize(BinaryWriter bw)
	{
		bw.Write(m_steps.Count);
		foreach (var step in m_steps){
			Step.CompleteSerialize(bw, step);
		}
		return bw;
	}
		
	protected override BinaryReader Deserialize(BinaryReader br)
	{
		int size = br.ReadInt32();
		for (uint i = 0; i < size; ++i)
		{
			Step step;
			Step.CompleteDeserialize(br, out step);
			m_steps.AddLast(step);
		}
		return br;
	}

}

}



