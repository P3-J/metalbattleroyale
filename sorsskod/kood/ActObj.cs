using Godot;
using System;

public partial class ActObj : RigidBody3D
{
	
	[Export] MeshInstance3D f1;
	[Export] MeshInstance3D f2;

	string[] masks = ["Protective Mask", "Ninja Mask"];
	string thisMask = "Protective Mask";


	public override void _Ready()
	{
		base._Ready();
	}


	public void MakeMask(string masktype)
	{
		

		switch (masktype)
		{
			
			case "Protective Mask":
				f1.Visible = true;
				thisMask = "Protective Mask";
				break;
			case "Ninja Mask":
				f2.Visible = true;
				thisMask = "Ninja Mask";
				break;

		}
	}


	public string GetNameOfMask()
	{
		return thisMask;
	}


}
