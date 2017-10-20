using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

using MiX.Integrate.Api.Client;
using MiX.Integrate.Shared.Entities.Groups;
using MiX.Integrate.Shared.Entities.Assets;

namespace MiX.Integrate.Samples.UpdateAsset
{
	class Program
	{
		static void Main(string[] args)
		{
			UpdateAnAsset().Wait();
		}

		private static async Task UpdateAnAsset()
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

				var assets = await GetAssets(group, apiBaseUrl, idServerResourceOwnerClientSettings);
				if (assets.Count == 0)
				{
					Console.WriteLine("");
					Console.WriteLine("=======================================================================");
					Console.WriteLine("No assets found!");
					return;
				}

				Asset asset = assets[0];
				asset.Notes = "Updated by MiX.Integrate";
				Console.WriteLine($"Updating asset with description '{0}'", asset.Description);
				var assetsClient = new AssetsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
				assetsClient.Update(asset);

				asset = assetsClient.Get(asset.AssetId);
				Console.WriteLine($"Asset with description '{0}' notes updated to '{1}'", asset.Description, asset.Notes);
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
