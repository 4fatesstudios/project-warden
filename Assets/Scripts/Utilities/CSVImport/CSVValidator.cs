using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FourFatesStudios.ProjectWarden.Utilities;
using UnityEditor;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Utilities.CSVImport
{
    public static class CSVValidator
    {
        public static CSVValidationResult Validate(string csvPath, Type targetType)
        {
            var lines = File.ReadAllLines(csvPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            if (lines.Length < 2)
            {
                CustomLogger.LogError(LogSystem.CSVImporter, $"CSV file '{csvPath}' has insufficient data rows.");
                return new CSVValidationResult();
            }

            var headerParts = lines[0].Split(',');
            var fieldMap = new Dictionary<string, (string fieldName, string type)>();

            foreach (var part in headerParts)
            {
                var split = part.Split(':');
                if (split.Length != 2)
                {
                    CustomLogger.LogError(LogSystem.CSVImporter, $"Invalid header format '{part}' in CSV.");
                    return new CSVValidationResult();
                }
                fieldMap[split[0].Trim().ToLower()] = (split[0].Trim(), split[1].Trim().ToLower());
            }

            var soFields = GetSerializedFields(targetType);
            var idField = soFields.FirstOrDefault(f => f.Name.Equals("id", StringComparison.OrdinalIgnoreCase));
            if (idField == null)
            {
                CustomLogger.LogError(LogSystem.CSVImporter, $"Target ScriptableObject '{targetType.Name}' is missing an 'id' field.");
                return new CSVValidationResult();
            }

            var result = new CSVValidationResult
            {
                HeaderFields = fieldMap.Values.Select(f => f.fieldName).ToList()
            };

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var values = line.Split(',');

                if (values.Length != headerParts.Length)
                {
                    result.InvalidRows.Add(i);
                    CustomLogger.LogError(LogSystem.CSVImporter, $"Row {i} has incorrect number of columns. Expected {headerParts.Length}, got {values.Length}.");
                    continue;
                }

                var rowId = values[Array.FindIndex(headerParts, h => h.StartsWith("id:"))].Trim();
                if (string.IsNullOrWhiteSpace(rowId))
                {
                    result.MissingIDs.Add(i);
                    CustomLogger.Log(LogSystem.CSVImporter, $"Row {i} is missing an ID.");
                    continue;
                }

                result.ValidIDs.Add(rowId);
            }

            return result;
        }

        private static List<FieldInfo> GetSerializedFields(Type type)
        {
            return type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                       .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null)
                       .ToList();
        }
    }

    public class CSVValidationResult
    {
        public List<string> HeaderFields = new();
        public List<string> ValidIDs = new();
        public List<int> InvalidRows = new();
        public List<int> MissingIDs = new();
    }
}
