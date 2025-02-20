


public class LocaleENG
{
	
	public static string[] LocStrings = new string[]
	{
		"Back",
		"Cancel",
		"Ok",
		"Quit",
		"Start",
		
		"Sorry, but...", "Another action is currently in progress",
		
		"Sorry, but...", "We've got nothing to undo right now.",
		
		"Switching...", "Select only the tube you want to perform the Switch on.",
		"Switching...", "You cannot do it on that axis here.",
		"Switching...", "Switching is impossible this way",
		
		"Injector Rotation:", "Select an Injector or Cap and the box you want to rotate it to.",
		"Injector Rotation:", "The target box is not placed properly.",
		"Injector Rotation:", "There is no proper path to your target box.",
		"Injector Rotation:", "This rotation would result in a collision.",
		
		"Injector Relocation:", "Select only the Injector or Cap you want to relocate.",
		
		"Teleportation:", "Select just the Injector or Cap you want to teleport.",
		"Teleportation:", "Your Injector or Cap needs to be on a simple loop",
		"Teleportation:", "Only a single tube may pass through the Injector loop",
		"Teleportation:", "  ...there's not enough space on the tube passing through that loop.",
		
		"Bridging:", "Select only the two boxes you want connected.",
		"Bridging:", "Select boxes of the same alignment.",
		"Bridging:", "Each box needs to be on a loop",
		"Bridging:", "Bridging only works for boxes that are not linked yet.",
		"Bridging:", "Bridging those two boxes would cause a collision", 
		
		"Sorry, but...", "An unknown step type occurred",
		
		"Sorry, but...", "You have no data saved to this slot yet.",
		
		"EULA",
		"Tutorial",
		
		"Main Menu",
		"Select a circuit",			
		"Press F1 for Help",
		"Undo",
		"Screenshot",
		"Simplify",
		"Split Tube",
		"Teleport",
		"Rotate Injector",
		"Bridge",
		"Injector Move",
		"Show Help",
		"Score",
		"Key Bindings",
		// the help string
#if ! UNITY_ANDROID && ! UNITY_IPHONE
		"'Z': Partition tubes\n" +
			"'X': Simplify selection\n" +
			"'S': Teleport Injector\n" +	
			"'V': Bridge Boxes\n" +	
			"'C': Rotate Injector\n" +	
			"'A': Move Injector Mode\n" +	
			"'U': Undo\n" +
#endif
			"'Ctrl': Add-Select Mode\n" +	
			"'Alt': Switching Mode\n" +	
//			"'Ctrl+Alt': Crossing Mode\n" +	
			"'Shift': Move Camera Mode\n" +	
#if ! UNITY_ANDROID && ! UNITY_IPHONE
//			"'IOKL,.': Choose Time Direction\n" +
//			"'TYGHBN': Choose Camera Angle\n" +
//			"'J': Toggle Isometric View\n" +
			"'Ctrl + NUM': Save progress\n" +
			"'NUM': Load progress" +
			"\n" +
#endif
			"",
		
		"It doesn't work like that", 
		
		"Play Tutorial"
	};
}