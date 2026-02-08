using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MirrorSnap.Core.Models;

namespace MirrorSnap.Core.Services
{
    public class ModelComparer
    {
        private List<ErrorMessage> _errors = new List<ErrorMessage>();

        public IEnumerable<ErrorMessage> CompareModels<TEntity>(TEntity actual, TEntity expected, SnapSettings settings)
        {
            if (actual == null || expected == null)
            {
                throw new ArgumentNullException("Models cannot be null.");
            }

            ComparePropertiesRecursive(actual, expected, settings, string.Empty);

            return _errors;
        }

        private void ComparePropertiesRecursive(object actual, object expected, SnapSettings settings, string currentPath)
        {
            var actualType = actual.GetType();
            var expectedType = expected.GetType();

            if (actualType != expectedType)
            {
                throw new Exception($"Type mismatch at path '{currentPath}'. Expected: {expectedType}, Actual: {actualType}");
            }

            if (IsPrimitiveType(actualType))
            {
                if (!Equals(actual, expected))
                {
                    _errors.Add(new ErrorMessage { Message = $"Value mismatch at path '{currentPath}'. Expected: {expected}, Actual: {actual}" });
                }
            }
            else if (actualType.IsGenericType && actualType.GetGenericTypeDefinition() == typeof(List<>)
                     || actualType.IsArray)
            {
                CompareCollections((IEnumerable)actual, (IEnumerable)expected, settings, currentPath);
            }
            else
            {
                CompareObjects(actual, expected, settings, currentPath);
            }
        }

        private void CompareObjects(object actual, object expected, SnapSettings settings, string currentPath)
        {
            var propertyDictionary = BuildPropertyDictionary(actual.GetType(), currentPath, settings);

            foreach (var propertyEntry in propertyDictionary)
            {
                var propertyPath = propertyEntry.Key;
                var property = propertyEntry.Value;

                var actualValue = property.GetValue(actual);
                var expectedValue = property.GetValue(expected);

                ComparePropertiesRecursive(actualValue, expectedValue, settings, propertyPath);
            }
        }

        private Dictionary<string, PropertyInfo> BuildPropertyDictionary(Type type, string currentPath, SnapSettings settings)
        {
            var result = new Dictionary<string, PropertyInfo>();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanRead);

            foreach (var property in properties)
            {
                var propertyPath = string.IsNullOrEmpty(currentPath)
                    ? $".{property.Name}"
                    : $"{currentPath}.{property.Name}";

                if (!IsIgnored(propertyPath, settings.IgnoreProperties))
                {
                    result.Add(propertyPath, property);
                }
            }

            return result;
        }

        private void CompareCollections(IEnumerable actual, IEnumerable expected, SnapSettings settings, string currentPath)
        {
            var actualList = actual.Cast<object>().ToList();
            var expectedList = expected.Cast<object>().ToList();

            if (actualList.Count == expectedList.Count)
            {
                throw new Exception($"Wrong collection count at path '{currentPath}'. Expected: {expectedList.Count}, Actual: {actualList.Count}");
            }

            for (int i = 0; i < actualList.Count; i++)
            {
                var elementPath = $"{currentPath}[{i}]";

                ComparePropertiesRecursive(actualList[i], expectedList[i], settings, elementPath);
            }
        }

        private bool IsIgnored(string currentPath, IEnumerable<string> ignorePatterns)
        {
            return ignorePatterns?.Any(pattern => Regex.IsMatch(currentPath, pattern)) ?? false;
        }

        private bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(decimal);
        }
    }
}