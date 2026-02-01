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
	[Export] Label PriceTag;
	[Export] Label BalanceLabel;

	enum MoveDirs { UP, DOWN, LEFT, RIGHT }

	private List<MoveDirs> currentDirs = new List<MoveDirs>();
	private bool inBenchMode = false;
	private bool canUseBench = false;
	private bool inConsoleMode = false;
	private bool canUseConsole = false;
	private bool canUsePassiveBench = false;
	private Globals glob;
	// sorri oleks ilusam viis teha ma ei viitsi
	private readonly MoveDirs[] protRecipe = { MoveDirs.UP, MoveDirs.UP, MoveDirs.DOWN, MoveDirs.RIGHT };
	private readonly MoveDirs[] ninjaRecipe = { MoveDirs.RIGHT, MoveDirs.UP, MoveDirs.DOWN, MoveDirs.DOWN };
	private readonly MoveDirs[] skinRecipe = { MoveDirs.UP, MoveDirs.LEFT, MoveDirs.DOWN, MoveDirs.RIGHT };
	private readonly MoveDirs[] gGlassesRecipe = { MoveDirs.DOWN, MoveDirs.UP, MoveDirs.UP, MoveDirs.LEFT };
	private readonly MoveDirs[] FestivalRecipe = { MoveDirs.LEFT, MoveDirs.DOWN, MoveDirs.DOWN, MoveDirs.RIGHT };

	public int moneyBalance = 500;

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
			PriceTag.Visible = false;
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

		if (collider.IsInGroup("passiveBench") && !holdingObj)
		{
			pupSprite.Visible = true;
			PriceTag.Visible = true;
			canUsePassiveBench = true;
			return;
		}
		else
		{
			canUsePassiveBench = false;
		}

		if (collider.IsInGroup("console"))
		{
			pupSprite.Visible = true;
			canUseConsole = true;
			return;
		}
		else
		{
			canUseConsole = false;
		}

		PriceTag.Visible = false;
		pupSprite.Visible = false;
	}

	private void HandlePassiveBenchInput(InputEvent e)
	{
		if (e is InputEventMouseButton)
		{
			if (e.IsActionPressed("lmb"))
			{
				glob.EmitSignal("PassiveBenchInteract", moneyBalance);
			}
		}
	}

	private void HandlePassiveBenchResponse(int balance, int newNeededMoney)
	{
		AddBalance(true, balance);
		if (newNeededMoney > 0)
		{
			PriceTag.Text = "Upgrade Cost: " + newNeededMoney.ToString() + " $";
		}
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
				DisableBenchMode();
			}
		}
	}

	private void AddBalance(bool isDone, int balance)
	{
		if (isDone)
		{
			moneyBalance += balance;
			BalanceLabel.Text = "Balance: " + moneyBalance.ToString() + " $";
		}
	}

	private void DisableBenchMode()
	{
		inBenchMode = false;
		benchCam.Current = false;
		keysParent.Visible = false;
		lmbParent.Visible = true;
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
		bool isProtMask = currentDirs.SequenceEqual(protRecipe);
		bool isNinjaMask = currentDirs.SequenceEqual(ninjaRecipe);
		bool isSkinMask = currentDirs.SequenceEqual(skinRecipe);
		bool isGlassesMask = currentDirs.SequenceEqual(gGlassesRecipe);
		bool isFestivalMask = currentDirs.SequenceEqual(FestivalRecipe);


		if (isProtMask){
			glob.EmitSignal("SpawnItem", "Protective Mask");
			DisableBenchMode();
		}
		if (isNinjaMask){
			glob.EmitSignal("SpawnItem", "Ninja Mask");
			DisableBenchMode();
		}
		if (isSkinMask){
			glob.EmitSignal("SpawnItem", "Human Skin Mask");
			DisableBenchMode();
		}
		if (isGlassesMask){
			glob.EmitSignal("SpawnItem", "Blinding Mask");
			DisableBenchMode();
		}
		if (isFestivalMask){
			glob.EmitSignal("SpawnItem", "Festival Mask");
			DisableBenchMode();
		}
		

		await Task.Delay(1000);
		currentDirs.Clear();
		RefreshInputUi();
	}


	private void HandleConsoleInput(InputEvent e)
	{
		if (e is InputEventKey eventKey && eventKey.Keycode == Key.Enter)
		{
			reg.SubmitOrder(eventKey);
			return;
		}

		if (e is InputEventKey)
		{
			reg.UseKeyInput((InputEventKey)e);
		}

		if (e is InputEventMouseButton)
		{

			if (e.IsActionPressed("lmb"))
			{
				inConsoleMode = false;
				cashCam.Current = false;
				lmbParent.Visible = true;
			}
		}

	}

}
