// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <br>This ReadOnlyInspectorPlayMode makes a field read-only and un-editable during play mode active.</br>
/// </summary>

[AttributeUsage(AttributeTargets.All, Inherited = true)]
public class ReadOnlyInspectorPlayModeAttribute : PropertyAttribute
{
    
}
