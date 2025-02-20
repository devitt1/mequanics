using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using pcs;
using tools;
using operation;
using parser;


/**
 * Manages a circuit
 *
 * This class holds a list of pieces and keep them in a coherent state. It
 * handles all the possible actions that one can make and supports smooth
 * animations for things like moving.
 */
public class Circuit : IEventHandler
{
	/** enum Action lists all things the user 
	* 	can actually do to a circuit. 
	* */
	protected enum Action
	{
		NONE,
		MOVE,
		SPLIT,
		MOVE_INJECTOR,
		MOVE_DUPLICATE
	}
	
	// 
	public static Vector3 s_offsetSpaces = new Vector3(0.5f, 0.5f, -0.5f);
	
	protected static SelectionCS s_selection;
	protected static SelectionCS Selected { get {
			if (s_selection == null){
				GameObject go = GameObject.FindGameObjectWithTag("selection");
				s_selection = go.GetComponent<SelectionCS>();
			}
			return s_selection;
		} 
	}
	
	/// The list of pieces for both alignments
	protected LinkedList<Piece>[] m_pieces = new LinkedList<Piece>[2];
	
	/// The list of pieces carrying an ID
	protected Dictionary<int, Piece> m_piecesVIP = new Dictionary<int, Piece>();
	
	/// Pieces used to fill gaps in animations
	protected LinkedList<Piece> m_gapFillers = new LinkedList<Piece>();
	
	public int NumPieces0 { get { return m_pieces[0].Count; } }
	public int NumPieces1 { get { return m_pieces[1].Count; } }
	
	/// The list of selected pieces
	protected HashSet<Piece> m_selection = new HashSet<Piece>();
	public int SelectionCount { get { return m_selection.Count; } }

	/// The undo history
	protected LinkedList<Step> m_steps = new LinkedList<Step>();
	/// Save actions in history
	protected bool m_saveSteps;

	/// True if a collision with piece of same alignment has occured
	protected bool m_isAlignColliding;
	/// True if a collision with piece of opposite alignment has occured
	protected bool m_isUnalignColliding;
	/// True if an injector is beeing sqeezed
	protected bool m_isSqueezing;
	/// True if a duplication put the circuit in a bad state
	protected bool m_isBadDuplicate;

	
//	/**
//	 * The offset of movement
//	 *
//	 * This is the offset according to the last MoveSelection 
//	 *
//	 * \sa SmoothMoveSelection
//	 */
	protected float m_offset;
	public float Offset { get { return m_offset; } }
	
//	/// Current move direction
	// i.e. the direction the move was started into ???
	protected Direction m_moveDirection;
	
	/**
	 * Number of steps since start of move
	 *
	 * This is orienteted, if negative it means that these are backward steps.
	 * Use m_moveDirection to get the axis of the move.
	 *
	 * \warning m_moveDirection == LEFT && m_moveSteps = 8 does *NOT* mean 8
	 * steps on the left!
	 */
	protected int m_moveSteps;
	
	/**
	 * Last position that is known to be a good state
	 * Given as offset from a move Starting point in positive axis direction
	 */
	protected int m_safeMoveSteps;
	
	/**
	 * List of potential boxes to merge
	 *
	 * \sa MoveBoxToBox
	 */
	protected List<KeyValuePair<Box, Box>> m_potentialMerge = new List<KeyValuePair<Box, Box>>();
	/**
	 * List of tubes that may be removed at the end of a move
	 */
	protected List<Piece> m_potentialRemove = new List<Piece>();
	/**
	 * List of boxes that should be considered as fixed
	 *
	 * Theses boxes are the boxes that were merged during a move and should
	 * reappear if we move another step.
	 *
	 * This is also used for undoing moves, we set the fixed boxes in step 0
	 * before calling MoveSelection.
	 *
	 * We keep only coordinates since the boxes themselves have already been
	 * destroyed. We sort them by step number.
	 */
	protected Dictionary<int, HashSet<KeyValuePair<bool, VectorInt3>>> m_fixedBoxes = 
		new Dictionary<int, HashSet<KeyValuePair<bool, VectorInt3>>>();

	/// The injector that is being moved
	protected Injector m_movingInjector;

	/// The ghost piece to show
	protected Piece m_ghostPiece;
	/// The target on which the ghost piece is
	protected Piece m_ghostTarget;

	/// Current action
	protected Action m_action;

//	/// The pieces that have been duplicated during the move
	protected List<Piece> m_duplicateSource = new List<Piece>();
	protected Tube m_duplicateSourceTube = null;
	/// When duplicating, box from which the path has been cut
	protected Box m_duplicateCutBox;
	/// Limit of the move for duplication move (in case there are fixed boxes)
	protected int m_maxDuplicateMove;

	/// True if the time axis is locked (because of chains of injectors)
	public bool m_timeLocked;
	/// Time axis
	public Direction m_timeDir;
	/// List of injectors by id (block ID ?)
	protected Dictionary<int, List<Injector>> m_injectors = new Dictionary<int, List<Injector>>();
	/// True if injectors are in wrong order
	public bool m_injectionBad;

	/// True if doing a crossing move
	protected bool m_crossing;
	/// List of unstable tag pairs
	protected HashSet<KeyValuePair<uint, uint>> m_unstable = new HashSet<KeyValuePair<uint, uint>>();
	
	
	protected CircuitRenderer m_renderer;
	
	//--------------------------------------------
	
	/// Constructor. 
	public Circuit()
	{
		this.m_saveSteps = true;
		this.m_isAlignColliding = false;
		this.m_isUnalignColliding = false;
		this.m_isSqueezing = false;
		this.m_isBadDuplicate = false;
		this.m_offset = 0F;
		this.m_moveDirection = Direction.INVALID;
		this.m_moveSteps = 0;
		this.m_safeMoveSteps = 0;
		this.m_action = Action.NONE;
		this.m_crossing = false;
		
		this.m_pieces[0] = new LinkedList<Piece>();
		this.m_pieces[1] = new LinkedList<Piece>();
		
		InitVisual();
	}
	
	public bool HandleEvent(UnityEngine.Event e)
	{
		return false;
	}
	
	/**
	* Create a piece (other than a box)
	* 
	* This function also creates the two bounding boxes if necessary and links
	* them together.
	*/
	public void CreatePiece(Dictionary<VectorInt3, Box>[] boxMap, PieceDescriptor pd)
	{
		Piece piece;
		switch (pd.type)
		{
			case PieceType.TUBE:
				piece = new Tube();
				break;
			case PieceType.INJECTOR:
			{
				var inj = new Injector();
				if (pd.injectionId != -1) {
					if (!m_injectors.ContainsKey(pd.injectionId)){
						m_injectors.Add(pd.injectionId, new List<Injector>());
					}
					m_injectors[pd.injectionId].Add(inj);
				}
				piece = inj;
			}
			break;
		case PieceType.CAP:
			piece = new Cap();
			break;
		default:
			System.Diagnostics.Debug.Assert(false);
			return;
		}
    
		piece.SetAxis(pd.axis);
		piece.SetPosition(pd.align, pd.position);
		if (pd.type == PieceType.TUBE){
			piece.SetLength(pd.length);
		}
		piece.SetColor(pd.color);
		piece.SetInjectionId(pd.injectionId);
		piece.SetTags(new HashSet<uint>(pd.tags));
			
		piece.InitVisual();	
		piece.InitPhysics();	
		
		int nAlign = pd.align ? 1 : 0;
		m_pieces[nAlign].AddLast(piece);
		

		switch (pd.axis)
		{
		case Axis.X:
			BindNeighbor(boxMap[nAlign], Direction.RIGHT, pd.position, piece).SetColor(pd.color);
			BindNeighbor(boxMap[nAlign], Direction.LEFT, pd.position, piece).SetColor(pd.color);
			break;
		case Axis.Y:
			BindNeighbor(boxMap[nAlign], Direction.UP, pd.position, piece).SetColor(pd.color);
			BindNeighbor(boxMap[nAlign], Direction.DOWN, pd.position, piece).SetColor(pd.color);
			break;
		case Axis.Z:
			BindNeighbor(boxMap[nAlign], Direction.FRONT, pd.position, piece).SetColor(pd.color);
			BindNeighbor(boxMap[nAlign], Direction.REAR, pd.position, piece).SetColor(pd.color);
			break;
		default:
			throw new ex.InvalidValue();
		}
    
		if (UnityEngine.Debug.isDebugBuild){
			FullCoherenceCheck();
		}
	}
	

	/**
	 * Remove piece from the circuit's piece list but 
	 * keep the object (including visuals)
	 */
	public void DeletePiece(Piece piece)
	{
		int nAlign = piece.GetAlignment() ? 1 : 0;
			/// \todo slow
			LinkedList<Piece> pieceList =  m_pieces[nAlign];
			foreach (Piece p in pieceList){
				if (p == piece){
					pieceList.Remove(p);	
					return;
				}
			}
	}
	
	/**
	 * Delete a piece at a given position
	 *
	 * \warning this function does not remove the piece from the selection
	 */
	public bool DeletePiece(bool align, VectorInt3 pos)
	{
		/// \todo slow
		/// \todo use decltype...
		int nAlign = align ? 1 : 0;
		foreach (Piece p in m_pieces[nAlign])
		{
		if (p.GetPosition() == pos)
		{
			m_pieces[nAlign].Remove(p);
			return true;
		}
		}
    
		return false;
	}
	
	/**
	 * Get the piece with position or nullptr
	 */
	public Piece GetPiece(bool align, VectorInt3 pos)
	{
		/// \todo slow //TODO2
		int nAlign = align ? 1 : 0;
		foreach (var p in m_pieces[nAlign]){
			if (p.GetPosition() == pos){
				return p;
			}
		}
		return null;
	}
	
	public Piece GetPiece(int id)
	{
		if (m_piecesVIP.ContainsKey(id))
		{
			return m_piecesVIP[id];
		}
		return null;
	}
	
	public bool SetPieceID(Piece piece, int id)
	{
		if (m_piecesVIP.ContainsKey(id) || id == -1){
			return false;
		} else {
			this.m_piecesVIP.Add(id, piece);
			piece.Id = id;
			return true;
		}
	}
	
//	public int GetPieceID(Piece piece)
//	{
//		if (m_piecesVIP.ContainsValue(piece)){
//			return m_piecesVIP.TryGetValue(;
//		} else {
//			return -1;
//		}
//	}

	/**
	 * Get the bounding box of the circuit
	 */
	public KeyValuePair<Vector3, Vector3> GetBoundingBox()
	{
		Vector3 min = new Vector3(System.Single.PositiveInfinity, System.Single.PositiveInfinity, System.Single.PositiveInfinity);
		Vector3 max = -min;

		foreach (Piece piece in m_pieces[0]){
			var box = piece.GetCollisionBox();
			if (box.Key.x < min.x) { min.x = box.Key.x; }
			if (box.Key.y < min.y) { min.y = box.Key.y; }
			if (box.Key.z < min.z) { min.z = box.Key.z; }
			if (box.Value.x > max.x) { max.x = box.Value.x; }
			if (box.Value.y > max.y) { max.y = box.Value.y; }
			if (box.Value.z > max.z) { max.z = box.Value.z; }
		}

		foreach (Piece piece in m_pieces[1]){
			var box = piece.GetCollisionBox();
			Vector3 vMinPiece = box.Key.ToVector3();
			Vector3 vMaxPiece = box.Value.ToVector3();
			vMinPiece += s_offsetSpaces;
			vMaxPiece += s_offsetSpaces;
			if (vMinPiece.x < min.x) { min.x = vMinPiece.x; }
			if (vMinPiece.y < min.y) { min.y = vMinPiece.y; }
			if (vMinPiece.z < min.z) { min.z = vMinPiece.z; }
			if (vMaxPiece.x > max.x) { max.x = vMaxPiece.x; }
			if (vMaxPiece.y > max.y) { max.y = vMaxPiece.y; }
			if (vMaxPiece.z > max.z) { max.z = vMaxPiece.z; }
		}
    
		return new KeyValuePair<Vector3, Vector3>(min, max);
	}

	/**
	 * Cancel a pending injector-move or tube-split
	 */
	public void CancelPlacement()
	{
		switch (m_action)
		{
		case Action.MOVE_INJECTOR:
			m_action = Action.NONE;
			m_movingInjector = null;
			if (m_ghostPiece != null){
				m_ghostPiece.UninitVisual();
			}
			m_ghostPiece = null;
			m_ghostTarget = null;
			break;
		case Action.SPLIT:
			m_action = Action.NONE;
			if (m_ghostPiece != null){
				m_ghostPiece.UninitVisual();
			}
			m_ghostPiece = null;
			m_ghostTarget = null;
			break;
		default:
			break;
		}
	}
	
	
	/**
	 * Return true if an action is in progress
	 */

	public bool IsActionInProgress()
	{
		return m_action != Action.NONE;
	}

	/**
	 * Undo last action
	 */
	public void UndoLastAction()
	{
		if (m_steps.Count == 0 || m_action != Action.NONE)
		{
			System.Diagnostics.Debug.WriteLine("nothing to undo or action in progress");
			
			throw new QBException(QBException.Type.UNDO_NO_MORE);
		}
    
		System.Diagnostics.Debug.WriteLine("undo, step type: " + m_steps.Last.Value.Type);

		m_saveSteps = false;
		m_steps.Last.Value.GetInverse().Execute(this);
		m_saveSteps = true;
    
		// no animation for undo
		ResetOffsets();
		// injectors may have been reordered
		CheckInjection();
    
		// we have to pop the move we just made and the move we undid
		m_steps.RemoveLast();
	}

//	/// \name Selection
//	/// \{
	/**
	 * Select a piece
	 *
	 * If the piece is a not a box, it's bounding boxes are selected too.
	 */
	public void Select(Piece piece)
	{
		System.Diagnostics.Debug.Assert(!(IsActionInProgress()));
		System.Diagnostics.Debug.Assert(piece != null);
    
		SelectOnly(piece);
    
		// if this is not a box, select surrounding boxes
		if (piece.GetType() != PieceType.BOX){
			foreach (KeyValuePair<Direction, Piece> p in piece){
				SelectOnly(p.Value);
			}
		}
	}
	
	/**
	 * Select all pieces
	 */
	public void SelectAll()
	{
			System.Diagnostics.Debug.Assert(!IsActionInProgress());
    
			foreach (var pieces in m_pieces){
			foreach (Piece p in pieces)
			{
				p.SetSelected(true);
				m_selection.Add(p);
			}
		}
	}
	
	
	/**
	 * Unselect a piece
	 *
	 * This may unselect bounding boxes if they are not linked to the selection
	 * anymore.
	 */
	public void Unselect(Piece piece)
	{
		System.Diagnostics.Debug.Assert(!IsActionInProgress());
		System.Diagnostics.Debug.Assert(piece.IsSelected());
    
		UnselectOnly(piece);
    
		// if it's a box, then also unselect all neighbors that are not boxes
		if (piece.GetType() == PieceType.BOX)
		{
			foreach (KeyValuePair<Direction, Piece> p in piece)
			{
				if (p.Value.IsSelected() && p.Value.GetType() != PieceType.BOX)
				UnselectOnly(p.Value);
			}
		}
		// if it's not a box
		else
		{
			// we unselect neighbor boxes if they don't have any selected non-box
			// neighbor
			foreach (KeyValuePair<Direction, Piece> p in piece)
			{
				bool unselect = true;
				foreach (KeyValuePair<Direction, Piece> p2 in p.Value)
				if (p2.Value.GetType() != PieceType.BOX && p2.Value.IsSelected())
				{
					unselect = false;
					break;
				}
	    
					if (unselect){
					UnselectOnly(p.Value);
				}
			}
		}
	}
	
	/**
	 * Clear selection
	 */
	public void UnselectAll()
	{
		foreach (var p in m_selection){
			p.SetSelected(false);
		}
		m_selection.Clear();
	}

	/**
	 * Select only a piece
	 *
	 * No check is done, be sure that the selection boundaries are all boxes !
	 */
	public void SelectOnly(Piece piece)
	{
		System.Diagnostics.Debug.Assert(piece != null);
    
		if (piece.IsSelected()){
			return;
		}

		System.Diagnostics.Debug.WriteLine("selecting " + piece + " at " + piece.GetPosition().ToString());
    
		piece.SetSelected(true);
		m_selection.Add(piece);
	}
	
	/**
	 * Unselect only a piece
	 *
	 * No check is done, be sure that the selection boundaries are all boxes !
	 */
	public void UnselectOnly(Piece piece)
	{
		System.Diagnostics.Debug.Assert(piece != null);
    
			if (!piece.IsSelected()){
			return;
		}
    
		piece.SetSelected(false);
		m_selection.Remove(piece);
	}
	
	public void SelectFrustrum(Plane[] planes)
	{
		List<Piece> piecesHit = new List<Piece>();
		foreach (var pieceList in this.m_pieces){
			foreach(Piece p in pieceList){
				if (p.IsCompletelyInsideFrustrum(ref planes)){
					piecesHit.Add(p);
				}
			}
		}
		foreach(Piece p in piecesHit){
			Select(p);
		}
	}
	
//	/// \}

	
	
