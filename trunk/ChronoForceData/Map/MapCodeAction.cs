#region File Description
//-----------------------------------------------------------------------------
// MapCodeAction.cs
//
// Created by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace ChronoForceData.Map
{
    public enum CodeType : int
    {
        PortalBuilding = 1,
        PortalMap = 2
    }

    /// <summary>
    /// This class contains information about handling map codes on the map, whether switching
    /// screens, cinematics, or triggered objects.
    /// </summary>
    public class MapCodeAction : ContentObject
    {
        #region Fields

        // Map code name used to identify which code it belongs to on the map
        string mapCodeName;
        // Map code on the map
        int code;
        // Type of code this represents
        CodeType type;
        // Filename to load actions from if any
        string filename;
        // List of strings for displaying dialog text
        List<string> dialog;

        // Level of the map (0 is the base, 1 is a building, 2 is a room inside a building, etc.
        int mapLevel;
        // Exit location where the portal takes the player
        string destinationMap;
        int destinationMapLevel;
        Point destinationMapPosition;

        #endregion

        #region Properties

        #region Basic Properites

        /// <summary>
        /// Map code name used to identify which code it belongs to on the map
        /// </summary>
        public string MapCodeName
        {
            get { return mapCodeName; }
            set { mapCodeName = value; }
        }

        /// <summary>
        /// Code number on the map
        /// </summary>
        public int Code
        {
            get { return code; }
            set { code = value; }
        }

        /// <summary>
        /// Code type of the given map position
        /// </summary>
        public CodeType Type
        {
            get { return type; }
            set { type = value; }
        }

        #endregion

        #region Map Properties

        /// <summary>
        /// Level of the map layer
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int MapLevel
        {
            get { return mapLevel; }
            set { mapLevel = value; }
        }

        /// <summary>
        /// Which map the portal takes the player when stepped on
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string DestinationMap
        {
            get { return destinationMap; }
            set { destinationMap = value; }
        }

        /// <summary>
        /// Destination level of the map layer
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int DestinationMapLevel
        {
            get { return destinationMapLevel; }
            set { destinationMapLevel = value; }
        }

        /// <summary>
        /// Where on the new map the portal will take the player to
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Point DestinationMapPosition
        {
            get { return destinationMapPosition; }
            set { destinationMapPosition = value; }
        }

        #endregion

        #endregion

        #region Content Type Reader

        /// <summary>
        /// Reads a map code from the pipeline
        /// </summary>
        public class MapCodeActionReader : ContentTypeReader<MapCodeAction>
        {
            /// <summary>
            /// Reads a MapCodeAction object from the content pipeline.
            /// </summary>
            protected override MapCodeAction Read(ContentReader input,
                MapCodeAction existingInstance)
            {
                MapCodeAction mCode = existingInstance;
                if (existingInstance == null)
                {
                    mCode = new MapCodeAction();
                }
    
                // Read in the code and type
                mCode.MapCodeName = input.ReadString();
                mCode.Code = input.ReadInt32();
                mCode.Type = (CodeType)input.ReadInt32();

                // Read the in portal data for now
                mCode.MapLevel = input.ReadInt32();
                mCode.DestinationMap = input.ReadString();
                mCode.DestinationMapLevel = input.ReadInt32();
                mCode.DestinationMapPosition = input.ReadObject<Point>();

                return mCode;
            }
        }

        #endregion
    }
}
