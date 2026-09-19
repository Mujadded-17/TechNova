namespace TechNova.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Set when the page was reached through a status code (404, 403, …)
        /// rather than an unhandled exception, so the copy can match.
        /// </summary>
        public int? StatusCode { get; set; }

        public string Heading => StatusCode switch
        {
            404 => "Page not found",
            403 => "You don't have access to this",
            401 => "Please sign in to continue",
            400 => "That request wasn't valid",
            _ => "Something went wrong"
        };

        public string Detail => StatusCode switch
        {
            404 => "The page you're looking for may have moved, or the link that brought you here is out of date.",
            403 => "Your account doesn't have permission to view this page. If you think that's a mistake, get in touch.",
            401 => "This page is only available once you're signed in.",
            400 => "We couldn't process that request. Try again, and check anything you typed.",
            _ => "An unexpected error occurred on our side. It has been logged, and nothing you entered was lost."
        };
    }
}