	/**
	 * Move smoothly the selection of offset in axis a
	 * returns true if SmoothMoveStep was applied
	 */
	public bool SmoothMoveSelection(float offset, Axis a)
	{
		bool result = false;
		
		// Some validity testing
		if (m_action == Action.NONE){
			m_action = Action.MOVE;
		} else if (m_action != Action.MOVE && m_action != Action.MOVE_DUPLICATE){
			return result;
		}
		
		// Move all pieces about offset and adjust topology where necessary
		
		if (m_moveDirection != Direction.INVALID) // we were already moving
		{ 
			// Apply offset
			if(Tools.DirectionIsPositive(m_moveDirection)){
				m_offset += offset;
			} else { // the move direction is negative
				// so we invert the offset
				m_offset -= offset;
			}
			
			while(m_offset > 1) // we moved in the same direction for more than 1 grid unit
			{
				if (GetWeakState()) // circuit is NOT in "weak state", so we can begin another step
				{
					  // Apply another discrete movement step 
					
					ResetOffsets();
					if (!Tools.DirectionIsPositive(m_moveDirection)){
							SmoothMoveStep(Tools.GetDirectionsFromAxis(a).Key);			
					} else {
							SmoothMoveStep(Tools.GetDirectionsFromAxis(a).Value);
					}
					result = true;
					
					// the move may have been blocked
					if (m_moveDirection != Direction.INVALID){
						m_offset -= 1;
					} else {
						System.Diagnostics.Debug.Assert(m_action == Action.MOVE_DUPLICATE);
					}
						
					// test if the target location may be occupied by the moving pieces
					CheckCollisions();
					CheckSqueezing();
					CheckBadDuplicate();
					CheckInjection();
					if (IsMoveLegal()){
							m_safeMoveSteps = m_moveSteps;
					}
				}
				else // circuit is in "weak state", so we can NOT begin another step
				{
					System.Diagnostics.Debug.WriteLine("colliding or squeezing, can't go further");
					// re-substract the latest offset
					if (!Tools.DirectionIsPositive(m_moveDirection)) {
							m_offset += offset;
					} else {
							m_offset -= offset;
					}
					break;
				}
			}
			
			if(m_offset < 0) // we moved backwards
			{
				System.Diagnostics.Debug.WriteLine("negative overflow");
					
				// Apply 2 discrete steps: 
				// one to cancel out the previous step and another 
				// one to proceed into the opposite direction
				if (!Tools.DirectionIsPositive(m_moveDirection))
				{
					SmoothMoveStep(Tools.GetDirectionsFromAxis(a).Value);
					ResetOffsets();
					CheckCollisions(); // needed by crossing
					SmoothMoveStep(Tools.GetDirectionsFromAxis(a).Value);
				}
				else
				{
					SmoothMoveStep(Tools.GetDirectionsFromAxis(a).Key);
					ResetOffsets();
					CheckCollisions(); // needed by crossing
					SmoothMoveStep(Tools.GetDirectionsFromAxis(a).Key);
				}
				result = true;
				
				// invert the official movement direction and the offset
				m_offset = -m_offset;
				if (m_moveDirection != Direction.INVALID){ // the move may have been blocked
					m_moveDirection = Tools.InvertDirection(m_moveDirection);
				} else {
					System.Diagnostics.Debug.Assert(m_action == Action.MOVE_DUPLICATE);
				}
			
				// test if the target location may be occupied by the moving pieces
				CheckCollisions();
				CheckSqueezing();
				CheckBadDuplicate();
				CheckInjection();
				if (IsMoveLegal()){
					m_safeMoveSteps = m_moveSteps;
				}
			}
		}
		else // We were not moving anything before
		{
			// Begin a discrete movement step
			m_offset += offset;
			
			// set move direction
			System.Diagnostics.Debug.WriteLine("not moving, starting");
			var dirs = Tools.GetDirectionsFromAxis(a);
			if (m_offset > 0){
				m_moveDirection = dirs.Value;
			} else {
				m_moveDirection = dirs.Key;
				m_offset = -m_offset;
			}
			
			// Apply a discrete movement step (1 grid unit)
			SmoothMoveStep(m_moveDirection); 
			result = true;
		  
			// test if the target location may be occupied by the moving pieces
			CheckCollisions();
			CheckSqueezing();
			CheckBadDuplicate();
			CheckInjection();
			if (IsMoveLegal()){ 
				m_safeMoveSteps = m_moveSteps;
			}
		}
		
		//NEW Move the selection parent node
//		Vector3 off = Vector3.zero;
//		off[(int)a] = m_offset; 
//		Selected.SetOffset(off);
	
		return result;
	}
	
	/**
	 * Commit the move started with SmoothMoveSelection
	 *
	 * Saves a step in undo history.
	 * Triggered when mouse button released after a Move drag or Duplicate drag
	 */
	public void CommitMove()
	{
		// Check we are actually doing a move right now
		if (m_action != Action.MOVE && m_action != Action.MOVE_DUPLICATE){
			return;
		}
		
		if (m_moveDirection != Direction.INVALID)
		{
			if (m_offset < 0.5 || !IsMoveLegal()) { // current move should not be completed
				// we cancel the last move
				// UnityEngine.Debug.Log("Undoing the last step");
				SmoothMoveStep(Tools.InvertDirection(m_moveDirection));
			}
	    
			// 
			if (m_duplicateSource.Count > 0)
			{
				if (System.Math.Abs(m_moveSteps) < 5)
				{
					System.Diagnostics.Debug.WriteLine("not far enough, cancelling");
					// cancel the move
					if (m_moveSteps > 0){
						while (m_moveSteps != 0){
							SmoothMoveStep(Tools.GetNegativeDirection(m_moveDirection));
						}
					} else {
						while (m_moveSteps != 0){
							SmoothMoveStep(Tools.GetPositiveDirection(m_moveDirection));
						}
					}
				} else {
					UnityEngine.Debug.Log("just far enough, commiting duplicate");
					System.Diagnostics.Debug.Assert(System.Math.Abs(m_moveSteps) == 5);
					CommitDuplicate();
				}
			}
			
			if (m_action == Action.MOVE_DUPLICATE)
			{
				System.Diagnostics.Debug.WriteLine("cancelling duplication");
				System.Diagnostics.Debug.Assert(m_moveSteps == 0);
				CancelDuplicate();
			}
	    
			// if the last "SmoothMoveStep" was resulting in a non-good state 
			// then snap back to the last good known state
			if (m_safeMoveSteps != m_moveSteps)
			{
					int toMoveBack = m_safeMoveSteps - m_moveSteps;
				System.Diagnostics.Debug.WriteLine("last know good state was at " + m_safeMoveSteps + ", moving " + toMoveBack);
					if (toMoveBack < 0){
					while (toMoveBack++ != 0){
						SmoothMoveStep(Tools.GetNegativeDirection(m_moveDirection));
						// we need to check collisions to detect crossings
						CheckCollisions();
					}
				} else {
					while (toMoveBack-- != 0){
						SmoothMoveStep(Tools.GetPositiveDirection(m_moveDirection));
						// we need to check collisions to detect crossings
						CheckCollisions();
					}
				}
			}
    
			// add to undo queue
			if (m_moveSteps != 0)
			{
				Move move;
				if (m_moveSteps > 0){
					move = new Move(m_selection, Tools.GetPositiveDirection(m_moveDirection), (uint)m_moveSteps);
					// Disregard fixed boxes outside the range actually travelled
					//TODO3 use different API to speed up this operation
					//TODO2 check if this removal operation does exactly what the original code did
					List<int> keys = new List<int>(m_fixedBoxes.Keys);
					foreach (int fb in keys){
						if (fb <= 0 || fb > m_moveSteps){
							m_fixedBoxes.Remove(fb);
						}
					}
				}
				else
				{
					move = new Move(m_selection, Tools.GetNegativeDirection(m_moveDirection), (uint)-m_moveSteps);
						// Disregard fixed boxes outside the range actually travelled
						// we skip the 0
						//TODO3 use different API to speed up this operation (iterate sorted list)
						//TODO2 check if this removal operation does exactly what the original code did
						List<int> keys = new List<int>(m_fixedBoxes.Keys);
						foreach (int fb in keys){
							if (fb >= 0 || fb < m_moveSteps){
								m_fixedBoxes.Remove(fb);
							}
						}
				}
				// Store relevant fixed boxes for this move to Move object
					foreach (var step in m_fixedBoxes){
					foreach (var box in step.Value){
							move.AddFixedBox((uint)(m_moveSteps > 0 ? step.Key : -step.Key), box.Key, box.Value);
					}
				}
					
				// add move action to undo-queue
					m_steps.AddLast(move);
			}
		}
    
		// reset everything
		if (m_action == Action.MOVE_DUPLICATE){
			CancelDuplicate();
		}
		ResetOffsets();
		m_offset = 0;
		m_moveDirection = Direction.INVALID;
		m_action = Action.NONE;
		m_moveSteps = 0;
		m_safeMoveSteps = 0;
		m_fixedBoxes.Clear();
		m_crossing = false;
    
		// recalculate things
		CheckCollisions();
		CheckSqueezing();
		CheckBadDuplicate();
		CheckInjection();
    
		System.Diagnostics.Debug.WriteLine("number of unstable pairs: " + m_unstable.Count);
    
		// should solve any collision
		System.Diagnostics.Debug.Assert(!m_isAlignColliding && !m_isUnalignColliding);
	}
	
	/**
	 * Reset the moving offsets
	 *
	 * \todo can be protected ?
	 */
	public void ResetOffsets()
	{
		foreach (var pieceList in m_pieces){
			foreach (Piece p in pieceList){
					p.ResetOffsets();
			}
		}
	
		// Remove & Destroy all gap fillers
		ClearGapFillers();
	}
	
	/**
	 * Remove & Destroy all gap filler pieces
	 * */
	protected void ClearGapFillers()
	{
		foreach (Piece p in m_gapFillers){
			p.UninitVisual();
		}
			m_gapFillers.Clear();
	}
	
	/**
	 * 	Move the selection towards direction d about 1.0. 
	 * 	Performs topological adjustments to allow for unselected things to stay where they are. 
	 */
	public void MoveSelection(Direction d)
	{
		System.Diagnostics.Debug.WriteLine("moving in direction " + (int)d);
    
		if (m_selection.Count == 0)
		{
			System.Diagnostics.Debug.WriteLine("nothing selected");
			return;
		}
		// for any selected tube, its boxes should be selected as well
		
		// Identify boundary pieces 
		// (= all boxes connected to pieces that are not selected)
			List<Box> boundaries = new List<Box>();
		foreach (var piece in m_selection)
		{
			if (!UnityEngine.Debug.isDebugBuild){
				// a boundary must be a box
				if (piece.GetType() != PieceType.BOX){
					continue;
				}
			}
			
			bool isBoundary = false;
			foreach (KeyValuePair<Direction, Piece> p in piece)
			{
					// if not selected it's a boundary
					if (!p.Value.IsSelected()){
					isBoundary = true;
					break;
				}
			}
			
			// special case, if it's a fixed box, consider it a boundary because it will not move
			if (!isBoundary && IsFixedBox(piece, d)){
				isBoundary = true;
			}
			
			if (isBoundary){
				// boundaries are boxes
				System.Diagnostics.Debug.Assert(piece.GetType() == PieceType.BOX);
				boundaries.Add((Box)piece);
			}
		}
		
		System.Diagnostics.Debug.WriteLine("found " + boundaries.Count + " boundaries");
    
		if (UnityEngine.Debug.isDebugBuild){
			// Make sure there are no duplicates among boundary pieces
	//		{
	//			int nBoundaries = boundaries.Count;
	//			boundaries.Sort((x, y) => x.GetPosition().GetHashCode() - y.GetPosition().GetHashCode());
	//			int index = 0;
	//			while (index < boundaries.Count - 1)
	//			{
	//				  if (boundaries[index].GetPosition().GetHashCode() == boundaries[index + 1].GetPosition().GetHashCode()){ 
	//				      boundaries.RemoveAt(index);
	//				} else {
	//				      index++;
	//				}
	//			}
	//			System.Diagnostics.Debug.Assert(nBoundaries == boundaries.Count);
	//		}
		}
    
			// gather all boxes in selection 
		HashSet<Piece> remaining = new HashSet<Piece>(); // remaining contains the boxes that are not yet moved
		foreach (Piece p in m_selection){
			if (p.GetType() == PieceType.BOX){
				remaining.Add(p);
			}
		}
		System.Diagnostics.Debug.WriteLine(remaining.Count + " boxes to move");
    
		// Gather all non-boxes in selection 
		HashSet<Piece> other = new HashSet<Piece>();
    
		// Move all boundary boxes (or process as applicable)
		foreach (var box in boundaries)
		{
			System.Diagnostics.Debug.Assert(box != null);
			System.Diagnostics.Debug.WriteLine("processing " + box.GetPosition().ToString());
    
			// add the neighbor tubes to other (except those neighboring along the axis of movement 
			foreach (KeyValuePair<Direction, Piece> n in box){
				if (	n.Value.IsSelected() 
						&& n.Value.GetType() != PieceType.BOX 
						&& Tools.GetAxisFromDirection(n.Key) != Tools.GetAxisFromDirection(d)){
					other.Add(n.Value);
				}
			}
    
			// find out if the box can move 
			bool willMove = WillMove(box, d);
		  
			// Check rare case that a selected box can stay in place without hindering 
			// other pieces from moving.
			if (!willMove && CanStay(box, d))
			{
				System.Diagnostics.Debug.WriteLine("box won't move and doesn't need to");
				remaining.Remove(box);
				continue;
			}
		  
			if (willMove) 
			{
				System.Diagnostics.Debug.WriteLine("simply moving boundary");
					MoveBox(box, d);
			} 
			else 
			{
				System.Diagnostics.Debug.WriteLine("box can't move, spawning a box");
				// else, we have to connect to the neighbor box if there is one, if there
				// is not, we must duplicate the box
		  
				// we keep the list of the box neighbors that are in the selection
				LinkedList<KeyValuePair<Direction, Piece>> neighborsInSel = new LinkedList<KeyValuePair<Direction, Piece>>();
					foreach (KeyValuePair<Direction, Piece> p in box){
					if (p.Value.IsSelected()){
							neighborsInSel.AddLast(p);
					}
				}
		  
				System.Diagnostics.Debug.WriteLine("box has " + neighborsInSel.Count + " neighbors in selection");
				
				Box newBox;
				var neighbor = box.GetNeighbor(d);
				// we duplicate the box in the asked direction if it is not
				newBox = DuplicateBox(box, d);
		  
				// if the box has a neighbor in the direction of the move and it's an
				// injector or a tube, we must reduce it
				if (neighbor != null)
				{
				System.Diagnostics.Debug.WriteLine("there is a neighbor in the direction of the move");
					
					switch (neighbor.GetType())
					{
						case PieceType.INJECTOR:
						case PieceType.CAP:
						case PieceType.TUBE:
						System.Diagnostics.Debug.WriteLine("it's a tube or injector, reducing");
						ReduceTube(neighbor, d);
						break;
					case PieceType.BOX:
						System.Diagnostics.Debug.WriteLine("it's a box, we may have to merge later");
						// we may have created a box on a box that was already there
						m_potentialMerge.Add(new KeyValuePair<Box, Box>((Box)neighbor, newBox));
						break;
					default:
						System.Diagnostics.Debug.Assert(false);
						break;
					}
		  
					// the box must have the tag of that neighbor
					newBox.ReplaceTags(neighbor.GetTags());
				}
				else 
				{
					// if there is no neighbor, the tag must be removed
					newBox.GetTags().Clear();
				}
		  
				// we must add the tags of the moving neighbors that are not in the move axis
				foreach (var n in neighborsInSel){
					if (n.Key != d && n.Key != Tools.InvertDirection(d)){
						newBox.AddTags(n.Value.GetTags());
					}
				}
		  
				// also, the box we left behind must have the tags of what's behind it
				// and the non-moving neighbors that are not on the move axis
				box.GetTags().Clear();
				foreach (KeyValuePair<Direction, Piece> n in box)
				if (n.Key != d && (!n.Value.IsSelected() || n.Key == Tools.InvertDirection(d))){
						box.AddTags(n.Value.GetTags());
				}
		  
				// we unselect the first box if the opposite move direction tube is not
				// selected
				Direction di = Tools.InvertDirection(d);
				Piece p2 = box.GetNeighbor(di);
				if (p2 != null)
				{
					if (!p2.IsSelected()){
						UnselectOnly(box);
					}
				} else {
					UnselectOnly(box);
				}
				SelectOnly(newBox);
		  
				// we must bind these neighbors with the new box
				foreach (var dn in neighborsInSel)
				{
					if (dn.Key != d && dn.Key != Tools.InvertDirection(d))
					{
						System.Diagnostics.Debug.Assert(box.GetNeighbor(dn.Key) == dn.Value);
						
						box.SetNeighbor(dn.Key, null);
						newBox.SetNeighbor(dn.Key, dn.Value);
						dn.Value.SetNeighbor(Tools.InvertDirection(dn.Key), newBox);
					}
				}
			}
		  
			// we handled this box
			remaining.Remove(box);
		}
    
		System.Diagnostics.Debug.WriteLine("handling " + remaining.Count + " remaining boxes");
    
		// Move all remaining boxes (in selection)
		foreach (var box in remaining)
		{
			// add their non-box neighbors to set of further pieces to be moved (if not in there already)
			foreach (KeyValuePair<Direction, Piece> p in box){
				if (p.Value.GetType() != PieceType.BOX && Tools.GetAxisFromDirection(p.Key) != Tools.GetAxisFromDirection(d)){
					other.Add(p.Value);
				}
			}
			MoveBox((Box)box, d);
		}
		
		// Merge all box pairs that ended up in the same location
		System.Diagnostics.Debug.WriteLine("merging boxes (" + m_potentialMerge.Count + ")");
			foreach (var bb in m_potentialMerge){
			if (bb.Key.GetPosition().Equals(bb.Value.GetPosition()))
			{
//				System.Diagnostics.Debug.WriteLine("merging on " + bb.Key.GetPosition().ToString());
					MergeBoxes(bb.Key, bb.Value, d);
					if (!m_fixedBoxes.ContainsKey(m_moveSteps)){
					m_fixedBoxes[m_moveSteps] = new HashSet<KeyValuePair<bool, VectorInt3>>();
				}
					m_fixedBoxes[m_moveSteps].Add(new KeyValuePair<bool, VectorInt3>(bb.Key.GetAlignment(), bb.Key.GetPosition()));
			}
		}
			m_potentialMerge.Clear();
    
		// Remove all tubes that are ready to
		System.Diagnostics.Debug.WriteLine("removing tubes (" + m_potentialRemove.Count + ")");
			foreach (var tube in m_potentialRemove){
			RemoveTube(tube);
		}
		m_potentialRemove.Clear();
//			ClearPotentialRemove(); //TODO3 
    
			// Move other pieces (non-boxes) in selection
		System.Diagnostics.Debug.WriteLine("handling " + other.Count + " tubes that didn't move yet");
			VectorInt3 move = Tools.GetVectorFromDirection(d);
			foreach (var p in other){
			MovePiece(p, move, d);
		}
    
			FixInjectors(d); 
    
			GenerateIds();
	}
	
	//TODO3 Test if this method is needed.
	protected void ClearPotentialRemove()
	{
		foreach (Piece p in this.m_potentialRemove){
			p.UninitVisual();
		}
			m_potentialRemove.Clear();
	}
	
	
	/**
	 * Flushes the list of fixed boxes for next move.
	 */
	public void FlushFixedBoxes()
	{
			m_fixedBoxes.Clear();
	}
	
	/**
	 * Add a fixed box for the next move.
	 *
	 * \warning Remember to call FlushFixedBoxes after the move.
	 */
	public void AddFixedBox(bool align, VectorInt3 position)
	{
			/// \todo use emplace
			if (!m_fixedBoxes.ContainsKey(0)){
			m_fixedBoxes[0] = new HashSet<KeyValuePair<bool, VectorInt3>>();
		}
			m_fixedBoxes[0].Add(new KeyValuePair<bool, VectorInt3>(align, position));
	}

	/**
	 * Set the current move step
	 *
	 * Should be called before MoveSelection when using AddFixedBox.
	 */
	public void SetMoveStep(int moveStep)
	{
		m_moveSteps = moveStep;
	}
	/// \}
	
	/// \name Crossing
	/// \{
	/**
	 * Start a crossing move
	 */
	public void StartCrossing()
	{
			m_crossing = true;
	}

	/**
	 * Return true if the circuit is in a stable state.
	 */
	public bool IsStable()
	{
		return m_unstable.Count == 0;
	}
	/// \}

