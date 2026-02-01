using Godot;
using System;

public partial class ActObj : RigidBody3D
{
	
	[Export] MeshInstance3D f1;
	[Export] MeshInstance3D f2;
	[Export] MeshInstance3D f3;
	[Export] MeshInstance3D f4;
	[Export] MeshInstance3D f5;


	string[] masks = ["Protective Mask", "Ninja Mask", "Festival Mask", "Human Skin Mask", "Blinding Mask"];
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
			case "Festival Mask":
				f3.Visible = true;
				thisMask = "Festival Mask";
				break;
			case "Human Skin Mask":
				f4.Visible = true;
				thisMask = "Human Skin Mask";
				break;
			case "Blinding Mask":
				f5.Visible = true;
				thisMask = "Blinding Mask";
				break;

		}
	}


	public string GetNameOfMask()
	{
		return thisMask;
	}


}
