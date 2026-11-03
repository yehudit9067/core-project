using TravelApi.Models;

namespace TravelApi.Services;

public static class TripValidator
{
    /// <summary>
    /// מחזיר הודעת שגיאה אם הבקשה אינה תקינה, או null אם היא תקינה.
    /// </summary>
    public static string? Validate(TripRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "יש להזין שם לטיול.";
        }

        if (string.IsNullOrWhiteSpace(request.Destination))
        {
            return "יש להזין יעד לטיול.";
        }

        if (request.EndDate < request.StartDate)
        {
            return "תאריך הסיום חייב להיות אחרי תאריך ההתחלה.";
        }

        if (request.Budget < 0)
        {
            return "התקציב אינו יכול להיות שלילי.";
        }

        return null;
    }
}
