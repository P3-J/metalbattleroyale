using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class CashRegister : PanelContainer
{
    private string sum = "0";

    private RichTextLabel sumAmountLabel;

    private VBoxContainer lineItemContainer;

    const int maxSumLength = 10;

    private List<int> itemPrices = [];

    private MarginContainer marginContainer;

    public override void _Ready()
    {
        sumAmountLabel = GetNode<RichTextLabel>(
            "MarginContainer/CashRegisterContainer/SumLineContainer/SumAmountLabel"
        );

        ChangeSumAmount(sum);

        lineItemContainer = GetNode<VBoxContainer>(
            "MarginContainer/CashRegisterContainer/LineItemContainer"
        );

        marginContainer = GetNode<MarginContainer>("MarginContainer");

        RichTextLabel cursor = GetNode<RichTextLabel>(
            "MarginContainer/CashRegisterContainer/SumLineContainer/Cursor"
        );

        Tween tween = GetTree().CreateTween();

        tween.SetLoops();
        tween.TweenProperty(cursor, "modulate:a", 1.0, 0);
        tween.TweenInterval(0.6f);
        tween.TweenProperty(cursor, "modulate:a", 0.0, 0);
        tween.TweenInterval(0.6f);

        AddLineItem("PlaceHolder Mask A", 1000);
        AddLineItem("PlaceHolder Mask B", 3000);
        AddLineItem("PlaceHolder Mask C", 4500);
    }

    public void AddLineItem(string itemName, int itemPrice)
    {
        PackedScene scene = GD.Load<PackedScene>("res://sorsskod/scenes/RegisterItem.tscn");

        HBoxContainer registerItem = scene.Instantiate<HBoxContainer>();

        RichTextLabel itemNameNode = registerItem.GetNode<RichTextLabel>("ItemName");
        RichTextLabel itemPriceNode = registerItem.GetNode<RichTextLabel>("ItemPrice");

        itemNameNode.Text = $"{itemName} : ";
        itemPriceNode.Text = $"${itemPrice}";

        lineItemContainer.AddChild(registerItem);

        itemPrices.Add(itemPrice);
    }

    public void UseKeyInput(InputEventKey eventKey)
    {
        if (!eventKey.Pressed)
            return;

        if (
            eventKey.Keycode >= Key.Key0 && eventKey.Keycode <= Key.Key9
            || eventKey.Keycode >= Key.Kp0 && eventKey.Keycode <= Key.Kp9
        )
        {
            if (sum.Length > maxSumLength)
            {
                return;
            }

            int pressedNumber = (char)eventKey.Unicode - '0';

            if (sum == "0")
            {
                ChangeSumAmount(pressedNumber.ToString());
            }
            else
            {
                ChangeSumAmount(sum + pressedNumber.ToString());
            }
        }

        if (eventKey.Keycode == Key.Backspace && sum.Length > 0)
        {
            ChangeSumAmount(sum[..^1]);
        }

        if (eventKey.Keycode == Key.Enter)
        {
            MakeResultMessageLabel(CheckSumEntryValidity() ? "SUCCESS!" : "FAIL!");
        }
    }

    private void ChangeSumAmount(string sumInput)
    {
        sum = sumInput;

        sumAmountLabel.Text = $"${sum}";
    }

    private bool CheckSumEntryValidity()
    {
        if (!(sum.Length > 0))
        {
            return false;
        }
        int realSum = itemPrices.Sum();
        long userSum = Convert.ToInt64(sum);

        return realSum == userSum;
    }

    private async void MakeResultMessageLabel(String message)
    {
        RichTextLabel resultLabel = new();

        resultLabel.AddThemeFontSizeOverride("normal_font_size", 50);

        resultLabel.Text = message;

        resultLabel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        resultLabel.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
        resultLabel.AutowrapMode = TextServer.AutowrapMode.Off;
        resultLabel.FitContent = true;

        marginContainer.AddChild(resultLabel);

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(resultLabel, "modulate", Colors.Green, 1.0f);
        tween.TweenProperty(resultLabel, "scale", Vector2.Zero, 1.0f);
        tween.TweenCallback(Callable.From(resultLabel.QueueFree));
    }
}
