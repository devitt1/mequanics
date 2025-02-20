using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/**
 * 
 * */
public class CapCS : InjectorCS
{	
	
	public override void SetColorVisual(Color c)
	{
		GetMaterial().color = c;
	}
}


