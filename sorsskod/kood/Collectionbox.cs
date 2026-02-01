using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Collectionbox : Area3D
{
	public List<ActObj> Bodies = new();
	Globals glob;

	public override void _Ready()
	{
		glob = GetNode<Globals>("/root/Globals");
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		glob.Connect("OrderDone", new Callable(this, nameof(DeleteFulfilledItems)));
		glob.Connect("OrderIn", new Callable(this, nameof(TriggerUpdate)));
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

	private void TriggerUpdate()
	{
		Godot.Collections.Array<Node3D> overlappingBodies = GetOverlappingBodies();

		IEnumerable<ActObj> filteredBodies = overlappingBodies.OfType<ActObj>();

		Bodies = [];

		foreach (ActObj actObj in filteredBodies)
		{
			Bodies.Add(actObj);
		}
		glob.EmitSignal("SendItemsToRegister", CollectNames());
	}

	private string[] CollectNames()
	{
		return [.. Bodies.Select(e => e.GetNameOfMask())];
	}

	private void DeleteFulfilledItems(
		bool isFulfilled,
		int orderValue = 0,
		string[] fulfilledMasks = null
	)
	{
		foreach (string maskName in fulfilledMasks)
		{
			int idx = Bodies.FindIndex(body => body.GetNameOfMask() == maskName);

			if (idx != -1)
			{
				Bodies[idx].QueueFree();
				Bodies.RemoveAt(idx);
			}
		}
	}
}
