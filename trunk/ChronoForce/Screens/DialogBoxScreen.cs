#region File Description
//-----------------------------------------------------------------------------
// DialogBoxScreen.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ChronoForce.Base;
#endregion

namespace ChronoForce.Screens
{
    /// <summary>
    /// This dialog box is used for displaying text from characters when talking
    /// to them, scripted events where the characters speak, and special cases 
    /// where in battle characters/enemies speak
    /// </summary>
    public class DialogBoxScreen : GameScreen
    {
        public enum DialogSpeed : int
        {
            Instant = 0,
            Fast = 1,
            Medium = 2,
            Slow = 3
        };

        #region Constants
        // Determines how fast to reveal the characters
        readonly int[] delayTimerSpeed = new int[4]{ 0, 10, 20, 50 };
        #endregion

        #region Fields

        // Title is the character speaking
        string title;
        // Message is the actual dialog that the character speaks
        List<string> messageList;
        // Current message to display on screen
        string currentMsg;
        // Position in the current text
        int textPos = 1;
        // Gradient for the background of the dialog box
        Texture2D gradientTexture;
        // Delay timer for controlling how fast to display the text
        int delayTimer;
        // Tells the box how fast to display the text
        DialogSpeed dialogSpeed;
        // If true, then someone is skipping ahead of the dialog with a button
        bool textComplete = false;
        // If true, the dialog is on a timer and will exit after time has passed
        bool dialogTimed = false;
        // Amount of time for the dialog to show if dialogTimed is true
        int dialogDuration = 0;
        // Delay timer used for a timed dialog
        int dialogTimer = 0;
        // True if the dialog will be centered
        bool centeredDialog = true;
        // True if the dialog will be autosized
        bool autoSizeDialog = true;
        // If the dialog isn't centered, where to render the dialog
        Vector2 dialogPosition = Vector2.Zero;
        // Size of the dialog box if not autosized
        Vector2 dialogSize = Vector2.Zero;

        #endregion

        #region Properties

        /// <summary>
        /// Access the dialog box position.  If the position is set, the dialog will
        /// disable auto centering.
        /// </summary>
        public Vector2 DialogPosition
        {
            get { return dialogPosition; }
            set
            {
                centeredDialog = false;
                dialogPosition = value;
            }
        }

        /// <summary>
        /// Access the dialog box size.  If the size is set, the dialog will
        /// disable autosizing.
        /// </summary>
        public Vector2 DialogSize
        {
            get { return dialogSize; }
            set
            {
                autoSizeDialog = false;
                dialogSize = value;
            }
        }

