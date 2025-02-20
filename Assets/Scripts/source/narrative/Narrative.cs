using UnityEngine;
using System.Collections.Generic;

using tools;
using pcs;
using ui;

namespace narr
{
	
	/**
	 * Narrative is used by GameInterface as a wrapper to filter all user input
	 * */
	public class Narrative : InputFilter
	{
		protected GameInterface m_gi;
			
		protected string m_uriCircuit;
		public string UriCircuit { get { return m_uriCircuit; } }
			
		protected LinkedList<narr.RTEvent> m_events;
		protected LinkedListNode<narr.RTEvent> m_eventCurrent;
			
		protected struct PieceDescVip
		{
			public VectorInt3 pos;
			public bool alignment;
			public int id;
			
			public PieceDescVip(VectorInt3 po, bool align, int i) { pos = po; alignment = align; id = i; }
		}
		protected LinkedList<PieceDescVip> m_piecesVIP;
		
		protected bool m_wasStarted = false;
		
		//
		//	
		
		public Narrative(IInputHandler wrapped, GameInterface gi) : base(wrapped)
		{
			m_gi = gi;
				
			m_events = new LinkedList<narr.RTEvent>();
		}
		
		public override bool HandleInput(UInput input)
		{
			// check if input is to be ignored
			
			if (m_eventCurrent != null)
			{
				RTEvent rte = m_eventCurrent.Value;
			
				// Check if the input is permissible
				bool inputOK = rte.CheckInput(input);
				if (inputOK) {
					// Check if any instantaneous Act are getting done by this
					if (input.Type == UInput.Typ.EndMove){
						Act a = rte.GetAct(typeof(ActUserMovePiece));
						if (a != null){
							((ActUserMovePiece)a).UpdateDone();
						}
						Act a2 = rte.GetAct(typeof(ActUserAchievementThreshold));
						if (a2 != null){
							((ActUserAchievementThreshold)a2).UpdateDone();
						}
					} else if (input.Type == UInput.Typ.Teleport){
						Act a = rte.GetAct(typeof(ActUserTeleport));
						if (a != null){
							((ActUserTeleport)a).Done = true;
						}
					} else if (input.Type == UInput.Typ.Simplify){
						Act a = rte.GetAct(typeof(ActUserSimplify));
						if (a != null){
							((ActUserSimplify)a).Done = true;
						}
					} else if (input.Type == UInput.Typ.StartInjectorMove){
						Act a = rte.GetAct(typeof(ActUserStartInjectorMove));
						if (a != null){
							a.Done = true;
						}
					} else if (input.Type == UInput.Typ.CommitInjectorMove){
						Act a = rte.GetAct(typeof(ActUserCommitInjectorMove));
						if (a != null){
							a.Done = true;
						}
					}
				} else {
					return false;
				}
			}
		
			bool handled = false;
			if (m_wrapped != null){
//				UnityEngine.Debug.Log("Sending input: " + input.Type.ToString());
				handled = m_wrapped.HandleInput(input);
			}
			return handled;
		}
			
		public void Start()
		{	
			// Register VIP pieces
			foreach (var p in m_piecesVIP){
				Piece piece = m_gi.GetPiece(p.pos, p.alignment);
				if (piece != null){
					m_gi.SetPieceID(piece, p.id);
				}
			}
			
			Mask2CS maskCS = Camera.mainCamera.transform.Find("mask2").GetComponent<Mask2CS>();
			maskCS.gameObject.SetActive(true);
			
			if (m_events.Count > 0)
			{
				m_eventCurrent = m_events.First;	
				m_eventCurrent.Value.Start(m_gi);
			}
			
			
			m_wasStarted = true;
		}
		
		public void Stop()
		{
			if (m_eventCurrent != null){
				m_eventCurrent.Value.Stop();
			}
			m_eventCurrent = null;
			
			Mask2CS maskCS = Camera.mainCamera.transform.Find("mask2").GetComponent<Mask2CS>();
			maskCS.gameObject.SetActive(false);
		}
		
		public void Update(float seconds)
		{
			if (m_eventCurrent != null)
			{
				m_eventCurrent.Value.Update(seconds); 
				if (m_eventCurrent.Value.IsDone){
//					UnityEngine.Debug.Log("Completed Event " + m_eventCurrent.ToString());
					
					m_eventCurrent.Value.Stop();
					m_events.RemoveFirst();
					
					if (m_events.Count > 0){
						m_eventCurrent = m_events.First;
						m_eventCurrent.Value.Start(m_gi);
					} else {
						m_eventCurrent = null;
					}
				}
			}
		}
		
		public bool WasStarted()
		{
			return m_wasStarted;
		}
		
	}

}