using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

using MiX.Integrate.Api.Client;
using MiX.Integrate.Shared.Entities.Groups;

namespace MiX.Integrate.Samples.OrganisationDetails
{
	class Program
	{
		static void Main(string[] args)
		{
			ShowOrganisationNamesAsync().Wait();
		}

		private static async Task ShowOrganisationNamesAsync()
		{
			var apiBaseUrl = ConfigurationManager.AppSettings["ApiUrl"];
			var idServerResourceOwnerClientSettings = new IdServerResourceOwnerClientSettings()
			{
				BaseAddress = ConfigurationManager.AppSettings["IdentityServerBaseAddress"],
				ClientId = ConfigurationManager.AppSettings["IdentityServerClientId"],
				ClientSecret = ConfigurationManager.AppSettings["IdentityServerClientSecret"],
				UserName = ConfigurationManager.AppSettings["IdentityServerUserName"],
				Password = ConfigurationManager.AppSettings["IdentityServerPassword"],
				Scopes = ConfigurationManager.AppSettings["IdentityServerScopes"]
			};

			try
			{
				var allowedOrganisations = await GetAllowedOrganisationsAsync(apiBaseUrl, idServerResourceOwnerClientSettings);
				PrintOrganisationDetails(allowedOrganisations);
			}
			catch (Exception ex)
			{
				Console.WriteLine("");
				Console.WriteLine("=======================================================================");
				Console.WriteLine("Unexpected error:");
				PrintException(ex);
			}
			finally
			{
				Console.WriteLine("");
				Console.WriteLine("=======================================================================");
				Console.WriteLine("");
				Console.WriteLine("Press any key to finish.");
				Console.ReadKey();
			}

			return;
		}

		private static async Task<List<Group>> GetAllowedOrganisationsAsync(string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving allowed organisation list");
			var groupsClient = new GroupsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var group = await groupsClient.GetAvailableOrganisationsAsync();
			return group;
		}

		private static void PrintOrganisationDetails(List<Group> organisations)
		{
			Console.WriteLine($"{organisations.Count} Organsiations found");
			Console.WriteLine(string.Empty);
			Console.WriteLine("ID".PadRight(25) + "Description".PadRight(50));
			Console.WriteLine($"{new String('=', 24)} {new String('=', 50)}");

			foreach (Group organisation in organisations)
			{
				Console.WriteLine(organisation.GroupId.ToString().PadRight(25) + organisation.Name);
			}

			Console.WriteLine(string.Empty);
			Console.WriteLine(string.Empty);
		}

		private static void PrintException(Exception ex)
		{
			Console.WriteLine(ex.Message);
			if (ex.InnerException != null) PrintException(ex.InnerException);
		}
	}
}
