
namespace DreamSoft.Domain.ValueObjects
{
    public class BaseTranslatedProperties : ValueObject
    {
        public string Name { get; private set; } = string.Empty;
        public string? Descripcion { get; private set; } = null;

        private BaseTranslatedProperties()
        {
        }

        public static BaseTranslatedProperties Create(string name, string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            return new BaseTranslatedProperties
            {
                Name = name.Trim(),
                Descripcion = descripcion?.Trim()
            };
        }

        public static BaseTranslatedProperties CreateWithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            return new BaseTranslatedProperties
            {
                Name = name.Trim(),
                Descripcion = null
            };
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Name;
            yield return Descripcion;
        }

        public override string ToString() => Name;
    }
}