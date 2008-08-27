using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChronoForce.Base;
using ChronoForceData.Base;
using ChronoForceData.Character;

namespace ChronoForce.Screens
{
    #region Event Args
    /// <summary>
    /// Custom event argument to pass which menu choice was selected.
    /// </summary>
    public class BattleEventArgs : EventArgs
    {
        private Ability choice;
        private int id;
        public BattleEventArgs(int id, Ability choice)
        {
            this.id = id;
            this.choice = choice;
        }

        /// <summary>
        /// ID of the menu that signaled this event.
        /// </summary>
        public int ID
        {
            get { return id; }
        }

        /// <summary>
        /// Specific choice selected from the menu.
        /// </summary>
        public Ability Choice
        {
            get { return choice; }
        }

    }
    #endregion

    /// <summary>
    /// A special varient of the MenuEntry class that has an ID on it to link it to a 
    /// specific character.
    /// </summary>
    class BattleMenuEntry : MenuEntry
    {
        #region Fields
        // ID will match the ID of the character who controls this menu
        int id = 0;
        #endregion

        #region Events

        /// <summary>
        /// Signaled when battle menu is selected
        /// </summary>
        public new event EventHandler<NumberEventArgs> Selected;

        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        protected internal override void OnSelectEntry()
        {
            if (Selected != null)
                Selected(this, new NumberEventArgs(id));
        }
        #endregion

        #region Public Accessors

        /// <summary>
        /// The ID of the menu.
        /// </summary>
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public BattleMenuEntry(string name)
            : base(name)
        {
            id = -1;
        }

        /// <summary>
        /// Constructor with provided menu id
        /// </summary>
        /// <param name="id">ID of the menu</param>
        /// <param name="items">Abilites to show on the menu</param>
        public BattleMenuEntry(string name, int id)
            : base(name)
        {
            this.id = id;
        }

        /// <summary>
        /// Draws each entry to the proper spot.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(MenuScreen screen, Vector2 position,
                                  bool isSelected, GameTime gameTime)
        {
            base.Draw(screen, position, isSelected, gameTime);
        }

        #endregion
    }
}
