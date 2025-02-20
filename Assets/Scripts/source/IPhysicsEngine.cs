using System.Collections.Generic;

using pcs;
using tools;
using operation;


// the following 4 aliases might be turned into actual classes that ensure respective criteria are met
using Strand = System.Collections.Generic.ICollection<VoxelStrip>; // TODO ensure pieces actually do form a strand
using Path = System.Collections.Generic.ICollection<VoxelStrip>; // TODO ensure pieces actually do form a path
using Block = System.Collections.Generic.ICollection<VoxelStrip>; // TODO ensure pieces actually do form a block

using Border = System.Collections.Generic.KeyValuePair<Voxel, tools.Direction>; // specifies a q-bit and which side of it belongs to the selection. alignment is omitted as this should be clear 



public struct Voxel
{
	public VectorInt3 p;
	public byte halfspace;
}

/// <summary>
/// Specifies a continguent set of voxels that 
/// vary in only one coordinate, thus forming a strip.
/// </summary>
public struct VoxelStrip
{
	public Voxel posBegin;
	public Axis axis;
	public ushort length;
}

public class SelectionVoxel : HashSet<Voxel>{} 
public class SelectionStrip : HashSet<VoxelStrip>{}



public enum EDefect
{
	Tube,
	Injector,
	Cap
}

public enum EManip
{
	Spawn,
	Delete, 
	Offset,
}

public struct Manipulation
{
	public EManip manip;
	public VoxelStrip locus;
	public EDefect defect;
	public VectorInt3 offset;
}




/// <summary>
/// </summary>
public struct Range 
{
	public int min; 
	public int max;
}

/// <summary>
/// </summary>
public enum EAngle
{
	Deg0,
	Deg90,
	Deg180,
	Deg270
};

/// <summary>
/// 
/// Formulates API for the meQuanics physics engine to be used by the meQuanics game client. 
/// Author: Klaus Bruegmann
/// 
/// </summary>
public interface IPhysicsEngine
{
	void Clear(); // Clears all voxels (meaning: removes all defect strands)
	
	// for initial circuit construction
	
//	bool CreateVoxel(VectorInt3 position2);
	bool CreateVoxel(Voxel voxel);
//	bool CreateVoxelStrand(VectorInt3 position2, Axis axis, int length);
	bool CreateVoxelStrip(VoxelStrip strip);	
//	bool CreateInjector(VectorInt3 position2, Axis axis);
	bool CreateInjector(Voxel voxel, Axis axis);
//	bool CreateFakeCap(VectorInt3 position2, Axis axis);
	bool CreateFakeCap(Voxel voxel, Axis axis);
	
	// for manipulating via UI
	
	// move pieces
	List<Range> GetPermissibleOffsets(SelectionStrip selection, Axis axis);
	List<Range> GetPermissibleOffsets(SelectionVoxel selection, Axis axis);
//	bool IsPermissibleOffset(SelectionStrip selection, Direction direction); // this is in case GetPermissibleOffsets feels like overkill ..
//	bool IsPermissibleOffset(SelectionVoxel selection, Direction direction); // this is in case GetPermissibleOffsets feels like overkill ..
	List<Manipulation> ApplyOffset(SelectionStrip selection, Axis axis, int offset);
	List<Manipulation> ApplyOffset(SelectionVoxel selection, Axis axis, int offset);
	
	// move pieces allowing for crossing // maybe this is obsolete if we have switching
	List<Range> GetPermissibleOffsetsCrossing(SelectionStrip selection, Axis axis); // this feels quite expensive
//	List<Range> GetPermissibleOffsetsCrossing(SelectionVoxel selection, Axis axis); // this feels quite expensive
	List<Manipulation> ApplyOffsetCrossing(SelectionStrip selection, Axis axis, int offset);
//	List<Manipulation> ApplyOffsetCrossing(SelectionVoxel selection, Axis axis, int offset);
	
	// rotate a set of pieces by a multiple of 90 degree // not implemented in current client
	bool IsPermissibleRotation(SelectionStrip selection, Axis axis, EAngle angle, VectorInt3 center);
//	bool IsPermissibleRotation(SelectionVoxel selection, Axis axis, EAngle angle, VectorInt3 center);
	List<Manipulation> ApplyRotate(SelectionStrip selection, Axis axis, EAngle angle, VectorInt3 center);
//	List<Manipulation> ApplyRotate(SelectionVoxel selection, Axis axis, EAngle angle, VectorInt3 center);
	
	// bridge
	List<Manipulation> ApplyBridge(Voxel pos1, Voxel pos2);
//	bool IsPermissibleBridge(Voxel pos1, Voxel pos2);
	
	// move injector
//	List<VoxelStrip> GetPermissibleOffsetsInjectorOnStrand(VoxelStrip injector);
	List<VoxelStrip> GetPermissibleOffsetsInjectorOnStrand(Voxel injector);
//	List<Manipulation> ApplyOffsetInjectorOnStrand(VoxelStrip injector, VoxelStrip destination);
	List<Manipulation> ApplyOffsetInjectorOnStrand(Voxel injector, VoxelStrip destination);
	
	// teleport
//	bool IsPermissibleTeleport(VoxelStrip injector);
	bool IsPermissibleTeleport(Voxel injector);
//	List<Manipulation> ApplyTeleport(VoxelStrip injector);
	List<Manipulation> ApplyTeleport(Voxel injector);
	
	// duplicate-move
	List<Range> GetPermissibleOffsetsDuplicate(SelectionStrip selection, Axis axis);
//	List<Range> GetPermissibleOffsetsDuplicate(SelectionVoxel selection, Axis axis);
	List<Manipulation> ApplyOffsetDuplicate(SelectionStrip selection, Axis axis, int offset);
//	List<Manipulation> ApplyOffsetDuplicate(SelectionVoxel selection, Axis axis, int offset);
	
	// switch
//	bool IsPermissibleSwitch(Strand noose, Axis axis);
//	bool IsPermissibleSwitch(VectorInt3 nooseBegin, VectorInt3 nooseEnd, Axis axis);
//	List<Manipulation> ApplySwitch(Strand noose, Axis axis);
//	List<Manipulation> ApplySwitch(VectorInt3 nooseBegin, VectorInt3 nooseEnd, Axis axis);
	
	// rotate injector
//	bool IsPermissibleRotateInjector(VoxelStrip injector, Voxel box);
	bool IsPermissibleRotateInjector(Voxel injector, Voxel box);
//	List<Manipulation> ApplyRotateInjector(VoxelStrip injector, Voxel box);
	List<Manipulation> ApplyRotateInjector(Voxel injector, Voxel box);
	
	//  for undoing/redoing steps
	
	bool ApplyManipulation(Manipulation man);
	bool ApplyManipulations(List<Manipulation> mans);
	bool ApplyManipulationInverse(Manipulation man);
	bool ApplyManipulationsInverse(List<Manipulation> mans);
	
	// Topological inquiry

	Path GetPathConnecting(Voxel voxel1, Voxel voxel2);
	
	Block GetBlock(Voxel voxel);
	Strand GetStrand(Voxel voxel);
	VoxelStrip GetStrip(Voxel voxel);
	
	List<Border> GetBorder(SelectionStrip selection);
//	List<Border> GetBorder(SelectionVoxel selection);
	
	// stuff contemplated for future development cycles
	
//	List<Manipulation> StraightenStrand(Strand strand); // optimize the strand in terms of length ("rubberband").
//	Strand FindRouteConnecting(Voxel voxel1, Voxel voxel2, List<VoxelStrip> ignoredStrips); // with 'Route' meaning free space to create a new strand.
	
}