using UnityEngine;



public enum EState
{
	MENUMAIN,
	MENUCIRCUITSELECT,
	PLAY,
	PLAYTUTORIAL,
	
	TERMINATE,
	QUIT,
	
	INVALID
}


/**
 *	Base class for objects that represent (mutually 
 *	exclusive) states the application can assume. 
 **/
public abstract class State : IEventHandler
{
	protected float m_time;
	
	protected GameObject m_receiver;
	
	protected EState m_stateType;
	public EState StateType	
	{
		get {
			return m_stateType;	
		}
	}
	
	public State()
	{
		m_time = 0;
		
		m_receiver = new GameObject("eventReceiver");
		EventReceiverCS receiverCS = m_receiver.AddComponent<EventReceiverCS>();
		receiverCS.m_handler = this;
	}
	
	public State(EState s) : this()
	{
		m_stateType = s;
	}
	
	protected int m_busyCount;
	protected GameObject m_busy;

	
	//
	//
	
	public virtual void Init()
	{
		if (m_busy == null){
			m_busy = PrefabHubCS.Instance.GetActivityIndicator3D();
		}
		m_busyCount = 0;
	}
	
	
	public virtual void Cleanup()
	{
		if (m_busy != null){
			// m_busy.SetActive(false);
			GameObject.Destroy(m_busy);
		}
	}
	
	// Update is called once per frame
	public virtual void Update(float seconds)
	{
		m_time += seconds;
	}
	
	public void ShowBusy(bool show)
	{
		if (show) {
			if (m_busyCount == 0) {
				m_busy.SetActive(true);
			} 
			m_busyCount++;
			
		} else {
			if (m_busyCount > 0){
				m_busyCount--;
			}
			if (m_busyCount == 0){
				m_busy.SetActive(false);
			}
		}
	}
	
	public abstract bool HandleEvent(UnityEngine.Event e);
	
}
