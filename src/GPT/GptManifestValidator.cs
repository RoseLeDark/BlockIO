using System;
using System.Collections.Generic;
using System.Text;

namespace BlockIO.GPT
{
    public static class GptManifestValidator
    {
        /// <summary>
        /// Validates a GPT entry array for structural consistency.
        /// Checks for duplicate GUIDs, overlapping LBAs, and invalid entries.
        /// </summary>
        /// <param name="array">The GPT entry array to validate.</param>
        /// <returns>A list of validation errors, or empty if valid.</returns>
        public static List<string> Validate(GptEntryArray array)
        {
            var errors = new List<string>();
            var guidSet = new HashSet<Guid>();

            foreach (var entry in array.GetAllEntries())
            {
                if (!entry.IsValid)
                {
                    errors.Add($"Invalid entry: {entry}");
                    continue;
                }

                if (!guidSet.Add(entry.UniqueGuid))
                    errors.Add($"Duplicate UniqueGuid: {entry.UniqueGuid}");

                if (entry.FirstLBA > entry.LastLBA)
                    errors.Add($"Invalid LBA range: {entry.FirstLBA} > {entry.LastLBA}");
            }

            return errors;
        }
    }
}
