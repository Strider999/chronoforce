using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ChronoForce.Base;
using ChronoForce.Character;
using ChronoForce.Engine;

namespace ChronoForce.Screens
{
    /// <summary>
    /// Stores all the battle menus in a single class that can also keep track of player
    /// positions in the party so it can update the respective battle menus.
    /// </summary>
    class BattleMenuScreen : MenuScreen
    {
        #region Fields
        // Keeps track of player abilites to properly display the choices
        Ability[] items = new Ability[ChronoConstants.cMaxPlayerParty];
        // Keeps track of the battle statistics
        BattleEngine battleEngine;
        // Flag is true if the character is selected a target
        bool selectingTarget = false;
        // ID of the selecting character
        int characterSelecting = -1;
        // ID of the target
        int targetID = -1;

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when an attack command is selected
        /// </summary>
        public event EventHandler<NumberEventArgs> AttackSelected;

        /// <summary>
        /// Method for raising the Attack event.
        /// </summary>
        protected internal void OnAttackSelected(int id, int target)
        {
            if (AttackSelected != null)
                AttackSelected(this, new NumberEventArgs(id, target));
        }

        /// <summary>
        /// Event triggered when an skill command is selected
        /// </summary>
        public event EventHandler<NumberEventArgs> SkillSelected;

        /// <summary>
        /// Method for raising the Skill event.
        /// </summary>
        protected internal void OnSkillSelected(int id, int target)
        {
            if (SkillSelected != null)
                SkillSelected(this, new NumberEventArgs(id, target));
        }

        /// <summary>
        /// Event triggered when an time/anti command is selected
        /// </summary>
        public event EventHandler<NumberEventArgs> TimeSelected;

        /// <summary>
        /// Method for raising the Time/Anti event.
        /// </summary>
        protected internal void OnTimeSelected(int id, int target)
        {
            if (TimeSelected != null)
                TimeSelected(this, new NumberEventArgs(id, target));
        }

        /// <summary>
        /// Event triggered when an item command is selected
        /// </summary>
        public event EventHandler<NumberEventArgs> ItemSelected;

        /// <summary>
        /// Method for raising the Item event.
        /// </summary>
        protected internal void OnItemSelected(int id, int target)
        {
            if (ItemSelected != null)
                ItemSelected(this, new NumberEventArgs(id, target));
        }
        #endregion

        #region Properties

        /// <summary>
        /// Whether or not the player is selecting a target
        /// </summary>
        public bool SelectingTarget
        {
            get { return selectingTarget; }
            set { selectingTarget = value; }
        }

