namespace FluentInfo.Controls.PrettyView;

public sealed partial class ChipControl
{
    public ChipControl(string text)
    {
        InitializeComponent();
        ContentTextBlock.Text = text;
    }
}