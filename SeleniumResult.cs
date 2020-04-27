using System;
using System.Collections.Generic;

namespace SeleniumResults
{
    public class SeleniumResult
    {
        public string Name { get; set; }
        public string Time { get; set; }
        public bool IsFailure { get; set; }
        public bool IsSel2 { get; set; }

        protected bool Equals(SeleniumResult other)
        {
            return Time == other.Time;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SeleniumResult) obj);
        }

        public override int GetHashCode()
        {
            return (Time != null ? Time.GetHashCode() : 0);
        }

        public static bool operator ==(SeleniumResult left, SeleniumResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SeleniumResult left, SeleniumResult right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"time-{Time}";
        }
    }
    
    
}