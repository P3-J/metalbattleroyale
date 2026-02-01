using System;
using Godot;

public partial class MoneyClock : PanelContainer
{
    private Globals glob;

    private RichTextLabel moneyLabel;

    public override void _Ready()
    {
        glob = GetNode<Globals>("/root/Globals");
        moneyLabel = GetNode<RichTextLabel>("MarginContainer/MoneyLabel");

        glob.Connect("OrderDone", new Callable(this, nameof(UpdateMoney)));
    }

    private void UpdateMoney(bool isDone, int balance, string[] maskNames)
    {
        if (isDone)
        {
            moneyLabel.Text = $"${balance}";
        }
    }
}
