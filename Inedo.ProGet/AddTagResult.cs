namespace Inedo.ProGet;

public enum AddTagResult
{
    /// <summary>
    /// The tag already exists with the desired target.
    /// </summary>
    Exists,
    /// <summary>
    /// A new tag was created.
    /// </summary>
    Created,
    /// <summary>
    /// An existing was updated to the desired target.
    /// </summary>
    Updated
}
