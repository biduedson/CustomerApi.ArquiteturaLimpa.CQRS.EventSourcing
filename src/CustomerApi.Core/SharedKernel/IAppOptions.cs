namespace CustomerApi.Core.SharedKernel;

public interface IAppOptions
{
    static abstract string ConfigSectionPath { get; }
}
