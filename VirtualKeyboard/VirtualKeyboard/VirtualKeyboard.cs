using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VirtualKeyboard
{
    public partial class VirtualKeyboard : UserControl, IKeyboard, IKeyboardMode
    {
        #region ctors

        /// <summary>
        /// Инициализирует новый экземпляр класса VirtualKeyboard.
        /// </summary>
        public VirtualKeyboard()
        {
            InitializeComponent();

            KeyColor = Color.FromArgb(180, 60, 60, 60);
            KeyForeColor = Color.White;
            BackColor = Color.FromArgb(220, 0, 0, 0);
            MinKeySize = 38;

            isLettersBlock = true;
            additionalUserKeysCount = 21;
            additionalUserKeys = @"`~!@#$%^&*()-_=+/?><.";
            CurrentLanguage = Language.English;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса VirtualKeyboard, задающий набор дополнительных клавиш.
        /// </summary>
        /// <param name="additionalKeys">Набор дополнительных клавиш. 21 клавиша.</param>
        public VirtualKeyboard(string additionalKeys) : this()
        {
            AdditionalUserKeys = additionalKeys;
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса VirtualKeyboard, 
        /// задающий набор дополнительных клавиш и указывает наличие цифрового блока отдельным рядом.
        /// </summary>
        /// <param name="separateNumericBlock">true для отдельного блока, false для совмещения с верхним рядом.</param>
        /// <param name="additionalKeys">Набор дополнительных клавиш. 21 клавиша.</param>
        public VirtualKeyboard(bool separateNumericBlock, string additionalKeys) : this()
        {
            AdditionalUserKeys = additionalKeys;
            IsSeparateNumericBlock = separateNumericBlock;
        }

        #endregion

        #region Public

        /// <summary>
        /// Минимальная ширина и высота клавиши.
        /// </summary>
        public readonly int MinKeySize;


        private Language currentLanguage;
        private string additionalUserKeys;
        private int additionalUserKeysCount;
        private bool isLettersBlock;
        private bool isUserAdditionalBlock;
        private bool isSeparateNumericBlock;
        private Color keyColor;
        private Color keyForeColor;


        #region Properties

        /// <summary>
        /// Задает или возвращает текущую раскладку.
        /// </summary>
        public Language CurrentLanguage
        {
            get
            {
                return currentLanguage;
            }
            set
            {
                currentLanguage = value;
                OnChangeTheLayout();
                MakeBlock();
            }
        }

        /// <summary>
        /// Возвращает или задает набор дополнительных клавиш. 21 символ.
        /// </summary>
        public string AdditionalUserKeys
        {
            get
            {
                return additionalUserKeys;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new Exception("Невозможно присвоить null или пустое значение.");

                if (value.Length == additionalUserKeysCount)
                    additionalUserKeys = value;
                else if (value.Length > additionalUserKeysCount)
                    additionalUserKeys = value.Substring(0, additionalUserKeysCount);
                else if (value.Length < additionalUserKeysCount)
                {
                    int originalKeysLenght = additionalUserKeys.Length - value.Length;
                    additionalUserKeys = value + additionalUserKeys.Substring(value.Length - 1, originalKeysLenght);
                }
            }
        }

        /// <summary>
        /// Возвращает или задает имена элементов, в которые будет транслироваться текст.
        /// </summary>
        public List<object> SubscribeControls { get; set; } = new List<object>();

        /// <summary>
        /// Возвращает или задает значение, указывающее будет ли цифровой блок вынесен отдельным рядом.
        /// true блок вынесен отдельным рядом, false блок совмещен с верхним буквенным рядом.
        /// </summary>
        public bool IsSeparateNumericBlock
        {
            get
            {
                return isSeparateNumericBlock;
            }
            set
            {
                isSeparateNumericBlock = value;
                MakeBlock();
            }
        }

        /// <summary>
        /// Возвращает или задает значение, указывающее выбран ли пользовательский блок дополнительных клавиш.
        /// true пользовательский блок, false стандартный.
        /// </summary>
        public bool IsUserAdditionalBlock { get => isUserAdditionalBlock; set => isUserAdditionalBlock = value; }

        /// <summary>
        /// Возвращает или задает значение, указывающее выбран ли текущим буквенный блок.
        /// true буквенный блок, false блок дополнительных клавиш.
        /// </summary>
        public bool IsLettersBlock
        {
            get
            {
                return isLettersBlock;
            }
            set
            {
                isLettersBlock = value;
                MakeBlock();
            }
        }

        #region Colors and Keys Size

        /// <summary>
        /// Возвращает или задает цвет клавиш. Не может быть больше (205, 205, 205) в RGB.
        /// </summary>
        public Color KeyColor
        {
            get
            {
                return keyColor;
            }
            set
            {
                if (value.R > 205 || value.G > 205 || value.B > 205)
                    throw new Exception("Значения компонентов RGB должны быть не больше чем (205, 205, 205)");
                else
                {
                    keyColor = value;

                    foreach (var key in Controls)
                    {
                        if (key is Control)
                            (key as Control).BackColor = value;
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает или задает цвет текста на клавиатуре.
        /// </summary>
        public Color KeyForeColor
        {
            get
            {
                return keyForeColor;
            }
            set
            {
                keyForeColor = value;

                foreach (var key in Controls)
                {
                    if (key is Control)
                        (key as Control).ForeColor = value;
                }
            }
        }

        /// <summary>
        /// Возвращает или задает высоту клавиш в пикселях.
        /// Высота должна быть не меньше, чем MinKeySize, иначе высота будет равной MinKeySize.
        /// </summary>
        public int KeyHeight
        {
            get
            {
                return buttonSize.Height;
            }
            set
            {
                if (value < MinKeySize)
                    buttonSize.Height = MinKeySize;
                else
                    buttonSize.Height = value;

                MakeBlock();
            }
        }

        /// <summary>
        /// Возвращает или задает ширину клавиш в пикселях.
        /// Ширина должна быть не меньше, чем MinKeySize, иначе ширина будет равной MinKeySize.
        /// </summary>
        public int KeyWidth
        {
            get
            {
                return buttonSize.Width;
            }
            set
            {
                if (value < MinKeySize)
                    buttonSize.Width = MinKeySize;
                else
                    buttonSize.Width = value;

                MakeBlock();
            }
        }

        public override Color BackColor { get => base.BackColor; set => base.BackColor = value; }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        /// Возвращает список имен элементов, в которых присутствует данная подстрока.
        /// </summary>
        /// <param name="value">Искомая строка.</param>
        /// <returns></returns>
        public List<string> Contains(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Аргумент не должен быть пустым или null", nameof(value));

            List<string> containList = new List<string>();

            foreach (var control in Controls)
            {
                if (control is TextBox)
                {
                    TextBox tb = control as TextBox;

                    if (tb.Text.Contains(value))
                        containList.Add(tb.Name);
                }
                else if (control is Label)
                {
                    Label label = control as Label;

                    if (label.Text.Contains(value))
                        containList.Add(label.Name);
                }
                else if (control is RichTextBox)
                {
                    RichTextBox rich = control as RichTextBox;

                    if (rich.Text.Contains(value))
                        containList.Add(rich.Name);
                }
            }

            return containList;
        }

        #endregion

        #region Methods

        private string lastEnteredCharacters = "";

        /// <summary>
        /// Записывает строку в конец.
        /// </summary>
        /// <param name="value">Строка, которую нужно записать.</param>
        public void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Аргумент не должен быть пустым или null", nameof(value));

            foreach (var control in SubscribeControls)
            {
                if (control is TextBox)
                {
                    (control as TextBox).Text += value;
                }
                else if (control is Label)
                    (control as Label).Text += value;
                else if (control is RichTextBox)
                    (control as RichTextBox).Text += value;
            }
            lastEnteredCharacters = value;

            OnAddText(new ChangeTextEventArgs(value));
        }

        /// <summary>
        /// Удаляет последние введенные символы.
        /// </summary>
        public void Undo()
        {
            Undo(lastEnteredCharacters);
        }

        /// <summary>
        /// Удаляет символы из конца подписанных элементов.
        /// </summary>
        /// <param name="value"></param>
        public void Undo(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Аргумент не должен быть пустым или null", nameof(value));

            foreach (var control in SubscribeControls)
            {
                if (control is TextBox)
                {
                    TextBox tb = control as TextBox;

                    if (tb.Text.EndsWith(value))
                        tb.Text.Remove(tb.Text.LastIndexOf(value));
                }
                else if (control is Label)
                {
                    Label label = control as Label;

                    if (label.Text.EndsWith(value))
                        label.Text.Remove(label.Text.LastIndexOf(value));
                }
                else if (control is RichTextBox)
                {
                    RichTextBox rich = control as RichTextBox;

                    if (rich.Text.EndsWith(value))
                        rich.Text.Remove(rich.Text.LastIndexOf(value));
                }
            }

            OnUndoText(new ChangeTextEventArgs(lastEnteredCharacters));

            lastEnteredCharacters = "";
        }

        /// <summary>
        /// Удаляет элемент из списка подписанных элементов (ControlNames).
        /// </summary>
        /// <param name="controlName">Имя элемента.</param>
        public void RemoteControl(string controlName)
        {
            if (string.IsNullOrWhiteSpace(controlName))
                throw new ArgumentException("Имя должно содержать осмысленное значение.", nameof(controlName));

            for (int i = 0; i < SubscribeControls.Count; i++)
            {
                if ((SubscribeControls[i] as Control).Name.Equals(controlName))
                {
                    SubscribeControls.RemoveAt(i);
                    break;
                }
            }
        }

        #endregion

        private void MakeBlock()
        {
            if (IsLettersBlock)
            {
                if (CurrentLanguage == Language.English)
                    MakeEnglishBlock();
                else
                    MakeRussianBlock();
            }
            else
                MakeAdditionalBlock();
        }

        #region Events

        /// <summary>
        /// Происходит при добавлении текста: нажатие на клавишу или используя метод.
        /// </summary>
        public event EventHandler<ChangeTextEventArgs> AddText;

        protected virtual void OnAddText(ChangeTextEventArgs e)
        {
            EventHandler<ChangeTextEventArgs> temp = AddText;

            if (temp != null)
                AddText(this, e);
        }

        /// <summary>
        /// Происходит при отмене последнего действия.
        /// </summary>
        public event EventHandler<ChangeTextEventArgs> UndoText;

        protected virtual void OnUndoText(ChangeTextEventArgs e)
        {
            EventHandler<ChangeTextEventArgs> temp = UndoText;

            if (temp != null)
                UndoText(this, e);
        }

        /// <summary>
        /// Происходит при смене раскладки.
        /// </summary>
        public event EventHandler ChangeTheLayout;

        protected virtual void OnChangeTheLayout()
        {
            EventHandler temp = ChangeTheLayout;

            if (temp != null)
                ChangeTheLayout(this, new EventArgs());
        }

        #endregion

        #endregion

        #region Making Views

        #region Making Blocks

        private Size RussianBlockSize
        {
            get
            {
                return new Size(14 * buttonSize.Width + distanceBetweenKeys * 15
                    , 4 * buttonSize.Height + distanceBetweenKeys * 5);
            }
        }

        private Size RussianBlockSizeWithNumeric
        {
            get
            {
                return new Size(14 * buttonSize.Width + distanceBetweenKeys * 15
                    , 5 * buttonSize.Height + distanceBetweenKeys * 6);
            }
        }

        private Size EnglishBlockSize
        {
            get
            {
                return new Size(12 * buttonSize.Width + distanceBetweenKeys * 13
                    , 4 * buttonSize.Height + distanceBetweenKeys * 5);
            }
        }

        private Size EnglishBlockSizeWithNumeric
        {
            get
            {
                return new Size(12 * buttonSize.Width + distanceBetweenKeys * 13
                    , 5 * buttonSize.Height + distanceBetweenKeys * 6);
            }
        }

        #region Making Letter Blocks

        private void MakeEnglishBlock()
        {
            Controls.Clear();

            string firstLine = "qwertyuiop";
            string secondLine = "asdfghjkl";
            string thirdLine = "zxcvbnm";

            int x = distanceBetweenKeys;
            int y = distanceBetweenKeys;

            if (IsSeparateNumericBlock)
            {
                MakeTopNumberBlock(x, y);
                y += buttonSize.Height + distanceBetweenKeys;
            }

            MakeFirstLine(x, y, firstLine);

            x = distanceBetweenKeys + buttonSize.Width / 2;
            y += buttonSize.Height + distanceBetweenKeys;

            MakeSecondLine(x, y, secondLine);

            x = distanceBetweenKeys;
            y += buttonSize.Height + distanceBetweenKeys;

            MakeThirdLine(x, y, thirdLine, "Shift", 13);

            y += buttonSize.Height + distanceBetweenKeys;

            MakeFourthLine(x, y, "ENG");
        }

        private void MakeRussianBlock()
        {
            Controls.Clear();

            string firstLine = "йцукенгшщзхъ";
            string secondLine = "фывапролджэ";
            string thirdLine = "ячсмитьбюё";
            int x = distanceBetweenKeys;
            int y = distanceBetweenKeys;

            if (IsSeparateNumericBlock)
            {
                MakeTopNumberBlock(x, y);
                y += buttonSize.Height + distanceBetweenKeys;
            }

            MakeFirstLine(x, y, firstLine);

            x = distanceBetweenKeys + buttonSize.Width / 2;
            y += buttonSize.Height + distanceBetweenKeys;

            MakeSecondLine(x, y, secondLine);

            x = distanceBetweenKeys;
            y += buttonSize.Height + distanceBetweenKeys;

            MakeThirdLine(x, y, thirdLine, "↑", 14);

            y += buttonSize.Height + distanceBetweenKeys;

            MakeFourthLine(x, y, "РУС");
        }

        private void MakeTopNumberBlock(int x, int y)
        {
            string numbers = "1234567890";

            for (int i = 0; i < numbers.Length; i++)
            {
                Controls.Add(GetLabelKey(numbers[i].ToString(), numbers[i].ToString(), x, y));
                x += buttonSize.Width + distanceBetweenKeys;
            }

            if (CurrentLanguage == Language.Russian)
            {
                Controls.Add(GetLabelKey("-", "-", x, y));
                x += buttonSize.Width + distanceBetweenKeys;
                Controls.Add(GetLabelKey("_", "_", x, y));
            }
        }

        private void MakeFirstLine(int x, int y, string firstLine)
        {
            int lineCount = 4;

            if (IsSeparateNumericBlock)
                lineCount = 5;

            Size = new Size(firstLine.Length * buttonSize.Width + buttonSize.Width * 2
                + (firstLine.Length + 3) * distanceBetweenKeys
                , buttonSize.Height * lineCount + distanceBetweenKeys * 5);

            for (int i = 0; i < firstLine.Length; i++)
            {
                Controls.Add(GetLabelKey(firstLine[i].ToString(), firstLine[i].ToString(), x, y));
                x += buttonSize.Width + distanceBetweenKeys;
            }

            Controls.Add(GetBackSpace(x, y));
        }

        private void MakeSecondLine(int x, int y, string secondLine)
        {
            for (int i = 0; i < secondLine.Length; i++)
            {
                Controls.Add(GetLabelKey(secondLine[i].ToString(), secondLine[i].ToString(), x, y));
                x += buttonSize.Width + distanceBetweenKeys;
            }

            Controls.Add(GetEnter(x, y));
        }

        private void MakeThirdLine(int x, int y, string thirdLine, string rightShiftText, int rShiftFontSize)
        {
            
            Controls.Add(GetLeftShift(x, y));
            x += buttonSize.Width + distanceBetweenKeys;

            for (int i = 0; i < thirdLine.Length; i++)
            {
                Controls.Add(GetLabelKey(thirdLine[i].ToString(), thirdLine[i].ToString(), x, y));
                x += buttonSize.Width + distanceBetweenKeys;
            }

            Controls.Add(GetLabelKey(",", ",", x, y));
            x += buttonSize.Width + distanceBetweenKeys;
            Controls.Add(GetLabelKey(".", ".", x, y));
            x += buttonSize.Width + distanceBetweenKeys;

            Controls.Add(GetRightShift(x, y, rightShiftText, 13));
        }

        private void MakeFourthLine(int x, int y, string languageText)
        {
            Controls.Add(GetAdditionalKey("&&123", x, y));
            x += buttonSize.Width + distanceBetweenKeys;

            Controls.Add(GetLabelKey("?", "?", x, y));
            x += buttonSize.Width + distanceBetweenKeys;

            Controls.Add(GetLabelKey("!", "!", x, y));
            x += buttonSize.Width + distanceBetweenKeys;

            Controls.Add(GetSpace(ref x, y, 4));

            Controls.Add(GetLabelKey("(", "(", x, y));
            x += buttonSize.Width + distanceBetweenKeys;

            Controls.Add(GetLabelKey(")", ")", x, y));
            x += buttonSize.Width + distanceBetweenKeys;

            Controls.Add(GetLanguageKey(x, y, languageText));
        }

        #endregion

        #region Making Additional Blocks

        /// <summary>
        /// Разместить на элементе управления блок дополнительных клавиш.
        /// </summary>
        private void MakeAdditionalBlock()
        {
            int x = distanceBetweenKeys;
            int y = distanceBetweenKeys;

            Controls.Clear();

            if (IsSeparateNumericBlock)
            {
                y += buttonSize.Height + distanceBetweenKeys;

                if (CurrentLanguage == Language.English)
                    Size = EnglishBlockSizeWithNumeric;
                else
                    Size = RussianBlockSizeWithNumeric;
            }
            else
            {
                if (CurrentLanguage == Language.English)
                    Size = EnglishBlockSize;
                else
                    Size = RussianBlockSize;
            }


            MakeRightNumericBlock(y);
            MakeSymbolsBlock(x, y, firstStdAddLine, secondStdAddLine, thirdStdAddLine);
            SetStandartTextToAdditional();

            y += (buttonSize.Height + distanceBetweenKeys) * 3;
            Controls.Add(GetAdditionalKey("ABC", x, y, true));
            x += buttonSize.Width + distanceBetweenKeys;
            Controls.Add(GetSpace(x, y, (buttonSize.Width + distanceBetweenKeys) * 6 - distanceBetweenKeys));
        }

        #region Making Num Block

        private void MakeRightNumericBlock(int y)
        {
            int width = distanceBetweenKeys + buttonSize.Width;
            int height = distanceBetweenKeys + buttonSize.Height;
            int x = Width - distanceBetweenKeys - buttonSize.Width;
            int originalX = x;

            Controls.Add(GetLabelKey("9", "9", x, y));
            x -= width;
            Controls.Add(GetLabelKey("8", "8", x, y));
            x -= width;
            Controls.Add(GetLabelKey("7", "7", x, y));
            x = originalX;
            y += height;

            Controls.Add(GetLabelKey("6", "6", x, y));
            x -= width;
            Controls.Add(GetLabelKey("5", "5", x, y));
            x -= width;
            Controls.Add(GetLabelKey("4", "4", x, y));
            x = originalX;
            y += height;

            Controls.Add(GetLabelKey("3", "3", x, y));
            x -= width;
            Controls.Add(GetLabelKey("2", "2", x, y));
            x -= width;
            Controls.Add(GetLabelKey("1", "1", x, y));
            y += height;

            Label zero = GetLabelKey("0", "0", x, y);
            zero.Width = distanceBetweenKeys * 2 + buttonSize.Width * 3;
            Controls.Add(zero);
        }

        #endregion

        #region Making Symbols Block

        private readonly string firstStdAddLine = "\"!@#$%&";
        private readonly string secondStdAddLine = "'()-_=+";
        private readonly string thirdStdAddLine = "\\;:~*/";

        /// <summary>
        /// Добавляет на элемент 3 символьные линии дополнительных клавиш для последующего заполнения.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="firstLine"></param>
        /// <param name="secondLine"></param>
        /// <param name="thirdLine"></param>
        private void MakeSymbolsBlock(int x, int y, string firstLine, string secondLine, string thirdLine)
        {
            char letterName = 'a';

            MakeAdditionalLeftBlockLine(x, y, firstLine, ref letterName);
            y += buttonSize.Height + distanceBetweenKeys;

            MakeAdditionalLeftBlockLine(x, y, secondLine, ref letterName);
            y += buttonSize.Height + distanceBetweenKeys;

            MakeAdditionalLeftBlockLine(x, y, thirdLine, ref letterName, true);
        }
        
        /// <summary>
        /// Добавляет на control линию клавиш для последующего заполнения.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="line"></param>
        /// <param name="letterName"></param>
        /// <param name="isLineWithSwitcher"></param>
        private void MakeAdditionalLeftBlockLine(int x, int y, string line, ref char letterName, bool isLineWithSwitcher = false)
        {
            if (isLineWithSwitcher)
            {
                Controls.Add(GetAdditionalSwitcher(x, y));
                x += buttonSize.Width + distanceBetweenKeys;
            }

            foreach (var symbol in line)
            {
                Controls.Add(GetLabelKey(letterName.ToString(), "", x, y));
                letterName++;
                x += buttonSize.Width + distanceBetweenKeys;
            }
        }

        /// <summary>
        /// Задает дополнительному блоку клавиш стандартный набор символов.
        /// </summary>
        private void SetStandartTextToAdditional()
        {
            char letter = 'a';
            string letters = "";
            string s = firstStdAddLine + secondStdAddLine + thirdStdAddLine;

            for (int i = 0; i < (firstStdAddLine + secondStdAddLine + thirdStdAddLine).Length; i++)
            {
                letters += letter.ToString();
                letter++;
            }

            foreach (var control in Controls)
            {
                if (control is Label)
                {
                    if (letters.Any(x => x == (control as Label).Name[0]) && (control as Label).Name.Length == 1)
                    {
                        string symbol = s[letters.IndexOf((control as Label).Name)].ToString();

                        if (symbol == "&")
                            (control as Label).Text = "&&";
                        else
                            (control as Label).Text = symbol;
                    }
                }
            }
        }

        /// <summary>
        /// Задает дополнительному блоку клавиш пользовательский набор символов.
        /// </summary>
        private void SetUserTextToAdditional()
        {
            char letter = 'a';
            string letters = "";
            string s = AdditionalUserKeys;

            for (int i = 0; i < (firstStdAddLine + secondStdAddLine + thirdStdAddLine).Length; i++)
            {
                letters += letter.ToString();
                letter++;
            }

            foreach (var control in Controls)
            {
                if (control is Label)
                {
                    if (letters.Any(x => x == (control as Label).Name[0]) && (control as Label).Name.Length == 1)
                    {
                        string symbol = s[letters.IndexOf((control as Label).Name)].ToString();

                        if (symbol == "&")
                            (control as Label).Text = "&&";
                        else
                            (control as Label).Text = symbol;
                    }
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #region Special Keys

        #region Switcher

        private CheckBox GetAdditionalSwitcher(int x, int y)
        {
            CheckBox switcher = new CheckBox()
            {
                Name = "switcerCheckBox",
                Text = "→",
                Location = new Point(x, y),
                Size = buttonSize,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 14),
                FlatStyle = FlatStyle.Flat,
                BackColor = KeyColor,
                ForeColor = KeyForeColor,
                Appearance = Appearance.Button
            };

            switcher.FlatAppearance.BorderColor = KeyColor;
            switcher.FlatAppearance.CheckedBackColor = Color.FromArgb(KeyColor.A, KeyColor.R + KeyLightUp, KeyColor.G + KeyLightUp, KeyColor.B + KeyLightUp);
            switcher.FlatAppearance.MouseOverBackColor = Color.FromArgb(KeyColor.A, KeyColor.R + KeyLightUp, KeyColor.G + KeyLightUp, KeyColor.B + KeyLightUp);

            switcher.CheckedChanged += Switcher_CheckedChanged;

            return switcher;
        }

        private void Switcher_CheckedChanged(object sender, EventArgs e)
        {
            (sender as CheckBox).Text = (sender as CheckBox).Checked ? "←" : "→";

            if ((sender as CheckBox).Checked)
                SetUserTextToAdditional();
            else
                SetStandartTextToAdditional();

            IsUserAdditionalBlock = !IsUserAdditionalBlock;
        }
        
        #endregion

        #region Language

        private Label GetLanguageKey(int x, int y, string text)
        {
            Label languageKey = new Label()
            {
                Name = "languageKey",
                Text = text,
                Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 12),
                BackColor = KeyColor,
                ForeColor = KeyForeColor,
                Size = new Size(Width - x - distanceBetweenKeys, buttonSize.Height)
            };

            languageKey.MouseEnter += LabelKey_MouseEnter;
            languageKey.MouseLeave += LabelKey_MouseLeave;
            languageKey.MouseClick += LanguageKey_MouseClick;

            return languageKey;
        }

        private void LanguageKey_MouseClick(object sender, MouseEventArgs e)
        {
            if (CurrentLanguage == Language.English)
                CurrentLanguage = Language.Russian;
            else
                CurrentLanguage = Language.English;
        }

        #endregion

        #region Space

        private Label GetSpace(ref int x, int y, int numberOfKeysOnTheRight)
        {
            int rightWidth = (buttonSize.Width + distanceBetweenKeys) * numberOfKeysOnTheRight + distanceBetweenKeys;
            Label space = GetLabelKey("space", " ", x, y);
            space.Size = new Size(Width - rightWidth - x, buttonSize.Height);
            x += space.Size.Width + distanceBetweenKeys;

            return space;
        }

        private Label GetSpace(int x, int y, int width)
        {
            Label space = GetLabelKey("space", " ", x, y);
            space.Size = new Size(width, buttonSize.Height);

            return space;
        }

        #endregion

        #region Additional Key

        private Label GetAdditionalKey(string value, int x, int y, bool isAdditionalBlock = false)
        {
            Label add = new Label()
            {
                Name = "additional",
                Text = "&&123",
                Location = new Point(x, y),
                Size = buttonSize,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 10),
                BackColor = KeyColor,
                ForeColor = KeyForeColor
            };

            if (isAdditionalBlock)
                add.Text = "ABC";

            add.MouseEnter += LabelKey_MouseEnter;
            add.MouseLeave += LabelKey_MouseLeave;
            add.Click += AdditionalKey_Click;

            return add;
        }

        private void AdditionalKey_Click(object sender, EventArgs e)
        {
            IsLettersBlock = !IsLettersBlock;
        }

        #endregion

        #region Shifts

        private CheckBox GetLeftShift(int x, int y)
        {
            CheckBox lShift = new CheckBox()
            {
                Name = "LShift",
                Text = "↑",
                Location = new Point(x, y),
                Size = buttonSize,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 14),
                FlatStyle = FlatStyle.Flat,
                BackColor = KeyColor,
                ForeColor = KeyForeColor,
                Appearance = Appearance.Button
            };

            lShift.CheckedChanged += Shift_CheckedChanged;

            lShift.FlatAppearance.BorderColor = KeyColor;
            lShift.FlatAppearance.CheckedBackColor = Color.FromArgb(KeyColor.A, KeyColor.R + KeyLightUp, KeyColor.G + KeyLightUp, KeyColor.B + KeyLightUp);
            lShift.FlatAppearance.MouseOverBackColor = Color.FromArgb(KeyColor.A, KeyColor.R + KeyLightUp, KeyColor.G + KeyLightUp, KeyColor.B + KeyLightUp);

            return lShift;
        }

        private void Shift_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox shift = sender as CheckBox ?? throw new Exception("sender - не Shift");
            
            if (shift.Checked)
            {
                ToUpper();
            }
            else if (shift.Name == "LShift")
            {
                if (!(Controls["RShift"] as CheckBox ?? throw new Exception("RShift не существует.")).Checked)
                {
                    ToLower();
                }
            }
            else if (shift.Name == "RShift")
            {
                if (!(Controls["LShift"] as CheckBox ?? throw new Exception("LShift не существует.")).Checked)
                {
                    ToLower();
                }
            }
            else
            {
                throw new Exception("Событие вызвало не Shift'ы");
            }
        }

        private CheckBox GetRightShift(int x, int y, string shiftText, int fontSize)
        {
            CheckBox rShift = new CheckBox()
            {
                Name = "RShift",
                Text = shiftText,
                Location = new Point(x, y),
                Size = new Size(Width - x - distanceBetweenKeys, buttonSize.Height),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", fontSize),
                FlatStyle = FlatStyle.Flat,
                BackColor = KeyColor,
                ForeColor = KeyForeColor,
                Appearance = Appearance.Button
            };

            rShift.CheckedChanged += Shift_CheckedChanged;

            rShift.FlatAppearance.BorderColor = KeyColor;
            rShift.FlatAppearance.CheckedBackColor = Color.FromArgb(KeyColor.A, KeyColor.R + KeyLightUp, KeyColor.G + KeyLightUp, KeyColor.B + KeyLightUp);
            rShift.FlatAppearance.MouseOverBackColor = Color.FromArgb(KeyColor.A, KeyColor.R + KeyLightUp, KeyColor.G + KeyLightUp, KeyColor.B + KeyLightUp);

            return rShift;
        }

        private void ToUpper()
        {
            foreach (var control in Controls)
            {
                if (control is Label)
                {
                    Label key = control as Label;

                    if (key.Text.Length == 1 && key.Text.All(x => char.IsLetter(x)))
                        key.Text = key.Text.ToUpper();
                }
            }
        }

        private void ToLower()
        {
            foreach (var control in Controls)
            {
                if (control is Label)
                {
                    Label key = control as Label;

                    if (key.Text.Length == 1 && key.Text.All(x => char.IsLetter(x)))
                        key.Text = key.Text.ToLower();
                }
            }
        }

        private void ShiftsReset()
        {
            if (IsLettersBlock)
            {
                CheckBox lShift = Controls["LShift"] as CheckBox ?? throw new Exception("LShift не существует.");
                CheckBox rShift = Controls["RShift"] as CheckBox ?? throw new Exception("RShift не существует.");

                if ((lShift.Checked || rShift.Checked) && !(lShift.Checked && rShift.Checked))
                {
                    lShift.Checked = false;
                    rShift.Checked = false;
                }
            }
        }

        #endregion

        #region BackSpace

        private Label GetBackSpace(int x, int y)
        {
            Label backspace = new Label()
            {
                Name = "backspace",
                Text = "BackSpace",
                Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 11),
                BackColor = KeyColor,
                ForeColor = KeyForeColor,
                Size = new Size(buttonSize.Width * 2 + distanceBetweenKeys, buttonSize.Height)
            };

            backspace.Click += Backspace_Click;
            backspace.MouseEnter += LabelKey_MouseEnter;
            backspace.MouseLeave += LabelKey_MouseLeave;

            return backspace;
        }

        private void Backspace_Click(object sender, EventArgs e)
        {
            if (sender != null)
                Undo();
        }

        #endregion

        #region Enter

        private Label GetEnter(int x, int y)
        {
            Label enter = new Label()
            {
                Name = "enter",
                Text = "Enter",
                Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 14),
                BackColor = KeyColor,
                ForeColor = KeyForeColor,
                Size = new Size(Width - x - distanceBetweenKeys, buttonSize.Height)
            };

            enter.Click += Enter_Click;
            enter.MouseEnter += LabelKey_MouseEnter;
            enter.MouseLeave += LabelKey_MouseLeave;

            return enter;
        }

        private void Enter_Click(object sender, EventArgs e)
        {
            Write(Environment.NewLine);
        }

        #endregion

        #endregion

        #region Ordinary Keys

        private Label GetLabelKey(string name, string value, int x, int y)
        {
            Label key = new Label()
            {
                Name = name,
                Text = value,
                Location = new Point(x, y),
                Size = buttonSize,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 14),
                BackColor = KeyColor,
                ForeColor = KeyForeColor
            };

            key.Click += TextKey_Click;
            key.MouseEnter += LabelKey_MouseEnter;
            key.MouseLeave += LabelKey_MouseLeave;

            return key;
        }

        private void TextKey_Click(object sender, EventArgs e)
        {
            if (sender != null)
            {
                Label label = sender as Label;
                Write(label.Text);
                ShiftsReset();
            }
        }

        private void LabelKey_MouseEnter(object sender, EventArgs e)
        {
            (sender as Label).BackColor = Color.FromArgb(KeyColor.A, KeyColor.R + KeyLightUp, KeyColor.G + KeyLightUp, KeyColor.B + KeyLightUp);
        }

        private void LabelKey_MouseLeave(object sender, EventArgs e)
        {
            (sender as Label).BackColor = Color.FromArgb(KeyColor.A, KeyColor.R, KeyColor.G, KeyColor.B);
        }
        
        #endregion

        private Size buttonSize = new Size(54, 54);
        private int distanceBetweenKeys = 4;
        private readonly int KeyLightUp = 50;

        #endregion
    }
}

