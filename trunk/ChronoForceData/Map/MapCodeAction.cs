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
using Microsoft.Xna.Framework.Content;
#endregion

namespace ChronoForceData.Map
{
    public enum MapCode : int
    {
        PortalBuilding = 1,
        PortalMap = 2
    }

    /// <summary>
    /// This class contains information about handling map codes on the map, whether switching
    /// screens, cinematics, or triggered objects.
    /// </summary>
    class MapCodeAction
    {
        #region Fields

        MapCode code;

        #endregion

        #region Properties

        /// <summary>
        /// Code type of the given map position
        /// </summary>
        public MapCode Code
        {
            get { return code; }
            set { code = value; }
        }

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
    
                // TODO:  Read in the variables

                return mCode;
            }
        }

        #endregion
    }
}
