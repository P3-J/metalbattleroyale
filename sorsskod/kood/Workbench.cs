using Godot;
using System;

public partial class Workbench : Node3D
{

    private Globals glob;
    [Export] PackedScene fireMask;
    [Export] PackedScene iceMask;
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

            case "FireMask":
                RigidBody3D fireM = fireMask.Instantiate<RigidBody3D>();
                GetParent().AddChild(fireM);
                fireM.GlobalPosition = spawnPoint.GlobalPosition;
                break;
             case "IceMask":
                RigidBody3D iceM = iceMask.Instantiate<RigidBody3D>();
                GetParent().AddChild(iceM);
                iceM.GlobalPosition = spawnPoint.GlobalPosition;
                break;

        }


    }

}
