using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Infrastructure.Data.Extensions;

internal static class ModelBuilderExtensions
{
    internal static void RemoveCascadeDeleteConvention(this ModelBuilder modelBuilder)
    {

        var foreignKeys = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(entity => entity.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade)
            .ToList();

        foreach (var fk in foreignKeys)
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }
}
