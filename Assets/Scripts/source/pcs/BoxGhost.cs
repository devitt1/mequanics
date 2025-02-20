using UnityEngine;

using tools;

namespace pcs
{

/// \todo boxes should return special segments on GetSegment
public class BoxGhost : Box
{
	public BoxGhost() : base(PieceType.BOXGHOST)
	{
	}
		
	protected override void TweakVisual()
	{	
		// Tweak renderer material color to render polygons semi-transparent and darker
		SetColorVisual(new Vector4(0.8f, 0.8f, 0.8f, 0.5f));
	}
		
}

}