	/// \name Duplication
	/// \{
	/**
	 * Start to duplicate on an axis
	 */
	public void StartDuplicate(Axis a)
	{
		// check if there s another action in progress
		if (m_action != Action.NONE)
		throw new QBException(QBException.Type.OTHER_ACTION_IN_PROGRESS);
    
		// Check if we have exactly 1 tube and 2 boxes selected
		if (m_selection.Count != 3)
		{
		System.Diagnostics.Debug.WriteLine("must select 3 pieces");
		throw new QBException(QBException.Type.DUP_INVALID_SELECTION);
		}
    
		Tube tube = null;
		int boxes = 0;
		int tubes = 0;
		foreach (Piece p in m_selection)
		switch (p.GetType())
		{
			case PieceType.BOX:
			++boxes;
			break;
			case PieceType.TUBE:
			tube = (Tube)p;
			System.Diagnostics.Debug.Assert(tube != null);
			++tubes;
			break;
			default:
			System.Diagnostics.Debug.WriteLine("unknown piece type in selection");
			throw new QBException(QBException.Type.DUP_INVALID_SELECTION);
		}
		if (boxes != 2 || tubes != 1)
		{
		System.Diagnostics.Debug.WriteLine("not 2 boxes and 1 tube");
		throw new QBException(QBException.Type.DUP_INVALID_SELECTION);
		}
    
		// Check if we are moving orthogonally to the tube orientation
		if (tube.GetAxis() == a)
		{
			System.Diagnostics.Debug.WriteLine("invalid move axis");
			throw new QBException(QBException.Type.DUP_INVALID_AXIS);
		}
		
		m_action = Action.MOVE_DUPLICATE;

		UnityEngine.Debug.Log("starting duplicate move");
	}

	/**
	 * Duplicate the piece in the direction if possible. 
	 */
	public void DuplicateSelection(Direction d, bool cutPath)
	{
//		UnityEngine.Debug.Log("duplicating");
    
			System.Diagnostics.Debug.Assert(m_selection.Count == 3);
    
		// copy selection to duplicate source
		m_duplicateSource.AddRange(m_selection);
    
		// get the tube and check if duplication is possible
		Tube tube = null;
		foreach (Piece p in m_duplicateSource)
		{
			if (p.GetType() != PieceType.BOX)
			{
				System.Diagnostics.Debug.Assert(p.GetType() == PieceType.TUBE);
				Tube t = (Tube)p;
				System.Diagnostics.Debug.Assert(t != null);
				tube = t;				
				m_duplicateSourceTube = tube;
			} else {
				if (p.GetNeighbor(d) == null){
					UnityEngine.Debug.Log("duplication impossible, " + "a box lack a neighbor in the move direction");
					throw new QBException(QBException.Type.DUP_IMPOSSIBLE);
					m_duplicateSource.Clear();
					m_duplicateSource = null;
				}
			}
		}
    
		System.Diagnostics.Debug.Assert(tube != null);
    
		// get the boxes in direction order (negative-positive)
		List<Box> boxes = new List<Box>();
		KeyValuePair<Direction, Direction> dirs = Tools.GetDirectionsFromAxis(tube.GetAxis());
		
		Box box = null;
		box = (Box)tube.GetNeighbor(dirs.Key);
		System.Diagnostics.Debug.Assert(box != null);
		boxes.Add(box);
		box = (Box)tube.GetNeighbor(dirs.Value);
		System.Diagnostics.Debug.Assert(box != null);
		boxes.Add(box);
		
		UnselectAll();
		
		System.Diagnostics.Debug.Assert(boxes.Count == 2);
    
			// select only the boxes
			foreach (var bx in boxes){
			SelectOnly(bx);
		}
		
		// find the length to the first neighbor
		int l1, l2;
		Piece n = boxes[0].GetNeighbor(d);
		if (n.GetType() == PieceType.TUBE){
			l1 = n.GetLength() + 1;
		} else {
			l1 = 1;
		}
		n = boxes[1].GetNeighbor(d);
		if (n.GetType() == PieceType.TUBE){
			l2 = n.GetLength() + 1;
		} else {
			l2 = 1;
		}
    
		// calculate the maximum move we can do with this duplicate
		// if this is >= 5, the duplicate is possible
		//TODO2 check: shouldnt this be Min? 
		m_maxDuplicateMove = System.Math.Max(l1, l2);
    
		// move the boxes, it will duplicate them
		MoveSelection(d);
		
		System.Diagnostics.Debug.Assert(m_selection.Count == 2);
    
			// make a new tube to connect the moved boxes
		var en = m_selection.GetEnumerator(); 
		en.MoveNext();
		Box box1 = (Box)en.Current; 
		en.MoveNext();
		Box box2 = (Box)en.Current;
		Tube newTube = LinkBoxes(tube.GetAxis(), box1, box2);
		System.Diagnostics.Debug.Assert(newTube != null);
		SelectOnly(newTube);
		
		tube.SetSqueezeDirection(Tools.GetPositiveDirection(d));
		if (!Tools.DirectionIsPositive(d))
		{
			var move = Tools.GetVectorFromDirection(d);
			tube.SetPositionOffset((-move).ToVector3());
		}
		
		newTube.SetPositionOffset((-Tools.GetVectorFromDirection(d)).ToVector3());
    
		// cut the longest path
		int cutInd = l2 > l1 ? 1 : 0;
		int uncutInd = l2 > l1 ? 0 : 1;
		m_duplicateCutBox = (Box)boxes[cutInd].GetNeighbor(d);
		System.Diagnostics.Debug.Assert(m_duplicateCutBox != null);
		Box duplicateUncutBox = (Box)boxes[uncutInd].GetNeighbor(d);
    
		// handle the tags
		newTube.ReplaceTags(m_duplicateCutBox.GetTags());
    
		foreach (var t in m_duplicateCutBox.GetTags())
		{
			bool p = tube.GetTags().Contains(t);
			if (!p){
				tube.GetTags().Add(t);
				boxes[0].GetTags().Add(t);
				boxes[1].GetTags().Add(t);
				duplicateUncutBox.GetTags().Add(t);
			}
			else
			{
				tube.GetTags().Remove(t);
				boxes[0].GetTags().Remove(t);
				boxes[1].GetTags().Remove(t);
			}
		}
    
		if (cutPath)
		{
		CutDuplicate(d);
		m_duplicateSource.Clear();
		m_duplicateSourceTube = null;
		m_duplicateCutBox = null;
		GenerateIds();
		}
	}
	
	/**
	 * Revert the duplication by merging back the pieces
	 * Merges 2 boxes and 1 tube to their original duplicate source. 
	 * Disposes of 1 tube that connected duplicate source and product
	 *
	 * The selection must contain the tube and the two boxes to merge.
	 *
	 * \note this function is meant for internal use.
	 */
	public void RevertDuplicate(Direction d)
	{
		UnityEngine.Debug.Log("reverting duplicate");
			System.Diagnostics.Debug.Assert(m_selection.Count == 3);
    
		// find the moving tube piece to be merged
		Tube tube = null;
		foreach (var p in m_selection){
			if (p.GetType() == PieceType.TUBE){
				tube = (Tube)p;
			}
		}
		System.Diagnostics.Debug.Assert(tube != null);
		
		var dirs = Tools.GetDirectionsFromAxis(tube.GetAxis());
		
		// Find the fixed tube to be merged into
		var fixedTube = tube.GetNeighbor(dirs.Key).GetNeighbor(d).GetNeighbor(dirs.Value);
		System.Diagnostics.Debug.Assert(fixedTube != null);
		System.Diagnostics.Debug.Assert(fixedTube.GetType() == PieceType.TUBE);
    
		// handle tags
		List<uint> toRemove = new List<uint>();
		foreach (uint t in tube.GetTags())
		{
			if (!fixedTube.GetTags().Contains(t)) {
				fixedTube.GetTags().Add(t);
			} else {
				toRemove.Add(t);
				fixedTube.GetTags().Remove(t);
			}
		}
     
		// remove the moving tube from topology
		tube.GetNeighbor(dirs.Key).SetNeighbor(dirs.Value, null);
		tube.GetNeighbor(dirs.Value).SetNeighbor(dirs.Key, null);
		UnselectOnly(tube);
		DeletePiece(tube);
		tube.UninitVisual();
		
		MoveSelection(d);
		SelectOnly(fixedTube);
    
		// small hack to remove tags on uncut box because we don't know which one it
		// is since this function can be called by undo::Merge which does not set
		// m_duplicateBoxCut
		Piece uncut = null;
		foreach (var t in toRemove)
		{
			if (uncut == null)
			{
				foreach (KeyValuePair<Direction, Piece> n in fixedTube)
				{
					bool rem = true;
					// if the box has tagged neighbors, the tag must not be removed
					foreach (KeyValuePair<Direction, Piece> n2 in n.Value){
						if (n2.Value.GetTags().Contains(t))
						{
							rem = false;
							break;
						}
					}
					if (rem)
					{
						uncut = n.Value;
						break;
					}
					}
				System.Diagnostics.Debug.Assert(uncut != null);
				uncut.GetTags().Remove(t);
			}
		}
    
		// create a gap filler tube from the deleted one
		var gf = new Tube();
		gf.SetPosition(fixedTube.GetAlignment(), fixedTube.GetPosition());
		gf.SetAxis(tube.GetAxis());
		gf.SetSqueezeDirection(Tools.GetNegativeDirection(d));
		gf.SetLength(tube.GetLength());
		gf.SetColor(tube.GetColor());
		gf.InitVisual();
		if (Tools.DirectionIsPositive(d)) {
			var move = Tools.GetVectorFromDirection(d);
			gf.Move(move);
			gf.SetPositionOffset((-move).ToVector3());
		}
		gf.ReplaceTags(tube.GetTags());
		m_gapFillers.AddLast(gf);
    
			fixedTube.SetPositionOffset((-Tools.GetVectorFromDirection(d)).ToVector3());
    
		GenerateIds();
	}
	/// \}

	/// \name Collision
	/// \{
	/**
	 * Check if there are collisions and set collision points for rendering
	 */
	public void CheckCollisions()
	{
//		if (UnityEngine.Debug.isDebugBuild){
//				FullCoherenceCheck();
//		}
    
		ResetCollisions();
		
		LinkedListNode<Piece> iter1, iter2;
		
		// check for collisions on the same alignment
		foreach (LinkedList<Piece> pieceList in m_pieces)
		{
			for (iter1 = pieceList.First; iter1 != null; iter1 = iter1.Next)
			{
				// skip the duplicate source, they must not collide during duplication
				if (m_duplicateSource.Contains(iter1.Value)){
					continue;
				}
		  
				for (iter2 = iter1.Next; iter2 != null; iter2 = iter2.Next)
				{
					if (m_duplicateSource.Contains(iter2.Value)){
							continue;
					}
								
					if (!iter1.Value.IsSelected() && !iter2.Value.IsSelected()){
						continue;
					}
		  
					bool c1 = iter1.Value.CheckCollision(iter2.Value, Piece.COLLDIST_ALIGN);
					if (c1){
	//					UnityEngine.Debug.Log("collision: " + iter1.Value.ToString() + " " + iter2.Value.ToString());
							m_isAlignColliding = true;
					}
					}
			}
		}
    
		// check for collisions on different alignments
		for (iter1 = m_pieces[0].First; iter1 != null; iter1 = iter1.Next){
			for (iter2 = m_pieces[1].First; iter2 != null; iter2 = iter2.Next)
			{
							
				if (!iter1.Value.IsSelected() && !iter2.Value.IsSelected()){
					continue;
				}
				bool c1 = iter1.Value.CheckCollision(iter2.Value, Piece.COLLDIST_MISALIGN);
				if (c1){
//					UnityEngine.Debug.Log("collision: " + iter1.Value.ToString() + " " + iter2.Value.ToString());
						m_isUnalignColliding = true;
				}
			}
		}
		

		
	}

	/**
	 * Return true if the circuit is in a colliding state
	 */
	public bool IsColliding()
	{
		return m_isAlignColliding || m_isUnalignColliding;
	}
	/// \}

	/// \name Injector ordering
	/// \{
	/**
	 * Check if injectors are ordered in time axis
	 */
	public void CheckInjection()
	{
			m_injectionBad = false;
    
		if (!m_timeLocked)
		return;
    
		int coord = Tools.GetIntFromAxis(Tools.GetAxisFromDirection(m_timeDir));
		bool sign = Tools.DirectionIsPositive(m_timeDir);
    
		// iterate blocks 
		foreach (var inj in m_injectors)
		{
		System.Diagnostics.Debug.Assert(inj.Value.Count != 0);
    
		// check the list of injectors to be ordered according to their coordinate on the time axis
		int pos = inj.Value[0].GetPosition()[coord];
    
		for (int i = 1; i < inj.Value.Count; ++i)
		{
			if (	 sign && inj.Value[i].GetPosition()[coord] <= pos || 
				!sign && inj.Value[i].GetPosition()[coord] >= pos)
			{
			m_injectionBad = true;
			return;
			}
			pos = inj.Value[i].GetPosition()[coord];
		}
		}
	}

	
	/**
	 * Return true if circuit has injectors ordered correctly
	 */
	public bool IsInjectionOk()
	{
		return !m_injectionBad;
	}
	
	/**
	 * Set the time axis for the circuit
	 */
	public void SetTimeDirection(Direction d)
	{
		m_timeDir = d;
		CheckInjection();
	}
	
	public void SetTimeLocked(bool locked)
	{
		this.m_timeLocked = locked;
	}
	/// \}

	/// \name Injector rotation
	/// \{
	
	/**
	 * Rotate an injector
	 *
	 * Call this function with 4 selected object and it will operate the
	 * rotation if possible.
	 */
	public void RotateInjector()
	{
		if (m_selection.Count != 4)
		{
			System.Diagnostics.Debug.WriteLine("exactly 4 pieces must be selected");
			throw new QBException(QBException.Type.ROTINJ_INVALID_SELECTION);
		}
    
		Piece injector = null;
		Box box1;
		Box box2;
		Box target = null;
    
		foreach (var p in m_selection)
		if (p.GetType() == PieceType.INJECTOR || p.GetType() == PieceType.CAP)
			injector = p;
    
		if (injector == null)
		{
			System.Diagnostics.Debug.WriteLine("no injector-like piece selected");
			throw new QBException(QBException.Type.ROTINJ_INVALID_SELECTION);
		}
    
		var dirs = Tools.GetDirectionsFromAxis(injector.GetAxis());
    
		box1 = (Box)(injector.GetNeighbor(dirs.Key));
		box2 = (Box)(injector.GetNeighbor(dirs.Value));
    
		System.Diagnostics.Debug.Assert(box1 != null);
		System.Diagnostics.Debug.Assert(box2 != null);
		System.Diagnostics.Debug.Assert(box1.IsSelected() && m_selection.Contains(box1));
		System.Diagnostics.Debug.Assert(box2.IsSelected() && m_selection.Contains(box2));
    
		foreach (var p in m_selection)
		if (p != injector && p != box1 && p != box2)
		{
			if (p.GetType() != PieceType.BOX)
			{
				System.Diagnostics.Debug.WriteLine("last piece is not a box");
				throw new QBException(QBException.Type.ROTINJ_INVALID_SELECTION);
			}
    
			target = (Box)(p);
			System.Diagnostics.Debug.Assert(target != null);
		}
    
			System.Diagnostics.Debug.Assert(target != null);
    
		Box coTarget;
		Axis targetAxis;
    
		if ((targetAxis = FindAlignment(box1, target)) != Axis.INVALID)
		coTarget = box1;
		else if ((targetAxis = FindAlignment(box2, target)) != Axis.INVALID)
		coTarget = box2;
		else
		{
			System.Diagnostics.Debug.WriteLine("last box not aligned with injector neighbors");
			throw new QBException(QBException.Type.ROTINJ_INVALID_TARGET);
		}
    
		if (targetAxis == injector.GetAxis())
		{
			System.Diagnostics.Debug.WriteLine("attempt to rotate injector more than 90 degrees");
			throw new QBException(QBException.Type.ROTINJ_INVALID_TARGET);
		}
    
		int dist = BoxDistance(targetAxis, coTarget, target);
		if (System.Math.Abs(dist) != 5)
		{
			System.Diagnostics.Debug.WriteLine("target too close or too far (" + dist + ")");
			throw new QBException(QBException.Type.ROTINJ_INVALID_TARGET);
		}
    
		List<Piece> walkedPieces = new List<Piece>();
    
		Box startBox = (coTarget == box1 ? box2 : box1);
		var targetDirs = Tools.GetDirectionsFromAxis(targetAxis);
		Piece curPiece = startBox;
		walkedPieces.Add(curPiece);
		if (dist > 0)
			curPiece = curPiece.GetNeighbor(targetDirs.Value);
		else
			curPiece = curPiece.GetNeighbor(targetDirs.Key);
		walkedPieces.Add(curPiece);
    
		int pos = 1;
		while (curPiece != null && pos < 5)
		{
		pos += curPiece.GetLength();
		// go to the next
		if (dist > 0)
			curPiece = curPiece.GetNeighbor(targetDirs.Value);
		else
			curPiece = curPiece.GetNeighbor(targetDirs.Key);
		walkedPieces.Add(curPiece);
		}
    
		if (curPiece == null || pos != 5)
		{
	System.Diagnostics.Debug.WriteLine("path too complex or incomplete");
		throw new QBException(QBException.Type.ROTINJ_INVALID_PATH);
		}
    
		// now follow the path parallel to the injector
		if (coTarget == box2)
		curPiece = curPiece.GetNeighbor(dirs.Value);
		else
		curPiece = curPiece.GetNeighbor(dirs.Key);
		walkedPieces.Add(curPiece);
    
		pos = 1;
		while (curPiece != null && pos < 5)
		{
		pos += curPiece.GetLength();
		// go to the next
		if (coTarget == box2)
			curPiece = curPiece.GetNeighbor(dirs.Value);
		else
			curPiece = curPiece.GetNeighbor(dirs.Key);
		walkedPieces.Add(curPiece);
		}
    
		if (curPiece == null || pos != 5)
		{
			System.Diagnostics.Debug.WriteLine("path too complex or incomplete");
			throw new QBException(QBException.Type.ROTINJ_INVALID_PATH);
		}
    
		walkedPieces.RemoveAt(walkedPieces.Count - 1);
    
		System.Diagnostics.Debug.Assert(curPiece == target);
    
		// check that there is nothing in the way

		Vector3 boxMin = Vector3.Min(target.GetPosition().ToVector3(), startBox.GetPosition().ToVector3());
		Vector3 boxMax = Vector3.Max(target.GetPosition().ToVector3(), startBox.GetPosition().ToVector3());
		++(boxMin[Tools.GetIntFromAxis(targetAxis)]);
		++(boxMin[Tools.GetIntFromAxis(injector.GetAxis())]);
		++(boxMax[Tools.GetIntFromAxis(Tools.GetComplementaryAxis(targetAxis, injector.GetAxis()))]);
    
		if (injector.GetAlignment())
		{
		boxMin += s_offsetSpaces;
		boxMax += s_offsetSpaces;
		}
    	
		KeyValuePair<Vector3, Vector3> collBox = new KeyValuePair<Vector3, Vector3>(boxMin, boxMax);
		
		System.Diagnostics.Debug.WriteLine(collBox.Value);
    
		/// \todo slow, use octree ?
		foreach (var pieceList in m_pieces){
			foreach (Piece p in pieceList){
				if (p.Intersect(collBox))
				{
					System.Diagnostics.Debug.WriteLine(p + " is in the way !");
					throw new QBException(QBException.Type.ROTINJ_COLLISION);
				}
			}
		}
    
		System.Diagnostics.Debug.WriteLine("everything ok, trying to rotate");
    
			VectorInt3 initialPosition = injector.GetPosition();
    
		if (startBox == box2)
		{
			startBox.SetNeighbor(dirs.Key, null);
			coTarget.SetNeighbor(dirs.Value, null);
		}
		else
		{
			startBox.SetNeighbor(dirs.Value, null);
			coTarget.SetNeighbor(dirs.Key, null);
		}
		
		injector.SetNeighbor(dirs.Key, null);
		injector.SetNeighbor(dirs.Value, null);
		injector.SetAxis(targetAxis);
		
		if (dist > 0)
		{
			injector.SetNeighbor(targetDirs.Key, coTarget);
			coTarget.SetNeighbor(targetDirs.Value, injector);
			injector.SetNeighbor(targetDirs.Value, target);
			target.SetNeighbor(targetDirs.Key, injector);
			injector.SetPosition(coTarget.GetPosition() + Tools.GetVectorFromDirection(targetDirs.Value));
		}
		else
		{
			injector.SetNeighbor(targetDirs.Value, coTarget);
			coTarget.SetNeighbor(targetDirs.Key, injector);
			injector.SetNeighbor(targetDirs.Key, target);
			target.SetNeighbor(targetDirs.Value, injector);
			injector.SetPosition(target.GetPosition() + Tools.GetVectorFromDirection(targetDirs.Value));
		}
    
		System.Diagnostics.Debug.WriteLine("finished rotating");
    
		/// \todo slow, check only with injector
		CheckCollisions();
		if (m_isAlignColliding || m_isUnalignColliding)
		{
			System.Diagnostics.Debug.WriteLine("collision detected, reverting rotation");
		  
			bool saveSteps = m_saveSteps;
			m_saveSteps = false;
		  
			// infinite recursivity should never happen since we were not in a
			// collision state before the rotation, unfortunately we have no good way
			// to assert that
			try
			{
					RotateInjector();
			}
			catch (QBException)
			{
					System.Diagnostics.Debug.Assert(false);
			}
		  
			m_saveSteps = saveSteps;
		  
			// collision state is reset by the call to RotateInjector
		  
			throw new QBException(QBException.Type.ROTINJ_COLLISION);
		}
    
		// handle tags
		System.Diagnostics.Debug.Assert(injector.GetTags().Count == 1);
    
		bool addThis;
		var it = injector.GetTags().GetEnumerator();
		it.MoveNext();
		var tag = it.Current;	
		if (walkedPieces[1].GetTags().Contains(tag))
		addThis = false;
		else
		addThis = true;
    
		foreach (Piece p in walkedPieces)
		if (addThis)
			p.GetTags().Add(tag);
		else
			p.GetTags().Remove(tag);
		if (addThis)
		target.GetTags().Add(tag);
    
		if (m_saveSteps)
		{
			m_steps.AddLast(new RotateInjector(injector.GetAlignment(), 
				coTarget.GetPosition(), startBox.GetPosition(), 
				target.GetPosition(), initialPosition, injector.GetPosition()));
		}
    
		CheckInjection();
		GenerateIds();
	}
	
