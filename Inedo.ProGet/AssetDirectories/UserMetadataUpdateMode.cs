namespace Inedo.ProGet.AssetDirectories;

/// <summary>
/// Specifies the mode used to set user-defined asset item metadata.
/// </summary>
public enum UserMetadataUpdateMode
{
    /// <summary>
    /// Properties will be created and updated as necessary but not deleted.
    /// </summary>
    CreateOrUpdate,
    /// <summary>
    /// Properties will be created, updated, and deleted as necessary to match the specified collection.
    /// </summary>
    ReplaceAll
}
