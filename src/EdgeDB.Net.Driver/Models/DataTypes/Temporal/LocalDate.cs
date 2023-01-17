using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.DataTypes
{
    public readonly struct LocalDate
    {
        public DateOnly DateOnly
            => DateOnly.FromDateTime(TemporalCommon.EdgeDBEpocDateTimeUTC.AddDays(Days).DateTime);

        public readonly int Days;

        internal LocalDate(int days)
        {
            Days = days;
        }

        public LocalDate(DateOnly date)
            : this(TemporalCommon.ToDays(date))
        {
            
        }
        
        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not LocalDate d)
                return false;

            return d.Days == Days;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Days.GetHashCode();


        public static implicit operator DateOnly(LocalDate t) => t.DateOnly;
        public static implicit operator LocalDate(DateOnly t) => new(t);

        public static bool operator ==(LocalDate left, LocalDate right) => left.Equals(right);
        public static bool operator !=(LocalDate left, LocalDate right) => !left.Equals(right);
    }
}
