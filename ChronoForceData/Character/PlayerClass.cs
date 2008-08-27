using System;
using System.Collections.Generic;
using ChronoForceData.Base;

namespace ChronoForceData.Character
{
    /// <summary>
    /// Main class for handling all player specific actions
    /// </summary>
    public class PlayerClass : CombatCharacter
    {
        #region Fields
        // Abilities the character possesses for battle
        Ability charAbilities = Ability.Attack | Ability.Skill | Ability.Item;  
        // Special player ID for determining what skills the player can use/gain and
        // for animating combo attacks with more than one person
        int specialID;

        // Equipment the character can wear
        Equipment weapon;
        Equipment armor;
        Equipment accessory;

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the player has the ability to attack
        /// </summary>
        public bool HasAttack
        {
            get { return ((charAbilities & Ability.Attack) == Ability.Attack); }
        }

        /// <summary>
        /// Returns try if the player has the ability to use skills
        /// </summary>
        public bool HasSkill
        {
            get { return ((charAbilities & Ability.Skill) == Ability.Skill); }
        }

        /// <summary>
        /// Returns true if the player has the ability to use time
        /// </summary>
        public bool HasTime
        {
            get { return ((charAbilities & Ability.Time) == Ability.Time); }
        }

        /// <summary>
        /// Returns true if the player has the ability to use anti-time
        /// </summary>
        public bool HasAnti
        {
            get { return ((charAbilities & Ability.Anti) == Ability.Anti); }
        }

        /// <summary>
        /// Returns true if the player can use items
        /// </summary>
        public bool HasItem
        {
            get { return ((charAbilities & Ability.Item) == Ability.Item); }
        }

        /// <summary>
        /// An ID for a specific player in the game.  Used for determining what skills can be
        /// used/gained and for animation of combo attacks.
        /// </summary>
        public int SpecialID
        {
            get { return specialID; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public PlayerClass()
            : base()
        {

        }

        /// <summary>
        /// Constructor with provided name
        /// </summary>
        /// <param name="name">Name of the player</param>
        public PlayerClass(string name)
            : base(name)
        {

        }

        /// <summary>
        /// Constructor with provided name and stats
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <param name="stats">Stats of the player</param>
        public PlayerClass(string name, CharacterStats stats)
            : base(name, stats)
        {

        }

        /// <summary>
        /// Constructor with provided name and stats
        /// </summary>
        /// <param name="name">Name of the player</param>
        /// <param name="stats">Stats of the player</param>
        /// <param name="playerID">ID used in the party position</param>
        /// <param name="sprite">Textures used to represent the player</param>
        /// <param name="specialID">Special ID for specific players to determine skills and animations</param>
        public PlayerClass(string name, CharacterStats stats, CharacterSprite sprite, int specialID)
            : base(name, stats, sprite)
        {
            this.specialID = specialID;
        }

        #endregion

        #region Battle Controls
        /// <summary>
        /// Calculates the attack damage from this character.
        /// </summary>
        /// <returns>The damage amount</returns>
        public override int PerformAttack()
        {
            // For now, just add everything together
            // TODO:  Need to create weapon to test out
            return ATK;
        }

        /// <summary>
        /// Calculates the skill damage from this character.
        /// </summary>
        /// <param name="skillID">ID of the skill used</param>
        /// <returns>The damage amount</returns>
        public override int PerformSkill(int skillID)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the time strength from this character
        /// </summary>
        /// <param name="timeID">ID of the time skill used</param>
        /// <returns>The damage/healing amount</returns>
        public override int PerformTime(int timeID)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the anti-time strength from this character
        /// </summary>
        /// <param name="antiID">ID of anti-time skill used</param>
        /// <returns>The damage/healing amount</returns>
        public override int PerformAnti(int antiID)
        {
            return 0;
        }

        /// <summary>
        /// Uses the specified item.
        /// </summary>
        /// <param name="itemID">ID of the item used</param>
        /// <returns>Amount the item heals if applicable</returns>
        public override int UseItem(int itemID)
        {
            return 0;
        }


        #endregion

        /// <summary>
        /// Level up the character and increase the stats
        /// </summary>
        public void LevelUp()
        {
 
        }
    }
}
