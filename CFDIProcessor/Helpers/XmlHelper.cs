using System;
using System.Xml.Linq;

namespace CFDIProcessor.Helpers
{
    /// <summary>
    /// Clase auxiliar para operaciones con XML
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Obtiene el valor de un atributo XML de forma segura
        /// </summary>
        public static string GetAttributeValue(XElement element, string attributeName)
        {
            return element?.Attribute(attributeName)?.Value;
        }

        /// <summary>
        /// Parsea un decimal de forma segura, retornando null si falla
        /// </summary>
        public static decimal? ParseDecimalOrNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(value, out decimal result))
                return result;

            return null;
        }

        /// <summary>
        /// Parsea un DateTime de forma segura, retornando null si falla
        /// </summary>
        public static DateTime? ParseDateTimeOrNull(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParse(value, out DateTime result))
                return result;

            return null;
        }

        /// <summary>
        /// Parsea un DateTime requerido, lanzando excepción si falla
        /// </summary>
        public static DateTime ParseDateTimeRequired(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"El campo {fieldName} es requerido");

            if (!DateTime.TryParse(value, out DateTime result))
                throw new FormatException($"El campo {fieldName} no tiene un formato de fecha válido: {value}");

            return result;
        }

        /// <summary>
        /// Parsea un decimal requerido, lanzando excepción si falla
        /// </summary>
        public static decimal ParseDecimalRequired(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"El campo {fieldName} es requerido");

            if (!decimal.TryParse(value, out decimal result))
                throw new FormatException($"El campo {fieldName} no tiene un formato numérico válido: {value}");

            return result;
        }
    }
}