	/**
	 * Return axis on which two pieces are aligned or 
	 * Axis::INVALID if the axis differs
	 */
	public Axis FindAlignment(Piece p1, Piece p2)
	{
		if (p1.GetAlignment() != p2.GetAlignment())
		return Axis.INVALID;
    
		Axis ret = Axis.INVALID;
		int c = 0;
		Axis[] coords = {Axis.X, Axis.Y, Axis.Z};
		foreach (var a in coords)
		{
		if (p1.GetPosition()[c] != p2.GetPosition()[c])
			if (ret != Axis.INVALID)
			// not aligned
			return Axis.INVALID;
			else
			ret = a;
		++c;
		}
		return ret;
	}
	/// \}

	/// \name Injector move
	/// \{
	/**
	 * Toggle injector moving
	 *
	 * This enable the showing of a ghost for the potential target of the
	 * injector. The ghost position is set thanks to PreviewInjectorAt
	 *
	 * \return true if injector move has been enabled, false otherwise.
	 */
	public void StartInjectorMove()
	{
		if (m_action == Action.MOVE_INJECTOR){
			return;
		}
		else if (m_action != Action.NONE){
			throw new QBException(QBException.Type.OTHER_ACTION_IN_PROGRESS);
		}
    
		System.Diagnostics.Debug.Assert(m_movingInjector == null);
		System.Diagnostics.Debug.Assert(m_ghostPiece == null);
		System.Diagnostics.Debug.Assert(m_ghostTarget == null);
    
			if (m_selection.Count != 3){
			throw new QBException(QBException.Type.MOVEINJ_INVALID_SELECTION);
		}
    
		// get the injector
		foreach (var p in m_selection){
		if (p.GetType() == PieceType.INJECTOR || p.GetType() == PieceType.CAP){
			if (m_movingInjector != null)
			{
			m_movingInjector = null;
			throw new QBException(QBException.Type.MOVEINJ_INVALID_SELECTION);
			}
			else
			{
			m_movingInjector = (Injector)p;
			System.Diagnostics.Debug.Assert(m_movingInjector != null);
			}
			}
		}
		if (m_movingInjector == null){
		throw new QBException(QBException.Type.MOVEINJ_INVALID_SELECTION);
		}
    
		m_action = Action.MOVE_INJECTOR;
	}
	
	/**
	 * Commit the injector move
	 *
	 * Save a step in the undo history.
	 */
	public void CommitInjectorMove()
	{
			System.Diagnostics.Debug.Assert(m_action == Action.MOVE_INJECTOR);
    
		// if we have no ghost, the move is invalid, just cancel it
		if (m_ghostPiece == null)
		{
			m_movingInjector = null;
			System.Diagnostics.Debug.Assert(m_ghostTarget == null);
			m_action = Action.NONE;
			return;
		}
    
		System.Diagnostics.Debug.Assert(m_movingInjector != null);
		System.Diagnostics.Debug.Assert(m_ghostTarget != null);
		System.Diagnostics.Debug.Assert(m_ghostTarget.GetType() == PieceType.TUBE);
		System.Diagnostics.Debug.Assert(m_movingInjector.GetPathId() == m_ghostTarget.GetPathId());
    
		Step stepMoveInj = MoveInjector(m_movingInjector, (Tube)m_ghostTarget, m_ghostPiece.GetPosition());
			m_steps.AddLast(stepMoveInj);
    
			m_movingInjector = null;
		if (m_ghostPiece != null){
			m_ghostPiece.UninitVisual();
			m_ghostPiece = null;
		}
		m_ghostTarget = null;
		m_action = Action.NONE;
	}
	
	/**
	 * Put the ghost on the first piece that intersects ray if on the same path
	 */
	public void PreviewInjectorAt(VectorInt3 posPiece, bool alignment, Vector3 posHit)
	{
		System.Diagnostics.Debug.Assert(m_action == Action.MOVE_INJECTOR);
		
		// Find piece hit by ray
		
		// Perform raycast
//		Piece p = null;
//		RaycastHit hit = new RaycastHit();
//		if (Physics.Raycast(ray, out hit, 100000.0f, 1 << LayerMask.NameToLayer("blockPicking"))){
//			if (hit.transform.gameObject != null){
//				PieceCS pv = hit.collider.transform.parent.GetComponent<PieceCS>();
//				if (pv != null) {
//					p = pv.piece;
//				}
//			}
//		}
		Piece p = GetPiece(alignment, posPiece);
		
		// Check if piece hit is eligible to receive the injector
		
	 	// if no piece or not a tube or not same path id or too short, don't show ghost
		if (p == null || // no piece hit
			p.GetType() != PieceType.TUBE || // hit piece is not a tube
			p.GetPathId() != m_movingInjector.GetPathId() || // hit piece is not on the same block like the injector
			p.GetLength() < 4) // tube is not long enough
		{
			// hide ghost piece
			if (m_ghostPiece != null){
				m_ghostPiece.UninitVisual();
				m_ghostPiece = null;
			}
			// abort
			m_ghostTarget = null;
			return;
		} else {
			// we have our target
			m_ghostTarget = p;
		}
		
		// determine the exact location of the destination tube
//		Vector3 rpos = hit.point;
		Vector3 rpos = posHit;
		if (p.GetAlignment()){
			rpos -= s_offsetSpaces;
		}
		VectorInt3 pos = new VectorInt3((int)System.Math.Round(rpos.x), (int)System.Math.Round(rpos.y), (int)System.Math.Round(rpos.z));
    
		// determine the exact position of the ghost piece
		switch (p.GetAxis())
		{
		case Axis.X:
			pos.x = System.Math.Max(p.GetPosition().x, (short)(pos.x - 2));
			{
			int margin = p.GetPosition().x + p.GetLength() - pos.x;
			if (margin < 4)
				pos.x -= (short)(4 - margin);
			}
			pos.y = p.GetPosition().y;
			pos.z = p.GetPosition().z;
			break;
		case Axis.Y:
			pos.x = p.GetPosition().x;
			pos.y = System.Math.Max(p.GetPosition().y, (short)(pos.y - 2));
			{
			int margin = p.GetPosition().y + p.GetLength() - pos.y;
			if (margin < 4)
				pos.y -= (short)(4 - margin);
			}
			pos.z = p.GetPosition().z;
			break;
		case Axis.Z:
			pos.x = p.GetPosition().x;
			pos.y = p.GetPosition().y;
			pos.z = System.Math.Max(p.GetPosition().z, (short)(pos.z - 2));
			{
			int margin = p.GetPosition().z + p.GetLength() - pos.z;
			if (margin < 4)
				pos.z -= (short)(4 - margin);
			}
			break;
		default:
			System.Diagnostics.Debug.Assert(false);
			break;
		}
    
		// create ghost piece if there is none
		if (m_movingInjector.GetType() == PieceType.INJECTOR){
			if (m_ghostPiece == null){
				m_ghostPiece = new InjectorGhost();
				m_ghostPiece.InitVisual();
			} else{
				System.Diagnostics.Debug.Assert(m_ghostPiece.GetType() == PieceType.INJECTOR);
			}
		} else if (m_movingInjector.GetType() == PieceType.CAP){
			if (m_ghostPiece == null){
				m_ghostPiece = new InjectorGhost();
				m_ghostPiece.InitVisual();
			} else {
				System.Diagnostics.Debug.Assert(m_ghostPiece.GetType() == PieceType.CAP);
			}
		} else{
			System.Diagnostics.Debug.Assert(false);
		}
    
		// put it at the right position
		m_ghostPiece.SetAxis(p.GetAxis());
		m_ghostPiece.SetPosition(p.GetAlignment(), pos);
		
		m_ghostPiece.SetColor(p.GetColor());
		m_ghostPiece.UpdateVisual(0f);
	}
	
	/**
	 * Return true if an injector move is in progress.
	 */
	public bool IsMovingInjector()
	{
		return m_action == Action.MOVE_INJECTOR;
	}
	
	/**
	 * Moves an injector to another position on a path
	 * returns a MultiStep that represents the respective operations
	 */
	public Step MoveInjector(Injector injector, Tube targetTube, VectorInt3 position)
	{
		System.Diagnostics.Debug.Assert(injector != null);
		System.Diagnostics.Debug.Assert(targetTube != null);
		System.Diagnostics.Debug.Assert(targetTube.GetLength() >= 4);
    
		MultiStep multiStep = new MultiStep();
		VectorInt3 initialPosition = injector.GetPosition();
		bool initialAlignment = injector.GetAlignment();
    
		// replacement tube
		Tube repTube = new Tube();
		repTube.SetAxis(injector.GetAxis());
		repTube.SetPosition(injector.GetAlignment(), injector.GetPosition());
		repTube.SetLength(4);
		repTube.SetColor(injector.GetColor());
		repTube.InitVisual();
		repTube.ReplaceTags(injector.GetTags());
    
		int nAlignTube = repTube.GetAlignment() ? 1 : 0;
			m_pieces[nAlignTube].AddLast(repTube);
    
		TransferNeighbors(repTube, injector);
    
		DeletePiece(injector);
    
		// we put the injector in the right position
		
		injector.SetAxis(targetTube.GetAxis());
		injector.SetPosition(targetTube.GetAlignment(), position);
		injector.SetColor(targetTube.GetColor());
		injector.UpdateVisual(0f);
    
		
		int nAlignInj = injector.GetAlignment() ? 1 : 0;
		m_pieces[nAlignInj].AddLast(injector);
    
		// now we split the destination tube if necessary
		var dirs = Tools.GetDirectionsFromAxis(injector.GetAxis());
		if (injector.GetPosition() != targetTube.GetPosition())
		{
		// put a box on the negative side
		Box box = new Box();
		box.SetPosition(injector.GetAlignment(), injector.GetPosition() + Tools.GetVectorFromDirection(dirs.Key));
		injector.SetNeighbor(dirs.Key, box);
		box.SetNeighbor(dirs.Value, injector);
		box.SetColor(injector.GetColor());
			box.InitVisual();
    
		int nAlignBox = box.GetAlignment() ? 1 : 0;
		m_pieces[nAlignBox].AddLast(box);
    
		Box negBox = (Box)targetTube.GetNeighbor(dirs.Key);
		System.Diagnostics.Debug.Assert(negBox != null);
    
		Tube tube = LinkBoxes(injector.GetAxis(), negBox, box);
		box.ReplaceTags(targetTube.GetTags());
		if (tube != null)
			tube.ReplaceTags(targetTube.GetTags());
    
			multiStep.PushStep(new Split(box.GetAlignment(), negBox.GetPosition() + Tools.GetVectorFromDirection(dirs.Value), box.GetPosition()));
		}
		else
		{
		targetTube.GetNeighbor(dirs.Key).SetNeighbor(dirs.Value, injector);
		injector.SetNeighbor(dirs.Key, targetTube.GetNeighbor(dirs.Key));
		}
		// same for positive side
		var rVec = Tools.GetVectorFromDirection(dirs.Value);
		if (injector.GetPosition() + rVec * 4 != targetTube.GetPosition() + rVec * targetTube.GetLength())
		{
		// put a box on the positive side
		var box = new Box();
		box.SetPosition(injector.GetAlignment(), injector.GetPosition() + 4 * rVec);
		injector.SetNeighbor(dirs.Value, box);
		box.SetNeighbor(dirs.Key, injector);
		box.SetColor(injector.GetColor());
		box.InitVisual();
    
			int nAlignBox = box.GetAlignment() ? 1 : 0;
		m_pieces[nAlignBox].AddLast(box);
    
		Box posBox = (Box)targetTube.GetNeighbor(dirs.Value);
		System.Diagnostics.Debug.Assert(posBox != null);
    
		var tube = LinkBoxes(injector.GetAxis(), box, posBox);
		box.ReplaceTags(targetTube.GetTags());
		if (tube != null)
			tube.ReplaceTags(targetTube.GetTags());
    
		multiStep.PushStep(new Split(box.GetAlignment(), injector.GetPosition(), box.GetPosition()));
		}
		else
		{
		targetTube.GetNeighbor(dirs.Value).SetNeighbor(dirs.Key, injector);
		injector.SetNeighbor(dirs.Value, targetTube.GetNeighbor(dirs.Value));
		}
    
		injector.ReplaceTags(targetTube.GetTags());
    
		// delete the target tube, has been replaced
		DeletePiece(targetTube);
		targetTube.UninitVisual();
    
		UnselectAll();
    
		CheckInjection();
		GenerateIds();
    
		multiStep.PushStep(new InjectorMove(initialAlignment, initialPosition, injector.GetAlignment(), injector.GetPosition()));
    
		return multiStep;
	}
	/// \}

	/// \name Tube split
	/// \{
	/**
	 * Toggle tube splitting
	 *
	 * This enable the showing of a ghost for the potential point of split. The
	 * ghost position is set thanks to PreviewBoxAt
	 *
	 * \return true if tube splitting has been enabled, false otherwise.
	 */
	public void StartTubeSplit()
	{
			if (m_action == Action.SPLIT){
			return;
		} else if (m_action != Action.NONE){
			throw new QBException(QBException.Type.OTHER_ACTION_IN_PROGRESS);
		}
    
		m_action = Action.SPLIT;
		
		System.Diagnostics.Debug.Assert(m_ghostPiece == null);
		System.Diagnostics.Debug.Assert(m_ghostTarget == null);
		
		UnselectAll();
	}
	
	/**
	 * Commit the split
	 *
	 * Save a step in undo history
	 */
	public void CommitTubeSplit()
	{
//		System.Diagnostics.Debug.Assert(m_action == Action.SPLIT);
    
		// if no piece, just cancel the move
		// if there is a ghost piece then perform split
		if (m_ghostPiece != null)
		{
			System.Diagnostics.Debug.Assert(m_ghostTarget != null);
	    
			SplitTube((Tube)m_ghostTarget, m_ghostPiece.GetPosition());
	    
			m_steps.AddLast(new Split(m_ghostTarget.GetAlignment(), m_ghostTarget.GetPosition(), m_ghostPiece.GetPosition()));
				
			
			m_ghostPiece.UninitVisual();
			m_ghostPiece = null;
		}
    
		m_ghostTarget = null;
		m_action = Action.NONE;
	}
	
	/**
	 * Put the ghost on the first tube that intersects ray
	 */
	public void PreviewBoxAt(Ray ray)
	{
//			System.Diagnostics.Debug.Assert(m_action == Action.SPLIT);
    
		Piece pieceHit = null;
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast(ray, out hit, 100000.0f, 1 << LayerMask.NameToLayer("blockPicking"))){
			if (hit.transform.gameObject != null){
				PieceCS pv = hit.collider.transform.parent.GetComponent<PieceCS>();
				if (pv != null) {
					pieceHit = pv.piece;
				}
			}
		}
		
    
		// if cursor is not on a tube, hide ghost
		if (pieceHit == null || pieceHit.GetType() != PieceType.TUBE)
		{
			if (m_ghostPiece != null){
				m_ghostPiece.UninitVisual();
				m_ghostPiece = null;
			}
		m_ghostTarget = null;
		return;
		}
    
		m_ghostTarget = pieceHit;
    
		// calculate ghost position
		Vector3 rpos = hit.point;
    
		rpos -= new Vector3(0.5f, 0.5f, 0.5f);
		if (pieceHit.GetAlignment()){
			rpos -= s_offsetSpaces;
		}
    
		VectorInt3 pos = new VectorInt3((int)System.Math.Round(rpos.x), (int)System.Math.Round(rpos.y), (int)System.Math.Round(rpos.z));
    
		// two of the 3 coords are determined by the tube hit
		switch (pieceHit.GetAxis())
		{
		case Axis.X:
			pos.y = pieceHit.GetPosition().y;
			pos.z = pieceHit.GetPosition().z;
			break;
		case Axis.Y:
			pos.x = pieceHit.GetPosition().x;
			pos.z = pieceHit.GetPosition().z;
			break;
		case Axis.Z:
			pos.x = pieceHit.GetPosition().x;
			pos.y = pieceHit.GetPosition().y;
			break;
		default:
			System.Diagnostics.Debug.Assert(false);
			break;
		}
		
