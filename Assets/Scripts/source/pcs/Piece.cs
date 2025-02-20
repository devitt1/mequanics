using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

using tools;


namespace pcs
{
	
public enum PieceType
{
	INVALID,
	BOX,
	TUBE,
	INJECTOR,
	CAP,
	
	BOXGHOST,
	INJECTORGHOST
		
}
	
	
/**
 * 
 */
public class PieceDescriptor
{
	public PieceType type;
	public bool align;
	public VectorInt3 position = new VectorInt3();
	public Axis axis;
	public int length;
	public int injectionId;
	public Vector4 color = new Vector4();
	public HashSet<uint> tags = new HashSet<uint>();
		
  public PieceDescriptor()
  {
	  this.type = pcs.PieceType.INVALID;
	  this.align = false;
	  this.axis = Axis.INVALID;
	  this.length = 0;
	  this.injectionId = -1;
  }
		
  public PieceDescriptor(   
		PieceType type,
		bool align,
		Axis axis,
		int length,
		VectorInt3 position,
		Vector4 color) : this()
	{
		this.type = type;
		this.align = align;
		this.position = position;
		this.axis = axis;
		this.length = length;
		this.color = color;	
	}
		

	public override string ToString()
	{
		string aligned = this.align ? "(aligned)" : "(unaligned)";
		return "PieceDescriptor for " + this.type.ToString() + 
				" at " + this.position.ToString() + aligned +
				" towards " + this.axis.ToString() +
				" length " + this.length + " .";
	}	
		
}
	
/**
 * 
 */ 
public abstract class Piece : IEnumerable
{
	public struct Segment
	{
		public VectorInt3 vtv1;
		public VectorInt3 vtv2;
		public int theint;
			
		public Segment(VectorInt3 v1, VectorInt3 v2, int i)
		{
			vtv1 = v1;
			vtv2 = v2;
			theint = i;
		}
	}
		
	protected static Transform s_selection;
	protected static Transform Selected { get{ if (s_selection == null) { s_selection = GameObject.FindGameObjectWithTag("selection").transform; } return s_selection; }}
	protected static Transform s_piecesRoot;
	protected static Transform PiecesRoot { get{ if (s_piecesRoot == null) { s_piecesRoot = Selected.parent;} return s_piecesRoot; } }
		
	public static bool s_WireframesOGL = true;
		
	protected PieceCS m_cs;
	protected int m_id = -1;
	public int Id { get; set; }
			
	// put some security margin here, 0.1 is fine
	public const float COLLDIST_ALIGN = 3.9f;
	public const float COLLDIST_MISALIGN = 0.4f;
		
	protected PieceType m_type;
	protected VectorInt3 m_position = new VectorInt3();
	protected bool m_align;
	protected Axis m_axis;
	protected int m_length;

	protected int m_injectionId;

	protected Vector4 m_color = new Vector4();

	protected Dictionary<Direction, Piece> m_neighbors = new Dictionary<Direction, Piece>();

	protected bool m_selected;

	protected List<Vector3> m_collPoints = new List<Vector3>();
	protected List<Piece> m_colliders = new List<Piece>();
	protected bool m_squeezing;

	// offsets for smooth move drawing
	protected float m_lengthOff;
	protected Vector3 m_positionOff = new Vector3();
	protected Direction m_squeezeDirection;

	protected uint m_pathId;
	protected uint m_blockId;
	protected HashSet<uint> m_tags = new HashSet<uint>();
		
	private static uint g_pieceCount = 0;
	
	GameObject m_pieceUI;
		
	//----------------------------------------------
	
	public Piece() 
	{
		this.m_type = PieceType.INVALID;
		this.m_selected = false;
		this.m_squeezing = false;
		this.m_squeezeDirection = Direction.INVALID;
		this.m_lengthOff = 0F;
		this.m_positionOff = new Vector3(0, 0, 0);
		this.m_pathId = 0;
		this.m_blockId = 0;
		this.m_cs = null;	
			
	  	++g_pieceCount;
	}
		
