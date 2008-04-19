#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Modified by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace ChronoForce.Base
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        #region Fields

        // NOTE:  I'm setting this to one since this will only be a single player game,
        // but if it ever changes, this can help read other controllers.
        public const int MaxInputs = 1;

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly GamePadState[] CurrentGamePadStates;

        public readonly KeyboardState[] LastKeyboardStates;
        public readonly GamePadState[] LastGamePadStates;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            CurrentGamePadStates = new GamePadState[MaxInputs];

            LastKeyboardStates = new KeyboardState[MaxInputs];
            LastGamePadStates = new GamePadState[MaxInputs];
        }


        #endregion

        #region Properties

        // BIG NOTE:  These will be customized based on user settings later on.
        // For now, I'm leaving them hard coded.  Also, there will be more options
        // for input state depending on the screen loaded.
        /// <summary>
        /// Checks for a "menu up" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool UpKey
        {
            get
            {
                return IsNewKeyPress(ChronoForce.gameSettings.UpKey) ||
                       IsNewButtonPress(Buttons.DPadUp) ||
                       IsNewButtonPress(Buttons.LeftThumbstickUp);
            }
        }

        /// <summary>
        /// Checks for a "menu down" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool DownKey
        {
            get
            {
                return IsNewKeyPress(ChronoForce.gameSettings.DownKey) ||
                       IsNewButtonPress(Buttons.DPadDown) ||
                       IsNewButtonPress(Buttons.LeftThumbstickDown);
            }
        }

        /// <summary>
        /// Checks for a right direction input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool RightKey
        {
            get
            {
                return IsNewKeyPress(ChronoForce.gameSettings.RightKey) ||
                       IsNewButtonPress(Buttons.DPadRight) ||
                       IsNewButtonPress(Buttons.LeftThumbstickRight);
            }
        }

        /// <summary>
        /// Checks for a left direction input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool LeftKey
        {
            get
            {
                return IsNewKeyPress(ChronoForce.gameSettings.LeftKey) ||
                       IsNewButtonPress(Buttons.DPadLeft) ||
                       IsNewButtonPress(Buttons.LeftThumbstickLeft);
            }
        }

        /// <summary>
        /// Checks for a continuous "menu up" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool PressedUpKey
        {
            get
            {
                return IsKeyPress(ChronoForce.gameSettings.UpKey) ||
                       IsButtonPress(Buttons.DPadUp) ||
                       IsButtonPress(Buttons.LeftThumbstickUp);
            }
        }

        /// <summary>
        /// Checks for a continuous "menu down" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool PressedDownKey
        {
            get
            {
                return IsKeyPress(ChronoForce.gameSettings.DownKey) ||
                       IsButtonPress(Buttons.DPadDown) ||
                       IsButtonPress(Buttons.LeftThumbstickDown);
            }
        }

        /// <summary>
        /// Checks for a continuous right direction input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool PressedRightKey
        {
            get
            {
                return IsKeyPress(ChronoForce.gameSettings.RightKey) ||
                       IsButtonPress(Buttons.DPadRight) ||
                       IsButtonPress(Buttons.LeftThumbstickRight);
            }
        }

        /// <summary>
        /// Checks for a continuous left direction input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool PressedLeftKey
        {
            get
            {
                return IsKeyPress(ChronoForce.gameSettings.LeftKey) ||
                       IsButtonPress(Buttons.DPadLeft) ||
                       IsButtonPress(Buttons.LeftThumbstickLeft);
            }
        }

        /// <summary>
        /// Checks for a "menu select" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool SelectKey
        {
            get
            {
                return IsNewKeyPress(ChronoForce.gameSettings.ConfirmKey) ||
                       IsNewKeyPress(Keys.Enter) ||
                       IsNewButtonPress(Buttons.A) ||
                       IsNewButtonPress(Buttons.Start);
            }
        }

        /// <summary>
        /// Checks for a confirming input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool ConfirmKey
        {
            get
            {
                return IsNewKeyPress(ChronoForce.gameSettings.ConfirmKey) ||
                       IsNewButtonPress(Buttons.A);
            }
        }

        /// <summary>
        /// Checks for a "menu cancel" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool CancelKey
        {
            get
            {
                return IsNewKeyPress(ChronoForce.gameSettings.CancelKey) ||
                       IsNewButtonPress(Buttons.B) ||
                       IsNewButtonPress(Buttons.Back);
            }
        }


        /// <summary>
        /// Checks for a "pause the game" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool PauseKey
        {
            get
            {
                return IsNewKeyPress(ChronoForce.gameSettings.StartKey) ||
                       IsNewButtonPress(Buttons.Back) ||
                       IsNewButtonPress(Buttons.Start);
            }
        }

        #endregion

        #region Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                LastKeyboardStates[i] = CurrentKeyboardStates[i];
                LastGamePadStates[i] = CurrentGamePadStates[i];

                CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
            }
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsNewKeyPress(Keys key)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsNewKeyPress(key, (PlayerIndex)i))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsNewKeyPress(Keys key, PlayerIndex playerIndex)
        {
            return (CurrentKeyboardStates[(int)playerIndex].IsKeyDown(key) &&
                    LastKeyboardStates[(int)playerIndex].IsKeyUp(key));
        }

        /// <summary>
        /// Helper for checking if a key is still pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsKeyPress(Keys key)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsKeyPress(key, (PlayerIndex)i))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Helper for checking if a key is still pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsKeyPress(Keys key, PlayerIndex playerIndex)
        {
            return CurrentKeyboardStates[(int)playerIndex].IsKeyDown(key);
        }

        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsNewButtonPress(Buttons button)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsNewButtonPress(button, (PlayerIndex)i))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsNewButtonPress(Buttons button, PlayerIndex playerIndex)
        {
            return (CurrentGamePadStates[(int)playerIndex].IsButtonDown(button) &&
                    LastGamePadStates[(int)playerIndex].IsButtonUp(button));
        }

        /// <summary>
        /// Helper for checking if a button is still pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsButtonPress(Buttons button)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsButtonPress(button, (PlayerIndex)i))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Helper for checking if a button is still pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsButtonPress(Buttons button, PlayerIndex playerIndex)
        {
            return CurrentGamePadStates[(int)playerIndex].IsButtonDown(button);
        }

        /// <summary>
        /// Checks for a "menu select" input action from the specified player.
        /// </summary>
        public bool IsMenuSelect(PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Space, playerIndex) ||
                   IsNewKeyPress(Keys.Enter, playerIndex) ||
                   IsNewButtonPress(Buttons.A, playerIndex) ||
                   IsNewButtonPress(Buttons.Start, playerIndex);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action from the specified player.
        /// </summary>
        public bool IsMenuCancel(PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Escape, playerIndex) ||
                   IsNewButtonPress(Buttons.B, playerIndex) ||
                   IsNewButtonPress(Buttons.Back, playerIndex);
        }

        /// <summary>
        /// Reads the keyboard for direction keys (Up/Down, Left/Right, Rotate Counter/Clockwise, Zoom)
        /// only for a single press
        /// </summary>
        /// <param name="playerIndex">Controller to read from</param>
        /// <param name="lowerKey">Negative direction to check</param>
        /// <param name="upperKey">Postive direction to check</param>
        /// <returns>The amount of change based on the direction</returns>
        public float ReadNewKeyboardAxis(PlayerIndex playerIndex, Keys lowerKey, Keys upperKey)
        {
            float move = 0;

            if (IsNewKeyPress(lowerKey, playerIndex))
                move -= 1.0f;
            if (IsNewKeyPress(upperKey, playerIndex))
                move += 1.0f;

            return move;
        }

        /// <summary>
        /// Reads the keyboard for direction keys (Up/Down, Left/Right, Rotate Counter/Clockwise, Zoom)
        /// and updates while the key is held down
        /// </summary>
        /// <param name="playerIndex">Controller to read from</param>
        /// <param name="lowerKey">Negative direction to check</param>
        /// <param name="upperKey">Postive direction to check</param>
        /// <returns>The amount of change based on the direction</returns>
        public float ReadKeyboardAxis(PlayerIndex playerIndex, Keys lowerKey, Keys upperKey)
        {
            float move = 0;

            if ( IsKeyPress(lowerKey, playerIndex) )
                move -= 1.0f;
            if ( IsKeyPress(upperKey, playerIndex) )
                move += 1.0f;

            return move;
        }

        #endregion
    }
}
