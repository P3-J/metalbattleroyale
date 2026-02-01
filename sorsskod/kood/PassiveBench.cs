using Godot;
using System;
using System.ComponentModel;

public partial class PassiveBench : Node3D
{
	private Globals glob;
	bool unlocked = false;
	MeshInstance3D benchMesh;
	CpuParticles3D workingParticles;
	[Export] int neededMoney = 500;
	[Export] int generatedIncome = 100;
	Timer IncomeTimer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		workingParticles = GetNode<CpuParticles3D>("Particles");
		IncomeTimer = GetNode<Timer>("IncomeInterval");
		benchMesh = GetNode<MeshInstance3D>("entity_0_mesh_instance");
		glob = GetNode<Globals>("/root/Globals");
		glob.Connect("PassiveBenchInteract", new Callable(this, nameof(OnPassiveBenchInteract)));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnPassiveBenchInteract(int money)
	{
		GD.Print("Player interacted with Passive Bench. Current money: ", money);
		if (money >= neededMoney)
		{
			UpgradeBench();
		}
		else
		{
			GD.Print("Not enough money to upgrade Passive Bench.");
		}
	}

	private void UpgradeBench()
	{
		if (!unlocked)
		{
			unlocked = true;
			workingParticles.Emitting = true;
			GD.Print("Passive Bench upgraded! New features unlocked.");
			benchMesh.Transparency = 0.0f;
			IncomeTimer.Start();
			int temp = neededMoney;
			neededMoney = (int)(neededMoney * 1.5);
			glob.EmitSignal("PassiveBenchResponse", -temp, neededMoney);
		}
		else
		{
			generatedIncome = (int)(generatedIncome * 1.5);
			int temp = neededMoney;
			neededMoney = (int)(neededMoney * 1.5);
			glob.EmitSignal("PassiveBenchResponse", -temp, neededMoney);
		}
	}

	private void _on_income_interval_timeout()
	{
		if (unlocked)
		{
			GD.Print("Passive Bench generated income.");
			glob.EmitSignal("PassiveBenchResponse", generatedIncome, -1);
		}
	}
}
