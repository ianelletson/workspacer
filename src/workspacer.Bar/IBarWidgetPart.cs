using System;

namespace workspacer.Bar
{
    public interface IBarWidgetPart
    {
        string Text { get; }
        Color ForegroundColor { get; }
        Color BackgroundColor { get; }
        Action PartClicked { get; }
    }
}
