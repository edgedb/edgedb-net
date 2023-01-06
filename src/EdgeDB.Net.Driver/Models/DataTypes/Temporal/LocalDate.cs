using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    public readonly struct LocalDate
    {
        public DateOnly DateOnly
            => DateOnly.FromDateTime(TemporalCommon.EdgeDBEpocDateTime.AddDays(Days).DateTime);

        public readonly int Days;

        internal LocalDate(int days)
        {
            Days = days;
        }

        public LocalDate(DateOnly date)
            : this(TemporalCommon.ToDays(date))
        {
            
        }

        public static implicit operator DateOnly(LocalDate t) => t.DateOnly;
        public static implicit operator LocalDate(DateOnly t) => new(t);
    }
}
