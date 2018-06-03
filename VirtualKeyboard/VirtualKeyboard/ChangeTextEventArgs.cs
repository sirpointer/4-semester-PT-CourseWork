using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualKeyboard
{
    public class ChangeTextEventArgs
    {
        private readonly string _text;

        public ChangeTextEventArgs(string text)
        {
            _text = text;
        }

        public string Text { get => _text; }
    }
}
