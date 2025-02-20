using UnityEngine;
using System.Collections.Generic;

using pcs;
using ui;

namespace narr
{
	
	public abstract class Act
	{	
		protected GameInterface m_gi;
		protected bool m_done;
			
		public virtual void Start(GameInterface gi)
		{
			m_gi = gi;
		}
		public virtual void Progress(float seconds){}
		public virtual void Stop(){}
		
		public virtual bool Done { get { return m_done; } set{ m_done = value; }}
	}
		
	public class ActDialog : Act
	{
		string m_text;
		GuiDialog3DCS m_popupCS;
		
		public ActDialog(string text)
		{
			m_text = text;
		}
			
		public override void Start(GameInterface gi)
		{
			base.Start(gi);
			
			m_done = false;
				
			// display dialog box
			m_popupCS = Popups.Instance.CreateDialog(m_text, "Tutorial", HudPos.BOTTOMLEFT, null, false);
			Done = true;
		}
			
		public override void Stop()
		{
			// remove dialog box
			if (m_popupCS != null){
				GameObject.Destroy(m_popupCS.gameObject);
			}
		}
	}
		
	public class ActDialogOK : Act
	{
		string m_text;
		GuiDialog3DCS m_popupCS;
			
		public ActDialogOK(string text)
		{
			m_text = text;
		}
			
		public override void Start(GameInterface gi)
		{
			base.Start(gi);
			
			m_done = false;
				
			// display dialog box
			m_popupCS = Popups.Instance.CreateDialog(m_text, "Tutorial", HudPos.BOTTOMLEFT, this.DialogCB, true);
		}
			
		public override void Stop()
		{
			// remove dialog box
			if (m_popupCS != null){
				GameObject.Destroy(m_popupCS.gameObject);
			}
		}
			
		public void DialogCB()
		{
			m_done = true;
		}
	}
	
	public class ActExclamation : Act
	{
		Gui3DExclamationsCS.ItemsToSay m_item;
		Gui3DExclamationsCS m_ex;
		
		public ActExclamation(Gui3DExclamationsCS.ItemsToSay item)
		{
			m_item = item;
		}
		
		public override void Start(GameInterface gi)
		{
			// display dialog box
			Transform trans = Camera.mainCamera.transform.FindChild("exclamations");
			m_ex = trans.gameObject.GetComponent<Gui3DExclamationsCS>();
			m_ex.Exclaim(m_item);

			m_done = true;
		}
	}
		
		
	public class ActHighlightLocation : Act
	{
		Vector3 position;
		float radius;
		Mask2CS m_mask;	
			
		public ActHighlightLocation(Vector3 p, float r)
		{
			position = p;
			radius = r;
		}
		
		public override void Start(GameInterface gi)
		{
			base.Start(gi);
			
			// place and display mask
			m_mask = Camera.mainCamera.transform.Find("mask2").GetComponent<Mask2CS>();
			m_mask.SetAreaOfInterestSmooth(position, radius, 0.5f);

			Done = true;
		}
			
		public override void Stop()
		{
		}
	}
		
	public class ActHighlightGameObject : Act
	{
		GameObject m_go;
		float radius;
		Mask2CS m_mask;	
			
		public ActHighlightGameObject(string goTag, float r)
		{
			m_go = GameObject.FindGameObjectWithTag(goTag);
			radius = r;
		}
		
		public override void Start(GameInterface gi)
		{
			base.Start(gi);
			
			// place and display mask
			m_mask = Camera.mainCamera.transform.Find("mask2").GetComponent<Mask2CS>();
			if (m_go != null){
				m_mask.SetGoOfInterestSmooth(m_go, radius, 0.5f);
			} else {
				m_mask.SetAreaOfInterestSmooth(Vector3.zero, 100f, 0.5f);
			}

			Done = true;
		}
			
		public override void Stop()
		{
		}
	}
		
	public class ActHighlightPiece : Act
	{
		VectorInt3 posPiece;
		bool alignment;
		int m_id;
		
		Piece piece;
		
		Vector3 position;
		float radius;
		
		Mask2CS m_mask;	
		
		public ActHighlightPiece(VectorInt3 pp, bool align)
		{
			posPiece = pp;
			alignment = align;
			m_id = -1;
		}
		
		public ActHighlightPiece(int id)
		{
			m_id = id;
		}
			
		public override void Start(GameInterface gi)
		{
			base.Start(gi);
			if (m_id == -1) {
				piece = gi.GetPiece(posPiece, alignment);
			} else {
				piece = gi.GetPiece(m_id);
			}
			
			var bb = piece.GetCollisionBox();
			position = (bb.Key + bb.Value).ToVector3() * 0.5f + (alignment ? new Vector3(0.5f, 0.5f, 0.5f) : Vector3.zero);
			radius = ((bb.Key-bb.Value).ToVector3().magnitude * 0.5f * 1.5f);
			
			// place and display mask
			m_mask = Camera.mainCamera.transform.Find("mask2").GetComponent<Mask2CS>();
			m_mask.SetAreaOfInterestSmooth(position, radius, 0.5f);
			
			Done = true;
		}
			
