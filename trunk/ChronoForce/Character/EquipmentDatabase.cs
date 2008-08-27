#region File Description
//-----------------------------------------------------------------------------
// EquipmentDatabase.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// Modified by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using ChronoForce.Base;
#endregion

namespace ChronoForce.Character
{
    /// <summary>
    /// Keeps track of all equipable items from an external file and modifies equipment sent
    /// to this class for switching items on characters.
    /// </summary>
    class EquipmentDatabase
    {
        #region Constants
        // File that contains all the information about equipment
        const string defaultFile = "equipment.dat";
        #endregion

        #region Fields
        // List of all weapons
        List<Equipment> weapons;
        // List of all armor
        List<Equipment> armor;
        // List of all accessories
        List<Equipment> accessory;
        // Debugger for the database
        Debugger EquipDebug;
        // Flag for enabling debug
        bool debugOn = false;
        #endregion

        /// <summary>
        /// Contructor that loads a default database file
        /// </summary>
        EquipmentDatabase()
            : this(defaultFile)
        { }

        /// <summary>
        /// Constructor loading values from a specified file
        /// </summary>
        /// <param name="filename">Specified file with equipment information</param>
        EquipmentDatabase(string filename)
        {
            // Load the debugger
            EquipDebug = new Debugger("EquipDebug", debugOn);

            // First, check to see if the filename has the right format
            if (!filename.Contains(".dat"))
            {
                EquipDebug.debugPrint("[EquipmentDatabase ERROR]: Filename doesn't end in .dat");
                return;
            }

            // Checks to make sure the file is there
            if (!File.Exists(filename))
            {
                string msg = "[EquipmentDatabase ERROR]: File " + filename + " doesn't exist";
                EquipDebug.debugPrint(msg);
                return;
            }

            // Opens the file in binary
            BinaryReader bReader = new BinaryReader(File.OpenRead(filename));

            // TODO:  Load values from the file
        }

        /// <summary>
        /// Changes the supplied equipment to a new item
        /// </summary>
        /// <param name="item">Equipment to change</param>
        /// <param name="itemNumber">ID reference for the new equipment</param>
        public void changeEquipment(ref Equipment item, int itemNumber)
        {
            // TODO:  Look through the list and grab the new item
        }
    }
}
