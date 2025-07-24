using System;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEditor;

namespace FourFatesStudios.ProjectWarden.Utilities.CSVImport
{
    public static class CSVTypeParsers
    {
        public static readonly Dictionary<string, Func<SerializedProperty, string, int, string, bool>> Parsers = new() {
            ["int"] = TryInt,
            ["string"] = (prop, val, row, header) => {
                if (prop != null) prop.stringValue = val;
                return true;
            },
            ["bool"] = TryBool,
            ["aspect"] = TryEnum<Aspect>,
            ["stat"] = TryEnum<Stat>,
            ["statmodifiertype"] = TryEnum<StatModifierType>,
            ["statmodifier"] = TryStatModifier,
            ["statmodifier[]"] = TryStatModifierArray,
            // Add more types here...
        };

        private static bool TryInt(SerializedProperty prop, string val, int row, string header) {
            if (int.TryParse(val, out int result)) {
                if (prop != null) prop.intValue = result;
                return true;
            }
            LogFail(row, header, val, "int");
            return false;
        }

        private static bool TryBool(SerializedProperty prop, string val, int row, string header) {
            if (bool.TryParse(val, out bool result)) {
                if (prop != null) prop.boolValue = result;
                return true;
            }
            LogFail(row, header, val, "bool");
            return false;
        }

        private static bool TryEnum<T>(SerializedProperty prop, string val, int row, string header) where T : Enum {
            if (Enum.TryParse(typeof(T), val, out var result)) {
                if (prop != null) prop.enumValueIndex = (int)result;
                return true;
            }
            LogFail(row, header, val, $"enum of type {typeof(T).Name}");
            return false;
        }

        private static bool TryStatModifier(SerializedProperty prop, string val, int row, string header) {
            var parts = val.Split('|');
            if (parts.Length != 3) {
                LogFail(row, header, val, "format Stat|Type|Value");
                return false;
            }

            if (!Enum.TryParse(parts[0], out Stat stat)) {
                LogFail(row, header, parts[0], "Stat enum");
                return false;
            }

            if (!Enum.TryParse(parts[1], out StatModifierType type)) {
                LogFail(row, header, parts[1], "StatModifierType enum");
                return false;
            }

            if (!int.TryParse(parts[2], out int amount)) {
                LogFail(row, header, parts[2], "int modifier");
                return false;
            }

            if (prop != null) {
                prop.FindPropertyRelative("stat").enumValueIndex = (int)stat;
                prop.FindPropertyRelative("type").enumValueIndex = (int)type;
                prop.FindPropertyRelative("modifier").intValue = amount;
            }

            return true;
        }

        private static bool TryStatModifierArray(SerializedProperty prop, string val, int row, string header) {
            var entries = val.Split(';');

            if (prop != null) prop.arraySize = entries.Length;

            for (int i = 0; i < entries.Length; i++) {
                SerializedProperty el = prop != null ? prop.GetArrayElementAtIndex(i) : null;
                if (!TryStatModifier(el, entries[i], row, $"{header}[{i}]"))
                    return false;
            }
            return true;
        }

        private static void LogFail(int row, string header, string value, string expectedType) {
            CustomLogger.LogError(LogSystem.CSVImporter, $"Row {row}: Failed to parse '{value}' for column '{header}'. Expected {expectedType}.");
        }
    }
}
