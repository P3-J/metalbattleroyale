using Godot;
using System;

public partial class Workbench : Node3D
{

	private Globals glob;
	[Export] PackedScene Mask;
	[Export] Marker3D spawnPoint;
	public override void _Ready()
	{
		base._Ready();
		glob = GetNode<Globals>("/root/Globals");

		glob.Connect("SpawnItem", new Callable(this, nameof(SpawnItem)));
	}

	public void SpawnItem(string itemName)
	{

		switch (itemName)
		{

			case "Protective Mask":
				ActObj fireM = Mask.Instantiate<ActObj>();
				fireM.MakeMask("Protective Mask");
				GetParent().AddChild(fireM);
				fireM.GlobalPosition = spawnPoint.GlobalPosition;
				break;
			case "Ninja Mask":
				ActObj iceM = Mask.Instantiate<ActObj>();
				iceM.MakeMask("Ninja Mask");
				GetParent().AddChild(iceM);
				iceM.GlobalPosition = spawnPoint.GlobalPosition;
				break;
			case "Human Skin Mask":
				ActObj iceeM = Mask.Instantiate<ActObj>();
				iceeM.MakeMask("Human Skin Mask");
				GetParent().AddChild(iceeM);
				iceeM.GlobalPosition = spawnPoint.GlobalPosition;
				break;
			case "Blinding Mask":
				ActObj iceeeM = Mask.Instantiate<ActObj>();
				iceeeM.MakeMask("Blinding Mask");
				GetParent().AddChild(iceeeM);
				iceeeM.GlobalPosition = spawnPoint.GlobalPosition;
				break;
			case "Festival Mask":
				ActObj iceeeeM = Mask.Instantiate<ActObj>();
				iceeeeM.MakeMask("Festival Mask");
				GetParent().AddChild(iceeeeM);
				iceeeeM.GlobalPosition = spawnPoint.GlobalPosition;
				break;

		}


	}

}
