using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ResearchCalc
{
    public partial class ResearchCalc : Form
    {
        private Timer _renderLoop;
        private string _currentMessage;
        private readonly Stack<float> _input = new Stack<float>();
        private readonly Stack<Keys> _operation = new Stack<Keys>();
        private float _result;

        private string build = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        // Allow for numlock being off or on
        private readonly List<Keys> _numpadNumberOrDot = new List<Keys>
        {
            Keys.Home,
            Keys.Up,
            Keys.PageUp,
            Keys.Left,
            Keys.Clear,
            Keys.Right,
            Keys.End,
            Keys.Down,
            Keys.Next,
            Keys.Insert,
            Keys.Delete,

            Keys.NumPad7,
            Keys.NumPad8,
            Keys.NumPad9,
            Keys.NumPad4,
            Keys.NumPad5,
            Keys.NumPad6,
            Keys.NumPad1,
            Keys.NumPad2,
            Keys.NumPad3,
            Keys.NumPad0,
            Keys.Decimal
        };

        private readonly List<Keys> _numpadOperator = new List<Keys>
        {
            Keys.Divide,
            Keys.Multiply,
            Keys.Subtract,
            Keys.Add,
            Keys.Return
        };

        private float _firstPart;
        private float _secondPart;
        private string _lastOperation;

        public ResearchCalc()
        {
            InitializeComponent();

            _renderLoop = new Timer {Interval = 100}; // = 2fps
            _renderLoop.Tick += RenderLoopTick;
            _renderLoop.Start();
        }

        private void RenderLoopTick(object sender, EventArgs e)
        {
            Shared.Utilities.WriteTextLineToPictureBox("Escape| to quit    " + build, false, 0, pbxMain,
                new[] {Color.BlueViolet, Color.Black});
            Shared.Utilities.WriteTextLineToPictureBox(_currentMessage, false, 1, pbxMain, new[] {Color.Black});

            Shared.Utilities.WriteTextLineToPictureBox(_firstPart + " " + _lastOperation + " " + _secondPart, true, 4, pbxMain, new[] { Color.Red });
            Shared.Utilities.WriteTextLineToPictureBox(_result.ToString(), true, 5, pbxMain, new[] {Color.Green});
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            _currentMessage = keyData.ToString();

            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            if (_numpadNumberOrDot.Contains(keyData))
            {
                ProcessNumpad(keyData);
                return true;
            }
            if (_numpadOperator.Contains(keyData))
            {
                ProcessOperator(keyData);
                return true;
            }

            _currentMessage += " [unrecognised]";
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ProcessOperator(Keys keyData)
        {
            float inputFloat = 0;
            int multiplier = 1;
            int poppedCount = 0;
            int decimalDrop = 0;

            while (_input.Any())
            {
                var input = _input.Pop();

                if (float.IsNegativeInfinity(input))
                {
                    decimalDrop = poppedCount;
                }
                else
                {
                    inputFloat += (input * multiplier);
                    multiplier *= 10;
                    poppedCount++;
                }
            }

            if (decimalDrop != 0)
            {
                inputFloat /= (decimalDrop * 10);
            }

            // _input will be empty now
            
            switch (keyData)
            {
                case Keys.Return:
                    _secondPart = inputFloat;

                    if (!_numpadOperator.Contains(keyData) || !_operation.Any()) break;
                    var operation = _operation.Pop();
                    _lastOperation = operation.ToString();

                    switch (operation)
                    {
                        case Keys.Divide:
                            if (Math.Abs(_secondPart) > float.Epsilon*2) _result = _firstPart/_secondPart;
                            else _result = float.NaN;
                            break;
                        case Keys.Multiply:
                            _result = _firstPart * _secondPart;
                            break;
                        case Keys.Subtract:
                            _result = _firstPart - _secondPart;
                            break;
                        case Keys.Add:
                            _result = _firstPart + _secondPart;
                            break;
                        default:
                            throw new Exception("Unsupported operation: " + keyData);
                    }

                    break;
                default:
                    _operation.Push(keyData);
                    _firstPart = inputFloat;
                    break;
            }
        }

        private void ProcessNumpad(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.NumPad7:
                case Keys.Home:
                    _input.Push(7);
                    break;
                case Keys.NumPad8:
                case Keys.Up:
                    _input.Push(8);
                    break;
                case Keys.NumPad9:
                case Keys.PageUp:
                    _input.Push(9);
                    break;
                case Keys.NumPad4:
                case Keys.Left:
                    _input.Push(4);
                    break;
                case Keys.NumPad5:
                case Keys.Clear:
                    _input.Push(5);
                    break;
                case Keys.NumPad6:
                case Keys.Right:
                    _input.Push(6);
                    break;
                case Keys.NumPad1:
                case Keys.End:
                    _input.Push(1);
                    break;
                case Keys.NumPad2:
                case Keys.Down:
                    _input.Push(2);
                    break;
                case Keys.NumPad3:
                case Keys.Next:
                    _input.Push(3);
                    break;
                case Keys.NumPad0:
                case Keys.Insert:
                    _input.Push(0);
                    break;
                case Keys.Decimal:
                case Keys.Delete:
                    _input.Push(float.NegativeInfinity);
                    break;
            }
        }
    }
}