        /// <summary>
        /// Returns the target the player is selecting
        /// </summary>
        public int TargetID
        {
            get { return targetID; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor to create the battle menus
        /// </summary>
        /// <param name="battleEngine">Battle engine used for updating the menus</param>
        /// <param name="startPos">Where to render the first menu</param>
        public BattleMenuScreen(BattleEngine battleEngine, Vector2 startPos)
            : base("Battle Menu", startPos, 75, 0)
        {
            this.battleEngine = battleEngine;

            for (int i = 0; i < ChronoConstants.cMaxPlayerParty; i++)
            {
                // NOTE:  For now, we're just going to add all the menus needed
                // In reality, there should be another contructor for this
                // Create the new menu entries
                BattleMenuEntry attackMenuEntry = new BattleMenuEntry("Attack", i);
                BattleMenuEntry skillMenuEntry = new BattleMenuEntry("Skill", i);
                BattleMenuEntry timeMenuEntry = new BattleMenuEntry("Time", i);
                BattleMenuEntry itemMenuEntry = new BattleMenuEntry("Item", i);

                // Hook up menu event handlers
                attackMenuEntry.Selected += OnAttack;
                skillMenuEntry.Selected += OnSkill;
                timeMenuEntry.Selected += OnTime;
                itemMenuEntry.Selected += OnItem;

                // Show only the menus available
                attackMenuEntry.ShowMe = ((items[i] & Ability.Attack) == Ability.Attack);
                skillMenuEntry.ShowMe = ((items[i] & Ability.Skill) == Ability.Skill);
                timeMenuEntry.ShowMe = ((items[i] & Ability.Time) == Ability.Time);
                itemMenuEntry.ShowMe = ((items[i] & Ability.Item) == Ability.Item);

                // Add entries to the menu.
                MenuEntriesList[i].Add(attackMenuEntry);
                MenuEntriesList[i].Add(skillMenuEntry);
                MenuEntriesList[i].Add(timeMenuEntry);
                MenuEntriesList[i].Add(itemMenuEntry);

                // Set the menus to not show
                SetMenuVisible(i, false);
            }
        }

        #endregion

        #region Event Handlers

        // All of these just call the event method and invokes this class's
        // events.
        void OnAttack(object o, NumberEventArgs e)
        {
            characterSelecting = e.ID;

            // Get a target
            SelectAvailableTarget();
        }

        void OnSkill(object o, NumberEventArgs e)
        {
            OnSkillSelected(e.ID, e.Target);
        }

        void OnTime(object o, NumberEventArgs e)
        {
            OnTimeSelected(e.ID, e.Target);
        }

        void OnItem(object o, NumberEventArgs e)
        {
            OnItemSelected(e.ID, e.Target);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Helper function to find the first available target if any.  If there's no enemies alive
        /// when this is called, the selection is cancelled and reset.
        /// </summary>
        void SelectAvailableTarget()
        {
            // If there's no enemy alive, then don't do anything (should already have the victory
            // screen, but in case that doesn't show, stall until it does)
            if (battleEngine.EnemyAliveCount > 0)
            {
                // Note that we're not going to do anything until the enemy is selected
                selectingTarget = true;

                // Select the first alive target
                targetID = 0;
                while (!battleEngine.IsEnemyAlive(targetID))
                {
                    targetID++;

                    // Bound check
                    if (targetID > battleEngine.EnemyCount)
                        targetID = 0;
                }
            }
            else
            {
                // There's nothing to select from, so cancel it
                ResetBattleSelection();
            }
        }

        /// <summary>
        /// Resets the variables for selecting targets.
        /// </summary>
        void ResetBattleSelection()
        {
            selectingTarget = false;
            characterSelecting = -1;
            targetID = -1;
        }

        #endregion

        #region Inputs

        /// <summary>
        /// Handles the input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="elapsed"></param>
        public override void HandleInput(InputState input, int elapsed)
        {
            // If we're selecting a target, handle it here.  Otherwise, handle menu commands
            if (selectingTarget)
            {
                // Cancel from selecting
                if (input.CancelKey)
                {
                    ResetBattleSelection();
                }

                // If target is selected, clear the menu and signal the attack handler
                if (input.ConfirmKey)
                {
                    // Reset the selected menu entry and flag
                    ResetSelected();
                    selectingTarget = false;

                    // Signal the handler
                    OnAttackSelected(characterSelecting, targetID);
                    characterSelecting = -1;
                    targetID = -1;
                }

                // Select the next target
                if (input.RightKey || input.DownKey)
                {
                    // If there's no enemy don't try to select anything
                    if (battleEngine.EnemyAliveCount > 0)
                    {
                        // Keep cycling through the enemies until we reach on that is alive
                        do
                        {
                            targetID++;

                            // Bound check
                            if (targetID > battleEngine.EnemyCount)
                                targetID = 0;

                        } while (!battleEngine.IsEnemyAlive(targetID));
                    }
                }

                // Select the previous target
                if (input.LeftKey || input.UpKey)
                { 
                    // If there's no enemy don't try to select anything
                    if (battleEngine.EnemyAliveCount > 0)
                    {
                        // Keep cycling through the enemies until we reach on that is alive
                        do
                        {
                            targetID--;

                            // Bound check
                            if (targetID < 0)
                                targetID = battleEngine.EnemyCount - 1;
                        } while (!battleEngine.IsEnemyAlive(targetID) && battleEngine.EnemyAliveCount > 0);
                    }
                }
            }
            else
                base.HandleInput(input, elapsed);
        }

        /// <summary>
        /// Override the cancel so it doesn't do anything
        /// </summary>
        protected override void OnCancel()
        {
        }

        #endregion

        #region Drawing and Updating

        /// <summary>
        /// Update the class with the latest information based on the battle engine
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="otherScreenHasFocus"></param>
        /// <param name="coveredByOtherScreen"></param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            // Checks the battle engine for menu changes based on which characters are ready,
            // and status effects on the characters.
            for (int i = 0; i < ChronoConstants.cMaxPlayerParty; i++)
            {
                bool active;

                // Reset the menu items
                for (int j = 0; j < ChronoConstants.cMaxMenuItems; j++)
                    MenuEntriesList[i][j].ShowMe = false;

                // If the player is ready, go through the menu items
                if (battleEngine.PlayerReady[i])
                {
                    if (battleEngine.HasAttack(i, out active))
                    {
                        MenuEntriesList[i][0].ShowMe = true;
                        MenuEntriesList[i][0].IsActive = active;
                    }
                    else
                        MenuEntriesList[i][0].ShowMe = false;

                    if (battleEngine.HasSkill(i, out active))
                    {
                        MenuEntriesList[i][1].ShowMe = true;
                        MenuEntriesList[i][1].IsActive = active;
                    }
                    else
                        MenuEntriesList[i][1].ShowMe = false;

                    if (battleEngine.HasTime(i, out active))
                    {
                        MenuEntriesList[i][2].ShowMe = true;
                        MenuEntriesList[i][2].IsActive = active;
                    }
                    else
                        MenuEntriesList[i][2].ShowMe = false;

                    if (battleEngine.HasItem(i, out active))
                    {
                        MenuEntriesList[i][3].ShowMe = true;
                        MenuEntriesList[i][3].IsActive = active;
                    }
                    else
                        MenuEntriesList[i][3].ShowMe = false;
                }
            }

            // Updates the menus if not selecting a target
            if (selectingTarget)
            {
                // Checks to see if there's any enemies left.  If there isn't reset selecting target
                if (battleEngine.EnemyAliveCount > 0)
                    base.Update(gameTime, true, coveredByOtherScreen);
                else
                {
                    ResetBattleSelection();
                }
            }
            else
                base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Draws the background to the menus
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            // Renders the text for the menus
            base.Draw(gameTime);
        }

        #endregion
    }
}