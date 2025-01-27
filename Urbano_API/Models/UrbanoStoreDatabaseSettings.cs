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

		public string RegistrationsWeeklyCollectionName { get; set; } = null!;

		public string RegistrationsMonthlyCollectionName { get; set; } = null!;

		public string RegistrationsYearlyCollectionName { get; set; } = null!;
		public string SimulationsDailyCollectionName { get; set; } = null!;

		public string SimulationsWeeklyCollectionName { get; set; } = null!;

		public string SimulationsMonthlyCollectionName { get; set; } = null!;

		public string SimulationsYearlyCollectionName { get; set; } = null!;
		public string WalletCollectionName { get; set; } = null!;
		
		public string LoginsDailyCollectionName { get; set; } = null!;

		public string LoginsWeeklyCollectionName { get; set; } = null!;

		public string LoginsMonthlyCollectionName { get; set; } = null!;

		public string LoginsYearlyCollectionName { get; set; } = null!;
		
		public string UniqueLoginsYearlyCollectionName { get; set; } = null!;
		public string UniqueLoginsDailyCollectionName { get; set; } = null!;

		public string UniqueLoginsWeeklyCollectionName { get; set; } = null!;

		public string UniqueLoginsMonthlyCollectionName { get; set; } = null!;
	}
}

