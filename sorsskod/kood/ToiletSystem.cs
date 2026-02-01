using Godot;
using System;

public partial class ToiletSystem : Node3D
{
	private Globals glob;
	private Node3D junnid;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		glob = GetNode<Globals>("/root/Globals");
		junnid = GetNode<Node3D>("junnid");
		glob.Connect("PlayerUsedToilet", new Callable(this, nameof(OnPlayerUsedToilet)));
		glob.Connect("clearToilet", new Callable(this, nameof(ClearToiletArea)));
		glob.Connect("RequestLabelText", new Callable(this, nameof(GetToiletText)));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnPlayerUsedToilet()
	{
		junnid.Visible = true;
	}

	private void ClearToiletArea()
	{
		junnid.Visible = false;
	}

	private void GetToiletText(string type)
	{
		if (type != "Toilet")
		{
			return;
		}
		if (junnid.Visible)
		{
			glob.EmitSignal("LabelTextResponse", "Toilet is full brah.");
			return;
		}
		glob.EmitSignal("LabelTextResponse", "Use the toilet.");
	}
}
