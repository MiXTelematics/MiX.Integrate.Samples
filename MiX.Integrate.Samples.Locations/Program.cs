using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MiX.Integrate.Api.Client;
using MiX.Integrate.Shared.Entities.Groups;
using MiX.Integrate.Shared.Entities.Locations;

namespace MiX.Integrate.Samples.Locations
{
	class Program
	{
		static void Main(string[] args)
		{
			ShowSiteLocations().Wait();
		}

		private static async Task ShowSiteLocations()
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
			
      Console.WriteLine($"Connecting to: {apiBaseUrl}");

			try
			{
				var group = await GetGroupSummary(organisationGroupId, apiBaseUrl, idServerResourceOwnerClientSettings);
				var defaultSite = GetDefaultSite(group);

				if (defaultSite == null)
				{
					Console.WriteLine("");
					Console.WriteLine("=======================================================================");
					Console.WriteLine("Default Site not found!");
					return;
				}

				var locations = await GetLocations(defaultSite, apiBaseUrl, idServerResourceOwnerClientSettings);
				PrintLocationDetails(defaultSite, locations);
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

		private static GroupSummary GetDefaultSite(GroupSummary organisationGroup)
		{
      foreach (var subGroup in organisationGroup.SubGroups)
			{
				if (subGroup.Type == GroupType.DefaultSite) return subGroup;
			}

			return null;
		}

		private static async Task<GroupSummary> GetGroupSummary(long organisationGroupId, string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving Organisation details");
			var groupsClient = new GroupsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var group = await groupsClient.GetSubGroupsAsync(organisationGroupId);
			return group;
		}

		private static async Task<List<Location>> GetLocations(GroupSummary group, string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving Locations...");
			var locationsClient = new LocationsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var locations = await locationsClient.GetAllAsync(group.GroupId);
			return locations;
		}

		private static void PrintLocationDetails(GroupSummary group, List<Location> locations)
		{
			Console.WriteLine($"{locations.Count} locations found for {group.Name}");
			Console.WriteLine(string.Empty);
			Console.WriteLine("Locations");
			Console.WriteLine("=========");
			Console.WriteLine("ID".PadRight(25) + "Name".PadRight(80) + "Type".PadRight(10) + "Shape Type".PadRight(10));

			foreach (Location location in locations)
			{
				Console.WriteLine(location.LocationId.ToString().PadRight(25) + location.Name.PadRight(80) + location.LocationType.ToString().PadRight(10) + location.ShapeType.ToString().PadRight(10));
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