		public override void Stop()
		{
		}
	}
	
	public class ActHighlightNothing : Act
	{
		public override void Start(GameInterface gi)
		{	
			// remove mask
			Mask2CS mask = Camera.mainCamera.transform.Find("mask2").GetComponent<Mask2CS>();
			mask.SetAreaOfInterestSmooth(Vector3.zero, 100f, 0.5f);
		
			Done = true;
		}
		
	}
	
	public class ActUserAchievementThreshold : Act
	{
		int m_score; 
		
		public ActUserAchievementThreshold(int score)
		{
			m_score = score;
		}
		
		public override void Start(GameInterface gi)
		{
			base.Start(gi);
			
			Done = false;
		}
			
		public void UpdateDone() 
		{
			m_done = m_gi.GetScore1() < m_score;
		}
	}
		
		
	public class ActTimer : Act
	{
		public float m_duration;
		protected float m_remaining;
			
		public override bool Done { get { return m_duration < 0; } }
			
		//
		//
		
		public ActTimer(float duration)
		{
			m_duration = duration;
			m_done = true;
		}
		
		public override void Start(GameInterface gi)
		{
			base.Start(gi);
			
			m_done = false;
		}
			
		public override void Progress(float seconds)
		{
			if (!m_done && m_duration > 0){
				m_duration -= seconds;
			}
		}
	}
		
	public class ActUserMovePiece : Act
	{
		
		public VectorInt3 m_posPieceInitial;
		public bool m_alignmentPiece;
		
		protected VectorInt3 m_posPieceTarget;
		protected VectorInt3 m_posPieceTargetMin;
		protected VectorInt3 m_posPieceTargetMax;
		
		protected Piece m_piece;
		public Piece piece {  get { return m_piece; } }
			
		//
		//
			
		public ActUserMovePiece(VectorInt3 posInitial, bool alignment, VectorInt3 posTargetMin, VectorInt3 posTargetMax)
		{
			m_posPieceInitial = posInitial;
			m_alignmentPiece = alignment;
			
			m_posPieceTargetMin = posTargetMin;
			m_posPieceTargetMax = posTargetMax;
			m_posPieceTarget = posTargetMin;
		}
			
		public ActUserMovePiece(VectorInt3 posInitial, bool alignment, VectorInt3 posTarget)
		{
			m_posPieceInitial = posInitial;
			m_alignmentPiece = alignment;
			m_posPieceTarget = posTarget;
			m_posPieceTargetMin = m_posPieceTargetMax = posTarget;
		}
		
		public override void Start(GameInterface gi)
		{
			base.Start(gi);
			
			m_piece = m_gi.GetPiece(m_posPieceInitial, m_alignmentPiece);
		}	
		
		public void UpdateDone()
		{
			if (m_piece != null) {
				VectorInt3 pp = m_piece.GetPosition();
				if (m_posPieceTarget == m_posPieceTargetMin && 
					m_posPieceTarget == m_posPieceTargetMax){
					if (pp == m_posPieceTarget){
						m_done = true;
					} else {
						m_done = false;
					}
				} else {
					if (pp.x <= m_posPieceTargetMax.x &&
						pp.y <= m_posPieceTargetMax.y &&
						pp.z <= m_posPieceTargetMax.z &&
						pp.x >= m_posPieceTargetMin.x &&
						pp.y >= m_posPieceTargetMin.y &&
						pp.z >= m_posPieceTargetMin.z 
						){
						m_done = true;
					} else {
						m_done = false;
					}
				}
			} 	
		}
	}
			
	public class ActUserSelect : Act
	{
		public VectorInt3 m_posPiece;
		public bool m_alignmentPiece;
		public int m_pieceId;
		protected Piece m_piece;
		public Piece piece {  get { return m_piece; } }
			
		public override bool Done { get { return (m_piece != null) ? m_piece.IsSelected() : false; } }
			
		//
		//
			
		public ActUserSelect(VectorInt3 pos, bool alignment)
		{
			m_posPiece = pos;
			m_alignmentPiece = alignment;
			m_pieceId = -1;
		}
		
		public ActUserSelect(int id)
		{
			m_pieceId = id;
		}
		
		public override void Start(GameInterface gi)
		{
			base.Start(gi);
			
			if (m_pieceId == -1){
				m_piece = gi.GetPiece(m_posPiece, m_alignmentPiece);
			} else {
				m_piece = gi.GetPiece(m_pieceId);
			}
		}
	}
		
	public class ActUserSimplify : Act
	{	
	}
	
	public class ActUserTeleport : Act
	{
	}
	
	public class ActUserStartInjectorMove : Act
	{	
	}
	
	public class ActUserCommitInjectorMove : Act
	{	
	}
	
	public class ActUserRotateCamera : Act
	{	
		Vector3 m_posCameraOrig;
		
		public override void Start(GameInterface gi)
		{
			base.Start(gi);
			
			m_posCameraOrig = Camera.mainCamera.transform.position;
			m_done = false;
		}
		
		public override bool Done { get {
				return (m_posCameraOrig - Camera.mainCamera.transform.position).sqrMagnitude > 20f;
			}
			
		}
	}
}