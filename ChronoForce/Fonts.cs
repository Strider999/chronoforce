#region File Description
//-----------------------------------------------------------------------------
// Fonts.cs
//
// Created by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
#endregion

namespace ChronoForce
{
    /// <summary>
    /// Static storage of SpriteFont objects and colors for use throughout the game.
    /// </summary>
    static class Fonts
    {
        #region Fonts

        private static SpriteFont generalFont;
        /// <summary>
        /// Returns the general font used in the game
        /// </summary>
        public static SpriteFont GeneralFont
        {
            get { return generalFont; }
        }

        private static SpriteFont menuFont;
        /// <summary>
        /// Returns the menu font used for displaying menus
        /// </summary>
        public static SpriteFont MenuFont
        {
            get { return menuFont; }
        }
        private static SpriteFont battleFont;
        /// <summary>
        /// Returns the battle font used in battles
        /// </summary>
        public static SpriteFont BattleFont
        {
            get { return battleFont; }
        }

        private static SpriteFont debugFont;
        /// <summary>
        /// Returns the debug font for printing on-screen debugging messages
        /// </summary>
        public static SpriteFont DebugFont
        {
            get { return debugFont; }
        }

        private static SpriteFont titleFont;
        /// <summary>
        /// Returns the title font used for map and chapter titles
        /// </summary>
        public static SpriteFont TitleFont
        {
            get { return titleFont; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Load the fonts from the content pipeline.
        /// </summary>
        public static void LoadContent(ContentManager contentManager)
        {
            // check the parameters
            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }

            debugFont = contentManager.Load<SpriteFont>(@"Fonts\debugfont");
            menuFont = contentManager.Load<SpriteFont>(@"Fonts\menufont");
            generalFont = contentManager.Load<SpriteFont>(@"Fonts\generalfont");
            battleFont = contentManager.Load<SpriteFont>(@"Fonts\battleFont");
            titleFont = contentManager.Load<SpriteFont>(@"Fonts\titleFont");
        }

        /// <summary>
        /// Unload all the fonts
        /// </summary>
        public static void UnloadContent()
        {
            debugFont = null;
            menuFont = null;
            generalFont = null;
            battleFont = null;
            titleFont = null;
        }

        #endregion

    }
}
