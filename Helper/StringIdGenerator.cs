using System.Reflection;
using MeetingManagement.Attr.IdPrefix;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
namespace MeetingManagement.Helper;
public class PrefixStringIdGenerator : ValueGenerator<string>
{
    public override bool GeneratesTemporaryValues => false;

    public override string Next(EntityEntry entry)
    {
        var type = entry.Entity.GetType();

        var attr = type.GetCustomAttribute<IdPrefixAttribute>();

        var prefix = attr?.Prefix ?? "GEN";

        return $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")}";
    }
}
