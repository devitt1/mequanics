using UnityEngine;

public static class GlobalMembersCap
{

	//bool Intersect(ref float len, Ray ray, Vector3 p1, Vector3 p2);
}

namespace pcs
{

public class Cap : Injector
{
	public Cap() : base(PieceType.CAP)
	{
		m_length = 4;
	}
		
	protected override void TweakVisual()
	{	
		// Tweak renderer material color to render polygons semi-transparent
		this.m_color.w = 0.3f;
		
		SetColorVisual(m_color);
	}
		
}

}
	








