using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

using MiX.Integrate.Api.Client;
using MiX.Integrate.Shared.Entities.Assets;
using MiX.Integrate.Shared.Entities.Drivers;
using MiX.Integrate.Shared.Entities.Groups;

namespace MiX.Integrate.Samples.AssetsDriversPassengers
{
	class Program
	{
		static void Main(string[] args)
		{
			ShowAssetsDriversAsync().Wait();
		}

		private static async Task ShowAssetsDriversAsync()
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
				var group = await GetFirstAvailableOrganisationsAsync(apiBaseUrl, idServerResourceOwnerClientSettings);

				if (group == null)
				{
					Console.WriteLine("");
					Console.WriteLine("=======================================================================");
					Console.WriteLine("No available organisations found for user.");
				}
				else
				{
					var assets = await GetAssetsAsync(group, apiBaseUrl, idServerResourceOwnerClientSettings);
					PrintAssetDetails(group, assets);

					var drivers = await GetDriversAsync(group, apiBaseUrl, idServerResourceOwnerClientSettings);
					PrintDriverDetails(group, drivers);
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

		private static async Task<Group> GetFirstAvailableOrganisationsAsync(string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving Organisation details");
			var groupsClient = new GroupsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var groups = await groupsClient.GetAvailableOrganisationsAsync();
			return groups?[0];
		}

		private static async Task<List<Asset>> GetAssetsAsync(Group organisation, string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving Asset list...");
			var assetsClient = new AssetsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var assets = await assetsClient.GetAllAsync(organisation.GroupId);
			return assets;
		}

		private static void PrintAssetDetails(Group organisation, List<Asset> assets)
		{
			Console.WriteLine($"{assets.Count} assets found for {organisation.Name}");
			Console.WriteLine(string.Empty);
			Console.WriteLine("Assets");
			Console.WriteLine("======");
			Console.WriteLine("ID".PadRight(25) + "Description".PadRight(50) + "Registration".PadRight(20));

			foreach (Asset asset in assets)
			{
				Console.WriteLine(asset.AssetId.ToString().PadRight(25) + asset.Description.PadRight(50) + asset.RegistrationNumber.PadRight(20));
			}

			Console.WriteLine(string.Empty);
			Console.WriteLine(string.Empty);
		}

		private static async Task<List<Driver>> GetDriversAsync(Group organisation, string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving Driver list...");
			var driversClient = new DriversClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var drivers = await driversClient.GetAllDriversAsync(organisation.GroupId, "", "");
			return drivers;
		}

		private static void PrintDriverDetails(Group organisation, List<Driver> drivers)
		{
			Console.WriteLine($"{drivers.Count} drivers found for {organisation.Name}");
			Console.WriteLine(string.Empty);
			Console.WriteLine("Drivers");
			Console.WriteLine("======");
			Console.WriteLine("ID".PadRight(25) + "Name".PadRight(50));

			foreach (Driver driver in drivers)
			{
				Console.WriteLine(driver.DriverId.ToString().PadRight(25) + driver.Name.PadRight(50));
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
