using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using MiX.Integrate.Api.Client;
using MiX.Integrate.Shared.Entities.Assets;
using MiX.Integrate.Shared.Entities.Groups;

namespace MiX.Integrate.Samples.CurrentPositions
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

			Console.WriteLine($"Connecting to: {apiBaseUrl}");

			try
			{
				var group = await GetFirstAvailableOrganisationsAsync(apiBaseUrl, idServerResourceOwnerClientSettings);

				var assets = await GetAssetsAsync(group.GroupId, apiBaseUrl, idServerResourceOwnerClientSettings);

				if (assets.Count < 1)
				{
					Console.WriteLine($"No assets found for {group.Name}, terminating.");
					return;
				}
				else
				{
					Console.WriteLine($"{assets.Count} assets found for {group.Name}.");
				}

				var positionClient = new PositionsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
				var lastRequest = DateTime.MinValue;
				var groupList = new List<long> { group.GroupId };
				for (int i = 0; i < INTERATIONS; i++)
				{
					Console.WriteLine("");
					Console.WriteLine("=======================================================================");
					Console.WriteLine("Retrieving positions....");

					var positions = await positionClient.GetLatestByGroupIdsAsync(groupList, 1, lastRequest).ConfigureAwait(false);
					lastRequest = DateTime.UtcNow;
					Console.WriteLine($"Retrieved {positions.Count} positions.");

					foreach (var item in from pos in positions //.Take(10) // for sample, only print first ten positions.
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

					if (i < INTERATIONS - 1) await Task.Delay(30000); //wait 30 seconds
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

		private static async Task<GroupSummary> GetFirstAvailableOrganisationsAsync(string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving Organisation details");
			var groupsClient = new GroupsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var groups = await groupsClient.GetAvailableOrganisationsAsync();
			if (groups.Count > 0)
			{
				var organisation = groups[0];
				var group = await groupsClient.GetSubGroupsAsync(organisation.GroupId);
				return group;
			}
			else
			{
				Console.WriteLine("");
				Console.WriteLine("=======================================================================");
				Console.WriteLine("No available organisations found.");
				return null;
			}
		}

		private static async Task<List<Asset>> GetAssetsAsync(long groupId, string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving Asset list...");
			var assetsClient = new AssetsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var assets = await assetsClient.GetAllAsync(groupId);
			return assets;
		}

		private static void PrintException(Exception ex)
		{
			Console.WriteLine(ex.Message);
			if (ex.InnerException != null) PrintException(ex.InnerException);
		}
	}
}
