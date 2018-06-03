using System.Collections.Generic;

namespace VirtualKeyboard
{
    interface IKeyboardMode
    {
        Language CurrentLanguage { get; set; }

        string AdditionalUserKeys { get; set; }

        bool IsSeparateNumericBlock { get; set; }

        bool IsUserAdditionalBlock { get; set; }

        bool IsLettersBlock { get; set; }

        List<object> SubscribeControls { get; set; }

        List<string> Contains(string value);

        void RemoteControl(string controlName);
    }
}
