using System;
namespace Urbano_API.Models
{
	public class UrbanoStoreDatabaseSettings
	{
        public string ConnectionString { get; set; } = null!;

		public string DatabaseName { get; set; } = null!;

		public string UsersCollectionName { get; set; } = null!;

		public string VerificationsCollectionName { get; set; } = null!;

		public string MetricsCollectionName { get; set; } = null!;

		public string RegistrationsDailyCollectionName { get; set; } = null!;
	}
}

