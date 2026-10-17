using SjolystInnovation.AppKit.Storage;

namespace nstuning_api.Services
{
    /// <summary>The file store for dyno run PDF reports.</summary>
    public static class ReportStore
    {
        /// <summary>The key the report store is registered under.</summary>
        public const string Name = "reports";

        private const long DefaultMaxBytes = 50 * 1024 * 1024;

        /// <summary>Registers the report store from <c>Storage:ReportsPath</c> and <c>Storage:MaxReportBytes</c>.</summary>
        public static IServiceCollection AddReportStore(this IServiceCollection services, IConfiguration configuration) =>
            services.AddAppKitFileStore(Name, o =>
            {
                o.Path = configuration["Storage:ReportsPath"] ?? "/data/reports";
                o.MaxBytes = configuration.GetValue<long?>("Storage:MaxReportBytes") ?? DefaultMaxBytes;
                o.Allowed = AppKitFileKinds.Pdf;
            });
    }
}
