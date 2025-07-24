namespace BMAP.Core.Data.Dapper.Attributes;

/// <summary>
/// Indicates that a property should be ignored during database operations.
/// Properties marked with this attribute will not be included in SQL generation.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class IgnoreAttribute : Attribute
{
}