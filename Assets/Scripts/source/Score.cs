using System.Collections.Generic;

using tools;



public static class Score
{
	public static int ScoreFromSize(VectorInt3 size)
	{
		return (int)size.LengthAccum();
	}
	
	public static VectorInt3 SizeFromScore(int score)
	{
		int lenEdge = score/3;
		return new VectorInt3(lenEdge);
	}
	
	
	
	/**
	 * Get the cross-section score
	 */
	public static int GetCrossSectionScore(Circuit circuit)
	{
		var box = circuit.GetBoundingBox();
		VectorInt3 size = new VectorInt3(box.Value - box.Key);
		List<int> v = new List<int>() {size.x, size.y, size.z};
		if (circuit.m_timeLocked){
			v.RemoveAt(Tools.GetIntFromAxis(Tools.GetAxisFromDirection(circuit.m_timeDir)));
		} else {
			v.Sort();
		}
		return v[0] * v[1];
	}
}