	public Piece(PieceType type) : this()
	{
		this.m_type = type;
	}
		
	/**
	 * Copy constructor
	 *
	 * Does not copy neighbors, selectedness, offsets and IDs.
	 */
	public Piece(Piece p) : this(p.m_type)
	{
		this.m_position = p.m_position;
		this.m_align = p.m_align;
		this.m_axis = p.m_axis;
		this.m_length = p.m_length;
		this.m_color = p.m_color;
		this.m_squeezing = p.m_squeezing;
		this.m_squeezeDirection = Direction.INVALID; 
	}
			
//	~Piece()
//	{
//		if (m_cs != null)
//		{
//			// This will crash in Unity because destructor is not called from main thread
//			UninitVisual();
//		}
//	}
		
	public override string ToString()
	{
		string aligned = this.m_align ? "(aligned)" : "(unaligned)";
		return this.m_type.ToString() + 
				" at " + this.m_position.ToString() + aligned +
				" towards " + this.m_axis.ToString() +
				" length " + this.m_length + " .";
	}
		
	public virtual void InitVisual()
	{		
		m_cs = PrefabHubCS.Instance.GetPiece(m_type);
			
		// store a reference to the related Piece in the visual (for picking)
		m_cs.piece = this;
	
		// Tuck away model from scene tree root level
		m_cs.transform.parent = PiecesRoot;
			
		// tweak color aso
		TweakVisual();
	}
		
	public virtual void UninitVisual()
	{
		if (m_cs != null){
			GameObject.Destroy(m_cs.gameObject);
			m_cs = null;
		}
	}
		
	public virtual void InitPhysics()
	{
		UpdatePhysics();
	}
		
	// Update logic state (whatsoever)
	public virtual void Update()
	{
	}
			
	/**
	*	Update visible representation of this piece
	*/
	public virtual void UpdateVisual(float offset)
	{
	}
		
	/**
	*	Update physics representation of this piece
	*/
	public virtual void UpdatePhysics()
	{
	}
		
	protected abstract void TweakVisual();
		
	public virtual void Dispose()
	{
	  --g_pieceCount;
	}
		
	public virtual bool Intersect(KeyValuePair<Vector3, Vector3> box)
	{
	  	KeyValuePair<Vector3, Vector3> myBox = GetRealCollisionBox();
		Vector3 interMin;
		Vector3 interMax;
		interMin = Vector3.Max(box.Key, myBox.Key);
		interMax = Vector3.Min(box.Value, myBox.Value);

		for (byte c = 0; c < 3; ++c){
			if (interMin[c] >= interMax[c]){
		  		return false;
			}
		}
	  return true;
	}
		
//	public virtual bool Intersect(ref float len, Ray ray)
//	{
//	  Vector3 p1 = new Vector3(m_position);
//	  if (m_align)
//		p1 += new Vector3(0.5f, 0.5f, -0.5f);
//	  Vector3 p2 = new Vector3(p1);
//
//	  switch (m_axis)
//	  {
//		case Axis.X:
//		  p2 += Vector3(m_length, 1, 1);
//		  break;
//		case Axis.Y:
//		  p2 += Vector3(1, m_length, 1);
//		  break;
//		case Axis.Z:
//		  p2 += Vector3(1, 1, m_length);
//		  break;
//		default:
//		  Debug.Assert(false);
//		  break;
//	  }
//
//	  return tools.Intersect(len, ray, p1, p2);
//	}
		
	public void ResetCollision()
	{
		m_collPoints.Clear();
		m_colliders.Clear();
	}
		
	public virtual bool CheckCollision(Piece piece, float distMin)
	{
		return CheckCollision(piece, distMin, true);
	}
		
	public virtual bool CheckCollision(Piece piece, float distMin, bool setPoints)
	{
		if (CheckCollisionFast(piece, distMin)) {
			if (CheckCollisionSlow(piece, distMin, setPoints)){
				return true;
			}
		}
		return false;
	}
			
