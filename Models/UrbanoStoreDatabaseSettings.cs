using System;
namespace Urbano_API.Models
{
	public class UrbanoStoreDatabaseSettings
	{
        public string ConnectionString { get; set; } = null!;

		public string DatabaseName { get; set; } = null!;

		public string CollectionName { get; set; } = null!;
	}
}

