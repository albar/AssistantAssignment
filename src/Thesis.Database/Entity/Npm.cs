using System;

namespace Thesis.Database.Entity
{
    public class Npm : IEquatable<Npm>
    {
        public static readonly int[] Schema = {2, 2, 4};

        public Npm(string npm)
        {
            var split = npm.Split('.');
            if (split.Length != Schema.Length) throw new Exception("Invalid Npm Schema");
            var year = split[0];
            if (year.Length != Schema[0]) throw new Exception("Invalid Npm Schema");
            var major = split[1];
            if (major.Length != Schema[1]) throw new Exception("Invalid Npm Schema");
            var id = split[2];
            if (id.Length != Schema[2]) throw new Exception("Invalid Npm Schema");
            Year = year;
            Major = major;
            Id = id;
        }

        public string Year { get; }
        public string Major { get; }
        public string Id { get; }

        public override string ToString()
        {
            return string.Format($"{Year}.{Major}.{Id}");
        }

        public bool Equals(Npm other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Year, other.Year) && string.Equals(Major, other.Major) && string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Npm) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Year != null ? Year.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Major != null ? Major.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}