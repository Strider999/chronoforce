#region File Description
//-----------------------------------------------------------------------------
// Debugger.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ChronoForce
{
    /// <summary>
    /// Handles displaying debugging data on the screen or in the console.
    /// </summary>
    public class Debugger
    {
        #region Constants
        const int cMaxDebugMsgs = 5;
        const int cDefaultDebugTime = 2000; // How long to show a debug message in milliseconds
        #endregion

        #region Fields
        // Required graphics for rendering data to screen
        GraphicsDevice graphics;
        ContentManager contents;
        SpriteBatch spriteBatch;

        // Name of this debugger
        string name;
        // Flag for debug control
        bool debugOn;
        // Flag for whether or not to show the debug message
        bool showDebugMsg;
        // Flag for whether or not to use the graphical debugging
        bool useGraphics;
        // Font for printing debugs on the screen
        SpriteFont debugFont;
        // Counter for keep track of when to erase the debug messages
        int debugTimer;
        // List to keep track of debug strings
        List<string> debugList;
        // Flag for displaying status messages
        bool showStatusMsg;
        // Status message to display in the lower right
        string statusMsg = "";

        #endregion

        #region Public Accessors

        /// <summary>
        /// Debugger flag to activate or deactivate
        /// </summary>
        public bool DebugOn
        {
            set { debugOn = value; }
            get { return debugOn; }
        }

        /// <summary>
        /// Status message to display on the lower right
        /// </summary>
        public string StatusMsg
        {
            get { return statusMsg; }
            set { statusMsg = value; }
        }

        /// <summary>
        /// Flag for showing status messages on screen
        /// </summary>
        public bool ShowStatusMsg
        {
            get { return showStatusMsg; }
            set { showStatusMsg = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Simple constructor that doesn't load any graphical debugging
        /// </summary>
        /// <param name="on">Whether or not the debugger will be on</param>
        public Debugger(string name, bool on)
        {
            this.name = name;
            debugOn = on;
            useGraphics = false;
        }

        /// <summary>
        /// Initilized the debugger
        /// </summary>
        /// <param name="on">Whether or not the debugger will be on</param>
        public Debugger(string name, GraphicsDevice graphicsComponent, ContentManager contentManager, bool on)
        {
            this.name = name;

            if (graphicsComponent == null)
            {
                Console.WriteLine("[Debugger WARNING]: graphics is null, disabling graphical debugging");
                useGraphics = false;
            }
            else
            {
                // Load the graphics
                graphics = graphicsComponent;
                contents = contentManager;
                spriteBatch = new SpriteBatch(graphics);
                debugFont = contents.Load<SpriteFont>("debugfont");
                useGraphics = true;
            }

            // Initialize the debug list
            debugList = new List<string>(cMaxDebugMsgs);

            debugOn = on;
            showDebugMsg = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Prints the specified string into the console if debug is on
        /// </summary>
        /// <param name="msg">String to be printed</param>
        public void debugPrint(string msg)
        {
            if (debugOn)
                Console.WriteLine("[{0}]: {1}", name, msg);
        }

        /// <summary>
        /// Adds a new debug data message to the list for display
        /// </summary>
        /// <param name="debugMsg">Message to add to the list</param>
        public void Add(string debugMsg)
        {
            // If there's already a maximum amount of debug messages, remove the oldest one
            if (debugList.Count == cMaxDebugMsgs)
                debugList.RemoveAt(4);

            // Add the new message to the front of the list
            debugList.Insert(0, debugMsg);

            // Set the flag to show debug messages again and reset the timer
            showDebugMsg = true;
            debugTimer = 0;
        }

        #endregion

        #region Drawing and Rendering

        /// <summary>
        /// Renders the current debug information from the list
        /// </summary>
        /// <param name="gameTime">Used to check for elasped time</param>
        public void Draw(GameTime gameTime)
        {
            // Print out debug message
            if (debugOn && useGraphics)
            {
                spriteBatch.Begin();

                // Calculate the origin of the font
                Vector2 origin = new Vector2(0, debugFont.LineSpacing / 2);

                if (showDebugMsg)
                {
                    // Starting line for messages
                    int y = ChronoForce.gameSettings.WindowHeight - 10;

                    // Print each message in the list to the screen
                    foreach (string msg in debugList.ToArray())
                    {
                        spriteBatch.DrawString(debugFont, msg, new Vector2(0, y), Color.White, 0,
                                               origin, 1, SpriteEffects.None, 0);
                        y -= 15;
                    }

                    // Check to see if the debug message should go away now
                    debugTimer += gameTime.ElapsedGameTime.Milliseconds;
                    if (debugTimer >= cDefaultDebugTime)
                    {
                        debugTimer = 0;
                        showDebugMsg = false;

                        // Also clear the list
                        debugList.Clear();
                    }
                }

                // Render the status on the top right of the screen
                if (showStatusMsg)
                {
                    int statusX = ChronoForce.gameSettings.WindowWidth - (int)debugFont.MeasureString(statusMsg).X;
                    int statusY = 10;
                    spriteBatch.DrawString(debugFont, statusMsg, new Vector2(statusX, statusY), Color.White,
                        0, origin, 1, SpriteEffects.None, 0);
                }

                spriteBatch.End();
            }
        }

        #endregion
    }
}
