using Newtonsoft.Json.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ItchStandalone
{
    public class ItchApiClient
    {
        private readonly ILogger logger = LogManager.GetLogger();
        private readonly string apiKey;

        public ItchApiClient(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public async Task<List<GameMetadata>> GetLibraryGamesAsync()
        {
            var games = new List<GameMetadata>();

            if (string.IsNullOrEmpty(apiKey))
            {
                logger.Error("API Key is missing.");
                return games;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync($"https://itch.io/api/1/key/my-games?key={apiKey}");
                    var json = JObject.Parse(response);

                    if (json["games"] != null)
                    {
                        foreach (var item in json["games"])
                        {
                            var gameData = ParseGame(item);
                            if (gameData != null)
                            {
                                games.Add(gameData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to download library games.");
            }

            return games;
        }

        private GameMetadata ParseGame(JToken item)
        {
            try
            {
                var id = item["id"]?.ToString();
                var title = item["title"]?.ToString();
                var url = item["url"]?.ToString();
                var cover = item["cover_url"]?.ToString();
                
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(title))
                {
                    return null;
                }

                var metadata = new GameMetadata
                {
                    Source = new MetadataNameProperty("itch.io"),
                    GameId = id,
                    Name = title,
                    Links = new List<Link> { new Link("Itch.io", url) },
                    Description = item["short_text"]?.ToString() // Or fetch full description if needed
                };

                if (!string.IsNullOrEmpty(cover))
                {
                    metadata.CoverImage = new MetadataFile(cover);
                }

                // Additional metadata mapping can be done here (Developers, Genres, etc.)

                return metadata;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error parsing game data.");
                return null;
            }
        }
    }
}
