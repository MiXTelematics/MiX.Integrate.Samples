using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using MiX.Integrate.Api.Client;
using MiX.Integrate.Shared.Entities.Assets;
using MiX.Integrate.Shared.Entities.Groups;

namespace GetLatestPositions
{
	class Program
	{
		const byte INTERATIONS = 3;

		static void Main(string[] args)
		{
			ShowPositions().Wait();
		}

		private static async Task ShowPositions()
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

				var assets = await GetAssets(group, apiBaseUrl, idServerResourceOwnerClientSettings);

				if (assets.Count < 1)
				{
					Console.WriteLine($"No assets found for {group.Name}, terminating.");
					return;
				}
				else
				{
					Console.WriteLine($"{assets.Count} assets found for {group.Name}");
				}

				var assetIds = assets.Select(a => a.AssetId).ToList();

				var positionClient = new PositionsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
				var lastRequest = DateTime.UtcNow.AddDays(-1);

				for (int i = 0; i < INTERATIONS; i++)
				{
					Console.WriteLine("");
					Console.WriteLine("=======================================================================");
					Console.WriteLine("Retrieving positions....");

					var positions = await positionClient.GetLatestByAssetIdsAsync(assetIds, 1, lastRequest).ConfigureAwait(false);
					lastRequest = DateTime.UtcNow;
					Console.WriteLine($"Retrieved {positions.Count} positions.");

					foreach (var item in from pos in positions.Take(10) // for sample, only print first ten positions.
															 join ast in assets on pos.AssetId equals ast.AssetId
															 select new
															 {
																 Registration = ast.RegistrationNumber,
																 Timestamp = pos.Timestamp,
																 Latitude = pos.Latitude,
																 Longitutde = pos.Longitude,
																 Odometer = pos.OdometerKilometres
															 })
					{
						Console.WriteLine($"{item.Registration,-15}  {item.Timestamp}  {item.Latitude:N6} - {item.Longitutde:N6}  {item.Odometer,10:N1} Km");
					}

					if (i < INTERATIONS - 1) await Task.Delay(5000); //wait 5 seconds
				}
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

		private static async Task<List<Asset>> GetAssets(GroupSummary group, string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving Asset list...");
			var assetsClient = new AssetsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var assets = await assetsClient.GetAllAsync(group.GroupId);
			return assets;
		}

		private static void PrintException(Exception ex)
		{
			Console.WriteLine(ex.Message);
			if (ex.InnerException != null) PrintException(ex.InnerException);
		}
	}
}
