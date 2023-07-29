// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <br>This ReadOnlyInspectorAttribute makes a field read-only and un-editable at all time.</br>
/// <br>Note: This only works on single value field and NOT collections (List, Array, etc.)</br>
/// </summary>
public class ReadOnlyInspectorAttribute : PropertyAttribute
{

}