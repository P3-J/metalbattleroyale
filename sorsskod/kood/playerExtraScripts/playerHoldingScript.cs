using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Threading.Tasks;

public partial class Player
{
	[Export] Sprite2D pupSprite;
	[Export] Camera3D benchCam;
	[Export] Sprite2D[] uiSlots;
	[Export] Node2D lmbParent;
	[Export] Node2D keysParent;

	enum MoveDirs { UP, DOWN, LEFT, RIGHT }

	private List<MoveDirs> currentDirs = new List<MoveDirs>();
	private bool inBenchMode = false;
	private bool canUseBench = false;

	private readonly MoveDirs[] secretRecipe = { MoveDirs.UP, MoveDirs.UP, MoveDirs.DOWN, MoveDirs.RIGHT };

	private void CheckHandCollisionAndHoldItem()
	{
		if (!tryingToHoldItem)
		{
			objInHand = null;
			holdingObj = false;
		}

		if (holdingObj && objInHand != null && tryingToHoldItem)
		{
			Vector3 targetPosition = handMarker.GlobalPosition;
			Vector3 startingPosition = objInHand.GlobalPosition;
			int STR = 5; // crank that soulja
			objInHand.LinearVelocity = (targetPosition - startingPosition) * STR;
			pupSprite.Visible = false;
			return;
		}
		if (!handRay.IsColliding())
		{
			pupSprite.Visible = false;
			canUseBench = false;
			return;
		}
		Node3D collider = (Node3D)handRay.GetCollider();
		if (collider.IsInGroup("moveableObject"))
		{
			pupSprite.Visible = true;
			if (!tryingToHoldItem) return;
			RigidBody3D obj = (RigidBody3D)collider;
			holdingObj = true;
			objInHand = obj;
			return;
		}

		if (collider.IsInGroup("craftingBench") && !holdingObj)
		{
			pupSprite.Visible = true;
			canUseBench = true;
			return;
		}
		else
		{
			canUseBench = false;
		}

		pupSprite.Visible = false;
	}

	private void HandleBenchInput(InputEvent e)
	{
		// Only trigger on "Pressed", not released
		if (e is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (e.IsActionPressed("ui_up")) AddInput(MoveDirs.UP);
			else if (e.IsActionPressed("ui_down")) AddInput(MoveDirs.DOWN);
			else if (e.IsActionPressed("ui_left")) AddInput(MoveDirs.LEFT);
			else if (e.IsActionPressed("ui_right")) AddInput(MoveDirs.RIGHT);
		}

		if (e is InputEventMouseButton)
		{
			if (e.IsActionPressed("lmb"))
			{

				inBenchMode = false;
				benchCam.Current = false;
				keysParent.Visible = false;
				lmbParent.Visible = true;
			}
		}

	}

	private void AddInput(MoveDirs dir)
	{
		if (currentDirs.Count < 4)
		{
			currentDirs.Add(dir);
			RefreshInputUi();
		}

		if (currentDirs.Count == 4)
		{
			ConfirmCombination();
		}
	}

	private void RefreshInputUi()
	{
		foreach (var slot in uiSlots)
		{
			slot.Visible = false;
			slot.GlobalRotationDegrees = 0;
		}
		for (int i = 0; i < currentDirs.Count; i++)
		{
			uiSlots[i].Visible = true;

			switch (currentDirs[i])
			{
				case MoveDirs.UP:
					uiSlots[i].GlobalRotationDegrees = 0;
					break;
				case MoveDirs.DOWN:
					uiSlots[i].GlobalRotationDegrees = 180;
					break;
				case MoveDirs.LEFT:
					uiSlots[i].GlobalRotationDegrees = 270;
					break;
				case MoveDirs.RIGHT:
					uiSlots[i].GlobalRotationDegrees = 90;
					break;
			}
		}
	}

	private async void ConfirmCombination()
	{
		bool isCorrect = currentDirs.SequenceEqual(secretRecipe);

		if (isCorrect)
		{
			GD.Print("Success!");
		}
		else
		{
			GD.Print("Failed!");
		}

		await Task.Delay(1000);
    	currentDirs.Clear();

		currentDirs.Clear();
		RefreshInputUi();
	}
}