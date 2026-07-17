using System.Text.Json;
using System.Text.Json.Serialization;
using FantasyFootball.Models.ViewModels;
using OpenAI.Chat;

namespace FantasyFootball.Services
{
    // AI integracija: pretvara prirodnojezični opis igrača (HR/EN) u popunjeni
    // PlayerFormViewModel koristeći OpenAI (GPT-4o mini) + structured outputs.
    public class AiPlayerParser
    {
        private const string ModelId = "gpt-4o-mini";

        private readonly string? _apiKey;
        private readonly ILogger<AiPlayerParser> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public AiPlayerParser(IConfiguration config, ILogger<AiPlayerParser> logger)
        {
            // Ključ iz konfiguracije (user-secrets) ili iz env varijable OPENAI_API_KEY.
            _apiKey = config["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(_apiKey))
                _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            _logger = logger;
        }

        // true ako je API ključ postavljen — UI prema tome prikazuje/skriva AI unos.
        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        public async Task<PlayerFormViewModel?> ParseAsync(string prompt, CancellationToken ct = default)
        {
            if (!IsConfigured || string.IsNullOrWhiteSpace(prompt))
                return null;

            var client = new ChatClient(ModelId, _apiKey);

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1024,
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "player",
                    jsonSchema: BinaryData.FromString(SchemaJson),
                    jsonSchemaIsStrict: true)
            };

            try
            {
                ChatCompletion completion = await client.CompleteChatAsync(
                    [
                        new SystemChatMessage(SystemPrompt),
                        new UserChatMessage(prompt)
                    ],
                    options, ct);

                // Sigurnosni odbijač (refusal) → nema rezultata, pusti ručni unos.
                if (!string.IsNullOrEmpty(completion.Refusal))
                {
                    _logger.LogWarning("AI parser: zahtjev odbijen (refusal).");
                    return null;
                }

                var json = string.Concat(
                    completion.Content
                        .Where(p => p.Kind == ChatMessageContentPartKind.Text)
                        .Select(p => p.Text));

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

        // JSON schema za structured outputs (strict mode: sva polja obavezna,
        // additionalProperties=false) — odgovara poljima PlayerFormViewModel (camelCase).
        private const string SchemaJson = """
        {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "firstName":   { "type": "string",  "description": "Ime igrača." },
            "lastName":    { "type": "string",  "description": "Prezime igrača." },
            "position":    { "type": "string",  "enum": ["Goalkeeper", "Defender", "Midfielder", "Forward"], "description": "Pozicija igrača." },
            "club":        { "type": "string",  "description": "Klub igrača." },
            "nationality": { "type": "string",  "description": "Nacionalnost igrača." },
            "dateOfBirth": { "type": "string",  "description": "Datum rođenja u formatu YYYY-MM-DD." },
            "marketValue": { "type": "number",  "description": "Tržišna vrijednost u milijunima (npr. 14 za 14M)." },
            "goals":       { "type": "integer", "description": "Broj golova." },
            "assists":     { "type": "integer", "description": "Broj asistencija." },
            "cleanSheets": { "type": "integer", "description": "Broj clean sheetova." },
            "totalPoints": { "type": "integer", "description": "Ukupni fantasy bodovi." }
          },
          "required": ["firstName", "lastName", "position", "club", "nationality", "dateOfBirth", "marketValue", "goals", "assists", "cleanSheets", "totalPoints"]
        }
        """;
    }
}
