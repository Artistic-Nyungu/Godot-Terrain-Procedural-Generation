using Godot;
using System;

public partial class PathFollower : PathFollow3D
{
    [Export(PropertyHint.Range, "0,1,0.0001")]
    public float Speed { get; set; } = 0.05f;

    public override void _Process(double delta)
    {
        ProgressRatio += Speed * (float)delta;

        if (ProgressRatio >= 1f)
            ProgressRatio -= 1f;

        var pivot = GetNode<Node3D>("CameraPivot");

        pivot.LookAtFromPosition(Position, Vector3.Zero);
    }
}
