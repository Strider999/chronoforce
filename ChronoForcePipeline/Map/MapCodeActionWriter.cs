#region File Description
//-----------------------------------------------------------------------------
// AnimationWriter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
// - Modified by David Hsu
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
            output.Write(value.Code);
            output.Write((int)value.Type);
            output.Write(value.DestinationMap);
            output.WriteObject<Point>(value.DestinationMapPosition);
        }
    }
}
