namespace FantasyFootball.Models.ViewModels
{
    /// <summary>
    /// Konfiguracija za custom datepicker partial view (Views/Shared/_DatePicker.cshtml).
    /// Renderira vidljiv text input (formatiran po kulturi) + hidden input sa ISO datumom.
    /// </summary>
    public class DatePickerInputModel
    {
        /// <summary>name atribut hidden input-a (ono što ide u MVC binding).</summary>
        public string Name { get; set; } = "";

        /// <summary>id atribut korijenske komponente; auto-generira se ako je prazan.</summary>
        public string Id { get; set; } = "";

        /// <summary>Trenutna vrijednost (može biti null).</summary>
        public DateTime? Value { get; set; }

        /// <summary>Uključuje i izbor sati/minuta. Hidden value će tada biti ISO datetime.</summary>
        public bool IncludeTime { get; set; } = false;

        /// <summary>Označava polje kao obavezno (sprječava prazno slanje na strani klijenta).</summary>
        public bool Required { get; set; } = true;

        /// <summary>Dodatna CSS klasa na korijenskom div-u.</summary>
        public string CssClass { get; set; } = "";

        /// <summary>Tekst label-a. Ako je prazan, label se neće renderirati.</summary>
        public string LabelText { get; set; } = "";

        /// <summary>Placeholder za vidljivi input.</summary>
        public string Placeholder { get; set; } = "";
    }
}