	/**
	 * 	Cheap & coarse test if pieces could POSSIBLY overlap
	 * 	at a minimum allowed distance of distMin. 
	 * */ 
	protected virtual bool CheckCollisionFast(Piece other, float distMin)
	{
		VectorInt3 posOther = other.GetPosition();
		int lenOther = other.GetLength();
			
		VectorInt3 diffPositions = m_position - posOther;
		int distPositions = diffPositions.LengthAccum();
		float dist1 = ((float)distPositions) - (m_length - 1) - (lenOther-1) - 1.5f;
		if (dist1 > distMin + 1f) { return false; }
			
		return true;
	}
		
			
	/**
	 * 	Checks if this piece overlaps with argument piece 
	 * 	at a minimum allowed distance of distMin. Registers 
	 * 	collision points to global collection
	 * */ 
	public virtual bool CheckCollisionSlow(Piece other, float distMin, bool setPoints)
	{			
		// Identify minimum distance between any two faces of the pieces
		// Distance means grid unit steps along orthogonal axes (not Euklidian distance)
			
		// scale parameters by 2 (integer calculations)	
		int iDistMin = (int)Mathf.Ceil(distMin * 2);
		//
		bool alignOther = other.GetAlignment();
		VectorInt3 posOther = (other.GetPosition() * 2) + (alignOther ? new VectorInt3(Circuit.s_offsetSpaces * 2) : VectorInt3.zero);
		int lenOther = other.GetLength() * 2;
		Axis axisOther = other.GetAxis();	
		//
		VectorInt3 posThis = (m_position * 2) + (m_align ? new VectorInt3(Circuit.s_offsetSpaces * 2) : VectorInt3.zero);
		int lenThis = m_length * 2;
			
		// Discriminate by pieces lengths and orientations
		int dist;
		VectorInt3 posPerpThis = posThis;
		VectorInt3 posPerpOther = posOther;
		//
		if (lenThis == 2 && lenOther == 2) // 2 boxes
		{
			dist = (posThis - posOther).LengthAccum() - 2;
		} 
		else // at least one of them is not a box
		{
			if (m_axis == axisOther) { 
				int iAxis = (int)m_axis;
				int coordThis = UnityEngine.Mathf.Clamp(posOther[iAxis], posThis[iAxis], posThis[iAxis] + lenThis - 2);
				int coordOther = UnityEngine.Mathf.Clamp(coordThis, posOther[iAxis], posOther[iAxis] + lenOther - 2);
				posPerpThis[iAxis] = coordThis;
				posPerpOther[iAxis] = coordOther;
			} 
			else 
			{
				// find closes position on this piece
				int iAxisThis = (int)m_axis;
				if (lenThis > 2){
					int coordEndThis = posThis[iAxisThis] + (lenThis - 2);
					posPerpThis[(int)m_axis] = Mathf.Clamp(posOther[(int)iAxisThis], posThis[iAxisThis], coordEndThis);
				} 
				// find closes position on other piece
				int iAxisOther = (int)axisOther;
				if (lenOther > 2){
					int coordEndOther = posOther[iAxisOther] + (lenOther - 2);
					posPerpOther[(int)axisOther] = Mathf.Clamp(posThis[(int)iAxisOther], posOther[iAxisOther], coordEndOther);
				}
			}
			dist = (posPerpThis - posPerpOther).LengthAccum() - 2;
		}
		if (dist >= iDistMin) { 
			return false; 
		}
		
		// Pieces ARE within collision range. 
		// Now we rule out cases where is is still not considered a collision:
		// They might be (indirect) neighbors on the same block.
			
		// Consider alignment
		if (m_align != alignOther) // pieces have different alignment.
		{
			// They cant be on the same block, so there is a collision.
			System.Diagnostics.Debug.WriteLine("misaligned, there is a collision");
			if (setPoints){
		  		Collide(other, (posPerpThis / 2).ToVector3(), (posPerpOther / 2).ToVector3());
			}
			return true;
		}
		
		// Try to trace a (continuous) path from this piece to the other
		Direction[] dirs = Tools.GetDirectionSet(posPerpOther - posPerpThis); 
		if (FindPath(this, other, dirs)){
			return false;
		}
			
		// No path was found. The pieces are not (indirect) neighbors such as to 
		// ignore their proximity: The two pieces are in collision.
		System.Diagnostics.Debug.WriteLine("we have a collision");
		if (setPoints){
			Collide(other, (posPerpThis/2).ToVector3(), (posPerpOther/2).ToVector3());
		}
		return true;
	}
				