		// create ghost if there is none
		if (m_ghostPiece == null){
			m_ghostPiece = new BoxGhost();
			m_ghostPiece.InitVisual();
		} else {
			System.Diagnostics.Debug.Assert(m_ghostPiece.GetType() == PieceType.BOX);
		}
		m_ghostPiece.SetColor(pieceHit.GetColor());
		
			m_ghostPiece.SetPosition(pieceHit.GetAlignment(), pos);
		m_ghostPiece.UpdateVisual(0f);
	}
//	
//	/**
//	 * Return true if a splitting is in progress
//	 */
//
//;
//	public bool IsSplitting()
//	{
//		return m_action == Action.SPLIT;
//	}
	
	/**
	 * Split a tube at a given position
	 *
	 * \warning No check is done to be sure that position is on the tube.
	 */
	public void SplitTube(Tube tube, VectorInt3 position)
	{
		System.Diagnostics.Debug.Assert(tube != null);
    
		KeyValuePair<Direction, Direction> dirs = Tools.GetDirectionsFromAxis(tube.GetAxis());
    
		// create a box
		Box box = new Box();
		box.SetPosition(tube.GetAlignment(), position);
		box.SetColor(tube.GetColor());
		box.InitVisual();
    
		int nAlign = box.GetAlignment() ? 1 : 0;
		m_pieces[nAlign].AddLast(box);
    
		// the box on the negative side of the tube
		Box negBox = (Box)tube.GetNeighbor(dirs.Key);
		System.Diagnostics.Debug.Assert(negBox != null);
    
		// the box on the positive side of the tube
		Box posBox = (Box)tube.GetNeighbor(dirs.Value);
		System.Diagnostics.Debug.Assert(posBox != null);
    
		int len = BoxDistance(tube.GetAxis(), negBox, box) - 1;
    
		System.Diagnostics.Debug.Assert(len < tube.GetLength());
    
		// reduce the size of the tube and make it neighbor with the new box
		if (len >= 1)
		{
		box.SetNeighbor(dirs.Key, tube);
		tube.SetNeighbor(dirs.Value, box);
		tube.SetLength(len);
		}
		// if len == 0, remove the tube and make the boxes neighbors
		else
		{
		box.SetNeighbor(dirs.Key, negBox);
		negBox.SetNeighbor(dirs.Value, box);
		DeletePiece(tube);
			tube.UninitVisual();
		}
    
		// link boxes with a tube if necessary on the positive side
		var newTube = LinkBoxes(tube.GetAxis(), box, posBox);
    
		// set tags
		box.ReplaceTags(tube.GetTags());
		if (newTube != null){
			newTube.ReplaceTags(tube.GetTags());
		}
    
		GenerateIds();
	}
	/// \}

	/// \name Simplification
	/// \{
	/**
	 * Remove the cubes that can be removed in selection
	 */
	public void Simplify(bool addToHistory = true)
	{
		var ms = new MultiStep();
		
		foreach (Piece p in m_selection){
			if (p.GetType() == PieceType.BOX)
			{
				Piece tube = SimplifyBox((Box)p, false);
				if (tube != null){
					ms.PushStep(new Join(tube.GetAlignment(), tube.GetPosition(), p.GetPosition()));
				}
			}
		}
		
		GenerateIds();
    
		UnselectAll();
		if (addToHistory){
			if (!ms.IsEmpty()) {
				m_steps.AddLast(ms);
			}
		}
	}
	
	/**
	 * Remove box if possible and return the tube that has taken its place
	 */
	public Tube SimplifyBox(Box p, bool generateIds)
	{
		System.Diagnostics.Debug.Assert(p != null);
    
		Tube ret = null;
    
		// A List of pieces containing all neighbors of p
		List<KeyValuePair<Direction, Piece>> d = new List<KeyValuePair<Direction, Piece>>();
		foreach (KeyValuePair<Direction, Piece> n in p){
			d.Add(n);
		}
    
		// the only case where a box can be simplified is if it has neighbors on
		// two opposite sides only
		if (d.Count != 2 || d[0].Key != Tools.InvertDirection(d[1].Key)){
			return null;
		}
    
		// make sure the negative direction comes first
		if (Tools.DirectionIsPositive(d[0].Key)){
//			std.swap(d[0], d[1]);
			var temp = d[0];
			d[0] = d[1];
			d[1] = temp;
		}
    
		
		// we can't simplify a box with injector neighbors, that's why there is no
		// such case
		
		// Handle 4 cases: 
		//  Box-Box-Box
		// Tube-Box-Tube
		// Tube-Box-Box
		//  Box-Box-Tube
		if (d[0].Value.GetType() == PieceType.BOX && d[1].Value.GetType() == PieceType.BOX)
		{
			// Both neighbors are boxes. Just replace the box with a tube
			ret = SpawnTube<Tube>(Tools.GetAxisFromDirection(d[0].Key), 
					(Box)d[0].Value, 
					(Box)d[1].Value, 
					Direction.INVALID);
			System.Diagnostics.Debug.Assert(ret != null);
	    
			DeletePiece(p);
			p.UninitVisual();
		}
		else if (d[0].Value.GetType() == PieceType.TUBE && d[1].Value.GetType() == PieceType.TUBE)
		{
			// both neighbors are tubes: link the negative neighbor tube with the box beyond the positive neighbor tube
			d[0].Value.SetNeighbor(d[1].Key, d[1].Value.GetNeighbor(d[1].Key));
			d[1].Value.GetNeighbor(d[1].Key).SetNeighbor(d[0].Key, d[0].Value);
	    
			d[0].Value.SetLength(d[0].Value.GetLength() + 1 + d[1].Value.GetLength());
	    
			DeletePiece(p);
			DeletePiece(d[1].Value);
				
			p.UninitVisual();
			d[1].Value.UninitVisual();
	    
			ret = (Tube)d[0].Value;
			System.Diagnostics.Debug.Assert(ret != null);
		}
		else if (d[0].Value.GetType() == PieceType.TUBE && d[1].Value.GetType() == PieceType.BOX)
		{
			// 
			d[0].Value.SetNeighbor(d[1].Key, d[1].Value);
			d[1].Value.SetNeighbor(d[0].Key, d[0].Value);
	    
			d[0].Value.SetLength(d[0].Value.GetLength() + 1);
	    
			DeletePiece(p);
				
			p.UninitVisual();
	    
			ret = (Tube)d[0].Value;
			System.Diagnostics.Debug.Assert(ret != null);
		}
		else if (d[0].Value.GetType() == PieceType.BOX && d[1].Value.GetType() == PieceType.TUBE)
		{
			d[0].Value.SetNeighbor(d[1].Key, d[1].Value);
			d[1].Value.SetNeighbor(d[0].Key, d[0].Value);
	    
			d[1].Value.SetLength(d[1].Value.GetLength() + 1);
			d[1].Value.Move(Tools.GetVectorFromDirection(d[0].Key));
	    
			DeletePiece(p);
				
			p.UninitVisual();
	    
			ret = (Tube)d[1].Value;
			System.Diagnostics.Debug.Assert(ret != null);
		}
    
		/// \todo may be slow
		if (generateIds){
			GenerateIds();
		}
    
		return ret;
	}
	
	/// \}

	/// \name Loop bridging
	/// \{
	/**
	 * Bridge two boxes that are aligned
	 */
	public void Bridge()
	{
		if (m_selection.Count != 2)
		{
			System.Diagnostics.Debug.WriteLine("only 2 pieces should be selected");
			throw new QBException(QBException.Type.BRG_INVALID_SELECTION);
		}
    	var listSelection = new List<Piece>(m_selection);
		var p1 = listSelection[0];
		var p2 = listSelection[1];
    
		if (p1.GetType() != PieceType.BOX || p2.GetType() != PieceType.BOX)
		{
			System.Diagnostics.Debug.WriteLine("at least one piece is not a box");
			throw new QBException(QBException.Type.BRG_INVALID_SELECTION);
		}
    
		var b1 = (Box)(p1);
		var b2 = (Box)(p2);
    
		System.Diagnostics.Debug.Assert(b1 != null);
		System.Diagnostics.Debug.Assert(b2 != null);
    
		var color2 = Bridge(b1, b2);
    
		m_steps.AddLast(new Bridge(b1.GetAlignment(), b1.GetPosition(), b2.GetPosition(), color2));
	}
	
	/**
	 * Bridge two boxes that are aligned
	 *
	 * \warning This function makes little check to see if this is a valid
	 * move, it should be used only on UNDO.
	 */
	public Vector4 Bridge(Box b1, Box b2)
	{
		System.Diagnostics.Debug.Assert(b1 != null);
		System.Diagnostics.Debug.Assert(b2 != null);
    
		if (b1.GetAlignment() != b2.GetAlignment())
		{
		System.Diagnostics.Debug.WriteLine("boxes don't have the same alignment");
		throw new QBException(QBException.Type.BRG_NOT_ALIGNED);
		}
    
		if (!IsOnLoop(b1) || !IsOnLoop(b2))
		{
		System.Diagnostics.Debug.WriteLine("at least a box is not linked to a loop");
		throw new QBException(QBException.Type.BRG_NOT_A_LOOP);
		}
    
		if (b1.GetBlockId() == b2.GetBlockId())
		{
		System.Diagnostics.Debug.WriteLine("boxes are linked");
		throw new QBException(QBException.Type.BRG_SAME_PATH);
		}
    
		Axis a = Axis.INVALID;
		if (b1.GetPosition().x != b2.GetPosition().x)
		{
			a = Axis.X;
			if (b1.GetPosition().x > b2.GetPosition().x){
				Box temp = b1; 
				b1 = b2;
				b2 = temp;
			}
		}
		if (b1.GetPosition().y != b2.GetPosition().y)
		{
		if (a != Axis.INVALID)
		{
			System.Diagnostics.Debug.WriteLine("boxes are not on an axis");
				throw new QBException(QBException.Type.BRG_NOT_ALIGNED);
		}
		else {
					a = Axis.Y;
		}
		if (b1.GetPosition().y > b2.GetPosition().y){
				Box temp = b1; 
				b1 = b2;
				b2 = temp;
//					std.swap(b1, b2);
		}
		}
		if (b1.GetPosition().z != b2.GetPosition().z)
		{
		if (a != Axis.INVALID)
		{
		System.Diagnostics.Debug.WriteLine("boxes are not on an axis");
			throw new QBException(QBException.Type.BRG_NOT_ALIGNED);
		}
		else {
				a = Axis.Z;
		}
			if (b1.GetPosition().z > b2.GetPosition().z){
				Box temp = b1; 
				b1 = b2;
				b2 = temp;
//				std.swap(b1, b2);
			}
		}
    
		System.Diagnostics.Debug.WriteLine("everything's ok, trying to bridge");
		var tube = LinkBoxes(a, b1, b2);
		CheckCollisions();
		if (m_isAlignColliding || m_isUnalignColliding)
		{
			System.Diagnostics.Debug.WriteLine("collision detected, reverting bridge");
    
		var dirs = Tools.GetDirectionsFromAxis(a);
    
		b1.SetNeighbor(dirs.Value, null);
		b2.SetNeighbor(dirs.Key, null);
    
		// tube is not null here or it couldn't have triggered a collision
		System.Diagnostics.Debug.Assert(tube != null);
		DeletePiece(tube);
			//TODO2 check if to call UninitVisual() here
			tube.UninitVisual();
    
			if (UnityEngine.Debug.isDebugBuild){
				// we should not be colliding anymore
				CheckCollisions();
				System.Diagnostics.Debug.Assert(!m_isAlignColliding && !m_isUnalignColliding);
			}
		ResetCollisions();
    
		throw new QBException(QBException.Type.BRG_COLLISION);
		}
    
		var color2 = b2.GetColor();
    
		FillWithColor(b1);
		GenerateIds();
    
		return color2;
	}
	
	/**
	 * Revert a bridge
	 *
	 * \warning This function makes little check to see if this is a valid
	 * move, it should be used only on UNDO.
	 */
	public bool UnbridgeBoxes(Vector4 color2)
	{
		if (m_selection.Count != 2)
		{
			System.Diagnostics.Debug.WriteLine("only 2 pieces should be selected");
			return false;
		}
		var listSelection = new List<Piece>(m_selection);
		var p1 = listSelection[0];
		var p2 = listSelection[1];
    
		if (p1.GetType() != PieceType.BOX || p2.GetType() != PieceType.BOX)
		{
			System.Diagnostics.Debug.WriteLine("at least one piece is not a box");
			return false;
		}
    
		if (p1.GetAlignment() != p2.GetAlignment())
		{
			System.Diagnostics.Debug.WriteLine("boxes don't have the same alignment");
			return false;
		}
    
		var b1 = (Box)(p1);
		var b2 = (Box)(p2);
    
		System.Diagnostics.Debug.Assert(b1 != null);
		System.Diagnostics.Debug.Assert(b2 != null);
    
		Axis a = Axis.INVALID;
		if (b1.GetPosition().x != b2.GetPosition().x)
		{
		a = Axis.X;
		if (b1.GetPosition().x > b2.GetPosition().x){
				Box temp = b1; 
				b1 = b2;
				b2 = temp;
//				std.swap(b1, b2);
			}
		}
		if (b1.GetPosition().y != b2.GetPosition().y)
		{
		if (a != Axis.INVALID)
		{
			System.Diagnostics.Debug.WriteLine("boxes are not on an axis");
				return false;
		}
		else {
				a = Axis.Y;
		}
		if (b1.GetPosition().y > b2.GetPosition().y){
				Box temp = b1; 
				b1 = b2;
				b2 = temp;
//				std.swap(b1, b2);
			}
		}
		if (b1.GetPosition().z != b2.GetPosition().z)
		{
		if (a != Axis.INVALID)
		{
			System.Diagnostics.Debug.WriteLine("boxes are not on an axis");
				return false;
		}
		else{
				a = Axis.Z;
			}
		if (b1.GetPosition().z > b2.GetPosition().z){
				Box temp = b1; 
				b1 = b2;
				b2 = temp;
//				std.swap(b1, b2);
			}
			}
    
		System.Diagnostics.Debug.WriteLine("everything's ok, unbridging");
    
		var dirs = Tools.GetDirectionsFromAxis(a);
		
		var tube = b1.GetNeighbor(Tools.GetDirectionsFromAxis(a).Value);
		System.Diagnostics.Debug.Assert(tube != null);
		System.Diagnostics.Debug.Assert(tube.GetType() == PieceType.TUBE);
		System.Diagnostics.Debug.Assert(tube == b2.GetNeighbor(dirs.Key));
		
		b1.SetNeighbor(dirs.Value, null);
		b2.SetNeighbor(dirs.Key, null);
		
		DeletePiece(tube);
			//TODO2 check if to call UninitVisual() here
		tube.UninitVisual();
		
		b2.SetColor(color2);
		
		FillWithColor(b1);
		FillWithColor(b2);
    
		GenerateIds();
    
		System.Diagnostics.Debug.WriteLine("making checks");
    
		System.Diagnostics.Debug.Assert(IsOnLoop(b1) && IsOnLoop(b2));
		System.Diagnostics.Debug.Assert(b1.GetBlockId() != b2.GetBlockId());
		
		return true;
	}
	
	/// \}

	/// \name Injector teleportation
	/// \{
	/**
	 * 	Replace a circular path with an injector by an 
	 * 	injector on a defect passing through that circle
	 * */
	public void Teleport()
	{
		if (m_action != Action.NONE){
			throw new QBException(QBException.Type.OTHER_ACTION_IN_PROGRESS);
		}
    
		if (m_selection.Count != 3){
			throw new QBException(QBException.Type.TPINJ_INVALID_SELECTION);
		}
    
		// Find the injector
		Injector piece = null;
		foreach (var p in m_selection){
			if (p.GetType() == PieceType.INJECTOR || p.GetType() == PieceType.CAP)
			{
				// if there are two injectors or caps selected, we should have thrown before
				System.Diagnostics.Debug.Assert(piece == null);
		  
				piece = (Injector)(p);
				System.Diagnostics.Debug.Assert(piece != null);
			}
		}
    
		if (piece == null) {
			throw new QBException(QBException.Type.TPINJ_INVALID_SELECTION);
		}
    
		// now, we check if from this injector we can have a simple closed loop
		// (composed only of 4 boxes, 3 tubes and one injector)
		// and btw, we remember the lowest and the highest box for later
		Piece lowBox = null;
		Piece highBox = null;
    
		var dirs = Tools.GetDirectionsFromAxis(piece.GetAxis());
    
		var curPiece = piece.GetNeighbor(dirs.Value);
    
		System.Diagnostics.Debug.Assert(curPiece != null);
		System.Diagnostics.Debug.Assert(curPiece.GetType() == PieceType.BOX);
    
		if (curPiece.CountNeighbors() != 2){
			throw new QBException(QBException.Type.TPINJ_NOT_SIMPLE_LOOP);
		}
    
		Direction dir = Direction.INVALID;
		foreach (KeyValuePair<Direction, Piece> n in curPiece){
			if (n.Key != dirs.Key){
				dir = n.Key;
		  
				// just before going to the next piece, if dir is negative, this means
				// that this was the high box
				if (!Tools.DirectionIsPositive(dir))
				highBox = curPiece;
		  
				curPiece = n.Value;
			}
		}
    
		System.Diagnostics.Debug.Assert(dir != Direction.INVALID);
    
			if (Tools.GetAxisFromDirection(dir) == piece.GetAxis()){
			throw new QBException(QBException.Type.TPINJ_NOT_SIMPLE_LOOP);
		}
    
		if (curPiece.GetType() != PieceType.TUBE){
			throw new QBException(QBException.Type.TPINJ_NOT_SIMPLE_LOOP);
		}
    
		curPiece = curPiece.GetNeighbor(dir);
		
		System.Diagnostics.Debug.Assert(curPiece != null);
		System.Diagnostics.Debug.Assert(curPiece.GetType() == PieceType.BOX);
    
		if (curPiece.CountNeighbors() != 2)
		throw new QBException(QBException.Type.TPINJ_NOT_SIMPLE_LOOP);
    
		// if the previous one was not the highest box, this one is
		if (Tools.DirectionIsPositive(dir))
		highBox = curPiece;
    
		curPiece = curPiece.GetNeighbor(dirs.Key);
    
		if (curPiece == null || curPiece.GetType() != PieceType.TUBE)
		throw new QBException(QBException.Type.TPINJ_NOT_SIMPLE_LOOP);
    
		curPiece = curPiece.GetNeighbor(dirs.Key);
    
		System.Diagnostics.Debug.Assert(curPiece != null);
		System.Diagnostics.Debug.Assert(curPiece.GetType() == PieceType.BOX);
    
		if (curPiece.CountNeighbors() != 2)
		throw new QBException(QBException.Type.TPINJ_NOT_SIMPLE_LOOP);
    
		// same logic as above
		if (!Tools.DirectionIsPositive(dir))
		lowBox = curPiece;
    
		var dir2 = Tools.InvertDirection(dir);
		curPiece = curPiece.GetNeighbor(dir2);
    
		if (curPiece == null || curPiece.GetType() != PieceType.TUBE)
		throw new QBException(QBException.Type.TPINJ_NOT_SIMPLE_LOOP);
    
		curPiece = curPiece.GetNeighbor(dir2);
    
		System.Diagnostics.Debug.Assert(curPiece != null);
		System.Diagnostics.Debug.Assert(curPiece.GetType() == PieceType.BOX);
    
		if (curPiece.CountNeighbors() != 2)
		throw new QBException(QBException.Type.TPINJ_NOT_SIMPLE_LOOP);
    
		// same logic as above
		if (Tools.DirectionIsPositive(dir))
		lowBox = curPiece;
    
		curPiece = curPiece.GetNeighbor(dirs.Value);
    
		if (curPiece != piece)
		throw new QBException(QBException.Type.TPINJ_NOT_SIMPLE_LOOP);
    
		// piece will move and we want to keep something that is on that loop, let's
		// take any neighbor
		curPiece = curPiece.GetNeighbor(dirs.Key);
		System.Diagnostics.Debug.Assert(curPiece != null);
    
		// we have confirmed that the injector is on a closed loop, prepare a
		// collision volume to get the tube that goes inside
    
		Axis o2 = Tools.GetAxisFromDirection(dir);
    
//		Piece.BoxF collBox = new Piece.BoxF();
		Vector3 boxMin = lowBox.GetPosition().ToVector3();
		Vector3 boxMax = highBox.GetPosition().ToVector3();
		boxMin[Tools.GetIntFromAxis(piece.GetAxis())] += 1;
//		++collBox.Key[Tools.GetIntFromAxis(piece.GetAxis())];
		boxMin[Tools.GetIntFromAxis(o2)] += 1;
//		++collBox.Key[Tools.GetIntFromAxis(o2)];
		boxMax[Tools.GetIntFromAxis(Tools.GetComplementaryAxis(piece.GetAxis(), o2))] += 1;
//		++collBox.Value[Tools.GetIntFromAxis(Tools.GetComplementaryAxis(piece.GetAxis(), o2))];
    
		if (piece.GetAlignment()){
			boxMin += s_offsetSpaces;
			boxMax += s_offsetSpaces;
			}
		System.Diagnostics.Debug.WriteLine(boxMax);
		
		KeyValuePair<Vector3, Vector3> collBox = new KeyValuePair<Vector3, Vector3>(boxMin, boxMax);
    
			Tube target = null;
    
		// Find a 
		/// \todo slow, use octree ?
		foreach (var pieceList in m_pieces){
		foreach (Piece p in pieceList)
			if (p.Intersect(collBox))
			{
			if (p.GetType() != PieceType.TUBE || target != null){
					throw new QBException(QBException.Type.TPINJ_NOT_ONE_TUBE);
			}
			target = (Tube)(p);
			System.Diagnostics.Debug.Assert(target != null);
			}
		}
    
		if (target == null)
		throw new QBException(QBException.Type.TPINJ_NOT_ONE_TUBE);
    
		if (target.GetLength() < 4)
		throw new QBException(QBException.Type.TPINJ_TUBE_TOO_SHORT);
    
		// now find the position where to put the injector
		// the tube is not on the same alignment, thus it is shifted +-vec3(.5) but
		// it is not that big of a problem, we can ignore that
		Axis to = target.GetAxis();
    
		int pos = piece.GetPosition()[Tools.GetIntFromAxis(to)] - target.GetPosition()[Tools.GetIntFromAxis(to)];
    
		System.Diagnostics.Debug.Assert(pos >= 0);
    
		pos = System.Math.Max(pos - 2, 0);
    
		pos -= System.Math.Max((pos + 4) - target.GetLength(), 0);
    
		System.Diagnostics.Debug.Assert(pos >= 0);
    
		VectorInt3 absPos = target.GetPosition();
		absPos[Tools.GetIntFromAxis(to)] += pos;
    
		var multiStep = new MultiStep();
    
		// we must now put the injector at position pos
		multiStep.PushStep(MoveInjector(piece, target, absPos));
    
		multiStep.PushStep(DeleteBlock(curPiece));
    
		m_steps.AddLast(multiStep);
	}
	
	/**
	 * 
	 */
	public Step DeleteBlock(Piece piece)
	{
		LinkedList<Piece> remaining = new LinkedList<Piece>();
		remaining.AddFirst(piece);
		HashSet<Piece> toDelete = new HashSet<Piece>(); 
		toDelete.Add(piece);
    
		while (remaining.Count > 0)
		{
			var curPiece = remaining.First;
			
			foreach (KeyValuePair<Direction, Piece> n in curPiece.Value){
				if (!toDelete.Contains(n.Value)){
					toDelete.Add(n.Value);
					remaining.AddLast(n.Value);
				}
			}
    		remaining.RemoveFirst();
		}
    
		var step = new DeletePiece();
		
		foreach (var p in toDelete){
			step.AddPiece(p.GetDescriptor());
			DeletePiece(p);
			p.UninitVisual();
		}
    
		return step;
	}
	/// \}
	
	
	/// \name Save & load
	/// \{
	/**
	 * Save the steps in a stream
	 */
	public void SaveSteps(BinaryWriter bw)
	{	
		bw.Write(m_steps.Count);
			foreach (Step step in m_steps){
			bw = Step.CompleteSerialize(bw, step);
		}
	}
	
	/**
	 * Load the steps from a stream and apply them to the current circuit
	 */
	public void ReadSteps(BinaryReader br)
	{
		m_saveSteps = false;
		
		int numSteps = br.ReadInt32();
		for (uint i = 0; i < numSteps; ++i)
		{
			Step step;
			br = Step.CompleteDeserialize(br, out step);
			
			m_steps.AddLast(step);
			
			step.Execute(this);
		}
		ResetOffsets();
		UnselectAll();
		
		m_saveSteps = true;
	}
	/// \}
	
	/**
	 *  Returns the center of the bounding box of the selection (and the smooth movement offset)
	 */
	public KeyValuePair<Vector3, Vector3> GetSelectionCenter()
	{
		Vector3 position = Vector3.zero;
		Vector3 offset = Vector3.zero;
		
		if (m_selection.Count != 0){
		
			// calculate the bounding box of the selection
			Vector3 p1 = new Vector3(System.Single.PositiveInfinity, System.Single.PositiveInfinity, System.Single.PositiveInfinity);
			Vector3 p2 = (-p1);
			foreach (Piece p in m_selection)
			{
				KeyValuePair<VectorInt3, VectorInt3> box = p.GetCollisionBox();
				Vector3 first = box.Key.ToVector3();
				Vector3 second = box.Value.ToVector3();
				if (p.GetAlignment())
				{
					first += s_offsetSpaces;
					second += s_offsetSpaces;
				}
				p1 = Vector3.Min(p1, first);
				p2 = Vector3.Max(p2, second);
			}
			
			position = (p1 + p2) / 2.0f;
			
			// calculate the smooth movement offset of the selection
			if (m_moveDirection != Direction.INVALID){
				offset = -Tools.GetVectorFromDirection(m_moveDirection).ToVector3();
			}
		}
		
		return  new KeyValuePair<Vector3, Vector3>(position, offset);
	}


	/**
	 * 
	 * */
	public void FullCoherenceCheck()
	{
		uint selected = 0;
		bool align = false;
		foreach (var pieceList in m_pieces)
		{
		foreach (Piece p in pieceList)
		{
			// no null pointers
			System.Diagnostics.Debug.Assert(p != null);
			// piece is on correct alignment
			System.Diagnostics.Debug.Assert(p.GetAlignment() == align);
			foreach (KeyValuePair<Direction, Piece> n in p)
			{
			// no null neighbor
			System.Diagnostics.Debug.Assert(n.Value != null);
			// neighbor coherent
			System.Diagnostics.Debug.Assert(n.Value.GetNeighbor(Tools.InvertDirection(n.Key)) == p);
			// neighbor has same alignment
			System.Diagnostics.Debug.Assert(n.Value.GetAlignment() == p.GetAlignment());
			// neighbor is well positionned
			switch (n.Key)
			{
				case Direction.LEFT:
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().x < p.GetPosition().x);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().y == p.GetPosition().y);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().z == p.GetPosition().z);
				break;
				case Direction.RIGHT:
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().x > p.GetPosition().x);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().y == p.GetPosition().y);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().z == p.GetPosition().z);
				break;
				case Direction.DOWN:
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().x == p.GetPosition().x);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().y < p.GetPosition().y);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().z == p.GetPosition().z);
				break;
				case Direction.UP:
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().x == p.GetPosition().x);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().y > p.GetPosition().y);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().z == p.GetPosition().z);
				break;
				case Direction.REAR:
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().x == p.GetPosition().x);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().y == p.GetPosition().y);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().z < p.GetPosition().z);
				break;
				case Direction.FRONT:
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().x == p.GetPosition().x);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().y == p.GetPosition().y);
				System.Diagnostics.Debug.Assert(n.Value.GetPosition().z > p.GetPosition().z);
				break;
				default:
				System.Diagnostics.Debug.Assert(false);
				break;
			}
			}
			// non-box neighbors
			if (p.GetType() != PieceType.BOX)
			foreach (KeyValuePair<Direction, Piece> n in p)
			{
				// all neighbors must be boxes
				System.Diagnostics.Debug.Assert(n.Value.GetType() == PieceType.BOX);
				// neighbors must be aligned on axis
				System.Diagnostics.Debug.Assert(Tools.GetAxisFromDirection(n.Key) == p.GetAxis());
			}
			// count selected
			if (p.IsSelected()){
				++selected;
			}
		}
		align = true;
		}
		
		// Check pieces in selection 
		foreach (Piece p in m_selection)
		{
		// no null pointers
		System.Diagnostics.Debug.Assert(p != null);
		// selected piece is in circuit
//		System.Diagnostics.Debug.Assert(std.find(m_pieces[0].begin(), m_pieces[0].end(), p) != m_pieces[0].end() || std.find(m_pieces[1].begin(), m_pieces[1].end(), p) != m_pieces[1].end()); //TODO3 recreate this line
		// check that piece is selected
		System.Diagnostics.Debug.Assert(p.IsSelected());
		}

		// Check gap filler pieces
		foreach (Piece p in m_gapFillers)
		{
		// pointers are unique
//		System.Diagnostics.Debug.Assert(p.unique()); //TODO3 recreate this line
		// no neighbor
		foreach (KeyValuePair<Direction, Piece> n in p){ //TODO3 recreate these lines
			UnityEngine.Debug.LogError(n.ToString());
				System.Diagnostics.Debug.Assert(false);
		}
		}
    
		System.Diagnostics.Debug.Assert(selected == m_selection.Count);
    
		// FIXME do something about this
		// number of pieces
		//assert(pcs::Piece::GetPieceCount() ==
		//    m_pieces[0].Count + m_pieces[1].Count +
		//    (m_ghostPiece ? 1 : 0) + m_gapFillers.Count);
	}

	
	/**
	 * Begin a smooth move step: Move the selection 
	 * about 1.0 towards direction d. Perform all 
	 * necessary topological corrections to the circuit 
	 * in order to do so (incl those for Duplication).
	 */
	public void SmoothMoveStep(Direction d)
	{
		bool handled = false;
    
		if (Tools.DirectionIsPositive(d)){
			++m_moveSteps;
		} else { 
			--m_moveSteps;
		}
    
		if (m_action == Action.MOVE_DUPLICATE)
		{
			System.Diagnostics.Debug.Assert(m_moveSteps == (Tools.DirectionIsPositive(d) ? 1 : -1));
		
			try
			{
					DuplicateSelection(d, false);
			}
			catch
			{
				m_offset = 0;
				m_moveDirection = Direction.INVALID;
				m_moveSteps = 0;
				ResetOffsets();
				throw new QBException(QBException.Type.DUP_IMPOSSIBLE);
			}
			m_action = Action.MOVE;
			handled = true;
		}
    
		if (m_moveSteps == 0 && !(m_duplicateSource.Count == 0))
		{
			RevertDuplicate(d);
			m_action = Action.MOVE_DUPLICATE;
			handled = true;
		}
    
			if (!(m_duplicateSource.Count == 0) && System.Math.Abs(m_moveSteps) > 5) {
			CommitDuplicate();
		}
    
			if (!handled){
			MoveSelection(d);
		}
	}
	
	/**
	 * Move a piece without doing any check but handles crossing
	 */
	public void MovePiece(Piece piece, VectorInt3 move, Direction d)
	{
		CheckCrossing(piece, d);
		
		piece.Move(move);
		piece.SetPositionOffset((-move).ToVector3());
	}
	
	/**
	 * Move a box keeping its neighbors coherent
	 */
	public void MoveBox(Box box, Direction d)
	{
			System.Diagnostics.Debug.Assert(box != null);
    
			MovePiece(box, Tools.GetVectorFromDirection(d), d);
    
		System.Diagnostics.Debug.WriteLine("moving direction side");
			MoveBoxSide(box, d, d);
    
		System.Diagnostics.Debug.WriteLine("moving opposite direction side");
			MoveBoxSide(box, Tools.InvertDirection(d), d);
	}
	
	/**
	 * Keep a box side's neighbors coherent according to a move
	 * 
	 * This method is only called with sideDir being collinear with d ... 
	 */
	public void MoveBoxSide(Box box, Direction sideDir, Direction d)
	{
			Piece pSide = box.GetNeighbor(sideDir);
		if (pSide != null)
		{
			System.Diagnostics.Debug.WriteLine("has side");
			switch (pSide.GetType())
			{
			case PieceType.INJECTOR:
			case PieceType.CAP:
			case PieceType.TUBE:
					{
					System.Diagnostics.Debug.WriteLine("side is tube or injector");
					
					// Get the opposite side box of the neighbor
						Piece nextBox = pSide.GetNeighbor(sideDir);
						System.Diagnostics.Debug.Assert(nextBox.GetType() == PieceType.BOX);
		
					if (d == sideDir)
					{
						System.Diagnostics.Debug.WriteLine("reducing");
						ReduceTube(pSide, d);
					}
					else
					{
						System.Diagnostics.Debug.WriteLine("enlarging");
						// If the tube lies in positive axis direction it has to be moved as well.
						if (Tools.DirectionIsPositive(sideDir)){
							System.Diagnostics.Debug.WriteLine("moving");
								pSide.Move(Tools.GetVectorFromDirection(d));
						}
						ExpandTube(pSide, d);
					}
				}
				break;
				case PieceType.BOX:
				{
					System.Diagnostics.Debug.WriteLine("side is box");
						Box side = (Box)pSide;
		
					if (d == sideDir) 
					{
						// moving towards the neighbor
						if (!WillMove(side, d)){
							System.Diagnostics.Debug.WriteLine("side box won't move, moving this box to the side");
								MoveBoxToBox(side, box, d);
						}
					}
					else 
					{
						// not moving towards the neighbor
						System.Diagnostics.Debug.WriteLine("spawning tube");
						Tube tube;
						if (box == m_duplicateCutBox && sideDir != d){
								if (Tools.DirectionIsPositive(sideDir)){
								tube = SpawnTube<WeakTube>(Tools.GetAxisFromDirection(d), box, side, d);
							} else {
								tube = SpawnTube<WeakTube>(Tools.GetAxisFromDirection(d), side, box, d);
							}
						} else {
								if (Tools.DirectionIsPositive(sideDir)){
								tube = SpawnTube<Tube>(Tools.GetAxisFromDirection(d), box, side, d);
							} else {
								tube = SpawnTube<Tube>(Tools.GetAxisFromDirection(d), side, box, d);
							}
						}
						if (tube != null && box.IsSelected() && side.IsSelected()){
								SelectOnly(tube);
						}
					}
				}
					break;
				default:
					System.Diagnostics.Debug.Assert(false);
					break;
			}
		}
	}
	
	/**
	 * Duplicate the box 'box' with the new box to be 
	 * moved instead of the original box, and beneighbor 
	 * the two accordingly.
	 *
	 * \warning This does not keep neighbors coherent!
	 */
	public Box DuplicateBox(Box box, Direction d)
	{
		System.Diagnostics.Debug.Assert(box != null);
//		UnityEngine.Debug.Log("DuplicateBox called ...");
		
		bool align = box.GetAlignment();
		VectorInt3 move = Tools.GetVectorFromDirection(d);
		
		// Set up a new box 
		// to move on behalf of the original box
		var newBox = new Box();
		newBox.SetPosition(align, box.GetPosition() + move);
		newBox.SetAxis(Tools.GetAxisFromDirection(d)); // "rotate" new box to movement direction
		newBox.SetColor(box.GetColor());
		newBox.SetPositionOffset((-move).ToVector3());
		//
		newBox.InitVisual();
    
		// shrink (and offset) the original box to be gradually restored in it's unchanged position as the selection moves
		box.SetAxis(Tools.GetAxisFromDirection(d)); // "rotate" box to movement direction
		box.SetLengthOffset(-1); // shrink the box to 0 length
		if (!Tools.DirectionIsPositive(d)){ // offset the box if necessary
			box.SetPositionOffset((-move).ToVector3());
		}
		
		// Correct neighbor relationships with the new box
    	var n = box.GetNeighbor(d);
		if (n != null){
			newBox.SetNeighbor(d, n);
			n.SetNeighbor(Tools.InvertDirection(d), newBox);
		}
		newBox.SetNeighbor(Tools.InvertDirection(d), box);
		box.SetNeighbor(d, newBox);
    
		//
		int nAlign = align ? 1 : 0;
		m_pieces[nAlign].AddLast(newBox);
    
		return newBox;
	}
	
	/**
	 * Move a box to another box keeping neighbors coherent
	 *
	 * This function moves box2 to box. It checks if box2 can move or not and
	 * may just move the neighbors or move box2 and put it with box in
	 * m_potentialMerge.
	 */
	public void MoveBoxToBox(Box box, Box box2, Direction d)
	{
			System.Diagnostics.Debug.Assert(box.GetAlignment() == box2.GetAlignment());
			System.Diagnostics.Debug.Assert(!WillMove(box, d));
    
		// we must not rely on the fixed-box check because this box has already moved
		if (WillMove(box2, d, false))
		{
		System.Diagnostics.Debug.WriteLine("this box will move, there may be a merge, keeping it for " + "later");
		m_potentialMerge.Add(new KeyValuePair<Box, Box>(box, box2));
		}
		else
		{
		System.Diagnostics.Debug.WriteLine("this box won't move, moving only selected neighbors");
		foreach (KeyValuePair<Direction, Piece> p in box2)
			if (p.Value.IsSelected() && p.Key != d && p.Key != Tools.InvertDirection(d))
			{
			box2.SetNeighbor(p.Key, null);
			box.SetNeighbor(p.Key, p.Value);
			p.Value.SetNeighbor(Tools.InvertDirection(p.Key), box);
			}
		}
	}
	
	/**
	 * Merge two boxes together
	 */
	public void MergeBoxes(Box box, Box box2, Direction d)
	{
		System.Diagnostics.Debug.Assert(box.GetAlignment() == box2.GetAlignment());
    
		Direction boxDir = Direction.INVALID;
    
		foreach (KeyValuePair<Direction, Piece> p in box2){
			if (p.Value == box){
					boxDir = Tools.InvertDirection(p.Key);
			} else {
				box.SetNeighbor(p.Key, p.Value);
				p.Value.SetNeighbor(Tools.InvertDirection(p.Key), box);
			}
		}
    
		System.Diagnostics.Debug.Assert(boxDir != Direction.INVALID);
    
		// if the neighbor hasn't been overrided by the above loop, it means that
		// there is no neighbor
		if (box.GetNeighbor(boxDir) == box2) {
			box.SetNeighbor(boxDir, null);
		}
    
		// add the tags
		foreach (var t in box2.GetTags())
		box.GetTags().Add(t);
    
		UnselectOnly(box2);
		SelectOnly(box);
    
		DeletePiece(box2);
    
		box.SetPositionOffset(Tools.GetVectorFromDirection(boxDir).ToVector3());
    
		// put a gap filler for the old box that disappeared
		var boxGF = new Box();
		boxGF.SetPosition(box2.GetAlignment(), box2.GetPosition());
		boxGF.SetAxis(Tools.GetAxisFromDirection(d));
		boxGF.SetLength(0);
		boxGF.SetLengthOffset(1);
		boxGF.SetColor(box2.GetColor());
		//
		boxGF.InitVisual();
		
		// 
		box2.UninitVisual();
		
		var n = box.GetNeighbor(Tools.InvertDirection(boxDir));
			if (n != null){
			if (n.IsSelected()){
					boxGF.SetSelected(true);
			}
		}
		if (Tools.DirectionIsPositive(d))
		{
			VectorInt3 move = Tools.GetVectorFromDirection(d);
			boxGF.Move(move);
			boxGF.SetPositionOffset((-move).ToVector3());
		}
		boxGF.ReplaceTags(box.GetTags());
		m_gapFillers.AddLast(boxGF);
	}

	
	/**
	 * Reduce a tube and delete it if it become of size 0
	 *
	 * This function keep neighbors coherent
	 */
	public void ReduceTube(Piece tube, Direction d)
	{
		System.Diagnostics.Debug.Assert(tube != null);
    
		var move = Tools.GetVectorFromDirection(d);
		int length = tube.GetLength();
    
		// if the tube becomes 0-length, it will be deleted later
		tube.SetLength(length - 1);
		if (tube.GetLengthOffset() == 0){
			tube.SetLengthOffset(1);
		} else {
			tube.SetLengthOffset(0);
		}
		if (Tools.DirectionIsPositive(d))
		{
		tube.Move(move);
		tube.SetPositionOffset((-move).ToVector3());
		}
    
		if (tube.GetLength() == 0)
		m_potentialRemove.Add(tube);
	}

	
	/**
	 * Expand a tube
	 */
	public void ExpandTube(Piece tube, Direction d)
	{
		tube.SetLength(tube.GetLength() + 1);
		if (tube.GetLengthOffset() == 0)
		tube.SetLengthOffset(-1);
		else
		tube.SetLengthOffset(0);
		if (!Tools.DirectionIsPositive(d))
		tube.SetPositionOffset((-Tools.GetVectorFromDirection(d)).ToVector3());
	}
	
	/**
	 * Spawn a tube of size 1 between two boxes.
	 *
	 * b1 *must* be before b2.
	 */
	public Tube SpawnTube<T>(Axis axis, Box b1, Box b2, Direction d) where T : Tube, new()
	{
			System.Diagnostics.Debug.Assert(b1 != null);
			System.Diagnostics.Debug.Assert(b2 != null);
    
			System.Diagnostics.Debug.Assert(b1.GetAlignment() == b2.GetAlignment());
		  
			var dist = BoxDistance(axis, b1, b2);
		  
			System.Diagnostics.Debug.Assert(dist >= 1);
    
			// special case
			// this case is complicated and seem to occur only in one configuration
			// if a box that is not a boundary has to be moved and the box before it is a
			// boundary and has been moved, we have two boxes at the same position
			// this box will go in the "remaining" list in MoveSelection and will be
			// moved at the end
			// when MoveBoxSide is called on the negative side, it will call SpawnTube
			// and we get here
			// there is no need to spawn a tube, so we return
			if (dist == 1){
				return null;
			}
		  
			var directions = Tools.GetDirectionsFromAxis(axis);
			bool align = b1.GetAlignment();
		  
//				var tube = new default(T); 
			Tube tube = new T();
			tube.SetAxis(axis);
			tube.SetPosition(align, b1.GetPosition() + Tools.GetVectorFromDirection(directions.Value));
			tube.SetLength(1);
			tube.SetColor(b1.GetColor());
			if (d != Direction.INVALID)
			{
				tube.SetLengthOffset(-1);
				if (!Tools.DirectionIsPositive(d)){
						tube.SetPositionOffset((-Tools.GetVectorFromDirection(d)).ToVector3());
				}
			}
		
			tube.InitVisual();
		  
			// make the tube neighbor of boxes
			b1.SetNeighbor(directions.Value, tube);
			tube.SetNeighbor(directions.Key, b1);
			b2.SetNeighbor(directions.Key, tube);
			tube.SetNeighbor(directions.Value, b2);
		  
			// the tags to be assigned are the intersection of the tags of the bounding boxes
			var t1 = b1.GetTags();
			var t2 = b2.GetTags();
			System.Diagnostics.Debug.Assert(tube.GetTags().Count == 0);
//			std.set_intersection(t1.begin(), t1.end(), t2.begin(), t2.end(), std.inserter(tube.GetTags(), tube.GetTags().end()));
			HashSet<uint> intersect = new HashSet<uint>(t1);
			intersect.IntersectWith(t2);
			tube.SetTags(intersect);
		  
			int nAlign = align ? 1 : 0;
			m_pieces[nAlign].AddLast(tube);
		  
			return (Tube)tube;
	}
	
	/**
	 * Removes a tube that as reached 0-length
	 *
	 * This function may keep the tube as a gap filler until the next
	 * ResetOffsets().
	 */
	public void RemoveTube(Piece tube)
	{
		System.Diagnostics.Debug.Assert(tube != null);
		System.Diagnostics.Debug.Assert(tube.GetType() == PieceType.TUBE);
		
		// do not remove useful tubes
		if (tube.GetLength() > 0)
		{
			System.Diagnostics.Debug.WriteLine("tube is not 0-length");
			return;
		}
    
		System.Diagnostics.Debug.WriteLine("tube is 0-length");
		
		// delete the tube and make boxes neighbors
		KeyValuePair<Direction, Direction> tubeDirs = Tools.GetDirectionsFromAxis(tube.GetAxis());
		
		Box p1 = (Box)tube.GetNeighbor(tubeDirs.Key);
		Box p2 = (Box)tube.GetNeighbor(tubeDirs.Value);
    
		System.Diagnostics.Debug.Assert(p1 != null);
		System.Diagnostics.Debug.Assert(p2 != null);
    
		p1.SetNeighbor(tubeDirs.Value, p2);
		p2.SetNeighbor(tubeDirs.Key, p1);
    
			bool selected = tube.IsSelected();
    
		UnselectOnly(tube);
		DeletePiece(tube);
		
		if (System.Math.Round(tube.GetLengthOffset()) > 0){
			// we should keep the tube as a gap filler
			System.Diagnostics.Debug.WriteLine("keeping tube as gap filler");
			tube.SetSelected(selected);
			tube.ClearNeighbors();
			m_gapFillers.AddLast(tube);
		} else {
			tube.UninitVisual();
		}
	}
	
	/**
	 * Determine if a piece is prevented from moving 
	 * by it's neighborhood
	 */
	public bool WillMove(Piece piece, Direction d, bool fixedCheck = true)
	{
		if (piece == null)
		return false;
    
		if (!piece.IsSelected())
		return false;
    
		if (piece.GetType() != PieceType.BOX)
		return true;
    
		// a selected box moves if all neigbor pieces that are not on the axis of movement are selected
		// and it does not expand an injector
		// and it is not a fixed box with a neighbor behind
		//TODO2 check this foreach as it feels bug-prone
		foreach (KeyValuePair<Direction, Piece> p in piece){
			Direction dirNeigh = p.Key;
			Piece neigh = p.Value;
			if (	dirNeigh != d && 
					dirNeigh != Tools.InvertDirection(d) && 
					!neigh.IsSelected() 
				||
					(neigh.GetType() == PieceType.INJECTOR || neigh.GetType() == PieceType.CAP) && // neighbor is a cap or injector
					dirNeigh == Tools.InvertDirection(d) && // neighbor sits 'behind' the box when moved
					neigh.GetLength() == 4 && //TODO3 check/kick this line out
					!neigh.GetNeighbor(Tools.InvertDirection(d)).IsSelected())
			{
					return false;
			}
		}
		
		if (fixedCheck && IsFixedBox(piece, d))
		{
			System.Diagnostics.Debug.WriteLine("won't move because it's fixed");
			return false;
		}
		return true;
	}

	
	/**
	 * Return true if the box is listed as a fixed piece
	 */
	public bool IsFixedBox(Piece box, Direction d)
	{
		int pos = m_moveSteps + (Tools.DirectionIsPositive(d) ? - 1 : 1);
//		var iter = m_fixedBoxes.find(pos);
		if (	
//			iter != m_fixedBoxes.end() 
			m_fixedBoxes.ContainsKey(pos)
				&& 
//				iter.Value.find(std.make_pair(box.GetAlignment(), box.GetPosition())) != iter.Value.end())
				m_fixedBoxes[pos].Contains(new KeyValuePair<bool, VectorInt3>(box.GetAlignment(), box.GetPosition()))
		)
		{
			return true;
		} else {
				return false;
		}
	}
	
	/**
	 * Check if a selected(!) box could stay in its position without 
	 * preventing any other selected pieces to move. 
	 * This is only the case if it could "slide" along the moving selection.
	 */
	public bool CanStay(Box piece, Direction d)
	{
			System.Diagnostics.Debug.Assert(piece != null);
    
			// Deny "can stay" if the box doesn't have selected neighbors in both directions on the axis of movement
			Piece n = piece.GetNeighbor(d);
			if (n == null || !n.IsSelected()){
			return false;
		}
			n = piece.GetNeighbor(Tools.InvertDirection(d));
			if (n == null || !n.IsSelected()){
			return false;
		}
		
		// Deny "can stay" if the box has any selected neighbors on the other axes
		foreach (KeyValuePair<Direction, Piece> p in piece){
			if (p.Key != d && p.Key != Tools.InvertDirection(d) && p.Value.IsSelected()) { 
				// for a box, it moves if all pieces that are not in the move axis are selected
				return false;
			}
		}	
			return true;
	}
	
	/**
	 * Fix expanded injectors, they must not be longer than 4
	 */
	public void FixInjectors(Direction d)
	{
		// if in selection we have streched injectors, make them back to normal and
		// spawn a box
		foreach (var piece in m_selection)
		if ((piece.GetType() == PieceType.INJECTOR || piece.GetType() == PieceType.CAP) && piece.GetLength() > 4)
		{
			var box = (Box)(piece.GetNeighbor(Tools.InvertDirection(d)));
			System.Diagnostics.Debug.Assert(box != null);
    
			var newBox = DuplicateBox(box, d);
    
			// keep selection coherent
			UnselectOnly(box);
			SelectOnly(newBox);
    
			// reset injector
			piece.SetLength(4);
			piece.SetLengthOffset(0);
			if (Tools.DirectionIsPositive(d))
			{
			var move = Tools.GetVectorFromDirection(d);
			piece.Move(move);
			piece.SetPositionOffset((-move).ToVector3());
			}
		}
	}
	
	/**
	 * 	Return true if no collision or other non-permissible state was 
	 *  predicted for completion of the current move
	 */
	public bool IsMoveLegal()
	{
		return !(m_isAlignColliding || m_isUnalignColliding || m_isSqueezing || m_isBadDuplicate);
	}
	
	/**
	 * Return true if the circuit is in a good state and further move can be
	 * applied with crossing support
	 */
	public bool GetWeakState()
	{
			return !(m_isAlignColliding || m_isUnalignColliding && !m_crossing || m_isSqueezing || m_isBadDuplicate);
	}
	

	/**
	 * Return true if an injector is squeezed
	 */
	public bool CheckSqueezing()
	{
		m_isSqueezing = false;
		foreach (var pieceList in m_pieces)
		foreach (Piece p in pieceList)
			if (p.CheckSqueezing())
			{
			m_isSqueezing = true;
			break;
			}
		return m_isSqueezing;
	}

	/**
	 * Fill the circuit with path and block IDs
	 */
	public void GenerateIds()
	{
		// reset all path ids
		foreach (var pieceList in m_pieces)
		foreach (Piece p in pieceList)
		{
			p.SetPathId(0);
			p.SetBlockId(0);
		}
    
		// fill in new path ids
		uint curPathId = 0;
		uint curBlockId = 0;
		foreach (var pieceList in m_pieces)
		foreach (Piece p in pieceList)
		{
			if (p.GetPathId() == 0)
			FillPath(++curPathId, p, false);
			if (p.GetBlockId() == 0)
			FillPath(++curBlockId, p, true);
		}
	}
	
	/**
	 * Fill a path or a block with an ID
	 *
	 * If followJunctions is true, it will fill the whole block.
	 */
	public void FillPath(uint id, Piece piece, bool followJunctions)
	{
			LinkedList<Piece> remaining = new LinkedList<Piece>();
		remaining.AddFirst(piece);
    
		while (remaining.Count > 0)
		{
		var p = remaining.First.Value;
    
		// on junction points, we consider that a different path but the same block
		if ((followJunctions || p.CountNeighbors() <= 2) && piece.GetType() != PieceType.CAP)
		{
			if (followJunctions)
			p.SetBlockId(id);
			else
			p.SetPathId(id);
    
			foreach (KeyValuePair<Direction, Piece> n in p)
			{
			uint pid;
			if (followJunctions)
					pid = n.Value.GetBlockId();
			else
					pid = n.Value.GetPathId();
    
				System.Diagnostics.Debug.Assert(pid == 0 || pid == id);
    
			// if we don't have handled this piece yet, add it to list
			if (pid == 0){
					remaining.AddLast(n.Value);
			}
			}
		}
    
		remaining.RemoveFirst();
		}
	}

	/**
	 * Reset collision state of pieces
	 */
	public void ResetCollisions()
	{
		foreach (var pieceList in m_pieces){
			foreach (Piece p in pieceList){
					p.ResetCollision();
			}
		}
		m_isAlignColliding = false;
		m_isUnalignColliding = false;
	}

	/**
	 * Transfer neighbors from p2 to p1
	 */
	public void TransferNeighbors(Piece p1, Piece p2)
	{
		System.Diagnostics.Debug.Assert(p1 != null);
		System.Diagnostics.Debug.Assert(p2 != null);
    
		foreach (KeyValuePair<Direction, Piece> n in p2)
		{
		p1.SetNeighbor(n.Key, n.Value);
		n.Value.SetNeighbor(Tools.InvertDirection(n.Key), p1);
		}
    
		p2.ClearNeighbors();
	}
	
	/**
	 * Link b1 and b2 with a tube if necessary.
	 */
	public Tube LinkBoxes(Axis a, Box b1, Box b2)
	{
		System.Diagnostics.Debug.Assert(b1 != null);
		System.Diagnostics.Debug.Assert(b2 != null);
    
		Tube tube = null;
    
		int diff = BoxDistance(a, b1, b2);
    
		System.Diagnostics.Debug.Assert(diff != 0);
    
		if (diff < 0)
		{
		Box temp = b1;
		b1 = b2;
		b2 = temp;
//		std.swap(b1, b2);
			
		diff = -diff;
		}
    
		// if the boxes are more than 1 unit apart, we need a tube
		if (diff > 1)
		{
		tube = SpawnTube<Tube>(a, b1, b2, Direction.INVALID);
		System.Diagnostics.Debug.Assert(tube != null);
		tube.SetLength(diff - 1);
		}
		else // diff == 1
		{
		// no need of tube, we juste make boxes neigbors
		var dirs = Tools.GetDirectionsFromAxis(a);
		b1.SetNeighbor(dirs.Value, b2);
		b2.SetNeighbor(dirs.Key, b1);
		}
    
		return tube;
	}
	

	/**
	 * Calculate the distance between two boxes according to axis a.
	 *
	 * Two adjacent boxes have distance 1.
	 */
	public int BoxDistance(Axis a, Box b1, Box b2)
	{
		System.Diagnostics.Debug.Assert(b1 != null);
		System.Diagnostics.Debug.Assert(b2 != null);
    
		switch (a)
		{
		case Axis.X:
			return b2.GetPosition().x - b1.GetPosition().x;
		case Axis.Y:
			return b2.GetPosition().y - b1.GetPosition().y;
		case Axis.Z:
			return b2.GetPosition().z - b1.GetPosition().z;
		default:
			System.Diagnostics.Debug.Assert(false);
			return 0;
		}
	}

	/**
	 * 	Bind box neighbors to a newly created piece. 
	 *  Creating boxes if necessary.
	 *
	 * This function is used for circuit loading
	 */
	public Box BindNeighbor(Dictionary<VectorInt3, Box> boxMap, Direction direction, VectorInt3 position, Piece piece)
	{
		bool align = piece.GetAlignment();
		int alignInt = align ? 1 : 0;
		
		VectorInt3 boxPosition = new VectorInt3();
		VectorInt3 dirNeighbor = Tools.GetVectorFromDirection(direction);
		if (Tools.DirectionIsPositive(direction)){
			boxPosition = position + dirNeighbor * piece.GetLength();
		} else {
			boxPosition = position + dirNeighbor;
		}
		
	
		Box box;
		if (boxMap.ContainsKey(boxPosition)){ // found a box in place. Establishing neighborhood relationship
			box = boxMap[boxPosition];
		} else {   // if there was no box there, make a new one
			
			
			box = new Box();
			box.SetPosition(align, boxPosition);
			box.SetColor(piece.GetColor());
//			UnityEngine.Debug.Log("added box at position " + boxPosition.ToString());
			//
			box.InitVisual();
			
			m_pieces[alignInt].AddLast(box);
			
		  	boxMap.Add(boxPosition, box);
			
			BindBox(boxMap, box);
			}
		
		// bind piece with box
		box.SetNeighbor(Tools.InvertDirection(direction), piece);
		piece.SetNeighbor(direction, box);
		
		// add tags from the neighbor
		box.AddTags(piece.GetTags());
//		var bTags = box.GetTags();
//		var pTags = piece.GetTags();
//		bTags.insert(pTags.begin(), pTags.end());
    
			return box;
	}
	
	/**
	 * Bind a box with surrounding boxes if they exist
	 *
	 * This function is used for circuit loading
	 */
	public void BindBox(Dictionary<VectorInt3, Box> boxMap, Piece box)
	{
		foreach (Direction d in System.Enum.GetValues(typeof(Direction)))
		{
			if (d == Direction.INVALID) { continue; }
			VectorInt3 posN = new VectorInt3();
			posN = box.GetPosition() + Tools.GetVectorFromDirection(d);
			if (boxMap.ContainsKey(posN))
			{
				Box neig = boxMap[posN];
				if(neig.GetPosition() == posN){
					box.SetNeighbor(d, neig);
					neig.SetNeighbor(Tools.InvertDirection(d), box);
				}
			}
		}
	}
	
	// prototype for a callback for WalkPath method.
	public delegate bool funcVisit (Piece p, bool b);
	// prototype for a callback for TraverseBlock method.
	public delegate bool Traversor(Piece p, Dictionary<VectorInt3, KeyValuePair<Direction, Piece>> toTraverse);
	
	/**
	 * Traverse a block starting from a specific piece and call a 
	 * user-specified function on each piece (Traversor pattern).
	 *
	 * 
	 */
	public void TraverseBlock(Piece piece, Traversor traverse)
	{	
		Dictionary<VectorInt3, KeyValuePair<Direction, Piece>> toTraverse = new Dictionary<VectorInt3, KeyValuePair<Direction, Piece>>();
		
		// init recursion
		toTraverse.Add(piece.GetPosition(), new KeyValuePair<Direction, Piece>(Direction.INVALID, piece));
		
		// Recursion (depth-first traversal)
		while (toTraverse.Count > 0)
		{
			// extract the one element
			var e = toTraverse.GetEnumerator(); e.MoveNext();
			var currNode = e.Current;
			
			if (traverse(currNode.Value.Value, toTraverse)){
				break;
			}	
		}
	}
	
	/**
	 * Traverse a block starting from a specific piece and call a 
	 * user-specified function on each piece (Traversor pattern).
	 *
	 * func is not called for the starting piece. 
	 * 
	 * returns true if a func returned true at some point, 
	 * false otherwise.
	 */
