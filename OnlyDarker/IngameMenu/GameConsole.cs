using OnlyDarker.CommonUsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace OnlyDarker.IngameMenu
{
    public class GameConsole
    {
        private static GameConsole _instance;
        private ConsoleCommandsData _commands;
        private Rectangle _bounds => new Rectangle(0, 0, GlobalUse.WindowSize.X, 200);
        private Rectangle _textBox => new Rectangle(0, 0, GlobalUse.WindowSize.X, 100);
        private List<char> _buffer;
        private KeyboardState _lastKeyboardState;
        private string _bufferToString;
        private string _notificationMessage;
        private readonly List<string> _availableCommandsAliases;
        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (value == false)
                {
                    _buffer.Clear();
                    _notificationMessage = string.Empty;
                }
            }
        }
        private GameConsole()
        {
            _instance = this;
            _buffer = new();
            _bufferToString = string.Empty;
            _notificationMessage = string.Empty;
            _availableCommandsAliases = new();
            _commands = ConsoleCommandsData.GetInstance();
        }
        public static GameConsole GetInstance()
        {
            if (_instance is not null)
            {
                return _instance;
            }
            return new GameConsole();
        }
        public void Update(in KeyboardState keyboardState)
        {
            if (!IsActive)
                return;
            _availableCommandsAliases.Clear();
            var k = keyboardState.GetPressedKeys().FirstOrDefault();
            switch (k)
            {
                case Keys.Escape:
                    {
                        if (_lastKeyboardState.IsKeyDown(k))
                            break;
                        IsActive = false; break;
                    }
                case Keys.Enter:
                    {
                        if (_lastKeyboardState.IsKeyDown(k))
                            break;
                        StringBuilder stringBuilder = new();
                        foreach (char c in _buffer)
                        {
                            if (c != default)
                                stringBuilder.Append(c);
                        }
                        var command = stringBuilder.ToString();
                        if (TryExecute(command))
                        {
                            _buffer.Clear();
                        }
                        break;
                    }
                case Keys.Back:
                    {
                        if (_lastKeyboardState.IsKeyDown(k) || _buffer.Count == 0)
                            break;
                        _buffer.RemoveAt(_buffer.Count - 1);
                        _notificationMessage = string.Empty;
                        break;
                    }
                default:
                    {
                        if (_lastKeyboardState.IsKeyDown(k))
                            break;
                        if (KeyToChar(k, out char c))
                            _buffer.Add(c);
                        break;
                    }
            }
            StringBuilder sb = new();
            foreach (char c in _buffer)
            {
                if (c != default)
                    sb.Append(c);
            }
            _bufferToString = sb.ToString();
            if(_bufferToString != string.Empty)
            {
                foreach (var command in _commands.AvailableCommands.Where(command => command.Alias.StartsWith(_bufferToString)))
                {
                    _availableCommandsAliases.Add("   " + command.Alias);
                }
            }
            _lastKeyboardState = keyboardState;
        }
        public void Draw()
        {
            if (!IsActive)
                return;
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, _bounds, Color.Gray);
            GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, _textBox, Color.DarkGray);
            if (_bufferToString != string.Empty)
                GlobalUse.SpriteBatch.DrawString(GlobalUse.Arial, _bufferToString, _textBox.Location.ToVector2(), Color.White, 0F, _textBox.Location.ToVector2(), 0.4F, SpriteEffects.None, 0F);
            if (_notificationMessage != string.Empty)
                GlobalUse.SpriteBatch.DrawString(GlobalUse.Arial, _notificationMessage, new(_textBox.Location.X, _textBox.Location.Y + _textBox.Height), Color.Red, 0F, _textBox.Location.ToVector2(), 0.4F, SpriteEffects.None, 0F);
            if (_availableCommandsAliases.Count > 0)
            {
                var rect = new Rectangle(_bounds.Location.X, _bounds.Location.Y + _bounds.Height, 400, 33 * _availableCommandsAliases.Count);
                GlobalUse.SpriteBatch.Draw(GameBody.EmptyTexture, rect, Color.Gray * 0.8F);
                GameBody.DrawRectangleOutline(rect, Color.White, 2);
                int i = 0;
                foreach (string cmd in _availableCommandsAliases)
                {
                    GlobalUse.SpriteBatch.DrawString(GlobalUse.Arial, cmd, new(_textBox.Location.X, _textBox.Location.Y + _bounds.Height + i), Color.White, 0F, _textBox.Location.ToVector2(), 0.3F, SpriteEffects.None, 0F);
                    i += 30;
                }
            }
        }
        private bool TryExecute(string inputText)
        {
            var s = inputText.Split(' ');
            var command = _commands.AvailableCommands.FirstOrDefault(command => command.Alias == s[0]);
            if (command is null)
            {
                return false;
            }
            command.Execute(s[1], out bool isExecuted, out string message);
            _notificationMessage = message;
            return isExecuted;
        }

        private static bool KeyToChar(Keys key, out char c)
        {
            c = key.ToChar();
            return c != default;
        }
    }
    internal interface IConsoleCommand
    {
        string Alias { get; }
        string Description { get; }
        string ExecutionMessage { get; }
        void Execute(string arg, out bool isExecuted, out string executionMessage);
    }
    internal class StringConsoleCommand : IConsoleCommand
    {
        public string Alias { get; init; }

        public string Description { get; init; }

        public string ExecutionMessage { get; init; }
        public Action<string> Action { get; init; }
        public StringConsoleCommand(string alias, string description, string message, Action<string> action)
        {
            Alias = alias;
            Description = description;
            ExecutionMessage = message;
            Action = action;
        }
        public void Execute(string arg, out bool isExecuted, out string executionMessage)
        {
            try
            {
                Action?.Invoke(arg);
                isExecuted = true;
                executionMessage = ExecutionMessage + arg;
            }
            catch (ArgumentException)
            {
                isExecuted = false;
                executionMessage = "Execution failed: invalid arguments";
            }
            catch (InvalidCastException)
            {
                isExecuted = false;
                executionMessage = "Execution failed: invalid value type";
            }
            catch (FormatException)
            {
                isExecuted = false;
                executionMessage = "Execution failed: invalid value format";
            }
        }
    }

    internal class FloatConsoleCommand : IConsoleCommand
    {
        public string Alias { get; init; }
        public string Description { get; init; }
        public string ExecutionMessage { get; init; }
        public Action<float> Action { get; init; }
        public FloatConsoleCommand(string alias, string description, string message, Action<float> action)
        {
            Alias = alias;
            Description = description;
            ExecutionMessage = message;
            Action = action;
        }
        public void Execute(string arg, out bool isExecuted, out string executionMessage)
        {
            try
            {
                float value = float.Parse(arg);
                Action?.Invoke(value);
                isExecuted = true;
                executionMessage = ExecutionMessage + arg;
            }
            catch (ArgumentException)
            {
                isExecuted = false;
                executionMessage = "Execution failed: invalid arguments";
            }
            catch (InvalidCastException)
            {
                isExecuted = false;
                executionMessage = "Execution failed: invalid value type";
            }
            catch (FormatException)
            {
                isExecuted = false;
                executionMessage = "Execution failed: invalid value format";
            }
        }
    }
    internal class ParamlessConsoleCommand : IConsoleCommand
    {
        public string Alias { get; init; }

        public string Description { get; init; }

        public string ExecutionMessage { get; init; }
        public Action Action { get; init; }
        public ParamlessConsoleCommand(string alias, string description, string message, Action action)
        {
            Alias = alias;
            Description = description;
            ExecutionMessage = message;
            Action = action;
        }

        public void Execute(string arg, out bool isExecuted, out string executionMessage)
        {
            Action?.Invoke();
            isExecuted = true;
            executionMessage = ExecutionMessage;
        }
    }

    internal record ConsoleCommandsData
    {
        private static ConsoleCommandsData _instance;
        public List<IConsoleCommand> AvailableCommands { get; }
        ConsoleCommandsData()
        {
            AvailableCommands = new();
            //game executables g_
            AvailableCommands.Add(new ParamlessConsoleCommand("g_exit", "Exit game", string.Empty, GameBody.GetGameInstance().Exit));
            //main character executables mc_
            AvailableCommands.Add(new FloatConsoleCommand("mc_set_speed", "Sets character speed value", "Character speed set to ", GameBody.GetGameInstance().MainCharacter.SetSpeed));
            AvailableCommands.Add(new FloatConsoleCommand("mc_heal", "Heals character by value", "Character healed by ", GameBody.GetGameInstance().MainCharacter.Heal));
            AvailableCommands.Add(new FloatConsoleCommand("mc_set_stamina", "Sets character stamina to value", "Character stamina set to ", GameBody.GetGameInstance().MainCharacter.SetStamina));
            AvailableCommands.Add(new FloatConsoleCommand("mc_set_cc", "Sets character crit chance to % value", "Character crit chance set to ", GameBody.GetGameInstance().MainCharacter.SetCritChance));
            AvailableCommands.Add(new FloatConsoleCommand("mc_set_cd", "Sets character crit damage to % value", "Character crit damage set to ", GameBody.GetGameInstance().MainCharacter.SetCritDamage));
            AvailableCommands.Add(new FloatConsoleCommand("mc_take_damage", "Damages character by value", "Character damaged by ", GameBody.GetGameInstance().MainCharacter.TestTakingDamage));
            AvailableCommands.Add(new StringConsoleCommand("mc_set_pos", "Sets character position vector to (X,Y)", "Position set to ", GameBody.GetGameInstance().MainCharacter.ConsoleSetPosition));

            _instance = this;
        }
        public static ConsoleCommandsData GetInstance()
        {
            if (_instance is not null)
            {
                return _instance;
            }
            return new ConsoleCommandsData();
        }
    }
}
