namespace Domain.Helpers;

public static class UrlHelper
{
    public static Guid? GetGuidFromLastPartOfUrl(string url)
    {
        Uri uri = new(url);
        string lastSegment = uri.Segments.Last();

        // Remove trailing slash if present
        if (lastSegment.EndsWith('/'))
        {
            lastSegment = lastSegment.Remove(lastSegment.Length - 1);
        }

        return Guid.TryParse(lastSegment, out Guid guid) ? guid : null;
    }
}