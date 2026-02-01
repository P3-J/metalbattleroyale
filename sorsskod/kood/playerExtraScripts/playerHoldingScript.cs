using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class Player
{
	[Export] Sprite2D pupSprite;
	[Export] Sprite2D eKeySprite;
	[Export] Camera3D benchCam;
	[Export] Sprite2D[] uiSlots;
	[Export] Node2D lmbParent;
	[Export] Node2D keysParent;
	[Export] Label BalanceLabel;

	enum MoveDirs { UP, DOWN, LEFT, RIGHT }

	private List<MoveDirs> currentDirs = new List<MoveDirs>();
	private bool inBenchMode = false;
	private bool canUseBench = false;
	private bool inConsoleMode = false;
	private bool canUseConsole = false;
	private bool canUsePassiveBench = false;
	private bool canUseToilet = false;
	private bool usedToilet = false;
	private bool inToiletMode = false;
	private bool canClearToilet = false;
	private Vector3 resetPos = new Vector3(0, 1, 0);
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
			eKeySprite.Visible = false;
			infoTag.Visible = false;
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

		if (collider.IsInGroup("clearToilet"))
		{
			eKeySprite.Visible = true;
			canClearToilet = true;
			infoTag.Text = "Clear Toilet";
			infoTag.Visible = true;
			return;
		}
		else
		{
			canClearToilet = false;
		}

		if (collider.IsInGroup("toilet"))
		{
			eKeySprite.Visible = true;
			canUseToilet = true;
			if (infoTag.Visible == false)
				{
					glob.EmitSignal("RequestLabelText", "Toilet");
				}
			infoTag.Visible = true;
			return;
		}
		else {
			canUseToilet = false;
		}

		if (collider.IsInGroup("craftingBench") && !holdingObj)
		{
			eKeySprite.Visible = true;
			canUseBench = true;
			return;
		}
		else
		{
			canUseBench = false;
		}

		if (collider.IsInGroup("passiveBench") && !holdingObj)
		{
			eKeySprite.Visible = true;
			if (infoTag.Visible == false)
				{
					glob.EmitSignal("RequestLabelText", "PassiveBench");
				}
			infoTag.Visible = true;
			canUsePassiveBench = true;
			return;
		}
		else
		{
			canUsePassiveBench = false;
		}
		if (collider.IsInGroup("passiveBench") && !holdingObj)
		{
			pupSprite.Visible = true;
			//InfoTag.Visible = true;
			if (infoTag.Visible == false)
			{
				glob.EmitSignal("RequestLabelText", "PassiveBench");
			}
			infoTag.Visible = true;
			canUsePassiveBench = true;
			return;
		}
		else
		{
			canUsePassiveBench = false;
		}

		if (collider.IsInGroup("console"))
		{
			eKeySprite.Visible = true;
			canUseConsole = true;
			return;
		}
		else
		{
			canUseConsole = false;
		}

		eKeySprite.Visible = false;
		pupSprite.Visible = false;
		infoTag.Visible = false;
	}

	private void HandlePassiveBenchInput(InputEvent e)
	{
		if (e is not InputEventKey keyEvent)
		{
			return;
		}

		if (keyEvent.Keycode == Key.E && keyEvent.Pressed)
		{
			glob.EmitSignal("PassiveBenchInteract", moneyBalance);
		}
	}

	private void HandleClearToiletInput(InputEvent e)
	{
		if (e.IsActionPressed("interact"))
		{
			glob.EmitSignal("clearToilet");
			usedToilet = false;
			return;
		}
	}

	private void HandleToiletInput(InputEvent e)
	{
		if (e.IsActionPressed("interact"))
		{
			inToiletMode = true;
			resetPos = GlobalPosition;
			GlobalPosition = new Vector3(4.233f, 1.189f, 8.355f); // toilet location
			toiletLabel.Visible = true;
			usedToilet = true;
			return;
		}
	}

	private void HandlePassiveBenchResponse(int balance, int newNeededMoney)
	{
		AddBalance(true, balance);
	}

	private void HandleBenchInput(InputEvent e)
	{
		if (e is not InputEventKey keyEvent)
		{
			return;
		}

		// Only trigger on "Pressed", not released
		if (keyEvent.Pressed)
		{
			if (e.IsActionPressed("ui_up"))
				AddInput(MoveDirs.UP);
			else if (e.IsActionPressed("ui_down"))
				AddInput(MoveDirs.DOWN);
			else if (e.IsActionPressed("ui_left"))
				AddInput(MoveDirs.LEFT);
			else if (e.IsActionPressed("ui_right"))
				AddInput(MoveDirs.RIGHT);
			else if (keyEvent.Keycode == Key.E)
				DisableBenchMode();
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

		if (isProtMask)
		{
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
		if (e is not InputEventKey keyEvent || !keyEvent.Pressed)
		{
			return;
		}

		if (keyEvent.Keycode == Key.Enter)
		{
			reg.SubmitOrder(keyEvent);
			return;
		}
		else if (keyEvent.Keycode == Key.E)
		{
			inConsoleMode = false;
			cashCam.Current = false;
			lmbParent.Visible = true;
		}
		else
		{
			reg.UseKeyInput(keyEvent);
		}
	}
}
