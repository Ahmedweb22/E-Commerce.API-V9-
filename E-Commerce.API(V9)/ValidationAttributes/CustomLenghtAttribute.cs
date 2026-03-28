
namespace E_Commerce.API_V9_.ValidationAttributes
{
    public class CustomLenghtAttribute : ValidationAttribute
    {
        private readonly int minLength;
        private readonly int maxLength;

        public CustomLenghtAttribute(int minLength, int maxLength)
        {
            this.minLength = minLength;
            this.maxLength = maxLength;
        }
        public override bool IsValid(object? value)
        {
            if (value is string stringValue)
            {
                return stringValue.Length >= minLength && stringValue.Length <= maxLength;

            }
            return false;
        }
        public override string FormatErrorMessage(string name)
        {
            return $"The field {name} must be between {minLength} and {maxLength} characters long.";
        }
    }
}
