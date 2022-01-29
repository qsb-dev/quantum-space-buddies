using System.Collections.Generic;
using Mono.Cecil;

namespace Mirror.Weaver
{
    // Compares TypeReference using FullName
    public class TypeReferenceComparer : IEqualityComparer<TypeReference>
    {
        public bool Equals(TypeReference x, TypeReference y) =>
            x.FullName == y.FullName;

        public int GetHashCode(TypeReference obj) =>
            obj.FullName.GetHashCode();
    }
}
