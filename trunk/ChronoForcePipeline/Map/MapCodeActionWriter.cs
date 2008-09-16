#region File Description
//-----------------------------------------------------------------------------
// MapCodeActionWriter.cs
//
// Created by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ChronoForceData.Map;
#endregion

namespace ChronoForcePipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class MapCodeActionWriter : ChronoForceWriter<MapCodeAction>
    {
        protected override void Write(ContentWriter output, MapCodeAction value)
        {
            // Basic information
            output.Write(value.MapCodeName);
            output.Write(value.Code);
            output.Write((int)value.Type);

            // Map specific information
            output.Write(value.MapLevel);
            output.Write(value.DestinationMap);
            output.Write(value.DestinationMapLevel);
            output.WriteObject<Point>(value.DestinationMapPosition);
        }
    }
}
