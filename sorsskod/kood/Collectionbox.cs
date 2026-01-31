using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Collectionbox : Area3D
{
	
	public List<ActObj> Bodies = new();
	Globals glob;

	public override void _Ready()
	{
		glob = GetNode<Globals>("/root/Globals");
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

	private void OnBodyEntered(Node body)
	{
		if (body is ActObj rb && body.IsInGroup("moveableObject"))
		{
			Bodies.Add(rb);
			GD.Print("added bodies");
			TriggerAdd();
		}
	}

	private void OnBodyExited(Node body)
	{
		if (body is ActObj rb && body.IsInGroup("moveableObject"))
		{
			Bodies.Remove(rb);
			TriggerRemoval();
		}
	}

	

	private void TriggerAdd()
	{
		
		glob.EmitSignal("SendItemsToRegister", CollectNames());
	   

	}

	private void TriggerRemoval()
	{
		
		glob.EmitSignal("SendItemsToRegister", CollectNames());

	}

	private string[] CollectNames(){
		 return [.. Bodies
		.Select(e => e.GetNameOfMask())];
	}


}
