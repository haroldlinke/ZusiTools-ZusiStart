using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ZusiStart.Data;

namespace ZusiStart.ValidationRules
{
    public class ValidNameRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string name)
            {
                DataManager dm = DataManager.Instance;

                if (dm.ReplacementLocos.FirstOrDefault(rl => string.Compare(rl.Name, name, true) == 0) != null)
                {
                    return new ValidationResult(false, "Der Name ist bereits vergeben");
                }

                return new ValidationResult(true, null);
            }

            return new ValidationResult(false, "Der Name darf nicht leer sein");
        }
    }
}