        /// <summary>
        /// Access the dialog time.  If the time is set, the dialog will
        /// disable input and automatically exit after the time has passed.
        /// </summary>
        public int DialogTime
        {
            get { return dialogDuration; }
            set
            {
                dialogTimed = true;
                dialogDuration = value;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Simple constructor with title, one message, and speed setting
        /// </summary>
        /// <param name="titleArg">Person who is talking in the dialog</param>
        /// <param name="messageArg">Message of the dialog</param>
        /// <param name="speedSetting">How fast to display the dialog</param>
        public DialogBoxScreen(string titleArg, string messageArg, DialogSpeed speedSetting)
        {
            title = titleArg;
            messageList = new List<string>();
            messageList.Add(messageArg);

            // Get the first and only message to display
            currentMsg = messageArg;

            IsPopup = true;

            // Set the delay timer to a default
            dialogSpeed = speedSetting;

            // If the speed is instant, skip ahead will be true
            if (dialogSpeed == DialogSpeed.Instant)
                textComplete = true;
        }

        /// <summary>
        /// Constructor with title, a list of message, and speed setting
        /// </summary>
        /// <param name="titleArg">Person who is talking in the dialog</param>
        /// <param name="messageArg">List of message of the dialog</param>
        /// <param name="speedSetting">How fast to display the dialog</param>
        public DialogBoxScreen(string titleArg, List<string> messageArg, DialogSpeed speedSetting)
        {
            title = titleArg;
            messageList = messageArg;

            // Get the first message to display
            currentMsg = messageList[0];

            IsPopup = true;

            // Set the delay timer to a default
            dialogSpeed = speedSetting;

            // If the speed is instant, skip ahead will be true
            if (dialogSpeed == DialogSpeed.Instant)
                textComplete = true;
        }

        /// <summary>
        /// Simple constructor with title, one message, and speed setting on a timed setting
        /// </summary>
        /// <param name="titleArg">Person who is talking in the dialog</param>
        /// <param name="messageArg">Message of the dialog</param>
        /// <param name="speedSetting">How fast to display the dialog</param>
        /// <param name="duration">How long to display each dialog in milliseconds</param>
        public DialogBoxScreen(string titleArg, string messageArg, DialogSpeed speedSetting,
            int duration)
            : this(titleArg, messageArg, speedSetting)
        {
            dialogTimed = true;
            dialogDuration = duration;
        }

        /// <summary>
        /// Constructor with title, a list of message, and speed setting on a timed setting
        /// </summary>
        /// <param name="titleArg">Person who is talking in the dialog</param>
        /// <param name="messageArg">List of message of the dialog</param>
        /// <param name="speedSetting">How fast to display the dialog</param>
        /// <param name="duration">How long to display each dialog in milliseconds</param>
        public DialogBoxScreen(string titleArg, List<string> messageArg, DialogSpeed speedSetting,
            int duration)
            : this(titleArg, messageArg, speedSetting)
        {
            dialogTimed = true;
            dialogDuration = duration;
        }

        /// <summary>
        /// Simple constructor with title, one message, and speed setting on a timed setting.
        /// The dialog is also positioned as specified.
        /// </summary>
        /// <param name="titleArg">Person who is talking in the dialog</param>
        /// <param name="messageArg">Message of the dialog</param>
        /// <param name="speedSetting">How fast to display the dialog</param>
        /// <param name="position">Where to position the dialog box</param>
        public DialogBoxScreen(string titleArg, string messageArg, DialogSpeed speedSetting,
            Vector2 position)
            : this(titleArg, messageArg, speedSetting)
        {
            centeredDialog = false;
            dialogPosition = position;
        }

        /// <summary>
        /// Constructor with title, a list of message, and speed setting on a timed setting.
        /// The dialog is also positioned as specified.
        /// </summary>
        /// <param name="titleArg">Person who is talking in the dialog</param>
        /// <param name="messageArg">List of message of the dialog</param>
        /// <param name="speedSetting">How fast to display the dialog</param>
        /// <param name="position">Where to position the dialog box</param>
        public DialogBoxScreen(string titleArg, List<string> messageArg, DialogSpeed speedSetting,
            Vector2 position)
            : this(titleArg, messageArg, speedSetting)
        {
            centeredDialog = false;
            dialogPosition = position;
        }

        /// <summary>
        /// Simple constructor with title, one message, and speed setting on a timed setting.
        /// The dialog is also positioned and sized as specified.
        /// </summary>
        /// <param name="titleArg">Person who is talking in the dialog</param>
        /// <param name="messageArg">Message of the dialog</param>
        /// <param name="speedSetting">How fast to display the dialog</param>
        /// <param name="position">Where to position the dialog box</param>
        /// <param name="size">How big to render the dialog box</param>
        public DialogBoxScreen(string titleArg, string messageArg, DialogSpeed speedSetting,
            Vector2 position, Vector2 size)
            : this(titleArg, messageArg, speedSetting, position)
        {
            autoSizeDialog = false;
            dialogSize = size;
        }

        /// <summary>
        /// Constructor with title, a list of message, and speed setting on a timed setting.
        /// The dialog is also positioned and sized as specified.
        /// </summary>
        /// <param name="titleArg">Person who is talking in the dialog</param>
        /// <param name="messageArg">List of message of the dialog</param>
        /// <param name="speedSetting">How fast to display the dialog</param>
        /// <param name="size">How big to render the dialog box</param>
        public DialogBoxScreen(string titleArg, List<string> messageArg, DialogSpeed speedSetting,
            Vector2 position, Vector2 size)
            : this(titleArg, messageArg, speedSetting, position)
        {
            autoSizeDialog = false;
            dialogSize = size;
        }

        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent DialogBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            gradientTexture = content.Load<Texture2D>("dialoggradient");
        }


        #endregion

        #region Handle Input

        /// <summary>
        /// Responds to user input, triggering on a button press.
        /// </summary>
        /// <param name="elapsed">Time passed since the last update in milliseconds</param>
        public override void HandleInput(InputState input, int elapsed)
        {
            // If the dialog is timed, don't handle inputs
            if (dialogTimed)
            {
                // If the text is complete, wait the duration specified
                if (textComplete)
                {
                    dialogTimer += elapsed;

                    if (dialogTimer >= dialogDuration)
                    {
                        // Remove the current message from the list and get the next one
                        // If there's no next message, then this current dialog has ended
                        messageList.RemoveAt(0);
                        if (messageList.Count == 0)
                            ExitScreen();
                        else
                        {
                            // Get the next message
                            currentMsg = messageList[0];

                            // If the dialog isn't instant speed, then the text will be
                            // displayed slowly
                            if (dialogSpeed != DialogSpeed.Instant)
                                textComplete = false;

                            dialogTimer = 0;
                        }
                    }
                }
            }
            else
            {
                if (input.IsNewKeyPress(Keys.A))
                {
                    // Mark that the button has bee pressed.  Under draw, the text will either
                    // display fully (if still in the process) or continue
                    if (textComplete)
                    {
                        // Remove the current message from the list and get the next one
                        // If there's no next message, then this current dialog has ended
                        messageList.RemoveAt(0);
                        if (messageList.Count == 0)
                            ExitScreen();
                        else
                        {
                            // Get the next message
                            currentMsg = messageList[0];

                            // If the dialog isn't instant speed, then the text will be
                            // displayed slowly
                            if (dialogSpeed != DialogSpeed.Instant)
                                textComplete = false;
                        }
                    }
                    else
                    {
                        textComplete = true;
                    }
                }
            }
        }


        #endregion

        #region Draw

        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.GeneralFont;

            // Set the dialogPosition as defaults for the title and text position.  These
            // will be replaced if the dialog is autosized or centered.
            Vector2 titlePosition = dialogPosition;
            Vector2 textPosition = dialogPosition;
            Vector2 textSize = Vector2.Zero;

            // Shift the text down below the title
            textPosition.Y += font.MeasureString(title).Y;

            // Viewport sizes used for centering the dialog if necessary
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = viewportSize = new Vector2(viewport.Width, viewport.Height);

            // If we're autosizing the dialog, do this
            if (autoSizeDialog)
            {
                textSize = font.MeasureString(currentMsg);
                textSize.Y += font.MeasureString(title).Y;
            }

            // If we're centered, used the provided size to calculate the new
            // title and text positions
            if (centeredDialog)
            {
                titlePosition = (viewportSize - dialogSize) / 2;
                textPosition = (viewportSize - dialogSize) / 2;
            }

            // Round all the calculations to prevent an aliasing issue that causes
            // fuzzy fonts on the screen
            titlePosition.X = (float)Math.Round(titlePosition.X);
            titlePosition.Y = (float)Math.Round(titlePosition.Y);
            textPosition.X = (float)Math.Round(textPosition.X);
            textPosition.Y = (float)Math.Round(textPosition.Y);
            textPosition.Y += (float)Math.Round(font.MeasureString(title).Y);

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            // Get the background rectangle for the dialog box
            Rectangle backgroundRectangle;
            if (autoSizeDialog)
            {
                backgroundRectangle = new Rectangle((int)titlePosition.X - hPad,
                                                    (int)titlePosition.Y - vPad,
                                                    (int)textSize.X + hPad * 2,
                                                    (int)textSize.Y + vPad * 2);
            }
            else
            {
                backgroundRectangle = new Rectangle((int)titlePosition.X - hPad,
                                                    (int)titlePosition.Y - vPad,
                                                    (int)dialogSize.X + hPad * 2,
                                                    (int)dialogSize.Y + vPad * 2);
            }

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(gradientTexture, backgroundRectangle, Color.White);

            // Write the title
            spriteBatch.DrawString(font, title, titlePosition, Color.White);

            // Draw the dialog box text based on speed
            // If skipAhead is true, render the whole message at once
            if (textComplete)
            {
                spriteBatch.DrawString(font, currentMsg, textPosition, Color.White);
            }
            else
            {
                // Render the text one letter at a time based on the speed
                // As a safety check, see if the textPos is valid (i.e. not greater than the
                // length of the string)
                if (textPos < currentMsg.Length)
                    spriteBatch.DrawString(font, currentMsg.Substring(0, textPos), textPosition, Color.White);
                else
                    textComplete = true;

                // Update the timer
                delayTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (delayTimer >= delayTimerSpeed[(int)dialogSpeed] && !textComplete)
                {
                    delayTimer = 0;
                    textPos++;

                    // Checks to see if the current message is complete
                    if (textPos >= currentMsg.Length)
                    {
                        textComplete = true;
                        // Reset the text position
                        textPos = 0;
                    }
                }
            }

            spriteBatch.End();
        }


        #endregion
    }
}