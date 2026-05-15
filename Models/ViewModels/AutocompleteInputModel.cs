namespace FantasyFootball.Models.ViewModels
{
    /// <summary>
    /// Konfiguracija za AJAX autocomplete partial view (Views/Shared/_Autocomplete.cshtml).
    /// Renderira vidljiv search input + hidden input koji se šalje serveru.
    /// </summary>
    public class AutocompleteInputModel
    {
        /// <summary>name atribut hidden input-a (sprema ID ili tekst kao FreeText).</summary>
        public string Name { get; set; } = "";

        /// <summary>id atribut korijenske komponente; auto-generira se ako je prazan.</summary>
        public string Id { get; set; } = "";

        /// <summary>Za edit forme — trenutno odabrana vrijednost (id ili tekst).</summary>
        public string? InitialValue { get; set; }

        /// <summary>Tekstualni prikaz inicijalne vrijednosti.</summary>
        public string? InitialLabel { get; set; }

        /// <summary>URL endpointa koji vraća JSON niz [{id, label}, ...]. Prima query parametar ?q=.</summary>
        public string Endpoint { get; set; } = "";

        /// <summary>Placeholder za vidljivi search input.</summary>
        public string Placeholder { get; set; } = "Pretraži...";

        /// <summary>Označava polje kao obavezno.</summary>
        public bool Required { get; set; } = true;

        /// <summary>
        /// Ako je true, korisnik može upisati i vrijednost koja ne postoji u rezultatima
        /// — hidden input će tada sadržavati upisani tekst umjesto ID-a.
        /// </summary>
        public bool FreeText { get; set; } = false;

        /// <summary>Tekst label-a iznad polja. Prazno = nema label.</summary>
        public string LabelText { get; set; } = "";

        /// <summary>Dodatna CSS klasa na korijenskom div-u.</summary>
        public string CssClass { get; set; } = "";
    }
}
