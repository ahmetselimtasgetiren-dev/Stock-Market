using System;

namespace StockMarket.Content.Definitions
{
    public static class DefinitionValidation
    {
        public static bool TryValidateId(string id, out string error)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                error = "Definition ID is required.";
                return false;
            }

            if (!IsLowercaseLetter(id[0]))
            {
                error = "Definition ID must begin with a lowercase letter.";
                return false;
            }

            for (int index = 0; index < id.Length; index++)
            {
                char character = id[index];

                if (!IsLowercaseLetter(character) && !char.IsDigit(character) && character != '_')
                {
                    error = "Definition ID may contain only lowercase ASCII letters, numbers, and underscores.";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        public static bool TryValidateTicker(string ticker, out string error)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                error = "Ticker is required.";
                return false;
            }

            if (ticker.Length < 2 || ticker.Length > 5)
            {
                error = "Ticker must contain between two and five letters.";
                return false;
            }

            for (int index = 0; index < ticker.Length; index++)
            {
                char character = ticker[index];

                if (character < 'A' || character > 'Z')
                {
                    error = "Ticker may contain only uppercase ASCII letters.";
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private static bool IsLowercaseLetter(char character)
        {
            return character >= 'a' && character <= 'z';
        }
    }
}
