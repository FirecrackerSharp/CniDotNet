namespace CniDotNet.Data.Results.Error;

public enum WellKnownError
{
    IncompatibleCniVersion = 1,
    UnsupportedFieldInNetworkConfiguration = 2,
    ContainerUnknownOrDoesNotExist = 3,
    InvalidNecessaryEnvironmentVariables = 4,
    IoFailure = 5,
    FailedToDecodeContent = 6,
    InvalidNetworkConfiguration = 7,
    TryAgainLater = 11
}