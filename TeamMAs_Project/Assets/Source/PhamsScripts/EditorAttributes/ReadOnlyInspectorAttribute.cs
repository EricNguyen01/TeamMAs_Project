// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <br>This ReadOnlyInspectorAttribute makes a field read-only and un-editable at all time.</br>
/// </summary>

[AttributeUsage(AttributeTargets.All, Inherited = true)]
public class ReadOnlyInspectorAttribute : PropertyAttribute
{

}