//	public bool WalkPath(Piece piece, funcVisit func)
//	{
//		// Traverse graph depth-first 
//		
//		// Collection of pieces' neighbors still to visit
//		LinkedList<KeyValuePair<Piece, List<Direction>>> remaining = 
//			new LinkedList<KeyValuePair<Piece, List<Direction>>>();
//		// List of still un-visited neighbor directions of a piece
//		List<Direction> neigUnvisited = new List<Direction>(); 
//		// Collection of pieces where all neighbors have been visited
//		HashSet<Piece> handled = new HashSet<Piece>{piece}; 
//		
//		// Start traversal with first-degree neighbors
//		foreach (KeyValuePair<Direction, Piece> n in piece)
//		{
//			if (func(n.Value, false)){
//					return true;
//			} else { // if no repeated visit occurred, add neighbor direction to temp list
//					neigUnvisited.Add(n.Key);
//			}
//		}
//		List<Direction> dirListCurrent = new List<Direction>(neigUnvisited);
//		remaining.AddFirst(new KeyValuePair<Piece, List<Direction>>(piece, dirListCurrent));
//    
//    	// Recursively visit neighbors and add their neighbors to remaining
//		while (remaining.Count > 0) {
//			var curIter = remaining.First; //TODO3 check if these iterators behave like the original code
//			var current = remaining.First.Value;
//		
//			foreach (var dir in current.Value){
//				var piece2 = current.Key.GetNeighbor(dir);
//		
//				neigUnvisited.Clear();
//				foreach (KeyValuePair<Direction, Piece> n in piece2)
//				// we don't follow caps since they are not links and we don't go backward
//				if (n.Value.GetType() != PieceType.CAP && n.Key != Tools.InvertDirection(dir))
//				{
//					bool h = handled.Contains(n.Value);
//					if (func(n.Value, h)){
//						return true;
//					} else if (!h) {
//						neigUnvisited.Add(n.Key);
//					}
//				}
//		
//				List<Direction> dirListCurrent2 = new List<Direction>(neigUnvisited);
//				remaining.AddFirst(new KeyValuePair<Piece, List<Direction>>(piece2, dirListCurrent2));
//				
//				handled.Add(piece2);
//			}
//		
//			remaining.Remove(curIter);
//		}
//    
//		return false;
//	}
	
	/**
	 * Fills a path with the color of the given piece
	 */
	public void FillWithColor(Piece piece)
	{
		Vector4 color = piece.GetColor();
		HashSet<VectorInt3> traversed = new HashSet<VectorInt3>();

		TraverseBlock(
				piece, 
				(Piece p, Dictionary<VectorInt3, KeyValuePair<Direction, Piece>> toTraverse) =>
				{	
						p.SetColor(color);
			
					Direction currDir = toTraverse[p.GetPosition()].Key;
				
					// iterate neighbors
					foreach (KeyValuePair<Direction, Piece> neig in p){
						if (currDir == neig.Key) { continue; }
						if (!toTraverse.ContainsKey(neig.Value.GetPosition())){ 
							if (!traversed.Contains(neig.Value.GetPosition())){
								toTraverse.Add(neig.Value.GetPosition(), new KeyValuePair<Direction, Piece>(Tools.InvertDirection(neig.Key), neig.Value));
							}
						} 
					}	
					traversed.Add(p.GetPosition());
					toTraverse.Remove(p.GetPosition());
					return false;
				});	
	}
	
	
	
	/**
	 * Fills a path with the color of the given piece
	 */
	public void SelectBlock(Piece piece, bool invert)
	{
		HashSet<VectorInt3> traversed = new HashSet<VectorInt3>();

		TraverseBlock(
				piece, 
				(Piece p, Dictionary<VectorInt3, KeyValuePair<Direction, Piece>> toTraverse) =>
				{	
					if (invert && p.IsSelected()){
						this.UnselectOnly(p);
					} else {
						this.SelectOnly(p);
					}
			
					Direction currDir = toTraverse[p.GetPosition()].Key;
				
					// iterate neighbors
					foreach (KeyValuePair<Direction, Piece> neig in p){
						if (currDir == neig.Key) { continue; }
						if (!toTraverse.ContainsKey(neig.Value.GetPosition())){ 
							if (!traversed.Contains(neig.Value.GetPosition())){
								toTraverse.Add(neig.Value.GetPosition(), new KeyValuePair<Direction, Piece>(Tools.InvertDirection(neig.Key), neig.Value));
							}
						} 
					}	
					traversed.Add(p.GetPosition());
					toTraverse.Remove(p.GetPosition());
					return false;
				});	
	}
	
	/**
	 * Check if the piece is on a loop. 
	 * A cap is never considered to be part of a loop
	 */
	public bool IsOnLoop(Piece piece)
	{
		bool isOnLoop = false;
		
		TraverseBlock(piece, 
			(Piece p, Dictionary<VectorInt3, KeyValuePair<Direction, Piece>> toTraverse) =>
			{	
				if (p.GetType() == PieceType.CAP){
					return true;
				}
				Direction currDir = toTraverse[p.GetPosition()].Key;
				
				// iterate neighbors
				foreach (KeyValuePair<Direction, Piece> neig in p){
					if (currDir == neig.Key) { continue; }
					if (!toTraverse.ContainsKey(neig.Value.GetPosition())){ 
						toTraverse.Add(neig.Value.GetPosition(), new KeyValuePair<Direction, Piece>(Tools.InvertDirection(neig.Key), neig.Value));
					} else {
						if (neig.Value.GetType() != PieceType.CAP) {
							isOnLoop = true;
							return true;
						}
					}
				}		
				toTraverse.Remove(p.GetPosition());
				return false;
			});
		
		return isOnLoop;
	}
	

	/**
	 * Cut the weak-tubed path (even if the weak tube does not exist yet)
	 */
	public void CutDuplicate(Direction d)
	{
		System.Diagnostics.Debug.Assert(m_duplicateCutBox != null);
		
		Piece weaktube = m_duplicateCutBox.GetNeighbor(Tools.InvertDirection(d));
		System.Diagnostics.Debug.Assert(weaktube != null);
		
		var dirs = Tools.GetDirectionsFromAxis(Tools.GetAxisFromDirection(d));
		
		if (weaktube.GetType() == PieceType.TUBE)
		{
//			System.Diagnostics.Debug.Assert(typeid(*weaktube) == typeid(WeakTube)); //TODO3 recreate this 2nd assert
		
			weaktube.GetNeighbor(dirs.Key).SetNeighbor(dirs.Value, null);
			weaktube.GetNeighbor(dirs.Value).SetNeighbor(dirs.Key, null);
			
			DeletePiece(weaktube);
			weaktube.UninitVisual(); //TODO3 check if its correct to remove the visuals here
		}
		else
		{
			System.Diagnostics.Debug.Assert(weaktube.GetType() == PieceType.BOX);
			
			m_duplicateCutBox.SetNeighbor(Tools.InvertDirection(d), null);
			weaktube.SetNeighbor(d, null);
		}
    
		m_duplicateCutBox = null;
	}
	
	/**
	 * Commit the duplication making it a step.
	 */
	public void CommitDuplicate()
	{
		UnityEngine.Debug.Log("commiting duplicate");
    
		System.Diagnostics.Debug.Assert(m_action == Action.MOVE);
		System.Diagnostics.Debug.Assert(m_moveSteps != 0);
		
		Piece weaktube = m_duplicateCutBox.GetNeighbor(Tools.InvertDirection(m_moveDirection));
		System.Diagnostics.Debug.Assert(weaktube != null);
		System.Diagnostics.Debug.Assert(weaktube.GetType() == PieceType.TUBE);
//			System.Diagnostics.Debug.Assert(typeid(*weaktube) == typeid(WeakTube)); //TODO3 recreate this 2nd assert
    
		var dirs = Tools.GetDirectionsFromAxis(Tools.GetAxisFromDirection(m_moveDirection));
		
		CutDuplicate(m_moveDirection);
		
		VectorInt3 bound = new VectorInt3();
		if (m_moveSteps > 0)
		{
			// Disregard fixed boxes outside the range actually travelled // we skip the 0
			//TODO3 use different API to speed up this operation
			//TODO2 check if this removal operation does exactly what the original code did
			List<int> keys = new List<int>(m_fixedBoxes.Keys);
			foreach (int fb in keys){
				if (fb <= 0 || fb > m_moveSteps){
					m_fixedBoxes.Remove(fb);
				}
			}			
//			m_fixedBoxes.erase(m_fixedBoxes.begin(), m_fixedBoxes.upper_bound(0));
//			m_fixedBoxes.erase(m_fixedBoxes.upper_bound(m_moveSteps), m_fixedBoxes.end());
			bound = weaktube.GetNeighbor(dirs.Key).GetPosition();
		}
		else
		{
			// Disregard fixed boxes outside the range actually travelled // we skip the 0
			//TODO3 use different API to speed up this operation (iterate sorted list)
			//TODO2 check if this removal operation does exactly what the original code did
			List<int> keys = new List<int>(m_fixedBoxes.Keys);
			foreach (int fb in keys){
				if (fb >= 0 || fb < m_moveSteps){
					m_fixedBoxes.Remove(fb);
				}
			}
//			m_fixedBoxes.erase(m_fixedBoxes.lower_bound(0), m_fixedBoxes.end());
//			m_fixedBoxes.erase(m_fixedBoxes.begin(), m_fixedBoxes.lower_bound(m_moveSteps));
			bound = weaktube.GetNeighbor(dirs.Value).GetPosition();
		}
    
			Duplicate duplicate = 
			new Duplicate(m_selection, m_moveDirection, bound);
		foreach (var step in m_fixedBoxes){
			foreach (var box in step.Value){
				duplicate.AddFixedBox(m_moveSteps > 0 ? (uint)step.Key : (uint)-step.Key, box.Key, box.Value);
			}
		}
			m_steps.AddLast(duplicate);
    
		// reset everything to make it like we have just started a move
		if (m_fixedBoxes.ContainsKey(m_moveSteps))
		{
			var step2 = m_fixedBoxes[m_moveSteps];
			var boxes = step2;
			m_fixedBoxes.Clear();
			if (m_moveSteps > 0){
					m_fixedBoxes[1] = boxes;
			} else {
					m_fixedBoxes[-1] = boxes;
			}
		} else {
			m_fixedBoxes.Clear();
		}
		
		if (m_moveSteps > 0){
			m_moveSteps -= 5;
			m_safeMoveSteps -= 5;
		} else {
			m_moveSteps += 5;
			m_safeMoveSteps += 5;
		}
		
		m_duplicateSource.Clear();
		m_duplicateSourceTube = null;
		GenerateIds();
	}

	/**
	 * Cancel a duplicate (only on works on first step)
	 */
	public void CancelDuplicate()
	{
		UnityEngine.Debug.Log("cancelling duplicate");
    
		m_duplicateSource.Clear();
		m_duplicateSourceTube = null;
		m_duplicateCutBox = null;
		GenerateIds();
	}

	
	/**
	 * Check if a duplication move will put the circuit in a colliding state
	 */
	public bool CheckBadDuplicate()
	{
		m_isBadDuplicate = m_duplicateSource.Count > 0 && System.Math.Abs(m_moveSteps) > m_maxDuplicateMove;
		return m_isBadDuplicate;
	}
	

	/**
	 * Check if there is a crossing happening on a move. 
	 */
	public void CheckCrossing(Piece piece, Direction dir)
	{
			if (piece.GetColliders().Count == 0){
			return;
		}
    
		System.Diagnostics.Debug.WriteLine("checking for crossing with " + piece.GetColliders().Count + " colliders");
    
		var dirs1 = GetDirectionsForCrossing(piece, dir);

		foreach (var cp in piece.GetColliders()){
			foreach (var dir1 in dirs1){
				foreach (var dir2 in GetDirectionsForCrossing(cp, dir)){
					if (CheckSegmentCrossing((dir1.vtv1), (dir1.vtv2), (dir1.theint), 
											(dir2.vtv1), (dir2.vtv2), (dir2.theint), 
											dir)){
						HandleCross(piece, cp);
					}
				}
			}
		}
	}
	
	/**
	 * Give the list of tuples (position, axis vector, length) to check against
	 * crossing
	 *
	 * The position is used for the w vector calculation whereas the direction
	 * is the u or v vector.
	 */
	public List<Piece.Segment> GetDirectionsForCrossing(Piece piece, Direction d)
	{
		// we can never have a segment which cross another by moving on the plane
		// defined by the two segments
		// if we have something like -- | we can not have the left segment going
		// right or the right segment going left because crossing occur when pieces
		// are on different alignments
    	var result = new List<Piece.Segment>();
		if (piece.GetType() != PieceType.BOX)
		if (piece.GetAxis() == Tools.GetAxisFromDirection(d))
		{
			System.Diagnostics.Debug.WriteLine("same direction, no segment to check");
				return result;
		}
		else
		{
			System.Diagnostics.Debug.WriteLine("orthogonal, one segment to check");
			result.Add(piece.GetSegment());
				return result;
		}
		else
		{
			System.Diagnostics.Debug.WriteLine("box, two segments to check");
			var caxis = Tools.GetComplementaryAxis(Tools.GetAxisFromDirection(d));
			result.Add(piece.GetSegment(caxis.Key));
			result.Add(piece.GetSegment(caxis.Value));
			return result;
		}
	}

	/**
	 * Check if a crossing is happening between two segments
	 */
	public bool CheckSegmentCrossing(VectorInt3 p1, VectorInt3 u, int l1, VectorInt3 p2, VectorInt3 v, int l2, Direction dir)
	{
		// from http://softsurfer.com/Archive/algorithm_0106/algorithm_0106.htm
		// we can do all this stuff with integers so that we have perfect precision
    
		int moveCoord = Tools.GetIntFromAxis(Tools.GetAxisFromDirection(dir));
		VectorInt3 move = Tools.GetVectorFromDirection(dir);
    
		var w = p1 - p2;
    
		var a = Tools.dot(u, u);
		var b = Tools.dot(u, v);
		var c = Tools.dot(v, v);
		var d = Tools.dot(u, w);
		var e = Tools.dot(v, w);
		var D = a * c - b * b;
		int sc;
		int tc;
    
		if (D == 0)
		{
			// segments are parallel
			System.Diagnostics.Debug.WriteLine("parallel segments");
			return false;
		}
		else
		{
			// segments are not parallel
			sc = (int)((b * e - c * d) / D);
			tc = (int)((a * e - b * d) / D);
		}
    
		// we use >= on the upper bound so that if the intersection occurs just in
		// between two pieces, only one crossing is detected
		if (sc < 0 || sc >= l1 || tc < 0 || tc >= l2)
		{
			System.Diagnostics.Debug.WriteLine("no intersection, piece too short");
			return false;
		}
    
		var dP = -(w + (sc * u) - (tc * v));
    
		if (dP[moveCoord] == 0 || dP[(moveCoord + 1) % 3] != 0 || dP[(moveCoord + 2) % 3] != 0)
		{
			// this seems to never happen, should be handled by the above case
			System.Diagnostics.Debug.WriteLine("piece does not move on the collision axis");
			return false;
		}
    
		var da = dP[moveCoord] - move[moveCoord] * 4;
		if (da < 0 && dP[moveCoord] < 0 || da > 0 && dP[moveCoord] > 0)
		{
			System.Diagnostics.Debug.WriteLine("crossing will not occur yet or has already occured");
			return false;
		}
    
		// we have a crossing
		return true;
	}
	
	/**
	 * Handle crossing of two pieces
	 */
	public void HandleCross(Piece p1, Piece p2)
	{
		System.Diagnostics.Debug.WriteLine("crossing happened");
    
		foreach (var t1 in p1.GetTags()) {
			foreach (var t2 in p2.GetTags())
			{
				System.Diagnostics.Debug.Assert(t1 != t2);
				
				KeyValuePair<uint, uint> p = new KeyValuePair<uint, uint>();
				if (t1 < t2)
					p = new KeyValuePair<uint, uint>(t1, t2);
				else
					p = new KeyValuePair<uint, uint>(t2, t1);
					
				if (m_unstable.Contains(p))
				{
					System.Diagnostics.Debug.WriteLine("stabilized on " + p.Key + " and " + p.Value);
					m_unstable.Remove(p);
				}
				else
				{
					m_unstable.Add(p);
					System.Diagnostics.Debug.WriteLine("unstabilized on " + p.Key + " and " + p.Value);
				}
			}
		}
	}

	
	public void InitVisual()
	{
	}
	
	public void Update()
	{
	}
		
	public void UpdateVisual()
	{	
		foreach (Piece p in m_pieces[0]){
			p.UpdateVisual(m_offset);
		}
		foreach (Piece p in m_pieces[1]){
			p.UpdateVisual(m_offset);
		}
		foreach (Piece p in m_gapFillers){
			p.UpdateVisual(m_offset);
		}
			
	}
	
	public void UpdatePhysics()
	{
		foreach (Piece p in m_pieces[0]){
			p.UpdatePhysics();
		}
		foreach (Piece p in m_pieces[1]){
			p.UpdatePhysics();
		}
	}
	
	/**
		* Dispose of all visuals representing the circuit
		* */ 
	public void UninitVisual()
	{
		foreach (var pieceList in m_pieces){
			foreach (Piece p in pieceList) {
				p.UninitVisual();
			}
		}	
		
		foreach (Piece p in m_gapFillers) {
			p.UninitVisual();
		}
		
		if (m_ghostPiece != null){
			m_ghostPiece.UninitVisual();
		}
	
		
	}
	
	public HashSet<KeyValuePair<uint, uint>> GetUnstablePairs()
	{
		return this.m_unstable;
	}
	
	
}


