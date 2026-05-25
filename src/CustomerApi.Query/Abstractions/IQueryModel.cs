using System;

namespace CustomerApi.Query.Abstractions;

public interface IQueryModel;

public interface IQueryModel<out Tkey> : IQueryModel where Tkey : IEquatable<Tkey>
{
    Tkey Id { get; }
}
