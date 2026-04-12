namespace BlogProject.Web.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the request");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Response zaten gönderilmişse bir şey yapma
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot handle exception");
                return;
            }

            // Sadece gerçek hataları logla (OperationCanceledException gibi normal durumları atla)
            if (ex is not OperationCanceledException and not TaskCanceledException)
            {
                // Stack trace'siz hafif loglama
                _logger.LogError("Unhandled exception: {ExceptionType} - {Message}",
                               ex.GetType().Name, ex.Message);
            }

            // Response ayarları
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/plain";

            // Basit error response (JSON serialization yok)
            await context.Response.WriteAsync("Internal Server Error");
        }
    }

}