	protected bool FindPath(Piece iter, Piece other, Direction[] dirs)
	{
		foreach (int i in Enumerable.Range(0, 3)){
			Direction dir = dirs[i];
			if (dir == Direction.INVALID) {	
				continue;
			} else {
				Piece neig = iter.GetNeighbor(dir);
				if (neig == null){
					continue;	
				} else {
					if (neig == other){
						return true; // found a path
					} 
						
					Direction[] dirRecurse = new Direction[3]{dirs[0], dirs[1], dirs[2]};
						
					if (neig.GetPosition()[i] == other.GetPosition()[i]){
						dirRecurse[i] = Direction.INVALID;
					}
						
					if(FindPath(neig, other, dirRecurse))
					{
						return true;
					}
				}
			}
		}
		
		return false;
	}
		
					
	public void SetColor(Vector4 color)
	{
	  	m_color = color;
			
		if (m_cs != null){
			TweakVisual();
		}
	}

	public Vector4 GetColor()
	{
	  return m_color;
	}

	public void SetPosition(bool align, VectorInt3 pos)
	{
	  m_align = align;
	  m_position = pos;
	}
	public void SetPosition(VectorInt3 pos)
	{
	  m_position = pos;
	}
	public void Move(VectorInt3 v)
	{
	  m_position += v;
	}
	public void SetAxis(Axis axis)
	{
	  m_axis = axis;
	}
	public void SetLength(int length)
	{
	  	m_length = length;
			
		UpdatePhysics();
	}
	public void SetInjectionId(int id)
	{
	  	m_injectionId = id;
	}
		
	public void SetSelected()
	{
		SetSelected(true);
	}	
		
	public virtual void SetSelected(bool selected)
	{
		if (m_selected == selected){
			return;
		}
			
		// set color
		if (selected){
			SetColorVisual(Color.Lerp(m_color, Color.white, 0.7f));
			if (m_cs != null){
				m_cs.transform.parent = Selected;
			}
		} else {
			SetColorVisual(m_color);
			if (m_cs != null){
				m_cs.transform.parent = PiecesRoot;
			}
		}
			
		m_selected = selected;
			
	}
		
	public HashSet<uint> GetTags()
	{
	  return m_tags;
	}
		
	public void SetTags(HashSet<uint> hashset)
	{
	  	m_tags = hashset;
	}
		
	public void AddTags(IEnumerable<uint> e)
	{
		foreach (uint ui in e){
			m_tags.Add(ui);
		}
	}
	
	public void ReplaceTags(IEnumerable<uint> e)
	{
		m_tags = new HashSet<uint>(e);
	}
		
		

	public new PieceType GetType()
	{
	  return m_type;
	}

	public bool GetAlignment()
	{
	  return m_align;
	}


	public VectorInt3 GetPosition()
	{
	  return m_position;
	}

	public Axis GetAxis()
	{
	  return m_axis;
	}

	public int GetLength()
	{
	  return m_length;
	}

	public int GetInjectionId()
	{
	  return m_injectionId;
	}

	public bool IsSelected()
	{
	  return m_selected;
	}

	public bool IsColliding()
	{
	  return m_colliders.Count > 0;
	}
		
