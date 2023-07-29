// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <br>This ReadOnlyInspectorPlayMode makes a field read-only and un-editable during play mode active.</br>
/// <br>Note: This only works on single value field and NOT collections (List, Array, etc.)</br>
/// </summary>
public class ReadOnlyInspectorPlayModeAttribute : PropertyAttribute
{
    
}
