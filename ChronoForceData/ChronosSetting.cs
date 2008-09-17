#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace ChronoForceData
{
    /// <summary>
    /// Keeps track of custom settings, from keyboard/button layouts to save files to
    /// screen resolutions.  Only one of these will exist and provide data for all classes.
    /// </summary>
    public static class ChronosSetting
    {
        #region Fields
        // Size of the display window
        static int windowWidth;
        static int windowHeight;
        static Vector2 windowSize;
        static Rectangle windowFullSize;

        // Input key configurations
        static Keys upKey;
        static Keys downKey;
        static Keys leftKey;
        static Keys rightKey;
        static Keys confirmKey;
        static Keys cancelKey;
        static Keys menuKey;
        static Keys startKey;
        static Keys selectKey;
        
        #endregion

        #region Public Accessors

        /// <summary>
        /// Returns the key for up.
        /// </summary>
        public static Keys UpKey
        {
            get { return upKey; }
            set { upKey = value; }
        }

        /// <summary>
        /// Returns the key for down.
        /// </summary>
        public static Keys DownKey
        {
            get { return downKey; }
            set { downKey = value; }
        }

        /// <summary>
        /// Returns the key for left.
        /// </summary>
        public static Keys LeftKey
        {
            get { return leftKey; }
            set { leftKey = value; }
        }

        /// <summary>
        /// Returns the key for right.
        /// </summary>
        public static Keys RightKey
        {
            get { return rightKey; }
            set { rightKey = value; }
        }

        /// <summary>
        /// Returns the key for select, used for confirming and action,
        /// selecting a menu item, and continuing a conversation.
        /// </summary>
        public static Keys ConfirmKey
        {
            get { return confirmKey; }
            set { confirmKey = value; }
        }

        /// <summary>
        /// Returns the key for cancel, used for cancelling a command,
        /// going back a menu, or defaulting to "no" for choices
        /// </summary>
        public static Keys CancelKey
        {
            get { return cancelKey; }
            set { cancelKey = value; }
        }

        /// <summary>
        /// Returns the key for the menu, used mainly to access the menu.
        /// </summary>
        public static Keys MenuKey
        {
            get { return menuKey; }
            set { menuKey = value; }
        }

        /// <summary>
        /// Returns the key for start, used for pausing the game.
        /// </summary>
        public static Keys StartKey
        {
            get { return startKey; }
            set { startKey = value; }
        }

        /// <summary>
        /// Returns the key for select, used for confirming and action,
        /// selecting a menu item, and continuing a conversation.
        /// </summary>
        public static Keys SelectKey
        {
            get { return selectKey; }
            set { selectKey = value; }
        }

        /// <summary>
        /// Returns the window width for the resolution
        /// </summary>
        public static int WindowWidth
        {
            get { return windowWidth; }
            set { 
                windowWidth = value;
                windowSize.X = value;
            }
        }

        /// <summary>
        /// Returns the window height for the resolution
        /// </summary>
        public static int WindowHeight
        {
            get { return windowHeight; }
            set { 
                windowHeight = value;
                windowSize.Y = value;
            }
        }

        /// <summary>
        /// Returns the vector form of window size
        /// </summary>
        public static Vector2 WindowSize
        {
            get { return windowSize; }
        }

        /// <summary>
        /// Returns a rectangle that covers the full screen (or window)
        /// </summary>
        public static Rectangle WindowFullSize
        {
            get { return windowFullSize; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Function to load default values
        /// </summary>
        public static void LoadSetting()
        {
            // Default values for the settings
            // These are XBox friendly
            //windowWidth = 853;
            //windowHeight = 480;

            // Default values for desktop PC settings
            windowWidth = 800;
            windowHeight = 600;
            windowSize = new Vector2(windowWidth, windowHeight);
            windowFullSize = new Rectangle(0, 0, windowWidth, windowHeight);

            // Default values for the keys
            upKey = Keys.Up;
            downKey = Keys.Down;
            leftKey = Keys.Left;
            rightKey = Keys.Right;
            startKey = Keys.Enter;
            selectKey = Keys.LeftShift;
            confirmKey = Keys.A;
            cancelKey = Keys.S;
            menuKey = Keys.X;
        }

        /// <summary>
        /// Overloaded function with setting file to load from
        /// </summary>
        /// <param name="filename">Setting file</param>
        public static void LoadSetting(string filename)
        {
            // If the file doesn't exist, load the defaults
            // NOTE:  Maybe create the file?
            if (!File.Exists(filename))
            {
                // Create the default file

                return;
            }


        }

        #endregion

    }
}