	public List<Piece> GetColliders()
	{
	  return m_colliders;
	}
		
	/**
	 * 	Returns the segment of the piece for crossing calculations
	 * */
	public Piece.Segment GetSegment(Axis pref = Axis.INVALID)
	{
		Axis axis;
		  if (m_type == PieceType.BOX && pref != Axis.INVALID)
		    axis = pref;
		  else
		    axis = m_axis;
		
		  VectorInt3 p1;
		  int length;
		  var dirs = Tools.GetDirectionsFromAxis(axis);
		
		  p1 = m_position * 4;
		  if (m_align){
		    	p1 += new VectorInt3(2);
			}
		  p1 += new VectorInt3(2);
		
		  VectorInt3 dirVec = Tools.GetVectorFromDirection(Tools.GetDirectionsFromAxis(axis).Value);
		
		  p1 -= dirVec * 1;
		  length = m_length * 4 - 2;
		
		  if (HasNeighbor(dirs.Key))
		  {
		    p1 -= dirVec * 1;
		    ++length;
		  }
		  if (HasNeighbor(dirs.Value))
		    ++length;
		
		  return new Piece.Segment(p1, dirVec, length);
	}

	public PieceDescriptor GetDescriptor()
	{
		PieceDescriptor pd = new PieceDescriptor();
		pd.type = m_type;
		pd.align = m_align;
		pd.position = m_position;
		pd.axis = m_axis;
		pd.length = m_length;
		pd.color = m_color;
		pd.tags = new HashSet<uint>(m_tags);
		
		return pd;
	}

	public void ClearNeighbors()
	{
		m_neighbors.Clear();
	}
		
	public void UnbindNeighbors()
	{
	  foreach (KeyValuePair<Direction, Piece> n in this)
	  {
		System.Diagnostics.Debug.Assert(n.Value.GetNeighbor(Tools.InvertDirection(n.Key)) == this);
		n.Value.SetNeighbor(Tools.InvertDirection(n.Key), null);
	  }
	  ClearNeighbors();
	}
		
	public void SetNeighbor(Direction direction, Piece piece)
	{
		if (piece == null){
			m_neighbors.Remove(direction);
		} else {
			m_neighbors[direction] = piece;
		}
	}
		
	public Piece GetNeighbor(Direction direction)
	{
		if (m_neighbors.ContainsKey(direction)){
			return m_neighbors[direction];
		} else {
			return null;	
		}
	}
		
	public Dictionary<Direction, Piece> GetNeighbors()
	{
		return this.m_neighbors;
	}


	public bool HasNeighbor(Direction direction)
	{
	  return m_neighbors.ContainsKey(direction);
	}
	

	public uint CountNeighbors()
	{
	  return (uint)m_neighbors.Count;
	}

	public void ResetOffsets()
	{
	  m_positionOff = Vector3.zero;
	  m_lengthOff = 0F;
	  m_squeezeDirection = Direction.INVALID;
	}
	public virtual void SetLengthOffset(float offset)
	{
	  m_lengthOff = offset;
	}
		
	public void SetPositionOffset(Vector3 offset)
	{
	  m_positionOff = offset;
	}

	public float GetLengthOffset()
	{
	  return m_lengthOff;
	}
		
	public void SetSqueezeDirection(Direction d)
	{
	  m_squeezeDirection = d;
	}

	/**
	 * Get the pieces's collision box (ignoring grid alignment)
	 * */
	public virtual KeyValuePair<VectorInt3, VectorInt3> GetCollisionBox()
	{
		System.Diagnostics.Debug.Assert(m_axis != Axis.INVALID);
			
	  	VectorInt3 posMin = m_position; 
		VectorInt3 posMax = m_position; 
		posMax[(int)m_axis] += m_length; 
		posMax[(((int)m_axis)+1)%3] += 1; 
		posMax[(((int)m_axis)+2)%3] += 1; 
		
	  	return new KeyValuePair<VectorInt3, VectorInt3>(posMin, posMax);
	}
		
