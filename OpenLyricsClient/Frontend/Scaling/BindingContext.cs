﻿using System.Collections.Generic;

namespace OpenLyricsClient.Frontend.Scaling;

/// <summary>
/// Represents a context that can be scaled by the <see cref="ScalingManager"/>.
/// </summary>
public class BindingContext : HashSet<ScalableObject>
{
}