using UnityEngine;
using System.Collections.Generic;


using tools;
using pcs;


namespace ui
{
	
public abstract class UInput
{
	public enum Typ
	{
		SwitchCameraProjection,
		LookAtSceneFrom,
		
		UndoLastAction,
		
		SetTimeDirection,
		
		SelectPiece,
		SelectBlock,	
		SelectFrustrum,	
		UnselectAll,
	
		HitPieceRay,
		GetProminentAxes,
//		SetCursorAxes,
		SetCursorAxisActive,
		SetCursorAxisNormal,
		UpdateCursorPos,
		MatchAxis,
		ShowCursor,
		SaveProgress,
		LoadProgress,
		Screenshot,
		ShowHelp,
		MoveCamera,
		MoveSelection,
		
		Bridge,
		Teleport,
		Simplify,
		RotateInjector,
		
		StartTubeSplit,
		StartInjectorMove,
		PreviewBoxAt,
		PreviewInjectorAt,
		CommitTubeSplit,
		CommitInjectorMove,
		CancelPlacement,
		
		BeginMove,
		BeginMoveDuplicate,
		BeginMoveCrossing,
		
		UIMoveSelection,
				
		
		EndMove,
	}
		
	public abstract Typ Type{ get; }
}
	
	
public class UISwitchCameraProjection : UInput
{
	public override Typ Type{ get { return UInput.Typ.SwitchCameraProjection; } }
}
	
	
	
public class UILookAtSceneFrom : UInput
{
	public override Typ Type{ get { return UInput.Typ.LookAtSceneFrom; } }
		
	public Direction dir;
		
	public UILookAtSceneFrom(Direction r){ dir = r; }
}


// Instantaneous operations (selection independent)

public class UIUndoLastAction : UInput
{
	public override Typ Type{ get { return UInput.Typ.UndoLastAction; } }
}
	
public class UISetTimeDirection : UInput
{
	public override Typ Type{ get { return UInput.Typ.SetTimeDirection; } }
		
	public Direction dir;	
		
	public UISetTimeDirection(Direction r){ dir = r; }
}

public class UISelectPiece : UInput
{
	public override Typ Type{ get { return UInput.Typ.SelectPiece; } }
		
//	public Piece m_piece;
	public VectorInt3 m_pos;
	public bool m_alignment;
	public bool m_invert;
	public int m_id = -1;
		
//	public UISelectPiece(Piece p, bool invert){ m_piece = p; m_invert = invert; }
	public UISelectPiece(int id, bool invert){ m_id = id; m_invert = invert; }
	public UISelectPiece(VectorInt3 pos, bool alignment, bool invert){ m_pos = pos; m_alignment = alignment; m_invert = invert; }
	
	public override bool Equals(object ob) 
	{
		if (ob is UISelectPiece){
			UISelectPiece other = (UISelectPiece)ob;
			return this.m_invert == other.m_invert &&
					(
						(	
							this.m_pos == other.m_pos && 
							this.m_alignment == other.m_alignment
						) || (
							this.m_id == other.m_id
						)
					);
		} else {
			return false;
		}
	}
}
	
public class UISelectBlock : UInput
{
	public override Typ Type{ get { return UInput.Typ.SelectBlock; } }
		
	public Piece m_piece;
	public bool m_invert;
		
	public UISelectBlock(Piece piece, bool invert){ m_piece = piece; m_invert = invert; }
}	
	
public class UISelectFrustrum : UInput
{
	public override Typ Type{ get { return UInput.Typ.SelectFrustrum; } }
		
	public Rect rectViewport; 
	public bool additive;
		
	public UISelectFrustrum(Rect r, bool addit){ rectViewport = r; additive = addit; }
}
	
public class UIUnselectAll : UInput
{
	public override Typ Type{ get { return UInput.Typ.UnselectAll; } }
		
}

// TODO

public class UIHitPieceRay : UInput
{
	public override Typ Type{ get { return UInput.Typ.HitPieceRay; } }
		
	public Ray ray; 
	public PieceHit result;
		
	public UIHitPieceRay(Ray r){ ray = r; result = new PieceHit(); }
}
	
public class UIGetProminentAxes : UInput
{
	public override Typ Type{ get { return UInput.Typ.GetProminentAxes; } }
		
	public Vector3 p; 
	public List<Axis> result;
		
	public UIGetProminentAxes(Vector3 r){ p = r; result = new List<Axis>(); }
}
	
//public class UISetCursorAxes : UInput
//{
//	public override Typ Type{ get { return UInput.Typ.SetCursorAxes; } }
//	
//	public Axis active; 
//	public Axis normal;
//		
//	public UISetCursorAxes(Axis a, Axis n){ active = a; normal = n; }
//		
//}
	
public class UISetCursorAxisActive : UInput
{
	public override Typ Type{ get { return UInput.Typ.SetCursorAxisActive; } }
		
	public Axis active; 
		
	public UISetCursorAxisActive(Axis a){ active = a; }
		
	public override bool Equals(object ob) 
	{
		if (ob is UISetCursorAxisActive){
			UISetCursorAxisActive other = (UISetCursorAxisActive)ob;
			return this.active == other.active;
		} else {
			return false;
		}
	}
}
	
public class UISetCursorAxisNormal : UInput
{
	public override Typ Type{ get { return UInput.Typ.SetCursorAxisNormal; } }
		
	public Axis normal;
		
