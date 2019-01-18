using FluentValidation;
using FluentValidation.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentDeepEqual
{
    internal static class Initializer
    {
        static Initializer()
        {
            if (ValidatorOptions.LanguageManager != null && ValidatorOptions.LanguageManager is LanguageManager)
            {
                var langMgr = (LanguageManager)ValidatorOptions.LanguageManager;

                langMgr.AddTranslation("en", "_FDE_Null", "Value is null. No point doing a deeper comparison.");
                langMgr.AddTranslation("en", "_FDE_CollCountMismatch", "The number of items in the two collections does not match. No point doing a deeper comparison.");
                langMgr.AddTranslation("en", "_FDE_ItemNotFoundInColl", "Could not find this item in other collection.");
            }
        }

        public static void NoOp()
        {
            // does nothing, but ensures a constructor is kicked off at most once
        }
    }
}
