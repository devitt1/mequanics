public static class GlobalMembersHelp
{
//	static string helpstring = 
//#if UNITY_ANDROID || UNITY_IPHONE
//		" 'Ctrl': Add-Select Mode\n" +	
//		" 'Alt': Duplicate Mode\n" +	
//		" 'Ctrl+Alt': Crossing Mode\n" +	
//		" 'Shift': Move Camera Mode\n";	
//#else
//		" 'Z': Partition tubes\n" +
//		" 'X': Simplify selection\n" +
//		" 'S': Teleport Injector\n" +	
//		" 'V': Bridge Boxes\n" +	
//		" 'C': Rotate Injector\n" +	
//		" 'A': Injector Move Mode\n" +	
//		" \n" +	
//		" 'U': Undo\n" +
//		" 'Ctrl': Add-Select Mode\n" +	
//		" 'Alt': Duplicate Mode\n" +	
//		" 'Ctrl+Alt': Crossing Mode\n" +	
//		" 'Shift': Move Camera Mode\n" +	
//		" \n" +	
//		" 'IOKL,.': Choose Time Direction\n" +
//		" 'TYGHBN': Choose Camera Angle\n" +
//		" 'J': Toggle Isometric View\n" +
//		" \n" +		
//		" 'Ctrl + NUM': Save circuit\n" +
//		" 'NUM': Load previously saved circuit\n";
//#endif
	
	public static string GetHelpString()
	{
		return Locale.Str(PID.HelpString);
	}

}




