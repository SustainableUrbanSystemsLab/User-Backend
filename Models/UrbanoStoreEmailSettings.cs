using System;
namespace Urbano_API.Models
{
	public class UrbanoStoreEmailSettings
	{
        public string APIKey { get; set; } = null!;

        public string SenderAddress { get; set; } = null!;

        public string SenderName { get; set; } = null!;
    }
}

