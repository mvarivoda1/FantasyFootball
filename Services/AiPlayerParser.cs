using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Anthropic;
using Anthropic.Models.Messages;
using FantasyFootball.Models.ViewModels;

namespace FantasyFootball.Services
{
    // AI integracija: pretvara prirodnojezični opis igrača (HR/EN) u popunjeni
    // PlayerFormViewModel koristeći Claude (Anthropic) + structured outputs.
    public class AiPlayerParser
    {
        private const string ModelId = "claude-opus-4-8";

        private readonly string? _apiKey;
        private readonly ILogger<AiPlayerParser> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public AiPlayerParser(IConfiguration config, ILogger<AiPlayerParser> logger)
        {
            // Ključ iz konfiguracije (user-secrets) ili iz env varijable ANTHROPIC_API_KEY.
            _apiKey = config["Anthropic:ApiKey"];
            if (string.IsNullOrWhiteSpace(_apiKey))
                _apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

            _logger = logger;
        }

        // true ako je API ključ postavljen — UI prema tome prikazuje/skriva AI unos.
        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        public async Task<PlayerFormViewModel?> ParseAsync(string prompt, CancellationToken ct = default)
        {
            if (!IsConfigured || string.IsNullOrWhiteSpace(prompt))
                return null;

            var client = new AnthropicClient { ApiKey = _apiKey };

            try
            {
                var response = await client.Messages.Create(new MessageCreateParams
                {
                    Model = ModelId,
                    MaxTokens = 1024,
                    System = SystemPrompt,
                    OutputConfig = new OutputConfig
                    {
                        Format = new JsonOutputFormat { Schema = BuildSchema() }
                    },
                    Messages =
                    [
                        new() { Role = Role.User, Content = prompt }
                    ]
                }, cancellationToken: ct);

                // Sigurnosni odbijač (refusal) → nema rezultata, pusti ručni unos.
                if (response.StopReason == "refusal")
                {
                    _logger.LogWarning("AI parser: zahtjev odbijen (refusal).");
                    return null;
                }

                var sb = new StringBuilder();
                foreach (var text in response.Content.Select(b => b.Value).OfType<TextBlock>())
                    sb.Append(text.Text);

                var json = sb.ToString();
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                var vm = JsonSerializer.Deserialize<PlayerFormViewModel>(json, JsonOpts);
                if (vm == null)
                    return null;

                NormalizeDefaults(vm);
                _logger.LogInformation("AI parser uspješno parsirao igrača '{First} {Last}'.", vm.FirstName, vm.LastName);
                return vm;
            }
            catch (Exception ex)
            {
                // App se ne smije srušiti — logiraj i pusti ručni unos.
                _logger.LogError(ex, "AI parser nije uspio obraditi upit.");
                return null;
            }
        }

        // Primijeni razumne defaulte ako AI nije popunio sva polja.
        private static void NormalizeDefaults(PlayerFormViewModel vm)
        {
            if (vm.DateOfBirth == default)
                vm.DateOfBirth = new DateTime(2000, 1, 1);
            if (vm.MarketValue <= 0)
                vm.MarketValue = 1.0;
        }

        private const string SystemPrompt =
            "Ti si asistent za unos podataka u fantasy football aplikaciju. " +
            "Iz korisnikova opisa (hrvatski ili engleski) izvuci atribute nogometaša i vrati ih " +
            "u zadanoj JSON shemi. Position mapiraj na: Goalkeeper (golman/vratar), Defender (branič/obrana), " +
            "Midfielder (vezni), Forward (napadač). Ako vrijednost nije navedena, koristi 0 za statistiku, " +
            "1.0 za tržišnu vrijednost i 2000-01-01 za datum rođenja. Ako je naveden samo nadimak ili jedno ime, " +
            "razdvoji na ime i prezime najbolje što možeš.";

        // JSON schema koja odgovara poljima PlayerFormViewModel (camelCase) za structured outputs.
        private static Dictionary<string, JsonElement> BuildSchema()
        {
            var properties = new
            {
                firstName = new { type = "string", description = "Ime igrača." },
                lastName = new { type = "string", description = "Prezime igrača." },
                position = new
                {
                    type = "string",
                    @enum = new[] { "Goalkeeper", "Defender", "Midfielder", "Forward" },
                    description = "Pozicija igrača."
                },
                club = new { type = "string", description = "Klub igrača." },
                nationality = new { type = "string", description = "Nacionalnost igrača." },
                dateOfBirth = new { type = "string", description = "Datum rođenja u formatu YYYY-MM-DD." },
                marketValue = new { type = "number", description = "Tržišna vrijednost u milijunima (npr. 14 za 14M)." },
                goals = new { type = "integer", description = "Broj golova." },
                assists = new { type = "integer", description = "Broj asistencija." },
                cleanSheets = new { type = "integer", description = "Broj clean sheetova." },
                totalPoints = new { type = "integer", description = "Ukupni fantasy bodovi." }
            };

            return new Dictionary<string, JsonElement>
            {
                ["type"] = JsonSerializer.SerializeToElement("object"),
                ["additionalProperties"] = JsonSerializer.SerializeToElement(false),
                ["properties"] = JsonSerializer.SerializeToElement(properties),
                ["required"] = JsonSerializer.SerializeToElement(
                    new[] { "firstName", "lastName", "position", "club", "nationality" })
            };
        }
    }
}
