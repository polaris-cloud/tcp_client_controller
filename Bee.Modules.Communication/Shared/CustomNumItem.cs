namespace Bee.Modules.Communication.Shared;

public class CustomNumItem
{
    public bool IsStandard { get; set; } = true;
    public int? Num { get; set; }
    public string Unit { get; set; }
    public FrameFormat SendFrameFormat { get; set; }
    public FrameFormat ReceiveFrameFormat { get; set; }
}