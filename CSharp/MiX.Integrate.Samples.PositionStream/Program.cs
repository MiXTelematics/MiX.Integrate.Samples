using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MiX.Integrate.Api.Client;
using MiX.Integrate.Shared.Entities.Assets;
using MiX.Integrate.Shared.Entities.Groups;
using MiX.Integrate.Shared.Entities.Positions;

namespace MiX.Integrate.Samples.PositionStream
{
	class Program
	{
		static CancellationToken _cancelToken;

		static void Main(string[] args)
		{
			using (var taskCanceller = new CancellationTokenSource())
			{
				_cancelToken = taskCanceller.Token;
				//start the sample running in the background
				var sampleTask = Task.Run(ShowPositions, _cancelToken);
				//then wait for key press
				Console.ReadKey();
				taskCanceller.Cancel();
				try
				{
					sampleTask.Wait();
				}
				catch (AggregateException ae)
				{
					foreach (var e in ae.InnerExceptions)
						Console.WriteLine("{0}: {1}", e.GetType().Name, e.Message);
				}
			}
		}

		private static async Task ShowPositions()
		{
			try
			{
				//
				// Retrieve base URI from configuration file:
				var apiBaseUrl = ConfigurationManager.AppSettings["ApiUrl"];
				Console.WriteLine($"Connecting to: {apiBaseUrl}");

				//
				// Retrieve security settings from configuration file:
				//  note:  the helper client does the authentication with these settings and attached the
				//         returned token to all other calls.
				var idServerResourceOwnerClientSettings = new IdServerResourceOwnerClientSettings()
				{
					BaseAddress = ConfigurationManager.AppSettings["IdentityServerBaseAddress"],
					ClientId = ConfigurationManager.AppSettings["IdentityServerClientId"],
					ClientSecret = ConfigurationManager.AppSettings["IdentityServerClientSecret"],
					UserName = ConfigurationManager.AppSettings["IdentityServerUserName"],
					Password = ConfigurationManager.AppSettings["IdentityServerPassword"],
					Scopes = ConfigurationManager.AppSettings["IdentityServerScopes"]
				};

				//
				// Retrieve list of groups the authenticated user has access to:
				var groups = await GetAvailableOrganisationsAsync(apiBaseUrl, idServerResourceOwnerClientSettings);
				if ((groups?.Count ?? 0) < 1)
				{
					Console.WriteLine("");
					Console.WriteLine("=======================================================================");
					Console.WriteLine("No available organisations found - terminating.");
					return;
				}
				// the rest of this sample will only process using the first available organisation
				var group = groups[36];// 0];

				//
				// Retrieve a list of Assets available in the organisation
				var assets = await GetAssetsAsync(group.GroupId, apiBaseUrl, idServerResourceOwnerClientSettings);
				if (assets.Count < 1)
				{
					Console.WriteLine("");
					Console.WriteLine("=======================================================================");
					Console.WriteLine($"No assets found for {group.Name}, terminating.");
					return;
				}
				else
				{
					Console.WriteLine($"{assets.Count} assets found for {group.Name}.");
				}


				//
				// For this sample code the start point will be 1 hour before the sample
				// is executed. In a production service this sould only be seeded on
				// first execution and persisted between executions so that the stream
				// is read correctly
				string getSinceToken = DateTime.UtcNow.AddHours(-1).ToString("yyyyMMddHHmmssfff");

				//
				// Setup helper client and go into process loop
				var positionClient = new PositionsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
				var groupIds = new List<long> { group.GroupId };
				do
				{
					Console.WriteLine("");
					Console.WriteLine("=======================================================================");
					Console.WriteLine("Requesting positions....");

					var haveMoreItems = false;
					do
					{
						var requestResult = await positionClient.GetCreatedSinceForGroupsAsync(groupIds, "Asset", getSinceToken, 50).ConfigureAwait(false);
						haveMoreItems = requestResult.HasMoreItems;
						var positions = requestResult.Items;
						Console.WriteLine($"Retrieved {positions.Count} positions.");

						ProcessPositions(positions, assets);

						// persist token for next retrieval.
						getSinceToken =  requestResult.GetSinceToken ;
					} while (haveMoreItems);

					//
					// pause to prevent excessive calls to API.
					await Task.Delay(30000, _cancelToken); //wait 30 seconds
				} while (!_cancelToken.IsCancellationRequested);
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
			}

			return;
		}

		private static async Task<List<Group>> GetAvailableOrganisationsAsync(string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving Organisation details");
			var groupsClient = new GroupsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var groups = await groupsClient.GetAvailableOrganisationsAsync();
			return groups;
		}

		private static async Task<List<Asset>> GetAssetsAsync(long groupId, string apiBaseUrl, IdServerResourceOwnerClientSettings idServerResourceOwnerClientSettings)
		{
			Console.WriteLine("Retrieving Asset list...");
			var assetsClient = new AssetsClient(apiBaseUrl, idServerResourceOwnerClientSettings);
			var assets = await assetsClient.GetAllAsync(groupId);
			return assets;
		}

		private static void ProcessPositions(List<Position> positions, List<Asset> assets)
		{

			//Sample just prints positions to console window.

			// Join postion and asset lists to be able to print Asset registration number and position.
			foreach (var item in from pos in positions
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
		}

		private static void PrintException(Exception ex)
		{
			Console.WriteLine(ex.Message);
			if (ex.InnerException != null) PrintException(ex.InnerException);
		}
	}
}
