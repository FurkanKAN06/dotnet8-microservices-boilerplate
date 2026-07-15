using System;
using System.Text.RegularExpressions;

namespace EmployeeService.Domain.ValueObjects
{
    public sealed class IdentityNumber
    {
        public string Value { get; }

        private IdentityNumber(string value)
        {
            Value = value;
        }

        public static IdentityNumber Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 11 || !Regex.IsMatch(value, @"^\d{11}$"))
            {
                throw new ArgumentException("TCKN 11 haneli ve sadece rakamlardan oluşmalıdır.");
            }

            return new IdentityNumber(value);
        }

        public override bool Equals(object? obj)
        {
            if (obj is IdentityNumber other)
            {
                return Value == other.Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
