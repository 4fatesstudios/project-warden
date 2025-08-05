using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.Utilities;
using UnityEditor;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Utilities.CSVImport
{
    public static class CSVValidator {
        public static CSVValidationResult Validate(TextAsset csvFile, Type soType, string outputFolder) {
            var result = new CSVValidationResult();
            if (csvFile == null || soType == null) return result;

            string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) {
                result.HeaderErrors.Add("CSV has no data rows.");
                return result;
            }

            string[] headersRaw = lines[0].Split(',');
            string[] headers = headersRaw.Select(h => h.Split(':')[0].Trim()).ToArray();
            string[] declaredTypes = headersRaw.Select(h => h.Contains(":") ? h.Split(':')[1].Trim() : "string").ToArray();

            int idIndex = Array.FindIndex(headers, h => h.Equals("ID", StringComparison.OrdinalIgnoreCase));
            if (idIndex < 0) {
                result.HeaderErrors.Add("CSV must include an 'ID' column.");
                return result;
            }

            var fields = soType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var props = soType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < headers.Length; i++) {
                string header = headers[i];
                string declaredType = declaredTypes[i];

                if (header.Equals("ID", StringComparison.OrdinalIgnoreCase)) {
                    bool found = fields.Any(f => f.Name.Equals("id", StringComparison.OrdinalIgnoreCase)) ||
                                 props.Any(p => p.Name.Equals("ID", StringComparison.OrdinalIgnoreCase));
                    if (!found)
                        result.HeaderErrors.Add($"Header '{header}' not found as 'id' field or property in {soType.Name}.");
                    continue;
                }

                bool hasField = fields.Any(f => f.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
                bool hasProp = props.Any(p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));

                if (!hasField && !hasProp)
                    result.HeaderErrors.Add($"Header '{header}' not found as field or property in {soType.Name}.");

                Type actualType = hasField
                    ? fields.First(f => f.Name.Equals(header, StringComparison.OrdinalIgnoreCase)).FieldType
                    : props.First(p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase)).PropertyType;

                if (!CSVTypeParsers.Parsers.ContainsKey(declaredType.ToLower()))
                    result.HeaderErrors.Add($"Declared type '{declaredType}' for column '{header}' is not supported.");
            }

            if (result.HeaderMismatch) return result;

            HashSet<string> seenIds = new();
            Dictionary<string, ScriptableObject> existing = AssetDatabase.FindAssets($"t:{soType.Name}", new[] { outputFolder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(so => so != null)
                .ToDictionary(so => ((BaseDataSO)so).ID);

            for (int row = 1; row < lines.Length; row++) {
                string[] values = lines[row].Split(',');
                if (values.Length != headers.Length) {
                    result.EntryErrors.Add($"Row {row + 1}: Column count mismatch. Expected {headers.Length}, got {values.Length}.");
                    continue;
                }

                string id = values[idIndex].Trim();
                if (string.IsNullOrEmpty(id)) {
                    result.EntryErrors.Add($"Row {row + 1}: Missing ID.");
                    continue;
                }

                if (seenIds.Contains(id)) {
                    result.EntryWarnings.Add($"Row {row + 1}: Duplicate ID '{id}' detected. Only latest will be used.");
                }
                seenIds.Add(id);

                List<string> missingFields = new();
                List<string> typeMismatch = new();

                for (int c = 0; c < headers.Length; c++) {
                    string val = values[c].Trim();
                    if (string.IsNullOrEmpty(val) && c != idIndex)
                        missingFields.Add(headers[c]);
                    else if (!CSVTypeParsers.Parsers[declaredTypes[c].ToLower()].Invoke(null, val, row + 1, headers[c]))
                        typeMismatch.Add(headers[c]);
                }

                if (missingFields.Any())
                    result.EntryWarnings.Add($"Row {row + 1}: Missing data in column(s): {string.Join(", ", missingFields)}.");

                if (typeMismatch.Any())
                    result.EntryWarnings.Add($"Row {row + 1}: Type mismatch in column(s): {string.Join(", ", typeMismatch)}.");

                if (missingFields.Any() || typeMismatch.Any())
                    result.InvalidIDs.Add(id);
                else {
                    result.ValidIDs.Add(id);
                    if (existing.ContainsKey(id)) result.UpdatedIDs.Add(id);
                    else result.NewIDs.Add(id);
                }
            }

            // Mark for deletion
            foreach (var id in existing.Keys) {
                if (!seenIds.Contains(id)) result.DeletedIDs.Add(id);
            }
            
            return result;
        }
    }
}
