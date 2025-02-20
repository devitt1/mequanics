

public class QBException : System.Exception
{
	
	public enum Type
	{
		OTHER_ACTION_IN_PROGRESS = 0,
		
		UNDO_NO_MORE,
		
		DUP_INVALID_SELECTION,
		DUP_INVALID_AXIS,
		DUP_IMPOSSIBLE,
		
		ROTINJ_INVALID_SELECTION,
		ROTINJ_INVALID_TARGET,
		ROTINJ_INVALID_PATH,
		ROTINJ_COLLISION,
		
		MOVEINJ_INVALID_SELECTION,
		
		TPINJ_INVALID_SELECTION,
		TPINJ_NOT_SIMPLE_LOOP,
		TPINJ_NOT_ONE_TUBE,
		TPINJ_TUBE_TOO_SHORT,
		
		BRG_INVALID_SELECTION,
		BRG_NOT_ALIGNED,
		BRG_NOT_A_LOOP,
		BRG_SAME_PATH,
		BRG_COLLISION,
		
		STP_INVALID_TYPE,
		
		SAVE_SLOTUNUSED,
		
		NB_TYPES
	}

	public QBException(Type t)
	{
		this.m_type = t;
	}

	public Type GetTypeQB()
	{
	  	return m_type;
	}
	
	public string GetTitle()
	{
		if (m_type < 0 || m_type >= Type.NB_TYPES){
			return null;
		}
		int pid = ((int)m_type) * 2 + (int)PID.ER_OTHER_ACTION_IN_PROGRESS_TITLE;
		return Locale.Instance.Get((PID)pid);
	}
	
	public string GetMessage()
	{
	  if (m_type < 0 || m_type >= Type.NB_TYPES){
			return null;
		}
		int pid = ((int)m_type) * 2 + (int)PID.ER_OTHER_ACTION_IN_PROGRESS;
		return Locale.Instance.Get((PID)pid);
	}

	protected Type m_type;
	public Type Ty { get{ return m_type; } } 
	
	
}


