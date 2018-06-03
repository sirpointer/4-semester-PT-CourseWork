using System.Drawing;

namespace VirtualKeyboard
{
    interface IKeyboard
    {
        void Write(string value);

        void Undo();

        void Undo(string value);

        Color KeyColor { get; set; }

        Color KeyForeColor { get; set; }

        int KeyHeight { get; set; }

        int KeyWidth { get; set; }
    }
}
