using Godot;
using System;

public partial class Shopworld : Node3D
{

    [Export] Camera3D startcam;
    [Export] Node2D menustuff;
    Globals glob;

    public override void _Ready()
    {
        base._Ready();
        glob = GetNode<Globals>("/root/Globals");
        glob.EmitSignal("PlayerMovement", false);
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public void _on_button_pressed()
    {
        startcam.Current = false;
        menustuff.Visible = false;
        glob.EmitSignal("PlayerMovement", true);
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }



}
