using System.Collections.Generic;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Utilities.CSVImport
{
    public class CSVValidationResult
    {
        public bool HeaderMismatch => HeaderErrors.Count > 0;
        public List<string> HeaderErrors = new();
        public List<string> EntryWarnings = new();
        public List<string> EntryErrors = new();
    
        public List<string> ValidIDs = new();
        public List<string> InvalidIDs = new();
        public List<string> NewIDs = new();
        public List<string> UpdatedIDs = new();
        public List<string> DeletedIDs = new();

        public int ValidCount => ValidIDs.Count;
        public int InvalidCount => InvalidIDs.Count;
        public int NewCount => NewIDs.Count;
        public int UpdatedCount => UpdatedIDs.Count;
        public int DeletedCount => DeletedIDs.Count;
    }
}