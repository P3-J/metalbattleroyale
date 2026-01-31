using Godot;
using System;

public partial class Globals : Node
{
    [Signal] public delegate void SpawnItemEventHandler(string itemName);

}
