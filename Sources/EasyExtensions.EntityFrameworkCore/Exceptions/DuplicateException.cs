using System.Net;

namespace EasyExtensions.EntityFrameworkCore.Exceptions
{
    /// <summary>
    /// Duplicate exception.
    /// </summary>
    /// <param name="objectName"> Object name. </param>
    [Serializable]
    public class DuplicateException(string objectName)
        : WebApiException(HttpStatusCode.Conflict, objectName, "Object already exists")
    { }
}