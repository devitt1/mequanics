using UnityEngine;
using System.Collections.Generic;

using tools;
using pcs;


/**
 * Defines the interface by which class Game should be talked to.
 * */
public interface IGame
{
	// Inquire game state
	//
	
	float GetScore();
	float[] GetAchievementThresholds();
	
//	int GetUserID();
	string GetCircuitID();
	
	// Change game state
	//
	
	bool MovePieces(List<Piece> pieces, Axis axis, int distance);
	bool Duplicate(Piece tube, Axis axis, int distance);
	bool Bridge(Piece p0, Piece p1);
	bool Teleport(Piece injector);
	bool Simplify(List<Piece> pieces);
	bool Subdevide(Piece tube, Vector3 posSplit);
	
	bool SetTimeDirection(Direction direction);
}
