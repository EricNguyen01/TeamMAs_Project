// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// <br>This DisableIfAttribute makes a field read-only or editable based on a provided bool cond.</br>
/// </summary>
/// 
[AttributeUsage(AttributeTargets.All, Inherited = true)]
public class DisableIfAttribute : PropertyAttribute
{
    public string targettedProperty;

    public bool disableCond = false;

    public DisableIfAttribute(string property, bool disableCond)
    {
        targettedProperty = property;

        this.disableCond = disableCond;
    }
}