	public UISetCursorAxisNormal(Axis n){ normal = n; }
		
}
	
public class UIUpdateCursorPos : UInput
{
	public override Typ Type{ get { return UInput.Typ.UpdateCursorPos; } }
		
}
	
public class UIMatchAxis : UInput
{
	public override Typ Type{ get { return UInput.Typ.MatchAxis; } }
		
	public Vector3 pos;
	public Vector2 offsetViewport; 
	public List<Axis> candidates; 
	public Axis result;
		
	public UIMatchAxis(Vector3 p, Vector2 offset, List<Axis> cand){ pos = p; offsetViewport = offset; candidates = cand; result = Axis.INVALID; }
}
	
public class UIShowCursor : UInput
{
	public override Typ Type{ get { return UInput.Typ.ShowCursor; } }
		
	public bool show; 
	public float delay;
		
	public UIShowCursor(bool r, float d){ show = r; delay = d; }
}
	
public class UISaveProgress : UInput
{
	public override Typ Type{ get { return UInput.Typ.SaveProgress; } }
		
	public int slot;
		
	public UISaveProgress(int s){ slot = s; }
}
	
public class UILoadProgress : UInput
{
	public override Typ Type{ get { return UInput.Typ.LoadProgress; } }
		
	public int slot;
		
	public UILoadProgress(int s){ slot = s; }
}
	
public class UIScreenshot : UInput
{
	public override Typ Type{ get { return UInput.Typ.Screenshot; } }
		
}
	
public class UIShowHelp : UInput
{
	public override Typ Type{ get { return UInput.Typ.ShowHelp; } }
		
}
	
public class UIMoveCamera : UInput
{
	public override Typ Type{ get { return UInput.Typ.MoveCamera; } }
		
	public Vector3 v;
		
	public UIMoveCamera(Vector3 r){ v = r; }
}
	
public class UIMoveSelection : UInput
{
	public override Typ Type{ get { return UInput.Typ.MoveSelection; } }
		
	public Vector2 v;
		
	public UIMoveSelection(Vector2 r){ v = r; }
}

//public class UIMoveSelection : UInput
//{
//	public override Typ Type{ get { return UInput.Typ.MoveSelection; } }
//		
//	public Axis axis; s
//	public float distance;	
//		
//	public UIMoveSelection(Axis a, float dist){ axis = a; distance = dist; }
//}	

// Instantaneous operations (selection dependent)

public class UIBridge : UInput
{
	public override Typ Type{ get { return UInput.Typ.Bridge; } }
		
	
}
	
public class UITeleport : UInput
{
	public override Typ Type{ get { return UInput.Typ.Teleport; } }
		
}
	
public class UISimplify : UInput
{
	public override Typ Type{ get { return UInput.Typ.Simplify; } }
		
}
	
public class UIRotateInjector : UInput
{
	public override Typ Type{ get { return UInput.Typ.RotateInjector; } }
		
}
	

// Discrete timed operations

public class UIStartTubeSplit : UInput
{
	public override Typ Type{ get { return UInput.Typ.StartTubeSplit; } }
		
}
	
public class UIStartInjectorMove : UInput
{
	public override Typ Type{ get { return UInput.Typ.StartInjectorMove; } }
		
	
}
	
public class UIPreviewBoxAt : UInput
{
	public override Typ Type{ get { return UInput.Typ.PreviewBoxAt; } }
		
	public Vector3 pos;
		
	public UIPreviewBoxAt(Vector3 r){ pos = r; }
}
	
public class UIPreviewInjectorAt : UInput
{
	public override Typ Type{ get { return UInput.Typ.PreviewInjectorAt; } }
		
	public VectorInt3 pos;
	public bool alignment;
	public Vector3 posHit;
		
	public UIPreviewInjectorAt(VectorInt3 p, bool align, Vector3 pHit){ pos = p; alignment = align; posHit = pHit; }
		
	public override bool Equals(object ob) 
	{
		if (ob is UIPreviewInjectorAt){
			UIPreviewInjectorAt other = (UIPreviewInjectorAt)ob;
			return this.pos == other.pos && 
					this.alignment == other.alignment;
				// skip posHit as we dont care where the piece was hit.
		} else {
			return false;
		}
	}
}
	
public class UICommitTubeSplit : UInput
{
	public override Typ Type{ get { return UInput.Typ.CommitTubeSplit; } }
		
}
	
public class UICommitInjectorMove : UInput
{
	public override Typ Type{ get { return UInput.Typ.CommitInjectorMove; } }
		
}
	
public class UICancelPlacement : UInput
{
	public override Typ Type{ get { return UInput.Typ.CancelPlacement; } }
		
}
	

// Continuous timed operations

public class UIBeginMove : UInput
{
	public override Typ Type{ get { return UInput.Typ.BeginMove; } }
		
	public Axis axis;
		
	public UIBeginMove(Axis r){ axis = r; }
}
	
public class UIBeginMoveDuplicate : UInput
{
	public override Typ Type{ get { return UInput.Typ.BeginMoveDuplicate; } }
		
	public Axis axis;
		
	public UIBeginMoveDuplicate(Axis r){ axis = r; }
}
	
public class UIBeginMoveCrossing : UInput
{
	public override Typ Type{ get { return UInput.Typ.BeginMoveCrossing; } }
		
	public Axis axis;
		
	public UIBeginMoveCrossing(Axis r){ axis = r; }
}
	

	

public class UIEndMove : UInput
{
	public override Typ Type{ get { return UInput.Typ.EndMove; } }
		
}
	
	
}
