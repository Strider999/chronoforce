#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace ChronoForceData.Base
{
    /// <summary>
    /// Keeps track of custom settings, from keyboard/button layouts to save files to
    /// screen resolutions.  Only one of these will exist and provide data for all classes.
    /// </summary>
    class ChronosSetting
    {
        #region Fields
        // Size of the display window
        int windowWidth;
        int windowHeight;

        // Input key configurations
        Keys upKey;
        Keys downKey;
        Keys leftKey;
        Keys rightKey;
        Keys confirmKey;
        Keys cancelKey;
        Keys menuKey;
        Keys startKey;
        Keys selectKey;
        
        #endregion

        #region Public Accessors

        /// <summary>
        /// Returns the key for up.
        /// </summary>
        public Keys UpKey
        {
            get { return upKey; }
            set { upKey = value; }
        }

        /// <summary>
        /// Returns the key for down.
        /// </summary>
        public Keys DownKey
        {
            get { return downKey; }
            set { downKey = value; }
        }

        /// <summary>
        /// Returns the key for left.
        /// </summary>
        public Keys LeftKey
        {
            get { return leftKey; }
            set { leftKey = value; }
        }

        /// <summary>
        /// Returns the key for right.
        /// </summary>
        public Keys RightKey
        {
            get { return rightKey; }
            set { rightKey = value; }
        }

        /// <summary>
        /// Returns the key for select, used for confirming and action,
        /// selecting a menu item, and continuing a conversation.
        /// </summary>
        public Keys ConfirmKey
        {
            get { return confirmKey; }
            set { confirmKey = value; }
        }

        /// <summary>
        /// Returns the key for cancel, used for cancelling a command,
        /// going back a menu, or defaulting to "no" for choices
        /// </summary>
        public Keys CancelKey
        {
            get { return cancelKey; }
            set { cancelKey = value; }
        }

        /// <summary>
        /// Returns the key for the menu, used mainly to access the menu.
        /// </summary>
        public Keys MenuKey
        {
            get { return menuKey; }
            set { menuKey = value; }
        }

        /// <summary>
        /// Returns the key for start, used for pausing the game.
        /// </summary>
        public Keys StartKey
        {
            get { return startKey; }
            set { startKey = value; }
        }

        /// <summary>
        /// Returns the key for select, used for confirming and action,
        /// selecting a menu item, and continuing a conversation.
        /// </summary>
        public Keys SelectKey
        {
            get { return selectKey; }
            set { selectKey = value; }
        }

        /// <summary>
        /// Returns the window width for the resolution
        /// </summary>
        public int WindowWidth
        {
            get { return windowWidth; }
            set { windowWidth = value; }
        }

        /// <summary>
        /// Returns the window height for the resolution
        /// </summary>
        public int WindowHeight
        {
            get { return windowHeight; }
            set { windowHeight = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor to load default values
        /// </summary>
        public ChronosSetting()
        {
            // Default values for the settings
            // These are XBox friendly
            //windowWidth = 853;
            //windowHeight = 480;

            // Default values for desktop PC settings
            windowWidth = 800;
            windowHeight = 600;

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
        /// Constructor with setting file to load from
        /// </summary>
        /// <param name="filename">Setting file</param>
        ChronosSetting(string filename)
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
