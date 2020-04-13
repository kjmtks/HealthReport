using System;
namespace NCVC.App.Models
{
    /*
    public class HealthItemValue
    {
        public int HealthItemId { get; set; }
        public virtual HealthItem HealthItem { get; set; }
        public int HealthId { get; set; }
        public virtual Health Health { get; set; }
        public string ValueString { get; set; }

        //---------------------

        
        public bool IsAppropriate()
        {
            switch (HealthItem.ValueType)
            {
                case HealthItemValueType.Decimal:
                    if(!decimal.TryParse(ValueString, out var value))
                    {
                        return false;
                    }
                    return HealthItem.AppropriateDecimalMinimaum <= value && value <= HealthItem.AppropriateDecimalMaximum;
                case HealthItemValueType.String:
                    return ValueString?.Trim() == HealthItem.AppropriateStringValue.Trim();
                default:
                    return false;
            }
        }

    }
    */
}
