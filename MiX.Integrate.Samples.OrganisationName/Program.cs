using System;
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
			ShowOrganisationName().Wait();
		}

		private static async Task ShowOrganisationName()
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
			var organisationGroupId = long.Parse(ConfigurationManager.AppSettings["OrganisationGroupId"]);

			try
			{
				var group = await GetGroupSummary(organisationGroupId, apiBaseUrl, idServerResourceOwnerClientSettings);
				Console.WriteLine("Organisation Name : {0}", group.Name);
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

		private static async Task<GroupSummary> GetGroupSummary(long organisationGroupId, string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving Organisation details");
			var groupsClient = new GroupsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var group = await groupsClient.GetSubGroupsAsync(organisationGroupId);
			return group;
		}

		private static void PrintException(Exception ex)
		{
			Console.WriteLine(ex.Message);
			if (ex.InnerException != null) PrintException(ex.InnerException);
		}
	}
}
