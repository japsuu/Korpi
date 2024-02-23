namespace Korpi.Client.Utils;

public static class Extensions
{
    /// <summary>
    /// Retrieves custom attributes in a typed enumerable.
    /// </summary>
    /// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this type are returned.</typeparam>
    /// <param name="type">The member on which to look for custom attributes.</param>
    /// <param name="inherit">Specifies whether to search this member's inheritance chain to find the attributes.</param>
    /// <returns>An IEnumerable of custom attributes applied to this member.</returns>
    public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit)
    {
        return type.GetCustomAttributes(typeof(T), inherit).Cast<T>();
    }
}