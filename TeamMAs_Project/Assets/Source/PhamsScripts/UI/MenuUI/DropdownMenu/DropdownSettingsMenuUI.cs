// Script Author: Pham Nguyen. All Rights Reserved. 
// GitHub: https://github.com/EricNguyen01.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public abstract class DropdownSettingsMenuUI : DropdownMenuUI
    {
        protected override void Awake()
        {
            base.Awake();

            if (!GameSettings.gameSettingsInstance)
            {
                GameSettings.CreateGameSettingsInstance();
            }
        }
    }
}