	/**
	 * Get the piece's collision box taking grid alignment into account
	 * */
	public virtual KeyValuePair<Vector3, Vector3> GetRealCollisionBox()
	{
	  KeyValuePair<VectorInt3, VectorInt3> box = GetCollisionBox();
		Vector3 min = box.Key.ToVector3();
		Vector3 max = box.Value.ToVector3();
			
	  if (m_align)
	  {
		min += Circuit.s_offsetSpaces;
		max += Circuit.s_offsetSpaces;
	  }
		
	  	return new KeyValuePair<Vector3, Vector3>(min, max);		
	}

	public virtual bool CheckSqueezing()
	{
	  return false;
	}

	public void SetPathId(uint id)
	{
	  m_pathId = id;
	}

	public uint GetPathId()
	{
	  return m_pathId;
	}
		
	public void SetBlockId(uint id)
	{
	  m_blockId = id;
	}

	public uint GetBlockId()
	{
	  return m_blockId;
	}

//	public void Debug()
//	{
//		Debug.WriteLine("Debugging Piece, type: " + (int)m_type);
//  		foreach (var n in m_neighbors){
//			var p = n.second.lock();
//			if (p){
//				System.Diagnostics.Debug.WriteLine("dir: " << (int)n.first << ", type: " << (int)p->m_type);
//			}
//		}		
//	}

	public static uint GetPieceCount()
	{
	  return g_pieceCount;
	}

	protected void Collide(Piece piece, Vector3 p1, Vector3 p2)
	{
		m_collPoints.Add(p1);
		m_colliders.Add(piece);
		piece.m_collPoints.Add(p2);
		piece.m_colliders.Add(this);
	}
		
	public void UpdateShaderConstants(float offset)
	{
		if (m_cs == null){
			return;
		}
		Material m = m_cs.GetMaterial();
		if (m != null)
		{
			m.SetFloat("_fMoveOffset", offset);		
			
			int nPoints = m_collPoints.Count;
			if (nPoints <= 10){
				if (nPoints > 0){	
					int index = nPoints - 1;
					Vector4 cpView = m_collPoints[index];
					cpView.w = 1.0f;
					cpView = Camera.mainCamera.worldToCameraMatrix * cpView;
					string nameConstant = "_posColl" + index;
					m.SetVector(nameConstant, cpView);
				}
				m.SetFloat("_nCollPoints", (float)nPoints);		
			}
		}
	}
	
	protected void SetColorVisual(Color c)
	{
		if (m_cs != null){
			m_cs.SetColorVisual(c);
		}
	}
		
		
	//----------------------------------------------
		
	public System.Collections.IEnumerator GetEnumerator()
	{
		foreach (Direction dir in Enum.GetValues(typeof(Direction))){
			if (dir == Direction.INVALID) { continue; }
			if (m_neighbors.ContainsKey(dir)){
				yield return new KeyValuePair<Direction, Piece>(dir, m_neighbors[dir]);
			}
		}
	}
		
	public bool IsInsideFrustrum(ref Plane[] planes)
	{
		Bounds b = m_cs.gameObject.GetComponent<Collider>().bounds;
		bool insideOrIntersectingAll = GeometryUtility.TestPlanesAABB(planes, b);		
		return insideOrIntersectingAll;
	}
		
	public bool IsCompletelyInsideFrustrum(ref Plane[] planes)
	{
		Bounds b = m_cs.GetColliderMesh().bounds;
		Plane[] ps = new Plane[1];
		foreach (Plane p in planes){
			// invert plane 
			ps[0].distance = -p.distance;
			ps[0].normal = -p.normal;
			bool insideOrIntersecting = GeometryUtility.TestPlanesAABB(ps, b);	
			if (insideOrIntersecting) { 
				return false; 
			}
		}
		return true;
	}
	
}

}



