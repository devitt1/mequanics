using System.Runtime.Serialization;
using System.IO;


namespace operation
{

public enum StepType
{
  INVALID,
  MOVE,
  SPLIT,
  JOIN,
  MOVE_INJECTOR,
  MULTISTEP,
  MERGE,
  MOVE_DUPLICATE,
  BRIDGE,
  UNBRIDGE,
  ROTATE_INJECTOR,
  DELETE_PIECE,
  CREATE_PIECE
}
	


public abstract class Step
{
	/**
	 *  This method applies a step's changes to the circuit instantly (nothing smooth here).  
	 * */
	public abstract void Execute(Circuit circuit);

	public abstract Step GetInverse();

	protected StepType m_type;
	public StepType Type {
		get{
			return m_type;
		}
	}
		
	//
	//
		
	public Step(StepType type)
	{
		this.m_type = type;
	}
		
	public virtual void Dispose()
	{
	}

	protected abstract BinaryWriter Serialize(BinaryWriter bw);
	protected abstract BinaryReader Deserialize(BinaryReader br);
		
		
		
	public static BinaryWriter CompleteSerialize(BinaryWriter bw, Step s)
	{
	  bw.Write((int)s.m_type);
			
	  return s.Serialize(bw);
	}
		
	public static BinaryReader CompleteDeserialize(BinaryReader br, out Step s)
	{
	  StepType type = (StepType)br.ReadInt32();
			
	  switch (type)
	  {
		case StepType.MOVE:
			s = new Move();
			break;
		case StepType.SPLIT:
			s = new Split();
			break;
		case StepType.JOIN:
			s = new Join();
			break;
		case StepType.MOVE_INJECTOR:
			s = new InjectorMove();
			break;
		case StepType.MULTISTEP:
			s = new MultiStep();
			break;
		case StepType.MERGE:
			s = new Merge();
			break;
		case StepType.MOVE_DUPLICATE:
			s = new Duplicate();
			break;
		case StepType.BRIDGE:
			s = new Bridge();
			break;
		case StepType.UNBRIDGE:
			s = new Unbridge();
			break;
//		case StepType.e:
//			s = new RotateInjector();
//			break;
		case StepType.DELETE_PIECE:
			s = new DeletePiece();
			break;
		default:
			throw new QBException(QBException.Type.STP_INVALID_TYPE);
	  }
			
	  	return s.Deserialize(br);
	}
		
}

}


