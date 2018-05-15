namespace FindTextByUrl
{
    using System;
    using System.Linq;

    static class Helpers
    {
        /// <summary>
        /// Determines whether [is digits only] [the specified text].
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>
        ///   <c>true</c> if [is digits only] [the specified text]; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsDigitsOnly(this String text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return true;
            }

            return text.ToCharArray().All(c => Char.IsDigit(c));
        }
    }
}
